using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using Licitaciones.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Licitaciones.EndToEndTests.Infrastructure;

/// <summary>
/// Inicia PostgreSQL y la aplicación Web sobre Kestrel para las pruebas E2E.
/// </summary>
public sealed partial class LicitacionesE2eFixture : IAsyncLifetime
{
    private static readonly TimeSpan TiempoMaximoInicio =
        TimeSpan.FromSeconds(60);

    private readonly PostgreSqlContainer _postgres =
        new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("licitaciones_e2e")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    private readonly ConcurrentQueue<string> _registros = new();
    private readonly TaskCompletionSource<Uri> _direccionDisponible =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
    private Process? _procesoWeb;
    private Task? _lecturaSalida;
    private Task? _lecturaErrores;

    /// <summary>
    /// Dirección HTTP asignada dinámicamente a la aplicación Web.
    /// </summary>
    public Uri DireccionBase { get; private set; } = null!;

    /// <summary>
    /// Directorio donde una prueba puede guardar evidencia de un fallo.
    /// </summary>
    public string DirectorioEvidencias { get; private set; } = null!;

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        try
        {
            await _postgres.StartAsync();
            await AplicarMigracionesAsync();

            var raizRepositorio = EncontrarRaizRepositorio();
            DirectorioEvidencias =
                Environment.GetEnvironmentVariable("E2E_ARTIFACTS_PATH") ??
                Path.Combine(raizRepositorio, "TestResults", "e2e");

            IniciarAplicacionWeb(raizRepositorio);
            DireccionBase = await EsperarDireccionAsync();
            await EsperarAplicacionListaAsync();
        }
        catch
        {
            await DisposeAsync();
            throw;
        }
    }

    /// <summary>
    /// Crea un contexto independiente conectado a la base de la prueba.
    /// </summary>
    public LicitacionesDbContext CrearContexto()
    {
        var opciones = new DbContextOptionsBuilder<LicitacionesDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        return new LicitacionesDbContext(opciones);
    }

    /// <summary>
    /// Devuelve la salida capturada del proceso Web para diagnosticar fallos.
    /// </summary>
    public string ObtenerRegistros()
    {
        return string.Join(Environment.NewLine, _registros);
    }

    /// <inheritdoc />
    public async Task DisposeAsync()
    {
        if (_procesoWeb is not null)
        {
            try
            {
                if (!_procesoWeb.HasExited)
                {
                    _procesoWeb.Kill(entireProcessTree: true);
                }

                await _procesoWeb.WaitForExitAsync()
                    .WaitAsync(TimeSpan.FromSeconds(10));
            }
            catch (InvalidOperationException)
            {
                // El proceso terminó entre la comprobación y la limpieza.
            }
            catch (TimeoutException)
            {
                // El contenedor se elimina igualmente al finalizar la fixture.
            }

            await EsperarLecturaAsync(_lecturaSalida);
            await EsperarLecturaAsync(_lecturaErrores);
            _procesoWeb.Dispose();
        }

        await _postgres.DisposeAsync();
    }

    private async Task AplicarMigracionesAsync()
    {
        await using var contexto = CrearContexto();
        await contexto.Database.MigrateAsync();
    }

    private void IniciarAplicacionWeb(string raizRepositorio)
    {
        var proyectoWeb = Path.Combine(
            raizRepositorio,
            "src",
            "Licitaciones.Web",
            "Licitaciones.Web.csproj");
        var configuracion = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyConfigurationAttribute>()?
            .Configuration ?? "Debug";

        var inicio = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = Path.GetDirectoryName(proyectoWeb)!,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        inicio.ArgumentList.Add("run");
        inicio.ArgumentList.Add("--project");
        inicio.ArgumentList.Add(proyectoWeb);
        inicio.ArgumentList.Add("--configuration");
        inicio.ArgumentList.Add(configuracion);
        inicio.ArgumentList.Add("--no-build");
        inicio.ArgumentList.Add("--no-launch-profile");
        inicio.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        inicio.Environment["DOTNET_ENVIRONMENT"] = "Development";
        inicio.Environment["ASPNETCORE_URLS"] = "http://127.0.0.1:0";
        inicio.Environment["ConnectionStrings__Licitaciones"] =
            _postgres.GetConnectionString();
        inicio.Environment[
            "Logging__LogLevel__Microsoft.Hosting.Lifetime"] = "Information";

        _procesoWeb = new Process
        {
            StartInfo = inicio,
            EnableRaisingEvents = true
        };

        if (!_procesoWeb.Start())
        {
            throw new InvalidOperationException(
                "No fue posible iniciar la aplicación Web para la prueba E2E.");
        }

        _lecturaSalida = CapturarSalidaAsync(
            _procesoWeb.StandardOutput,
            buscarDireccion: true);
        _lecturaErrores = CapturarSalidaAsync(
            _procesoWeb.StandardError,
            buscarDireccion: false);
    }

    private async Task<Uri> EsperarDireccionAsync()
    {
        if (_procesoWeb is null)
        {
            throw new InvalidOperationException(
                "La aplicación Web no fue iniciada.");
        }

        var procesoFinalizado = _procesoWeb.WaitForExitAsync();
        var esperaAgotada = Task.Delay(TiempoMaximoInicio);
        var tareaTerminada = await Task.WhenAny(
            _direccionDisponible.Task,
            procesoFinalizado,
            esperaAgotada);

        if (tareaTerminada == _direccionDisponible.Task)
        {
            return await _direccionDisponible.Task;
        }

        throw new InvalidOperationException(
            "La aplicación Web no publicó una dirección HTTP dentro del tiempo esperado." +
            Environment.NewLine +
            ObtenerRegistros());
    }

    private async Task EsperarAplicacionListaAsync()
    {
        using var cliente = new HttpClient
        {
            BaseAddress = DireccionBase,
            Timeout = TimeSpan.FromSeconds(3)
        };
        var limite = DateTimeOffset.UtcNow.Add(TiempoMaximoInicio);
        Exception? ultimoError = null;

        while (DateTimeOffset.UtcNow < limite)
        {
            try
            {
                using var respuesta = await cliente.GetAsync(
                    "/licitaciones/crear");
                if (respuesta.IsSuccessStatusCode)
                {
                    return;
                }

                ultimoError = new HttpRequestException(
                    $"La aplicación respondió HTTP {(int)respuesta.StatusCode}.");
            }
            catch (HttpRequestException excepcion)
            {
                ultimoError = excepcion;
            }
            catch (TaskCanceledException excepcion)
            {
                ultimoError = excepcion;
            }

            if (_procesoWeb?.HasExited == true)
            {
                break;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250));
        }

        throw new InvalidOperationException(
            "La aplicación Web no estuvo disponible dentro del tiempo esperado." +
            Environment.NewLine +
            ObtenerRegistros(),
            ultimoError);
    }

    private async Task CapturarSalidaAsync(
        StreamReader lector,
        bool buscarDireccion)
    {
        while (await lector.ReadLineAsync() is { } linea)
        {
            _registros.Enqueue(linea);
            if (!buscarDireccion)
            {
                continue;
            }

            var coincidencia = PatronDireccion().Match(linea);
            if (coincidencia.Success &&
                Uri.TryCreate(
                    coincidencia.Groups["direccion"].Value,
                    UriKind.Absolute,
                    out var direccion))
            {
                _direccionDisponible.TrySetResult(direccion);
            }
        }
    }

    private static async Task EsperarLecturaAsync(Task? tarea)
    {
        if (tarea is null)
        {
            return;
        }

        try
        {
            await tarea.WaitAsync(TimeSpan.FromSeconds(5));
        }
        catch (TimeoutException)
        {
            // La limpieza no debe quedar bloqueada por la captura de salida.
        }
    }

    private static string EncontrarRaizRepositorio()
    {
        for (var directorio = new DirectoryInfo(AppContext.BaseDirectory);
             directorio is not null;
             directorio = directorio.Parent)
        {
            if (File.Exists(Path.Combine(directorio.FullName, "Licitaciones.sln")))
            {
                return directorio.FullName;
            }
        }

        throw new DirectoryNotFoundException(
            "No se encontró la raíz del repositorio para iniciar Licitaciones.Web.");
    }

    [GeneratedRegex(
        @"Now listening on:\s+(?<direccion>https?://\S+)",
        RegexOptions.CultureInvariant)]
    private static partial Regex PatronDireccion();
}

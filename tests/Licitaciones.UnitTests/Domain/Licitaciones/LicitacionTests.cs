using Licitaciones.Domain.Licitaciones;

namespace Licitaciones.UnitTests.Domain.Licitaciones;

public sealed class LicitacionTests
{
    public static TheoryData<decimal> PresupuestosNoPositivos => new()
    {
        0m,
        -0.01m
    };

    public static TheoryData<decimal> PresupuestosConMasDeDosDecimales => new()
    {
        0.001m,
        123.456m
    };

    public static TheoryData<decimal> PresupuestosValidos => new()
    {
        0.01m,
        1_250_000.50m,
        9_999_999_999_999_999.99m
    };

    [Fact]
    public void Crear_ConLongitudesMaximas_ConservaCodigoYTitulo()
    {
        var codigo = new string('C', Licitacion.LongitudMaximaCodigo);
        var titulo = new string('T', Licitacion.LongitudMaximaTitulo);

        var licitacion = CrearLicitacion(codigo: codigo, titulo: titulo);

        Assert.Equal(codigo, licitacion.Codigo);
        Assert.Equal(titulo, licitacion.Titulo);
    }

    [Theory]
    [InlineData(101, 1, "codigo")]
    [InlineData(1, 201, "titulo")]
    public void Crear_ConTextoMayorAlMaximo_LanzaErrorControlado(
        int longitudCodigo,
        int longitudTitulo,
        string parametroEsperado)
    {
        var excepcion = Assert.Throws<ArgumentException>(
            () => CrearLicitacion(
                codigo: new string('C', longitudCodigo),
                titulo: new string('T', longitudTitulo)));

        Assert.Equal(parametroEsperado, excepcion.ParamName);
        Assert.Contains("no puede superar", excepcion.Message);
    }

    [Theory]
    [InlineData("LIC-\0-001", "Compra", "codigo")]
    [InlineData("LIC-001", "Compra\nreservada", "titulo")]
    public void Crear_ConCaracterDeControl_LanzaErrorControlado(
        string codigo,
        string titulo,
        string parametroEsperado)
    {
        var excepcion = Assert.Throws<ArgumentException>(
            () => CrearLicitacion(codigo: codigo, titulo: titulo));

        Assert.Equal(parametroEsperado, excepcion.ParamName);
        Assert.Contains("caracteres de control", excepcion.Message);
    }

    [Fact]
    public void Crear_ConCodigosUnicodeEquivalentes_GeneraMismaNormalizacion()
    {
        var compuesto = CrearLicitacion(codigo: "LIC-CAFÉ");
        var descompuesto = CrearLicitacion(codigo: "LIC-CAFE\u0301");

        Assert.Equal(compuesto.Codigo, descompuesto.Codigo);
        Assert.Equal(
            compuesto.CodigoNormalizado,
            descompuesto.CodigoNormalizado);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_ConCodigoVacio_LanzaErrorControlado(string? codigo)
    {
        var excepcion = Assert.Throws<ArgumentException>(
            () => CrearLicitacion(codigo: codigo!));

        Assert.Equal("codigo", excepcion.ParamName);
        Assert.Contains(
            "El código de la licitación es obligatorio.",
            excepcion.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_ConTituloVacio_LanzaErrorControlado(string? titulo)
    {
        var excepcion = Assert.Throws<ArgumentException>(
            () => CrearLicitacion(titulo: titulo!));

        Assert.Equal("titulo", excepcion.ParamName);
        Assert.Contains(
            "El título de la licitación es obligatorio.",
            excepcion.Message);
    }

    [Fact]
    public void Crear_ConCodigoYTituloValidos_LimpiaExtremosSinAlterarContenido()
    {
        var licitacion = CrearLicitacion(
            codigo: "  LiC  001  ",
            titulo: "  Compra de equipo tecnológico  ");

        Assert.Equal("LiC  001", licitacion.Codigo);
        Assert.Equal("LIC  001", licitacion.CodigoNormalizado);
        Assert.Equal("Compra de equipo tecnológico", licitacion.Titulo);
        Assert.NotEqual(
            new Licitacion(
                "LIC 001",
                "Otra compra",
                1m,
                FechaCierreValida()).CodigoNormalizado,
            licitacion.CodigoNormalizado);
    }

    [Theory]
    [InlineData("LIC-001")]
    [InlineData(" lic-001 ")]
    [InlineData("LiC-001")]
    public void Crear_ConCodigosEquivalentes_GeneraMismaNormalizacion(string codigo)
    {
        var licitacion = CrearLicitacion(codigo: codigo);

        Assert.Equal("LIC-001", licitacion.CodigoNormalizado);
    }

    [Theory]
    [MemberData(nameof(PresupuestosNoPositivos))]
    public void Crear_ConPresupuestoNoPositivo_LanzaErrorControlado(
        decimal presupuesto)
    {
        var excepcion = Assert.Throws<ArgumentException>(
            () => CrearLicitacion(presupuesto: presupuesto));

        Assert.Equal("presupuestoEstimadoCrc", excepcion.ParamName);
        Assert.Contains(
            "El presupuesto estimado debe ser mayor que cero.",
            excepcion.Message);
    }

    [Theory]
    [MemberData(nameof(PresupuestosConMasDeDosDecimales))]
    public void Crear_ConPresupuestoConMasDeDosDecimales_LanzaErrorControlado(
        decimal presupuesto)
    {
        var excepcion = Assert.Throws<ArgumentException>(
            () => CrearLicitacion(presupuesto: presupuesto));

        Assert.Equal("presupuestoEstimadoCrc", excepcion.ParamName);
        Assert.Contains(
            "El presupuesto estimado no puede tener más de dos decimales.",
            excepcion.Message);
    }

    [Fact]
    public void Crear_ConPresupuestoSuperiorAlMaximo_LanzaErrorControlado()
    {
        var excepcion = Assert.Throws<ArgumentException>(
            () => CrearLicitacion(
                presupuesto: 10_000_000_000_000_000.00m));

        Assert.Equal("presupuestoEstimadoCrc", excepcion.ParamName);
        Assert.Contains("máximo permitido", excepcion.Message);
    }

    [Theory]
    [MemberData(nameof(PresupuestosValidos))]
    public void Crear_ConPresupuestoDentroDelRango_ConservaMonto(decimal presupuesto)
    {
        var licitacion = CrearLicitacion(presupuesto: presupuesto);

        Assert.Equal(presupuesto, licitacion.PresupuestoEstimadoCrc);
    }

    [Fact]
    public void Crear_SinFechaCierre_LanzaErrorControlado()
    {
        var excepcion = Assert.Throws<ArgumentException>(
            () => new Licitacion(
                "LIC-001",
                "Compra de equipo",
                1_000m,
                default));

        Assert.Equal("fechaCierre", excepcion.ParamName);
        Assert.Contains("La fecha de cierre es obligatoria.", excepcion.Message);
    }

    [Fact]
    public void Crear_ConDatosValidos_IniciaBorradorConIdentificadorYAuditoriaUtc()
    {
        var fechaCreacion = new DateTimeOffset(
            2026,
            8,
            24,
            10,
            30,
            0,
            TimeSpan.FromHours(-6));
        var fechaCierreAnterior = new DateTimeOffset(
            2026,
            8,
            23,
            18,
            0,
            0,
            TimeSpan.FromHours(-6));

        var licitacion = new Licitacion(
            "LIC-001",
            "Compra de equipo",
            1_500_000.25m,
            fechaCierreAnterior,
            fechaCreacion);

        Assert.NotEqual(Guid.Empty, licitacion.Id);
        Assert.Equal(EstadoLicitacion.Borrador, licitacion.Estado);
        Assert.Equal(fechaCierreAnterior.ToUniversalTime(), licitacion.FechaCierre);
        Assert.Equal(TimeSpan.Zero, licitacion.FechaCierre.Offset);
        Assert.Equal(fechaCreacion.ToUniversalTime(), licitacion.CreatedAt);
        Assert.Equal(licitacion.CreatedAt, licitacion.UpdatedAt);
        Assert.Equal(TimeSpan.Zero, licitacion.CreatedAt.Offset);
        Assert.Equal(0u, licitacion.Version);
    }

    [Fact]
    public void Publicar_DesdeBorradorConCierreFuturo_CambiaEstadoYAuditoriaUtc()
    {
        var fechaCreacion = new DateTimeOffset(
            2026, 8, 24, 10, 0, 0, TimeSpan.FromHours(-6));
        var fechaActual = new DateTimeOffset(
            2026, 8, 25, 9, 30, 0, TimeSpan.FromHours(-6));
        var licitacion = new Licitacion(
            "LIC-001",
            "Compra de equipo",
            1_000m,
            fechaActual.AddDays(1),
            fechaCreacion);

        licitacion.Publicar(fechaActual);

        Assert.Equal(EstadoLicitacion.Publicada, licitacion.Estado);
        Assert.Equal(fechaActual.ToUniversalTime(), licitacion.UpdatedAt);
        Assert.Equal(fechaCreacion.ToUniversalTime(), licitacion.CreatedAt);
        Assert.Equal(TimeSpan.Zero, licitacion.UpdatedAt.Offset);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Publicar_ConCierreNoFuturo_LanzaErrorSinModificar(int minutos)
    {
        var fechaActual = new DateTimeOffset(
            2026, 8, 25, 15, 0, 0, TimeSpan.Zero);
        var licitacion = new Licitacion(
            "LIC-001",
            "Compra de equipo",
            1_000m,
            fechaActual.AddMinutes(minutos),
            fechaActual.AddDays(-1));
        var updatedAtOriginal = licitacion.UpdatedAt;

        var excepcion = Assert.Throws<PublicacionLicitacionInvalidaException>(
            () => licitacion.Publicar(fechaActual));

        Assert.Equal(
            "La fecha de cierre debe ser futura para publicar la licitación.",
            excepcion.Message);
        Assert.Equal(
            MotivoPublicacionInvalida.FechaCierre,
            excepcion.Motivo);
        Assert.Equal(EstadoLicitacion.Borrador, licitacion.Estado);
        Assert.Equal(updatedAtOriginal, licitacion.UpdatedAt);
    }

    [Fact]
    public void Publicar_CuandoYaEstaPublicada_LanzaErrorSinModificarAuditoria()
    {
        var primeraPublicacion = new DateTimeOffset(
            2026, 8, 25, 15, 0, 0, TimeSpan.Zero);
        var licitacion = new Licitacion(
            "LIC-001",
            "Compra de equipo",
            1_000m,
            primeraPublicacion.AddDays(1),
            primeraPublicacion.AddDays(-1));
        licitacion.Publicar(primeraPublicacion);

        var excepcion = Assert.Throws<PublicacionLicitacionInvalidaException>(
            () => licitacion.Publicar(primeraPublicacion.AddHours(1)));

        Assert.Equal(
            "Solo se pueden publicar licitaciones en estado Borrador.",
            excepcion.Message);
        Assert.Equal(MotivoPublicacionInvalida.Estado, excepcion.Motivo);
        Assert.Equal(EstadoLicitacion.Publicada, licitacion.Estado);
        Assert.Equal(primeraPublicacion, licitacion.UpdatedAt);
    }

    private static Licitacion CrearLicitacion(
        string codigo = "LIC-001",
        string titulo = "Compra de equipo",
        decimal presupuesto = 1_000m,
        DateTimeOffset? fechaCierre = null)
    {
        return new Licitacion(
            codigo,
            titulo,
            presupuesto,
            fechaCierre ?? FechaCierreValida());
    }

    private static DateTimeOffset FechaCierreValida()
    {
        return new DateTimeOffset(
            2026,
            9,
            30,
            18,
            0,
            0,
            TimeSpan.FromHours(-6));
    }
}

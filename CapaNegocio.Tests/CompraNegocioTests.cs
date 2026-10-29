using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using CapaNegocio.Compras;
using CapaNegocio.DTOs.Compras;
using CapaNegocio.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace CapaNegocio.Tests;

/// <summary>
/// Pruebas de la lógica pura de compras (spec 005, T2): cálculo de importes
/// (G1a), analizador de precios (G2a) y analizador de cantidades (G3). No
/// intervienen EF Core, <c>CompraNegocio</c> ni el catálogo de mensajes: se
/// comparan motivos tipados y valores, nunca textos (RF-55 se cubre desde T3).
/// </summary>
public class CompraNegocioTests
{
    // Catálogo de mensajes del negocio (plan §5). Se copia aquí el texto exacto
    // para comparar por igualdad, sin leer las constantes privadas de
    // CompraNegocio (RF-55).
    private const string MensajeCantidadInvalida = "La cantidad debe ser un número entero mayor que cero escrito solo con dígitos.";
    private const string MensajeCantidadSuperaMaximo = "La cantidad supera el máximo permitido.";
    private const string MensajeCantidadAcumuladaSuperaMaximo = "La cantidad acumulada supera el máximo permitido.";
    private const string MensajePrecioCompraVacio = "El precio de compra es obligatorio y debe ser mayor que cero.";
    private const string MensajePrecioCompraNoPositivo = "El precio de compra debe ser mayor que cero.";
    private const string MensajePrecioCompraFormato = "El precio de compra admite solo números, con coma o punto decimal y hasta dos decimales.";
    private const string MensajePrecioCompraDecimales = "El precio de compra admite como máximo dos decimales.";
    private const string MensajePrecioCompraMaximo = "El precio de compra supera el máximo permitido.";
    private const string MensajePrecioVentaVacio = "El precio de venta es obligatorio y debe ser mayor que cero.";
    private const string MensajePrecioVentaNoPositivo = "El precio de venta debe ser mayor que cero.";
    private const string MensajePrecioVentaFormato = "El precio de venta admite solo números, con coma o punto decimal y hasta dos decimales.";
    private const string MensajePrecioVentaDecimales = "El precio de venta admite como máximo dos decimales.";
    private const string MensajePrecioVentaMaximo = "El precio de venta supera el máximo permitido.";
    private const string MensajeLineaAgregada = "Línea agregada.";
    private const string MensajeLineaActualizada = "Línea actualizada.";
    private const string MensajeLineaInexistente = "La línea indicada no existe en el detalle de la compra.";
    private const string MensajeConfirmacionCancelarCompra = "¿Confirma que desea descartar el proveedor y todas las líneas de la compra en carga?";
    private const string MensajeProductoInexistenteLineaFormato = "El producto de la línea {0} no existe. Vuelva a seleccionarlo.";
    private const string MensajeProductoInactivoFormato = "El producto {0} - {1} de la línea {2} está inactivo y no puede comprarse.";
    private const string MensajeProductoDuplicadoEnEdicionFormato = "El producto {0} - {1} ya está cargado en otra línea. Quite una de las dos líneas para acumular la cantidad.";
    private const string MensajeAcumulacionFormato = "Se acumuló la cantidad del producto {0}: ahora tiene {1} unidades.";
    private const string MensajeSinProveedoresCoincidentes = "No se encontraron proveedores que coincidan con la búsqueda.";
    private const string MensajeSinProductosCoincidentes = "No se encontraron productos que coincidan con la búsqueda.";
    private const string MensajeProveedoresEncontradosFormato = "Se encontraron {0} proveedores.";
    private const string MensajeUnProveedorEncontrado = "Se encontró 1 proveedor.";
    private const string MensajeProductosEncontradosFormato = "Se encontraron {0} productos.";
    private const string MensajeUnProductoEncontrado = "Se encontró 1 producto.";
    private const string MensajePreciosPropuestos = "Se propusieron los precios actuales del catálogo.";
    private const string MensajeProductoNoEncontrado = "El producto indicado no existe.";
    private const string MensajeProductoInactivoAlPrellenar = "El producto indicado está inactivo y no puede comprarse.";
    private const string MensajeErrorPersistencia = "No se pudo completar la operación por un problema con el origen de datos. Intente nuevamente.";
    private const string MensajeErrorBuscarProveedores = "No se pudo buscar proveedores por un problema con el origen de datos. Intente nuevamente.";
    private const string MensajeErrorBuscarProductos = "No se pudo buscar productos por un problema con el origen de datos. Intente nuevamente.";
    private const string MensajeErrorPreciosProducto = "No se pudieron obtener los precios del producto por un problema con el origen de datos. Intente nuevamente.";
    private const string MensajeProveedorRequerido = "Debe seleccionar un proveedor antes de registrar la compra.";
    private const string MensajeProveedorInexistente = "El proveedor seleccionado no existe. Vuelva a buscarlo.";
    private const string MensajeProveedorInactivoFormato = "El proveedor {0} está inactivo y no puede registrar compras.";
    private const string MensajeUsuarioSinIdentificar = "No se pudo identificar un usuario responsable válido para registrar la compra.";
    private const string MensajeUsuarioInexistente = "El usuario responsable de la compra no existe.";
    private const string MensajeUsuarioInactivo = "El usuario responsable de la compra está inactivo.";
    private const string MensajeSinItems = "La compra debe tener al menos un ítem.";
    private const string MensajeMontoTotalSuperaMaximo = "El monto total de la compra supera el máximo permitido.";
    private const string MensajeProductoRepetidoFormato = "El producto {0} - {1} está repetido en el detalle de la compra.";
    private const string MensajeRegistroExitosoFormato = "Compra N° {0} registrada exitosamente.";
    private const string MensajeHistorialVacio = "Todavía no hay compras registradas.";
    private const string MensajeComprasEncontradasFormato = "Se encontraron {0} compras.";
    private const string MensajeUnaCompraEncontrada = "Se encontró 1 compra.";
    private const string MensajeCompraNoEncontrada = "La compra indicada no existe.";
    private const string TextoSinProveedor = "(Sin proveedor)";
    private const string TextoSinUsuario = "(Sin usuario)";
    private const string MensajePermisoRegistrarCompra = "No tiene permisos para registrar compras.";
    private const string MensajePermisoConsultarCompras = "No tiene permisos para consultar las compras.";

    // ---------- G1a · Cálculo de importes (RF-7, RF-33, RF-34, RF-35, RNF-1) ----------

    // RF-33: 3 × 10,33 = 30,99, sin redondeos ni truncamientos intermedios.
    [Fact]
    public void CalcularSubtotal_TresUnidadesPorDiezConTreintaYTres_DevuelveTreintaConNoventaYNueve()
    {
        decimal subtotal = CalculoImportesCompra.CalcularSubtotal(3, 10.33m);

        Assert.Equal(30.99m, subtotal);
    }

    // RF-33 / RNF-1: 1 × 10,5 = 10,5 (el cálculo puro no produce textos ni formatea).
    [Fact]
    public void CalcularSubtotal_UnaUnidadPorDiezConCinco_DevuelveDiezConCinco()
    {
        decimal subtotal = CalculoImportesCompra.CalcularSubtotal(1, 10.5m);

        Assert.Equal(10.5m, subtotal);
    }

    // RF-34: la suma es exacta y cuadra centavo a centavo.
    [Fact]
    public void CalcularMontoTotal_SumaExactaDeSubtotales_NoGeneraDiferenciasDeCentavos()
    {
        decimal[] subtotales =
        {
            CalculoImportesCompra.CalcularSubtotal(3, 10.33m),   // 30,99
            CalculoImportesCompra.CalcularSubtotal(1, 10.5m),    // 10,50
            CalculoImportesCompra.CalcularSubtotal(7, 0.07m)     // 0,49
        };

        decimal total = CalculoImportesCompra.CalcularMontoTotal(subtotales);

        Assert.Equal(41.98m, total);
    }

    // RF-7 / RF-34: total con cero líneas.
    [Fact]
    public void CalcularMontoTotal_SinLineas_DevuelveCero()
    {
        decimal total = CalculoImportesCompra.CalcularMontoTotal(Array.Empty<decimal>());

        Assert.Equal(0m, total);
    }

    // RF-7 / RF-34: total con una línea.
    [Fact]
    public void CalcularMontoTotal_ConUnaLinea_DevuelveSuSubtotal()
    {
        decimal total = CalculoImportesCompra.CalcularMontoTotal(new[] { 10.33m });

        Assert.Equal(10.33m, total);
    }

    // RF-7 / RF-34: total con varias líneas.
    [Fact]
    public void CalcularMontoTotal_ConVariasLineas_DevuelveLaSumaDeSusSubtotales()
    {
        decimal total = CalculoImportesCompra.CalcularMontoTotal(new[] { 30.99m, 10.33m, 10m });

        Assert.Equal(51.32m, total);
    }

    // RF-35 / RNF-13: el precio de venta de la línea no altera el total.
    [Fact]
    public void CalcularMontoTotal_NoCambiaAlVariarElPrecioDeVentaDeLasLineas()
    {
        LineaCompraDto conVentaBaja = new() { IdProducto = 1, Cantidad = 2, PrecioCompra = 10.5m, PrecioVenta = 11m };
        LineaCompraDto conVentaAlta = new() { IdProducto = 1, Cantidad = 2, PrecioCompra = 10.5m, PrecioVenta = 99m };

        decimal totalConVentaBaja = CalculoImportesCompra.CalcularMontoTotal(new[]
        {
            CalculoImportesCompra.CalcularSubtotal(conVentaBaja.Cantidad, conVentaBaja.PrecioCompra)
        });
        decimal totalConVentaAlta = CalculoImportesCompra.CalcularMontoTotal(new[]
        {
            CalculoImportesCompra.CalcularSubtotal(conVentaAlta.Cantidad, conVentaAlta.PrecioCompra)
        });

        Assert.Equal(21m, totalConVentaBaja);
        Assert.Equal(totalConVentaBaja, totalConVentaAlta);
    }

    // D-15 / D-20: los límites son los del plan y no cambian por accidente.
    [Fact]
    public void ConstantesDeLimites_CoincidenConElPlan()
    {
        Assert.Equal(1, CalculoImportesCompra.CantidadMinima);
        Assert.Equal(1_000_000, CalculoImportesCompra.CantidadMaxima);
        Assert.Equal(0.01m, CalculoImportesCompra.PrecioMinimo);
        Assert.Equal(999_999_999.99m, CalculoImportesCompra.PrecioMaximo);
        Assert.Equal(9_999_999_999_999_999.99m, CalculoImportesCompra.MontoTotalMaximo);
        Assert.Equal(2, CalculoImportesCompra.DecimalesPrecio);
    }

    // ---------- G2a · Analizador de importes (RF-22 … RF-26, RF-28) ----------

    public static TheoryData<string, decimal> ImportesValidos => new()
    {
        { "10,33", 10.33m },
        { "10.33", 10.33m },
        { "10,5", 10.5m },
        { "0,01", 0.01m },
        { "999999999,99", 999_999_999.99m },
        { "999999999.99", 999_999_999.99m }
    };

    // RF-25: coma o punto como separador decimal; nunca como separador de miles.
    [Theory]
    [MemberData(nameof(ImportesValidos))]
    public void AnalizarImporte_TextoValido_DevuelveElValorSinMotivo(string texto, decimal esperado)
    {
        ResultadoAnalisisImporte resultado = AnalizadorImporte.Analizar(texto);

        Assert.True(resultado.EsValido);
        Assert.Equal(esperado, resultado.Valor);
        Assert.Null(resultado.Motivo);
    }

    // RF-22 / RF-23 / RF-28: null, vacío o solo espacios.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AnalizarImporte_TextoSinContenido_DevuelveVacio(string? texto)
    {
        ResultadoAnalisisImporte resultado = AnalizadorImporte.Analizar(texto);

        Assert.False(resultado.EsValido);
        Assert.Equal(MotivoPrecioInvalido.Vacio, resultado.Motivo);
    }

    // RF-25: caracteres no admitidos, separadores repetidos o mal ubicados y
    // signos que la spec no admite (un único «-» inicial es la única excepción).
    [Theory]
    [InlineData("1.2.3")]
    [InlineData("1.234,56")]
    [InlineData("1,234.56")]
    [InlineData("1.000.000.000,00")]
    [InlineData("10,")]
    [InlineData(",5")]
    [InlineData("abc")]
    [InlineData("1 0")]
    [InlineData("dos")]
    [InlineData("5-")]
    [InlineData("- 5")]
    [InlineData("--5")]
    [InlineData("+5")]
    [InlineData("-")]
    public void AnalizarImporte_FormatoNoAdmitido_DevuelveFormatoInvalido(string texto)
    {
        ResultadoAnalisisImporte resultado = AnalizadorImporte.Analizar(texto);

        Assert.False(resultado.EsValido);
        Assert.Equal(MotivoPrecioInvalido.FormatoInvalido, resultado.Motivo);
    }

    // RF-22 / RF-23: cero y valores negativos.
    [Theory]
    [InlineData("0")]
    [InlineData("0,00")]
    [InlineData("-5")]
    public void AnalizarImporte_ValorNoPositivo_DevuelveNoPositivo(string texto)
    {
        ResultadoAnalisisImporte resultado = AnalizadorImporte.Analizar(texto);

        Assert.False(resultado.EsValido);
        Assert.Equal(MotivoPrecioInvalido.NoPositivo, resultado.Motivo);
    }

    // RF-26 / D-12: más de dos dígitos decimales; «1.000» se lee como 1,000.
    [Theory]
    [InlineData("1.000")]
    [InlineData("10,333")]
    [InlineData("0,001")]
    public void AnalizarImporte_MasDeDosDecimales_DevuelveDecimalesExcedidos(string texto)
    {
        ResultadoAnalisisImporte resultado = AnalizadorImporte.Analizar(texto);

        Assert.False(resultado.EsValido);
        Assert.Equal(MotivoPrecioInvalido.DecimalesExcedidos, resultado.Motivo);
    }

    // RF-24 / D-15: por encima de 999.999.999,99, incluido el desborde de decimal.
    [Theory]
    [InlineData("1000000000")]
    [InlineData("1000000000.00")]
    [InlineData("1000000000,00")]
    [InlineData("99999999999999999999999999999999999999")]
    public void AnalizarImporte_SuperaElMaximo_DevuelveSuperaMaximo(string texto)
    {
        ResultadoAnalisisImporte resultado = AnalizadorImporte.Analizar(texto);

        Assert.False(resultado.EsValido);
        Assert.Equal(MotivoPrecioInvalido.SuperaMaximo, resultado.Motivo);
    }

    // RF-28 §6.3, precedencia 1: el texto vacío se evalúa antes que el formato
    // (los espacios, incluido el tabulador, se recortan antes de analizar).
    [Fact]
    public void AnalizarImporte_SoloEspaciosConTabulador_DevuelveVacio()
    {
        ResultadoAnalisisImporte resultado = AnalizadorImporte.Analizar(" \t ");

        Assert.Equal(MotivoPrecioInvalido.Vacio, resultado.Motivo);
    }

    // RF-28 §6.3, precedencia 2: el formato se evalúa antes que el valor ≤ 0.
    [Theory]
    [InlineData("--0")]
    [InlineData("+0")]
    [InlineData("0-")]
    public void AnalizarImporte_ConFormatoYValorInvalido_DevuelveFormatoInvalido(string texto)
    {
        ResultadoAnalisisImporte resultado = AnalizadorImporte.Analizar(texto);

        Assert.Equal(MotivoPrecioInvalido.FormatoInvalido, resultado.Motivo);
    }

    // RF-28 §6.3, precedencia 3: el valor ≤ 0 se evalúa antes que los decimales.
    [Theory]
    [InlineData("-0,001")]
    [InlineData("-10,333")]
    public void AnalizarImporte_NoPositivoConDecimalesExcedidos_DevuelveNoPositivo(string texto)
    {
        ResultadoAnalisisImporte resultado = AnalizadorImporte.Analizar(texto);

        Assert.Equal(MotivoPrecioInvalido.NoPositivo, resultado.Motivo);
    }

    // RF-28 §6.3, precedencia 4: los decimales se evalúan antes que el máximo.
    [Theory]
    [InlineData("1000000000,000")]
    [InlineData("1000000000.000")]
    public void AnalizarImporte_SobreElMaximoConDecimalesExcedidos_DevuelveDecimalesExcedidos(string texto)
    {
        ResultadoAnalisisImporte resultado = AnalizadorImporte.Analizar(texto);

        Assert.Equal(MotivoPrecioInvalido.DecimalesExcedidos, resultado.Motivo);
    }

    // ---------- G3 · Analizador de cantidades (RF-20, RF-21) ----------

    public static TheoryData<string, int> CantidadesValidas => new()
    {
        { "1", 1 },
        { "10", 10 },
        { "1000000", 1_000_000 }
    };

    // RF-20 / RF-21: solo dígitos, de 1 a 1.000.000.
    [Theory]
    [MemberData(nameof(CantidadesValidas))]
    public void AnalizarCantidad_TextoValido_DevuelveElValorSinMotivo(string texto, int esperado)
    {
        ResultadoAnalisisCantidad resultado = AnalizadorCantidad.Analizar(texto);

        Assert.True(resultado.EsValido);
        Assert.Equal(esperado, resultado.Valor);
        Assert.Null(resultado.Motivo);
    }

    // RF-20: null, vacío o solo espacios.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AnalizarCantidad_TextoSinContenido_DevuelveVacio(string? texto)
    {
        ResultadoAnalisisCantidad resultado = AnalizadorCantidad.Analizar(texto);

        Assert.False(resultado.EsValido);
        Assert.Equal(MotivoCantidadInvalida.Vacio, resultado.Motivo);
    }

    // RF-20: separadores de miles o decimales, signos, espacios internos y letras.
    [Theory]
    [InlineData("-1")]
    [InlineData("1,5")]
    [InlineData("1.000")]
    [InlineData("1.000.001")]
    [InlineData("10 0")]
    [InlineData("dos")]
    [InlineData("+5")]
    [InlineData("1-")]
    public void AnalizarCantidad_ConCaracteresNoAdmitidos_DevuelveFormatoInvalido(string texto)
    {
        ResultadoAnalisisCantidad resultado = AnalizadorCantidad.Analizar(texto);

        Assert.False(resultado.EsValido);
        Assert.Equal(MotivoCantidadInvalida.FormatoInvalido, resultado.Motivo);
    }

    // RF-20: la cantidad debe ser mayor que cero.
    [Theory]
    [InlineData("0")]
    [InlineData("000")]
    public void AnalizarCantidad_Cero_DevuelveNoPositivo(string texto)
    {
        ResultadoAnalisisCantidad resultado = AnalizadorCantidad.Analizar(texto);

        Assert.False(resultado.EsValido);
        Assert.Equal(MotivoCantidadInvalida.NoPositivo, resultado.Motivo);
    }

    // RF-21 / D-15: por encima de 1.000.000, incluido el desborde de int.
    [Theory]
    [InlineData("1000001")]
    [InlineData("99999999999")]
    public void AnalizarCantidad_SuperaElMaximo_DevuelveSuperaMaximo(string texto)
    {
        ResultadoAnalisisCantidad resultado = AnalizadorCantidad.Analizar(texto);

        Assert.False(resultado.EsValido);
        Assert.Equal(MotivoCantidadInvalida.SuperaMaximo, resultado.Motivo);
    }

    // ---------- G1b · Subtotal en preparación y quita de líneas (RF-29, RF-34, RF-65, RNF-4) ----------

    public static TheoryData<int?, string?, string?, string?> PreparacionesInvalidas => new()
    {
        { null, "1", "10", "12" },      // sin producto elegido
        { 0, "1", "10", "12" },         // producto sin identificar
        { 1, null, "10", "12" },        // sin cantidad
        { 1, "abc", "10", "12" },       // cantidad no numérica
        { 1, "0", "10", "12" },         // cantidad no positiva
        { 1, "1000001", "10", "12" },   // cantidad sobre el máximo
        { 1, "1", "10,333", "12" },     // precio de compra con tres decimales
        { 1, "1", "", "12" },           // precio de compra vacío
        { 1, "1", "10", "0" }           // precio de venta no positivo
    };

    // RF-65 / D-21: con cualquier dato inválido no hay subtotal ni se suma al total.
    [Theory]
    [MemberData(nameof(PreparacionesInvalidas))]
    public async Task CalcularSubtotalEnPreparacion_ConEntradaInvalida_DevuelveInvalidoSinSubtotal(
        int? idProducto, string? cantidad, string? precioCompra, string? precioVenta)
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        var negocio = new CompraNegocio(db);

        SubtotalLineaDto preparacion = negocio.CalcularSubtotalEnPreparacion(
            idProducto, cantidad, precioCompra, precioVenta);

        Assert.False(preparacion.EsValido);
        Assert.Null(preparacion.Subtotal);
    }

    // RF-34 / RF-65: con entrada válida calcula el subtotal y no altera el total de la grilla.
    [Fact]
    public async Task CalcularSubtotalEnPreparacion_ConEntradaValida_CalculaSinSumarAlTotalDeLaGrilla()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        var negocio = new CompraNegocio(db);
        IReadOnlyList<LineaCompraDto> lineas = new[] { Linea(1, "COD1", "Uno", 1, 10m, 12m) };

        SubtotalLineaDto preparacion = negocio.CalcularSubtotalEnPreparacion(1, "3", "10,33", "15");
        decimal totalGrilla = CalculoImportesCompra.CalcularMontoTotal(lineas.Select(l => l.Subtotal));

        Assert.True(preparacion.EsValido);
        Assert.Equal(30.99m, preparacion.Subtotal);
        Assert.Equal(10m, totalGrilla);
    }

    // RF-29: quitar una línea recalcula el total y no toca la base (RNF-4).
    [Fact]
    public async Task QuitarLinea_ConIndiceValido_DevuelveLaListaSinLaLineaYRecalculaElTotal()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        var negocio = new CompraNegocio(db);
        IReadOnlyList<LineaCompraDto> lineas = new[]
        {
            Linea(1, "COD1", "Uno", 3, 10.33m, 15m),
            Linea(2, "COD2", "Dos", 1, 10.5m, 12m)
        };

        RespuestaCompraEnCarga respuesta = negocio.QuitarLinea(lineas, 0);

        Assert.True(respuesta.Exito);
        Assert.Single(respuesta.Lineas);
        Assert.Equal(2, respuesta.Lineas[0].IdProducto);
        Assert.Equal(10.5m, respuesta.MontoTotal);
    }

    // RF-29: quitar la única línea deja el total en cero.
    [Fact]
    public async Task QuitarLinea_DeLaUnicaLinea_DejaElTotalEnCero()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        var negocio = new CompraNegocio(db);
        IReadOnlyList<LineaCompraDto> lineas = new[] { Linea(1, "COD1", "Uno", 3, 10.33m, 15m) };

        RespuestaCompraEnCarga respuesta = negocio.QuitarLinea(lineas, 0);

        Assert.True(respuesta.Exito);
        Assert.Empty(respuesta.Lineas);
        Assert.Equal(0m, respuesta.MontoTotal);
    }

    // RF-29 / RNF-11: índice fuera de rango ⇒ Exito = false, mensaje y lista intacta.
    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    [InlineData(99)]
    public async Task QuitarLinea_ConIndiceFueraDeRango_DevuelveLaListaIntacta(int indice)
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        var negocio = new CompraNegocio(db);
        IReadOnlyList<LineaCompraDto> lineas = new[]
        {
            Linea(1, "COD1", "Uno", 3, 10.33m, 15m),
            Linea(2, "COD2", "Dos", 1, 10.5m, 12m)
        };

        RespuestaCompraEnCarga respuesta = negocio.QuitarLinea(lineas, indice);

        Assert.False(respuesta.Exito);
        Assert.Equal(MensajeLineaInexistente, respuesta.Mensaje);
        AssertLineasIguales(lineas, respuesta.Lineas);
        Assert.Equal(41.49m, respuesta.MontoTotal);
    }

    // ---------- G2b · Textos exactos del catálogo (RF-55) ----------

    public static TheoryData<string, MotivoPrecioInvalido, string> CasosDePrecioDeCompra => new()
    {
        { "", MotivoPrecioInvalido.Vacio, MensajePrecioCompraVacio },
        { "abc", MotivoPrecioInvalido.FormatoInvalido, MensajePrecioCompraFormato },
        { "-5", MotivoPrecioInvalido.NoPositivo, MensajePrecioCompraNoPositivo },
        { "10,333", MotivoPrecioInvalido.DecimalesExcedidos, MensajePrecioCompraDecimales },
        { "1000000000", MotivoPrecioInvalido.SuperaMaximo, MensajePrecioCompraMaximo }
    };

    public static TheoryData<string, MotivoPrecioInvalido, string> CasosDePrecioDeVenta => new()
    {
        { "", MotivoPrecioInvalido.Vacio, MensajePrecioVentaVacio },
        { "abc", MotivoPrecioInvalido.FormatoInvalido, MensajePrecioVentaFormato },
        { "-5", MotivoPrecioInvalido.NoPositivo, MensajePrecioVentaNoPositivo },
        { "10,333", MotivoPrecioInvalido.DecimalesExcedidos, MensajePrecioVentaDecimales },
        { "1000000000", MotivoPrecioInvalido.SuperaMaximo, MensajePrecioVentaMaximo }
    };

    // RF-22 / RF-24 / RF-25 / RF-26: el texto del precio de compra lo redacta el negocio.
    [Theory]
    [MemberData(nameof(CasosDePrecioDeCompra))]
    public async Task AgregarLinea_ConPrecioDeCompraInvalido_DevuelveElTextoDelCatalogo(
        string precioCompra, MotivoPrecioInvalido motivo, string mensajeEsperado)
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);

        RespuestaOperacionLineaCompra respuesta = await negocio.AgregarLineaAsync(
            SolicitudAgregar(producto.IdProducto, precioCompra: precioCompra));

        Assert.Equal(ResultadoOperacionLineaCompra.PrecioInvalido, respuesta.Resultado);
        Assert.Equal(CampoPrecio.PrecioCompra, respuesta.CampoPrecio);
        Assert.Equal(motivo, respuesta.MotivoPrecio);
        Assert.Equal(mensajeEsperado, respuesta.Mensaje);
        Assert.Empty(respuesta.Lineas);
    }

    // RF-23 / RF-24 / RF-25 / RF-26: el texto del precio de venta lo redacta el negocio.
    [Theory]
    [MemberData(nameof(CasosDePrecioDeVenta))]
    public async Task AgregarLinea_ConPrecioDeVentaInvalido_DevuelveElTextoDelCatalogo(
        string precioVenta, MotivoPrecioInvalido motivo, string mensajeEsperado)
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);

        RespuestaOperacionLineaCompra respuesta = await negocio.AgregarLineaAsync(
            SolicitudAgregar(producto.IdProducto, precioVenta: precioVenta));

        Assert.Equal(ResultadoOperacionLineaCompra.PrecioInvalido, respuesta.Resultado);
        Assert.Equal(CampoPrecio.PrecioVenta, respuesta.CampoPrecio);
        Assert.Equal(motivo, respuesta.MotivoPrecio);
        Assert.Equal(mensajeEsperado, respuesta.Mensaje);
        Assert.Empty(respuesta.Lineas);
    }

    // RF-20 / RF-21: los dos textos posibles de la cantidad.
    [Theory]
    [InlineData("abc", MensajeCantidadInvalida)]
    [InlineData("0", MensajeCantidadInvalida)]
    [InlineData("1000001", MensajeCantidadSuperaMaximo)]
    public async Task AgregarLinea_ConCantidadInvalida_DevuelveElTextoDelCatalogo(
        string cantidad, string mensajeEsperado)
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);

        RespuestaOperacionLineaCompra respuesta = await negocio.AgregarLineaAsync(
            SolicitudAgregar(producto.IdProducto, cantidad: cantidad));

        Assert.Equal(ResultadoOperacionLineaCompra.CantidadInvalida, respuesta.Resultado);
        Assert.Equal(mensajeEsperado, respuesta.Mensaje);
        Assert.Empty(respuesta.Lineas);
    }

    // RF-18: producto inexistente en la primera línea.
    [Fact]
    public async Task AgregarLinea_ConProductoInexistente_DevuelveElTextoDelCatalogo()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        var negocio = new CompraNegocio(db);

        RespuestaOperacionLineaCompra respuesta = await negocio.AgregarLineaAsync(
            SolicitudAgregar(Importes.ProductoInexistente));

        Assert.Equal(ResultadoOperacionLineaCompra.ProductoInvalido, respuesta.Resultado);
        Assert.Equal(
            string.Format(MensajeProductoInexistenteLineaFormato, 1),
            respuesta.Mensaje);
        Assert.Empty(respuesta.Lineas);
    }

    // RF-18: producto inactivo identificado por código y por número de línea.
    [Fact]
    public async Task AgregarLinea_ConProductoInactivo_DevuelveElTextoDelCatalogo()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno", estado: false);
        var negocio = new CompraNegocio(db);
        IReadOnlyList<LineaCompraDto> lineas = new[] { Linea(99, "COD9", "Nueve", 1, 10m, 12m) };

        RespuestaOperacionLineaCompra respuesta = await negocio.AgregarLineaAsync(
            SolicitudAgregar(producto.IdProducto, lineas: lineas));

        Assert.Equal(ResultadoOperacionLineaCompra.ProductoInvalido, respuesta.Resultado);
        Assert.Equal(
            string.Format(MensajeProductoInactivoFormato, "COD1", "Uno", 2),
            respuesta.Mensaje);
        AssertLineasIguales(lineas, respuesta.Lineas);
    }

    // RF-27: línea agregada.
    [Fact]
    public async Task AgregarLinea_ConDatosValidos_DevuelveElTextoDelCatalogo()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);

        RespuestaOperacionLineaCompra respuesta = await negocio.AgregarLineaAsync(
            SolicitudAgregar(producto.IdProducto));

        Assert.Equal(ResultadoOperacionLineaCompra.Exito, respuesta.Resultado);
        Assert.Equal(MensajeLineaAgregada, respuesta.Mensaje);
    }

    // RF-30: línea actualizada.
    [Fact]
    public async Task EditarLinea_ConDatosValidos_DevuelveElTextoDelCatalogo()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);
        IReadOnlyList<LineaCompraDto> lineas = new[] { Linea(producto.IdProducto, "COD1", "Uno", 1, 10m, 12m) };

        RespuestaOperacionLineaCompra respuesta = await negocio.EditarLineaAsync(
            SolicitudEditar(0, producto.IdProducto, cantidad: "4", lineas: lineas));

        Assert.Equal(ResultadoOperacionLineaCompra.Exito, respuesta.Resultado);
        Assert.Equal(MensajeLineaActualizada, respuesta.Mensaje);
    }

    // RF-36: aviso de acumulación con el código y la cantidad acumulada.
    [Fact]
    public async Task AgregarLinea_ConAcumulacion_DevuelveElTextoDelCatalogo()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);
        IReadOnlyList<LineaCompraDto> lineas = new[] { Linea(producto.IdProducto, "COD1", "Uno", 2, 10m, 12m) };

        RespuestaOperacionLineaCompra respuesta = await negocio.AgregarLineaAsync(
            SolicitudAgregar(producto.IdProducto, cantidad: "3", lineas: lineas));

        Assert.Equal(ResultadoOperacionLineaCompra.ExitoConAcumulacion, respuesta.Resultado);
        Assert.Equal(string.Format(MensajeAcumulacionFormato, "COD1", 5), respuesta.Mensaje);
    }

    // RF-63: texto propio del rechazo de la acumulación sobre el máximo.
    [Fact]
    public async Task AgregarLinea_ConAcumulacionSobreElMaximo_DevuelveElTextoDelCatalogo()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);
        IReadOnlyList<LineaCompraDto> lineas = new[]
        {
            Linea(producto.IdProducto, "COD1", "Uno", 999_999, 10m, 12m)
        };

        RespuestaOperacionLineaCompra respuesta = await negocio.AgregarLineaAsync(
            SolicitudAgregar(producto.IdProducto, cantidad: "2", lineas: lineas));

        Assert.Equal(ResultadoOperacionLineaCompra.CantidadAcumuladaSuperaMaximo, respuesta.Resultado);
        Assert.Equal(MensajeCantidadAcumuladaSuperaMaximo, respuesta.Mensaje);
    }

    // RF-62: producto ya cargado en otra línea durante la edición.
    [Fact]
    public async Task EditarLinea_ConProductoDeOtraLinea_DevuelveElTextoDelCatalogo()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Producto uno = await SembrarProductoAsync(db, "COD1", "Uno");
        Producto dos = await SembrarProductoAsync(db, "COD2", "Dos");
        var negocio = new CompraNegocio(db);
        IReadOnlyList<LineaCompraDto> lineas = new[]
        {
            Linea(uno.IdProducto, "COD1", "Uno", 1, 10m, 12m),
            Linea(dos.IdProducto, "COD2", "Dos", 1, 20m, 25m)
        };

        RespuestaOperacionLineaCompra respuesta = await negocio.EditarLineaAsync(
            SolicitudEditar(1, uno.IdProducto, lineas: lineas));

        Assert.Equal(ResultadoOperacionLineaCompra.ProductoDuplicadoEnEdicion, respuesta.Resultado);
        Assert.Equal(
            string.Format(MensajeProductoDuplicadoEnEdicionFormato, "COD1", "Uno"),
            respuesta.Mensaje);
    }

    // RF-29 / RF-30: índice fuera de rango durante la edición.
    [Fact]
    public async Task EditarLinea_ConIndiceFueraDeRango_DevuelveElTextoDelCatalogo()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);
        IReadOnlyList<LineaCompraDto> lineas = new[] { Linea(producto.IdProducto, "COD1", "Uno", 1, 10m, 12m) };

        RespuestaOperacionLineaCompra respuesta = await negocio.EditarLineaAsync(
            SolicitudEditar(3, producto.IdProducto, lineas: lineas));

        Assert.Equal(ResultadoOperacionLineaCompra.LineaNoEncontrada, respuesta.Resultado);
        Assert.Equal(MensajeLineaInexistente, respuesta.Mensaje);
    }

    // RF-31: el texto de confirmación de limpiar la compra lo redacta el negocio.
    [Fact]
    public async Task ObtenerMensajeConfirmacionCancelarCompra_DevuelveElTextoDelCatalogo()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        var negocio = new CompraNegocio(db);

        Assert.Equal(MensajeConfirmacionCancelarCompra, negocio.ObtenerMensajeConfirmacionCancelarCompra());
    }

    // ---------- G4 · Validación de la línea y orden determinista (RF-18, RF-27, RF-28) ----------

    // RF-18: producto inexistente ⇒ primer error, lista intacta.
    [Fact]
    public async Task AgregarLinea_ConProductoInexistente_RechazaSinTocarLaLista()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Producto existente = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);
        IReadOnlyList<LineaCompraDto> lineas = new[]
        {
            Linea(existente.IdProducto, "COD1", "Uno", 2, 10m, 12m)
        };

        RespuestaOperacionLineaCompra respuesta = await negocio.AgregarLineaAsync(
            SolicitudAgregar(Importes.ProductoInexistente, lineas: lineas));

        Assert.Equal(ResultadoOperacionLineaCompra.ProductoInvalido, respuesta.Resultado);
        AssertLineasIguales(lineas, respuesta.Lineas);
        Assert.Equal(20m, respuesta.MontoTotal);
    }

    // RF-18: producto inactivo ⇒ rechazo con la lista intacta.
    [Fact]
    public async Task AgregarLinea_ConProductoInactivo_RechazaSinTocarLaLista()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Producto inactivo = await SembrarProductoAsync(db, "COD2", "Dos", estado: false);
        var negocio = new CompraNegocio(db);
        IReadOnlyList<LineaCompraDto> lineas = new[] { Linea(1, "COD1", "Uno", 1, 10m, 12m) };

        RespuestaOperacionLineaCompra respuesta = await negocio.AgregarLineaAsync(
            SolicitudAgregar(inactivo.IdProducto, lineas: lineas));

        Assert.Equal(ResultadoOperacionLineaCompra.ProductoInvalido, respuesta.Resultado);
        AssertLineasIguales(lineas, respuesta.Lineas);
    }

    // RF-27 / RF-28: con varios defectos se informa solo el primero, en el orden
    // producto → cantidad → precio de compra → precio de venta.
    [Fact]
    public async Task AgregarLinea_ConProductoInexistenteYCantidadYPrecioInvalidos_DevuelveElErrorDeProducto()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        var negocio = new CompraNegocio(db);

        RespuestaOperacionLineaCompra respuesta = await negocio.AgregarLineaAsync(
            SolicitudAgregar(
                Importes.ProductoInexistente,
                cantidad: "abc",
                precioCompra: "abc",
                precioVenta: "abc"));

        Assert.Equal(ResultadoOperacionLineaCompra.ProductoInvalido, respuesta.Resultado);
        Assert.Equal(MensajeSinProductoEnLaLinea(1), respuesta.Mensaje);
    }

    // RF-27 / RF-28: la cantidad se valida antes que los precios.
    [Fact]
    public async Task AgregarLinea_ConCantidadYPreciosInvalidos_DevuelveElErrorDeCantidad()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);

        RespuestaOperacionLineaCompra respuesta = await negocio.AgregarLineaAsync(
            SolicitudAgregar(producto.IdProducto, cantidad: "abc", precioCompra: "abc", precioVenta: "abc"));

        Assert.Equal(ResultadoOperacionLineaCompra.CantidadInvalida, respuesta.Resultado);
        Assert.Equal(MensajeCantidadInvalida, respuesta.Mensaje);
    }

    // RF-27 / RF-28: el precio de compra se valida antes que el de venta.
    [Fact]
    public async Task AgregarLinea_ConAmbosPreciosInvalidos_DevuelveElErrorDelPrecioDeCompra()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);

        RespuestaOperacionLineaCompra respuesta = await negocio.AgregarLineaAsync(
            SolicitudAgregar(producto.IdProducto, precioCompra: "abc", precioVenta: "10,333"));

        Assert.Equal(ResultadoOperacionLineaCompra.PrecioInvalido, respuesta.Resultado);
        Assert.Equal(CampoPrecio.PrecioCompra, respuesta.CampoPrecio);
        Assert.Equal(MotivoPrecioInvalido.FormatoInvalido, respuesta.MotivoPrecio);
        Assert.Equal(MensajePrecioCompraFormato, respuesta.Mensaje);
    }

    // RF-18 / RF-33: alta válida: agrega la línea y calcula el subtotal.
    [Fact]
    public async Task AgregarLinea_ConDatosValidos_AgregaLaLineaYCalculaElSubtotal()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);

        RespuestaOperacionLineaCompra respuesta = await negocio.AgregarLineaAsync(
            SolicitudAgregar(producto.IdProducto, cantidad: "3", precioCompra: "10,33", precioVenta: "15"));

        Assert.Equal(ResultadoOperacionLineaCompra.Exito, respuesta.Resultado);
        LineaCompraDto linea = Assert.Single(respuesta.Lineas);
        Assert.Equal(producto.IdProducto, linea.IdProducto);
        Assert.Equal("COD1", linea.CodigoProducto);
        Assert.Equal("Uno", linea.NombreProducto);
        Assert.Equal(3, linea.Cantidad);
        Assert.Equal(10.33m, linea.PrecioCompra);
        Assert.Equal(15m, linea.PrecioVenta);
        Assert.Equal(30.99m, linea.Subtotal);
        Assert.Equal(0, respuesta.IndiceLineaAfectada);
        Assert.Equal(30.99m, respuesta.MontoTotal);
    }

    // ---------- G5 · Operaciones sobre el detalle (RF-29, RF-30, RF-36, RF-37, RF-42, RF-43, RF-62, RF-63) ----------

    // RF-42: agregar un producto nuevo lo coloca al final y suma al total.
    [Fact]
    public async Task AgregarLinea_ConProductoNuevo_LoAgregaAlFinalYSumaAlTotal()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Producto uno = await SembrarProductoAsync(db, "COD1", "Uno");
        Producto dos = await SembrarProductoAsync(db, "COD2", "Dos");
        var negocio = new CompraNegocio(db);
        IReadOnlyList<LineaCompraDto> lineas = new[] { Linea(uno.IdProducto, "COD1", "Uno", 2, 10m, 12m) };

        RespuestaOperacionLineaCompra respuesta = await negocio.AgregarLineaAsync(
            SolicitudAgregar(dos.IdProducto, cantidad: "3", precioCompra: "20", precioVenta: "25", lineas: lineas));

        Assert.Equal(ResultadoOperacionLineaCompra.Exito, respuesta.Resultado);
        Assert.Equal(2, respuesta.Lineas.Count);
        Assert.Equal(dos.IdProducto, respuesta.Lineas[1].IdProducto);
        Assert.Equal(80m, respuesta.MontoTotal);
    }

    // RF-36 / RF-43: producto repetido ⇒ acumula la cantidad y toma los últimos
    // precios ingresados, sin crear una segunda línea.
    [Fact]
    public async Task AgregarLinea_ConProductoRepetido_AcumulaLaCantidadConLosUltimosPrecios()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);
        IReadOnlyList<LineaCompraDto> lineas = new[] { Linea(producto.IdProducto, "COD1", "Uno", 2, 10m, 12m) };

        RespuestaOperacionLineaCompra respuesta = await negocio.AgregarLineaAsync(
            SolicitudAgregar(producto.IdProducto, cantidad: "3", precioCompra: "11,50", precioVenta: "15", lineas: lineas));

        Assert.Equal(ResultadoOperacionLineaCompra.ExitoConAcumulacion, respuesta.Resultado);
        LineaCompraDto linea = Assert.Single(respuesta.Lineas);
        Assert.Equal(5, linea.Cantidad);
        Assert.Equal(11.5m, linea.PrecioCompra);
        Assert.Equal(15m, linea.PrecioVenta);
        Assert.Equal(57.5m, linea.Subtotal);
        Assert.Equal(0, respuesta.IndiceLineaAfectada);
        Assert.Equal(57.5m, respuesta.MontoTotal);
    }

    // RF-63 / RNF-11: la acumulación que supera 1.000.000 rechaza y deja la línea intacta.
    [Fact]
    public async Task AgregarLinea_ConAcumulacionQueSuperaElMaximo_RechazaSinTocarLaLinea()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);
        IReadOnlyList<LineaCompraDto> lineas = new[]
        {
            Linea(producto.IdProducto, "COD1", "Uno", 999_999, 10m, 12m)
        };

        RespuestaOperacionLineaCompra respuesta = await negocio.AgregarLineaAsync(
            SolicitudAgregar(producto.IdProducto, cantidad: "2", precioCompra: "20", precioVenta: "30", lineas: lineas));

        Assert.Equal(ResultadoOperacionLineaCompra.CantidadAcumuladaSuperaMaximo, respuesta.Resultado);
        AssertLineasIguales(lineas, respuesta.Lineas);
        Assert.Equal(9_999_990m, respuesta.MontoTotal);
    }

    // RF-63: la acumulación que llega justo al máximo se acepta.
    [Fact]
    public async Task AgregarLinea_ConAcumulacionHastaElMaximo_LaAcepta()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);
        IReadOnlyList<LineaCompraDto> lineas = new[]
        {
            Linea(producto.IdProducto, "COD1", "Uno", 999_999, 10m, 12m)
        };

        RespuestaOperacionLineaCompra respuesta = await negocio.AgregarLineaAsync(
            SolicitudAgregar(producto.IdProducto, cantidad: "1", lineas: lineas));

        Assert.Equal(ResultadoOperacionLineaCompra.ExitoConAcumulacion, respuesta.Resultado);
        LineaCompraDto linea = Assert.Single(respuesta.Lineas);
        Assert.Equal(1_000_000, linea.Cantidad);
    }

    // RF-37: nunca quedan dos líneas del mismo producto.
    [Fact]
    public async Task AgregarLinea_DosVecesElMismoProducto_DejaUnaSolaLinea()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);

        RespuestaOperacionLineaCompra primera = await negocio.AgregarLineaAsync(
            SolicitudAgregar(producto.IdProducto, cantidad: "1"));
        RespuestaOperacionLineaCompra segunda = await negocio.AgregarLineaAsync(
            SolicitudAgregar(producto.IdProducto, cantidad: "2", lineas: primera.Lineas));

        Assert.Equal(ResultadoOperacionLineaCompra.ExitoConAcumulacion, segunda.Resultado);
        Assert.Single(segunda.Lineas);
        Assert.Equal(3, segunda.Lineas[0].Cantidad);
    }

    // RF-30: editar reemplaza la línea del índice, sin duplicarla ni agregar filas.
    [Fact]
    public async Task EditarLinea_ConIndiceValido_ReemplazaLaLineaSinDuplicarla()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Producto uno = await SembrarProductoAsync(db, "COD1", "Uno");
        Producto dos = await SembrarProductoAsync(db, "COD2", "Dos");
        Producto tres = await SembrarProductoAsync(db, "COD3", "Tres");
        var negocio = new CompraNegocio(db);
        IReadOnlyList<LineaCompraDto> lineas = new[]
        {
            Linea(uno.IdProducto, "COD1", "Uno", 1, 10m, 12m),
            Linea(dos.IdProducto, "COD2", "Dos", 2, 20m, 25m)
        };

        RespuestaOperacionLineaCompra respuesta = await negocio.EditarLineaAsync(
            SolicitudEditar(0, tres.IdProducto, cantidad: "4", precioCompra: "7,25", precioVenta: "9", lineas: lineas));

        Assert.Equal(ResultadoOperacionLineaCompra.Exito, respuesta.Resultado);
        Assert.Equal(2, respuesta.Lineas.Count);
        Assert.Equal(tres.IdProducto, respuesta.Lineas[0].IdProducto);
        Assert.Equal("COD3", respuesta.Lineas[0].CodigoProducto);
        Assert.Equal(4, respuesta.Lineas[0].Cantidad);
        Assert.Equal(29m, respuesta.Lineas[0].Subtotal);
        Assert.Equal(dos.IdProducto, respuesta.Lineas[1].IdProducto);
        Assert.Equal(69m, respuesta.MontoTotal);
    }

    // RF-30 / RNF-3: el índice se valida antes de consultar la base: con el
    // contexto ya dispuesto la respuesta es LineaNoEncontrada y no un error de datos.
    [Fact]
    public async Task EditarLinea_ConIndiceFueraDeRango_NoConsultaLaBase()
    {
        AppDbContext db = await CrearContextoVacioAsync();
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);
        IReadOnlyList<LineaCompraDto> lineas = new[] { Linea(producto.IdProducto, "COD1", "Uno", 1, 10m, 12m) };
        await db.DisposeAsync();

        RespuestaOperacionLineaCompra respuesta = await negocio.EditarLineaAsync(
            SolicitudEditar(5, producto.IdProducto, lineas: lineas));

        Assert.Equal(ResultadoOperacionLineaCompra.LineaNoEncontrada, respuesta.Resultado);
        AssertLineasIguales(lineas, respuesta.Lineas);
    }

    // RF-62 / D-18 / RNF-11: si el producto ya está en otra línea, no se modifica ninguna.
    [Fact]
    public async Task EditarLinea_ConProductoDeOtraLinea_RechazaSinModificarNingunaLinea()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Producto uno = await SembrarProductoAsync(db, "COD1", "Uno");
        Producto dos = await SembrarProductoAsync(db, "COD2", "Dos");
        var negocio = new CompraNegocio(db);
        IReadOnlyList<LineaCompraDto> lineas = new[]
        {
            Linea(uno.IdProducto, "COD1", "Uno", 1, 10m, 12m),
            Linea(dos.IdProducto, "COD2", "Dos", 2, 20m, 25m)
        };

        RespuestaOperacionLineaCompra respuesta = await negocio.EditarLineaAsync(
            SolicitudEditar(1, uno.IdProducto, cantidad: "5", lineas: lineas));

        Assert.Equal(ResultadoOperacionLineaCompra.ProductoDuplicadoEnEdicion, respuesta.Resultado);
        AssertLineasIguales(lineas, respuesta.Lineas);
        Assert.Equal(50m, respuesta.MontoTotal);
    }

    // RF-30 / RF-37: editar conservando el propio producto no es duplicado.
    [Fact]
    public async Task EditarLinea_ManteniendoElMismoProducto_NoLoConsideraDuplicado()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);
        IReadOnlyList<LineaCompraDto> lineas = new[]
        {
            Linea(producto.IdProducto, "COD1", "Uno", 1, 10m, 12m),
            Linea(999, "COD9", "Nueve", 1, 5m, 6m)
        };

        RespuestaOperacionLineaCompra respuesta = await negocio.EditarLineaAsync(
            SolicitudEditar(0, producto.IdProducto, cantidad: "6", precioCompra: "10", lineas: lineas));

        Assert.Equal(ResultadoOperacionLineaCompra.Exito, respuesta.Resultado);
        Assert.Equal(6, respuesta.Lineas[0].Cantidad);
        Assert.Equal(60m, respuesta.Lineas[0].Subtotal);
        Assert.Equal(65m, respuesta.MontoTotal);
    }

    // RF-29: quitar conserva el orden de las líneas restantes.
    [Fact]
    public async Task QuitarLinea_DeUnaListaDeTres_ConservaElOrdenDeLasRestantes()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        var negocio = new CompraNegocio(db);
        IReadOnlyList<LineaCompraDto> lineas = new[]
        {
            Linea(1, "COD1", "Uno", 1, 10m, 12m),
            Linea(2, "COD2", "Dos", 2, 20m, 25m),
            Linea(3, "COD3", "Tres", 3, 30m, 35m)
        };

        RespuestaCompraEnCarga respuesta = negocio.QuitarLinea(lineas, 1);

        Assert.True(respuesta.Exito);
        Assert.Equal(2, respuesta.Lineas.Count);
        Assert.Equal(1, respuesta.Lineas[0].IdProducto);
        Assert.Equal(3, respuesta.Lineas[1].IdProducto);
        Assert.Equal(100m, respuesta.MontoTotal);
    }

    // ---------- G7 · Búsquedas acotadas (RF-9 … RF-12, RF-15 … RF-17) ----------

    // RF-9 / RF-15: coincidencia parcial por documento o razón social / código o
    // nombre, sin distinguir mayúsculas y minúsculas.
    [Theory]
    [InlineData("2045")]
    [InlineData("ferre")]
    [InlineData("FERRE")]
    public async Task BuscarProveedores_ConTextoParcial_DevuelveElProveedorActivo(string texto)
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor buscado = await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        await SembrarProveedorAsync(db, "30712345678", "Distribuidora Norte");
        var negocio = new CompraNegocio(db);

        RespuestaBusquedaProveedoresCompra respuesta = await negocio.BuscarProveedoresAsync(texto);

        Assert.Equal(ResultadoBusquedaCompras.ConResultados, respuesta.Resultado);
        ProveedorSeleccionadoDto proveedor = Assert.Single(respuesta.Proveedores);
        Assert.Equal(buscado.IdProveedor, proveedor.IdProveedor);
        Assert.Equal("Ferretería Central", proveedor.RazonSocial);
        Assert.Equal("20451234567", proveedor.Documento);
    }

    // RF-9 / RF-12: los proveedores inactivos no aparecen en los resultados.
    [Fact]
    public async Task BuscarProveedores_ConCoincidenciaInactiva_DevuelveSinResultados()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        await SembrarProveedorAsync(db, "20451234567", "Ferretería Central", estado: false);
        var negocio = new CompraNegocio(db);

        RespuestaBusquedaProveedoresCompra respuesta = await negocio.BuscarProveedoresAsync("ferretería");

        Assert.Equal(ResultadoBusquedaCompras.SinResultados, respuesta.Resultado);
        Assert.Empty(respuesta.Proveedores);
        Assert.Equal(MensajeSinProveedoresCoincidentes, respuesta.Mensaje);
    }

    // RF-11: sin coincidencias ⇒ resultado no encontrado y mensaje del catálogo.
    [Fact]
    public async Task BuscarProveedores_SinCoincidencias_DevuelveElMensajeDelCatalogo()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        var negocio = new CompraNegocio(db);

        RespuestaBusquedaProveedoresCompra respuesta = await negocio.BuscarProveedoresAsync("panadería");

        Assert.Equal(ResultadoBusquedaCompras.SinResultados, respuesta.Resultado);
        Assert.Empty(respuesta.Proveedores);
        Assert.Equal(MensajeSinProveedoresCoincidentes, respuesta.Mensaje);
    }

    // RF-9: tope de 20 resultados, en el orden razón social ascendente + id.
    [Fact]
    public async Task BuscarProveedores_ConMasDe20Coincidencias_DevuelveLasPrimeras20Ordenadas()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        for (int i = 25; i >= 1; i--)
        {
            await SembrarProveedorAsync(db, $"3000000000{i:D2}", $"Proveedor {i:D2}");
        }
        var negocio = new CompraNegocio(db);

        RespuestaBusquedaProveedoresCompra respuesta = await negocio.BuscarProveedoresAsync("proveedor");

        Assert.Equal(ResultadoBusquedaCompras.ConResultados, respuesta.Resultado);
        Assert.Equal(20, respuesta.Proveedores.Count);
        Assert.Equal("Proveedor 01", respuesta.Proveedores[0].RazonSocial);
        Assert.Equal("Proveedor 20", respuesta.Proveedores[19].RazonSocial);
        Assert.Equal(string.Format(MensajeProveedoresEncontradosFormato, 20), respuesta.Mensaje);
    }

    // RF-9: ante razones sociales iguales desempata el identificador ascendente.
    [Fact]
    public async Task BuscarProveedores_ConMismaRazonSocial_DesempataPorIdAscendente()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor centralUno = await SembrarProveedorAsync(db, "20000000001", "Ferretería Central");
        Proveedor centralDos = await SembrarProveedorAsync(db, "20000000002", "Ferretería Central");
        Proveedor antigua = await SembrarProveedorAsync(db, "20000000003", "Ferretería Antigua");
        var negocio = new CompraNegocio(db);

        RespuestaBusquedaProveedoresCompra respuesta = await negocio.BuscarProveedoresAsync("ferretería");

        Assert.Equal(3, respuesta.Proveedores.Count);
        Assert.Equal(antigua.IdProveedor, respuesta.Proveedores[0].IdProveedor);
        Assert.Equal(centralUno.IdProveedor, respuesta.Proveedores[1].IdProveedor);
        Assert.Equal(centralDos.IdProveedor, respuesta.Proveedores[2].IdProveedor);
    }

    // RF-9: mensaje del catálogo con un solo resultado.
    [Fact]
    public async Task BuscarProveedores_ConUnResultado_DevuelveElTextoDelCatalogo()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        var negocio = new CompraNegocio(db);

        RespuestaBusquedaProveedoresCompra respuesta = await negocio.BuscarProveedoresAsync("2045");

        Assert.Equal(MensajeUnProveedorEncontrado, respuesta.Mensaje);
    }

    // RNF-5: la proyección de Compras no expone correo ni teléfono.
    [Fact]
    public async Task BuscarProveedores_DevuelveUnaProyeccionSinCorreoNiTelefono()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        var negocio = new CompraNegocio(db);

        RespuestaBusquedaProveedoresCompra respuesta = await negocio.BuscarProveedoresAsync("ferre");

        Assert.Equal(ResultadoBusquedaCompras.ConResultados, respuesta.Resultado);
        Assert.Single(respuesta.Proveedores);
        Assert.Null(typeof(ProveedorSeleccionadoDto).GetProperty("Correo"));
        Assert.Null(typeof(ProveedorSeleccionadoDto).GetProperty("Telefono"));
    }

    // RF-10 / plan §9.1: con el contexto dispuesto, obtener SinResultados y
    // mensaje vacío demuestra que el texto vacío no ejecuta ninguna consulta.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task BuscarProveedores_ConTextoVacio_NoConsultaLaBase(string? texto)
    {
        AppDbContext db = await CrearContextoVacioAsync();
        await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        var negocio = new CompraNegocio(db);
        await db.DisposeAsync();

        RespuestaBusquedaProveedoresCompra respuesta = await negocio.BuscarProveedoresAsync(texto);

        Assert.Equal(ResultadoBusquedaCompras.SinResultados, respuesta.Resultado);
        Assert.Empty(respuesta.Proveedores);
        Assert.Equal(string.Empty, respuesta.Mensaje);
    }

    // RF-15: coincidencia parcial por código o nombre, sin distinguir mayúsculas.
    [Theory]
    [InlineData("000011")]
    [InlineData("gase")]
    [InlineData("GASE")]
    public async Task BuscarProductos_ConTextoParcial_DevuelveElProductoActivo(string texto)
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Producto buscado = await SembrarProductoAsync(db, "7790001000011", "Gaseosa Cola");
        await SembrarProductoAsync(db, "7790001000028", "Agua Mineral");
        var negocio = new CompraNegocio(db);

        RespuestaBusquedaProductosCompra respuesta = await negocio.BuscarProductosAsync(texto);

        Assert.Equal(ResultadoBusquedaCompras.ConResultados, respuesta.Resultado);
        ProductoBuscadoDto producto = Assert.Single(respuesta.Productos);
        Assert.Equal(buscado.IdProducto, producto.IdProducto);
        Assert.Equal("7790001000011", producto.Codigo);
        Assert.Equal("Gaseosa Cola", producto.Nombre);
    }

    // RF-15: los productos inactivos no aparecen en los resultados.
    [Fact]
    public async Task BuscarProductos_ConCoincidenciaInactiva_DevuelveSinResultados()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        await SembrarProductoAsync(db, "7790001000011", "Gaseosa Cola", estado: false);
        var negocio = new CompraNegocio(db);

        RespuestaBusquedaProductosCompra respuesta = await negocio.BuscarProductosAsync("gaseosa");

        Assert.Equal(ResultadoBusquedaCompras.SinResultados, respuesta.Resultado);
        Assert.Empty(respuesta.Productos);
        Assert.Equal(MensajeSinProductosCoincidentes, respuesta.Mensaje);
    }

    // RF-17: sin coincidencias ⇒ mensaje del catálogo, sin tratarlo como error.
    [Fact]
    public async Task BuscarProductos_SinCoincidencias_DevuelveElMensajeDelCatalogo()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        await SembrarProductoAsync(db, "7790001000011", "Gaseosa Cola");
        var negocio = new CompraNegocio(db);

        RespuestaBusquedaProductosCompra respuesta = await negocio.BuscarProductosAsync("cerveza");

        Assert.Equal(ResultadoBusquedaCompras.SinResultados, respuesta.Resultado);
        Assert.Empty(respuesta.Productos);
        Assert.Equal(MensajeSinProductosCoincidentes, respuesta.Mensaje);
    }

    // RF-15: tope de 20 resultados, en el orden nombre ascendente + id.
    [Fact]
    public async Task BuscarProductos_ConMasDe20Coincidencias_DevuelveLasPrimeras20Ordenadas()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        for (int i = 25; i >= 1; i--)
        {
            await SembrarProductoAsync(db, $"COD{i:D3}", $"Producto {i:D2}");
        }
        var negocio = new CompraNegocio(db);

        RespuestaBusquedaProductosCompra respuesta = await negocio.BuscarProductosAsync("producto");

        Assert.Equal(ResultadoBusquedaCompras.ConResultados, respuesta.Resultado);
        Assert.Equal(20, respuesta.Productos.Count);
        Assert.Equal("Producto 01", respuesta.Productos[0].Nombre);
        Assert.Equal("Producto 20", respuesta.Productos[19].Nombre);
        Assert.Equal(string.Format(MensajeProductosEncontradosFormato, 20), respuesta.Mensaje);
    }

    // RF-15: ante nombres iguales desempata el identificador ascendente.
    [Fact]
    public async Task BuscarProductos_ConMismoNombre_DesempataPorIdAscendente()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Producto primero = await SembrarProductoAsync(db, "COD001", "Tornillo");
        Producto segundo = await SembrarProductoAsync(db, "COD002", "Tornillo");
        var negocio = new CompraNegocio(db);

        RespuestaBusquedaProductosCompra respuesta = await negocio.BuscarProductosAsync("tornillo");

        Assert.Equal(2, respuesta.Productos.Count);
        Assert.Equal(primero.IdProducto, respuesta.Productos[0].IdProducto);
        Assert.Equal(segundo.IdProducto, respuesta.Productos[1].IdProducto);
    }

    // RF-15: mensaje del catálogo con un solo resultado.
    [Fact]
    public async Task BuscarProductos_ConUnResultado_DevuelveElTextoDelCatalogo()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        await SembrarProductoAsync(db, "7790001000011", "Gaseosa Cola");
        var negocio = new CompraNegocio(db);

        RespuestaBusquedaProductosCompra respuesta = await negocio.BuscarProductosAsync("gase");

        Assert.Equal(MensajeUnProductoEncontrado, respuesta.Mensaje);
    }

    // RF-16 / plan §9.1: con el contexto dispuesto, la lista vacía sin mensaje
    // demuestra que el texto vacío no ejecuta ninguna consulta.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task BuscarProductos_ConTextoVacio_NoConsultaLaBase(string? texto)
    {
        AppDbContext db = await CrearContextoVacioAsync();
        await SembrarProductoAsync(db, "7790001000011", "Gaseosa Cola");
        var negocio = new CompraNegocio(db);
        await db.DisposeAsync();

        RespuestaBusquedaProductosCompra respuesta = await negocio.BuscarProductosAsync(texto);

        Assert.Equal(ResultadoBusquedaCompras.SinResultados, respuesta.Resultado);
        Assert.Empty(respuesta.Productos);
        Assert.Equal(string.Empty, respuesta.Mensaje);
    }

    // ---------- G6 · Prellenado de precios (RF-38 … RF-41) ----------

    // RF-38: precios actuales del catálogo.
    [Fact]
    public async Task ObtenerPreciosProducto_ConProductoActivo_DevuelveLosPreciosDelCatalogo()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Producto producto = await SembrarProductoAsync(
            db, "COD1", "Uno", precioCompra: 12.34m, precioVenta: 18.9m);
        var negocio = new CompraNegocio(db);

        RespuestaPreciosProducto respuesta = await negocio.ObtenerPreciosProductoAsync(producto.IdProducto);

        Assert.Equal(ResultadoPreciosProducto.Exito, respuesta.Resultado);
        Assert.Equal(MensajePreciosPropuestos, respuesta.Mensaje);
        Assert.NotNull(respuesta.Precios);
        Assert.Equal(producto.IdProducto, respuesta.Precios!.IdProducto);
        Assert.Equal(12.34m, respuesta.Precios.PrecioCompra);
        Assert.Equal(18.9m, respuesta.Precios.PrecioVenta);
    }

    // RF-39: un precio cargado en 0 se propone en 0 para que RF-22 o RF-23 lo rechacen.
    [Fact]
    public async Task ObtenerPreciosProducto_ConPreciosEnCero_LosDevuelveEnCero()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Producto producto = await SembrarProductoAsync(
            db, "COD1", "Uno", precioCompra: 0m, precioVenta: 0m);
        var negocio = new CompraNegocio(db);

        RespuestaPreciosProducto respuesta = await negocio.ObtenerPreciosProductoAsync(producto.IdProducto);

        Assert.Equal(ResultadoPreciosProducto.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Precios);
        Assert.Equal(0m, respuesta.Precios!.PrecioCompra);
        Assert.Equal(0m, respuesta.Precios.PrecioVenta);
    }

    // RF-38: producto inactivo ⇒ resultado propio y sin precios.
    [Fact]
    public async Task ObtenerPreciosProducto_ConProductoInactivo_DevuelveProductoInactivo()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno", estado: false);
        var negocio = new CompraNegocio(db);

        RespuestaPreciosProducto respuesta = await negocio.ObtenerPreciosProductoAsync(producto.IdProducto);

        Assert.Equal(ResultadoPreciosProducto.ProductoInactivo, respuesta.Resultado);
        Assert.Equal(MensajeProductoInactivoAlPrellenar, respuesta.Mensaje);
        Assert.Null(respuesta.Precios);
    }

    // RF-38: producto inexistente ⇒ resultado propio y sin precios.
    [Fact]
    public async Task ObtenerPreciosProducto_ConProductoInexistente_DevuelveProductoNoEncontrado()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        var negocio = new CompraNegocio(db);

        RespuestaPreciosProducto respuesta = await negocio.ObtenerPreciosProductoAsync(Importes.ProductoInexistente);

        Assert.Equal(ResultadoPreciosProducto.ProductoNoEncontrado, respuesta.Resultado);
        Assert.Equal(MensajeProductoNoEncontrado, respuesta.Mensaje);
        Assert.Null(respuesta.Precios);
    }

    // RF-41 / RF-47: prellenar no modifica stock, precios, usuario, fecha ni motivo.
    [Fact]
    public async Task ObtenerPreciosProducto_NoModificaElCatalogo()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        var producto = new Producto
        {
            IdCategoria = 1,
            Codigo = "COD1",
            Nombre = "Uno",
            Descripcion = "Dato previo de la prueba",
            PrecioCompra = 12.34m,
            PrecioVenta = 18.9m,
            Stock = 7,
            Estado = true,
            IdUsuario = 1,
            IdUsuarioModifica = 3,
            FechaModificacion = new DateTime(2026, 1, 2, 3, 4, 5),
            MotivoAjusteStock = "Ajuste previo de la prueba"
        };
        db.Productos.Add(producto);
        await db.SaveChangesAsync();

        Producto antes = await db.Productos.AsNoTracking().FirstAsync(p => p.IdProducto == producto.IdProducto);
        var negocio = new CompraNegocio(db);

        RespuestaPreciosProducto respuesta = await negocio.ObtenerPreciosProductoAsync(producto.IdProducto);

        Producto despues = await db.Productos.AsNoTracking().FirstAsync(p => p.IdProducto == producto.IdProducto);

        Assert.Equal(ResultadoPreciosProducto.Exito, respuesta.Resultado);
        Assert.Equal(antes.Stock, despues.Stock);
        Assert.Equal(antes.PrecioCompra, despues.PrecioCompra);
        Assert.Equal(antes.PrecioVenta, despues.PrecioVenta);
        Assert.Equal(antes.IdUsuarioModifica, despues.IdUsuarioModifica);
        Assert.Equal(antes.FechaModificacion, despues.FechaModificacion);
        Assert.Equal(antes.MotivoAjusteStock, despues.MotivoAjusteStock);
        Assert.Equal(0, db.ChangeTracker.Entries<Producto>().Count(e => e.State != EntityState.Unchanged));
    }

    // ---------- G11 · Fallos de lectura de búsquedas y prellenado (RF-57) ----------

    // RF-57: la búsqueda de proveedores no propaga la excepción y queda reintentable.
    [Fact]
    public async Task BuscarProveedores_ConContextoDispuesto_DevuelveErrorPersistencia()
    {
        AppDbContext db = await CrearContextoVacioAsync();
        await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        var negocio = new CompraNegocio(db);
        await db.DisposeAsync();

        RespuestaBusquedaProveedoresCompra respuesta = await negocio.BuscarProveedoresAsync("ferre");

        Assert.Equal(ResultadoBusquedaCompras.ErrorPersistencia, respuesta.Resultado);
        Assert.Equal(MensajeErrorBuscarProveedores, respuesta.Mensaje);
        Assert.Empty(respuesta.Proveedores);
    }

    // RF-57: la búsqueda de productos no propaga la excepción y queda reintentable.
    [Fact]
    public async Task BuscarProductos_ConContextoDispuesto_DevuelveErrorPersistencia()
    {
        AppDbContext db = await CrearContextoVacioAsync();
        await SembrarProductoAsync(db, "7790001000011", "Gaseosa Cola");
        var negocio = new CompraNegocio(db);
        await db.DisposeAsync();

        RespuestaBusquedaProductosCompra respuesta = await negocio.BuscarProductosAsync("gase");

        Assert.Equal(ResultadoBusquedaCompras.ErrorPersistencia, respuesta.Resultado);
        Assert.Equal(MensajeErrorBuscarProductos, respuesta.Mensaje);
        Assert.Empty(respuesta.Productos);
    }

    // RF-57: el prellenado con fallo de lectura devuelve ErrorPersistencia y sin precios.
    [Fact]
    public async Task ObtenerPreciosProducto_ConContextoDispuesto_DevuelveErrorPersistencia()
    {
        AppDbContext db = await CrearContextoVacioAsync();
        await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);
        await db.DisposeAsync();

        RespuestaPreciosProducto respuesta = await negocio.ObtenerPreciosProductoAsync(1);

        Assert.Equal(ResultadoPreciosProducto.ErrorPersistencia, respuesta.Resultado);
        Assert.Equal(MensajeErrorPreciosProducto, respuesta.Mensaje);
        Assert.Null(respuesta.Precios);
    }

    // ---------- G8 · Registro exitoso (RF-4, RF-6, RF-7, RF-33, RF-44, RF-46 … RF-48) ----------

    // RF-44 / RF-4: una línea se persiste con cabecera, fecha y estado.
    [Fact]
    public async Task RegistrarCompra_ConUnaLinea_PersisteLaCabeceraYElDetalle()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor proveedor = await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        Usuario usuario = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);
        var solicitud = SolicitudRegistrar(
            proveedor.IdProveedor, Linea(producto.IdProducto, "COD1", "Uno", 3, 10.33m, 15m));

        DateTime antes = DateTime.Now;
        RespuestaRegistroCompra respuesta = await negocio.RegistrarCompraAsync(
            solicitud, usuario.IdUsuario, "Administrador");
        DateTime despues = DateTime.Now;

        Assert.Equal(ResultadoRegistroCompra.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.IdCompra);
        Assert.Equal(
            string.Format(MensajeRegistroExitosoFormato, respuesta.IdCompra!.Value), respuesta.Mensaje);
        Assert.Equal(30.99m, respuesta.MontoTotal);
        Assert.InRange(respuesta.FechaRegistro!.Value, antes, despues);

        Compra cabecera = await db.Compras.AsNoTracking().SingleAsync();
        Assert.Equal(respuesta.IdCompra, cabecera.IdCompra);
        Assert.Equal(proveedor.IdProveedor, cabecera.IdProveedor);
        Assert.Equal(usuario.IdUsuario, cabecera.IdUsuario);
        Assert.Equal(30.99m, cabecera.MontoTotal);
        Assert.True(cabecera.Estado);
        Assert.InRange(cabecera.FechaRegistro, antes, despues);

        DetalleCompra detalle = await db.DetallesCompra.AsNoTracking().SingleAsync();
        Assert.Equal(cabecera.IdCompra, detalle.IdCompra);
        Assert.Equal(producto.IdProducto, detalle.IdProducto);
        Assert.Equal(3, detalle.Cantidad);
        Assert.Equal(10.33m, detalle.PrecioCompra);
        Assert.Equal(30.99m, detalle.Subtotal);
    }

    // RF-7 / RF-33: varias líneas persisten con sus subtotales y el total exacto.
    [Fact]
    public async Task RegistrarCompra_ConVariasLineas_PersisteTodasYElMontoTotal()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor proveedor = await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        Usuario usuario = await SembrarUsuarioAsync(db);
        Producto uno = await SembrarProductoAsync(db, "COD1", "Uno");
        Producto dos = await SembrarProductoAsync(db, "COD2", "Dos");
        Producto tres = await SembrarProductoAsync(db, "COD3", "Tres");
        var negocio = new CompraNegocio(db);
        var solicitud = SolicitudRegistrar(
            proveedor.IdProveedor,
            Linea(uno.IdProducto, "COD1", "Uno", 2, 10.5m, 12m),
            Linea(dos.IdProducto, "COD2", "Dos", 3, 20m, 25m),
            Linea(tres.IdProducto, "COD3", "Tres", 1, 0.01m, 0.02m));

        RespuestaRegistroCompra respuesta = await negocio.RegistrarCompraAsync(
            solicitud, usuario.IdUsuario, "Encargado");

        Assert.Equal(ResultadoRegistroCompra.Exito, respuesta.Resultado);
        Assert.Equal(81.01m, respuesta.MontoTotal);

        Compra cabecera = await db.Compras.AsNoTracking().SingleAsync();
        Assert.Equal(81.01m, cabecera.MontoTotal);

        List<DetalleCompra> detalles = await db.DetallesCompra.AsNoTracking().ToListAsync();
        Assert.Equal(3, detalles.Count);
        Assert.Equal(81.01m, detalles.Sum(d => d.Subtotal));
        Assert.Equal(21m, detalles.Single(d => d.IdProducto == uno.IdProducto).Subtotal);
        Assert.Equal(60m, detalles.Single(d => d.IdProducto == dos.IdProducto).Subtotal);
        Assert.Equal(0.01m, detalles.Single(d => d.IdProducto == tres.IdProducto).Subtotal);
        Assert.Null(typeof(DetalleCompra).GetProperty("PrecioVenta"));
    }

    // RNF-13: el subtotal que llega en el DTO se ignora y se recalcula.
    [Fact]
    public async Task RegistrarCompra_ConSubtotalErroneoEnElDto_GuardaElRecalculado()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor proveedor = await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        Usuario usuario = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);
        var solicitud = SolicitudRegistrar(
            proveedor.IdProveedor,
            Linea(producto.IdProducto, "COD1", "Uno", 4, 5.5m, 8m, subtotal: 9_999m));

        RespuestaRegistroCompra respuesta = await negocio.RegistrarCompraAsync(
            solicitud, usuario.IdUsuario, "Administrador");

        Assert.Equal(ResultadoRegistroCompra.Exito, respuesta.Resultado);
        Assert.Equal(22m, respuesta.MontoTotal);
        DetalleCompra detalle = await db.DetallesCompra.AsNoTracking().SingleAsync();
        Assert.Equal(22m, detalle.Subtotal);
        Assert.Equal(4, detalle.Cantidad);
        Assert.Equal(5.5m, detalle.PrecioCompra);
    }


    // RF-47 / RF-48: registrar no modifica stock, precios, usuario de modificación,
    // fecha de modificación ni motivo de ajuste del producto.
    [Fact]
    public async Task RegistrarCompra_NoModificaElCatalogoDeProductos()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor proveedor = await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        Usuario usuario = await SembrarUsuarioAsync(db);
        var producto = new Producto
        {
            IdCategoria = 1,
            Codigo = "COD1",
            Nombre = "Uno",
            Descripcion = "Dato previo de la prueba",
            PrecioCompra = 12.34m,
            PrecioVenta = 18.9m,
            Stock = 7,
            Estado = true,
            IdUsuario = 1,
            IdUsuarioModifica = 3,
            FechaModificacion = new DateTime(2026, 1, 2, 3, 4, 5),
            MotivoAjusteStock = "Ajuste previo de la prueba"
        };
        db.Productos.Add(producto);
        await db.SaveChangesAsync();
        var negocio = new CompraNegocio(db);
        Producto antes = await LeerProductoAsync(db, producto.IdProducto);

        RespuestaRegistroCompra respuesta = await negocio.RegistrarCompraAsync(
            SolicitudRegistrar(
                proveedor.IdProveedor, Linea(producto.IdProducto, "COD1", "Uno", 2, 12.34m, 18.9m)),
            usuario.IdUsuario,
            "Administrador");

        Producto despues = await LeerProductoAsync(db, producto.IdProducto);

        Assert.Equal(ResultadoRegistroCompra.Exito, respuesta.Resultado);
        Assert.Equal(antes.Stock, despues.Stock);
        Assert.Equal(antes.PrecioCompra, despues.PrecioCompra);
        Assert.Equal(antes.PrecioVenta, despues.PrecioVenta);
        Assert.Equal(antes.IdUsuarioModifica, despues.IdUsuarioModifica);
        Assert.Equal(antes.FechaModificacion, despues.FechaModificacion);
        Assert.Equal(antes.MotivoAjusteStock, despues.MotivoAjusteStock);
        Assert.Equal(0, db.ChangeTracker.Entries<Producto>().Count(e => e.State != EntityState.Unchanged));
    }

    // ---------- G9 · Registro rechazado (RF-5, RF-14, RF-18, RF-19, RF-24, RF-26 … RF-28, RF-37, RF-64) ----------

    // RF-14: proveedor obligatorio; con null, 0 o negativo no se escribe nada.
    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task RegistrarCompra_SinProveedor_DevuelveProveedorInvalido(int? idProveedor)
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Usuario usuario = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);

        RespuestaRegistroCompra respuesta = await negocio.RegistrarCompraAsync(
            SolicitudRegistrar(idProveedor, Linea(producto.IdProducto, "COD1", "Uno", 1, 10m, 12m)),
            usuario.IdUsuario,
            "Administrador");

        Assert.Equal(ResultadoRegistroCompra.ProveedorInvalido, respuesta.Resultado);
        Assert.Equal(MensajeProveedorRequerido, respuesta.Mensaje);
        Assert.Null(respuesta.IdCompra);
        await AssertSinComprasAsync(db);
    }

    // RF-14: proveedor inexistente.
    [Fact]
    public async Task RegistrarCompra_ConProveedorInexistente_DevuelveProveedorInvalido()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Usuario usuario = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);

        RespuestaRegistroCompra respuesta = await negocio.RegistrarCompraAsync(
            SolicitudRegistrar(
                Importes.ProveedorInexistente, Linea(producto.IdProducto, "COD1", "Uno", 1, 10m, 12m)),
            usuario.IdUsuario,
            "Administrador");

        Assert.Equal(ResultadoRegistroCompra.ProveedorInvalido, respuesta.Resultado);
        Assert.Equal(MensajeProveedorInexistente, respuesta.Mensaje);
        await AssertSinComprasAsync(db);
    }

    // RF-14: proveedor inactivo, identificado por su razón social.
    [Fact]
    public async Task RegistrarCompra_ConProveedorInactivo_DevuelveProveedorInvalido()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor proveedor = await SembrarProveedorAsync(db, "20451234567", "Ferretería Central", estado: false);
        Usuario usuario = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);

        RespuestaRegistroCompra respuesta = await negocio.RegistrarCompraAsync(
            SolicitudRegistrar(
                proveedor.IdProveedor, Linea(producto.IdProducto, "COD1", "Uno", 1, 10m, 12m)),
            usuario.IdUsuario,
            "Administrador");

        Assert.Equal(ResultadoRegistroCompra.ProveedorInvalido, respuesta.Resultado);
        Assert.Equal(string.Format(MensajeProveedorInactivoFormato, "Ferretería Central"), respuesta.Mensaje);
        await AssertSinComprasAsync(db);
    }

    // RF-14 / D-11: el proveedor se da de baja después de seleccionarlo.
    [Fact]
    public async Task RegistrarCompra_ConProveedorDadoDeBajaDespuesDeSeleccionarlo_DevuelveProveedorInvalido()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor proveedor = await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        Usuario usuario = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);
        var solicitud = SolicitudRegistrar(
            proveedor.IdProveedor, Linea(producto.IdProducto, "COD1", "Uno", 1, 10m, 12m));

        proveedor.Estado = false;
        await db.SaveChangesAsync();

        RespuestaRegistroCompra respuesta = await negocio.RegistrarCompraAsync(
            solicitud, usuario.IdUsuario, "Administrador");

        Assert.Equal(ResultadoRegistroCompra.ProveedorInvalido, respuesta.Resultado);
        Assert.Equal(string.Format(MensajeProveedorInactivoFormato, "Ferretería Central"), respuesta.Mensaje);
        await AssertSinComprasAsync(db);
    }

    // RF-5: usuario de sesión sin identificar; no se consulta ni se escribe.
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task RegistrarCompra_ConUsuarioSinIdentificar_DevuelveUsuarioInvalido(int idUsuario)
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor proveedor = await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);

        RespuestaRegistroCompra respuesta = await negocio.RegistrarCompraAsync(
            SolicitudRegistrar(
                proveedor.IdProveedor, Linea(producto.IdProducto, "COD1", "Uno", 1, 10m, 12m)),
            idUsuario,
            "Administrador");

        Assert.Equal(ResultadoRegistroCompra.UsuarioInvalido, respuesta.Resultado);
        Assert.Equal(MensajeUsuarioSinIdentificar, respuesta.Mensaje);
        await AssertSinComprasAsync(db);
    }

    // RF-5: usuario inexistente.
    [Fact]
    public async Task RegistrarCompra_ConUsuarioInexistente_DevuelveUsuarioInvalido()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor proveedor = await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);

        RespuestaRegistroCompra respuesta = await negocio.RegistrarCompraAsync(
            SolicitudRegistrar(
                proveedor.IdProveedor, Linea(producto.IdProducto, "COD1", "Uno", 1, 10m, 12m)),
            Importes.UsuarioInexistente,
            "Administrador");

        Assert.Equal(ResultadoRegistroCompra.UsuarioInvalido, respuesta.Resultado);
        Assert.Equal(MensajeUsuarioInexistente, respuesta.Mensaje);
        await AssertSinComprasAsync(db);
    }

    // RF-5 / D-14: usuario inactivo (sin releer el rol de la base).
    [Fact]
    public async Task RegistrarCompra_ConUsuarioInactivo_DevuelveUsuarioInvalido()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor proveedor = await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        Usuario usuario = await SembrarUsuarioAsync(db, estado: false);
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);

        RespuestaRegistroCompra respuesta = await negocio.RegistrarCompraAsync(
            SolicitudRegistrar(
                proveedor.IdProveedor, Linea(producto.IdProducto, "COD1", "Uno", 1, 10m, 12m)),
            usuario.IdUsuario,
            "Administrador");

        Assert.Equal(ResultadoRegistroCompra.UsuarioInvalido, respuesta.Resultado);
        Assert.Equal(MensajeUsuarioInactivo, respuesta.Mensaje);
        await AssertSinComprasAsync(db);
    }

    // RF-19: sin líneas.
    [Fact]
    public async Task RegistrarCompra_SinLineas_DevuelveSinItems()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor proveedor = await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        Usuario usuario = await SembrarUsuarioAsync(db);
        var negocio = new CompraNegocio(db);

        RespuestaRegistroCompra respuesta = await negocio.RegistrarCompraAsync(
            new SolicitudRegistrarCompraDto { IdProveedor = proveedor.IdProveedor },
            usuario.IdUsuario,
            "Administrador");

        Assert.Equal(ResultadoRegistroCompra.SinItems, respuesta.Resultado);
        Assert.Equal(MensajeSinItems, respuesta.Mensaje);
        await AssertSinComprasAsync(db);
    }

    // RF-18: el producto se desactiva entre la carga de la línea y la confirmación.
    [Fact]
    public async Task RegistrarCompra_ConProductoDesactivadoDespuesDeLaCarga_DevuelveProductoInvalido()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor proveedor = await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        Usuario usuario = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);
        var solicitud = SolicitudRegistrar(
            proveedor.IdProveedor, Linea(producto.IdProducto, "COD1", "Uno", 1, 10m, 12m));

        producto.Estado = false;
        await db.SaveChangesAsync();

        RespuestaRegistroCompra respuesta = await negocio.RegistrarCompraAsync(
            solicitud, usuario.IdUsuario, "Administrador");

        Assert.Equal(ResultadoRegistroCompra.ProductoInvalido, respuesta.Resultado);
        Assert.Equal(string.Format(MensajeProductoInactivoFormato, "COD1", "Uno", 1), respuesta.Mensaje);
        Assert.Equal(0, respuesta.IndiceLineaAfectada);
        Assert.Equal(producto.IdProducto, respuesta.IdProductoAfectado);
        await AssertSinComprasAsync(db);
    }

    // RF-37 / RNF-6: dos líneas del mismo producto ⇒ rechazo con el mensaje de
    // producto repetido y 0 filas.
    [Fact]
    public async Task RegistrarCompra_ConDosLineasDelMismoProducto_DevuelveProductoRepetido()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor proveedor = await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        Usuario usuario = await SembrarUsuarioAsync(db);
        Producto uno = await SembrarProductoAsync(db, "COD1", "Uno");
        Producto dos = await SembrarProductoAsync(db, "COD2", "Dos");
        var negocio = new CompraNegocio(db);

        RespuestaRegistroCompra respuesta = await negocio.RegistrarCompraAsync(
            SolicitudRegistrar(
                proveedor.IdProveedor,
                Linea(uno.IdProducto, "COD1", "Uno", 1, 10m, 12m),
                Linea(dos.IdProducto, "COD2", "Dos", 1, 20m, 25m),
                Linea(uno.IdProducto, "COD1", "Uno", 2, 10m, 12m)),
            usuario.IdUsuario,
            "Administrador");

        Assert.Equal(ResultadoRegistroCompra.ProductoInvalido, respuesta.Resultado);
        Assert.Equal(string.Format(MensajeProductoRepetidoFormato, "COD1", "Uno"), respuesta.Mensaje);
        Assert.Equal(2, respuesta.IndiceLineaAfectada);
        Assert.Equal(uno.IdProducto, respuesta.IdProductoAfectado);
        await AssertSinComprasAsync(db);
    }

    // RF-26 / plan §6.4: el precio de compra con tres decimales enviado directo
    // al negocio se revalida y deja 0 filas.
    [Fact]
    public async Task RegistrarCompra_ConPrecioDeCompraDeTresDecimales_DevuelveDecimalesExcedidos()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor proveedor = await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        Usuario usuario = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);

        RespuestaRegistroCompra respuesta = await negocio.RegistrarCompraAsync(
            SolicitudRegistrar(
                proveedor.IdProveedor, Linea(producto.IdProducto, "COD1", "Uno", 1, 10.333m, 12m)),
            usuario.IdUsuario,
            "Administrador");

        Assert.Equal(ResultadoRegistroCompra.PrecioInvalido, respuesta.Resultado);
        Assert.Equal(CampoPrecio.PrecioCompra, respuesta.CampoPrecio);
        Assert.Equal(MotivoPrecioInvalido.DecimalesExcedidos, respuesta.MotivoPrecio);
        Assert.Equal(MensajePrecioCompraDecimales, respuesta.Mensaje);
        Assert.Equal(0, respuesta.IndiceLineaAfectada);
        Assert.Equal(producto.IdProducto, respuesta.IdProductoAfectado);
        await AssertSinComprasAsync(db);
    }

    // RF-26: ídem con el precio de venta.
    [Fact]
    public async Task RegistrarCompra_ConPrecioDeVentaDeTresDecimales_DevuelveDecimalesExcedidos()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor proveedor = await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        Usuario usuario = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);

        RespuestaRegistroCompra respuesta = await negocio.RegistrarCompraAsync(
            SolicitudRegistrar(
                proveedor.IdProveedor, Linea(producto.IdProducto, "COD1", "Uno", 1, 10m, 10.333m)),
            usuario.IdUsuario,
            "Administrador");

        Assert.Equal(ResultadoRegistroCompra.PrecioInvalido, respuesta.Resultado);
        Assert.Equal(CampoPrecio.PrecioVenta, respuesta.CampoPrecio);
        Assert.Equal(MotivoPrecioInvalido.DecimalesExcedidos, respuesta.MotivoPrecio);
        Assert.Equal(MensajePrecioVentaDecimales, respuesta.Mensaje);
        await AssertSinComprasAsync(db);
    }

    // RF-24: precio de compra por encima del máximo ⇒ mensaje propio y 0 filas.
    [Fact]
    public async Task RegistrarCompra_ConPrecioDeCompraFueraDeRango_DevuelveSuperaMaximo()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor proveedor = await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        Usuario usuario = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);

        RespuestaRegistroCompra respuesta = await negocio.RegistrarCompraAsync(
            SolicitudRegistrar(
                proveedor.IdProveedor, Linea(producto.IdProducto, "COD1", "Uno", 1, 1_000_000_000m, 12m)),
            usuario.IdUsuario,
            "Administrador");

        Assert.Equal(ResultadoRegistroCompra.PrecioInvalido, respuesta.Resultado);
        Assert.Equal(CampoPrecio.PrecioCompra, respuesta.CampoPrecio);
        Assert.Equal(MotivoPrecioInvalido.SuperaMaximo, respuesta.MotivoPrecio);
        Assert.Equal(MensajePrecioCompraMaximo, respuesta.Mensaje);
        await AssertSinComprasAsync(db);
    }

    // RF-24: ídem con el precio de venta.
    [Fact]
    public async Task RegistrarCompra_ConPrecioDeVentaFueraDeRango_DevuelveSuperaMaximo()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor proveedor = await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        Usuario usuario = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        var negocio = new CompraNegocio(db);

        RespuestaRegistroCompra respuesta = await negocio.RegistrarCompraAsync(
            SolicitudRegistrar(
                proveedor.IdProveedor, Linea(producto.IdProducto, "COD1", "Uno", 1, 10m, 1_000_000_000m)),
            usuario.IdUsuario,
            "Administrador");

        Assert.Equal(ResultadoRegistroCompra.PrecioInvalido, respuesta.Resultado);
        Assert.Equal(CampoPrecio.PrecioVenta, respuesta.CampoPrecio);
        Assert.Equal(MotivoPrecioInvalido.SuperaMaximo, respuesta.MotivoPrecio);
        Assert.Equal(MensajePrecioVentaMaximo, respuesta.Mensaje);
        await AssertSinComprasAsync(db);
    }

    // RF-64 / D-20: 11 líneas de 1.000.000 × 999.999.999,99 superan el tope del total.
    [Fact]
    public async Task RegistrarCompra_ConMontoTotalSobreElTope_DevuelveMontoTotalSuperaMaximo()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor proveedor = await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        Usuario usuario = await SembrarUsuarioAsync(db);
        var lineas = new List<LineaCompraDto>();

        for (int i = 1; i <= 11; i++)
        {
            Producto producto = await SembrarProductoAsync(db, $"COD{i:D3}", $"Producto {i:D2}");
            lineas.Add(Linea(producto.IdProducto, $"COD{i:D3}", $"Producto {i:D2}", 1_000_000, 999_999_999.99m, 1m));
        }

        var negocio = new CompraNegocio(db);

        RespuestaRegistroCompra respuesta = await negocio.RegistrarCompraAsync(
            new SolicitudRegistrarCompraDto { IdProveedor = proveedor.IdProveedor, Lineas = lineas },
            usuario.IdUsuario,
            "Administrador");

        Assert.Equal(ResultadoRegistroCompra.MontoTotalSuperaMaximo, respuesta.Resultado);
        Assert.Equal(MensajeMontoTotalSuperaMaximo, respuesta.Mensaje);
        await AssertSinComprasAsync(db);
    }

    // RF-28 / §6.1: el proveedor se informa antes que el usuario y las líneas.
    [Fact]
    public async Task RegistrarCompra_ConProveedorInvalidoYUsuarioSinIdentificar_DevuelveElErrorDeProveedor()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        var negocio = new CompraNegocio(db);

        RespuestaRegistroCompra respuesta = await negocio.RegistrarCompraAsync(
            new SolicitudRegistrarCompraDto { IdProveedor = Importes.ProveedorInexistente }, 0, "Administrador");

        Assert.Equal(ResultadoRegistroCompra.ProveedorInvalido, respuesta.Resultado);
        Assert.Equal(MensajeProveedorInexistente, respuesta.Mensaje);
        await AssertSinComprasAsync(db);
    }

    // RF-28 / §6.1: el usuario se informa antes que las líneas y que «sin ítems».
    [Fact]
    public async Task RegistrarCompra_ConUsuarioInvalidoYSinLineas_DevuelveElErrorDeUsuario()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor proveedor = await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        var negocio = new CompraNegocio(db);

        RespuestaRegistroCompra respuesta = await negocio.RegistrarCompraAsync(
            new SolicitudRegistrarCompraDto { IdProveedor = proveedor.IdProveedor },
            Importes.UsuarioInexistente,
            "Administrador");

        Assert.Equal(ResultadoRegistroCompra.UsuarioInvalido, respuesta.Resultado);
        Assert.Equal(MensajeUsuarioInexistente, respuesta.Mensaje);
        await AssertSinComprasAsync(db);
    }

    // RF-28: con errores en dos líneas se informa el de la primera de la grilla.
    [Fact]
    public async Task RegistrarCompra_ConErroresEnDosLineas_DevuelveElDeLaPrimeraLinea()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor proveedor = await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        Usuario usuario = await SembrarUsuarioAsync(db);
        Producto uno = await SembrarProductoAsync(db, "COD1", "Uno");
        Producto dos = await SembrarProductoAsync(db, "COD2", "Dos");
        var negocio = new CompraNegocio(db);

        RespuestaRegistroCompra respuesta = await negocio.RegistrarCompraAsync(
            SolicitudRegistrar(
                proveedor.IdProveedor,
                Linea(uno.IdProducto, "COD1", "Uno", 1, 0m, 12m),
                Linea(dos.IdProducto, "COD2", "Dos", 0, 20m, 25m)),
            usuario.IdUsuario,
            "Administrador");

        Assert.Equal(ResultadoRegistroCompra.PrecioInvalido, respuesta.Resultado);
        Assert.Equal(CampoPrecio.PrecioCompra, respuesta.CampoPrecio);
        Assert.Equal(MotivoPrecioInvalido.NoPositivo, respuesta.MotivoPrecio);
        Assert.Equal(MensajePrecioCompraNoPositivo, respuesta.Mensaje);
        Assert.Equal(0, respuesta.IndiceLineaAfectada);
        await AssertSinComprasAsync(db);
    }

    // ---------- G10 · Atomicidad ante fallo de persistencia (RF-45, RF-46, RF-56) ----------

    // §9.1: el primer SaveChangesAsync falla ⇒ ErrorPersistencia con el texto
    // único, sin excepción propagada, sin IdCompra, con la solicitud intacta y
    // 0 filas de COMPRA/DETALLE_COMPRA comprobadas con el contexto sano. El
    // proveedor InMemory no ejecuta un rollback real: aquí se verifica que no
    // quede ninguna fila escrita y que la cabecera y las líneas queden
    // desadjuntadas; la atomicidad contra SQL Server se comprueba a mano en T10.
    [Fact]
    public async Task RegistrarCompra_ConFalloDePersistencia_DevuelveErrorPersistenciaSinFilas()
    {
        EscenarioDeFallo escenario = await CrearEscenarioDeFalloAsync();
        await using AppDbContext sano = escenario.Sano;
        await using var falloDb = new FalloDbContext(escenario.Opciones);
        var negocio = new CompraNegocio(falloDb);

        RespuestaRegistroCompra respuesta = await negocio.RegistrarCompraAsync(
            escenario.Solicitud, escenario.IdUsuario, "Administrador");

        Assert.Equal(ResultadoRegistroCompra.ErrorPersistencia, respuesta.Resultado);
        Assert.Equal(MensajeErrorPersistencia, respuesta.Mensaje);
        Assert.Null(respuesta.IdCompra);
        Assert.Equal(2, escenario.Solicitud.Lineas.Count);
        Assert.Empty(falloDb.ChangeTracker.Entries<Compra>());
        Assert.Empty(falloDb.ChangeTracker.Entries<DetalleCompra>());
        await AssertSinComprasAsync(sano);
    }

    // RF-45 / RNF-11: el registro fallido no deja nada rastreado ni líneas
    // tocadas, y la misma solicitud se vuelve a registrar con un contexto sano.
    [Fact]
    public async Task RegistrarCompra_TrasUnFalloDePersistencia_ElReintentoRegistraLaCompra()
    {
        EscenarioDeFallo escenario = await CrearEscenarioDeFalloAsync();
        await using AppDbContext sano = escenario.Sano;

        await using (var falloDb = new FalloDbContext(escenario.Opciones))
        {
            RespuestaRegistroCompra fallo = await new CompraNegocio(falloDb).RegistrarCompraAsync(
                escenario.Solicitud, escenario.IdUsuario, "Administrador");

            Assert.Equal(ResultadoRegistroCompra.ErrorPersistencia, fallo.Resultado);
            Assert.Null(fallo.IdCompra);
            await AssertSinComprasAsync(sano);
        }

        RespuestaRegistroCompra reintento = await new CompraNegocio(sano).RegistrarCompraAsync(
            escenario.Solicitud, escenario.IdUsuario, "Administrador");

        Assert.Equal(ResultadoRegistroCompra.Exito, reintento.Resultado);
        Assert.NotNull(reintento.IdCompra);
        Assert.Equal(81m, reintento.MontoTotal);

        Compra cabecera = await sano.Compras.AsNoTracking().SingleAsync();
        Assert.Equal(reintento.IdCompra, cabecera.IdCompra);
        Assert.Equal(81m, cabecera.MontoTotal);
        Assert.Equal(2, await sano.DetallesCompra.AsNoTracking().CountAsync());
        Assert.Equal(81m, (await sano.DetallesCompra.AsNoTracking().ToListAsync()).Sum(d => d.Subtotal));
    }

    // ---------- G12 · Historial de compras (RF-49 … RF-51, RF-53(a), RNF-5, RNF-12) ----------

    // RF-51: sin compras ⇒ SinCompras con el mensaje del catálogo.
    [Fact]
    public async Task ObtenerHistorial_SinCompras_DevuelveSinCompras()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();

        RespuestaHistorialCompras respuesta = await new CompraNegocio(db)
            .ObtenerHistorialAsync("Administrador");

        Assert.Equal(ResultadoHistorialCompras.SinCompras, respuesta.Resultado);
        Assert.Equal(MensajeHistorialVacio, respuesta.Mensaje);
        Assert.Empty(respuesta.Compras);
    }

    // RF-49/RF-50: orden descendente por fecha, resuelto en la consulta.
    [Fact]
    public async Task ObtenerHistorial_ConVariasCompras_OrdenaPorFechaDescendente()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor proveedor = await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        Usuario usuario = await SembrarUsuarioAsync(db);
        Compra enero = await SembrarCompraAsync(
            db, proveedor.IdProveedor, usuario.IdUsuario, new DateTime(2026, 1, 10, 8, 0, 0), 10m);
        Compra marzo = await SembrarCompraAsync(
            db, proveedor.IdProveedor, usuario.IdUsuario, new DateTime(2026, 3, 5, 18, 45, 0), 30m);
        Compra febrero = await SembrarCompraAsync(
            db, proveedor.IdProveedor, usuario.IdUsuario, new DateTime(2026, 2, 20, 12, 0, 0), 20m);

        RespuestaHistorialCompras respuesta = await new CompraNegocio(db)
            .ObtenerHistorialAsync("Administrador");

        Assert.Equal(ResultadoHistorialCompras.ConCompras, respuesta.Resultado);
        Assert.Equal(string.Format(MensajeComprasEncontradasFormato, 3), respuesta.Mensaje);
        Assert.Equal(
            new[] { marzo.IdCompra, febrero.IdCompra, enero.IdCompra },
            respuesta.Compras.Select(c => c.IdCompra));
        Assert.Equal(new[] { 30m, 20m, 10m }, respuesta.Compras.Select(c => c.MontoTotal));
    }

    // RF-50: con la misma fecha y hora se desempata por IdCompra descendente.
    [Fact]
    public async Task ObtenerHistorial_ConLaMismaFechaYHora_DesempataPorIdCompraDescendente()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor proveedor = await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        Usuario usuario = await SembrarUsuarioAsync(db);
        DateTime mismaFecha = new(2026, 4, 1, 10, 30, 0);
        Compra primera = await SembrarCompraAsync(db, proveedor.IdProveedor, usuario.IdUsuario, mismaFecha, 10m);
        Compra segunda = await SembrarCompraAsync(db, proveedor.IdProveedor, usuario.IdUsuario, mismaFecha, 20m);

        RespuestaHistorialCompras respuesta = await new CompraNegocio(db)
            .ObtenerHistorialAsync("Administrador");

        Assert.True(primera.IdCompra < segunda.IdCompra);
        Assert.Equal(
            new[] { segunda.IdCompra, primera.IdCompra },
            respuesta.Compras.Select(c => c.IdCompra));
    }

    // RF-49: las compras inactivas se listan con Estado = false (baja lógica).
    [Fact]
    public async Task ObtenerHistorial_ConComprasInactivas_LasIncluyeConEstadoFalse()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor proveedor = await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        Usuario usuario = await SembrarUsuarioAsync(db);
        Compra activa = await SembrarCompraAsync(
            db, proveedor.IdProveedor, usuario.IdUsuario, new DateTime(2026, 5, 1, 9, 0, 0), 10m);
        Compra inactiva = await SembrarCompraAsync(
            db, proveedor.IdProveedor, usuario.IdUsuario, new DateTime(2026, 5, 2, 9, 0, 0), 20m, estado: false);

        RespuestaHistorialCompras respuesta = await new CompraNegocio(db)
            .ObtenerHistorialAsync("Administrador");

        Assert.Equal(2, respuesta.Compras.Count);
        Assert.True(respuesta.Compras.Single(c => c.IdCompra == activa.IdCompra).Estado);
        Assert.False(respuesta.Compras.Single(c => c.IdCompra == inactiva.IdCompra).Estado);
        Assert.Equal(string.Format(MensajeComprasEncontradasFormato, 2), respuesta.Mensaje);
    }

    // RNF-12: la fecha viaja como DateTime, el estado como bool y el importe como
    // decimal; el negocio no formatea ni redondea nada.
    [Fact]
    public async Task ObtenerHistorial_DevuelveLosTiposCrudosSinFormatear()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor proveedor = await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        Usuario usuario = await SembrarUsuarioAsync(db);
        DateTime fecha = new(2026, 6, 7, 15, 7, 0);
        await SembrarCompraAsync(db, proveedor.IdProveedor, usuario.IdUsuario, fecha, 1234.5m);

        RespuestaHistorialCompras respuesta = await new CompraNegocio(db)
            .ObtenerHistorialAsync("Administrador");

        HistorialCompraDto fila = Assert.Single(respuesta.Compras);
        Assert.IsType<DateTime>(fila.FechaRegistro);
        Assert.IsType<bool>(fila.Estado);
        Assert.IsType<decimal>(fila.MontoTotal);
        Assert.Equal(fecha, fila.FechaRegistro);
        Assert.True(fila.Estado);
        Assert.Equal(1234.5m, fila.MontoTotal);
        Assert.Equal(MensajeUnaCompraEncontrada, respuesta.Mensaje);
    }

    // RF-49/RNF-5: nombre + apellido recortado y ningún dato interno del usuario.
    [Fact]
    public async Task ObtenerHistorial_DevuelveNombreYApellidoSinDatosInternos()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor proveedor = await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        Usuario usuario = await SembrarUsuarioAsync(db, nombre: " Ana ", apellido: "Gómez");
        await SembrarCompraAsync(
            db, proveedor.IdProveedor, usuario.IdUsuario, new DateTime(2026, 1, 1, 0, 0, 0), 10m);

        RespuestaHistorialCompras respuesta = await new CompraNegocio(db)
            .ObtenerHistorialAsync("Administrador");

        HistorialCompraDto fila = Assert.Single(respuesta.Compras);
        Assert.Equal("Ana Gómez", fila.NombreUsuario);
        Assert.Equal("Ferretería Central", fila.RazonSocialProveedor);
        Assert.Equal(
            new[] { "Estado", "FechaRegistro", "IdCompra", "MontoTotal", "NombreUsuario", "RazonSocialProveedor" },
            typeof(HistorialCompraDto).GetProperties().Select(p => p.Name).OrderBy(n => n, StringComparer.Ordinal));
    }

    // M11 / §3.4: navegaciones nulas ⇒ textos defensivos, sin excepción (InMemory
    // no valida claves foráneas, así que la compra se siembra con ids inexistentes).
    [Fact]
    public async Task ObtenerHistorial_ConProveedorYUsuarioInexistentes_DevuelveTextosDefensivos()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        await SembrarCompraAsync(
            db, Importes.ProveedorInexistente, Importes.UsuarioInexistente,
            new DateTime(2026, 7, 8, 11, 0, 0), 99m);

        RespuestaHistorialCompras respuesta = await new CompraNegocio(db)
            .ObtenerHistorialAsync("Administrador");

        HistorialCompraDto fila = Assert.Single(respuesta.Compras);
        Assert.Equal(TextoSinProveedor, fila.RazonSocialProveedor);
        Assert.Equal(TextoSinUsuario, fila.NombreUsuario);
        Assert.Equal(99m, fila.MontoTotal);
    }

    // ---------- G13 · Compra con detalle (RF-53(b), RF-54, D-17) ----------

    // RF-54: cabecera completa y líneas ordenadas por IdDetalleCompra.
    [Fact]
    public async Task ObtenerCompraConDetalle_ConLineas_DevuelveCabeceraYLineasOrdenadas()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor proveedor = await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        Usuario usuario = await SembrarUsuarioAsync(db);
        Producto uno = await SembrarProductoAsync(db, "COD1", "Uno");
        Producto dos = await SembrarProductoAsync(db, "COD2", "Dos");
        Producto tres = await SembrarProductoAsync(db, "COD3", "Tres");
        DateTime fecha = new(2026, 5, 6, 9, 30, 0);
        Compra compra = await SembrarCompraAsync(db, proveedor.IdProveedor, usuario.IdUsuario, fecha, 65m);
        await SembrarDetalleAsync(db, compra.IdCompra, tres.IdProducto, 1, 10m);
        await SembrarDetalleAsync(db, compra.IdCompra, uno.IdProducto, 2, 12.5m);
        await SembrarDetalleAsync(db, compra.IdCompra, dos.IdProducto, 3, 10m);

        RespuestaCompraConDetalle respuesta = await new CompraNegocio(db)
            .ObtenerCompraConDetalleAsync(compra.IdCompra, "Administrador");

        Assert.Equal(ResultadoCompraConDetalle.Encontrada, respuesta.Resultado);
        Assert.Equal(string.Empty, respuesta.Mensaje);
        CompraConDetalleDto dto = Assert.IsType<CompraConDetalleDto>(respuesta.Compra);
        Assert.Equal(compra.IdCompra, dto.IdCompra);
        Assert.Equal(fecha, dto.FechaRegistro);
        Assert.True(dto.Estado);
        Assert.Equal(65m, dto.MontoTotal);
        Assert.Equal("Ferretería Central", dto.RazonSocialProveedor);
        Assert.Equal("Usuario Prueba", dto.NombreUsuario);
        Assert.Equal(new[] { "COD3", "COD1", "COD2" }, dto.Lineas.Select(l => l.CodigoProducto));
        Assert.Equal(new[] { "Tres", "Uno", "Dos" }, dto.Lineas.Select(l => l.NombreProducto));
        Assert.Equal(new[] { 1, 2, 3 }, dto.Lineas.Select(l => l.Cantidad));
        Assert.Equal(new[] { 10m, 12.5m, 10m }, dto.Lineas.Select(l => l.PrecioCompra));
        Assert.Equal(new[] { 10m, 25m, 30m }, dto.Lineas.Select(l => l.Subtotal));
    }

    // RF-54: una compra sin líneas devuelve la lista vacía, no un error.
    [Fact]
    public async Task ObtenerCompraConDetalle_SinLineas_DevuelveListaVacia()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor proveedor = await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        Usuario usuario = await SembrarUsuarioAsync(db);
        Compra compra = await SembrarCompraAsync(
            db, proveedor.IdProveedor, usuario.IdUsuario, new DateTime(2026, 2, 2, 2, 2, 2), 0m);

        RespuestaCompraConDetalle respuesta = await new CompraNegocio(db)
            .ObtenerCompraConDetalleAsync(compra.IdCompra, "Administrador");

        Assert.Equal(ResultadoCompraConDetalle.Encontrada, respuesta.Resultado);
        Assert.Empty(respuesta.Compra!.Lineas);
    }

    // D-2/D-17: el detalle no expone el precio de venta.
    [Fact]
    public void LineaCompraDetalleDto_NoDeclaraPrecioVenta()
    {
        Assert.Null(typeof(LineaCompraDetalleDto).GetProperty("PrecioVenta"));
    }

    // RF-54: compra inexistente ⇒ CompraNoEncontrada con el mensaje del catálogo.
    [Fact]
    public async Task ObtenerCompraConDetalle_ConIdInexistente_DevuelveCompraNoEncontrada()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();

        RespuestaCompraConDetalle respuesta = await new CompraNegocio(db)
            .ObtenerCompraConDetalleAsync(Importes.CompraInexistente, "Administrador");

        Assert.Equal(ResultadoCompraConDetalle.CompraNoEncontrada, respuesta.Resultado);
        Assert.Equal(MensajeCompraNoEncontrada, respuesta.Mensaje);
        Assert.Null(respuesta.Compra);
    }

    // M11 / §3.4: proveedor, usuario y producto inexistentes ⇒ textos defensivos
    // (y cadenas vacías en la línea), sin excepción y sin filtrar datos internos.
    [Fact]
    public async Task ObtenerCompraConDetalle_ConNavegacionesNulas_DevuelveTextosDefensivos()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Compra compra = await SembrarCompraAsync(
            db, Importes.ProveedorInexistente, Importes.UsuarioInexistente,
            new DateTime(2026, 8, 9, 10, 0, 0), 10m);
        await SembrarDetalleAsync(db, compra.IdCompra, Importes.ProductoInexistente, 1, 10m);

        RespuestaCompraConDetalle respuesta = await new CompraNegocio(db)
            .ObtenerCompraConDetalleAsync(compra.IdCompra, "Administrador");

        Assert.Equal(ResultadoCompraConDetalle.Encontrada, respuesta.Resultado);
        CompraConDetalleDto dto = respuesta.Compra!;
        Assert.Equal(TextoSinProveedor, dto.RazonSocialProveedor);
        Assert.Equal(TextoSinUsuario, dto.NombreUsuario);
        LineaCompraDetalleDto linea = Assert.Single(dto.Lineas);
        Assert.Equal(string.Empty, linea.CodigoProducto);
        Assert.Equal(string.Empty, linea.NombreProducto);
        Assert.Equal(10m, linea.Subtotal);
    }

    // RNF-3/RNF-4: la consulta del detalle no modifica ni rastrea datos.
    [Fact]
    public async Task ObtenerCompraConDetalle_NoModificaLosDatos()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor proveedor = await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        Usuario usuario = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");
        Compra compra = await SembrarCompraAsync(
            db, proveedor.IdProveedor, usuario.IdUsuario, new DateTime(2026, 3, 3, 3, 3, 3), 30.99m);
        await SembrarDetalleAsync(db, compra.IdCompra, producto.IdProducto, 3, 10.33m);
        int comprasAntes = await db.Compras.AsNoTracking().CountAsync();
        int detallesAntes = await db.DetallesCompra.AsNoTracking().CountAsync();

        await new CompraNegocio(db).ObtenerCompraConDetalleAsync(compra.IdCompra, "Administrador");

        Assert.Equal(comprasAntes, await db.Compras.AsNoTracking().CountAsync());
        Assert.Equal(detallesAntes, await db.DetallesCompra.AsNoTracking().CountAsync());
        Assert.Equal(0, db.ChangeTracker.Entries().Count(e => e.State != EntityState.Unchanged));
        Compra despues = await db.Compras.AsNoTracking().SingleAsync(c => c.IdCompra == compra.IdCompra);
        Assert.Equal(30.99m, despues.MontoTotal);
        Assert.Equal(compra.FechaRegistro, despues.FechaRegistro);
    }

    // ---------- G11 (T7) · Fallos de lectura del historial y del detalle (RF-57) ----------

    // RF-57: el historial no propaga la excepción y deja la consulta reintentable.
    [Fact]
    public async Task ObtenerHistorial_ConContextoDispuesto_DevuelveErrorPersistencia()
    {
        AppDbContext db = await CrearContextoVacioAsync();
        await SembrarCompraAsync(db, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0), 10m);
        var negocio = new CompraNegocio(db);
        await db.DisposeAsync();

        RespuestaHistorialCompras respuesta = await negocio.ObtenerHistorialAsync("Administrador");

        Assert.Equal(ResultadoHistorialCompras.ErrorPersistencia, respuesta.Resultado);
        Assert.Equal(MensajeErrorPersistencia, respuesta.Mensaje);
        Assert.Empty(respuesta.Compras);
    }

    // RF-57: el detalle no propaga la excepción y devuelve ErrorPersistencia.
    [Fact]
    public async Task ObtenerCompraConDetalle_ConContextoDispuesto_DevuelveErrorPersistencia()
    {
        AppDbContext db = await CrearContextoVacioAsync();
        await SembrarCompraAsync(db, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0), 10m);
        var negocio = new CompraNegocio(db);
        await db.DisposeAsync();

        RespuestaCompraConDetalle respuesta = await negocio.ObtenerCompraConDetalleAsync(1, "Administrador");

        Assert.Equal(ResultadoCompraConDetalle.ErrorPersistencia, respuesta.Resultado);
        Assert.Equal(MensajeErrorPersistencia, respuesta.Mensaje);
        Assert.Null(respuesta.Compra);
    }

    // ---------- G14 · Permisos RBAC (RF-8, RF-28, RF-59, RF-60, RF-61) ----------

    // RF-60: el rol se evalúa primero y solo se informa el permiso; no se escribe nada.
    [Theory]
    [InlineData("Vendedor")]
    [InlineData("  vendedor ")]
    [InlineData("Otro")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RegistrarCompra_ConRolNoAutorizado_DevuelvePermisoDenegadoSinFilas(string? rol)
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Proveedor proveedor = await SembrarProveedorAsync(db, "20451234567", "Ferretería Central");
        Usuario usuario = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoAsync(db, "COD1", "Uno");

        RespuestaRegistroCompra respuesta = await new CompraNegocio(db).RegistrarCompraAsync(
            SolicitudRegistrar(
                proveedor.IdProveedor, Linea(producto.IdProducto, "COD1", "Uno", 1, 10m, 12m)),
            usuario.IdUsuario,
            rol);

        Assert.Equal(ResultadoRegistroCompra.PermisoDenegado, respuesta.Resultado);
        Assert.Equal(MensajePermisoRegistrarCompra, respuesta.Mensaje);
        Assert.Null(respuesta.IdCompra);
        await AssertSinComprasAsync(db);
    }

    // RF-28/RF-59: con rol no autorizado y datos también inválidos se informa
    // únicamente el permiso, nunca un error de datos.
    [Fact]
    public async Task RegistrarCompra_ConRolNoAutorizadoYDatosInvalidos_DevuelveSoloElMensajeDePermiso()
    {
        await using AppDbContext db = await CrearContextoVacioAsync();

        RespuestaRegistroCompra respuesta = await new CompraNegocio(db).RegistrarCompraAsync(
            new SolicitudRegistrarCompraDto { IdProveedor = Importes.ProveedorInexistente },
            0,
            "Vendedor");

        Assert.Equal(ResultadoRegistroCompra.PermisoDenegado, respuesta.Resultado);
        Assert.Equal(MensajePermisoRegistrarCompra, respuesta.Mensaje);
        Assert.Null(respuesta.IdCompra);
        await AssertSinComprasAsync(db);
    }

    // RF-60: con el contexto dispuesto, obtener PermisoDenegado (y no
    // ErrorPersistencia) demuestra que no se ejecutó ninguna consulta.
    [Fact]
    public async Task RegistrarCompra_ConContextoDispuestoYRolNoAutorizado_DevuelvePermisoDenegado()
    {
        AppDbContext db = await CrearContextoVacioAsync();
        var negocio = new CompraNegocio(db);
        await db.DisposeAsync();

        RespuestaRegistroCompra respuesta = await negocio.RegistrarCompraAsync(
            new SolicitudRegistrarCompraDto { IdProveedor = 1 }, 1, "Vendedor");

        Assert.Equal(ResultadoRegistroCompra.PermisoDenegado, respuesta.Resultado);
        Assert.Equal(MensajePermisoRegistrarCompra, respuesta.Mensaje);
        Assert.Null(respuesta.IdCompra);
    }

    // RF-61: el historial exige permiso de consulta y no entrega datos.
    [Theory]
    [InlineData("Vendedor")]
    [InlineData("Otro")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ObtenerHistorial_ConRolNoAutorizado_DevuelvePermisoDenegadoSinDatos(string? rol)
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        await SembrarCompraAsync(db, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0), 10m);

        RespuestaHistorialCompras respuesta = await new CompraNegocio(db).ObtenerHistorialAsync(rol);

        Assert.Equal(ResultadoHistorialCompras.PermisoDenegado, respuesta.Resultado);
        Assert.Equal(MensajePermisoConsultarCompras, respuesta.Mensaje);
        Assert.Empty(respuesta.Compras);
        Assert.Equal(1, await db.Compras.AsNoTracking().CountAsync());
    }

    // RF-61: con el contexto dispuesto, PermisoDenegado (y no ErrorPersistencia)
    // demuestra que el historial no consultó el origen de datos.
    [Fact]
    public async Task ObtenerHistorial_ConContextoDispuestoYRolNoAutorizado_DevuelvePermisoDenegado()
    {
        AppDbContext db = await CrearContextoVacioAsync();
        var negocio = new CompraNegocio(db);
        await db.DisposeAsync();

        RespuestaHistorialCompras respuesta = await negocio.ObtenerHistorialAsync("Vendedor");

        Assert.Equal(ResultadoHistorialCompras.PermisoDenegado, respuesta.Resultado);
        Assert.Equal(MensajePermisoConsultarCompras, respuesta.Mensaje);
        Assert.Empty(respuesta.Compras);
    }

    // RF-61: el detalle exige permiso de consulta y no entrega la compra.
    [Theory]
    [InlineData("Vendedor")]
    [InlineData("Otro")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ObtenerCompraConDetalle_ConRolNoAutorizado_DevuelvePermisoDenegadoSinDatos(string? rol)
    {
        await using AppDbContext db = await CrearContextoVacioAsync();
        Compra compra = await SembrarCompraAsync(
            db, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0), 10m);

        RespuestaCompraConDetalle respuesta = await new CompraNegocio(db)
            .ObtenerCompraConDetalleAsync(compra.IdCompra, rol);

        Assert.Equal(ResultadoCompraConDetalle.PermisoDenegado, respuesta.Resultado);
        Assert.Equal(MensajePermisoConsultarCompras, respuesta.Mensaje);
        Assert.Null(respuesta.Compra);
    }

    // RF-61: con el contexto dispuesto, PermisoDenegado (y no ErrorPersistencia)
    // demuestra que el detalle no consultó el origen de datos.
    [Fact]
    public async Task ObtenerCompraConDetalle_ConContextoDispuestoYRolNoAutorizado_DevuelvePermisoDenegado()
    {
        AppDbContext db = await CrearContextoVacioAsync();
        var negocio = new CompraNegocio(db);
        await db.DisposeAsync();

        RespuestaCompraConDetalle respuesta = await negocio.ObtenerCompraConDetalleAsync(1, "Vendedor");

        Assert.Equal(ResultadoCompraConDetalle.PermisoDenegado, respuesta.Resultado);
        Assert.Equal(MensajePermisoConsultarCompras, respuesta.Mensaje);
        Assert.Null(respuesta.Compra);
    }

    // ---------- Ayudas de las pruebas de CompraNegocio ----------

    private static SolicitudRegistrarCompraDto SolicitudRegistrar(
        int? idProveedor, params LineaCompraDto[] lineas)
    {
        return new SolicitudRegistrarCompraDto { IdProveedor = idProveedor, Lineas = lineas };
    }

    private static async Task AssertSinComprasAsync(AppDbContext db)
    {
        Assert.Equal(0, await db.Compras.AsNoTracking().CountAsync());
        Assert.Equal(0, await db.DetallesCompra.AsNoTracking().CountAsync());
    }

    private static async Task<Producto> LeerProductoAsync(AppDbContext db, int idProducto)
        => await db.Productos.AsNoTracking().FirstAsync(p => p.IdProducto == idProducto);

    private static async Task<Usuario> SembrarUsuarioAsync(
        AppDbContext db, bool estado = true, string nombre = "Usuario", string apellido = "Prueba")
    {
        var usuario = new Usuario
        {
            IdRol = 1,
            Dni = 30111222,
            Nombre = nombre,
            Apellido = apellido,
            Sexo = "F",
            Clave = "hash-de-prueba",
            Estado = estado
        };

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
        return usuario;
    }

    private static async Task<Compra> SembrarCompraAsync(
        AppDbContext db, int idProveedor, int idUsuario, DateTime fechaRegistro,
        decimal montoTotal, bool estado = true)
    {
        var compra = new Compra
        {
            IdProveedor = idProveedor,
            IdUsuario = idUsuario,
            FechaRegistro = fechaRegistro,
            MontoTotal = montoTotal,
            Estado = estado
        };

        db.Compras.Add(compra);
        await db.SaveChangesAsync();
        return compra;
    }

    private static async Task<DetalleCompra> SembrarDetalleAsync(
        AppDbContext db, int idCompra, int idProducto, int cantidad, decimal precioCompra)
    {
        var detalle = new DetalleCompra
        {
            IdCompra = idCompra,
            IdProducto = idProducto,
            Cantidad = cantidad,
            PrecioCompra = precioCompra,
            Subtotal = CalculoImportesCompra.CalcularSubtotal(cantidad, precioCompra)
        };

        db.DetallesCompra.Add(detalle);
        await db.SaveChangesAsync();
        return detalle;
    }

    private static LineaCompraDto Linea(
        int idProducto, string codigo, string nombre, int cantidad, decimal precioCompra, decimal precioVenta,
        decimal? subtotal = null)
    {
        return new LineaCompraDto
        {
            IdProducto = idProducto,
            CodigoProducto = codigo,
            NombreProducto = nombre,
            Cantidad = cantidad,
            PrecioCompra = precioCompra,
            PrecioVenta = precioVenta,
            Subtotal = subtotal ?? CalculoImportesCompra.CalcularSubtotal(cantidad, precioCompra)
        };
    }

    private static SolicitudAgregarLineaCompraDto SolicitudAgregar(
        int idProducto,
        string? cantidad = "1",
        string? precioCompra = "10,5",
        string? precioVenta = "12",
        IReadOnlyList<LineaCompraDto>? lineas = null)
    {
        return new SolicitudAgregarLineaCompraDto
        {
            IdProducto = idProducto,
            Cantidad = cantidad,
            PrecioCompra = precioCompra,
            PrecioVenta = precioVenta,
            LineasActuales = lineas ?? Array.Empty<LineaCompraDto>()
        };
    }

    private static SolicitudEditarLineaCompraDto SolicitudEditar(
        int indiceLinea,
        int idProducto,
        string? cantidad = "1",
        string? precioCompra = "10,5",
        string? precioVenta = "12",
        IReadOnlyList<LineaCompraDto>? lineas = null)
    {
        return new SolicitudEditarLineaCompraDto
        {
            IndiceLinea = indiceLinea,
            IdProducto = idProducto,
            Cantidad = cantidad,
            PrecioCompra = precioCompra,
            PrecioVenta = precioVenta,
            LineasActuales = lineas ?? Array.Empty<LineaCompraDto>()
        };
    }

    private static string MensajeSinProductoEnLaLinea(int numeroLinea)
        => string.Format(MensajeProductoInexistenteLineaFormato, numeroLinea);

    private static void AssertLineasIguales(
        IReadOnlyList<LineaCompraDto> esperadas, IReadOnlyList<LineaCompraDto> reales)
    {
        Assert.Equal(esperadas.Count, reales.Count);

        for (int i = 0; i < esperadas.Count; i++)
        {
            Assert.Equal(esperadas[i].IdProducto, reales[i].IdProducto);
            Assert.Equal(esperadas[i].CodigoProducto, reales[i].CodigoProducto);
            Assert.Equal(esperadas[i].NombreProducto, reales[i].NombreProducto);
            Assert.Equal(esperadas[i].Cantidad, reales[i].Cantidad);
            Assert.Equal(esperadas[i].PrecioCompra, reales[i].PrecioCompra);
            Assert.Equal(esperadas[i].PrecioVenta, reales[i].PrecioVenta);
            Assert.Equal(esperadas[i].Subtotal, reales[i].Subtotal);
        }
    }

    private static async Task<Producto> SembrarProductoAsync(
        AppDbContext db, string codigo, string nombre, bool estado = true,
        decimal precioCompra = 10.5m, decimal precioVenta = 12m)
    {
        var producto = new Producto
        {
            IdCategoria = 1,
            Codigo = codigo,
            Nombre = nombre,
            Descripcion = "Dato previo de la prueba",
            PrecioCompra = precioCompra,
            PrecioVenta = precioVenta,
            Stock = 5,
            Estado = estado,
            IdUsuario = 1
        };

        db.Productos.Add(producto);
        await db.SaveChangesAsync();
        return producto;
    }

    private static async Task<Proveedor> SembrarProveedorAsync(
        AppDbContext db, string cuit, string razonSocial, bool estado = true)
    {
        var proveedor = new Proveedor
        {
            Cuit = cuit,
            RazonSocial = razonSocial,
            Correo = "proveedor@mail.com",
            Telefono = "1145678901",
            Estado = estado
        };

        db.Proveedores.Add(proveedor);
        await db.SaveChangesAsync();
        return proveedor;
    }

    private static async Task<AppDbContext> CrearContextoVacioAsync()
    {
        var db = new AppDbContext(CrearOpciones($"CompraNegocioTests-{Guid.NewGuid()}"));

        await db.Database.EnsureCreatedAsync();
        return db;
    }

    private static DbContextOptions<AppDbContext> CrearOpciones(string nombreBase)
    {
        // El almacén en memoria no admite transacciones: se silencia solo el aviso
        // para poder probar el registro (T5b). El código de negocio sigue abriendo
        // la transacción igual que lo hará contra SQL Server.
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(nombreBase)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
    }

    /// <summary>
    /// Escenario del plan §9.1: base en memoria compartida por nombre entre el
    /// contexto sano (sembrado) y el <see cref="FalloDbContext"/>.
    /// </summary>
    private static async Task<EscenarioDeFallo> CrearEscenarioDeFalloAsync()
    {
        DbContextOptions<AppDbContext> opciones =
            CrearOpciones($"CompraNegocioTests-Fallo-{Guid.NewGuid()}");

        var sano = new AppDbContext(opciones);
        await sano.Database.EnsureCreatedAsync();

        Proveedor proveedor = await SembrarProveedorAsync(sano, "20451234567", "Ferretería Central");
        Usuario usuario = await SembrarUsuarioAsync(sano);
        Producto uno = await SembrarProductoAsync(sano, "COD1", "Uno");
        Producto dos = await SembrarProductoAsync(sano, "COD2", "Dos");

        return new EscenarioDeFallo
        {
            Opciones = opciones,
            Sano = sano,
            IdUsuario = usuario.IdUsuario,
            Solicitud = SolicitudRegistrar(
                proveedor.IdProveedor,
                Linea(uno.IdProducto, "COD1", "Uno", 2, 10.5m, 12m),
                Linea(dos.IdProducto, "COD2", "Dos", 3, 20m, 25m))
        };
    }

    /// <summary>Identificadores que no existen en la base de la prueba.</summary>
    private static class Importes
    {
        public const int ProductoInexistente = 9999;

        public const int ProveedorInexistente = 9999;

        public const int UsuarioInexistente = 9999;

        public const int CompraInexistente = 9999;
    }

    /// <summary>Piezas compartidas del escenario de fallo de persistencia (§9.1).</summary>
    private sealed class EscenarioDeFallo
    {
        public DbContextOptions<AppDbContext> Opciones { get; init; } = null!;

        public AppDbContext Sano { get; init; } = null!;

        public int IdUsuario { get; init; }

        public SolicitudRegistrarCompraDto Solicitud { get; init; } = null!;
    }

    /// <summary>
    /// Doble del plan §9.1: falla en el <b>primer</b> <c>SaveChangesAsync</c> con
    /// <see cref="DbUpdateException"/>, antes de que EF escriba en el almacén, para
    /// probar la respuesta de <c>ErrorPersistencia</c>. No simula el rollback: el
    /// proveedor InMemory no ejecuta transacciones reales.
    /// </summary>
    private sealed class FalloDbContext : AppDbContext
    {
        public FalloDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => throw new DbUpdateException("Fallo simulado del origen de datos.");

        public override int SaveChanges()
            => throw new DbUpdateException("Fallo simulado del origen de datos.");
    }
}

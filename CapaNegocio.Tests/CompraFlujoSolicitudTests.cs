using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

public class CompraFlujoSolicitudTests
{
    private static async Task<AppDbContext> CrearContextoAsync(string nombreDb)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: nombreDb)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var context = new AppDbContext(options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    private static async Task<(Proveedor proveedor, Usuario usuario, Producto producto)> SembrarDatosPruebaAsync(AppDbContext db)
    {
        var rol = new Rol { IdRol = 2, Descripcion = "Encargado" };
        db.Roles.Add(rol);

        var usuario = new Usuario
        {
            IdUsuario = 10,
            IdRol = 2,
            Nombre = "Carlos",
            Apellido = "Encargado",
            Correo = "carlos@comercio.com",
            Clave = "hash123",
            Estado = true
        };
        db.Usuarios.Add(usuario);

        var categoria = new Categoria { IdCategoria = 1, Descripcion = "Hardware", Estado = true };
        db.Categorias.Add(categoria);

        var proveedor = new Proveedor
        {
            IdProveedor = 5,
            Cuit = "30-11223344-9",
            RazonSocial = "Mayorista Tech SA",
            Correo = "ventas@mayoristatech.com",
            Telefono = "11-4455-6677",
            Estado = true
        };
        db.Proveedores.Add(proveedor);

        var producto = new Producto
        {
            IdProducto = 100,
            IdCategoria = 1,
            IdUsuario = 10,
            Codigo = "PROD-100",
            Nombre = "Placa de Video RTX 4060",
            Stock = 2,
            StockMinimo = 5,
            PrecioCompra = 300m,
            PrecioVenta = 450m,
            Estado = true
        };
        db.Productos.Add(producto);

        await db.SaveChangesAsync();
        return (proveedor, usuario, producto);
    }

    [Fact]
    public async Task RegistrarCompra_EstadoPendiente_NoModificaStockYDisparaCorreo()
    {
        // Arrange
        await using var db = await CrearContextoAsync(nameof(RegistrarCompra_EstadoPendiente_NoModificaStockYDisparaCorreo));
        var (proveedor, usuario, producto) = await SembrarDatosPruebaAsync(db);
        var negocio = new CompraNegocio(db);

        var solicitud = new SolicitudRegistrarCompraDto
        {
            IdProveedor = proveedor.IdProveedor,
            EstadoSolicitud = "Pendiente",
            Observaciones = "Pedido urgente para stock crítico",
            Lineas = new List<LineaCompraDto>
            {
                new()
                {
                    IdProducto = producto.IdProducto,
                    CodigoProducto = producto.Codigo!,
                    NombreProducto = producto.Nombre!,
                    Cantidad = 10,
                    PrecioCompra = 310m,
                    PrecioVenta = 480m
                }
            }
        };

        // Act
        var respuesta = await negocio.RegistrarCompraAsync(solicitud, usuario.IdUsuario, "Encargado");

        // Assert
        Assert.Equal(ResultadoRegistroCompra.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.IdCompra);
        Assert.Equal("Pendiente", respuesta.EstadoSolicitud);
        Assert.NotNull(respuesta.InfoCorreo);
        Assert.True(respuesta.InfoCorreo.Exitoso);

        // Stock del producto NO debe haber cambiado
        var productoActualizado = await db.Productos.FindAsync(producto.IdProducto);
        Assert.Equal(2, productoActualizado!.Stock);

        var compraDb = await db.Compras.FindAsync(respuesta.IdCompra.Value);
        Assert.NotNull(compraDb);
        Assert.Equal("Pendiente", compraDb.EstadoSolicitud);
        Assert.Null(compraDb.FechaRecepcion);
    }

    [Fact]
    public async Task ConfirmarSolicitudCompra_ActualizaPreciosYRecalculaTotalSinModificarStock()
    {
        // Arrange
        await using var db = await CrearContextoAsync(nameof(ConfirmarSolicitudCompra_ActualizaPreciosYRecalculaTotalSinModificarStock));
        var (proveedor, usuario, producto) = await SembrarDatosPruebaAsync(db);
        var negocio = new CompraNegocio(db);

        var solicitudReg = new SolicitudRegistrarCompraDto
        {
            IdProveedor = proveedor.IdProveedor,
            EstadoSolicitud = "Pendiente",
            Lineas = new List<LineaCompraDto>
            {
                new() { IdProducto = producto.IdProducto, CodigoProducto = producto.Codigo!, NombreProducto = producto.Nombre!, Cantidad = 10, PrecioCompra = 100m, PrecioVenta = 150m }
            }
        };
        var reg = await negocio.RegistrarCompraAsync(solicitudReg, usuario.IdUsuario, "Encargado");

        var solicitudConf = new SolicitudConfirmarCompraDto
        {
            IdCompra = reg.IdCompra!.Value,
            Observaciones = "Proveedor confirmó descuento especial por volumen",
            Lineas = new List<LineaConfirmacionCompraDto>
            {
                new() { IdProducto = producto.IdProducto, PrecioCompra = 280m, PrecioVenta = 420m }
            }
        };

        // Act
        var respuesta = await negocio.ConfirmarSolicitudCompraAsync(solicitudConf, usuario.IdUsuario, "Encargado");

        // Assert
        Assert.True(respuesta.Exitoso);
        Assert.Equal("Confirmada", respuesta.EstadoSolicitud);

        var compraDb = await db.Compras.Include(c => c.Detalles).FirstOrDefaultAsync(c => c.IdCompra == reg.IdCompra.Value);
        Assert.NotNull(compraDb);
        Assert.Equal("Confirmada", compraDb.EstadoSolicitud);
        Assert.NotNull(compraDb.FechaConfirmacion);
        Assert.Equal(2800m, compraDb.MontoTotal); // 10 * 280
        Assert.Equal(280m, compraDb.Detalles!.First().PrecioCompra);

        // Stock sigue sin modificarse
        var prodDb = await db.Productos.FindAsync(producto.IdProducto);
        Assert.Equal(2, prodDb!.Stock);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public async Task ConfirmarSolicitudCompra_ConPrecioCeroONegativo_RetornaError(decimal precioInvalido)
    {
        // Arrange
        await using var db = await CrearContextoAsync($"{nameof(ConfirmarSolicitudCompra_ConPrecioCeroONegativo_RetornaError)}_{precioInvalido}");
        var (proveedor, usuario, producto) = await SembrarDatosPruebaAsync(db);
        var negocio = new CompraNegocio(db);

        var reg = await negocio.RegistrarCompraAsync(new SolicitudRegistrarCompraDto
        {
            IdProveedor = proveedor.IdProveedor,
            EstadoSolicitud = "Pendiente",
            Lineas = new List<LineaCompraDto>
            {
                new() { IdProducto = producto.IdProducto, CodigoProducto = producto.Codigo!, NombreProducto = producto.Nombre!, Cantidad = 10 }
            }
        }, usuario.IdUsuario, "Encargado");

        var solicitudConf = new SolicitudConfirmarCompraDto
        {
            IdCompra = reg.IdCompra!.Value,
            Lineas = new List<LineaConfirmacionCompraDto>
            {
                new() { IdProducto = producto.IdProducto, PrecioCompra = precioInvalido }
            }
        };

        // Act
        var respuesta = await negocio.ConfirmarSolicitudCompraAsync(solicitudConf, usuario.IdUsuario, "Encargado");

        // Assert
        Assert.False(respuesta.Exitoso);
        Assert.Contains("mayor a cero", respuesta.Mensaje);
    }

    [Fact]
    public async Task RecibirCompra_DesdeConfirmada_IncrementaStockYActualizaPrecios()
    {
        // Arrange
        await using var db = await CrearContextoAsync(nameof(RecibirCompra_DesdeConfirmada_IncrementaStockYActualizaPrecios));
        var (proveedor, usuario, producto) = await SembrarDatosPruebaAsync(db);
        var negocio = new CompraNegocio(db);

        var reg = await negocio.RegistrarCompraAsync(new SolicitudRegistrarCompraDto
        {
            IdProveedor = proveedor.IdProveedor,
            EstadoSolicitud = "Pendiente",
            Lineas = new List<LineaCompraDto>
            {
                new() { IdProducto = producto.IdProducto, CodigoProducto = producto.Codigo!, NombreProducto = producto.Nombre!, Cantidad = 8, PrecioCompra = 250m, PrecioVenta = 400m }
            }
        }, usuario.IdUsuario, "Encargado");

        await negocio.ConfirmarSolicitudCompraAsync(new SolicitudConfirmarCompraDto
        {
            IdCompra = reg.IdCompra!.Value,
            Lineas = new List<LineaConfirmacionCompraDto>
            {
                new() { IdProducto = producto.IdProducto, PrecioCompra = 260m, PrecioVenta = 410m }
            }
        }, usuario.IdUsuario, "Encargado");

        // Act
        var respuesta = await negocio.RecibirCompraAsync(reg.IdCompra.Value, usuario.IdUsuario, "Encargado");

        // Assert
        Assert.True(respuesta.Exitoso);
        Assert.Equal("Recibida", respuesta.EstadoSolicitud);

        var compraDb = await db.Compras.FindAsync(reg.IdCompra.Value);
        Assert.Equal("Recibida", compraDb!.EstadoSolicitud);
        Assert.NotNull(compraDb.FechaRecepcion);

        var prodDb = await db.Productos.FindAsync(producto.IdProducto);
        Assert.Equal(10, prodDb!.Stock); // 2 previos + 8 recibidos = 10
        Assert.Equal(260m, prodDb.PrecioCompra);
    }

    [Fact]
    public async Task AnularCompra_EstadoPendiente_CancelaSinModificarStock()
    {
        // Arrange
        await using var db = await CrearContextoAsync(nameof(AnularCompra_EstadoPendiente_CancelaSinModificarStock));
        var (proveedor, usuario, producto) = await SembrarDatosPruebaAsync(db);
        var negocio = new CompraNegocio(db);

        var reg = await negocio.RegistrarCompraAsync(new SolicitudRegistrarCompraDto
        {
            IdProveedor = proveedor.IdProveedor,
            EstadoSolicitud = "Pendiente",
            Lineas = new List<LineaCompraDto>
            {
                new() { IdProducto = producto.IdProducto, CodigoProducto = producto.Codigo!, NombreProducto = producto.Nombre!, Cantidad = 5, PrecioCompra = 100m, PrecioVenta = 150m }
            }
        }, usuario.IdUsuario, "Encargado");

        // Act
        var resAnulacion = await negocio.AnularCompraAsync(new SolicitudAnularCompraDto
        {
            IdCompra = reg.IdCompra!.Value,
            Motivo = "Proveedor sin disponibilidad de stock"
        }, usuario.IdUsuario, "Encargado");

        // Assert
        Assert.True(resAnulacion.Exitoso);
        Assert.Equal("Anulada", resAnulacion.EstadoSolicitud);

        var compraDb = await db.Compras.FindAsync(reg.IdCompra.Value);
        Assert.False(compraDb!.Estado);
        Assert.Equal("Anulada", compraDb.EstadoSolicitud);
        Assert.Equal("Proveedor sin disponibilidad de stock", compraDb.MotivoAnulacion);
        Assert.NotNull(compraDb.FechaAnulacion);
        Assert.Equal(usuario.IdUsuario, compraDb.IdUsuarioAnula);

        var prodDb = await db.Productos.FindAsync(producto.IdProducto);
        Assert.Equal(2, prodDb!.Stock); // Sin cambios
    }

    [Fact]
    public async Task AnularCompra_EstadoRecibida_ConStockSuficiente_RevierteStockYGuardaAuditoria()
    {
        // Arrange
        await using var db = await CrearContextoAsync(nameof(AnularCompra_EstadoRecibida_ConStockSuficiente_RevierteStockYGuardaAuditoria));
        var (proveedor, usuario, producto) = await SembrarDatosPruebaAsync(db);
        var negocio = new CompraNegocio(db);

        // Compra directa recibida (suma 6 unidades al stock)
        var reg = await negocio.RegistrarCompraAsync(new SolicitudRegistrarCompraDto
        {
            IdProveedor = proveedor.IdProveedor,
            EstadoSolicitud = "Recibida",
            Lineas = new List<LineaCompraDto>
            {
                new() { IdProducto = producto.IdProducto, CodigoProducto = producto.Codigo!, NombreProducto = producto.Nombre!, Cantidad = 6, PrecioCompra = 100m, PrecioVenta = 150m }
            }
        }, usuario.IdUsuario, "Encargado");

        var prodPostCompra = await db.Productos.FindAsync(producto.IdProducto);
        Assert.Equal(8, prodPostCompra!.Stock); // 2 + 6 = 8

        // Act
        var resAnular = await negocio.AnularCompraAsync(new SolicitudAnularCompraDto
        {
            IdCompra = reg.IdCompra!.Value,
            Motivo = "Mercadería dañada en transporte devuelta a fábrica"
        }, usuario.IdUsuario, "Encargado");

        // Assert
        Assert.True(resAnular.Exitoso);
        var prodPostAnulacion = await db.Productos.FindAsync(producto.IdProducto);
        Assert.Equal(2, prodPostAnulacion!.Stock); // 8 - 6 = 2 (revertido)

        var compraDb = await db.Compras.FindAsync(reg.IdCompra.Value);
        Assert.False(compraDb!.Estado);
        Assert.Equal("Anulada", compraDb.EstadoSolicitud);
        Assert.Equal("Mercadería dañada en transporte devuelta a fábrica", compraDb.MotivoAnulacion);
    }

    [Fact]
    public async Task AnularCompra_EstadoRecibida_ConStockInsuficiente_RechazaOperacion()
    {
        // Arrange
        await using var db = await CrearContextoAsync(nameof(AnularCompra_EstadoRecibida_ConStockInsuficiente_RechazaOperacion));
        var (proveedor, usuario, producto) = await SembrarDatosPruebaAsync(db);
        var negocio = new CompraNegocio(db);

        var reg = await negocio.RegistrarCompraAsync(new SolicitudRegistrarCompraDto
        {
            IdProveedor = proveedor.IdProveedor,
            EstadoSolicitud = "Recibida",
            Lineas = new List<LineaCompraDto>
            {
                new() { IdProducto = producto.IdProducto, CodigoProducto = producto.Codigo!, NombreProducto = producto.Nombre!, Cantidad = 10, PrecioCompra = 100m, PrecioVenta = 150m }
            }
        }, usuario.IdUsuario, "Encargado");

        // Simulamos venta posterior: stock baja a 3 (insuficiente para revertir las 10 recibidas)
        var prodDb = await db.Productos.FindAsync(producto.IdProducto);
        prodDb!.Stock = 3;
        await db.SaveChangesAsync();

        // Act
        var resAnular = await negocio.AnularCompraAsync(new SolicitudAnularCompraDto
        {
            IdCompra = reg.IdCompra!.Value,
            Motivo = "Intento de cancelación tardía"
        }, usuario.IdUsuario, "Encargado");

        // Assert
        Assert.False(resAnular.Exitoso);
        Assert.Contains("stock insuficiente", resAnular.Mensaje);
        Assert.Contains("RTX 4060", resAnular.Mensaje);

        // Compra debe seguir activa y recibida
        var compraDb = await db.Compras.FindAsync(reg.IdCompra.Value);
        Assert.True(compraDb!.Estado);
        Assert.Equal("Recibida", compraDb.EstadoSolicitud);
    }

    [Fact]
    public async Task AnularCompra_RolNoAutorizado_RechazaPorPermisos()
    {
        // Arrange
        await using var db = await CrearContextoAsync(nameof(AnularCompra_RolNoAutorizado_RechazaPorPermisos));
        var (proveedor, usuario, producto) = await SembrarDatosPruebaAsync(db);
        var negocio = new CompraNegocio(db);

        var reg = await negocio.RegistrarCompraAsync(new SolicitudRegistrarCompraDto
        {
            IdProveedor = proveedor.IdProveedor,
            EstadoSolicitud = "Pendiente",
            Lineas = new List<LineaCompraDto>
            {
                new() { IdProducto = producto.IdProducto, CodigoProducto = producto.Codigo!, NombreProducto = producto.Nombre!, Cantidad = 1, PrecioCompra = 100m, PrecioVenta = 150m }
            }
        }, usuario.IdUsuario, "Encargado");

        // Act - Administrador y Vendedor no pueden anular compras (exclusivo Encargado)
        var resAdmin = await negocio.AnularCompraAsync(new SolicitudAnularCompraDto
        {
            IdCompra = reg.IdCompra!.Value,
            Motivo = "Cancelación por admin"
        }, usuario.IdUsuario, "Administrador");

        var resVendedor = await negocio.AnularCompraAsync(new SolicitudAnularCompraDto
        {
            IdCompra = reg.IdCompra!.Value,
            Motivo = "Cancelación por vendedor"
        }, usuario.IdUsuario, "Vendedor");

        // Assert
        Assert.False(resAdmin.Exitoso);
        Assert.Contains("No tiene permisos", resAdmin.Mensaje);

        Assert.False(resVendedor.Exitoso);
        Assert.Contains("No tiene permisos", resVendedor.Mensaje);
    }

    [Fact]
    public async Task BuscarProductosParaCompra_IncluyeProductosAsociadosPorProveedorProducto()
    {
        // Arrange
        await using var db = await CrearContextoAsync(nameof(BuscarProductosParaCompra_IncluyeProductosAsociadosPorProveedorProducto));
        var (proveedor, _, producto) = await SembrarDatosPruebaAsync(db);
        var negocio = new CompraNegocio(db);

        // Asignamos producto al proveedor mediante la tabla puente PROVEEDOR_PRODUCTO
        db.ProveedoresProductos.Add(new ProveedorProducto
        {
            IdProveedor = proveedor.IdProveedor,
            IdProducto = producto.IdProducto
        });
        await db.SaveChangesAsync();

        // Act
        var res = await negocio.BuscarProductosParaCompraAsync(
            textoBusqueda: null,
            idProveedor: proveedor.IdProveedor,
            mostrarTodos: false,
            rol: "Encargado");

        // Assert
        Assert.Equal(ResultadoBusquedaCompras.ConResultados, res.Resultado);
        var encontrado = Assert.Single(res.Productos);
        Assert.Equal(producto.IdProducto, encontrado.IdProducto);
        Assert.Equal(2, encontrado.Stock);
        Assert.Equal(5, encontrado.StockMinimo);
        Assert.True(encontrado.EsBajoStockMinimo); // 2 <= 5 => true
    }
}

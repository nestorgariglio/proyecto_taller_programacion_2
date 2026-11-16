using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CapaDatos;

/// <summary>
/// Repositorio de persistencia de ventas mediante el procedimiento almacenado sp_RegistrarVenta.
/// </summary>
public class VentaRepositorio : IVentaRepositorio
{
    private readonly string _cadenaConexion;

    public VentaRepositorio(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion ?? throw new ArgumentNullException(nameof(cadenaConexion));
    }

    public async Task<ResultadoVentaDatos> RegistrarVentaAsync(
        ParametrosRegistroVentaDatos parametros,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(parametros);

        try
        {
            await using var conexion = new SqlConnection(_cadenaConexion);
            await conexion.OpenAsync(ct);

            await using var comando = new SqlCommand("dbo.sp_RegistrarVenta", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@IdCliente", SqlDbType.Int)
            {
                Value = (object?)parametros.IdCliente ?? DBNull.Value
            });

            comando.Parameters.Add(new SqlParameter("@DocumentoCliente", SqlDbType.VarChar, 20)
            {
                Value = (object?)parametros.DocumentoCliente ?? DBNull.Value
            });

            comando.Parameters.Add(new SqlParameter("@NombreCliente", SqlDbType.NVarChar, 200)
            {
                Value = (object?)parametros.NombreCliente ?? DBNull.Value
            });

            comando.Parameters.Add(new SqlParameter("@IdTipo", SqlDbType.Int)
            {
                Value = (object?)parametros.IdTipo ?? DBNull.Value
            });

            comando.Parameters.Add(new SqlParameter("@IdUsuario", SqlDbType.Int)
            {
                Value = (object?)parametros.IdUsuario ?? DBNull.Value
            });

            comando.Parameters.Add(new SqlParameter("@MontoPago", SqlDbType.Decimal)
            {
                Precision = 18,
                Scale = 2,
                Value = parametros.MontoPago
            });

            comando.Parameters.Add(new SqlParameter("@MontoCambio", SqlDbType.Decimal)
            {
                Precision = 18,
                Scale = 2,
                Value = parametros.MontoCambio
            });

            comando.Parameters.Add(new SqlParameter("@MontoTotal", SqlDbType.Decimal)
            {
                Precision = 18,
                Scale = 2,
                Value = parametros.MontoTotal
            });

            var tabla = new DataTable();
            tabla.Columns.Add("IdProducto", typeof(int));
            tabla.Columns.Add("Cantidad", typeof(int));
            tabla.Columns.Add("PrecioVenta", typeof(decimal));
            tabla.Columns.Add("Subtotal", typeof(decimal));

            foreach (var item in parametros.Detalles)
            {
                tabla.Rows.Add(item.IdProducto, item.Cantidad, item.PrecioVenta, item.Subtotal);
            }

            var paramDetalles = new SqlParameter("@Detalles", SqlDbType.Structured)
            {
                TypeName = "dbo.TipoDetalleVenta",
                Value = tabla
            };
            comando.Parameters.Add(paramDetalles);

            var pIdVenta = new SqlParameter("@IdVentaGenerada", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            var pCodigo = new SqlParameter("@CodigoResultado", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            var pMensaje = new SqlParameter("@MensajeResultado", SqlDbType.NVarChar, 500)
            {
                Direction = ParameterDirection.Output
            };

            comando.Parameters.Add(pIdVenta);
            comando.Parameters.Add(pCodigo);
            comando.Parameters.Add(pMensaje);

            await comando.ExecuteNonQueryAsync(ct);

            int idVenta = pIdVenta.Value is int id ? id : 0;
            int codigoResultado = pCodigo.Value is int cod ? cod : -1;
            string mensaje = pMensaje.Value as string ?? string.Empty;

            return new ResultadoVentaDatos(codigoResultado == 0, idVenta, codigoResultado, mensaje);
        }
        catch (SqlException ex)
        {
            return new ResultadoVentaDatos(false, 0, ex.Number, ex.Message);
        }
    }

    public async Task<IReadOnlyList<ResumenVentaDatos>> ObtenerHistorialVentasAsync(
        CancellationToken ct = default)
    {
        const string sql = @"
SELECT
    v.id_venta,
    COALESCE(v.nro_comprobante, CONCAT('V-', RIGHT('00000000' + CAST(v.id_venta AS VARCHAR(8)), 8))) AS nro_comprobante,
    v.fecha_registro,
    COALESCE(v.documento_cliente, c.dni, '0') AS documento_cliente,
    COALESCE(v.nombre_cliente, LTRIM(RTRIM(COALESCE(c.nombre, '') + ' ' + COALESCE(c.apellido, ''))), N'Consumidor Final') AS nombre_cliente,
    COALESCE(LTRIM(RTRIM(u.nombre + ' ' + u.apellido)), N'(Sin registrar)') AS nombre_operador,
    v.monto_total,
    v.monto_pago,
    v.monto_cambio,
    v.estado,
    v.motivo_anulacion,
    v.fecha_anulacion,
    LTRIM(RTRIM(ua.nombre + ' ' + ua.apellido)) AS nombre_usuario_anula
FROM dbo.VENTA v
LEFT JOIN dbo.CLIENTE c ON c.id_cliente = v.id_cliente
LEFT JOIN dbo.USUARIO u ON u.id_usuario = v.id_usuario
LEFT JOIN dbo.USUARIO ua ON ua.id_usuario = v.id_usuario_anula
ORDER BY v.fecha_registro DESC, v.id_venta DESC;";

        var resultado = new List<ResumenVentaDatos>();
        await using var conexion = new SqlConnection(_cadenaConexion);
        await conexion.OpenAsync(ct);
        await using var comando = new SqlCommand(sql, conexion);
        await using var lector = await comando.ExecuteReaderAsync(ct);

        while (await lector.ReadAsync(ct))
        {
            resultado.Add(new ResumenVentaDatos(
                IdVenta: lector.GetInt32(0),
                NroComprobante: lector.GetString(1),
                FechaRegistro: lector.GetDateTime(2),
                DocumentoCliente: lector.GetString(3),
                NombreCliente: lector.GetString(4),
                NombreOperador: lector.GetString(5),
                MontoTotal: lector.GetDecimal(6),
                MontoPago: lector.GetDecimal(7),
                MontoCambio: lector.GetDecimal(8),
                Estado: lector.GetBoolean(9),
                MotivoAnulacion: lector.IsDBNull(10) ? null : lector.GetString(10),
                FechaAnulacion: lector.IsDBNull(11) ? null : lector.GetDateTime(11),
                NombreUsuarioAnula: lector.IsDBNull(12) ? null : lector.GetString(12)));
        }

        return resultado;
    }

    public async Task<IReadOnlyList<DetalleLineaVentaDatos>> ObtenerDetalleVentaAsync(
        int idVenta,
        CancellationToken ct = default)
    {
        const string sql = @"
SELECT
    dv.id_producto,
    COALESCE(p.codigo, '') AS codigo,
    COALESCE(p.nombre, N'(Producto)') AS nombre,
    dv.cantidad,
    dv.precio_venta,
    dv.subtotal
FROM dbo.DETALLE_VENTA dv
LEFT JOIN dbo.PRODUCTO p ON p.id_producto = dv.id_producto
WHERE dv.id_venta = @IdVenta
ORDER BY dv.id_detalle_venta ASC;";

        var resultado = new List<DetalleLineaVentaDatos>();
        await using var conexion = new SqlConnection(_cadenaConexion);
        await conexion.OpenAsync(ct);
        await using var comando = new SqlCommand(sql, conexion);
        comando.Parameters.Add(new SqlParameter("@IdVenta", SqlDbType.Int) { Value = idVenta });
        await using var lector = await comando.ExecuteReaderAsync(ct);

        while (await lector.ReadAsync(ct))
        {
            resultado.Add(new DetalleLineaVentaDatos(
                IdProducto: lector.GetInt32(0),
                Codigo: lector.GetString(1),
                NombreProducto: lector.GetString(2),
                Cantidad: lector.GetInt32(3),
                PrecioVenta: lector.GetDecimal(4),
                Subtotal: lector.GetDecimal(5)));
        }

        return resultado;
    }

    public async Task<ResultadoVentaDatos> AnularVentaAsync(
        int idVenta,
        int idUsuarioAnula,
        string motivoAnulacion,
        CancellationToken ct = default)
    {
        const string sql = @"
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @EstadoActual BIT;
    SELECT @EstadoActual = estado
    FROM dbo.VENTA WITH (UPDLOCK, HOLDLOCK)
    WHERE id_venta = @IdVenta;

    IF @EstadoActual IS NULL
        THROW 50010, N'La venta indicada no existe.', 1;

    IF @EstadoActual = 0
        THROW 50011, N'La venta seleccionada ya se encuentra anulada.', 1;

    UPDATE dbo.VENTA
    SET estado = 0,
        id_usuario_anula = @IdUsuarioAnula,
        fecha_anulacion = GETDATE(),
        motivo_anulacion = @Motivo
    WHERE id_venta = @IdVenta;

    UPDATE p
    SET p.stock = p.stock + dv.cantidad
    FROM dbo.PRODUCTO p WITH (UPDLOCK, HOLDLOCK)
    INNER JOIN dbo.DETALLE_VENTA dv ON dv.id_producto = p.id_producto
    WHERE dv.id_venta = @IdVenta;

    COMMIT TRANSACTION;
    SELECT 0 AS Codigo, N'Venta anulada y stock restituido con éxito.' AS Mensaje;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    SELECT ERROR_NUMBER() AS Codigo, ERROR_MESSAGE() AS Mensaje;
END CATCH;";

        try
        {
            await using var conexion = new SqlConnection(_cadenaConexion);
            await conexion.OpenAsync(ct);
            await using var comando = new SqlCommand(sql, conexion);
            comando.Parameters.Add(new SqlParameter("@IdVenta", SqlDbType.Int) { Value = idVenta });
            comando.Parameters.Add(new SqlParameter("@IdUsuarioAnula", SqlDbType.Int) { Value = idUsuarioAnula });
            comando.Parameters.Add(new SqlParameter("@Motivo", SqlDbType.NVarChar, 300) { Value = motivoAnulacion });

            await using var lector = await comando.ExecuteReaderAsync(ct);
            if (await lector.ReadAsync(ct))
            {
                int codigo = lector.GetInt32(0);
                string mensaje = lector.GetString(1);
                return new ResultadoVentaDatos(codigo == 0, idVenta, codigo, mensaje);
            }

            return new ResultadoVentaDatos(false, 0, -1, "No se obtuvo respuesta al anular la venta.");
        }
        catch (SqlException ex)
        {
            return new ResultadoVentaDatos(false, 0, ex.Number, ex.Message);
        }
    }
}

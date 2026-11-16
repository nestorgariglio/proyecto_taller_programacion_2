using System;
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
}

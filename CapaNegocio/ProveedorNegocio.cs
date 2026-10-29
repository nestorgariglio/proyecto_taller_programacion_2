using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CapaDatos;
using CapaEntidad;
using CapaNegocio.DTOs.Proveedores;
using CapaNegocio.Enums;

namespace CapaNegocio;

public class ProveedorNegocio
{
    private readonly AppDbContext _context;

    public ProveedorNegocio(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    private static ProveedorRespuestaDto MapearProveedor(Proveedor p)
    {
        return new ProveedorRespuestaDto
        {
            IdProveedor = p.IdProveedor,
            Cuit = p.Cuit ?? string.Empty,
            RazonSocial = p.RazonSocial ?? string.Empty,
            Correo = p.Correo,
            Telefono = p.Telefono,
            Estado = p.Estado,
            FechaRegistro = p.FechaRegistro
        };
    }

    private static readonly Regex PatronCuit = new(@"^[0-9]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex PatronRazonSocial = new(@"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ' -]{2,}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex PatronCorreo = new(@"^[^\s@]+@[^\s@]+\.[A-Za-z]{2,}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex PatronTelefono = new(@"^\d+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private const string MensajeErrorPersistencia = "No se pudo completar la operación por un problema con el origen de datos. Intente nuevamente.";
    private const int ConteoComprasNoDisponible = -1;

    private static string? NormalizarOpcional(string? valor)
    {
        string? recortado = valor?.Trim();
        return string.IsNullOrEmpty(recortado) ? null : recortado;
    }

    private static string? ValidarObligatoriosProveedor(string? cuit, string? razonSocial)
    {
        if (string.IsNullOrWhiteSpace(cuit)) return "El CUIT es obligatorio.";
        if (string.IsNullOrWhiteSpace(razonSocial)) return "La Razón Social es obligatoria.";
        return null;
    }

    private static string? ValidarCuit(string? cuitRecortado)
    {
        if (cuitRecortado is null || !PatronCuit.IsMatch(cuitRecortado))
            return "El CUIT debe ser solo números (solo dígitos 0-9).";
        return null;
    }

    private static string? ValidarRazonSocial(string? razonRecortado)
    {
        if (razonRecortado is null || !PatronRazonSocial.IsMatch(razonRecortado))
            return "La Razón Social debe admitir letras, espacios internos, apóstrofo y guion.";
        return null;
    }

    private static string? ValidarCorreo(string? correoRecortado)
    {
        if (correoRecortado is null) return null;
        if (!PatronCorreo.IsMatch(correoRecortado))
            return "El correo no tiene un formato válido (ejemplo: nombre@dominio.com).";
        return null;
    }

    private static string? ValidarTelefono(string? telefonoRecortado)
    {
        if (telefonoRecortado is null) return null;
        if (!PatronTelefono.IsMatch(telefonoRecortado))
            return "El teléfono solo admite dígitos.";
        return null;
    }

    private static string? ValidarDatosProveedor(string? cuit, string? razonSocial, string? correo, string? telefono)
    {
        return ValidarObligatoriosProveedor(cuit, razonSocial)
            ?? ValidarCuit(cuit)
            ?? ValidarRazonSocial(razonSocial)
            ?? ValidarCorreo(correo)
            ?? ValidarTelefono(telefono);
    }

    private async Task<string?> ValidarUnicidadCuitAsync(string cuit, int? idExcluir)
    {
        string comparacion = cuit.ToUpper();
        bool existe = await _context.Proveedores.AsNoTracking()
            .AnyAsync(p => (!idExcluir.HasValue || p.IdProveedor != idExcluir.Value)
                && p.Cuit != null && p.Cuit.ToUpper() == comparacion);
        if (!existe) return null;
        var existente = await _context.Proveedores.AsNoTracking()
            .Where(p => (!idExcluir.HasValue || p.IdProveedor != idExcluir.Value)
                && p.Cuit != null && p.Cuit.ToUpper() == comparacion)
            .Select(p => new { p.Estado })
            .FirstOrDefaultAsync();
        if (existente != null && existente.Estado)
            return $"Ya existe un proveedor con el CUIT \"{cuit}\".";
        return $"Ya existe un proveedor con el CUIT \"{cuit}\", actualmente inactivo.";
    }

    private async Task<string?> ValidarUnicidadCorreoAsync(string correo, int? idExcluir)
    {
        string comparacion = correo.ToUpper();
        bool existe = await _context.Proveedores.AsNoTracking()
            .AnyAsync(p => (!idExcluir.HasValue || p.IdProveedor != idExcluir.Value)
                && p.Correo != null && p.Correo != "" && p.Correo.ToUpper() == comparacion);
        return existe ? $"Ya existe un proveedor con el correo \"{correo}\"." : null;
    }

    private static bool EsFalloDeOrigenDeDatos(Exception excepcion)
    {
        return excepcion is DbUpdateException || excepcion is System.Data.Common.DbException || excepcion is InvalidOperationException;
    }

    private void DesadjuntarSiEsPosible(Proveedor p)
    {
        if (p != null) try { _context?.Entry(p)?.State = Microsoft.EntityFrameworkCore.EntityState.Detached; } catch { }
    }

    public async Task<RespuestaCreacionProveedor> CrearProveedorAsync(CrearProveedorDto dto)
    {
        string? cuit = NormalizarOpcional(dto.Cuit);
        string? razonSocial = NormalizarOpcional(dto.RazonSocial);
        string? correo = NormalizarOpcional(dto.Correo);
        string? telefono = NormalizarOpcional(dto.Telefono);

        if (string.IsNullOrWhiteSpace(cuit) || string.IsNullOrWhiteSpace(razonSocial))
        {
            // redundante con ValidarDatosProveedor; se mantiene por claridad
        }

        string? errorValidacion = ValidarDatosProveedor(cuit, razonSocial, correo, telefono);
        if (errorValidacion is not null)
        {
            return new RespuestaCreacionProveedor { Resultado = ResultadoCreacionProveedor.DatosInvalidos, Mensaje = errorValidacion };
        }

        var nuevo = new Proveedor { Cuit = cuit!, RazonSocial = razonSocial!, Correo = correo, Telefono = telefono, Estado = true };
        try
        {
            string? mensajeCuitDuplicado = await ValidarUnicidadCuitAsync(cuit!, null);
            if (mensajeCuitDuplicado is not null)
                return new RespuestaCreacionProveedor { Resultado = ResultadoCreacionProveedor.CuitDuplicado, Mensaje = mensajeCuitDuplicado };

            if (correo is not null)
            {
                string? mensajeCorreoDuplicado = await ValidarUnicidadCorreoAsync(correo, null);
                if (mensajeCorreoDuplicado is not null)
                    return new RespuestaCreacionProveedor { Resultado = ResultadoCreacionProveedor.CorreoDuplicado, Mensaje = mensajeCorreoDuplicado };
            }

            _context.Proveedores.Add(nuevo);
            await _context.SaveChangesAsync();
            var creado = MapearProveedor(nuevo);
            return new RespuestaCreacionProveedor { Resultado = ResultadoCreacionProveedor.Exito, Proveedor = creado, Mensaje = $"Proveedor {creado.RazonSocial} registrado exitosamente." };
        }
        catch (Exception ex) when (EsFalloDeOrigenDeDatos(ex))
        {
            DesadjuntarSiEsPosible(nuevo);
            return new RespuestaCreacionProveedor { Resultado = ResultadoCreacionProveedor.ErrorPersistencia, Mensaje = MensajeErrorPersistencia };
        }
    }

    public async Task<RespuestaModificacionProveedor> ModificarProveedorAsync(ModificarProveedorDto dto)
    {
        if (dto.IdProveedor <= 0)
            return new RespuestaModificacionProveedor { Resultado = ResultadoModificacionProveedor.DatosInvalidos, Mensaje = "Debe indicar un proveedor válido." };

        string? cuit = NormalizarOpcional(dto.Cuit);
        string? razonSocial = NormalizarOpcional(dto.RazonSocial);
        string? correo = NormalizarOpcional(dto.Correo);
        string? telefono = NormalizarOpcional(dto.Telefono);

        string? errorValidacion = ValidarDatosProveedor(cuit, razonSocial, correo, telefono);
        if (errorValidacion is not null)
            return new RespuestaModificacionProveedor { Resultado = ResultadoModificacionProveedor.DatosInvalidos, Mensaje = errorValidacion };

        Proveedor? proveedor = null;
        try
        {
            proveedor = await _context.Proveedores.FirstOrDefaultAsync(p => p.IdProveedor == dto.IdProveedor);
            if (proveedor is null)
                return new RespuestaModificacionProveedor { Resultado = ResultadoModificacionProveedor.ProveedorNoEncontrado, Mensaje = "El proveedor indicado no existe." };

            string? mensajeCuitDuplicado = await ValidarUnicidadCuitAsync(cuit!, dto.IdProveedor);
            if (mensajeCuitDuplicado is not null)
                return new RespuestaModificacionProveedor { Resultado = ResultadoModificacionProveedor.CuitDuplicado, Mensaje = mensajeCuitDuplicado };

            if (correo is not null)
            {
                string? mensajeCorreoDuplicado = await ValidarUnicidadCorreoAsync(correo, dto.IdProveedor);
                if (mensajeCorreoDuplicado is not null)
                    return new RespuestaModificacionProveedor { Resultado = ResultadoModificacionProveedor.CorreoDuplicado, Mensaje = mensajeCorreoDuplicado };
            }

            proveedor.Cuit = cuit!;
            proveedor.RazonSocial = razonSocial!;
            proveedor.Correo = correo;
            proveedor.Telefono = telefono;

            await _context.SaveChangesAsync();
            var modificado = MapearProveedor(proveedor);
            return new RespuestaModificacionProveedor { Resultado = ResultadoModificacionProveedor.Exito, Proveedor = modificado, Mensaje = $"Proveedor {modificado.RazonSocial} modificado exitosamente." };
        }
        catch (Exception ex) when (EsFalloDeOrigenDeDatos(ex))
        {
            if (proveedor is not null) DesadjuntarSiEsPosible(proveedor);
            return new RespuestaModificacionProveedor { Resultado = ResultadoModificacionProveedor.ErrorPersistencia, Mensaje = MensajeErrorPersistencia };
        }
    }

    public async Task<ProveedorRespuestaDto?> ObtenerProveedorAsync(int id)
    {
        var p = await _context.Proveedores.AsNoTracking().FirstOrDefaultAsync(x => x.IdProveedor == id);
        return p is null ? null : MapearProveedor(p);
    }

    public async Task<RespuestaListadoProveedores> ListarProveedoresAsync(FiltroEstadoProveedor filtro, string? textoBusqueda = null)
    {
        string? textoRecortado = textoBusqueda?.Trim();
        bool hayTexto = !string.IsNullOrEmpty(textoRecortado);
        try
        {
            IQueryable<Proveedor> consulta = _context.Proveedores.AsNoTracking();
            switch (filtro)
            {
                case FiltroEstadoProveedor.Activos: consulta = consulta.Where(p => p.Estado); break;
                case FiltroEstadoProveedor.Inactivos: consulta = consulta.Where(p => !p.Estado); break;
            }
            if (hayTexto)
            {
                string upper = textoRecortado!.ToUpper();
                consulta = consulta.Where(p => (p.Cuit != null && p.Cuit.ToUpper().Contains(upper)) || (p.RazonSocial != null && p.RazonSocial.ToUpper().Contains(upper)));
            }
            consulta = consulta.OrderBy(p => p.RazonSocial).ThenBy(p => p.IdProveedor);
            var lista = await consulta.Select(p => new ProveedorRespuestaDto { IdProveedor = p.IdProveedor, Cuit = p.Cuit ?? string.Empty, RazonSocial = p.RazonSocial ?? string.Empty, Correo = p.Correo, Telefono = p.Telefono, Estado = p.Estado, FechaRegistro = p.FechaRegistro }).ToListAsync();
            if (hayTexto && lista.Count == 0)
                return new RespuestaListadoProveedores { Resultado = ResultadoListadoProveedores.SinResultados, Mensaje = "No se encontraron proveedores que coincidan con la búsqueda." };
            return new RespuestaListadoProveedores
            {
                Resultado = ResultadoListadoProveedores.ConResultados,
                Proveedores = lista,
                Mensaje = lista.Count == 0 ? "No hay proveedores para mostrar con el filtro seleccionado." : (lista.Count == 1 ? "Se encontró 1 proveedor." : $"Se encontraron {lista.Count} proveedores.")
            };
        }
        catch (Exception ex) when (EsFalloDeOrigenDeDatos(ex))
        {
            return new RespuestaListadoProveedores { Resultado = ResultadoListadoProveedores.ErrorPersistencia, Mensaje = MensajeErrorPersistencia, Proveedores = new List<ProveedorRespuestaDto>() };
        }
    }

    public async Task<RespuestaListadoProveedores> BuscarProveedoresParaPosAsync(string? textoBusqueda)
    {
        string? textoRecortado = textoBusqueda?.Trim();
        if (string.IsNullOrEmpty(textoRecortado))
            return new RespuestaListadoProveedores { Resultado = ResultadoListadoProveedores.SinResultados, Mensaje = "No se encontraron proveedores que coincidan con la búsqueda." };
        try
        {
            string upper = textoRecortado.ToUpper();
            var consulta = _context.Proveedores.AsNoTracking().Where(p => p.Estado)
                .Where(p => (p.Cuit != null && p.Cuit.ToUpper().Contains(upper)) || (p.RazonSocial != null && p.RazonSocial.ToUpper().Contains(upper)))
                .OrderBy(p => p.RazonSocial).ThenBy(p => p.IdProveedor).Take(20);
            var lista = await consulta.Select(p => new ProveedorRespuestaDto { IdProveedor = p.IdProveedor, Cuit = p.Cuit ?? string.Empty, RazonSocial = p.RazonSocial ?? string.Empty, Correo = p.Correo, Telefono = p.Telefono, Estado = p.Estado, FechaRegistro = p.FechaRegistro }).ToListAsync();
            if (lista.Count == 0)
                return new RespuestaListadoProveedores { Resultado = ResultadoListadoProveedores.SinResultados, Mensaje = "No se encontraron proveedores que coincidan con la búsqueda." };
            return new RespuestaListadoProveedores { Resultado = ResultadoListadoProveedores.ConResultados, Proveedores = lista, Mensaje = lista.Count == 1 ? "Se encontró 1 proveedor." : $"Se encontraron {lista.Count} proveedores." };
        }
        catch (Exception ex) when (EsFalloDeOrigenDeDatos(ex))
        {
            return new RespuestaListadoProveedores { Resultado = ResultadoListadoProveedores.ErrorPersistencia, Mensaje = MensajeErrorPersistencia, Proveedores = new List<ProveedorRespuestaDto>() };
        }
    }

    public async Task<int> ContarComprasPorProveedorAsync(int idProveedor)
    {
        try { return await _context.Compras.AsNoTracking().CountAsync(c => c.IdProveedor == idProveedor); }
        catch (Exception ex) when (EsFalloDeOrigenDeDatos(ex)) { return ConteoComprasNoDisponible; }
    }

    public async Task<RespuestaConfirmacionBajaProveedor> ObtenerConfirmacionBajaProveedorAsync(int idProveedor, string? rol)
    {
        if (!PoliticaAcceso.PuedeAdministrarProveedores(rol))
            return new RespuestaConfirmacionBajaProveedor { Resultado = ResultadoConfirmacionBajaProveedor.PermisoDenegado, Mensaje = "No tiene permisos para dar de baja proveedores." };
        try
        {
            var proveedor = await _context.Proveedores.AsNoTracking().FirstOrDefaultAsync(p => p.IdProveedor == idProveedor);
            if (proveedor is null)
                return new RespuestaConfirmacionBajaProveedor { Resultado = ResultadoConfirmacionBajaProveedor.ProveedorNoEncontrado, Mensaje = "El proveedor indicado no existe." };
            if (!proveedor.Estado)
                return new RespuestaConfirmacionBajaProveedor { Resultado = ResultadoConfirmacionBajaProveedor.ProveedorYaInactivo, Mensaje = $"El proveedor {proveedor.RazonSocial ?? ""} ya está inactivo." };
            int comprasAsociadas = await ContarComprasPorProveedorAsync(idProveedor);
            if (comprasAsociadas == ConteoComprasNoDisponible)
                return new RespuestaConfirmacionBajaProveedor { Resultado = ResultadoConfirmacionBajaProveedor.ErrorPersistencia, Mensaje = MensajeErrorPersistencia };
            string mensaje = comprasAsociadas switch
            {
                0 => $"¿Desea dar de baja al proveedor {proveedor.RazonSocial ?? ""}?",
                1 => $"El proveedor {proveedor.RazonSocial ?? ""} tiene 1 compra asociada. ¿Desea darle de baja y conservar las compras?",
                _ => $"El proveedor {proveedor.RazonSocial ?? ""} tiene {comprasAsociadas} compras asociadas. ¿Desea darle de baja y conservar las compras?"
            };
            return new RespuestaConfirmacionBajaProveedor { Resultado = ResultadoConfirmacionBajaProveedor.ConfirmacionRequerida, ComprasAsociadas = comprasAsociadas, Mensaje = mensaje };
        }
        catch (Exception ex) when (EsFalloDeOrigenDeDatos(ex))
        {
            return new RespuestaConfirmacionBajaProveedor { Resultado = ResultadoConfirmacionBajaProveedor.ErrorPersistencia, Mensaje = MensajeErrorPersistencia };
        }
    }

    public async Task<RespuestaBajaProveedor> DarBajaProveedorAsync(int idProveedor, string? rol)
    {
        if (!PoliticaAcceso.PuedeAdministrarProveedores(rol))
            return new RespuestaBajaProveedor { Resultado = ResultadoBajaProveedor.PermisoDenegado, Mensaje = "No tiene permisos para dar de baja proveedores." };
        if (idProveedor <= 0)
            return new RespuestaBajaProveedor { Resultado = ResultadoBajaProveedor.DatosInvalidos, Mensaje = "Debe indicar un proveedor válido." };
        Proveedor? proveedor = null;
        try
        {
            proveedor = await _context.Proveedores.FirstOrDefaultAsync(p => p.IdProveedor == idProveedor);
            if (proveedor is null)
                return new RespuestaBajaProveedor { Resultado = ResultadoBajaProveedor.ProveedorNoEncontrado, Mensaje = "El proveedor indicado no existe." };
            if (!proveedor.Estado)
                return new RespuestaBajaProveedor { Resultado = ResultadoBajaProveedor.ProveedorYaInactivo, Mensaje = $"El proveedor {proveedor.RazonSocial ?? ""} ya está inactivo." };
            proveedor.Estado = false;
            await _context.SaveChangesAsync();
            var baja = MapearProveedor(proveedor);
            return new RespuestaBajaProveedor { Resultado = ResultadoBajaProveedor.Exito, Proveedor = baja, Mensaje = $"Proveedor {baja.RazonSocial} dado de baja exitosamente." };
        }
        catch (Exception ex) when (EsFalloDeOrigenDeDatos(ex))
        {
            if (proveedor is not null) DesadjuntarSiEsPosible(proveedor);
            return new RespuestaBajaProveedor { Resultado = ResultadoBajaProveedor.ErrorPersistencia, Mensaje = MensajeErrorPersistencia };
        }
    }

    public async Task<RespuestaActivacionProveedor> ReactivarProveedorAsync(int idProveedor, string? rol)
    {
        if (!PoliticaAcceso.PuedeAdministrarProveedores(rol))
            return new RespuestaActivacionProveedor { Resultado = ResultadoActivacionProveedor.PermisoDenegado, Mensaje = "No tiene permisos para reactivar proveedores." };
        if (idProveedor <= 0)
            return new RespuestaActivacionProveedor { Resultado = ResultadoActivacionProveedor.DatosInvalidos, Mensaje = "Debe indicar un proveedor válido." };
        Proveedor? proveedor = null;
        try
        {
            proveedor = await _context.Proveedores.FirstOrDefaultAsync(p => p.IdProveedor == idProveedor);
            if (proveedor is null)
                return new RespuestaActivacionProveedor { Resultado = ResultadoActivacionProveedor.ProveedorNoEncontrado, Mensaje = "El proveedor indicado no existe." };
            if (proveedor.Estado)
                return new RespuestaActivacionProveedor { Resultado = ResultadoActivacionProveedor.ProveedorYaActivo, Mensaje = $"El proveedor {proveedor.RazonSocial ?? ""} ya está activo." };
            proveedor.Estado = true;
            await _context.SaveChangesAsync();
            var reactivado = MapearProveedor(proveedor);
            return new RespuestaActivacionProveedor { Resultado = ResultadoActivacionProveedor.Exito, Proveedor = reactivado, Mensaje = $"Proveedor {reactivado.RazonSocial} reactivado exitosamente." };
        }
        catch (Exception ex) when (EsFalloDeOrigenDeDatos(ex))
        {
            if (proveedor is not null) DesadjuntarSiEsPosible(proveedor);
            return new RespuestaActivacionProveedor { Resultado = ResultadoActivacionProveedor.ErrorPersistencia, Mensaje = MensajeErrorPersistencia };
        }
    }
}



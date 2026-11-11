using Xunit;
using CapaNegocio.DTOs.Categorias;
using CapaNegocio.Enums;

namespace CapaNegocio.IntegrationTests;

/// <summary>
/// Pruebas contra SQL Server real (DB_SISTEMA_VENTA_TEST): la insensibilidad a
/// mayúsculas de la unicidad (RF-3/RF-4) depende de la colación de la BD, que
/// InMemory no modela. No se borran filas: cada ejecución usa descripciones únicas.
/// </summary>
public class CategoriaUnicidadSqlServerTests
{
    private static string DescripcionUnica() => $"ITEST-{Guid.NewGuid():N}"[..20];

    private static CategoriaNegocio CrearNegocio() => new(BaseSqlServerPruebas.CrearFabrica());

    [FactSqlServer]
    public async Task CrearCategoria_MismaDescripcionConOtraCaja_RechazaDuplicada()
    {
        var negocio = CrearNegocio();
        string descripcion = DescripcionUnica();

        var alta = await negocio.CrearCategoriaAsync(new CrearCategoriaDto { Descripcion = descripcion });
        Assert.Equal(ResultadoCreacionCategoria.Exito, alta.Resultado);

        var duplicada = await negocio.CrearCategoriaAsync(
            new CrearCategoriaDto { Descripcion = descripcion.ToLowerInvariant() });

        Assert.Equal(ResultadoCreacionCategoria.DescripcionDuplicada, duplicada.Resultado);
        Assert.Null(duplicada.Categoria);
    }

    [FactSqlServer]
    public async Task CrearCategoria_DuplicadoDeInactivaConOtraCaja_MensajeIndicaInactiva()
    {
        var negocio = CrearNegocio();
        string descripcion = DescripcionUnica();

        var alta = await negocio.CrearCategoriaAsync(new CrearCategoriaDto { Descripcion = descripcion });
        var baja = await negocio.DarBajaCategoriaAsync(alta.Categoria!.IdCategoria, productosActivosConfirmados: 0);
        Assert.Equal(ResultadoBajaCategoria.Exito, baja.Resultado);

        var duplicada = await negocio.CrearCategoriaAsync(
            new CrearCategoriaDto { Descripcion = descripcion.ToUpperInvariant() });

        Assert.Equal(ResultadoCreacionCategoria.DescripcionDuplicada, duplicada.Resultado);
        Assert.Contains("inactiva", duplicada.Mensaje);
    }

    [FactSqlServer]
    public async Task ModificarCategoria_HaciaDescripcionDeOtraConOtraCaja_RechazaDuplicada()
    {
        var negocio = CrearNegocio();
        string primera = DescripcionUnica();
        string segunda = DescripcionUnica();

        await negocio.CrearCategoriaAsync(new CrearCategoriaDto { Descripcion = primera });
        var alta = await negocio.CrearCategoriaAsync(new CrearCategoriaDto { Descripcion = segunda });

        var respuesta = await negocio.ModificarCategoriaAsync(new ModificarCategoriaDto
        {
            IdCategoria = alta.Categoria!.IdCategoria,
            Descripcion = primera.ToLowerInvariant()
        });

        Assert.Equal(ResultadoModificacionCategoria.DescripcionDuplicada, respuesta.Resultado);
    }

    [FactSqlServer]
    public async Task ModificarCategoria_SoloCambiaLaCajaDeSuPropiaDescripcion_AutoexclusionPermiteGuardar()
    {
        var negocio = CrearNegocio();
        string descripcion = DescripcionUnica();

        var alta = await negocio.CrearCategoriaAsync(new CrearCategoriaDto { Descripcion = descripcion });

        var respuesta = await negocio.ModificarCategoriaAsync(new ModificarCategoriaDto
        {
            IdCategoria = alta.Categoria!.IdCategoria,
            Descripcion = descripcion.ToLowerInvariant()
        });

        Assert.Equal(ResultadoModificacionCategoria.Exito, respuesta.Resultado);
    }
}

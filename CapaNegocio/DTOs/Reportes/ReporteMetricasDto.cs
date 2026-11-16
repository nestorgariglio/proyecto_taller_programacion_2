namespace CapaNegocio.DTOs.Reportes;

/// <summary>
/// DTO contenedor de métricas, evolución temporal, categorías y resumen para el módulo de reportes.
/// </summary>
public class ReporteMetricasDto
{
    public decimal FacturacionTotal { get; set; }
    public decimal GananciaTotal { get; set; }
    public decimal TicketPromedio { get; set; }
    public int CantidadVentas { get; set; }
    public List<EvolucionVentaItemDto> EvolucionVentas { get; set; } = new();
    public List<ParticipacionCategoriaItemDto> ParticipacionCategorias { get; set; } = new();
    public List<ResumenDiarioItemDto> ResumenDiario { get; set; } = new();
}

public class EvolucionVentaItemDto
{
    public DateTime Fecha { get; set; }
    public string EtiquetaFecha { get; set; } = string.Empty;
    public decimal TotalFacturado { get; set; }
}

public class ParticipacionCategoriaItemDto
{
    public string Categoria { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public double Porcentaje { get; set; }
}

public class ResumenDiarioItemDto
{
    public DateTime Fecha { get; set; }
    public string FechaTexto { get; set; } = string.Empty;
    public int ComprobantesEmitidos { get; set; }
    public decimal VentasTotales { get; set; }
    public decimal Ganancia { get; set; }
}

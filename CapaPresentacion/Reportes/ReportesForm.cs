using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CapaNegocio;
using CapaNegocio.DTOs.Reportes;
using CapaPresentacion.Tema;
using MaterialSkin;
using MaterialSkin.Controls;
using LiveCharts;
using LiveCharts.Wpf;

// Alias explícitos para resolver ambigüedad entre WinForms y WPF
using CartesianChart = LiveCharts.WinForms.CartesianChart;
using PieChart = LiveCharts.WinForms.PieChart;

namespace CapaPresentacion.Reportes
{
    public partial class ReportesForm : UserControl
    {
        private readonly ReporteNegocio? _reporteNegocio;
        private readonly System.Windows.Media.Brush _colorDeepOrange =
            (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#FF5722");

        private MaterialComboBox _cboPeriodo = null!;
        private MaterialButton _btnFiltrar = null!;
        private MaterialLabel _lblFacturacionTotal = null!;
        private MaterialLabel _lblGananciaTotal = null!;
        private MaterialLabel _lblTicketPromedio = null!;
        private CartesianChart _chartVentas = null!;
        private PieChart _chartCategorias = null!;
        private DataGridView _dgv = null!;

        public ReportesForm(ReporteNegocio? reporteNegocio = null)
        {
            _reporteNegocio = reporteNegocio;
            InitializeComponent();
            BackColor = TemaAplicacion.FondoPrincipal;
            ArmarInterfazMaterial();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await CargarReporteAsync();
        }

        public async Task CargarReporteAsync()
        {
            if (_reporteNegocio == null) return;

            try
            {
                if (_btnFiltrar != null) _btnFiltrar.Enabled = false;

                string periodo = _cboPeriodo?.SelectedItem?.ToString() ?? "Últimos 7 días";
                ReporteMetricasDto metricas = await _reporteNegocio.ObtenerReporteAsync(periodo);

                var cultura = CultureInfo.GetCultureInfo("es-AR");

                if (_lblFacturacionTotal != null)
                    _lblFacturacionTotal.Text = metricas.FacturacionTotal.ToString("C2", cultura);

                if (_lblGananciaTotal != null)
                    _lblGananciaTotal.Text = metricas.GananciaTotal.ToString("C2", cultura);

                if (_lblTicketPromedio != null)
                    _lblTicketPromedio.Text = metricas.TicketPromedio.ToString("C2", cultura);

                // 1. Gráfico de Evolución de Ventas
                if (_chartVentas != null)
                {
                    _chartVentas.AxisX.Clear();
                    _chartVentas.AxisX.Add(new Axis
                    {
                        Title = "Período",
                        Labels = metricas.EvolucionVentas.Select(x => x.EtiquetaFecha).ToArray(),
                        Foreground = System.Windows.Media.Brushes.Gray
                    });

                    _chartVentas.Series = new SeriesCollection
                    {
                        new ColumnSeries
                        {
                            Title = "Facturado ($)",
                            Values = new ChartValues<double>(metricas.EvolucionVentas.Select(x => (double)x.TotalFacturado)),
                            Fill = _colorDeepOrange
                        }
                    };
                }

                // 2. Gráfico de Participación por Categoría
                if (_chartCategorias != null)
                {
                    var coleccionPie = new SeriesCollection();
                    if (metricas.ParticipacionCategorias.Count > 0)
                    {
                        foreach (var cat in metricas.ParticipacionCategorias)
                        {
                            coleccionPie.Add(new PieSeries
                            {
                                Title = cat.Categoria,
                                Values = new ChartValues<double> { (double)cat.Monto },
                                DataLabels = true,
                                LabelPoint = cp => $"{cp.SeriesView.Title}: {cp.Y.ToString("C0", cultura)} ({cp.Participation:P0})"
                            });
                        }
                    }
                    else
                    {
                        coleccionPie.Add(new PieSeries
                        {
                            Title = "Sin ventas",
                            Values = new ChartValues<double> { 1 },
                            DataLabels = false
                        });
                    }
                    _chartCategorias.Series = coleccionPie;
                }

                // 3. Grilla de Resumen Diario
                if (_dgv != null)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("Fecha");
                    dt.Columns.Add("Comprobantes Emitidos");
                    dt.Columns.Add("Ventas Totales");
                    dt.Columns.Add("Ganancia Est.");

                    foreach (var resumen in metricas.ResumenDiario)
                    {
                        dt.Rows.Add(
                            resumen.FechaTexto,
                            resumen.ComprobantesEmitidos.ToString(),
                            resumen.VentasTotales.ToString("C2", cultura),
                            resumen.Ganancia.ToString("C2", cultura)
                        );
                    }

                    _dgv.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al actualizar reporte: {ex.Message}");
            }
            finally
            {
                if (_btnFiltrar != null) _btnFiltrar.Enabled = true;
            }
        }

        private void ArmarInterfazMaterial()
        {
            Controls.Clear();

            Panel panelScroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(15),
                BackColor = TemaAplicacion.FondoPrincipal
            };

            // 1. FILTROS
            MaterialCard cardFiltros = new MaterialCard
            {
                Location = new Point(15, 15),
                Height = 90,
                Width = panelScroll.ClientSize.Width - 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Padding = new Padding(15)
            };

            MaterialLabel lblTitulo = new MaterialLabel { Text = "📊 TABLERO DE REPORTES Y MÉTRICAS", FontType = MaterialSkinManager.fontType.H6, Location = new Point(15, 15), AutoSize = true };
            _cboPeriodo = new MaterialComboBox { Hint = "Período", Location = new Point(380, 15), Width = 200, StartIndex = 0 };
            _cboPeriodo.Items.AddRange(new object[] { "Últimos 7 días", "Este Mes", "Último Trimestre", "Año Actual" });
            _cboPeriodo.SelectedIndexChanged += async (s, e) => await CargarReporteAsync();

            _btnFiltrar = new MaterialButton { Text = "🔄 Actualizar", Location = new Point(595, 20), Height = 48, Type = MaterialButton.MaterialButtonType.Contained };
            _btnFiltrar.Click += async (s, e) => await CargarReporteAsync();

            cardFiltros.Controls.Add(lblTitulo);
            cardFiltros.Controls.Add(_cboPeriodo);
            cardFiltros.Controls.Add(_btnFiltrar);

            // 2. KPIs (GRILLA FLUIDA 33.3%)
            TableLayoutPanel tableKpis = new TableLayoutPanel
            {
                Location = new Point(15, 120),
                Height = 100,
                Width = panelScroll.ClientSize.Width - 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ColumnCount = 3,
                RowCount = 1
            };
            tableKpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tableKpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tableKpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));

            MaterialCard cardKpi1 = new MaterialCard { Dock = DockStyle.Fill, Margin = new Padding(5) };
            cardKpi1.Controls.Add(new MaterialLabel { Text = "💰 FACTURACIÓN TOTAL", FontType = MaterialSkinManager.fontType.Subtitle2, Location = new Point(15, 15), AutoSize = true });
            _lblFacturacionTotal = new MaterialLabel { Text = "$ 0,00", FontType = MaterialSkinManager.fontType.H5, Location = new Point(15, 45), AutoSize = true, UseAccent = true };
            cardKpi1.Controls.Add(_lblFacturacionTotal);

            MaterialCard cardKpi2 = new MaterialCard { Dock = DockStyle.Fill, Margin = new Padding(5) };
            cardKpi2.Controls.Add(new MaterialLabel { Text = "📈 GANANCIA (MARGEN REAL)", FontType = MaterialSkinManager.fontType.Subtitle2, Location = new Point(15, 15), AutoSize = true });
            _lblGananciaTotal = new MaterialLabel { Text = "$ 0,00", FontType = MaterialSkinManager.fontType.H5, Location = new Point(15, 45), AutoSize = true };
            cardKpi2.Controls.Add(_lblGananciaTotal);

            MaterialCard cardKpi3 = new MaterialCard { Dock = DockStyle.Fill, Margin = new Padding(5) };
            cardKpi3.Controls.Add(new MaterialLabel { Text = "🧾 TICKET PROMEDIO", FontType = MaterialSkinManager.fontType.Subtitle2, Location = new Point(15, 15), AutoSize = true });
            _lblTicketPromedio = new MaterialLabel { Text = "$ 0,00", FontType = MaterialSkinManager.fontType.H5, Location = new Point(15, 45), AutoSize = true };
            cardKpi3.Controls.Add(_lblTicketPromedio);

            tableKpis.Controls.Add(cardKpi1, 0, 0);
            tableKpis.Controls.Add(cardKpi2, 1, 0);
            tableKpis.Controls.Add(cardKpi3, 2, 0);

            // 3. GRÁFICOS INTERACTIVOS (GRILLA 60% / 40%)
            MaterialCard cardGraficos = new MaterialCard
            {
                Location = new Point(15, 235),
                Height = 300,
                Width = panelScroll.ClientSize.Width - 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Padding = new Padding(15)
            };

            MaterialLabel lblTitGraficos = new MaterialLabel { Text = "📈 EVOLUCIÓN DE VENTAS Y PARTICIPACIÓN POR CATEGORÍA", FontType = MaterialSkinManager.fontType.Subtitle1, Location = new Point(15, 12), AutoSize = true };

            TableLayoutPanel tableCharts = new TableLayoutPanel
            {
                Location = new Point(15, 45),
                Height = 235,
                Width = cardGraficos.Width - 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ColumnCount = 2,
                RowCount = 1
            };
            tableCharts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tableCharts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            _chartVentas = new CartesianChart { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            _chartVentas.AxisY.Add(new Axis { Title = "Monto ($)", LabelFormatter = value => value.ToString("N0"), Foreground = System.Windows.Media.Brushes.Gray });

            _chartCategorias = new PieChart { Dock = DockStyle.Fill, BackColor = Color.Transparent, LegendLocation = LegendLocation.Bottom };

            tableCharts.Controls.Add(_chartVentas, 0, 0);
            tableCharts.Controls.Add(_chartCategorias, 1, 0);

            cardGraficos.Controls.Add(lblTitGraficos);
            cardGraficos.Controls.Add(tableCharts);

            // 4. TABLA DE RESUMEN DIARIO
            MaterialCard cardTabla = new MaterialCard
            {
                Location = new Point(15, 550),
                Height = 230,
                Width = panelScroll.ClientSize.Width - 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Padding = new Padding(15)
            };

            MaterialLabel lblTitTabla = new MaterialLabel { Text = "📄 RESUMEN DIARIO DE OPERACIONES", FontType = MaterialSkinManager.fontType.Subtitle1, Location = new Point(15, 12), AutoSize = true };

            _dgv = new DataGridView
            {
                Location = new Point(15, 45),
                Size = new Size(cardTabla.Width - 30, 165),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.FromArgb(40, 40, 40),
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false
            };

            _dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
            _dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Roboto", 9, FontStyle.Bold);
            _dgv.ColumnHeadersHeight = 35;
            _dgv.DefaultCellStyle.BackColor = Color.FromArgb(50, 50, 50);
            _dgv.DefaultCellStyle.ForeColor = Color.White;
            _dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(70, 70, 70);
            _dgv.DefaultCellStyle.SelectionForeColor = Color.White;

            cardTabla.Controls.Add(lblTitTabla);
            cardTabla.Controls.Add(_dgv);

            panelScroll.Controls.Add(cardFiltros);
            panelScroll.Controls.Add(tableKpis);
            panelScroll.Controls.Add(cardGraficos);
            panelScroll.Controls.Add(cardTabla);

            Controls.Add(panelScroll);
        }
    }
}

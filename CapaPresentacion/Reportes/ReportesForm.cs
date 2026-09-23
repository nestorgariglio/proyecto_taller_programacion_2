using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using LiveCharts;
using LiveCharts.Wpf;

// Alias explícitos para resolver ambigüedad entre WinForms y WPF
using CartesianChart = LiveCharts.WinForms.CartesianChart;
using PieChart = LiveCharts.WinForms.PieChart;

namespace CapaPresentacion.Reportes
{
    public partial class ReportesForm : Form
    {
        public ReportesForm()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            ArmarInterfazMaterial();
        }

        private void ArmarInterfazMaterial()
        {
            Controls.Clear();

            Panel panelScroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(15)
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
            MaterialComboBox cboPeriodo = new MaterialComboBox { Hint = "Período", Location = new Point(380, 15), Width = 200, StartIndex = 0 };
            cboPeriodo.Items.AddRange(new object[] { "Últimos 7 días", "Este Mes", "Último Trimestre", "Año Actual" });
            MaterialButton btnFiltrar = new MaterialButton { Text = "🔄 Actualizar", Location = new Point(595, 20), Height = 48, Type = MaterialButton.MaterialButtonType.Contained };

            cardFiltros.Controls.Add(lblTitulo);
            cardFiltros.Controls.Add(cboPeriodo);
            cardFiltros.Controls.Add(btnFiltrar);

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
            cardKpi1.Controls.Add(new MaterialLabel { Text = "$ 3.450.000,00", FontType = MaterialSkinManager.fontType.H5, Location = new Point(15, 45), AutoSize = true, UseAccent = true });

            MaterialCard cardKpi2 = new MaterialCard { Dock = DockStyle.Fill, Margin = new Padding(5) };
            cardKpi2.Controls.Add(new MaterialLabel { Text = "📈 GANANCIA (EST. 30%)", FontType = MaterialSkinManager.fontType.Subtitle2, Location = new Point(15, 15), AutoSize = true });
            cardKpi2.Controls.Add(new MaterialLabel { Text = "$ 1.035.000,00", FontType = MaterialSkinManager.fontType.H5, Location = new Point(15, 45), AutoSize = true });

            MaterialCard cardKpi3 = new MaterialCard { Dock = DockStyle.Fill, Margin = new Padding(5) };
            cardKpi3.Controls.Add(new MaterialLabel { Text = "🧾 TICKET PROMEDIO", FontType = MaterialSkinManager.fontType.Subtitle2, Location = new Point(15, 15), AutoSize = true });
            cardKpi3.Controls.Add(new MaterialLabel { Text = "$ 24.640,00", FontType = MaterialSkinManager.fontType.H5, Location = new Point(15, 45), AutoSize = true });

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

            CartesianChart chartVentas = new CartesianChart { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            var colorDeepOrange = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#FF5722");
            chartVentas.Series = new SeriesCollection { new ColumnSeries { Title = "Facturado ($)", Values = new ChartValues<double> { 120000, 185400, 210000, 150000, 310000, 280000, 340000 }, Fill = colorDeepOrange } };
            chartVentas.AxisX.Add(new Axis { Title = "Día", Labels = new[] { "14/09", "15/09", "16/09", "17/09", "18/09", "19/09", "20/09" }, Foreground = System.Windows.Media.Brushes.Gray });
            chartVentas.AxisY.Add(new Axis { Title = "Monto ($)", LabelFormatter = value => value.ToString("N0"), Foreground = System.Windows.Media.Brushes.Gray });

            PieChart chartCategorias = new PieChart { Dock = DockStyle.Fill, BackColor = Color.Transparent, LegendLocation = LegendLocation.Bottom };
            chartCategorias.Series = new SeriesCollection
            {
                new PieSeries { Title = "Periféricos", Values = new ChartValues<double> { 45 }, DataLabels = true },
                new PieSeries { Title = "Monitores", Values = new ChartValues<double> { 30 }, DataLabels = true },
                new PieSeries { Title = "Audio", Values = new ChartValues<double> { 15 }, DataLabels = true },
                new PieSeries { Title = "Otros", Values = new ChartValues<double> { 10 }, DataLabels = true }
            };

            tableCharts.Controls.Add(chartVentas, 0, 0);
            tableCharts.Controls.Add(chartCategorias, 1, 0);

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

            DataGridView dgv = new DataGridView
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

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Roboto", 9, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 35;
            dgv.DefaultCellStyle.BackColor = Color.FromArgb(50, 50, 50);
            dgv.DefaultCellStyle.ForeColor = Color.White;

            DataTable dt = new DataTable();
            dt.Columns.Add("Fecha");
            dt.Columns.Add("Comprobantes Emitidos");
            dt.Columns.Add("Ventas Totales");
            dt.Columns.Add("Ganancia Est. (30%)");
            dt.Rows.Add("20/09/2026", "14", "$ 185.400,00", "$ 55.620,00");
            dt.Rows.Add("19/09/2026", "22", "$ 310.000,00", "$ 93.000,00");
            dt.Rows.Add("18/09/2026", "18", "$ 280.000,00", "$ 84.000,00");

            dgv.DataSource = dt;

            cardTabla.Controls.Add(lblTitTabla);
            cardTabla.Controls.Add(dgv);

            panelScroll.Controls.Add(cardFiltros);
            panelScroll.Controls.Add(tableKpis);
            panelScroll.Controls.Add(cardGraficos);
            panelScroll.Controls.Add(cardTabla);

            Controls.Add(panelScroll);
        }
    }
}
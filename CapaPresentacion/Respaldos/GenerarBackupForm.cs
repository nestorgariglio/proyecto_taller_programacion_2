using CapaNegocio;
using CapaNegocio.DTOs.Respaldos;
using CapaNegocio.Enums;
using MaterialSkin;
using MaterialSkin.Controls;

namespace CapaPresentacion.Respaldos;

/// <summary>
/// Modal «Generar backup». Solo usa <see cref="RespaldoNegocio"/> y sus DTOs; la
/// operación es asíncrona, no bloquea la interfaz y se puede cancelar (Cancelar, X o
/// Escape): el cierre espera a que SQL Server termine realmente el comando.
/// </summary>
public sealed class GenerarBackupForm : MaterialForm
{
    private readonly RespaldoNegocio _negocio;
    private readonly MaterialLabel _lblDescripcion;
    private readonly MaterialLabel _lblMensaje;
    private readonly MaterialProgressBar _progreso;
    private readonly MaterialButton _btnGenerar;
    private readonly MaterialButton _btnCancelar;
    private CancellationTokenSource? _cts;
    private bool _cerrarAlTerminar;
    private string? _rol;
    private bool _ocupado;

    public GenerarBackupForm(RespaldoNegocio negocio)
    {
        ArgumentNullException.ThrowIfNull(negocio);
        _negocio = negocio;

        Text = "Generar backup";
        Sizable = false;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(620, 330);

        _lblDescripcion = new MaterialLabel
        {
            Dock = DockStyle.Fill,
            AutoSize = false,
            Text = "Se generará un respaldo completo de la base de datos del sistema en la carpeta backup_restore. "
                + "El archivo se verifica al finalizar y nunca se sobrescribe un respaldo existente."
        };
        _lblMensaje = new MaterialLabel { Dock = DockStyle.Fill, AutoSize = false, Text = string.Empty };
        _progreso = new MaterialProgressBar { Dock = DockStyle.Fill, Style = ProgressBarStyle.Marquee, Visible = false };
        _btnGenerar = new MaterialButton { Text = "Generar", Type = MaterialButton.MaterialButtonType.Contained, AutoSize = true };
        _btnCancelar = new MaterialButton { Text = "Cancelar", Type = MaterialButton.MaterialButtonType.Outlined, AutoSize = true };

        var botones = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false };
        botones.Controls.Add(_btnCancelar);
        botones.Controls.Add(_btnGenerar);

        var tabla = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(12) };
        tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
        tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 14F));
        tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
        tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
        tabla.Controls.Add(_lblDescripcion, 0, 0);
        tabla.Controls.Add(_progreso, 0, 1);
        tabla.Controls.Add(_lblMensaje, 0, 2);
        tabla.Controls.Add(botones, 0, 3);
        Controls.Add(tabla);

        _btnGenerar.Click += async (_, _) => await GenerarAsync();
        _btnCancelar.Click += (_, _) => SolicitarCancelacion();
        FormClosing += (_, e) =>
        {
            if (_ocupado)
            {
                // X = Cancelar: se pide la cancelación y se cierra cuando el comando termine realmente.
                e.Cancel = true;
                _cerrarAlTerminar = true;
                SolicitarCancelacion();
            }
        };

        MaterialSkinManager.Instance.AddFormToManage(this);
    }

    /// <summary>Establece el rol de la sesión con el que el negocio evalúa el permiso.</summary>
    public void AplicarSesion(string? rol) => _rol = rol;

    /// <summary>Escape equivale a Cancelar.</summary>
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape)
        {
            SolicitarCancelacion();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void SolicitarCancelacion()
    {
        if (!_ocupado)
        {
            Close();
            return;
        }

        _btnCancelar.Enabled = false;
        _lblMensaje.Text = "Cancelando... esperando a que SQL Server detenga el comando.";
        _cts?.Cancel();
    }

    private async Task GenerarAsync()
    {
        if (_ocupado)
        {
            return;
        }

        EstablecerOcupado(true);
        _lblMensaje.Text = "Generando y verificando el backup...";
        _cts = new CancellationTokenSource();

        try
        {
            RespuestaGeneracionRespaldo r = await _negocio.GenerarBackupAsync(_rol, _cts.Token);
            if (IsDisposed)
            {
                return;
            }

            _lblMensaje.Text = r.Resultado == ResultadoRespaldo.Exito && r.RutaArchivo is not null
                ? $"{r.Mensaje}{Environment.NewLine}{r.RutaArchivo}"
                : r.Resultado == ResultadoRespaldo.Cancelado
                    ? "Generación de backup cancelada. No se generó ningún respaldo válido."
                    : r.Mensaje;
        }
        catch (Exception)
        {
            if (!IsDisposed)
            {
                _lblMensaje.Text = "No se pudo generar el backup por un error inesperado.";
            }
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
            if (!IsDisposed)
            {
                EstablecerOcupado(false);
                if (_cerrarAlTerminar)
                {
                    Close();
                }
            }
        }
    }

    private void EstablecerOcupado(bool ocupado)
    {
        _ocupado = ocupado;
        _progreso.Visible = ocupado;
        _btnGenerar.Enabled = !ocupado;
        _btnCancelar.Enabled = true;
    }
}

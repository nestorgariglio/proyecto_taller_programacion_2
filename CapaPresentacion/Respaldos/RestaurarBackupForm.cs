using CapaNegocio;
using CapaNegocio.DTOs.Respaldos;
using CapaNegocio.Enums;
using MaterialSkin;
using MaterialSkin.Controls;

namespace CapaPresentacion.Respaldos;

/// <summary>
/// Formulario modal para la selección y restauración de respaldos de la base de datos.
/// </summary>
public sealed class RestaurarBackupForm : MaterialForm
{
    private readonly RespaldoNegocio _negocio;
    private readonly MaterialListView _lista;
    private readonly MaterialLabel _lblEstado;
    private readonly MaterialProgressBar _progreso;
    private readonly MaterialButton _btnRestaurar;
    private readonly MaterialButton _btnCancelar;
    private List<RespaldoDisponibleDto> _respaldos = new();
    private CancellationTokenSource? _cts;
    private string? _rol;
    private bool _ocupado;
    private bool _faseIrreversible;
    private bool _cerrarAlTerminar;

    private const string MensajeEnCurso =
        "La restauración está en curso y no se puede cancelar de forma segura. Espere a que finalice.";

    public RestaurarBackupForm(RespaldoNegocio negocio)
    {
        ArgumentNullException.ThrowIfNull(negocio);
        _negocio = negocio;

        Text = "Restaurar backup";
        Sizable = false;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(820, 480);

        _lista = new MaterialListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            MultiSelect = false,
            HideSelection = false,
            MinimumSize = new Size(0, 200)
        };
        _lista.Columns.Add("Fecha y hora", 170);
        _lista.Columns.Add("Archivo", 360);
        _lista.Columns.Add("Tipo", 100);
        _lista.Columns.Add("Tamaño", 100);

        _lblEstado = new MaterialLabel { Dock = DockStyle.Fill, AutoSize = false, Text = string.Empty };
        _progreso = new MaterialProgressBar { Dock = DockStyle.Fill, Style = ProgressBarStyle.Marquee, Visible = false };
        _btnRestaurar = new MaterialButton { Text = "Restaurar", Type = MaterialButton.MaterialButtonType.Contained, AutoSize = true, Enabled = false };
        _btnCancelar = new MaterialButton { Text = "Cancelar", Type = MaterialButton.MaterialButtonType.Outlined, AutoSize = true };

        var botones = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false };
        botones.Controls.Add(_btnCancelar);
        botones.Controls.Add(_btnRestaurar);

        var tabla = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(12) };
        tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 14F));
        tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
        tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
        tabla.Controls.Add(_lista, 0, 0);
        tabla.Controls.Add(_progreso, 0, 1);
        tabla.Controls.Add(_lblEstado, 0, 2);
        tabla.Controls.Add(botones, 0, 3);
        Controls.Add(tabla);

        _lista.SelectedIndexChanged += (_, _) => ActualizarBotones();
        _btnRestaurar.Click += async (_, _) => await RestaurarAsync();
        _btnCancelar.Click += (_, _) => SolicitarCancelacion();
        FormClosing += (_, e) =>
        {
            if (_ocupado)
            {
                // X = Cancelar. Si la fase destructiva comenzó, no se puede cerrar.
                e.Cancel = true;
                _cerrarAlTerminar = !_faseIrreversible;
                SolicitarCancelacion();
            }
        };
        Shown += async (_, _) => await CargarAsync();

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

        if (_faseIrreversible)
        {
            _lblEstado.Text = MensajeEnCurso;
            return;
        }

        _btnCancelar.Enabled = false;
        _lblEstado.Text = "Cancelando... esperando a que finalice el comando en curso.";
        _cts?.Cancel();
    }

    /// <summary>Invocado por el negocio justo antes de SINGLE_USER: desde aquí no se puede cancelar.</summary>
    private void MarcarFaseIrreversible()
    {
        if (InvokeRequired)
        {
            Invoke(MarcarFaseIrreversible);
            return;
        }

        _faseIrreversible = true;
        _btnCancelar.Enabled = false;
        _lblEstado.Text = MensajeEnCurso;
    }

    private async Task CargarAsync()
    {
        EstablecerOcupado(true);
        _lista.Items.Clear();
        _lblEstado.Text = "Leyendo respaldos...";
        _cts = new CancellationTokenSource();
        var avance = new Progress<string>(t => { if (!IsDisposed) _lblEstado.Text = t; });

        try
        {
            RespuestaListadoRespaldos r = await _negocio.ListarRespaldosAsync(_rol, avance, _cts.Token);
            if (IsDisposed)
            {
                return;
            }

            _respaldos = r.Respaldos.ToList();
            foreach (RespaldoDisponibleDto b in _respaldos)
            {
                var item = new ListViewItem(b.FechaBackup.ToString("dd/MM/yyyy HH:mm:ss"));
                item.SubItems.Add(b.NombreArchivo);
                item.SubItems.Add(b.Tipo);
                item.SubItems.Add(b.TamanoLegible);
                _lista.Items.Add(item);
            }

            // Sin preselección: el usuario debe elegir explícitamente.
            _lista.SelectedItems.Clear();
            _lblEstado.Text = r.Mensaje;
        }
        catch (Exception)
        {
            if (!IsDisposed)
            {
                _lblEstado.Text = "No se pudo leer la lista de respaldos por un error inesperado.";
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

    private async Task RestaurarAsync()
    {
        if (_ocupado || _lista.SelectedIndices.Count != 1)
        {
            return;
        }

        RespaldoDisponibleDto elegido = _respaldos[_lista.SelectedIndices[0]];
        EstablecerOcupado(true);
        _cts = new CancellationTokenSource();
        bool reiniciar = false;
        bool recargar = false;
        _faseIrreversible = false;

        try
        {
            // (1) Validación sin lock.
            _lblEstado.Text = "Validando el respaldo seleccionado...";
            ValidacionRespaldoDto validacion = await _negocio.ValidarParaRestauracionAsync(_rol, elegido.RutaArchivo, _cts.Token);
            if (validacion.Resultado != ResultadoRespaldo.Exito || validacion.Respaldo is null)
            {
                _lblEstado.Text = validacion.Mensaje;
                return;
            }

            // (2) Confirmación; no se mantiene ningún semáforo mientras el diálogo está abierto.
            var dialogo = new MaterialDialog(
                this, "Confirmar restauración",
                RespaldoNegocio.ConstruirTextoConfirmacion(validacion.Respaldo),
                "RESTAURAR", true, "CANCELAR");
            if (dialogo.ShowDialog(this) != DialogResult.OK)
            {
                _lblEstado.Text = "Restauración cancelada. No se realizó ningún cambio.";
                return;
            }

            // (3) Ejecución.
            var avance = new Progress<string>(t => { if (!IsDisposed) _lblEstado.Text = t; });
            RespuestaRestauracion r = await _negocio.EjecutarRestauracionAsync(
                _rol, validacion, avance, _cts.Token, MarcarFaseIrreversible);
            if (IsDisposed)
            {
                return;
            }

            _lblEstado.Text = r.Mensaje;
            recargar = r.Resultado != ResultadoRespaldo.Exito && r.RutaBackupSeguridad is not null;
            reiniciar = r.Resultado == ResultadoRespaldo.Exito;
            if (reiniciar)
            {
                MaterialMessageBox.Show(this,
                    r.Mensaje + Environment.NewLine + "La aplicación se reiniciará para cargar la base restaurada.",
                    "Restauración completada");
            }
        }
        catch (OperationCanceledException)
        {
            if (!IsDisposed)
            {
                _lblEstado.Text = "Operación cancelada. No se realizó ningún cambio.";
            }
        }
        catch (Exception)
        {
            if (!IsDisposed)
            {
                _lblEstado.Text = "No se pudo completar la restauración por un error inesperado.";
            }
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
            _faseIrreversible = false;
            if (!reiniciar && !IsDisposed)
            {
                EstablecerOcupado(false);
            }
            else
            {
                _ocupado = false;
            }
        }

        if (reiniciar)
        {
            Application.Restart();
            return;
        }

        if (!IsDisposed)
        {
            if (_cerrarAlTerminar)
            {
                Close();
                return;
            }

            if (recargar)
            {
                // Se generó un respaldo válido (p. ej. el automático): se actualiza el listado.
                string mensaje = _lblEstado.Text;
                await CargarAsync();
                _lblEstado.Text = mensaje;
            }
        }
    }

    private void EstablecerOcupado(bool ocupado)
    {
        _ocupado = ocupado;
        _progreso.Visible = ocupado;
        _lista.Enabled = !ocupado;
        _btnCancelar.Enabled = true;
        ActualizarBotones();
    }

    private void ActualizarBotones()
        => _btnRestaurar.Enabled = !_ocupado && _lista.SelectedIndices.Count == 1;
}

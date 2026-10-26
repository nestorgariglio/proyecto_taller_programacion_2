using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CapaNegocio;
using CapaNegocio.DTOs.Categorias;
using CapaNegocio.DTOs.Productos;
using CapaNegocio.Enums;
using MaterialSkin;
using MaterialSkin.Controls;
using CapaPresentacion.Tema;

namespace CapaPresentacion.Productos
{
    public partial class ProductosForm : UserControl
    {
        private readonly CategoriaNegocio _categoriaNegocio;
        private readonly ProductoNegocio _productoNegocio;
        private MaterialTextBox2 _txtBuscar = null!;
        private MaterialComboBox _cboCategoria = null!;
        private MaterialComboBox _cboEstado = null!;
        private DataGridView _dgvProductos = null!;
        private MaterialButton _btnGestionCategorias = null!;
        private MaterialButton _btnNuevo = null!;
        private MaterialButton _btnEditar = null!;
        private MaterialButton _btnEstado = null!;
        private MaterialButton _btnAjustarStock = null!;
        private MaterialButton _btnActualizar = null!;
        private bool _cargandoCategorias;
        private bool _cargandoListado;
        private bool _recargaPendiente;
        private bool _actualizandoFiltros;
        private string? _rolActual;
        private int _idUsuarioActual;
        private Panel _panelPrincipal = null!;
        private TableLayoutPanel _panelRaiz = null!;
        private TableLayoutPanel _panelFiltros = null!;
        private FlowLayoutPanel _panelAcciones = null!;

        /// <summary>
        /// Inicializa la vista de productos.
        /// </summary>
        /// <param name="categoriaNegocio">Servicio que lista las categorías activas del combo (RF-11).</param>
        /// <param name="productoNegocio">Servicio que ejecuta las operaciones del ABM de productos (RF-1/RF-9).</param>
        public ProductosForm(CategoriaNegocio categoriaNegocio, ProductoNegocio productoNegocio)
        {
            ArgumentNullException.ThrowIfNull(categoriaNegocio);
            ArgumentNullException.ThrowIfNull(productoNegocio);

            _categoriaNegocio = categoriaNegocio;
            _productoNegocio = productoNegocio;
            InitializeComponent();
            BackColor = TemaAplicacion.FondoPrincipal;
            ArmarInterfazMaterial();

            this.Load += (s, e) =>
            {
                if (_panelRaiz != null)
                {
                    _panelRaiz.PerformLayout();
                }
            };

            _dgvProductos.DataBindingComplete += dgvProductos_DataBindingComplete;
            ActualizarEstadoBotones();
        }

        /// <summary>
        /// Opción de los combos de filtro: conserva la relación por Id (nunca por
        /// descripción) y el texto que se muestra al usuario (RF-15/RF-17).
        /// </summary>
        private sealed class OpcionFiltro
        {
            public OpcionFiltro(int? id, string descripcion)
            {
                Id = id;
                Descripcion = descripcion;
            }

            /// <summary>Identificador del filtro; <c>null</c> representa "Todos/Todas".</summary>
            public int? Id { get; }

            /// <summary>Texto visible de la opción.</summary>
            public string Descripcion { get; }

            public override string ToString() => Descripcion;
        }

        /// <summary>
        /// Carga en el combo de categorías únicamente las categorías activas más
        /// la opción "Todas" (RF-15) y, a continuación, recarga el listado real de
        /// productos (RF-14). Se invoca al entrar en la pestaña de productos para
        /// reflejar los cambios del ABM de categorías y de productos sin reiniciar
        /// la aplicación (RNF-3).
        /// </summary>
        /// <returns>Tarea que representa la carga del combo y del listado.</returns>
        public async Task CargarCategoriasAsync()
        {
            if (_cargandoCategorias)
            {
                return;
            }

            _cargandoCategorias = true;

            try
            {
                IReadOnlyList<CategoriaRespuestaDto> categoriasActivas =
                    await _categoriaNegocio.ListarCategoriasActivasAsync();

                // Se conserva la categoría elegida al refrescar (por Id, no por texto).
                int? idCategoriaSeleccionada = ObtenerCategoriaSeleccionada()?.Id;

                _actualizandoFiltros = true;

                try
                {
                    _cboCategoria.Items.Clear();

                    // La opción "Todas" siempre queda presente, aun sin categorías
                    // activas: el combo no debe quedar vacío ni fallar (caso límite).
                    _cboCategoria.Items.Add(new OpcionFiltro(null, "Todas"));

                    foreach (CategoriaRespuestaDto categoria in categoriasActivas)
                    {
                        if (!string.IsNullOrWhiteSpace(categoria.Descripcion))
                        {
                            _cboCategoria.Items.Add(
                                new OpcionFiltro(categoria.IdCategoria, categoria.Descripcion));
                        }
                    }

                    _cboCategoria.SelectedIndex = BuscarIndiceCategoria(idCategoriaSeleccionada);
                }
                finally
                {
                    _actualizandoFiltros = false;
                }
            }
            catch (Exception exception)
            {
                // Caso límite: fallo o desconexión de la BD ⇒ se conserva al
                // menos la opción "Todas" y se informa el detalle en español (RF-16).
                if (_cboCategoria.Items.Count == 0)
                {
                    _actualizandoFiltros = true;
                    _cboCategoria.Items.Add(new OpcionFiltro(null, "Todas"));
                    _cboCategoria.SelectedIndex = 0;
                    _actualizandoFiltros = false;
                }

                MaterialMessageBox.Show(
                    this,
                    $"No se pudieron cargar las categorías del combo.\nDetalle: {exception.Message}",
                    "Error SQL");

                return;
            }
            finally
            {
                _cargandoCategorias = false;
            }

            // RF-14/RF-15: al entrar en la pestaña también se refresca la grilla.
            await CargarProductosAsync();
        }

        /// <summary>
        /// Carga el listado real de productos —activos e inactivos— armando el
        /// <see cref="FiltroProductosDto"/> con los filtros elegidos en pantalla:
        /// búsqueda por Código y/o Nombre, categoría por Id y estado (RF-14/RF-15).
        /// Las reglas de filtrado viven en <see cref="ProductoNegocio"/>: aquí solo
        /// se arma el filtro y se bindea el resultado.
        /// </summary>
        /// <returns>Tarea que representa la carga del listado.</returns>
        private async Task CargarProductosAsync()
        {
            if (_cargandoListado)
            {
                // Si cambian los filtros mientras hay una consulta en curso, se
                // vuelve a cargar al terminar para no mostrar un listado obsoleto.
                _recargaPendiente = true;
                return;
            }

            _cargandoListado = true;

            try
            {
                var filtro = new FiltroProductosDto
                {
                    Texto = _txtBuscar.Text,
                    IdCategoria = ObtenerCategoriaSeleccionada()?.Id,
                    Estado = ObtenerEstadoSeleccionado()
                };

                List<ProductoRespuestaDto> productos =
                    await _productoNegocio.ListarProductosAsync(filtro);

                _dgvProductos.DataSource = productos;
                if (productos.Count > 0)
                {
                    _dgvProductos.ClearSelection();
                    _dgvProductos.Rows[0].Selected = true;
                }
            }
            catch (Exception exception)
            {
                // RF-16: un fallo de BD se informa en español sin perder los
                // filtros ingresados ni el listado que ya estaba en pantalla.
                MaterialMessageBox.Show(
                    this,
                    $"No se pudo cargar el listado de productos.\nDetalle: {exception.Message}",
                    "Error SQL");
            }
            finally
            {
                _cargandoListado = false;

                if (_recargaPendiente)
                {
                    _recargaPendiente = false;
                    await CargarProductosAsync();
                }
            }
        }

        /// <summary>
        /// Devuelve la opción de categoría seleccionada en el filtro.
        /// </summary>
        /// <returns>Opción seleccionada, o <c>null</c> cuando no hay selección.</returns>
        private OpcionFiltro? ObtenerCategoriaSeleccionada()
        {
            return _cboCategoria.SelectedItem as OpcionFiltro;
        }

        /// <summary>
        /// Devuelve el estado seleccionado en el filtro: <c>true</c> = activos,
        /// <c>false</c> = inactivos y <c>null</c> = todos (RF-14).
        /// </summary>
        /// <returns>Estado por el que filtrar, o <c>null</c> para todos.</returns>
        private bool? ObtenerEstadoSeleccionado()
        {
            return _cboEstado.SelectedIndex switch
            {
                1 => true,
                2 => false,
                _ => null
            };
        }

        /// <summary>
        /// Busca el índice de la categoría indicada dentro del combo.
        /// </summary>
        /// <param name="idCategoria">Identificador a buscar; <c>null</c> = "Todas".</param>
        /// <returns>Índice encontrado; 0 ("Todas") cuando no está presente.</returns>
        private int BuscarIndiceCategoria(int? idCategoria)
        {
            for (int indice = 0; indice < _cboCategoria.Items.Count; indice++)
            {
                if (_cboCategoria.Items[indice] is OpcionFiltro opcion
                    && opcion.Id == idCategoria)
                {
                    return indice;
                }
            }

            return 0;
        }

        /// <summary>
        /// Aplica el gating por rol a las acciones de escritura del ABM.
        /// Solo Administrador y Encargado pueden gestionar productos (RF-11/RF-18/D-18-bis):
        /// "Nuevo", "Editar", "Desactivar", "Reactivar", "Ajustar stock" y
        /// "Gestionar categorías" quedan ocultos para cualquier otro rol. El
        /// Vendedor no ve la pestaña "Productos" (D-18-bis).
        /// </summary>
        /// <param name="rol">Descripción del rol del usuario (puede ser null o vacío).</param>
        /// <param name="idUsuario">Usuario logueado, para enviar la auditoría a Negocio (RF-20).</param>
        /// <remarks>
        /// Lo invoca <c>inicio.AplicarPermisosPorRol()</c> con el rol de la sesión
        /// ya resuelto, evitando cambiar el ctor (y el registro de DI) de la vista.
        /// "Actualizar" no se oculta: es una acción de lectura que solo refresca el
        /// listado (RF-14/RF-15) y no escribe datos.
        /// </remarks>
        public void AplicarRolUsuario(string? rol, int idUsuario = 0)
        {
            _rolActual = rol;
            _idUsuarioActual = idUsuario;

            bool puedeGestionarProductos = PoliticaAcceso.PuedeGestionarProductos(rol);
            _btnNuevo.Visible = puedeGestionarProductos;
            _btnEditar.Visible = puedeGestionarProductos;
            _btnEstado.Visible = puedeGestionarProductos;
            _btnAjustarStock.Visible = puedeGestionarProductos;
            _btnGestionCategorias.Visible = PoliticaAcceso.TieneAcceso(rol, "Categorías");

            ActualizarEstadoBotones();
        }

        private async void txtBuscar_TextChanged(object? sender, EventArgs e)
        {
            if (_actualizandoFiltros)
            {
                return;
            }

            await CargarProductosAsync();
        }

        private async void cboCategoria_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_actualizandoFiltros)
            {
                return;
            }

            await CargarProductosAsync();
        }

        private async void cboEstado_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_actualizandoFiltros)
            {
                return;
            }

            await CargarProductosAsync();
        }

        private void dgvProductos_SelectionChanged(object? sender, EventArgs e)
        {
            ActualizarEstadoBotones();
        }

        private void dgvProductos_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            ActualizarEstadoBotones();
        }

        /// <summary>
        /// Muestra el estado del producto en español ("Activo"/"Inactivo") en la
        /// columna de la grilla (RF-14).
        /// </summary>
        private void dgvProductos_CellFormatting(
            object? sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0
                || sender is not DataGridView grilla
                || e.ColumnIndex >= grilla.Columns.Count)
            {
                return;
            }

            if (grilla.Columns[e.ColumnIndex].Name == "colEstado" && e.Value is bool estado)
            {
                e.Value = estado ? "Activo" : "Inactivo";
                e.FormattingApplied = true;
            }
        }

        private async void btnActualizar_Click(object? sender, EventArgs e)
        {
            // RF-1/RF-14/RF-15: "Actualizar" solo recarga el listado desde Negocio
            // conservando los filtros elegidos. No es una operación de escritura,
            // por eso no lleva guard de permiso: no modifica datos y el Vendedor
            // no accede a esta pestaña (D-18-bis).
            await CargarProductosAsync();
        }

        private async void btnNuevo_Click(object? sender, EventArgs e)
        {
            // RF-11/RF-18: guard defensivo en tiempo de ejecución. Aunque el botón se
            // oculta por rol, si un rol no autorizado lograra invocar el alta (por
            // ejemplo, disparando el handler por código y no por clic), se rechaza aquí.
            if (!PoliticaAcceso.PuedeGestionarProductos(_rolActual))
            {
                MaterialMessageBox.Show(
                    this,
                    "Acceso no autorizado. Este módulo solo está disponible para Administrador o Encargado.",
                    "Acceso denegado");
                return;
            }

            using var modal = new ProductoModalForm(
                _productoNegocio, _categoriaNegocio, _idUsuarioActual);

            if (modal.ShowDialog(FindForm()) == DialogResult.OK)
            {
                // RF-14: el listado se recarga desde Negocio tras la operación.
                await CargarProductosAsync();
            }
        }

        private async void btnEditar_Click(object? sender, EventArgs e)
        {
            // RF-11/RF-18: guard defensivo en tiempo de ejecución (defensa en profundidad).
            if (!PoliticaAcceso.PuedeGestionarProductos(_rolActual))
            {
                MaterialMessageBox.Show(
                    this,
                    "Acceso no autorizado. Este módulo solo está disponible para Administrador o Encargado.",
                    "Acceso denegado");
                return;
            }

            if (_dgvProductos.CurrentRow?.DataBoundItem is not ProductoRespuestaDto producto)
            {
                MaterialMessageBox.Show(
                    this,
                    "Debe seleccionar un producto del listado para editarlo.",
                    "Aviso");
                return;
            }

            // D-20: el producto inactivo también se puede editar sin reactivarlo.
            using var modal = new ProductoModalForm(
                _productoNegocio, _categoriaNegocio, _idUsuarioActual, producto);

            if (modal.ShowDialog(FindForm()) == DialogResult.OK)
            {
                // RF-14: el listado se recarga desde Negocio tras la operación.
                await CargarProductosAsync();
            }
        }

        /// <summary>
        /// Cambia el estado del producto seleccionado con el único botón de estado
        /// (RF-5/RF-8): da de baja lógica el producto activo y reactiva el inactivo,
        /// pidiendo siempre confirmación en español antes de escribir.
        /// </summary>
        private async void btnEstado_Click(object? sender, EventArgs e)
        {
            // RF-11/RF-18: guard defensivo en tiempo de ejecución (defensa en profundidad).
            if (!PoliticaAcceso.PuedeGestionarProductos(_rolActual))
            {
                MaterialMessageBox.Show(
                    this,
                    "Acceso no autorizado. Este módulo solo está disponible para Administrador o Encargado.",
                    "Acceso denegado");
                return;
            }

            if (_dgvProductos.CurrentRow?.DataBoundItem is not ProductoRespuestaDto producto)
            {
                // Sin fila no hay estado del producto del que derivar la acción: se
                // conserva el mensaje original según el texto vigente del botón.
                string mensaje = _btnEstado.Text == "Reactivar"
                    ? "Debe seleccionar un producto del listado para reactivarlo."
                    : "Debe seleccionar un producto del listado para darlo de baja.";

                MaterialMessageBox.Show(this, mensaje, "Aviso");
                return;
            }

            if (FindForm() is not Form formularioPrincipal)
            {
                return;
            }

            // RF-6/RF-8: el estado del producto decide la acción y el texto del diálogo.
            bool estaActivo = producto.Estado;
            string tituloDialogo = estaActivo ? "Dar de baja producto" : "Reactivar producto";
            string preguntaDialogo = estaActivo
                ? $"¿Confirma que desea dar de baja el producto {producto.Nombre}?"
                : $"¿Confirma que desea reactivar el producto {producto.Nombre}?";

            // RF-12/D-13: confirmación simple en español antes de escribir.
            var dialogo = new MaterialDialog(
                formularioPrincipal,
                tituloDialogo,
                preguntaDialogo,
                "SÍ",
                true,
                "NO");

            // Cancelar ⇒ sin ninguna escritura: el producto queda sin cambios.
            if (dialogo.ShowDialog(formularioPrincipal) != DialogResult.OK)
            {
                return;
            }

            _btnEstado.Enabled = false;

            try
            {
                if (estaActivo)
                {
                    // La baja es lógica y la resuelve Negocio (RNF-3, sin DELETE físico).
                    RespuestaBajaProducto respuesta =
                        await _productoNegocio.DarBajaProductoAsync(
                            producto.IdProducto, _idUsuarioActual);

                    if (respuesta.Resultado != ResultadoBajaProducto.Exito)
                    {
                        // Casos límite (ya inactivo / sin permiso / no encontrado): se
                        // muestra el mensaje en español de Negocio, sin modificar nada.
                        MaterialMessageBox.Show(this, respuesta.Mensaje, "Atención");
                        return;
                    }

                    MaterialMessageBox.Show(this, respuesta.Mensaje, "Producto dado de baja");
                }
                else
                {
                    RespuestaReactivacionProducto respuesta =
                        await _productoNegocio.ReactivarProductoAsync(
                            producto.IdProducto, _idUsuarioActual);

                    if (respuesta.Resultado != ResultadoReactivacionProducto.Exito)
                    {
                        // D-12/RF-13: si la categoría está inactiva, Negocio bloquea la
                        // reactivación y aquí solo se muestra su mensaje en español.
                        MaterialMessageBox.Show(this, respuesta.Mensaje, "Atención");
                        return;
                    }

                    MaterialMessageBox.Show(this, respuesta.Mensaje, "Producto reactivado");
                }

                await CargarProductosAsync();
            }
            catch (Exception exception)
            {
                // RF-16: fallo de BD informado en español, conservando los filtros.
                MaterialMessageBox.Show(
                    this,
                    $"No se pudo cambiar el estado del producto.\nDetalle: {exception.Message}",
                    "Error SQL");
            }
            finally
            {
                ActualizarEstadoBotones();
            }
        }

        private async void btnAjustarStock_Click(object? sender, EventArgs e)
        {
            // RF-11/RF-18: guard defensivo en tiempo de ejecución (defensa en profundidad).
            if (!PoliticaAcceso.PuedeGestionarProductos(_rolActual))
            {
                MaterialMessageBox.Show(
                    this,
                    "Acceso no autorizado. Este módulo solo está disponible para Administrador o Encargado.",
                    "Acceso denegado");
                return;
            }

            if (_dgvProductos.CurrentRow?.DataBoundItem is not ProductoRespuestaDto producto)
            {
                MaterialMessageBox.Show(
                    this,
                    "Debe seleccionar un producto del listado para ajustar su stock.",
                    "Aviso");
                return;
            }

            // RF-10/D-14: el modal pide el stock absoluto y el motivo obligatorio;
            // las reglas (rango, motivo, permiso) las valida CapaNegocio.
            using var modal = new AjustarStockModalForm(
                _productoNegocio, producto, _idUsuarioActual);

            if (modal.ShowDialog(FindForm()) == DialogResult.OK)
            {
                // RF-10/RF-14: el stock de la grilla se recarga desde Negocio.
                await CargarProductosAsync();
            }
        }

        private async void btnGestionCategorias_Click(object? sender, EventArgs e)
        {
            // RF-12: guard defensivo en tiempo de ejecución. Aunque el botón se
            // oculta por rol, si un rol no autorizado lograra invocar el ABM (por
            // ejemplo, disparando el handler por código y no por clic), el sistema
            // lo rechaza aquí antes de abrir el modal.
            if (!PoliticaAcceso.TieneAcceso(_rolActual, "Categorías"))
            {
                MaterialMessageBox.Show(
                    this,
                    "Acceso no autorizado. Este módulo solo está disponible para Administrador o Encargado.",
                    "Acceso denegado");
                return;
            }

            // RF-10: abre el ABM de categorías en una ventana modal que reutiliza
            // GestionCategoriasControl sin duplicar lógica (reencuadre aprobado D-4).
            using (var modal = new GestionCategoriasModalForm(_categoriaNegocio, _rolActual))
            {
                // El modal no distingue si hubo operaciones ni fuerza DialogResult
                // (cerrarlo con la X devuelve Cancel), así que el combo se refresca
                // SIEMPRE al volver, sin condicionarlo al resultado (D-7 / RNF-3).
                modal.ShowDialog(this);
            }

            await CargarCategoriasAsync();
        }

        /// <summary>
        /// Habilita las acciones del ABM según la fila seleccionada y el rol
        /// (RF-7/RF-12): "Nuevo" queda disponible para quien puede gestionar
        /// productos; "Actualizar" es una acción de lectura y nunca se deshabilita
        /// por selección; "Editar" y "Ajustar stock" requieren una fila seleccionada
        /// (también inactivos, D-20); el único botón de estado muestra "Desactivar"
        /// sobre un producto activo y "Reactivar" sobre uno inactivo, y queda
        /// deshabilitado cuando no hay selección.
        /// </summary>
        private void ActualizarEstadoBotones()
        {
            ProductoRespuestaDto? producto =
                _dgvProductos.CurrentRow?.DataBoundItem as ProductoRespuestaDto;
            bool puedeGestionarProductos =
                PoliticaAcceso.PuedeGestionarProductos(_rolActual);

            // Defensa para CurrentRow = null (CF-1): si no hay fila seleccionada,
            // los botones de edición/acciones se deshabilitan sin lanzar excepciones.
            if (producto is null)
            {
                _btnEditar.Enabled = false;
                _btnEstado.Enabled = false;
                _btnAjustarStock.Enabled = false;
            }
            else
            {
                _btnEditar.Enabled = puedeGestionarProductos;
                _btnEstado.Enabled = puedeGestionarProductos;
                _btnEstado.Text = producto.Estado ? "Desactivar" : "Reactivar";
                _btnAjustarStock.Enabled = puedeGestionarProductos;
            }
            _btnNuevo.Enabled = puedeGestionarProductos;
        }

        private void ArmarInterfazMaterial()
        {
            Controls.Clear();

            // 1. Contenedor principal de la vista
            _panelPrincipal = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = TemaAplicacion.FondoPrincipal,
                Padding = new Padding(15)
            };

            // 2. Panel raíz con distribución por filas
            _panelRaiz = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = TemaAplicacion.FondoPrincipal,
                AutoSize = false
            };

            _panelRaiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _panelRaiz.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _panelRaiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            _panelRaiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            _panelRaiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // 3. Encabezado
            MaterialLabel lblTitulo = new MaterialLabel
            {
                Text = "CATÁLOGO Y GESTIÓN DE PRODUCTOS",
                FontType = MaterialSkinManager.fontType.H6,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Margin = new Padding(5, 5, 5, 8)
            };

            // 4. Panel de filtros con distribución por columnas
            _panelFiltros = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = TemaAplicacion.FondoPrincipal,
                Margin = new Padding(0, 0, 0, 4),
                AutoSize = false,
                Height = 56
            };

            _panelFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            _panelFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            _panelFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            _panelFiltros.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // Controles MaterialSkin 2 (Buscador, Combos)
            _txtBuscar = new MaterialTextBox2
            {
                Hint = "Buscar por código o nombre...",
                Dock = DockStyle.Fill,
                Margin = new Padding(5, 4, 10, 4),
                AnimateReadOnly = false,
                Depth = 0
            };

            // El combo se puebla con las categorías activas reales en
            // CargarCategoriasAsync() (RF-11), guardando el Id de cada una.
            _cboCategoria = new MaterialComboBox
            {
                Hint = "Categoría",
                Dock = DockStyle.Fill,
                Margin = new Padding(5, 4, 10, 4),
                StartIndex = 0,
                Depth = 0
            };

            // RF-15: el filtro de estado deja de estar mockeado y ofrece
            // Todos / Activos / Inactivos, mapeados a <c>bool?</c>.
            _cboEstado = new MaterialComboBox
            {
                Hint = "Estado",
                Dock = DockStyle.Fill,
                Margin = new Padding(5, 4, 5, 4),
                StartIndex = 0,
                Depth = 0
            };
            _cboEstado.Items.AddRange(new object[] { "Todos", "Activos", "Inactivos" });
            _cboEstado.SelectedIndex = 0;

            _panelFiltros.Controls.Add(_txtBuscar, 0, 0);
            _panelFiltros.Controls.Add(_cboCategoria, 1, 0);
            _panelFiltros.Controls.Add(_cboEstado, 2, 0);

            // 5. Panel de acciones
            MaterialButton btnNuevo = new MaterialButton
            {
                Text = "Nuevo",
                Height = 36,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Type = MaterialButton.MaterialButtonType.Contained,
                HighEmphasis = true,
                UseAccentColor = false,
                Depth = 0,
                // Nace oculto: AplicarRolUsuario lo muestra solo a Administrador y
                // Encargado (RF-11/RF-18). Así ningún otro rol ve la acción.
                Visible = false,
                Margin = new Padding(4, 6, 4, 6)
            };
            _btnNuevo = btnNuevo;
            _btnNuevo.Click += btnNuevo_Click;

            _panelAcciones = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = false,
                Padding = new Padding(0),
                BackColor = TemaAplicacion.FondoPrincipal,
                Margin = new Padding(0)
            };

            MaterialButton btnEditar = new MaterialButton
            {
                Text = "Editar",
                Height = 36,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Type = MaterialButton.MaterialButtonType.Contained,
                HighEmphasis = true,
                UseAccentColor = false,
                Depth = 0,
                Visible = false,
                Margin = new Padding(4, 6, 4, 6)
            };
            _btnEditar = btnEditar;
            _btnEditar.Click += btnEditar_Click;

            // RF-5/RF-6: un único botón de estado que alterna "Desactivar" (producto
            // activo) y "Reactivar" (producto inactivo), con la misma protección
            // por rol que el resto de las acciones de escritura.
            MaterialButton btnEstado = new MaterialButton
            {
                Text = "Desactivar",
                Height = 36,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Type = MaterialButton.MaterialButtonType.Contained,
                HighEmphasis = true,
                UseAccentColor = false,
                Depth = 0,
                Visible = false,
                Margin = new Padding(4, 6, 4, 6)
            };
            _btnEstado = btnEstado;
            _btnEstado.Click += btnEstado_Click;

            // RF-10/D-14: ajuste manual de stock por valor absoluto y motivo
            // obligatorio, con la misma protección por rol que el resto de las
            // acciones de escritura.
            MaterialButton btnAjustarStock = new MaterialButton
            {
                Text = "Ajustar stock",
                Height = 36,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Type = MaterialButton.MaterialButtonType.Contained,
                HighEmphasis = true,
                UseAccentColor = false,
                Depth = 0,
                Visible = false,
                Margin = new Padding(4, 6, 4, 6)
            };
            _btnAjustarStock = btnAjustarStock;
            _btnAjustarStock.Click += btnAjustarStock_Click;

            // RF-1/RF-14: "Actualizar" refresca el listado real desde Negocio
            // conservando los filtros. Es una acción de lectura: no se oculta por
            // rol ni se deshabilita según la selección (el Vendedor no entra a la
            // pestaña, D-18-bis).
            MaterialButton btnActualizar = new MaterialButton
            {
                Text = "Actualizar",
                Height = 36,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Type = MaterialButton.MaterialButtonType.Contained,
                HighEmphasis = true,
                UseAccentColor = false,
                Depth = 0,
                Margin = new Padding(4, 6, 4, 6)
            };
            _btnActualizar = btnActualizar;
            _btnActualizar.Click += btnActualizar_Click;

            // Botón para abrir el ABM de categorías en ventana modal (RF-10).
            // Nace oculto: AplicarRolUsuario lo muestra solo a Administrador y
            // Encargado (RF-12). Así, aunque nunca se invoque el gating (rol
            // ausente), ningún otro rol ve la acción.
            _btnGestionCategorias = new MaterialButton
            {
                Text = "Gestionar categorías",
                Height = 36,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Type = MaterialButton.MaterialButtonType.Contained,
                HighEmphasis = true,
                UseAccentColor = false,
                Depth = 0,
                Visible = false,
                Margin = new Padding(4, 6, 4, 6)
            };
            _btnGestionCategorias.Click += btnGestionCategorias_Click;

            _panelAcciones.Controls.Add(btnNuevo);
            _panelAcciones.Controls.Add(btnEditar);
            _panelAcciones.Controls.Add(btnEstado);
            _panelAcciones.Controls.Add(btnAjustarStock);
            _panelAcciones.Controls.Add(btnActualizar);
            _panelAcciones.Controls.Add(_btnGestionCategorias);

            // Configurar _panelAcciones directamente en el TableLayoutPanel (patrón GestionCategoriasControl)
            _panelAcciones.Dock = DockStyle.Fill;
            _panelAcciones.FlowDirection = FlowDirection.LeftToRight;
            _panelAcciones.WrapContents = false;
            _panelAcciones.AutoScroll = false;
            _panelAcciones.Padding = new Padding(0);
            _panelAcciones.BackColor = TemaAplicacion.FondoPrincipal;
            _panelAcciones.Margin = new Padding(0, 4, 0, 4);

            // 6. DataGridView Estilizado para Dark Mode de MaterialSkin
            DataGridView dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.FromArgb(48, 48, 48),
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                EnableHeadersVisualStyles = false,
                Margin = new Padding(5, 4, 5, 5)
            };

            // Estilos oscuros del DataGridView para integrarse con Material
            dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = Color.FromArgb(55, 71, 79),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                SelectionBackColor = Color.FromArgb(55, 71, 79),
                SelectionForeColor = Color.White,
                WrapMode = DataGridViewTriState.True
            };
            dgv.ColumnHeadersHeight = 42;

            dgv.DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = Color.FromArgb(48, 48, 48),
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.White,
                SelectionBackColor = Color.FromArgb(255, 87, 34),
                SelectionForeColor = Color.White,
                WrapMode = DataGridViewTriState.False
            };
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.GridColor = Color.FromArgb(80, 80, 80);
            dgv.RowTemplate.Height = 36;

            // RF-14: columnas del listado real de Negocio (ProductoRespuestaDto).
            dgv.Columns.AddRange(new DataGridViewColumn[]
            {
                CrearColumna("colCodigo", "Codigo", "Código", 90),
                CrearColumna("colNombre", "Nombre", "Nombre", 210),
                CrearColumna("colCategoria", "NombreCategoria", "Categoría", 130),
                CrearColumna("colPrecioCompra", "PrecioCompra", "P. Compra", 90),
                CrearColumna("colPrecioVenta", "PrecioVenta", "P. Venta", 90),
                CrearColumna("colStock", "Stock", "Stock", 70),
                CrearColumna("colEstado", "Estado", "Estado", 70)
            });

            // Los precios se muestran con dos decimales y alineados a la derecha.
            dgv.Columns["colPrecioCompra"]!.DefaultCellStyle.Format = "N2";
            dgv.Columns["colPrecioCompra"]!.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleRight;
            dgv.Columns["colPrecioVenta"]!.DefaultCellStyle.Format = "N2";
            dgv.Columns["colPrecioVenta"]!.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleRight;
            dgv.Columns["colStock"]!.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleRight;

            // El estado de negocio viaja como bool y se muestra como "Activo"/"Inactivo".
            dgv.CellFormatting += dgvProductos_CellFormatting;
            dgv.SelectionChanged += dgvProductos_SelectionChanged;

            _dgvProductos = dgv;

            // Ensamblar la vista
            _panelRaiz.Controls.Add(lblTitulo, 0, 0);
            _panelRaiz.Controls.Add(_panelFiltros, 0, 1);
            _panelRaiz.Controls.Add(_panelAcciones, 0, 2);
            _panelRaiz.Controls.Add(dgv, 0, 3);

            _panelPrincipal.Controls.Add(_panelRaiz);
            Controls.Add(_panelPrincipal);

            // La grilla se completa con el listado real (CargarProductosAsync) al
            // entrar en la pestaña y con cada cambio de filtro (RF-14/RF-15).
            // Los handlers se suscriben al final para no disparar consultas
            // durante la construcción de la vista.
            _txtBuscar.TextChanged += txtBuscar_TextChanged;
            _cboCategoria.SelectedIndexChanged += cboCategoria_SelectedIndexChanged;
            _cboEstado.SelectedIndexChanged += cboEstado_SelectedIndexChanged;

            // El combo de categorías arranca con "Todas" hasta que Negocio
            // devuelva las categorías activas (CargarCategoriasAsync).
            _actualizandoFiltros = true;
            _cboCategoria.Items.Add(new OpcionFiltro(null, "Todas"));
            _cboCategoria.SelectedIndex = 0;
            _actualizandoFiltros = false;

            ActualizarEstadoBotones();
        }

        /// <summary>
        /// Crea una columna de texto de la grilla de productos, enlazada a la
        /// propiedad del <see cref="ProductoRespuestaDto"/> indicada.
        /// </summary>
        private static DataGridViewTextBoxColumn CrearColumna(
            string nombre, string propiedad, string encabezado, int peso)
        {
            return new DataGridViewTextBoxColumn
            {
                Name = nombre,
                DataPropertyName = propiedad,
                HeaderText = encabezado,
                FillWeight = peso,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
        }
    }
}

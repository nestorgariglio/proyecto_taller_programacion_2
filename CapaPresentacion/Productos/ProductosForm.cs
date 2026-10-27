using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CapaNegocio;
using CapaNegocio.DTOs.Categorias;
using CapaNegocio.DTOs.Productos;
using CapaNegocio.Enums;
using CapaNegocio.Stock;
using MaterialSkin;
using MaterialSkin.Controls;
using Microsoft.Extensions.Configuration;
using CapaPresentacion.Tema;

namespace CapaPresentacion.Productos
{
    public partial class ProductosForm : UserControl
    {
        // Paleta de criticidad de la grilla (D-17). Son los cuatro colores
        // aprobados sobre el fondo oscuro; el texto de la fila va siempre en blanco.
        private static readonly Color FondoAgotado = Color.FromArgb(0xB7, 0x1C, 0x1C);           // #B71C1C
        private static readonly Color FondoAgotadoInactivo = Color.FromArgb(0x6E, 0x20, 0x20);   // #6E2020
        private static readonly Color FondoBajoUmbral = Color.FromArgb(0xF9, 0xA8, 0x25);        // #F9A825
        private static readonly Color FondoBajoUmbralInactivo = Color.FromArgb(0x8C, 0x6D, 0x0A); // #8C6D0A

        // Textos de la grilla vacía (RF-13/D-16). El de error es distinto a propósito:
        // un fallo de base de datos no debe leerse como "no hay productos".
        private const string MensajeSinProductos = "No hay productos registrados.";
        private const string MensajeSinCoincidencias =
            "Ningún producto coincide con los filtros aplicados.";
        private const string MensajeErrorCarga = "No se pudo cargar el listado de productos.";

        private readonly CategoriaNegocio _categoriaNegocio;
        private readonly ProductoNegocio _productoNegocio;
        private readonly IConfiguration _configuracion;
        private readonly ResolucionUmbralStock _resolucionUmbral;
        private MaterialTextBox2 _txtBuscar = null!;
        private MaterialComboBox _cboCategoria = null!;
        private MaterialComboBox _cboEstado = null!;
        private MaterialCheckbox _chkSoloCriticos = null!;
        private Label _lblUmbral = null!;
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

        // Mensaje que la grilla dibuja sobre su área de filas cuando no hay filas
        // (D-16); <c>null</c> cuando hay resultados o no corresponde avisar.
        private string? _mensajeVacio;

        /// <summary>
        /// Inicializa la vista de productos.
        /// </summary>
        /// <param name="categoriaNegocio">Servicio que lista las categorías activas del combo (RF-11).</param>
        /// <param name="productoNegocio">Servicio que ejecuta las operaciones del ABM de productos (RF-1/RF-9).</param>
        /// <param name="configuracion">
        /// Configuración de la aplicación, de donde se lee una única vez el umbral
        /// mínimo de stock (RF-9/RF-10, D-15). La interpretación del valor queda en
        /// <see cref="ResolucionUmbralStock"/>: la vista no parsea el texto.
        /// </param>
        public ProductosForm(
            CategoriaNegocio categoriaNegocio,
            ProductoNegocio productoNegocio,
            IConfiguration configuracion)
        {
            ArgumentNullException.ThrowIfNull(categoriaNegocio);
            ArgumentNullException.ThrowIfNull(productoNegocio);
            ArgumentNullException.ThrowIfNull(configuracion);

            _categoriaNegocio = categoriaNegocio;
            _productoNegocio = productoNegocio;
            _configuracion = configuracion;

            // Lectura única del parámetro (D-15/RNF-9): el resultado tipado se
            // guarda en el campo y de él salen tanto el texto como el valor que
            // se pasará a la consulta.
            _resolucionUmbral = ResolucionUmbralStock.Resolver(
                _configuracion["StockCritico:UmbralMinimo"]);

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
        /// la aplicación (RNF-3). Si la categoría que estaba elegida dejó de estar
        /// activa, el filtro vuelve a "Todas" y se informa una única vez por este
        /// evento de refresco (RF-24), conservando texto, estado y "Solo críticos".
        /// </summary>
        /// <returns>Tarea que representa la carga del combo y del listado.</returns>
        public async Task CargarCategoriasAsync()
        {
            if (_cargandoCategorias)
            {
                return;
            }

            _cargandoCategorias = true;
            bool categoriaDesaparecida = false;

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

                    // RF-24: si había una categoría elegida y ya no está entre las
                    // activas, BuscarIndiceCategoria devuelve "Todas" (índice 0).
                    int indiceCategoria = BuscarIndiceCategoria(idCategoriaSeleccionada);
                    categoriaDesaparecida = idCategoriaSeleccionada.HasValue
                        && indiceCategoria == 0;

                    _cboCategoria.SelectedIndex = indiceCategoria;
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

                // RF-16: el fallo deja el texto de error para que la grilla vacía
                // lo dibuje en su Paint (primera carga), sin confundirlo con el
                // mensaje de "no hay productos". Un único modal de error.
                _mensajeVacio = MensajeErrorCarga;

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

            // RF-24: el aviso va una sola vez por este evento de refresco, después
            // de recargar el listado y sin tocar los demás filtros. Vive aquí y no
            // en TextChanged ni en cada recarga de la grilla. Es informativo y
            // modal a propósito: no forma parte del aviso persistente del umbral
            // degradado que RF-23 prohíbe mostrar como modal.
            if (categoriaDesaparecida)
            {
                MaterialMessageBox.Show(
                    this,
                    "La categoría seleccionada ya no está disponible."
                        + "\nSe muestran todos los productos.",
                    "Categoría no disponible");
            }
        }

        /// <summary>
        /// Carga el listado real de productos —activos e inactivos— armando el
        /// <see cref="FiltroProductosDto"/> con los filtros elegidos en pantalla:
        /// búsqueda por Código y/o Nombre, categoría por Id, estado (RF-14/RF-15) y
        /// "Solo críticos" (RF-6/RF-11). Las reglas de filtrado y de criticidad
        /// viven en <see cref="ProductoNegocio"/>: aquí solo se arma el filtro, se
        /// pasa el umbral vigente y se bindea el resultado. Si el listado vuelve
        /// vacío guarda el mensaje que la grilla dibujará (RF-13) y, ante un fallo
        /// de base de datos, conserva el listado y los filtros previos (RF-16).
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
                    Estado = ObtenerEstadoSeleccionado(),
                    // RF-6/RF-11: la casilla viaja dentro del filtro; null cuando
                    // está apagada, igual que el resto de los filtros opcionales.
                    SoloCriticos = _chkSoloCriticos.Checked ? true : (bool?)null
                };

                // El umbral se toma tal como lo resolvió el constructor (RF-9):
                // en modo degradado es null y el negocio solo marca los agotados
                // (RF-10). La vista no parsea ni recalcula nada.
                List<ProductoRespuestaDto> productos =
                    await _productoNegocio.ListarProductosAsync(
                        filtro, _resolucionUmbral.Valor);

                // RF-13/D-16: el texto de la grilla vacía se decide antes del bind
                // para que el repintado que dispara el DataSource ya lo encuentre.
                _mensajeVacio = await ObtenerMensajeVacioAsync(productos);

                _dgvProductos.DataSource = productos;

                // RF-21: la única coincidencia exacta ya viene en la posición 0; aquí
                // solo se decide qué fila queda activa tras el bind.
                AplicarSeleccionTrasCarga(productos);
            }
            catch (Exception exception)
            {
                // RF-16: un fallo de BD se informa en español sin perder los
                // filtros ingresados ni el listado que ya estaba en pantalla. El
                // texto dibujado es propio del error, nunca el de "sin productos".
                _mensajeVacio = MensajeErrorCarga;

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
        /// Obtiene el mensaje de grilla vacía que corresponde al listado recién
        /// consultado (RF-13/D-16): sin filas y con catálogo vacío informa que no
        /// hay productos registrados; sin filas pero con productos cargados informa
        /// que ningún producto coincide con los filtros. La consulta de conteo se
        /// hace <b>una única vez</b> y solo cuando el listado volvió vacío (RNF-1).
        /// Si esa consulta falla, devuelve un texto genérico de error sin propagar
        /// la excepción ni tocar los filtros.
        /// </summary>
        /// <param name="productos">Listado recién obtenido de Negocio.</param>
        /// <returns>Mensaje a dibujar, o <c>null</c> cuando hay filas.</returns>
        private async Task<string?> ObtenerMensajeVacioAsync(List<ProductoRespuestaDto> productos)
        {
            if (productos.Count > 0)
            {
                return null;
            }

            try
            {
                int totalCatalogo = await _productoNegocio.ContarProductosAsync();
                return totalCatalogo == 0 ? MensajeSinProductos : MensajeSinCoincidencias;
            }
            catch (Exception)
            {
                // Un fallo del conteo no debe romper la vista ni borrar los filtros.
                return MensajeErrorCarga;
            }
        }

        /// <summary>
        /// Dibuja el mensaje de grilla vacía centrado sobre el área de filas, debajo
        /// de los encabezados (RF-13/D-16), sin agregar controles ni forzar repintados.
        /// No dibuja nada cuando hay filas, cuando no hay mensaje o cuando el área
        /// visible no deja espacio.
        /// </summary>
        private void dgvProductos_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not DataGridView grilla
                || string.IsNullOrEmpty(_mensajeVacio)
                || grilla.RowCount > 0)
            {
                return;
            }

            int alturaEncabezados = grilla.ColumnHeadersVisible
                ? grilla.ColumnHeadersHeight
                : 0;

            Rectangle areaFilas = new Rectangle(
                grilla.ClientRectangle.Left,
                grilla.ClientRectangle.Top + alturaEncabezados,
                grilla.ClientRectangle.Width,
                grilla.ClientRectangle.Height - alturaEncabezados);

            if (areaFilas.Width <= 0 || areaFilas.Height <= 0)
            {
                return;
            }

            TextRenderer.DrawText(
                e.Graphics,
                _mensajeVacio,
                grilla.Font,
                areaFilas,
                Color.White,
                TextFormatFlags.HorizontalCenter
                    | TextFormatFlags.VerticalCenter
                    | TextFormatFlags.WordBreak
                    | TextFormatFlags.NoPrefix);
        }

        /// <summary>
        /// Decide la fila activa después de enlazar el listado (RF-21). Con filas y
        /// sin más de una coincidencia exacta de Código selecciona la fila 0 con
        /// <see cref="DataGridView.CurrentCell"/>; sin filas o con dos o más
        /// coincidencias exactas deja la grilla sin selección. No reordena: el
        /// orden y la prioridad del código exacto los resuelve
        /// <see cref="ProductoNegocio"/>.
        /// </summary>
        /// <param name="productos">Filas recién enlazadas, en su orden final.</param>
        private void AplicarSeleccionTrasCarga(List<ProductoRespuestaDto> productos)
        {
            if (productos.Count == 0 || ContarCoincidenciasExactasDeCodigo(productos) > 1)
            {
                // Sin filas o inconsistencia de unicidad: nada queda activo y los
                // botones de acción se deshabilitan (CF-1).
                _dgvProductos.CurrentCell = null;
                _dgvProductos.ClearSelection();
                ActualizarEstadoBotones();
                return;
            }

            // CurrentCell (y no solo Selected) deja la fila realmente activa, de modo
            // que ActualizarEstadoBotones y el ABM trabajan sobre ella.
            _dgvProductos.CurrentCell = _dgvProductos.Rows[0].Cells["colCodigo"];
            ActualizarEstadoBotones();
        }

        /// <summary>
        /// Cuenta cuántas filas mostradas tienen un Código idéntico al texto
        /// recortado del buscador, con el mismo criterio de la capa de negocio
        /// (ordinal, sin distinguir mayúsculas) (RF-21/D-12).
        /// </summary>
        /// <param name="productos">Filas mostradas por la grilla.</param>
        /// <returns>Cantidad de coincidencias exactas de Código; 0 si no hay texto.</returns>
        private int ContarCoincidenciasExactasDeCodigo(List<ProductoRespuestaDto> productos)
        {
            string texto = _txtBuscar.Text.Trim();
            if (texto.Length == 0)
            {
                return 0;
            }

            int coincidencias = 0;

            foreach (ProductoRespuestaDto producto in productos)
            {
                if (string.Equals(producto.Codigo, texto, StringComparison.OrdinalIgnoreCase))
                {
                    coincidencias++;
                }
            }

            return coincidencias;
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

        /// <summary>
        /// Recarga el listado al encender o apagar "Solo críticos" (RF-6/RF-11):
        /// una sola recarga por cambio. No reinicia la casilla, de modo que
        /// conserva su estado al cambiar de pestaña y al usar "Actualizar" (D-13).
        /// </summary>
        private async void chkSoloCriticos_CheckedChanged(object? sender, EventArgs e)
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
        /// Pinta el fondo de la fila según la criticidad ya calculada por
        /// <see cref="ProductoNegocio"/> (RF-8/RF-14, RNF-6): el nivel sale del DTO,
        /// la vista nunca compara el stock. Usa el tono atenuado de D-17 cuando el
        /// producto está inactivo. Las filas <c>Normal</c> conservan el estilo actual.
        /// El fondo y el de selección de las celdas se excluyen del pintado para que
        /// no tapen el relleno de criticidad cuando la fila está seleccionada (RF-22).
        /// </summary>
        private void dgvProductos_RowPrePaint(object? sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (sender is not DataGridView grilla
                || e.RowIndex < 0
                || e.RowIndex >= grilla.Rows.Count)
            {
                return;
            }

            ProductoRespuestaDto? producto =
                grilla.Rows[e.RowIndex].DataBoundItem as ProductoRespuestaDto;
            if (ObtenerRellenoCriticidad(producto) is not Color relleno)
            {
                return;
            }

            using (var brocha = new SolidBrush(relleno))
            {
                e.Graphics.FillRectangle(brocha, e.RowBounds);
            }

            e.PaintParts &= ~DataGridViewPaintParts.Background;
            e.PaintParts &= ~DataGridViewPaintParts.SelectionBackground;
        }

        /// <summary>
        /// Muestra el estado del producto en español ("Activo"/"Inactivo") en la
        /// columna de la grilla (RF-14), agrega en la columna de stock el indicador
        /// de criticidad conservando la cantidad (RF-8) y fija el color de selección
        /// del nivel para que la selección no reemplace el resaltado (RF-22).
        /// </summary>
        private void dgvProductos_CellFormatting(
            object? sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0
                || sender is not DataGridView grilla
                || e.ColumnIndex >= grilla.Columns.Count
                || e.RowIndex >= grilla.Rows.Count)
            {
                return;
            }

            ProductoRespuestaDto? producto =
                grilla.Rows[e.RowIndex].DataBoundItem as ProductoRespuestaDto;

            // El fondo de selección usa el mismo color del nivel de criticidad.
            if (ObtenerRellenoCriticidad(producto) is Color relleno)
            {
                e.CellStyle.SelectionBackColor = relleno;
                e.CellStyle.SelectionForeColor = Color.White;
                e.CellStyle.ForeColor = Color.White;
            }

            string nombreColumna = grilla.Columns[e.ColumnIndex].Name;
            if (nombreColumna == "colEstado" && e.Value is bool estado)
            {
                e.Value = estado ? "Activo" : "Inactivo";
                e.FormattingApplied = true;
                return;
            }

            if (nombreColumna == "colStock" && producto is not null)
            {
                switch (producto.NivelCriticidad)
                {
                    case NivelCriticidadStock.Agotado:
                        e.Value = $"{producto.Stock} · SIN STOCK";
                        e.FormattingApplied = true;
                        break;
                    case NivelCriticidadStock.BajoUmbral:
                        e.Value = $"{producto.Stock} · BAJO";
                        e.FormattingApplied = true;
                        break;
                }
            }
        }

        /// <summary>
        /// Marca la selección de una fila resaltada con un borde blanco de 2 px
        /// sobre el perímetro de la fila (RF-22), sin repintar el fondo: así el
        /// color de criticidad se conserva y la fila sigue leyéndose en escala de
        /// grises (RNF-4/RNF-10). Las filas sin resaltado usan el pintado estándar.
        /// </summary>
        private void dgvProductos_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (sender is not DataGridView grilla
                || e.RowIndex < 0
                || e.ColumnIndex < 0
                || e.RowIndex >= grilla.Rows.Count
                || e.ColumnIndex >= grilla.Columns.Count)
            {
                return;
            }

            ProductoRespuestaDto? producto =
                grilla.Rows[e.RowIndex].DataBoundItem as ProductoRespuestaDto;
            if (ObtenerRellenoCriticidad(producto) is not Color
                || !grilla.Rows[e.RowIndex].Selected)
            {
                return;
            }

            // Pintado estándar sin fondo ni fondo de selección: el relleno de
            // criticidad lo dibujó RowPrePaint y no debe taparse.
            Graphics? grafico = e.Graphics;
            if (grafico is null)
            {
                return;
            }

            DataGridViewPaintParts partes = e.PaintParts
                & ~DataGridViewPaintParts.Background
                & ~DataGridViewPaintParts.SelectionBackground;
            e.Paint(e.ClipBounds, partes);

            Rectangle celda = e.CellBounds;
            using var lapiz = new Pen(Color.White, 2F);
            grafico.DrawLine(lapiz, celda.Left, celda.Top, celda.Right - 1, celda.Top);
            grafico.DrawLine(
                lapiz, celda.Left, celda.Bottom - 1, celda.Right - 1, celda.Bottom - 1);

            // Los bordes verticales solo en los extremos de la fila.
            if (e.ColumnIndex == 0)
            {
                grafico.DrawLine(
                    lapiz, celda.Left, celda.Top, celda.Left, celda.Bottom - 1);
            }

            if (e.ColumnIndex == grilla.Columns.Count - 1)
            {
                grafico.DrawLine(
                    lapiz, celda.Right - 1, celda.Top, celda.Right - 1, celda.Bottom - 1);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Devuelve el color de relleno que corresponde al nivel de criticidad del
        /// producto (D-17), atenuado cuando está inactivo, o <c>null</c> cuando el
        /// nivel es <see cref="NivelCriticidadStock.Normal"/>.
        /// </summary>
        /// <param name="producto">Fila de la grilla, o <c>null</c> si no hay dato.</param>
        /// <returns>Color de relleno de la fila, o <c>null</c> para el estilo actual.</returns>
        private static Color? ObtenerRellenoCriticidad(ProductoRespuestaDto? producto)
        {
            if (producto is null)
            {
                return null;
            }

            return producto.NivelCriticidad switch
            {
                NivelCriticidadStock.Agotado =>
                    producto.Estado ? FondoAgotado : FondoAgotadoInactivo,
                NivelCriticidadStock.BajoUmbral =>
                    producto.Estado ? FondoBajoUmbral : FondoBajoUmbralInactivo,
                _ => null
            };
        }

        private async void btnActualizar_Click(object? sender, EventArgs e)
        {
            // RF-1/RF-14/RF-24: "Actualizar" refresca las categorías activas (y con
            // ellas detecta si la elegida dejó de estarlo, avisando una sola vez)
            // y recarga el listado desde Negocio conservando texto, estado y "Solo
            // críticos". No es una operación de escritura, por eso no lleva guard de
            // permiso: no modifica datos y el Vendedor no accede a esta pestaña.
            await CargarCategoriasAsync();
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
        /// pidiendo siempre confirmación en español antes de escribir. La acción
        /// aplicable, el título y la pregunta los resuelve la CapaNegocio sobre el
        /// estado <b>actual</b> del producto registrado (RF-21/RF-22).
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

            // Se deshabilita ANTES de la consulta: todo el flujo (confirmación,
            // aviso/diálogo, ejecución y recarga) va dentro del try, de modo que un
            // fallo de BD al componer la confirmación caiga en el mismo catch (RF-16)
            // y el finally rehabilite el botón (plan §7.1).
            _btnEstado.Enabled = false;

            try
            {
                // RF-21/RF-22: la acción aplicable, el título y la pregunta en español
                // los compone la CapaNegocio a partir del estado actual del producto;
                // la vista no deriva la acción del listado ni redacta los textos.
                ConfirmacionCambioEstadoProducto confirmacion =
                    await _productoNegocio.ObtenerConfirmacionCambioEstadoAsync(producto.IdProducto);

                // RF-23: si no hay acción aplicable (producto inexistente o
                // identificador inválido) se informa el motivo con un aviso, sin
                // diálogo SÍ/NO y sin escribir nada.
                if (!confirmacion.PuedeEjecutarse)
                {
                    MaterialMessageBox.Show(this, confirmacion.Mensaje, "Atención");
                    return;
                }

                // RF-12/D-13: confirmación simple en español antes de escribir.
                var dialogo = new MaterialDialog(
                    formularioPrincipal,
                    confirmacion.TituloDialogo,
                    confirmacion.Mensaje,
                    "SÍ",
                    true,
                    "NO");

                // RF-24: cancelar ⇒ sin ninguna escritura: el producto queda sin cambios.
                if (dialogo.ShowDialog(formularioPrincipal) != DialogResult.OK)
                {
                    return;
                }

                // RF-22: la vista solo ejecuta la acción confirmada por Negocio.
                switch (confirmacion.Accion)
                {
                    case AccionCambioEstadoProducto.Baja:
                        // La baja es lógica y la resuelve Negocio (RNF-3, sin DELETE físico).
                        RespuestaBajaProducto respuestaBaja =
                            await _productoNegocio.DarBajaProductoAsync(
                                confirmacion.IdProducto ?? producto.IdProducto, _idUsuarioActual);

                        if (respuestaBaja.Resultado != ResultadoBajaProducto.Exito)
                        {
                            // Casos límite (ya inactivo / sin permiso / no encontrado): se
                            // muestra el mensaje en español de Negocio, sin modificar nada.
                            MaterialMessageBox.Show(this, respuestaBaja.Mensaje, "Atención");
                            return;
                        }

                        MaterialMessageBox.Show(this, respuestaBaja.Mensaje, "Producto dado de baja");
                        break;

                    case AccionCambioEstadoProducto.Reactivacion:
                        RespuestaReactivacionProducto respuestaReactivacion =
                            await _productoNegocio.ReactivarProductoAsync(
                                confirmacion.IdProducto ?? producto.IdProducto, _idUsuarioActual);

                        if (respuestaReactivacion.Resultado != ResultadoReactivacionProducto.Exito)
                        {
                            // D-12/RF-13: si la categoría está inactiva, Negocio bloquea la
                            // reactivación y aquí solo se muestra su mensaje en español.
                            MaterialMessageBox.Show(this, respuestaReactivacion.Mensaje, "Atención");
                            return;
                        }

                        MaterialMessageBox.Show(this, respuestaReactivacion.Mensaje, "Producto reactivado");
                        break;

                    default:
                        // Defensivo: Ninguna siempre viaja con PuedeEjecutarse = false (RF-23).
                        return;
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

        /// <summary>
        /// Compone el texto informativo del umbral aplicado para el <c>Label</c>
        /// de la fila de filtros (RF-19). Con umbral vigente muestra el valor en
        /// unidades; en modo degradado informa la causa y el criterio sin mostrar
        /// ningún número (RF-10/RF-23). No parsea el texto: usa la resolución ya
        /// calculada una sola vez en el constructor (D-15).
        /// </summary>
        /// <returns>Texto del umbral, en una o dos líneas según el modo.</returns>
        private string ObtenerTextoUmbral()
        {
            if (_resolucionUmbral.EsDegradado)
            {
                return "Umbral no disponible: configuración inválida."
                    + Environment.NewLine
                    + "Solo se marca \"SIN STOCK\" (0 o negativo).";
            }

            return $"Umbral: {_resolucionUmbral.Valor.GetValueOrDefault()} unid.";
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
                // RF-1/RF-19: búsqueda, categoría, estado, columna reservada para
                // la casilla "Solo críticos" (T6) y texto del umbral.
                ColumnCount = 5,
                RowCount = 1,
                BackColor = TemaAplicacion.FondoPrincipal,
                Margin = new Padding(0, 0, 0, 4),
                AutoSize = false,
                Height = 56
            };

            _panelFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36F));
            _panelFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16F));
            _panelFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 13F));
            _panelFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12F));
            _panelFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 23F));
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

            // RF-6/RF-11: casilla "Solo críticos" (D-13). Se inicializa apagada
            // solo aquí, durante la construcción de la vista; ni las recargas ni
            // "Actualizar" ni el cambio de pestaña reinician su estado.
            _chkSoloCriticos = new MaterialCheckbox
            {
                Text = "Solo críticos",
                Checked = false,
                AutoSize = false,
                Dock = DockStyle.Fill,
                BackColor = TemaAplicacion.FondoPrincipal,
                ForeColor = Color.White,
                Margin = new Padding(5, 4, 5, 4)
            };

            // RF-19: texto del umbral vigente. Sale de la resolución calculada una
            // sola vez en el constructor (D-15); en modo degradado muestra el
            // aviso persistente en dos líneas, sin número y sin modal (RF-10/RF-23).
            _lblUmbral = new Label
            {
                Text = ObtenerTextoUmbral(),
                AutoSize = false,
                Dock = DockStyle.Fill,
                BackColor = TemaAplicacion.FondoPrincipal,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 8.25F),
                Margin = new Padding(5, 4, 5, 4)
            };

            _panelFiltros.Controls.Add(_txtBuscar, 0, 0);
            _panelFiltros.Controls.Add(_cboCategoria, 1, 0);
            _panelFiltros.Controls.Add(_cboEstado, 2, 0);
            _panelFiltros.Controls.Add(_chkSoloCriticos, 3, 0);
            _panelFiltros.Controls.Add(_lblUmbral, 4, 0);

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
            // Resaltado por criticidad (D-17) y borde de selección de la fila (RF-22).
            dgv.RowPrePaint += dgvProductos_RowPrePaint;
            dgv.CellPainting += dgvProductos_CellPainting;
            // Mensaje de grilla vacía dibujado sobre el área de filas (RF-13/D-16).
            dgv.Paint += dgvProductos_Paint;
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
            // La casilla ya quedó apagada al crearla, antes de esta suscripción:
            // no hay ningún CheckedChanged espurio al armar la vista (D-13).
            _chkSoloCriticos.CheckedChanged += chkSoloCriticos_CheckedChanged;

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

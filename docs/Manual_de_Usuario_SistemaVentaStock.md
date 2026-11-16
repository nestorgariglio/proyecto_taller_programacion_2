# Manual de Usuario del Sistema
**SistemaVentaStock - Sistema de Gestión de Stock, Compras y Punto de Venta (POS)**

---

- **Asignatura**: Taller de Programación II
- **Institución**: Facultad de Ciencias Exactas y Naturales y Agrimensura (FaCENA) – Universidad Nacional del Nordeste (UNNE)
- **Equipo de Desarrollo**: Grupo 70
- **Fecha de Emisión**: Noviembre de 2026

---

## Índice General

1. [Introducción y Propósito](#1-introducción-y-propósito)
2. [Requisitos del Sistema e Instalación](#2-requisitos-del-sistema-e-instalación)
3. [Seguridad y Control de Acceso](#3-seguridad-y-control-de-acceso)
   - 3.1. [Inicio de Sesión (Login)](#31-inicio-de-sesión-login)
   - 3.2. [Perfiles de Usuario y Roles (RBAC)](#32-perfiles-de-usuario-y-roles-rbac)
   - 3.3. [Política de Bloqueo por Intentos Fallidos](#33-política-de-bloqueo-por-intentos-fallidos)
4. [Entorno de Trabajo y Navegación](#4-entorno-de-trabajo-y-navegación)
   - 4.1. [Pantalla Principal (Dashboard / Home)](#41-pantalla-principal-dashboard--home)
   - 4.2. [Menú Dinámico por Rol](#42-menú-dinámico-por-rol)
5. [Módulo de Catálogos e Inventario](#5-módulo-de-catálogos-e-inventario)
   - 5.1. [Gestión de Categorías / Rubros](#51-gestión-de-categorías--rubros)
   - 5.2. [Gestión de Productos y Precios](#52-gestión-de-productos-y-precios)
   - 5.3. [Alertas de Stock Crítico](#53-alertas-de-stock-crítico)
6. [Módulo de Terceros](#6-módulo-de-terceros)
   - 6.1. [Padrón de Clientes](#61-padrón-de-clientes)
   - 6.2. [Padrón de Proveedores](#62-padrón-de-proveedores)
7. [Módulo de Compras y Abastecimiento](#7-módulo-de-compras-y-abastecimiento)
   - 7.1. [Registro de Facturas de Compra](#71-registro-de-facturas-de-compra)
   - 7.2. [Actualización Automática de Stock y Precios](#72-actualización-automática-de-stock-y-precios)
8. [Módulo de Punto de Venta (POS / Facturación)](#8-módulo-de-punto-de-venta-pos--facturación)
   - 8.1. [Búsqueda e Ingreso Manual de Productos](#81-búsqueda-e-ingreso-manual-de-productos)
   - 8.2. [Operación del Carrito de Ventas](#82-operación-del-carrito-de-ventas)
   - 8.3. [Asignación de Cliente o Consumidor Final](#83-asignación-de-cliente-o-consumidor-final)
   - 8.4. [Procesamiento de Pago, Cálculo de Vuelto y Emisión](#84-procesamiento-de-pago-cálculo-de-vuelto-y-emisión)
9. [Módulo de Administración y Mantenimiento de Base de Datos](#9-módulo-de-administración-y-mantenimiento-de-base-de-datos)
   - 9.1. [Generación de Respaldos (Backup)](#91-generación-de-respaldos-backup)
   - 9.2. [Restauración de la Base de Datos (Restore)](#92-restauración-de-la-base-de-datos-restore)
10. [Solución de Problemas Frecuentes (FAQ)](#10-solución-de-problemas-frecuentes-faq)

---

## 1. Introducción y Propósito

El presente **Manual de Usuario** tiene como propósito guiar al personal comercial y técnico en la operación cotidiana del sistema **SistemaVentaStock**. Este software ha sido concebido para optimizar la administración integral de comercios minoristas, cubriendo desde la recepción de mercadería y el control de existencias, hasta la atención rápida y transaccional en el Punto de Venta (POS).

El sistema se destaca por:
- Operatividad garantizada en **Red Local (LAN)** sin dependencia de conexión a Internet.
- **Transacciones atómicas** en motor SQL Server: todas las operaciones críticas se confirman o revierten en su totalidad, garantizando la consistencia del dinero y del stock.
- **Trazabilidad estricta**: las anulaciones o bajas no destruyen información física, sino que se gestionan como bajas lógicas (`Estado = 0`).

---

## 2. Requisitos del Sistema e Instalación

### 2.1. Requisitos de Software
- **Sistema Operativo**: Windows 10 (versión 1809 o superior), Windows 11 o Windows Server 2019+.
- **Plataforma de Ejecución**: .NET Runtime 10.0 (o superior).
- **Motor de Base de Datos**: Microsoft SQL Server 2016, 2019 o 2022 (Express, Standard o Enterprise).

### 2.2. Configuración Inicial (`appsettings.json`)
Antes de iniciar la aplicación por primera vez, verifique el archivo de configuración ubicado en la carpeta principal:

```json
{
  "ConnectionStrings": {
    "CadenaSQL": "Server=localhost\\SQLEXPRESS;Database=DB_SISTEMA_VENTA;Integrated Security=true;TrustServerCertificate=true;"
  },
  "StockCritico": {
    "UmbralMinimo": "5"
  },
  "Respaldos": {
    "RutaCarpeta": "C:\\RespaldosSistema"
  }
}
```
> [!NOTE]
> Si `RutaCarpeta` se deja vacía (`""`), el sistema guardará los respaldos automáticamente en la carpeta raíz del proyecto.

---

## 3. Seguridad y Control de Acceso

### 3.1. Inicio de Sesión (Login)
Al ejecutar el sistema, se presenta la pantalla de autenticación:

1. Ingrese su **Documento (DNI)** sin puntos ni caracteres especiales (ej: `90000001`).
2. Ingrese su **Contraseña**.
3. Presione el botón **INGRESAR** o la tecla **Enter**.

El sistema validará su identidad contra la base de datos comparando el hash seguro **BCrypt**.

### 3.2. Perfiles de Usuario y Roles (RBAC)
El acceso a las distintas pantallas se encuentra estrictamente delimitado según su rol:

| Perfil / Rol | Módulos y Funciones Habilitadas |
| :--- | :--- |
| **Administrador** | Administración técnica: gestión de usuarios y permisos, ABM autorizado de categorías, productos, clientes y proveedores, consulta de información y módulo exclusivo de Backup / Restore. Sin permiso para registrar compras, operar el POS, vender, cobrar ni anular comprobantes comerciales. |
| **Encargado** | Gestión de catálogo de productos, categorías y padrón de proveedores; registro y consulta de compras, reportes y autorización y ejecución de anulaciones de compras y ventas como responsable comercial. Sin permiso para vender, cobrar ni realizar Backup / Restore. |
| **Vendedor / Cajero** | Operación en el Punto de Venta (POS): búsqueda de artículos, armado de carritos, gestión y selección de clientes, ventas, cobro en caja y consulta básica de existencias. Sin permiso para registrar compras, autorizar anulaciones ni realizar Backup / Restore. |

El Administrador técnico no tiene acceso comercial total. El ABM de datos maestros no permite editar comprobantes ni modificar existencias por fuera de las reglas de negocio. Las anulaciones corresponden exclusivamente al Encargado y deben registrar el usuario responsable, la fecha y hora y el motivo, conservando el comprobante original mediante baja lógica. Las restricciones se validan en la capa de negocio, no solamente ocultando opciones de menú.

### 3.3. Política de Bloqueo por Intentos Fallidos
Por motivos de seguridad informática, si se ingresa una contraseña errónea **3 veces consecutivas** para el mismo documento, la cuenta quedará transitoriamente bloqueada para frustrar intentos de acceso por fuerza bruta. En tal caso, deberá solicitar a un **Administrador** el desbloqueo o blanqueo de su clave desde el módulo de gestión de usuarios.

---

## 4. Entorno de Trabajo y Navegación

### 4.1. Pantalla Principal (Dashboard / Home)
Tras iniciar sesión, accederá al panel principal del sistema, diseñado bajo lineamientos visuales modernos con **MaterialSkin**:
- **Tarjetas de Estado Rápido**: Muestra el total de ventas del día, el monto comercial acumulado y la cantidad de artículos en alerta de stock crítico.
- **Acceso a Respaldos (Solo Administrador)**: Panel dedicado para disparar copias de seguridad inmediatas o abrir el asistente de recuperación de base de datos.

### 4.2. Menú Dinámico por Rol
En el margen izquierdo o barra superior encontrará los módulos habilitados para su usuario. Las opciones para las cuales no posea privilegios serán automáticamente ocultadas para garantizar una interfaz despejada y segura.

---

## 5. Módulo de Catálogos e Inventario

### 5.1. Gestión de Categorías / Rubros
Permite organizar los productos por familias comerciales (ej: *Bebidas*, *Almacén*, *Limpieza*):
- **Creación**: Escriba el nombre en el formulario y presione **Guardar**.
- **Modificación**: Seleccione la categoría de la lista, modifique la descripción y guarde los cambios.
- **Baja Lógica**: Si un rubro deja de utilizarse, puede desactivarse. El sistema impedirá dar de baja una categoría si todavía contiene productos activos asociados.

### 5.2. Gestión de Productos y Precios
Formulario central para la administración de artículos comercializables:
- **Código del Producto**: Se ingresa manualmente mediante teclado. Es único e irrepetible.
- **Precios**: Se gestionan dos valores: `Precio de Compra` (costo) y `Precio de Venta` (al público).
- **Stock**: Muestra las existencias reales en depósito. El stock se actualiza automáticamente con cada compra o venta. En caso de requerir un ajuste manual (por rotura o merma), el sistema solicitará obligatoriamente un `Motivo de Ajuste`.

### 5.3. Alertas de Stock Crítico
Los artículos cuyas existencias alcancen o caigan por debajo del umbral mínimo (configurado por defecto en 5 unidades) se destacarán con alertas visuales de color para facilitar la reposición anticipada de mercadería.

---

## 6. Módulo de Terceros

### 6.1. Padrón de Clientes
Permite registrar a los compradores recurrentes para asociarlos a comprobantes fiscales:
- Ingrese DNI, Nombre, Apellido, Correo electrónico y Teléfono.
- El sistema valida el formato del correo y la unicidad del documento.
- Los clientes registrados quedan inmediatamente disponibles para ser buscados en el módulo de caja.

### 6.2. Padrón de Proveedores
Administra a los distribuidores mayoristas:
- Requiere CUIT (exactamente 11 dígitos numéricos sin guiones), Razón Social y datos de contacto.
- Es indispensable que un proveedor esté registrado y activo (`Estado = 1`) para poder cargarle una factura de compra.

---

## 7. Módulo de Compras y Abastecimiento

### 7.1. Registro de Facturas de Compra
Operación exclusiva del rol **Encargado**; el Administrador técnico y el Vendedor / Cajero no pueden registrar compras.

Cuando llega mercadería al depósito:
1. Diríjase a la pestaña **Compras**.
2. Seleccione el **Proveedor** en el desplegable.
3. Busque el artículo por código o descripción.
4. Ingrese la **Cantidad recibida**, el **Precio de Compra unitario** facturado y el **Precio de Venta sugerido**.
5. Presione **Agregar Línea**. Puede añadir tantos productos como contenga el comprobante.
6. Presione **Confirmar Compra**.

### 7.2. Actualización Automática de Stock y Precios
Al presionar *Confirmar Compra*, el sistema ejecuta una transacción atómica:
- Incrementa inmediatamente el stock físico de cada producto sumando las cantidades ingresadas.
- Actualiza el `Precio de Compra` y el `Precio de Venta` en el catálogo de productos con los nuevos valores vigentes.
- Guarda la cabecera y el detalle de la compra para auditoría histórica.

---

## 8. Módulo de Punto de Venta (POS / Facturación)

La operación de ventas y cobros está habilitada exclusivamente para el rol **Vendedor / Cajero**; el Administrador técnico y el Encargado no pueden vender ni cobrar.

La interacción se realiza mediante teclado y mouse. Los códigos de producto se ingresan manualmente.

El módulo de **Ventas** está optimizado para brindar la máxima agilidad en la línea de cajas:

```
┌─────────────────────────────────────────┬──────────────────────────────────────────┐
│ PANEL IZQUIERDO: INGRESO DE PRODUCTOS   │ PANEL DERECHO: DETALLE DEL CARRITO Y PAGO│
│                                         │                                          │
│ [ DNI o Nombre Cliente ] [Cons. Final]  │ [ Grilla interactiva de ítems cargados ] │
│  Cliente: Consumidor Final              │                                          │
│                                         │ [ Quitar seleccionado ]                  │
│ [ Código o Nombre ]  [ Cant: 1 ]        │                                          │
│ [ Grilla de Coincidencias ]             │  TOTAL: $ 1.250,00                       │
│                                         │  [ Paga con: $ 2.000 ]  Vuelto: $ 750,00 │
│ [ ➕ Agregar al Carrito ]               │  [ 💳 CONFIRMAR Y FACTURAR ] (Verde)     │
└─────────────────────────────────────────┴──────────────────────────────────────────┘
```

### 8.1. Búsqueda e Ingreso Manual de Productos
1. En el campo **Código de barras o nombre**, escriba mediante teclado el código completo del producto o parte de su nombre.
2. La grilla inferior mostrará las coincidencias en tiempo real con su stock disponible y precio unitario.
3. Seleccione el producto de la grilla con el mouse o el teclado, ingrese la cantidad deseada (por defecto `1`) y presione **Enter** desde el campo de cantidad o haga clic en **➕ Agregar al Carrito**.

### 8.2. Operación del Carrito de Ventas
- **Validación instantánea**: El sistema no permite agregar productos con existencias insuficientes.
- **Edición en grilla**: Si desea modificar la cantidad de una línea ya agregada, haga doble clic sobre la celda `Cant.` en el carrito, tipee el nuevo número y presione **Enter**.
- **Quitar producto**: Seleccione la fila y presione **Quitar seleccionado**.

### 8.3. Asignación de Cliente o Consumidor Final
- **Consumidor Final (Modo Rápido)**: Por defecto, el sistema atiende como *Consumidor Final* sin exigir carga de datos personales.
- **Cliente Registrado**: Si el cliente solicita comprobante a su nombre, escriba su DNI o nombre en el buscador superior y elija la coincidencia en la lista desplegable (con las flechas `↓`/`↑` y **Enter** o haciendo clic). El sistema asignará al cliente y congelará sus datos en el comprobante.
- **Restablecimiento**: Presione el botón **Cons. Final** en cualquier momento para volver a la modalidad genérica.

### 8.4. Procesamiento de Pago, Cálculo de Vuelto y Emisión
1. Observe el monto en la etiqueta **TOTAL**.
2. En el campo **Paga con $**, ingrese el dinero en efectivo entregado por el cliente (ej: `2000`).
3. El sistema evaluará el pago en tiempo real:
   - Si el monto es insuficiente, mostrará en pantalla: `Falta: $...` y el botón de cobro permanecerá deshabilitado.
   - Si el monto es suficiente, calculará el vuelto exacto: `Vuelto: $...` y habilitará el botón **💳 CONFIRMAR Y FACTURAR**.
4. Presione **CONFIRMAR Y FACTURAR**:
   - Se ejecuta el Stored Procedure `sp_RegistrarVenta` en el motor de base de datos.
   - Se descuenta el stock de forma atómica y se graba el comprobante inmutable.
   - Se muestra el cuadro de confirmación con el **N° de Venta** y el **Vuelto** a entregar.
   - El carrito se vacía automáticamente y el foco regresa al campo de búsqueda de productos, listo para atender al siguiente cliente.

---

## 9. Módulo de Administración y Mantenimiento de Base de Datos

> [!IMPORTANT]
> Estas operaciones son de criticidad máxima y solo están disponibles para el rol **Administrador**.

### 9.1. Generación de Respaldos (Backup)
1. Inicie sesión con una cuenta de Administrador y diríjase a la pantalla **Inicio (Home)**.
2. En el panel de Respaldos, presione el botón **Generar Backup**.
3. El sistema solicitará confirmación e instruirá a SQL Server para generar un archivo nativo `.bak` con fecha y hora (ej: `DB_SISTEMA_VENTA_20261111_195000.bak`).
4. Una vez finalizado, se notificará la correcta creación del archivo en la ruta configurada.

### 9.2. Restauración de la Base de Datos (Restore)
En caso de requerir volver la base de datos a un punto temporal previo por contingencia o reinstalación:
1. Presione el botón **Restaurar Backup**.
2. Se abrirá una ventana de selección de archivos: localice y seleccione el archivo `.bak` deseado.
3. El sistema desplegará un mensaje de **ADVERTENCIA CRÍTICA**, notificándole que la base actual será reemplazada en su totalidad.
4. Si confirma la acción, el sistema:
   - Cerrará temporalmente las conexiones activas al motor.
   - Ejecutará la restauración física del archivo.
   - Restituirá la base de datos a su estado operativo multi-usuario.
   - Informará el éxito del procedimiento.

---

## 10. Solución de Problemas Frecuentes (FAQ)

### ¿Qué hago si al iniciar sesión dice "Usuario inactivo. Contacte al administrador"?
Su cuenta ha sido desactivada por política interna. Solicite a un usuario con rol Administrador que ingrese al módulo de Usuarios y active nuevamente la casilla de estado de su perfil.

### ¿Por qué el botón "CONFIRMAR Y FACTURAR" no se habilita en caja?
Verifique que:
1. Haya al menos un producto cargado en el carrito.
2. El importe ingresado en el campo **Paga con $** sea un número válido y cubra o supere el **TOTAL** de la venta.

### Al intentar cobrar me sale "Stock insuficiente para el producto X". ¿Qué ocurrió?
El sistema aplica control de concurrencia en tiempo real. Si otro puesto de venta cobró las últimas existencias milisegundos antes, el motor SQL protege la consistencia bloqueando la venta en lugar de generar inventario negativo. Ajuste la cantidad en el carrito según la disponibilidad informada.

### ¿Dónde se guardan los archivos de copia de seguridad (.bak)?
Si no definió una ruta específica en `appsettings.json`, los archivos se depositan directamente en la carpeta raíz donde se encuentra instalado el sistema.

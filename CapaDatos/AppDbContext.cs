using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using CapaEntidad;

namespace CapaDatos
{
    /// <summary>
    /// Contexto de Entity Framework Core para las entidades del sistema.
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Inicializa el contexto con la configuración indicada por la aplicación.
        /// </summary>
        /// <param name="options">Opciones de conexión y comportamiento del contexto.</param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<TipoFactura> TiposFactura { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<DetalleCompra> DetallesCompra { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }
    }
}

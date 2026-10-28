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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>()
                .HasIndex(usuario => usuario.Dni)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasIndex(usuario => usuario.Correo)
                .IsUnique()
                .HasFilter("[correo] IS NOT NULL");

            modelBuilder.Entity<Usuario>()
                .Property(usuario => usuario.Sexo)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsRequired();

            modelBuilder.Entity<Usuario>()
                .ToTable(tabla => tabla.HasCheckConstraint(
                    "CK_USUARIO_SEXO",
                    "[sexo] IN ('M', 'F')"
                ));

            modelBuilder.Entity<Categoria>()
                .Property(categoria => categoria.Descripcion)
                .HasMaxLength(100);

            modelBuilder.Entity<Categoria>()
                .HasIndex(categoria => categoria.Descripcion)
                .IsUnique()
                .HasFilter("[descripcion] IS NOT NULL");

            modelBuilder.Entity<Cliente>()
                .Property(cliente => cliente.Dni)
                .HasMaxLength(20)
                .IsUnicode(false);

            modelBuilder.Entity<Cliente>()
                .Property(cliente => cliente.Nombre)
                .HasMaxLength(100);

            modelBuilder.Entity<Cliente>()
                .Property(cliente => cliente.Apellido)
                .HasMaxLength(100);

            modelBuilder.Entity<Cliente>()
                .Property(cliente => cliente.Correo)
                .HasMaxLength(254);

            modelBuilder.Entity<Cliente>()
                .Property(cliente => cliente.Telefono)
                .HasMaxLength(15)
                .IsUnicode(false);

            modelBuilder.Entity<Cliente>()
                .HasIndex(cliente => cliente.Dni)
                .IsUnique()
                .HasFilter("[dni] IS NOT NULL");

            modelBuilder.Entity<Cliente>()
                .HasIndex(cliente => cliente.Correo)
                .IsUnique()
                .HasFilter("[correo] IS NOT NULL AND [correo] <> ''");

        }
    }
}

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
        public DbSet<ProveedorProducto> ProveedoresProductos { get; set; }

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

            modelBuilder.Entity<Proveedor>(entity =>
            {
                entity.Property(proveedor => proveedor.Cuit)
                    .HasMaxLength(13)
                    .IsUnicode(false)
                    .IsRequired();

                entity.Property(proveedor => proveedor.RazonSocial)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .IsRequired();

                entity.Property(proveedor => proveedor.Correo)
                    .HasMaxLength(254)
                    .IsUnicode(false);

                entity.Property(proveedor => proveedor.Telefono)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.HasIndex(proveedor => proveedor.Cuit)
                    .IsUnique()
                    .HasFilter("[cuit] IS NOT NULL");

                entity.HasIndex(proveedor => proveedor.Correo)
                    .IsUnique()
                    .HasFilter("[correo] IS NOT NULL AND [correo] <> ''");
            });

            modelBuilder.Entity<Producto>(entity =>
            {
                entity.ToTable(tb => tb.HasTrigger("TR_PRODUCTO_FECHA_MODIFICACION"));
                entity.Property(producto => producto.Codigo)
                    .HasMaxLength(13);

                entity.HasIndex(producto => producto.Codigo)
                    .IsUnique()
                    .HasFilter("[codigo] IS NOT NULL");

                entity.Property(producto => producto.Nombre)
                    .HasMaxLength(100);

                entity.Property(producto => producto.Descripcion)
                    .HasMaxLength(300);

                entity.Property(producto => producto.Marca)
                    .HasMaxLength(100);

                entity.Property(producto => producto.StockMinimo)
                    .HasDefaultValue(5);

                entity.Property(producto => producto.MotivoAjusteStock)
                    .HasMaxLength(300);

                entity.Property(producto => producto.IdUsuario)
                    .IsRequired();

                entity.Property(producto => producto.IdUsuarioModifica)
                    .IsRequired(false);

                entity.Property(producto => producto.FechaModificacion)
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(producto => producto.FechaRegistro)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasOne(producto => producto.Categoria)
                    .WithMany()
                    .HasForeignKey(producto => producto.IdCategoria)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Usuario>()
                    .WithMany()
                    .HasForeignKey(producto => producto.IdUsuario)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Usuario>()
                    .WithMany()
                    .HasForeignKey(producto => producto.IdUsuarioModifica)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ProveedorProducto>(entity =>
            {
                entity.HasKey(pp => new { pp.IdProveedor, pp.IdProducto });

                entity.Property(pp => pp.FechaRegistro)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasOne(pp => pp.Proveedor)
                    .WithMany(p => p.ProductosProveedor)
                    .HasForeignKey(pp => pp.IdProveedor)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pp => pp.Producto)
                    .WithMany(p => p.ProveedoresProducto)
                    .HasForeignKey(pp => pp.IdProducto)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Compra>(entity =>
            {
                entity.Property(compra => compra.EstadoSolicitud)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasDefaultValue("Recibida")
                    .IsRequired();

                entity.Property(compra => compra.Observaciones)
                    .HasMaxLength(300);

                entity.Property(compra => compra.MotivoAnulacion)
                    .HasMaxLength(300);

                entity.HasOne(compra => compra.UsuarioAnula)
                    .WithMany()
                    .HasForeignKey(compra => compra.IdUsuarioAnula)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Venta>(entity =>
            {
                entity.Property(venta => venta.NroComprobante)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(venta => venta.DocumentoCliente)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(venta => venta.NombreCliente)
                    .HasMaxLength(200);

                entity.Property(venta => venta.MotivoAnulacion)
                    .HasMaxLength(300);

                entity.HasOne(venta => venta.Usuario)
                    .WithMany()
                    .HasForeignKey(venta => venta.IdUsuario)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(venta => venta.UsuarioAnula)
                    .WithMany()
                    .HasForeignKey(venta => venta.IdUsuarioAnula)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DetalleVenta>(entity =>
            {
                entity.Property(detalle => detalle.PrecioCompra)
                    .HasColumnType("decimal(18,2)")
                    .HasDefaultValue(0m);
            });

        }
    }
}

using Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Data
{
    public class AdopcionDbContext : DbContext
    {
        public AdopcionDbContext(DbContextOptions<AdopcionDbContext> options) : base(options)
        {
        }

        public DbSet<Animal> Animales { get; set; }

        public DbSet<FotoAnimal> Fotos { get; set; }

        public DbSet<SolicitudAdopcion> Solicitudes { get; set; }

        public DbSet<Donacion> Donaciones { get; set; }

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Fondo> Fondos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Coincide con DECIMAL(18,2) de la tabla creada en SQL Server
            modelBuilder.Entity<Donacion>()
                .Property(d => d.Monto)
                .HasPrecision(18, 2);

            // Coincide con la tabla Fondos creada con crear-tabla-fondo.sql
            modelBuilder.Entity<Fondo>()
                .Property(f => f.SaldoInicial)
                .HasPrecision(18, 2);

            // Coincide con la columna agregada con agregar-columna-cuestionario.sql
            modelBuilder.Entity<SolicitudAdopcion>()
                .Property(x => x.ResultadoCuestionario)
                .HasMaxLength(40);

            // Relaciones (llaves foráneas) que crea relacionar-tablas.sql:
            //   Fotos -> Animales, Solicitudes -> Animales (ya existían con la migración Inicial)
            //   Solicitudes -> Usuarios, Donaciones -> Usuarios, Donaciones -> Fondos
            // Si se borra una cuenta, sus solicitudes y donaciones se conservan (UsuarioId queda en NULL).
            // No hay propiedad de navegación a Usuario a propósito: así nunca viaja ContrasenaHash en el JSON.
            modelBuilder.Entity<SolicitudAdopcion>()
                .HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(s => s.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Donacion>()
                .HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Donacion>()
                .HasOne<Fondo>()
                .WithMany()
                .HasForeignKey(d => d.FondoId)
                .OnDelete(DeleteBehavior.SetNull);

            // Coincide con la tabla Usuarios creada con tabla-usuarios.sql
            modelBuilder.Entity<Usuario>(u =>
            {
                u.Property(x => x.NombreUsuario).HasMaxLength(50);
                u.Property(x => x.Correo).HasMaxLength(100);
                u.Property(x => x.ContrasenaHash).HasMaxLength(200);
                u.Property(x => x.Rol).HasMaxLength(20);
                u.HasIndex(x => x.NombreUsuario).IsUnique();
                u.HasIndex(x => x.Correo).IsUnique();
            });
        }
    }
}

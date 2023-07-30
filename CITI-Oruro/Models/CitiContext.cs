using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CITI_Oruro.Models;

public partial class CitiContext : DbContext
{
    public CitiContext()
    {
    }

    public CitiContext(DbContextOptions<CitiContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cuotum> Cuota { get; set; }

    public virtual DbSet<DetalleCuotum> DetalleCuota { get; set; }

    public virtual DbSet<DetalleInscripcion> DetalleInscripcions { get; set; }

    public virtual DbSet<Ingeniero> Ingenieros { get; set; }

    public virtual DbSet<Inscripcion> Inscripcions { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        //        => optionsBuilder.UseSqlServer("server=LAPTOP-2L5H5J2L\\SQLEXPRESS; database=CITI; Integrated Security=true; TrustServerCertificate=true");
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cuotum>(entity =>
        {
            entity.HasKey(e => e.IdCuota);

            entity.Property(e => e.Detalle)
                .HasMaxLength(1500)
                .IsFixedLength();
            entity.Property(e => e.Fecha).HasColumnType("datetime");

            entity.HasOne(d => d.IdIngenieroNavigation).WithMany(p => p.Cuota)
                .HasForeignKey(d => d.IdIngeniero)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Cuota_FK_Ingeniero");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Cuota)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Cuota_FK_Usuario");
        });

        modelBuilder.Entity<DetalleCuotum>(entity =>
        {
            entity.HasKey(e => e.IdDetalleCuota);

            entity.Property(e => e.DetalleMes)
                .HasMaxLength(20)
                .IsFixedLength();

            entity.HasOne(d => d.IdCuotaNavigation).WithMany(p => p.DetalleCuota)
                .HasForeignKey(d => d.IdCuota)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("DetalleCuota_FK_Cuota");
        });

        modelBuilder.Entity<DetalleInscripcion>(entity =>
        {
            entity.HasKey(e => e.IdDetalleInscripcion);

            entity.ToTable("DetalleInscripcion");

            entity.Property(e => e.Fecha).HasColumnType("datetime");

            entity.HasOne(d => d.IdIngenieroNavigation).WithMany(p => p.DetalleInscripcions)
                .HasForeignKey(d => d.IdIngeniero)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("DetalleInscripcion_FK_Ingeniero");

            entity.HasOne(d => d.IdInscripcionNavigation).WithMany(p => p.DetalleInscripcions)
                .HasForeignKey(d => d.IdInscripcion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("DetalleInscripcion_FK_Inscripcion");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.DetalleInscripcions)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("DetalleInscripcion_FK_Usuario");
        });

        modelBuilder.Entity<Ingeniero>(entity =>
        {
            entity.HasKey(e => e.IdIngeniero);

            entity.ToTable("Ingeniero");

            entity.HasIndex(e => e.Ci, "Ingeniero_Ci_UNI").IsUnique();

            entity.HasIndex(e => e.Rni, "Ingeniero_RNI_UNI").IsUnique();

            entity.Property(e => e.Especialidad)
                .HasMaxLength(40)
                .IsFixedLength();
            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.NombreCompleto)
                .HasMaxLength(100)
                .IsFixedLength();
            entity.Property(e => e.Rni).HasColumnName("RNI");
        });

        modelBuilder.Entity<Inscripcion>(entity =>
        {
            entity.HasKey(e => e.IdInscripcion);

            entity.ToTable("Inscripcion");

            entity.Property(e => e.Tipo)
                .HasMaxLength(20)
                .IsFixedLength();
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario);

            entity.ToTable("Usuario");

            entity.Property(e => e.Cuenta)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.NombreCompleto)
                .HasMaxLength(150)
                .IsFixedLength();
            entity.Property(e => e.Password)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Tipo)
                .HasMaxLength(20)
                .IsFixedLength();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

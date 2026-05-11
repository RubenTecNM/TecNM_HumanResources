using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Models.DTOs;

namespace RecursosHumanosWeb.Models;

public partial class RecursosHumanosContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;
    public int? _auditOrigenOverride; // Permitir override manual para procesos de sistema

    public RecursosHumanosContext()
    {
    }

    public RecursosHumanosContext(DbContextOptions<RecursosHumanosContext> options, IHttpContextAccessor? httpContextAccessor = null)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public virtual DbSet<ApiFunction> ApiFunctions { get; set; }

    public virtual DbSet<ApiKey> ApiKeys { get; set; }

    public virtual DbSet<ApiPermiso> ApiPermisos { get; set; }

    public virtual DbSet<ApiPermisosApiKeysTabla> ApiPermisosApiKeysTablas { get; set; }

    public virtual DbSet<Area> Areas { get; set; }

    public virtual DbSet<Corte> Cortes { get; set; }

    public virtual DbSet<Departamento> Departamentos { get; set; }

    public virtual DbSet<Permiso> Permisos { get; set; }

    public virtual DbSet<Puesto> Puestos { get; set; }

    public virtual DbSet<Tabla> Tablas { get; set; }

    public virtual DbSet<TipoPermiso> TipoPermisos { get; set; }

    public virtual DbSet<TipoUsuario> TipoUsuarios { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }
    public virtual DbSet<TipoOrigen> TipoOrigenes { get; set; }
    public virtual DbSet<TipoEstadoCorte> TipoEstadoCortes { get; set; }
    public virtual DbSet<TipoEstadoPermiso> TipoEstadoPermisos { get; set; }
    public virtual DbSet<ResetPasswordToken> ResetPasswordTokens { get; set; }
    public virtual DbSet<Auditoria> Auditorias { get; set; }
    public virtual DbSet<VwPermisosUsuario> VwPermisosUsuarios { get; set; }
    public DbSet<LoginDTO> LoginResultDTO { get; set; }
    public DbSet<PermisosDTO> SearchPermissionsDTO { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("RecursosHumanosContext"));
        }

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApiFunction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ApiFunct__3214EC0745513D7B");

            entity.ToTable(tb => tb.HasTrigger("trg_ApiFunctions_UpdateFechaModificacion"));

            entity.Property(e => e.Descripcion)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Estatus).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.IdUsuarioModifica).HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.IdUsuarioCreaNavigation).WithMany(p => p.ApiFunctionIdUsuarioCreaNavigations)
                .HasForeignKey(d => d.IdUsuarioCrea)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApiFunctions_Usuario_Crea");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.ApiFunctionIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_ApiFunctions_Usuario_Modifica");
        });

        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ApiKey__3214EC07C3DAD720");

            entity.ToTable("ApiKey", tb => tb.HasTrigger("trg_ApiKey_UpdateFechaModificacion"));

            entity.Property(e => e.Clave)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.Estatus).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.IdUsuarioModifica).HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.IdUsuarioCreaNavigation).WithMany(p => p.ApiKeyIdUsuarioCreaNavigations)
                .HasForeignKey(d => d.IdUsuarioCrea)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApiKeys_Usuario_Crea");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.ApiKeyIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_ApiKeys_Usuario_Modifica");
        });

        modelBuilder.Entity<ApiPermiso>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ApiPermi__3214EC07EB278C1E");

            entity.ToTable("ApiPermiso", tb => tb.HasTrigger("trg_ApiPermiso_UpdateFechaModificacion"));

            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Estatus).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.IdUsuarioModifica).HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.IdUsuarioCreaNavigation).WithMany(p => p.ApiPermisoIdUsuarioCreaNavigations)
                .HasForeignKey(d => d.IdUsuarioCrea)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApiPermisos_Usuario_Crea");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.ApiPermisoIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_ApiPermisos_Usuario_Modifica");
        });

        modelBuilder.Entity<ApiPermisosApiKeysTabla>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ApiPermi__3214EC078F3D9269");

            entity.ToTable("ApiPermisosApiKeysTabla", tb => tb.HasTrigger("trg_ApiPermisosApiKeysTabla_UpdateFechaModificacion"));

            entity.Property(e => e.Estatus).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.IdUsuarioModifica).HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.IdApiKeyNavigation).WithMany(p => p.ApiPermisosApiKeysTablas)
                .HasForeignKey(d => d.IdApiKey)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApiPermisosApiKeysTablas_ApiKeys");

            entity.HasOne(d => d.IdApiPermisoNavigation).WithMany(p => p.ApiPermisosApiKeysTablas)
                .HasForeignKey(d => d.IdApiPermiso)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApiPermisosApiKeysTablas_ApiPermisos");

            entity.HasOne(d => d.IdTablaNavigation).WithMany(p => p.ApiPermisosApiKeysTablas)
                .HasForeignKey(d => d.IdTabla)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApiPermisosApiKeysTablas_Tablas");

            entity.HasOne(d => d.IdUsuarioCreaNavigation).WithMany(p => p.ApiPermisosApiKeysTablaIdUsuarioCreaNavigations)
                .HasForeignKey(d => d.IdUsuarioCrea)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApiPermisosApiKeysTablas_UsuarioCrea");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.ApiPermisosApiKeysTablaIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_ApiPermisosApiKeysTablas_UsuarioModifica");
        });

        modelBuilder.Entity<Area>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Area__3214EC07A750D956");

            entity.ToTable("Area", tb => tb.HasTrigger("trg_Area_UpdateFechaModificacion"));

            entity.Property(e => e.Descripcion)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Estatus).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.IdUsuarioModifica).HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.IdUsuarioCreaNavigation).WithMany(p => p.AreaIdUsuarioCreaNavigations)
                .HasForeignKey(d => d.IdUsuarioCrea)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Area__IdUsuarioC__24285DB4");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.AreaIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK__Area__IdUsuarioM__251C81ED");
        });

        modelBuilder.Entity<Corte>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Corte__3214EC07702F01F4");

            entity.ToTable("Corte", tb => tb.HasTrigger("trg_Corte_UpdateFechaModificacion"));

            entity.Property(e => e.IdEstadoCorte).HasDefaultValue(1);
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(0)
                .HasDefaultValueSql("(getdate())");
            entity.Property(e => e.FechaModificacion)
                .HasPrecision(0)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.IdUsuarioModifica).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Inicia).HasColumnType("datetime");
            entity.Property(e => e.Termina).HasColumnType("datetime");

            entity.HasOne(d => d.IdEstadoCorteNavigation).WithMany(p => p.Cortes)
                .HasForeignKey(d => d.IdEstadoCorte)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Corte_TipoEstadoCorte");

            entity.HasOne(d => d.IdUsuarioCreaNavigation).WithMany(p => p.CorteIdUsuarioCreaNavigations)
                .HasForeignKey(d => d.IdUsuarioCrea)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Corte__IdUsuario__74AE54BC");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.CorteIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK__Corte__IdUsuario__75A278F5");
        });

        modelBuilder.Entity<Departamento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Departam__3214EC071FFCF6A1");

            entity.ToTable("Departamento", tb => tb.HasTrigger("trg_Departamento_UpdateFechaModificacion"));

            entity.Property(e => e.Descripcion)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Estatus).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.IdUsuarioModifica).HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.IdUsuarioCreaNavigation).WithMany(p => p.DepartamentoIdUsuarioCreaNavigations)
                .HasForeignKey(d => d.IdUsuarioCrea)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Departame__IdUsu__27F8EE98");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.DepartamentoIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK__Departame__IdUsu__28ED12D1");
        });

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Permiso__3214EC07503FFDCA");

            entity.ToTable("Permiso", tb => tb.HasTrigger("trg_Permiso_UpdateFechaModificacion"));

            entity.Property(e => e.Estatus).HasDefaultValue(true);
            entity.Property(e => e.IdEstadoPermiso).HasDefaultValue(1);
            entity.Property(e => e.Evidencia)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Fecha1).HasColumnType("datetime");
            entity.Property(e => e.Fecha2).HasColumnType("datetime");
            entity.Property(e => e.FechaAutorizacion)
                .HasPrecision(0)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(0)
                .HasDefaultValueSql("(getdate())");
            entity.Property(e => e.FechaModificacion)
                .HasPrecision(0)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.IdUsuarioModifica).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Motivo)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.IdCorteNavigation).WithMany(p => p.Permisos)
                .HasForeignKey(d => d.IdCorte)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Permiso__IdCorte__2A6B46EF");

            entity.HasOne(d => d.IdEstadoPermisoNavigation).WithMany(p => p.Permisos)
                .HasForeignKey(d => d.IdEstadoPermiso)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Permiso_TipoEstadoPermiso");

            entity.HasOne(d => d.IdTipoPermisoNavigation).WithMany(p => p.Permisos)
                .HasForeignKey(d => d.IdTipoPermiso)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Permiso__IdTipoP__2B5F6B28");

            entity.HasOne(d => d.IdUsuarioAutorizaNavigation).WithMany(p => p.PermisoIdUsuarioAutorizaNavigations)
                .HasForeignKey(d => d.IdUsuarioAutoriza)
                .HasConstraintName("FK__Permiso__IdUsuar__2F2FFC0C");

            entity.HasOne(d => d.IdUsuarioCreaNavigation).WithMany(p => p.PermisoIdUsuarioCreaNavigations)
                .HasForeignKey(d => d.IdUsuarioCrea)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Permiso__IdUsuar__2D47B39A");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.PermisoIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK__Permiso__IdUsuar__2E3BD7D3");

            entity.HasOne(d => d.IdUsuarioSolicitaNavigation).WithMany(p => p.PermisoIdUsuarioSolicitaNavigations)
                .HasForeignKey(d => d.IdUsuarioSolicita)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Permiso__IdUsuar__2C538F61");
        });

        modelBuilder.Entity<Puesto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Puesto__3214EC07319CC249");

            entity.ToTable("Puesto", tb => tb.HasTrigger("trg_Puesto_UpdateFechaModificacion"));

            entity.Property(e => e.ClaveTabular)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Estatus).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.IdUsuarioModifica).HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.IdUsuarioCreaNavigation).WithMany(p => p.PuestoIdUsuarioCreaNavigations)
                .HasForeignKey(d => d.IdUsuarioCrea)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Puesto__IdUsuari__308E3499");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.PuestoIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK__Puesto__IdUsuari__318258D2");
        });

        modelBuilder.Entity<Tabla>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tabla__3214EC0771DFB8B8");

            entity.ToTable("Tabla", tb => tb.HasTrigger("trg_Tabla_UpdateFechaModificacion"));

            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Estatus).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.IdUsuarioModifica).HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.IdUsuarioCreaNavigation).WithMany(p => p.TablaIdUsuarioCreaNavigations)
                .HasForeignKey(d => d.IdUsuarioCrea)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tablas_Usuario_Crea");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.TablaIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_Tablas_Usuario_Modifica");
        });

        modelBuilder.Entity<TipoPermiso>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TipoPerm__3214EC07AEB8B6FC");

            entity.ToTable("TipoPermiso", tb => tb.HasTrigger("trg_TipoPermiso_UpdateFechaModificacion"));

            entity.Property(e => e.Descripcion)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Estatus).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.IdUsuarioModifica).HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.IdUsuarioCreaNavigation).WithMany(p => p.TipoPermisoIdUsuarioCreaNavigations)
                .HasForeignKey(d => d.IdUsuarioCrea)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TipoPermi__IdUsu__345EC57D");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.TipoPermisoIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK__TipoPermi__IdUsu__3552E9B6");
        });

        modelBuilder.Entity<TipoUsuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TipoUsua__3214EC070D31226E");

            entity.ToTable("TipoUsuario", tb => tb.HasTrigger("trg_TipoUsuario_UpdateFechaModificacion"));

            entity.Property(e => e.Descripcion)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Estatus).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.IdUsuarioModifica).HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.IdUsuarioCreaNavigation).WithMany(p => p.TipoUsuarioIdUsuarioCreaNavigations)
                .HasForeignKey(d => d.IdUsuarioCrea)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TipoUsuar__IdUsu__36470DEF");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.TipoUsuarioIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK__TipoUsuar__IdUsu__373B3228");
        });

        modelBuilder.Entity<TipoEstadoCorte>(entity =>
        {
            entity.ToTable("TipoEstadoCorte", tb => tb.HasTrigger("trg_TipoEstadoCorte_UpdateFechaModificacion"));
            entity.Property(e => e.Descripcion).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Color).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.Estatus).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.HasOne(d => d.IdUsuarioCreaNavigation).WithMany().HasForeignKey(d => d.IdUsuarioCrea).HasConstraintName("FK_TipoEstadoCorte_UsuarioCrea");
        });

        modelBuilder.Entity<TipoEstadoPermiso>(entity =>
        {
            entity.ToTable("TipoEstadoPermiso", tb => tb.HasTrigger("trg_TipoEstadoPermiso_UpdateFechaModificacion"));
            entity.Property(e => e.Descripcion).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Color).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.Estatus).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.HasOne(d => d.IdUsuarioCreaNavigation).WithMany().HasForeignKey(d => d.IdUsuarioCrea).HasConstraintName("FK_TipoEstadoPermiso_UsuarioCrea");
        });

        modelBuilder.Entity<TipoOrigen>(entity =>
        {
            entity.ToTable("TipoOrigen");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Descripcion)
                .HasMaxLength(64)
                .IsUnicode(false);

            entity.Property(e => e.Estatus).HasDefaultValue(true);

            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.Property(e => e.FechaModificacion)
                .HasColumnType("datetime");

            entity.HasOne(d => d.IdUsuarioCreaNavigation).WithMany()
                .HasForeignKey(d => d.IdUsuarioCrea)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TipoOrigen_Usuario_Crea");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany()
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_TipoOrigen_Usuario_Modifica");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuario__3214EC07981AC028");

            entity.ToTable("Usuario", tb => tb.HasTrigger("trg_Usuario_UpdateFechaModificacion"));

            entity.Property(e => e.Clave)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Estatus).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.IdApiKey).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.IdUsuarioModifica).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.IdAreaNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdArea)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuario__IdArea__382F5661");

            entity.HasOne(d => d.IdDepartamentoNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdDepartamento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuario__IdDepar__39237A9A");

            entity.HasOne(d => d.IdPuestoNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdPuesto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuario__IdPuest__3A179ED3");

            entity.HasOne(d => d.IdTipoUsuarioNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdTipoUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuario__IdTipoU__3B0BC30C");

            entity.HasOne(d => d.IdUsuarioCreaNavigation).WithMany(p => p.InverseIdUsuarioCreaNavigation)
                .HasForeignKey(d => d.IdUsuarioCrea)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuario__IdUsuar__3BFFE745");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.InverseIdUsuarioModificaNavigation)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK__Usuario__IdUsuar__3CF40B7E");
        });

        modelBuilder.Entity<ResetPasswordToken>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("ResetPasswordToken", tb => tb.HasTrigger("trg_ResetPasswordToken_UpdateFechaModificacion"));

            entity.Property(e => e.Selector)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TokenHash)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.Estatus).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaCaducidad)
                .HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.Utilizado).HasDefaultValue(false);
            entity.Property(e => e.IdUsuarioModifica).HasDefaultValueSql("(NULL)");

            // FK heredadas de BaseEntity
            entity.HasOne(d => d.IdUsuarioCreaNavigation)
                .WithMany()
                .HasForeignKey(d => d.IdUsuarioCrea)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ResetPasswordToken_UsuarioCrea");

            entity.HasOne(d => d.IdUsuarioModificaNavigation)
                .WithMany()
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_ResetPasswordToken_UsuarioModifica");

            // FK propia de ResetPasswordToken
            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(d => d.IdUsuarioSolicita)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ResetPasswordToken_UsuarioSolicita");
        });

        modelBuilder.Entity<Auditoria>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Auditoria");

            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Estatus).HasDefaultValue(true);

            entity.HasOne(d => d.IdApiFunctionNavigation).WithMany()
                .HasForeignKey(d => d.IdApiFunction)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Auditoria_ApiFunction");

            entity.HasOne(d => d.IdApiKeyNavigation).WithMany()
                .HasForeignKey(d => d.IdApiKey)
                .HasConstraintName("FK_Auditoria_ApiKey");

            entity.HasOne(d => d.IdTablaNavigation).WithMany()
                .HasForeignKey(d => d.IdTabla)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Auditoria_Tabla");

            entity.HasOne(d => d.IdUsuarioCreaNavigation).WithMany()
                .HasForeignKey(d => d.IdUsuarioCrea)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Auditoria_UsuarioCrea");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany()
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_Auditoria_UsuarioModifica");

            entity.HasOne(d => d.IdTipoOrigenNavigation).WithMany(p => p.Auditoria)
                .HasForeignKey(d => d.IdTipoOrigen)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Auditoria_TipoOrigen");
        });

        modelBuilder.Entity<LoginDTO>(entity =>
        {
            entity.HasNoKey(); // IMPORTANTE: Al ser un DTO de un SP, no tiene llave
        });

        modelBuilder.Entity<PermisosDTO>(entity =>
        {
            entity.HasNoKey();
        });
        OnModelCreatingPartial(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await SetSqlSessionContext();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        SetSqlSessionContext().GetAwaiter().GetResult();
        return base.SaveChanges();
    }

    private async Task SetSqlSessionContext()
    {
        var httpContext = _httpContextAccessor?.HttpContext;
        if (httpContext == null) return;

        // 1. Obtener Ids de contexto
        var apiKeyId = httpContext.Items["ApiKeyId"] as int?;
        int? userId = null;
        
        var userIdClaim = httpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out int id)) 
            userId = id;
        else if (httpContext.Items["ApiUserId"] is int apiUserId) 
            userId = apiUserId;

        if (!userId.HasValue && !apiKeyId.HasValue) return;

        // 2. Asegurar que la conexión esté abierta para mantener el SESSION_CONTEXT entre llamadas
        if (Database.GetDbConnection().State != System.Data.ConnectionState.Open)
        {
            await Database.OpenConnectionAsync();
        }

        // 3. Establecer contexto en SQL Server
        if (userId.HasValue)
        {
            await Database.ExecuteSqlRawAsync("EXEC sp_set_session_context @key=N'IdUsuario', @value=@val", 
                new Microsoft.Data.SqlClient.SqlParameter("@val", userId.Value));
        }

        if (apiKeyId.HasValue)
        {
            await Database.ExecuteSqlRawAsync("EXEC sp_set_session_context @key=N'IdApiKey', @value=@val", 
                new Microsoft.Data.SqlClient.SqlParameter("@val", apiKeyId.Value));
        }

        // 4. Establecer IdTipoOrigen
        int idTipoOrigen;
        if (_auditOrigenOverride.HasValue)
        {
            idTipoOrigen = _auditOrigenOverride.Value; // Manual override (ej. Sistema Automático)
        }
        else if (apiKeyId.HasValue)
        {
            idTipoOrigen = 2; // API Externa
        }
        else
        {
            idTipoOrigen = 1; // Interfaz Web (Default)
        }

        if (idTipoOrigen != 1) // Solo enviar si es diferente al default del trigger (para optimizar)
        {
             await Database.ExecuteSqlRawAsync("EXEC sp_set_session_context @key=N'IdTipoOrigen', @value=@val", 
                new Microsoft.Data.SqlClient.SqlParameter("@val", idTipoOrigen));
        }
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

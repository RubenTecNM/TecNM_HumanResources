using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Models.DTOs;

namespace RecursosHumanosWeb.Models;

public partial class HumanResourcesContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;
    public int? _auditOriginOverride; // Allow manual override for system processes

    public HumanResourcesContext()
    {
    }

    public HumanResourcesContext(DbContextOptions<HumanResourcesContext> options, IHttpContextAccessor? httpContextAccessor = null)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public virtual DbSet<ApiFunction> ApiFunctions { get; set; }

    public virtual DbSet<ApiKey> ApiKeys { get; set; }

    public virtual DbSet<ApiPermission> ApiPermissions { get; set; }

    public virtual DbSet<ApiKeyTablePermission> ApiKeyTablePermissions { get; set; }

    public virtual DbSet<Area> Areas { get; set; }

    public virtual DbSet<Cutoff> Cutoffs { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Position> Positions { get; set; }

    public virtual DbSet<DatabaseTable> DatabaseTables { get; set; }

    public virtual DbSet<PermissionType> PermissionTypes { get; set; }

    public virtual DbSet<UserType> UserTypes { get; set; }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<OriginType> OriginTypes { get; set; }
    public virtual DbSet<CutoffStatusType> CutoffStatusTypes { get; set; }
    public virtual DbSet<PermissionStatusType> PermissionStatusTypes { get; set; }
    public virtual DbSet<ResetPasswordToken> ResetPasswordTokens { get; set; }
    public virtual DbSet<AuditLog> AuditLogs { get; set; }
    public virtual DbSet<VwPermissionsUsers> VwPermissionsUsers { get; set; }
    public DbSet<LoginDTO> LoginResultDTO { get; set; }
    public DbSet<PermissionsDTO> SearchPermissionsDTO { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("HumanResourcesContext"));
        }

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApiFunction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ApiFunct__3214EC0745513D7B");

            entity.ToTable(tb => tb.HasTrigger("trg_ApiFunction_UpdateModifiedAt"));

            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedByUserId).HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.ApiFunctionCreatedBy)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApiFunctions_Usuario_Crea");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.ApiFunctionUpdatedBy)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_ApiFunctions_Usuario_Modifica");
        });

        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ApiKey__3214EC07C3DAD720");

            entity.ToTable("ApiKey", tb => tb.HasTrigger("trg_ApiKey_UpdateModifiedAt"));

            entity.Property(e => e.Key)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedByUserId).HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.ApiKeyCreatedBy)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApiKeys_Usuario_Crea");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.ApiKeyUpdatedBy)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_ApiKeys_Usuario_Modifica");
        });

        modelBuilder.Entity<ApiPermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ApiPermi__3214EC07EB278C1E");

            entity.ToTable("ApiPermission", tb => tb.HasTrigger("trg_ApiPermission_UpdateModifiedAt"));

            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedByUserId).HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.ApiPermissionCreatedBy)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApiPermisos_Usuario_Crea");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.ApiPermissionUpdatedBy)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_ApiPermisos_Usuario_Modifica");
        });

        modelBuilder.Entity<ApiKeyTablePermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ApiPermi__3214EC078F3D9269");

            entity.ToTable("ApiKeyTablePermission", tb => tb.HasTrigger("trg_ApiKeyTablePermission_UpdateModifiedAt"));

            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedByUserId).HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.ApiKey).WithMany(p => p.ApiKeyTablePermissions)
                .HasForeignKey(d => d.ApiKeyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApiPermisosApiKeysTablas_ApiKeys");

            entity.HasOne(d => d.ApiPermission).WithMany(p => p.ApiKeyTablePermissions)
                .HasForeignKey(d => d.ApiPermissionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApiPermisosApiKeysTablas_ApiPermisos");

            entity.HasOne(d => d.Table).WithMany(p => p.ApiKeyTablePermissions)
                .HasForeignKey(d => d.TableId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApiPermisosApiKeysTablas_Tablas");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.ApiKeyTablePermissionCreatedBy)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApiPermisosApiKeysTablas_UsuarioCrea");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.ApiKeyTablePermissionUpdatedBy)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_ApiPermisosApiKeysTablas_UsuarioModifica");
        });

        modelBuilder.Entity<Area>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Area__3214EC07A750D956");

            entity.ToTable("Area", tb => tb.HasTrigger("trg_Area_UpdateModifiedAt"));

            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedByUserId).HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.AreaCreatedBy)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Area__IdUsuarioC__24285DB4");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.AreaUpdatedBy)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK__Area__IdUsuarioM__251C81ED");
        });

        modelBuilder.Entity<Cutoff>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Corte__3214EC07702F01F4");

            entity.ToTable("Cutoff", tb => tb.HasTrigger("trg_Cutoff_UpdateModifiedAt"));

            entity.Property(e => e.CutoffStatusTypeId).HasDefaultValue(1);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(getdate())");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.UpdatedByUserId).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.EndDate).HasColumnType("datetime");

            entity.HasOne(d => d.CutoffStatusType).WithMany(p => p.Cutoffs)
                .HasForeignKey(d => d.CutoffStatusTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Corte_TipoEstadoCorte");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.CutoffCreatedBy)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Corte__IdUsuario__74AE54BC");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.CutoffUpdatedBy)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK__Corte__IdUsuario__75A278F5");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Departam__3214EC071FFCF6A1");

            entity.ToTable("Department", tb => tb.HasTrigger("trg_Department_UpdateModifiedAt"));

            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedByUserId).HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.DepartmentCreatedBy)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Departame__IdUsu__27F8EE98");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.DepartmentUpdatedBy)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK__Departame__IdUsu__28ED12D1");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Permiso__3214EC07503FFDCA");

            entity.ToTable("Permission", tb => tb.HasTrigger("trg_Permission_UpdateModifiedAt"));

            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.PermissionStatusTypeId).HasDefaultValue(1);
            entity.Property(e => e.Evidence)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.AuthorizedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(getdate())");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.UpdatedByUserId).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Reason)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Cutoff).WithMany(p => p.Permissions)
                .HasForeignKey(d => d.CutoffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Permiso__IdCorte__2A6B46EF");

            entity.HasOne(d => d.PermissionStatusType).WithMany(p => p.Permissions)
                .HasForeignKey(d => d.PermissionStatusTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Permiso_TipoEstadoPermiso");

            entity.HasOne(d => d.PermissionType).WithMany(p => p.Permissions)
                .HasForeignKey(d => d.PermissionTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Permiso__IdTipoP__2B5F6B28");

            entity.HasOne(d => d.AuthorizingUser).WithMany(p => p.PermissionAuthorizedBy)
                .HasForeignKey(d => d.AuthorizingUserId)
                .HasConstraintName("FK__Permiso__IdUsuar__2F2FFC0C");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.PermissionCreatedBy)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Permiso__IdUsuar__2D47B39A");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.PermissionUpdatedBy)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK__Permiso__IdUsuar__2E3BD7D3");

            entity.HasOne(d => d.RequestingUser).WithMany(p => p.PermissionRequestedBy)
                .HasForeignKey(d => d.RequestingUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Permiso__IdUsuar__2C538F61");
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Puesto__3214EC07319CC249");

            entity.ToTable("Position", tb => tb.HasTrigger("trg_Position_UpdateModifiedAt"));

            entity.Property(e => e.TabularCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedByUserId).HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.PositionCreatedBy)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Puesto__IdUsuari__308E3499");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.PositionUpdatedBy)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK__Puesto__IdUsuari__318258D2");
        });

        modelBuilder.Entity<DatabaseTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tabla__3214EC0771DFB8B8");

            entity.ToTable("DatabaseTable", tb => tb.HasTrigger("trg_DatabaseTable_UpdateModifiedAt"));

            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedByUserId).HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.DatabaseTableCreatedBy)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tablas_Usuario_Crea");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.DatabaseTableUpdatedBy)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_Tablas_Usuario_Modifica");
        });

        modelBuilder.Entity<PermissionType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TipoPerm__3214EC07AEB8B6FC");

            entity.ToTable("PermissionType", tb => tb.HasTrigger("trg_PermissionType_UpdateModifiedAt"));

            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedByUserId).HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.PermissionTypeCreatedBy)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TipoPermi__IdUsu__345EC57D");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.PermissionTypeUpdatedBy)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK__TipoPermi__IdUsu__3552E9B6");
        });

        modelBuilder.Entity<UserType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TipoUsua__3214EC070D31226E");

            entity.ToTable("UserType", tb => tb.HasTrigger("trg_UserType_UpdateModifiedAt"));

            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedByUserId).HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.UserTypeCreatedBy)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TipoUsuar__IdUsu__36470DEF");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.UserTypeUpdatedBy)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK__TipoUsuar__IdUsu__373B3228");
        });

        modelBuilder.Entity<CutoffStatusType>(entity =>
        {
            entity.ToTable("CutoffStatusType", tb => tb.HasTrigger("trg_CutoffStatusType_UpdateModifiedAt"));
            entity.Property(e => e.Description).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Color).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.HasOne(d => d.CreatedByUser).WithMany().HasForeignKey(d => d.CreatedByUserId).HasConstraintName("FK_TipoEstadoCorte_UsuarioCrea");
        });

        modelBuilder.Entity<PermissionStatusType>(entity =>
        {
            entity.ToTable("PermissionStatusType", tb => tb.HasTrigger("trg_PermissionStatusType_UpdateModifiedAt"));
            entity.Property(e => e.Description).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Color).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.HasOne(d => d.CreatedByUser).WithMany().HasForeignKey(d => d.CreatedByUserId).HasConstraintName("FK_TipoEstadoPermiso_UsuarioCrea");
        });

        modelBuilder.Entity<OriginType>(entity =>
        {
            entity.ToTable("OriginType");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Description)
                .HasMaxLength(64)
                .IsUnicode(false);

            entity.Property(e => e.Status).HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByUser).WithMany()
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TipoOrigen_Usuario_Crea");

            entity.HasOne(d => d.UpdatedByUser).WithMany()
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_TipoOrigen_Usuario_Modifica");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuario__3214EC07981AC028");

            entity.ToTable("User", tb => tb.HasTrigger("trg_User_UpdateModifiedAt"));

            entity.Property(e => e.PasswordHash)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.ApiKeyId).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.UpdatedByUserId).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Area).WithMany(p => p.Users)
                .HasForeignKey(d => d.AreaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuario__IdArea__382F5661");

            entity.HasOne(d => d.Department).WithMany(p => p.Users)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuario__IdDepar__39237A9A");

            entity.HasOne(d => d.Position).WithMany(p => p.Users)
                .HasForeignKey(d => d.PositionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuario__IdPuest__3A179ED3");

            entity.HasOne(d => d.UserType).WithMany(p => p.Users)
                .HasForeignKey(d => d.UserTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuario__IdTipoU__3B0BC30C");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.InverseCreatedBy)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuario__IdUsuar__3BFFE745");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.InverseUpdatedBy)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK__Usuario__IdUsuar__3CF40B7E");
        });

        modelBuilder.Entity<ResetPasswordToken>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("ResetPasswordToken", tb => tb.HasTrigger("trg_ResetPasswordToken_UpdateModifiedAt"));

            entity.Property(e => e.Selector)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TokenHash)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExpiresAt)
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.Used).HasDefaultValue(false);
            entity.Property(e => e.UpdatedByUserId).HasDefaultValueSql("(NULL)");

            // FKs inherited from BaseEntity
            entity.HasOne(d => d.CreatedByUser)
                .WithMany()
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ResetPasswordToken_UsuarioCrea");

            entity.HasOne(d => d.UpdatedByUser)
                .WithMany()
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_ResetPasswordToken_UsuarioModifica");

            // ResetPasswordToken-specific FK
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(d => d.RequestingUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ResetPasswordToken_UsuarioSolicita");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("AuditLog");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue(true);

            entity.HasOne(d => d.ApiFunction).WithMany()
                .HasForeignKey(d => d.ApiFunctionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Auditoria_ApiFunction");

            entity.HasOne(d => d.ApiKey).WithMany()
                .HasForeignKey(d => d.ApiKeyId)
                .HasConstraintName("FK_Auditoria_ApiKey");

            entity.HasOne(d => d.Table).WithMany()
                .HasForeignKey(d => d.TableId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Auditoria_Tabla");

            entity.HasOne(d => d.CreatedByUser).WithMany()
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Auditoria_UsuarioCrea");

            entity.HasOne(d => d.UpdatedByUser).WithMany()
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_Auditoria_UsuarioModifica");

            entity.HasOne(d => d.OriginType).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.OriginTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Auditoria_TipoOrigen");
        });

        modelBuilder.Entity<LoginDTO>(entity =>
        {
            entity.HasNoKey(); // IMPORTANT: As it is a DTO from an SP, it has no key
        });

        modelBuilder.Entity<PermissionsDTO>(entity =>
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

        // 1. Get context ids
        var apiKeyId = httpContext.Items["ApiKeyId"] as int?;
        int? userId = null;
        
        var userIdClaim = httpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out int id)) 
            userId = id;
        else if (httpContext.Items["ApiUserId"] is int apiUserId) 
            userId = apiUserId;

        if (!userId.HasValue && !apiKeyId.HasValue) return;

        // 2. Ensure the connection is open to keep SESSION_CONTEXT between calls
        if (Database.GetDbConnection().State != System.Data.ConnectionState.Open)
        {
            await Database.OpenConnectionAsync();
        }

        // 3. Set the context in SQL Server
        if (userId.HasValue)
        {
            await Database.ExecuteSqlRawAsync("EXEC sp_set_session_context @key=N'UserId', @value=@val", 
                new Microsoft.Data.SqlClient.SqlParameter("@val", userId.Value));
        }

        if (apiKeyId.HasValue)
        {
            await Database.ExecuteSqlRawAsync("EXEC sp_set_session_context @key=N'ApiKeyId', @value=@val", 
                new Microsoft.Data.SqlClient.SqlParameter("@val", apiKeyId.Value));
        }

        // 4. Set OriginTypeId
        int originTypeId;
        if (_auditOriginOverride.HasValue)
        {
            originTypeId = _auditOriginOverride.Value; // Manual override (e.g. Automatic System)
        }
        else if (apiKeyId.HasValue)
        {
            originTypeId = 2; // External API
        }
        else
        {
            originTypeId = 1; // Web Interface (Default)
        }

        if (originTypeId != 1) // Only send if different from the trigger default (to optimize)
        {
             await Database.ExecuteSqlRawAsync("EXEC sp_set_session_context @key=N'OriginTypeId', @value=@val", 
                new Microsoft.Data.SqlClient.SqlParameter("@val", originTypeId));
        }

        // 5. Set ApiFunctionId based on the HTTP method
        int? apiFunctionId = httpContext.Request.Method switch
        {
            "GET" => 1,
            "POST" => 2,
            "PUT" => 3,
            "DELETE" => 4, // Mapped to PHYSICAL DELETE by default
            "PATCH" => 5,
            "HEAD" => 6,
            "OPTIONS" => 7,
            _ => null
        };

        if (apiFunctionId.HasValue)
        {
            await Database.ExecuteSqlRawAsync("EXEC sp_set_session_context @key=N'ApiFunctionId', @value=@val",
                new Microsoft.Data.SqlClient.SqlParameter("@val", apiFunctionId.Value));
        }
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

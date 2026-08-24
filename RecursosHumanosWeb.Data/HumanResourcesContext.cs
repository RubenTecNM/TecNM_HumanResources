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
    public virtual DbSet<ShiftType> ShiftTypes { get; set; }
    public virtual DbSet<ResetPasswordToken> ResetPasswordTokens { get; set; }
    public virtual DbSet<AuditLog> AuditLogs { get; set; }

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
            entity.HasKey(e => e.Id).HasName("PK_ApiFunction");

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
                .HasConstraintName("FK_ApiFunction_CreatedByUserId");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.ApiFunctionUpdatedBy)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_ApiFunction_UpdatedByUserId");
        });

        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ApiKey");

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
                .HasConstraintName("FK_ApiKey_CreatedByUserId");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.ApiKeyUpdatedBy)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_ApiKey_UpdatedByUserId");
        });

        modelBuilder.Entity<ApiPermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ApiPermission");

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
                .HasConstraintName("FK_ApiPermission_CreatedByUserId");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.ApiPermissionUpdatedBy)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_ApiPermission_UpdatedByUserId");
        });

        modelBuilder.Entity<ApiKeyTablePermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ApiKeyTablePermission");

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
                .HasConstraintName("FK_ApiKeyTablePermission_ApiKeyId");

            entity.HasOne(d => d.ApiPermission).WithMany(p => p.ApiKeyTablePermissions)
                .HasForeignKey(d => d.ApiPermissionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApiKeyTablePermission_ApiPermissionId");

            entity.HasOne(d => d.Table).WithMany(p => p.ApiKeyTablePermissions)
                .HasForeignKey(d => d.TableId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApiKeyTablePermission_TableId");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.ApiKeyTablePermissionCreatedBy)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApiKeyTablePermission_CreatedByUserId");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.ApiKeyTablePermissionUpdatedBy)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_ApiKeyTablePermission_UpdatedByUserId");
        });

        modelBuilder.Entity<Area>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Area");

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
                .HasConstraintName("FK_Area_CreatedByUserId");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.AreaUpdatedBy)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_Area_UpdatedByUserId");
        });

        modelBuilder.Entity<Cutoff>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Cutoff");

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
                .HasConstraintName("FK_Cutoff_CutoffStatusTypeId");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.CutoffCreatedBy)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cutoff_CreatedByUserId");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.CutoffUpdatedBy)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_Cutoff_UpdatedByUserId");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Department");

            entity.ToTable("Department", tb => tb.HasTrigger("trg_Department_UpdateModifiedAt"));

            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedByUserId).HasDefaultValueSql("(NULL)");

            entity.Property(e => e.AreaId)
                .HasColumnName("AreaId")
                .HasColumnType("int");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.DepartmentCreatedBy)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Department_CreatedByUserId");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.DepartmentUpdatedBy)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_Department_UpdatedByUserId");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Permission");

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
            entity.Property(e => e.AuthorizationComment)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasOne(d => d.Cutoff).WithMany(p => p.Permissions)
                .HasForeignKey(d => d.CutoffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Permission_CutoffId");

            entity.HasOne(d => d.PermissionStatusType).WithMany(p => p.Permissions)
                .HasForeignKey(d => d.PermissionStatusTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Permission_PermissionStatusTypeId");

            entity.HasOne(d => d.PermissionType).WithMany(p => p.Permissions)
                .HasForeignKey(d => d.PermissionTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Permission_PermissionTypeId");

            entity.HasOne(d => d.AuthorizingUser).WithMany(p => p.PermissionAuthorizedBy)
                .HasForeignKey(d => d.AuthorizingUserId)
                .HasConstraintName("FK_Permission_AuthorizingUserId");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.PermissionCreatedBy)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Permission_CreatedByUserId");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.PermissionUpdatedBy)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_Permission_UpdatedByUserId");

            entity.HasOne(d => d.RequestingUser).WithMany(p => p.PermissionRequestedBy)
                .HasForeignKey(d => d.RequestingUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Permission_RequestingUserId");

            entity.HasOne(d => d.ShiftType).WithMany(p => p.Permissions)
                .HasForeignKey(d => d.Shift)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Permission_Shift");
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Position");

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
                .HasConstraintName("FK_Position_CreatedByUserId");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.PositionUpdatedBy)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_Position_UpdatedByUserId");
        });

        modelBuilder.Entity<DatabaseTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_DatabaseTable");

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
                .HasConstraintName("FK_DatabaseTable_CreatedByUserId");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.DatabaseTableUpdatedBy)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_DatabaseTable_UpdatedByUserId");
        });

        modelBuilder.Entity<PermissionType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_PermissionType");

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
                .HasConstraintName("FK_PermissionType_CreatedByUserId");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.PermissionTypeUpdatedBy)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_PermissionType_UpdatedByUserId");
        });

        modelBuilder.Entity<ShiftType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ShiftType");

            entity.ToTable("ShiftType", tb => tb.HasTrigger("trg_ShiftType_UpdateModifiedAt"));

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

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.ShiftTypeCreatedBy)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ShiftType_CreatedByUserId");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.ShiftTypeUpdatedBy)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_ShiftType_UpdatedByUserId");
        });

        modelBuilder.Entity<UserType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_UserType");

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
                .HasConstraintName("FK_UserType_CreatedByUserId");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.UserTypeUpdatedBy)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_UserType_UpdatedByUserId");
        });

        modelBuilder.Entity<CutoffStatusType>(entity =>
        {
            entity.ToTable("CutoffStatusType", tb => tb.HasTrigger("trg_CutoffStatusType_UpdateModifiedAt"));
            entity.Property(e => e.Description).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Color).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.HasOne(d => d.CreatedByUser).WithMany().HasForeignKey(d => d.CreatedByUserId).HasConstraintName("FK_CutoffStatusType_CreatedByUserId");
        });

        modelBuilder.Entity<PermissionStatusType>(entity =>
        {
            entity.ToTable("PermissionStatusType", tb => tb.HasTrigger("trg_PermissionStatusType_UpdateModifiedAt"));
            entity.Property(e => e.Description).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Color).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.HasOne(d => d.CreatedByUser).WithMany().HasForeignKey(d => d.CreatedByUserId).HasConstraintName("FK_PermissionStatusType_CreatedByUserId");
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
                .HasConstraintName("FK_OriginType_CreatedByUserId");

            entity.HasOne(d => d.UpdatedByUser).WithMany()
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_OriginType_UpdatedByUserId");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_User");

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
                .HasConstraintName("FK_User_AreaId");

            entity.HasOne(d => d.Department).WithMany(p => p.Users)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_DepartmentId");

            entity.HasOne(d => d.Position).WithMany(p => p.Users)
                .HasForeignKey(d => d.PositionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_PositionId");

            entity.HasOne(d => d.UserType).WithMany(p => p.Users)
                .HasForeignKey(d => d.UserTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_UserTypeId");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.InverseCreatedBy)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_UpdatedByUserId");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.InverseUpdatedBy)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_User_CreatedByUserId");
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
                .HasConstraintName("FK_ResetPasswordToken_CreatedByUserId");

            entity.HasOne(d => d.UpdatedByUser)
                .WithMany()
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_ResetPasswordToken_UpdatedByUserId");

            // ResetPasswordToken-specific FK
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(d => d.RequestingUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ResetPasswordToken_RequestingUserId");
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
                .HasConstraintName("FK_AuditLog_ApiFunctionId");

            entity.HasOne(d => d.ApiKey).WithMany()
                .HasForeignKey(d => d.ApiKeyId)
                .HasConstraintName("FK_AuditLog_ApiKeyId");

            entity.HasOne(d => d.Table).WithMany()
                .HasForeignKey(d => d.TableId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AuditLog_TableId");

            entity.HasOne(d => d.CreatedByUser).WithMany()
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AuditLog_CreatedByUserId");

            entity.HasOne(d => d.UpdatedByUser).WithMany()
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_AuditLog_UpdatedByUserId");

            entity.HasOne(d => d.OriginType).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.OriginTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AuditLog_OriginTypeId");

            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_AuditLog_CreatedAt");
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

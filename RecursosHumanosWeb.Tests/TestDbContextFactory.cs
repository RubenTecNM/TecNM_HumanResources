using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using RecursosHumanosWeb.Mapping;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.Enums;
using RecursosHumanosWeb.Services;

namespace RecursosHumanosWeb.Tests;

public static class TestDbContextFactory
{
    public static HumanResourcesContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<HumanResourcesContext>()
            .UseInMemoryDatabase(dbName)
            .EnableSensitiveDataLogging()
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new HumanResourcesContext(options);
    }

    public static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
        return config.CreateMapper();
    }

    public static PermissionService CreatePermissionService(HumanResourcesContext context)
    {
        return new PermissionService(
            context,
            CreateMapper(),
            NullLogger<PermissionService>.Instance,
            new MemoryCache(new MemoryCacheOptions()));
    }

    public static CutoffService CreateCutoffService(HumanResourcesContext context)
    {
        return new CutoffService(context, NullLogger<CutoffService>.Instance);
    }

    public static UserService CreateUserService(HumanResourcesContext context)
    {
        return new UserService(context, NullLogger<UserService>.Instance);
    }

    public static User CreateUser(string name, UserTypeEnum userType, int areaId = 1)
    {
        return new User
        {
            Name = name,
            Email = $"{name.ToLowerInvariant()}@test.local",
            PasswordHash = "hash",
            UserTypeId = (int)userType,
            AreaId = areaId,
            DepartmentId = 1,
            PositionId = 1,
            Status = true
        };
    }

    public static Cutoff CreateCutoff(DateTime start, DateTime end, CutoffStatusEnum status = CutoffStatusEnum.Open)
    {
        return new Cutoff
        {
            StartDate = start,
            EndDate = end,
            CutoffStatusTypeId = (int)status,
            Status = true,
            CreatedByUserId = 1
        };
    }

    public static Permission CreatePermission(int requestingUserId, DateTime start, DateTime? end, int cutoffId, PermissionStatusEnum status = PermissionStatusEnum.Pending, bool withPay = true)
    {
        return new Permission
        {
            Reason = "Motivo de prueba",
            StartDate = start,
            EndDate = end,
            CutoffId = cutoffId,
            PermissionStatusTypeId = (int)status,
            WithPay = withPay,
            RequestingUserId = requestingUserId,
            PermissionTypeId = 1,
            Status = true,
            CreatedByUserId = 1
        };
    }

    /// <summary>
    /// Crea el contexto con datos semilla: área, usuarios de los cuatro tipos y un corte activo vigente.
    /// </summary>
    public static async Task<HumanResourcesContext> SeedAsync(string dbName)
    {
        var context = CreateContext(dbName);

        context.Areas.Add(new Area { Description = "Área General", Status = true, CreatedByUserId = 1 });
        context.PermissionTypes.Add(new PermissionType { Description = "Permiso General", Status = true, CreatedByUserId = 1 });
        context.Departments.Add(new Department { Description = "Departamento General", Status = true, CreatedByUserId = 1 });
        context.Positions.Add(new Position { Description = "Puesto General", TabularCode = "GEN", Status = true, CreatedByUserId = 1 });

        await context.SaveChangesAsync();

        context.Users.Add(CreateUser("admin", UserTypeEnum.Administrator));
        context.Users.Add(CreateUser("jefe", UserTypeEnum.Authorizer));
        context.Users.Add(CreateUser("rh", UserTypeEnum.HumanResources));
        context.Users.Add(CreateUser("empleado", UserTypeEnum.Employee));

        var today = DateTime.Now.Date;
        context.Cutoffs.Add(CreateCutoff(today.AddMonths(-1), today.AddMonths(1).AddDays(-1)));

        await context.SaveChangesAsync();
        return context;
    }
}

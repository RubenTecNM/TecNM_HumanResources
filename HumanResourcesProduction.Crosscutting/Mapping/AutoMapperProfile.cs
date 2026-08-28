using AutoMapper;
using HumanResourcesProduction.Models;
using HumanResourcesProduction.Models.ViewModels.User;
using HumanResourcesProduction.Models.ViewModels;
using HumanResourcesProduction.Models.DTOs;

namespace HumanResourcesProduction.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // 1. Mapeo para Detalles (Optimizado para ProjectTo)
            CreateMap<User, UserDetailsViewModel>()
                // AutoMapper maneja automáticamente el nulo: si UserType es nulo, devuelve null.
                // Usamos Null-coalescing para asignar el valor por defecto.
                .ForMember(dest => dest.UserTypeName, opt => opt.MapFrom(src => src.UserType.Description ?? "N/A"))
                .ForMember(dest => dest.AreaDescription, opt => opt.MapFrom(src => src.Area.Description ?? "N/A"))
                .ForMember(dest => dest.DepartmentDescription, opt => opt.MapFrom(src => src.Department.Description ?? "N/A"))
                .ForMember(dest => dest.PositionDescription, opt => opt.MapFrom(src => src.Position.Description ?? "N/A"))

                // Campos string planos: sin mapeo explícito, la convención de AutoMapper
                // aplicaría ToString() sobre la entidad de navegación y produciría el
                // nombre del tipo ("HumanResourcesProduction.Models.Area") en lugar del dato.
                .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => src.UserType.Description ?? ""))
                .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Area.Description ?? ""))
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.Department.Description ?? ""))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Position.Description ?? ""))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedByUser.Name ?? "Sistema"))
                .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src =>
                    src.UpdatedByUser != null ? src.UpdatedByUser.Name : ""))
                
                // Lógica para auditoría: Al usar ProjectTo, esto se traduce a un LEFT JOIN en SQL
                .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => 
                    src.CreatedByUser.Name ?? "Sistema/Auto-creado"))
                .ForMember(dest => dest.UpdatedByName, opt => opt.MapFrom(src => 
                    src.UpdatedByUser.Name ?? "Sin cambios"));

            // 2. Mapeos de Creación y Edición
            CreateMap<UserCreateViewModel, User>();
            
            CreateMap<UserUpdateViewModel, User>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                // Solo mapea la clave si el user escribió algo en el campo
                .ForMember(dest => dest.PasswordHash, opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.Password)));

            CreateMap<User, UserUpdateViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            CreateMap<Permission, PermissionsDTO>()
                .ForMember(d => d.Name, o => o.MapFrom(s => s.RequestingUser.Name ?? "N/A"))
                .ForMember(d => d.PermissionType, o => o.MapFrom(s => s.PermissionType.Description ?? "N/A"))
                .ForMember(d => d.StatusDescription, o => o.MapFrom(s => s.PermissionStatusType.Description ?? "N/A"))
                .ForMember(d => d.StatusColor, o => o.MapFrom(s => s.PermissionStatusType.Color ?? "secondary"))
                .ForMember(d => d.ShiftDescription, o => o.MapFrom(s => s.ShiftType != null ? s.ShiftType.Description : null));

            CreateMap<Permission, PermissionsCreateViewModel>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartDate.HasValue ? src.StartDate.Value.ToString("HH:mm") : null))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndDate.HasValue ? src.EndDate.Value.ToString("HH:mm") : null))
                .ReverseMap()
                .ForMember(dest => dest.Evidence, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Evidence)));

            // 3b. Creación (VM sin WithPay/Id/Evidence para evitar over-posting)
            CreateMap<PermissionCreateViewModel, Permission>();

            // 4. Listados
            CreateMap<Cutoff, CutoffsDTO>()
                .ForMember(d => d.StatusDescription, o => o.MapFrom(s => s.CutoffStatusType.Description ?? "N/A"))
                .ForMember(d => d.StatusColor, o => o.MapFrom(s => s.CutoffStatusType.Color ?? "secondary"));
            CreateMap<User, UsersDTO>();
            CreateMap<User, UsersListDTO>()
                .ForMember(d => d.Department, o => o.MapFrom(s => s.Department.Description ?? "N/A"));

           // 5. Areas
            CreateMap<HumanResourcesProduction.Models.ViewModels.Areas.AreaCreateViewModel, Area>();
            CreateMap<HumanResourcesProduction.Models.ViewModels.Areas.AreaUpdateViewModel, Area>();

            // 6. Departments
            CreateMap<HumanResourcesProduction.Models.ViewModels.Departments.DepartmentCreateViewModel, Department>();
            CreateMap<HumanResourcesProduction.Models.ViewModels.Departments.DepartmentUpdateViewModel, Department>();

            // 7. Positions
            CreateMap<HumanResourcesProduction.Models.ViewModels.Positions.PositionCreateViewModel, Position>();
            CreateMap<HumanResourcesProduction.Models.ViewModels.Positions.PositionUpdateViewModel, Position>();

            // 8. PermissionTypes
            CreateMap<HumanResourcesProduction.Models.ViewModels.PermissionTypes.PermissionTypeCreateViewModel, PermissionType>();
            CreateMap<HumanResourcesProduction.Models.ViewModels.PermissionTypes.PermissionTypeUpdateViewModel, PermissionType>();

            // 9. UserTypes
            CreateMap<HumanResourcesProduction.Models.ViewModels.UserTypes.UserTypeCreateViewModel, UserType>();
            CreateMap<HumanResourcesProduction.Models.ViewModels.UserTypes.UserTypeUpdateViewModel, UserType>();

            // 10. Cutoffs (API)
            CreateMap<HumanResourcesProduction.Models.ViewModels.Cutoffs.CutoffApiCreateViewModel, Cutoff>();
            CreateMap<HumanResourcesProduction.Models.ViewModels.Cutoffs.CutoffApiUpdateViewModel, Cutoff>();

            // 11. Permissions (API)
            CreateMap<HumanResourcesProduction.Models.ViewModels.Permissions.PermissionApiCreateViewModel, Permission>();
            CreateMap<HumanResourcesProduction.Models.ViewModels.Permissions.PermissionApiUpdateViewModel, Permission>();

            // 12. AuditLog
            CreateMap<AuditLog, AuditLogDTO>()
                .ForMember(d => d.DatabaseTable, o => o.MapFrom(s => s.Table.Description))
                .ForMember(d => d.Accion, o => o.MapFrom(s => s.ApiFunction.Description))
                .ForMember(d => d.User, o => o.MapFrom(s => s.CreatedByUser.Name))
                .ForMember(d => d.ApiKeyDescription, o => o.MapFrom(s => s.ApiKey != null ? (s.ApiKey.Key.Length >= 8 ? s.ApiKey.Key.Substring(0, 8) + "..." : s.ApiKey.Key) : "N/A"))
                .ForMember(d => d.OriginTypeDescription, o => o.MapFrom(s => s.OriginType.Description));
        }
    }
}
using AutoMapper;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.ViewModels.Usuario;
using RecursosHumanosWeb.Models.ViewModels;
using RecursosHumanosWeb.Models.DTOs;

namespace RecursosHumanosWeb.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // 1. Mapeo para Detalles (Optimizado para ProjectTo)
            CreateMap<Usuario, UsuarioDetailsViewModel>()
                // AutoMapper maneja automáticamente el nulo: si IdTipoUsuarioNavigation es nulo, devuelve null.
                // Usamos Null-coalescing para asignar el valor por defecto.
                .ForMember(dest => dest.TipoUsuarioNombre, opt => opt.MapFrom(src => src.IdTipoUsuarioNavigation.Descripcion ?? "N/A"))
                .ForMember(dest => dest.AreaDescripcion, opt => opt.MapFrom(src => src.IdAreaNavigation.Descripcion ?? "N/A"))
                .ForMember(dest => dest.DepartamentoDescripcion, opt => opt.MapFrom(src => src.IdDepartamentoNavigation.Descripcion ?? "N/A"))
                .ForMember(dest => dest.PuestoDescripcion, opt => opt.MapFrom(src => src.IdPuestoNavigation.Descripcion ?? "N/A"))
                
                // Lógica para auditoría: Al usar ProjectTo, esto se traduce a un LEFT JOIN en SQL
                .ForMember(dest => dest.UsuarioCreaNombre, opt => opt.MapFrom(src => 
                    src.IdUsuarioCreaNavigation.Nombre ?? "Sistema/Auto-creado"))
                .ForMember(dest => dest.UsuarioModificaNombre, opt => opt.MapFrom(src => 
                    src.IdUsuarioModificaNavigation.Nombre ?? "Sin cambios"));

            // 2. Mapeos de Creación y Edición
            CreateMap<UsuarioCreateViewModel, Usuario>();
            
            CreateMap<UsuarioUpdateViewModel, Usuario>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.IdUsuario))
                // Solo mapea la clave si el usuario escribió algo en el campo
                .ForMember(dest => dest.Clave, opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.Clave)));

            CreateMap<Usuario, UsuarioUpdateViewModel>()
                .ForMember(dest => dest.IdUsuario, opt => opt.MapFrom(src => src.Id));

            CreateMap<Permiso, PermisosDTO>()
                .ForMember(d => d.Nombre, o => o.MapFrom(s => s.IdUsuarioSolicitaNavigation.Nombre ?? "N/A"))
                .ForMember(d => d.TipoPermiso, o => o.MapFrom(s => s.IdTipoPermisoNavigation.Descripcion ?? "N/A"))
                .ForMember(d => d.EstadoDescripcion, o => o.MapFrom(s => s.IdEstadoPermisoNavigation.Descripcion ?? "N/A"))
                .ForMember(d => d.EstadoColor, o => o.MapFrom(s => s.IdEstadoPermisoNavigation.Color ?? "secondary"));

            CreateMap<Permiso, PermissionsCreateViewModel>()
                .ForMember(dest => dest.Fecha1Date, opt => opt.MapFrom(src => src.Fecha1))
                .ForMember(dest => dest.Hora1Time, opt => opt.MapFrom(src => src.Fecha1.HasValue ? src.Fecha1.Value.ToString("HH:mm") : null))
                .ForMember(dest => dest.Fecha2Date, opt => opt.MapFrom(src => src.Fecha2))
                .ForMember(dest => dest.Hora2Time, opt => opt.MapFrom(src => src.Fecha2.HasValue ? src.Fecha2.Value.ToString("HH:mm") : null))
                .ReverseMap()
                .ForMember(dest => dest.Evidencia, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Evidencia)));

            // 4. Listados
            CreateMap<Corte, CortesDTO>()
                .ForMember(d => d.EstadoDescripcion, o => o.MapFrom(s => s.IdEstadoCorteNavigation.Descripcion ?? "N/A"))
                .ForMember(d => d.EstadoColor, o => o.MapFrom(s => s.IdEstadoCorteNavigation.Color ?? "secondary"));
            CreateMap<Usuario, UsuariosDTO>();
            CreateMap<Usuario, UsuariosListDTO>()
                .ForMember(d => d.Departamento, o => o.MapFrom(s => s.IdDepartamentoNavigation.Descripcion ?? "N/A"));

           // 5. Areas
            CreateMap<RecursosHumanosWeb.Models.ViewModels.Areas.AreaCreateViewModel, Area>();
            CreateMap<RecursosHumanosWeb.Models.ViewModels.Areas.AreaUpdateViewModel, Area>();

            // 6. Departamentos
            CreateMap<RecursosHumanosWeb.Models.ViewModels.Departamentos.DepartamentoCreateViewModel, Departamento>();
            CreateMap<RecursosHumanosWeb.Models.ViewModels.Departamentos.DepartamentoUpdateViewModel, Departamento>();

            // 7. Puestos
            CreateMap<RecursosHumanosWeb.Models.ViewModels.Puestos.PuestoCreateViewModel, Puesto>();
            CreateMap<RecursosHumanosWeb.Models.ViewModels.Puestos.PuestoUpdateViewModel, Puesto>();

            // 8. TipoPermisos
            CreateMap<RecursosHumanosWeb.Models.ViewModels.TipoPermisos.TipoPermisoCreateViewModel, TipoPermiso>();
            CreateMap<RecursosHumanosWeb.Models.ViewModels.TipoPermisos.TipoPermisoUpdateViewModel, TipoPermiso>();

            // 9. TipoUsuarios
            CreateMap<RecursosHumanosWeb.Models.ViewModels.TipoUsuarios.TipoUsuarioCreateViewModel, TipoUsuario>();
            CreateMap<RecursosHumanosWeb.Models.ViewModels.TipoUsuarios.TipoUsuarioUpdateViewModel, TipoUsuario>();

            // 10. Cortes (API)
            CreateMap<RecursosHumanosWeb.Models.ViewModels.Cortes.CorteApiCreateViewModel, Corte>();
            CreateMap<RecursosHumanosWeb.Models.ViewModels.Cortes.CorteApiUpdateViewModel, Corte>();

            // 11. Permisos (API)
            CreateMap<RecursosHumanosWeb.Models.ViewModels.Permisos.PermisoApiCreateViewModel, Permiso>();
            CreateMap<RecursosHumanosWeb.Models.ViewModels.Permisos.PermisoApiUpdateViewModel, Permiso>();

            // 12. Auditoria
            CreateMap<Auditoria, AuditoriaDTO>()
                .ForMember(d => d.Tabla, o => o.MapFrom(s => s.IdTablaNavigation.Descripcion))
                .ForMember(d => d.Accion, o => o.MapFrom(s => s.IdApiFunctionNavigation.Descripcion))
                .ForMember(d => d.Usuario, o => o.MapFrom(s => s.IdUsuarioCreaNavigation.Nombre))
                .ForMember(d => d.ApiKeyDesc, o => o.MapFrom(s => s.IdApiKeyNavigation != null ? (s.IdApiKeyNavigation.Clave.Length >= 8 ? s.IdApiKeyNavigation.Clave.Substring(0, 8) + "..." : s.IdApiKeyNavigation.Clave) : "N/A"))
                .ForMember(d => d.TipoOrigenDesc, o => o.MapFrom(s => s.IdTipoOrigenNavigation.Descripcion));
        }
    }
}
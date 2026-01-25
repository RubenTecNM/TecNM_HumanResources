using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Filters;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.ViewModels.Permisos;

namespace RecursosHumanosWeb.Controllers.Api.Permisos
{
    [Route("api/Permisos")]
    [ApiController]
    public class PermisosApiController : ControllerBase
    {
        private readonly RecursosHumanosContext _context;
        private readonly IMapper _mapper;

        public PermisosApiController(RecursosHumanosContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Permisos
        [HttpGet]
        [ApiKeyAuthorize(TablaObjetivo = "Permiso", PermisoRequerido = "Leer")]
        public async Task<IActionResult> GetPermisos(int page = 1, int pageSize = 10)
        {
            var permisos = await _context.Permisos
                .Include(p => p.IdUsuarioSolicitaNavigation)
                .Include(p => p.IdTipoPermisoNavigation)
                .Where(p => p.Estatus == true)
                .OrderByDescending(p => p.FechaCreacion)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    Solicitante = p.IdUsuarioSolicitaNavigation.Nombre,
                    Tipo = p.IdTipoPermisoNavigation.Descripcion,
                    p.Fecha1,
                    p.Fecha2,
                    p.Motivo,
                    p.Revisado,
                    p.Goce
                })
                .ToListAsync();

            return Ok(permisos);
        }

        // GET: api/Permisos/5
        [HttpGet("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "Permiso", PermisoRequerido = "Leer")]
        public async Task<IActionResult> GetPermiso(int id)
        {
            var permiso = await _context.Permisos
                .Include(p => p.IdUsuarioSolicitaNavigation)
                .Include(p => p.IdTipoPermisoNavigation)
                .Include(p => p.IdCorteNavigation)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (permiso == null) return NotFound($"Permiso con ID {id} no encontrado.");

            // Mapeo manual rápido o DTO
            var result = new
            {
                permiso.Id,
                Solicitante = permiso.IdUsuarioSolicitaNavigation.Nombre,
                Tipo = permiso.IdTipoPermisoNavigation.Descripcion,
                permiso.Fecha1,
                permiso.Fecha2,
                permiso.Motivo,
                permiso.Evidencia, // URL relativa
                permiso.Revisado,
                permiso.Goce,
                permiso.FechaCreacion,
                Corte = permiso.IdCorteNavigation.Inicia.ToString("yyyy-MM-dd") + " - " + permiso.IdCorteNavigation.Termina.ToString("yyyy-MM-dd")
            };

            return Ok(result);
        }

        // POST: api/Permisos
        [HttpPost]
        [ApiKeyAuthorize(TablaObjetivo = "Permiso", PermisoRequerido = "Escribir")]
        public async Task<IActionResult> PostPermiso([FromBody] PermisoApiCreateViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = HttpContext.Items["ApiUserId"] as int? ?? 1;
            
            // Asignar solicitante: si viene en el modelo y es válido, usarlo. Si no, usar el usuario de la API Key (si hace sentido, o error)
            // Aquí asumiremos que el usuario de la API Key puede solicitar para sí mismo, o si es Admin, para otros.
            // Para simplificar: si IdUsuarioSolicita es null, usamos userId.
            int solicitanteId = model.IdUsuarioSolicita ?? userId;

            // Encontrar Corte activo
            var corteActivo = await _context.Cortes
                .Where(c => c.Estatus)
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync();

            if (corteActivo == null) return BadRequest("No hay un corte activo para registrar el permiso.");

            // Calcular días (lógica simple)
            int? dias = null;
            if (model.Fecha1 != default && model.Fecha2.HasValue)
            {
                dias = (int)((model.Fecha2.Value - model.Fecha1).TotalDays + 1);
            }

            var permiso = new Permiso
            {
                Motivo = model.Motivo,
                Fecha1 = model.Fecha1,
                Fecha2 = model.Fecha2,
                IdTipoPermiso = model.IdTipoPermiso,
                IdUsuarioSolicita = solicitanteId,
                IdCorte = corteActivo.Id,
                Goce = model.Goce,
                Estatus = true,
                Revisado = false,
                Dias = dias,
                FechaCreacion = DateTime.Now,
                FechaModificacion = DateTime.Now,
                IdUsuarioCrea = userId,
                IdUsuarioModifica = userId
            };

            _context.Permisos.Add(permiso);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPermiso), new { id = permiso.Id }, new { permiso.Id });
        }

        // PUT: api/Permisos/5
        [HttpPut("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "Permiso", PermisoRequerido = "Actualizar")]
        public async Task<IActionResult> PutPermiso(int id, [FromBody] PermisoApiUpdateViewModel model)
        {
            if (id != model.Id) return BadRequest("El ID de la URL no coincide con el del cuerpo.");

            var permiso = await _context.Permisos.FindAsync(id);
            if (permiso == null) return NotFound();

            if (permiso.Revisado == true) return BadRequest("No se puede modificar un permiso ya revisado.");

            var userId = HttpContext.Items["ApiUserId"] as int? ?? permiso.IdUsuarioModifica;

            // Actualizar campos
            permiso.Motivo = model.Motivo;
            permiso.Fecha1 = model.Fecha1;
            permiso.Fecha2 = model.Fecha2;
            permiso.IdTipoPermiso = model.IdTipoPermiso;
            permiso.Goce = model.Goce;
            
            permiso.FechaModificacion = DateTime.Now;
            permiso.IdUsuarioModifica = userId;

             // Recalcular días
            if (permiso.Fecha1.HasValue && permiso.Fecha2.HasValue)
            {
                permiso.Dias = (int)((permiso.Fecha2.Value - permiso.Fecha1.Value).TotalDays + 1);
            }

            _context.Update(permiso);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Permisos/5
        [HttpDelete("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "Permiso", PermisoRequerido = "Eliminar")]
        public async Task<IActionResult> DeletePermiso(int id)
        {
            var permiso = await _context.Permisos.FindAsync(id);
            if (permiso == null) return NotFound();

            if (!permiso.Estatus) return BadRequest("El permiso ya está inactivo.");

            var userId = HttpContext.Items["ApiUserId"] as int? ?? permiso.IdUsuarioModifica;

            permiso.Estatus = false;
            permiso.FechaModificacion = DateTime.Now;
            permiso.IdUsuarioModifica = userId;

            _context.Update(permiso);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

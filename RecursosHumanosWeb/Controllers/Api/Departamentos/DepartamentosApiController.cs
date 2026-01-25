using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Filters;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.ViewModels.Departamentos;

namespace RecursosHumanosWeb.Controllers.Api.Departamentos
{
    [Route("api/Departamentos")]
    [ApiController]
    public class DepartamentosApiController : ControllerBase
    {
        private readonly RecursosHumanosContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<DepartamentosApiController> _logger;

        public DepartamentosApiController(RecursosHumanosContext context, IMapper mapper, ILogger<DepartamentosApiController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/Departamentos
        [HttpGet]
        [ApiKeyAuthorize(TablaObjetivo = "Departamento", PermisoRequerido = "Leer")]
        public async Task<IActionResult> GetDepartamentos()
        {
            var departamentos = await _context.Departamentos
                .Where(d => d.Estatus == true)
                .Select(d => new
                {
                    d.Id,
                    d.Descripcion
                })
                .ToListAsync();

            if (!departamentos.Any()) return NotFound("No se encontraron departamentos activos.");

            return Ok(departamentos);
        }

        // GET: api/Departamentos/5
        [HttpGet("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "Departamento", PermisoRequerido = "Leer")]
        public async Task<IActionResult> GetDepartamento(int id)
        {
            var departamento = await _context.Departamentos
                .Where(d => d.Id == id)
                .Select(d => new
                {
                    d.Id,
                    d.Descripcion,
                    d.Estatus,
                    d.FechaCreacion,
                    d.FechaModificacion,
                    CreadoPor = d.IdUsuarioCreaNavigation.Nombre,
                    ModificadoPor = d.IdUsuarioModificaNavigation.Nombre
                })
                .FirstOrDefaultAsync();

            if (departamento == null) return NotFound($"Departamento con ID {id} no encontrado.");

            return Ok(departamento);
        }

        // POST: api/Departamentos
        [HttpPost]
        [ApiKeyAuthorize(TablaObjetivo = "Departamento", PermisoRequerido = "Escribir")]
        public async Task<IActionResult> PostDepartamento([FromBody] DepartamentoCreateViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = HttpContext.Items["ApiUserId"] as int? ?? 1;

            var departamento = _mapper.Map<Departamento>(model);
            departamento.Estatus = true;
            departamento.FechaCreacion = DateTime.Now;
            departamento.FechaModificacion = DateTime.Now;
            departamento.IdUsuarioCrea = userId;
            departamento.IdUsuarioModifica = userId;

            try
            {
                _context.Departamentos.Add(departamento);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetDepartamento), new { id = departamento.Id }, new { departamento.Id, departamento.Descripcion });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "No inner exception";
                _logger.LogError(ex, "Error al crear departamento vía API. Detalle: {Inner}", inner);
                return StatusCode(500, new { error = "Internal Server Error", message = ex.Message, detail = inner });
            }
        }

        // PUT: api/Departamentos/5
        [HttpPut("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "Departamento", PermisoRequerido = "Actualizar")]
        public async Task<IActionResult> PutDepartamento(int id, [FromBody] DepartamentoUpdateViewModel model)
        {
            if (id != model.Id) return BadRequest("El ID de la URL no coincide con el del cuerpo.");

            var departamento = await _context.Departamentos.FindAsync(id);
            if (departamento == null) return NotFound();

            var userId = HttpContext.Items["ApiUserId"] as int? ?? departamento.IdUsuarioModifica;

            _mapper.Map(model, departamento);
            departamento.FechaModificacion = DateTime.Now;
            departamento.IdUsuarioModifica = userId;

            try
            {
                _context.Update(departamento);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "No inner exception";
                _logger.LogError(ex, "Error al actualizar departamento {Id} vía API. Detalle: {Inner}", id, inner);
                return StatusCode(500, new { error = "Internal Server Error", message = ex.Message, detail = inner });
            }
        }

        // DELETE: api/Departamentos/5
        [HttpDelete("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "Departamento", PermisoRequerido = "Eliminar")]
        public async Task<IActionResult> DeleteDepartamento(int id)
        {
            var departamento = await _context.Departamentos.FindAsync(id);
            if (departamento == null) return NotFound();

            if (!departamento.Estatus) return BadRequest("El departamento ya está inactivo.");

            var userId = HttpContext.Items["ApiUserId"] as int? ?? departamento.IdUsuarioModifica;

            departamento.Estatus = false;
            departamento.FechaModificacion = DateTime.Now;
            departamento.IdUsuarioModifica = userId;

            _context.Update(departamento);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

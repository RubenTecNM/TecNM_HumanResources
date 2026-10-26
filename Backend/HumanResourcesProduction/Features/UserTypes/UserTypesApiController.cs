using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HumanResourcesProduction.Filters;
using HumanResourcesProduction.Models;
using HumanResourcesProduction.Models.ViewModels.UserTypes;

namespace HumanResourcesProduction.Features.UserTypes
{
    [Route("api/UserTypes")]
    [ApiController]
    public class UserTypesApiController : ControllerBase
    {
        private readonly HumanResourcesContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<UserTypesApiController> _logger;

        public UserTypesApiController(HumanResourcesContext context, IMapper mapper, ILogger<UserTypesApiController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/UserTypes
        [HttpGet]
        [ApiKeyAuthorize(TargetTable = "UserType", RequiredPermission = "Leer")]
        public async Task<IActionResult> GetUserTypes()
        {
            var userTypes = await _context.UserTypes
                .Where(t => t.Status == true)
                .Select(t => new
                {
                    t.Id,
                    t.Description
                })
                .ToListAsync();

            if (!userTypes.Any()) return NotFound("No se encontraron tipos de usuario activos.");

            return Ok(userTypes);
        }

        // GET: api/UserTypes/5
        [HttpGet("{id}")]
        [ApiKeyAuthorize(TargetTable = "UserType", RequiredPermission = "Leer")]
        public async Task<IActionResult> GetUserType(int id)
        {
            var userType = await _context.UserTypes
                .Where(t => t.Id == id)
                .Select(t => new
                {
                    t.Id,
                    t.Description,
                    t.Status,
                    t.CreatedAt,
                    t.UpdatedAt,
                    CreatedBy = t.CreatedByUser.Name,
                    UpdatedBy = t.UpdatedByUser.Name
                })
                .FirstOrDefaultAsync();

            if (userType == null) return NotFound($"Tipo de usuario con ID {id} no encontrado.");

            return Ok(userType);
        }

        // POST: api/UserTypes
        [HttpPost]
        [ApiKeyAuthorize(TargetTable = "UserType", RequiredPermission = "Escribir")]
        public async Task<IActionResult> PostUserType([FromBody] UserTypeCreateViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = HttpContext.Items["ApiUserId"] as int? ?? 1;

            var userType = _mapper.Map<UserType>(model);
            userType.Status = true;
            userType.CreatedAt = DateTime.Now;
            userType.UpdatedAt = DateTime.Now;
            userType.CreatedByUserId = userId;
            userType.UpdatedByUserId = userId;

            try
            {
                _context.UserTypes.Add(userType);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUserType), new { id = userType.Id }, new { userType.Id, userType.Description });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "No inner exception";
                _logger.LogError(ex, "Error al crear tipo de usuario vía API. Detalle: {Inner}", inner);
                return StatusCode(500, new { error = "Internal Server Error", message = ex.Message, detail = inner });
            }
        }

        // PUT: api/UserTypes/5
        [HttpPut("{id}")]
        [ApiKeyAuthorize(TargetTable = "UserType", RequiredPermission = "Actualizar")]
        public async Task<IActionResult> PutUserType(int id, [FromBody] UserTypeUpdateViewModel model)
        {
            if (id != model.Id) return BadRequest("El ID de la URL no coincide con el del cuerpo.");

            var userType = await _context.UserTypes.FindAsync(id);
            if (userType == null) return NotFound();

            var userId = HttpContext.Items["ApiUserId"] as int? ?? userType.UpdatedByUserId;

            _mapper.Map(model, userType);
            userType.UpdatedAt = DateTime.Now;
            userType.UpdatedByUserId = userId;

            try
            {
                _context.Update(userType);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "No inner exception";
                _logger.LogError(ex, "Error al actualizar tipo de usuario {Id} vía API. Detalle: {Inner}", id, inner);
                return StatusCode(500, new { error = "Internal Server Error", message = ex.Message, detail = inner });
            }
        }

        // PUT: api/UserTypes/5/deactivate  (borrado LÓGICO: no existe DELETE físico)
        [HttpPut("{id}/deactivate")]
        [ApiKeyAuthorize(TargetTable = "UserType", RequiredPermission = "Eliminar")]
        public async Task<IActionResult> DeactivateUserType(int id)
        {
            var userType = await _context.UserTypes.FindAsync(id);
            if (userType == null) return NotFound();

            if (!userType.Status) return BadRequest("El tipo de usuario ya está inactivo.");

            var userId = HttpContext.Items["ApiUserId"] as int? ?? userType.UpdatedByUserId;

            userType.Status = false;
            userType.UpdatedAt = DateTime.Now;
            userType.UpdatedByUserId = userId;

            _context.Update(userType);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Filters;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.ViewModels.User;
using RecursosHumanosWeb.Helpers;
using AutoMapper.QueryableExtensions;

namespace RecursosHumanosWeb.Controllers.Api
{
    // Heredamos de ControllerBase para API
    [Route("api/Users")]
    [ApiController]
    public class UsuariosApiController : ControllerBase
    {
        private readonly HumanResourcesContext _context;
        private readonly AutoMapper.IMapper _mapper;

        public UsuariosApiController(HumanResourcesContext context, AutoMapper.IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // --- ACCIONES DE LECTURA (GET) ---

        // Simula la obtención de la lista (Similar a Index de tu MVC Controller)
        [HttpGet]
        [ApiKeyAuthorize(TablaObjetivo = "User", PermisoRequerido = "Leer")]
        public async Task<IActionResult> GetUsuarios()
        {
            // Usar AutoMapper ProjectTo para proyectar eficientemente desde la BD
            var users = await _context.Users
                .Where(u => u.Status == true)
                .ProjectTo<RecursosHumanosWeb.Models.DTOs.UsersListDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            if (!users.Any()) return NotFound("No se encontraron users activos.");

            return Ok(users);
        }

        // Obtiene detalles de un user (Similar a Details de tu MVC Controller)
        [HttpGet("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "User", PermisoRequerido = "Leer")]
        public async Task<IActionResult> GetUsuario(int id)
        {
            // Proyectar a un DTO de detalle si necesitas más campos o usar el ViewModel
            var user = await _context.Users
                .Where(u => u.Id == id)
                .ProjectTo<RecursosHumanosWeb.Models.ViewModels.User.UserDetailsViewModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (user == null) return NotFound($"User con ID {id} no encontrado.");

            return Ok(user);
        }

        // --- ACCIONES DE ESCRITURA/CREACIÓN (POST) ---

        // Crea un nuevo user (Similar a POST Create de tu MVC Controller)
        [HttpPost]
        [ApiKeyAuthorize(TablaObjetivo = "User", PermisoRequerido = "Escribir")]
        public async Task<IActionResult> PostUsuario([FromBody] UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Aquí debes asegurarte de que la lógica de creación refleje la API Key.
            var userId = HttpContext.Items["ApiUserId"] as int? ?? 1; // ID del user de la API Key

            var newUser = new User
            {
                Name = model.Name,
                Email = model.Email,
                DepartmentId = model.DepartmentId, // Asumimos que el ViewModel trae estos IDs
                AreaId = model.AreaId,
                PositionId = model.PositionId,
                UserTypeId = model.UserTypeId,
                PasswordHash = string.IsNullOrWhiteSpace(model.Password) ? string.Empty : PasswordHasher.HashSha256(model.Password),
                Status = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedByUserId = userId, // Usamos el ID de la API Key
                UpdatedByUserId = userId
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            // Devolvemos el 201 CreatedAtAction (estándar REST)
            // Proyectar el nuevo user a UserDetailsViewModel para la respuesta
            var created = await _context.Users
                .Where(u => u.Id == newUser.Id)
                .ProjectTo<RecursosHumanosWeb.Models.ViewModels.User.UserDetailsViewModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (created != null)
            {
                return CreatedAtAction(nameof(GetUsuario), new { id = newUser.Id }, created);
            }

            // Fallback: map the entity we just saved to the details VM
            var fallback = _mapper.Map<RecursosHumanosWeb.Models.ViewModels.User.UserDetailsViewModel>(newUser);
            return CreatedAtAction(nameof(GetUsuario), new { id = newUser.Id }, fallback);
        }

        // --- ACCIONES DE ACTUALIZACIÓN (PUT) ---

        // Actualiza un user existente (Similar a POST Edit de tu MVC Controller)
        [HttpPut("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "User", PermisoRequerido = "Actualizar")]
        public async Task<IActionResult> PutUsuario(int id, [FromBody] UserUpdateViewModel model)
        {
            // CORRECCIÓN: Usar IdUsuario en lugar de Id
            if (id != model.IdUsuario || !ModelState.IsValid)
            {
                return BadRequest("El ID de la URL no coincide con el ID del modelo o el modelo no es válido.");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound($"User con ID {id} no encontrado.");
            }

            var userId = HttpContext.Items["ApiUserId"] as int? ?? user.UpdatedByUserId;
            
            // Mapear campos desde el ViewModel al entity usando AutoMapper
            _mapper.Map(model, user);
            
            // CORRECCIÓN: Hash de contraseña si se proporciona
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                // Evitar sobrescribir la clave si no se envió
                _context.Entry(user).Property(u => u.PasswordHash).IsModified = false;
            }
            else
            {
                // Hash SHA-256 antes de guardar
                user.PasswordHash = PasswordHasher.HashSha256(model.Password);
            }

            user.UpdatedAt = DateTime.Now;
            user.UpdatedByUserId = userId;

            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!_context.Users.Any(e => e.Id == id))
            {
                return NotFound($"User con ID {id} no encontrado durante la actualización.");
            }

            return NoContent(); // 204 No Content (estándar REST)
        }

        // --- ACCIONES DE ELIMINACIÓN (DELETE) ---

        // Elimina un user (Simula el POST Delete de tu MVC Controller, pero con Baja Lógica)
        [HttpDelete("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "User", PermisoRequerido = "Eliminar")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound($"User con ID {id} no encontrado.");
            }

            // Validar que el user no esté ya inactivo
            if (!user.Status)
            {
                return BadRequest("El usuario ya está inactivo.");
            }

            // Realizamos Baja Lógica (cambiar Status a false), que es la práctica recomendada.
            var userId = HttpContext.Items["ApiUserId"] as int? ?? user.UpdatedByUserId;

            user.Status = false;
            user.UpdatedAt = DateTime.Now;
            user.UpdatedByUserId = userId;

            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content
        }
    }
}

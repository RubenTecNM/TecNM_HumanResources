using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
    public class UsersApiController : ControllerBase
    {
        private readonly HumanResourcesContext _context;
        private readonly AutoMapper.IMapper _mapper;

        public UsersApiController(HumanResourcesContext context, AutoMapper.IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // --- ACCIONES DE LECTURA (GET) ---

        // GET: api/Users
        [HttpGet]
        [ApiKeyAuthorize(TargetTable = "User", RequiredPermission = "Leer")]
        public async Task<IActionResult> GetUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? name = null,
            [FromQuery] string? email = null,
            [FromQuery] int? departmentId = null,
            [FromQuery] bool? status = null,
            [FromQuery] string? sortOrder = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var query = _context.Users
                .Include(u => u.Department)
                .Include(u => u.Area)
                .Include(u => u.Position)
                .Include(u => u.UserType)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(u => u.Name.Contains(name));

            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(u => u.Email.Contains(email));

            if (departmentId.HasValue)
                query = query.Where(u => u.DepartmentId == departmentId.Value);

            if (status.HasValue)
                query = query.Where(u => u.Status == status.Value);

            // Ordenamiento (mismas opciones que el Index legacy)
            query = sortOrder switch
            {
                "nombre_desc" => query.OrderByDescending(u => u.Name ?? ""),
                "correo" => query.OrderBy(u => u.Email ?? ""),
                "correo_desc" => query.OrderByDescending(u => u.Email ?? ""),
                "department" => query.OrderBy(u => u.Department == null ? "" : (u.Department.Description ?? "")),
                "departamento_desc" => query.OrderByDescending(u => u.Department == null ? "" : (u.Department.Description ?? "")),
                "fecha" => query.OrderBy(u => u.CreatedAt),
                "fecha_desc" => query.OrderByDescending(u => u.CreatedAt),
                _ => query.OrderBy(u => u.Name ?? "")
            };

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.Status,
                    u.DepartmentId,
                    Department = u.Department != null ? u.Department.Description : null,
                    u.AreaId,
                    Area = u.Area != null ? u.Area.Description : null,
                    u.PositionId,
                    Position = u.Position != null ? u.Position.Description : null,
                    u.UserTypeId,
                    UserType = u.UserType != null ? u.UserType.Description : null,
                    u.CreatedAt,
                    u.UpdatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                items,
                totalItems,
                totalPages,
                page,
                pageSize
            });
        }

        // Obtiene detalles de un user (Similar a Details de tu MVC Controller)
        [HttpGet("{id}")]
        [ApiKeyAuthorize(TargetTable = "User", RequiredPermission = "Leer")]
        public async Task<IActionResult> GetUser(int id)
        {
            // Proyectar a un DTO de detalle si necesitas más campos o usar el ViewModel
            var user = await _context.Users
                .Where(u => u.Id == id)
                .ProjectTo<RecursosHumanosWeb.Models.ViewModels.User.UserDetailsViewModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (user == null) return NotFound($"User con ID {id} no encontrado.");

            return Ok(user);
        }

        // Buscador global de usuarios (Select2-like). Autentica por cookie de sesión
        // (policy RHOrAdmin, como el UserController.SearchUsers original) y NO por
        // API Key, para que el SPA lo consuma tras iniciar sesión sin exponer la key.
        // Filtra por nombre/correo, solo activos, top 20.
        [HttpGet("search")]
        [Authorize(Policy = "RHOrAdmin")]
        public async Task<IActionResult> SearchUsers([FromQuery] string? term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Ok(Array.Empty<object>());

            var matches = await _context.Users
                .Include(u => u.Department)
                .Where(u => u.Status && (u.Name.Contains(term) || u.Email.Contains(term)))
                .OrderBy(u => u.Name)
                .Take(20)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    Department = u.Department != null ? u.Department.Description : string.Empty
                })
                .ToListAsync();

            return Ok(matches);
        }

        // --- ACCIONES DE ESCRITURA/CREACIÓN (POST) ---

        // Crea un nuevo user (Similar a POST Create de tu MVC Controller)
        [HttpPost]
        [ApiKeyAuthorize(TargetTable = "User", RequiredPermission = "Escribir")]
        public async Task<IActionResult> PostUser([FromBody] UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Aquí debes asegurarte de que la lógica de creación refleje la API Key.
            var userId = HttpContext.Items["ApiUserId"] as int? ?? 1; // ID del user de la API Key

            // Regla de negocio: solo Admin/RH gestionan usuarios
            if (!await IsManagerAsync(userId))
                return StatusCode(403, new { error = "Forbidden", message = "Solo RH o administradores pueden crear usuarios." });

            // Regla de negocio: correo único
            var email = model.Email?.Trim();
            if (!string.IsNullOrEmpty(email) && await _context.Users.AnyAsync(u => u.Email == email))
                return BadRequest(new { message = "Ya existe un usuario registrado con ese correo." });

            var newUser = new User
            {
                Name = model.Name,
                Email = email,
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
                return CreatedAtAction(nameof(GetUser), new { id = newUser.Id }, created);
            }

            // Fallback: map the entity we just saved to the details VM
            var fallback = _mapper.Map<RecursosHumanosWeb.Models.ViewModels.User.UserDetailsViewModel>(newUser);
            return CreatedAtAction(nameof(GetUser), new { id = newUser.Id }, fallback);
        }

        // --- ACCIONES DE ACTUALIZACIÓN (PUT) ---

        // Actualiza un user existente (Similar a POST Edit de tu MVC Controller)
        [HttpPut("{id}")]
        [ApiKeyAuthorize(TargetTable = "User", RequiredPermission = "Actualizar")]
        public async Task<IActionResult> PutUser(int id, [FromBody] UserUpdateViewModel model)
        {
            // CORRECCIÓN: Usar Id en lugar de IdUsuario
            if (id != model.Id || !ModelState.IsValid)
            {
                return BadRequest("El ID de la URL no coincide con el ID del modelo o el modelo no es válido.");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound($"User con ID {id} no encontrado.");
            }

            var userId = HttpContext.Items["ApiUserId"] as int? ?? user.UpdatedByUserId;

            // Regla de negocio: solo Admin/RH gestionan usuarios
            if (!await IsManagerAsync(userId))
                return StatusCode(403, new { error = "Forbidden", message = "Solo RH o administradores pueden actualizar usuarios." });

            // Regla de negocio: correo único (excluyendo al propio usuario)
            var email = model.Email?.Trim();
            if (!string.IsNullOrEmpty(email) && await _context.Users.AnyAsync(u => u.Id != id && u.Email == email))
                return BadRequest(new { message = "Ya existe otro usuario registrado con ese correo." });

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
                user.TempPassword = model.Password; // visible para RH hasta que el usuario ingrese
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

        // --- ACCIONES DE ELIMINACIÓN LÓGICA (PUT, NO DELETE FÍSICO) ---
        // Por política: no existen DELETE reales en la API. El borrado es SIEMPRE lógico
        // (soft delete) a través de PUT. No se elimina el registro de la base de datos.

        // Elimina lógicamente un user (cambia Status a false = Baja Lógica).
        [HttpPut("{id}/deactivate")]
        [ApiKeyAuthorize(TargetTable = "User", RequiredPermission = "Eliminar")]
        public async Task<IActionResult> DeactivateUser(int id)
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

            // Regla de negocio: nadie puede darse de baja a sí mismo
            if (id == userId)
                return BadRequest("No puede darse de baja a sí mismo.");

            user.Status = false;
            user.UpdatedAt = DateTime.Now;
            user.UpdatedByUserId = userId;

            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content
        }

        // Reactiva lógicamente un user (cambia Status a true).
        [HttpPut("{id}/activate")]
        [ApiKeyAuthorize(TargetTable = "User", RequiredPermission = "Actualizar")]
        public async Task<IActionResult> ActivateUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound($"User con ID {id} no encontrado.");
            }

            if (user.Status)
            {
                return BadRequest("El usuario ya está activo.");
            }

            var userId = HttpContext.Items["ApiUserId"] as int? ?? user.UpdatedByUserId;

            // Regla de negocio: solo Admin/RH gestionan usuarios
            if (!await IsManagerAsync(userId))
                return StatusCode(403, new { error = "Forbidden", message = "Solo RH o administradores pueden reactivar usuarios." });

            user.Status = true;
            user.UpdatedAt = DateTime.Now;
            user.UpdatedByUserId = userId;

            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content
        }

        private async Task<bool> IsManagerAsync(int? userId)
        {
            var userType = await _context.Users.AsNoTracking()
                .Where(u => u.Id == (userId ?? 0))
                .Select(u => u.UserTypeId)
                .FirstOrDefaultAsync();

            return userType == (int)RecursosHumanosWeb.Models.Enums.UserTypeEnum.Administrator ||
                   userType == (int)RecursosHumanosWeb.Models.Enums.UserTypeEnum.HumanResources;
        }
    }
}

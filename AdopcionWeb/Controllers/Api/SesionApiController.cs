using Aplicacion.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AdopcionWeb.Controllers.Api
{
    public record RegistroDto(string? NombreUsuario, string? Correo, string? Contrasena);
    public record LoginDto(string? NombreUsuario, string? Contrasena);

    [ApiController]
    [Route("api/[controller]")]
    public class SesionApiController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public SesionApiController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        // GET: api/SesionApi -> estado de la sesión actual
        [HttpGet]
        public ActionResult Obtener()
        {
            var usuario = HttpContext.Session.GetString("Usuario");
            return Ok(new
            {
                activa = !string.IsNullOrEmpty(usuario),
                usuario,
                rol = HttpContext.Session.GetString("Rol"),
                correo = HttpContext.Session.GetString("Correo"),
                inicio = HttpContext.Session.GetString("InicioSesion")
            });
        }

        // POST: api/SesionApi/registrar -> crea cuenta de adoptante e inicia sesión
        [HttpPost("registrar")]
        public async Task<ActionResult> Registrar(RegistroDto datos)
        {
            try
            {
                // Validar que no sean nulos o vacíos
                if (string.IsNullOrWhiteSpace(datos.NombreUsuario))
                    return BadRequest(new { mensaje = "El nombre de usuario es requerido." });

                if (string.IsNullOrWhiteSpace(datos.Correo))
                    return BadRequest(new { mensaje = "El correo es requerido." });

                if (string.IsNullOrWhiteSpace(datos.Contrasena))
                    return BadRequest(new { mensaje = "La contraseña es requerida." });

                var usuario = await _usuarioService.RegistrarAsync(
                    datos.NombreUsuario.Trim(), 
                    datos.Correo.Trim(), 
                    datos.Contrasena);

                GuardarSesion(usuario.UsuarioId, usuario.NombreUsuario, usuario.Correo, usuario.Rol);
                return Ok(new { activa = true, usuario = usuario.NombreUsuario, rol = usuario.Rol });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { mensaje = "Error al registrar usuario. Intenta nuevamente." });
            }
        }

        // POST: api/SesionApi/iniciar -> valida usuario y contraseña
        [HttpPost("iniciar")]
        public async Task<ActionResult> Iniciar(LoginDto datos)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(datos.NombreUsuario))
                    return BadRequest(new { mensaje = "El nombre de usuario es requerido." });

                if (string.IsNullOrWhiteSpace(datos.Contrasena))
                    return BadRequest(new { mensaje = "La contraseña es requerida." });

                var usuario = await _usuarioService.IniciarSesionAsync(
                    datos.NombreUsuario.Trim(), 
                    datos.Contrasena);

                if (usuario == null)
                {
                    // Mensaje genérico: no revela si el usuario existe
                    return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos." });
                }

                GuardarSesion(usuario.UsuarioId, usuario.NombreUsuario, usuario.Correo, usuario.Rol);
                return Ok(new { activa = true, usuario = usuario.NombreUsuario, rol = usuario.Rol });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { mensaje = "Error al iniciar sesión. Intenta nuevamente." });
            }
        }

        // POST: api/SesionApi/cerrar -> borra todo lo guardado en la sesión
        [HttpPost("cerrar")]
        public ActionResult Cerrar()
        {
            HttpContext.Session.Clear();
            return Ok(new { activa = false });
        }

        // En la sesión solo se guarda id, nombre, correo y rol; nunca la contraseña ni su hash.
        // El id sirve para relacionar solicitudes y donaciones con la tabla Usuarios.
        private void GuardarSesion(int usuarioId, string nombre, string correo, string rol)
        {
            HttpContext.Session.Clear();
            HttpContext.Session.SetInt32("UsuarioId", usuarioId);
            HttpContext.Session.SetString("Usuario", nombre);
            HttpContext.Session.SetString("Correo", correo);
            HttpContext.Session.SetString("Rol", rol);
            HttpContext.Session.SetString("InicioSesion", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }
}

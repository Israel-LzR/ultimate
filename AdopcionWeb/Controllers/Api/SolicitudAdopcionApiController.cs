using AdopcionWeb.Filters;
using Aplicacion.Interfaces;
using Dominio.Entidades;
using Microsoft.AspNetCore.Mvc;

namespace AdopcionWeb.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class SolicitudAdopcionApiController : ControllerBase
    {
        private readonly ISolicitudAdopcionService _solicitudService;

        public SolicitudAdopcionApiController(ISolicitudAdopcionService solicitudService)
        {
            _solicitudService = solicitudService;
        }

        // GET: api/SolicitudAdopcionApi  (solo Admin: contiene datos personales)
        [RequiereSesion(Rol = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<SolicitudAdopcion>>> ObtenerTodos()
        {
            var solicitudes = await _solicitudService.ObtenerTodosAsync();
            return Ok(solicitudes);
        }

        // GET: api/SolicitudAdopcionApi/buscar?texto=ana&estado=Pendiente  (solo Admin)
        [RequiereSesion(Rol = "Admin")]
        [HttpGet("buscar")]
        public async Task<ActionResult<List<SolicitudAdopcion>>> Buscar([FromQuery] string? texto, [FromQuery] string? estado)
        {
            var solicitudes = await _solicitudService.BuscarAsync(texto, estado);
            return Ok(solicitudes);
        }

        // GET: api/SolicitudAdopcionApi/mias  (cualquier usuario con sesión ve solo las suyas)
        [RequiereSesion]
        [HttpGet("mias")]
        public async Task<ActionResult<List<SolicitudAdopcion>>> ObtenerMias()
        {
            var correo = HttpContext.Session.GetString("Correo") ?? "";
            var todas = await _solicitudService.ObtenerTodosAsync();

            var mias = todas
                .Where(s => string.Equals(s.Correo, correo, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Ok(mias);
        }

        [RequiereSesion(Rol = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<SolicitudAdopcion>> ObtenerPorId(int id)
        {
            var solicitud = await _solicitudService.ObtenerPorIdAsync(id);

            if (solicitud == null)
            {
                return NotFound();
            }

            return Ok(solicitud);
        }

        // POST: cualquier usuario con sesión (Adoptante o Admin) puede pedir una adopción
        [RequiereSesion]
        [HttpPost]
        public async Task<ActionResult> Agregar(SolicitudAdopcion solicitud)
        {
            // El correo sale de la cuenta con sesión, no del formulario: así "Mis solicitudes" siempre coincide
            solicitud.Correo = HttpContext.Session.GetString("Correo") ?? "";

            // Relación con la tabla Usuarios: también sale de la sesión, nunca del navegador
            solicitud.UsuarioId = HttpContext.Session.GetInt32("UsuarioId");

            // Cada solicitud exige haber salido APTO en el cuestionario de ese animal (queda en la sesión)
            var claveApto = "Apto_" + solicitud.AnimalId;
            var resultadoCuestionario = HttpContext.Session.GetString(claveApto);
            if (string.IsNullOrEmpty(resultadoCuestionario))
            {
                return BadRequest(new { mensaje = "Antes de enviar tu solicitud debes contestar el cuestionario y salir apto para adoptar." });
            }
            solicitud.ResultadoCuestionario = resultadoCuestionario; // lo decide el servidor, no el navegador

            await _solicitudService.AgregarAsync(solicitud);

            // Para otra solicitud (aunque sea del mismo animal) hay que contestar el cuestionario de nuevo
            HttpContext.Session.Remove(claveApto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = solicitud.SolicitudAdopcionId },
                solicitud
            );
        }

        [RequiereSesion(Rol = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Actualizar(int id, SolicitudAdopcion solicitud)
        {
            if (id != solicitud.SolicitudAdopcionId)
            {
                return BadRequest(new { mensaje = "El Id de la URL no coincide con el Id de la solicitud." });
            }

            var existente = await _solicitudService.ObtenerPorIdAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            // El admin solo cambia el estado: quién la envió y su cuestionario no se pueden alterar desde aquí
            solicitud.UsuarioId = existente.UsuarioId;
            solicitud.ResultadoCuestionario = existente.ResultadoCuestionario;

            await _solicitudService.ActualizarAsync(solicitud);

            return NoContent();
        }

        [RequiereSesion(Rol = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Eliminar(int id)
        {
            var existente = await _solicitudService.ObtenerPorIdAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            await _solicitudService.EliminarAsync(id);

            return NoContent();
        }
    }
}

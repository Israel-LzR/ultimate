using AdopcionWeb.Filters;
using Aplicacion.Interfaces;
using Dominio.Entidades;
using Microsoft.AspNetCore.Mvc;

namespace AdopcionWeb.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class DonacionApiController : ControllerBase
    {
        private readonly IDonacionService _donacionService;
        private readonly IConfiguration _configuracion;

        public DonacionApiController(IDonacionService donacionService, IConfiguration configuracion)
        {
            _donacionService = donacionService;
            _configuracion = configuracion;
        }

        // GET: api/DonacionApi
        [RequiereSesion(Rol = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<Donacion>>> ObtenerTodos()
        {
            var donaciones = await _donacionService.ObtenerTodosAsync();
            return Ok(donaciones);
        }

        // GET: api/DonacionApi/resumen  (solo Admin)
        // Fondo del refugio = saldo inicial (tabla Fondos; "Donaciones:SaldoInicial" de appsettings.json es solo respaldo) + todo lo donado.
        [RequiereSesion(Rol = "Admin")]
        [HttpGet("resumen")]
        public async Task<ActionResult> ObtenerResumen()
        {
            var saldoInicial = _configuracion.GetValue<decimal>("Donaciones:SaldoInicial", 87000m);
            var resumen = await _donacionService.ObtenerResumenAsync(saldoInicial);
            return Ok(resumen);
        }

        // GET: api/DonacionApi/5
        [RequiereSesion(Rol = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Donacion>> ObtenerPorId(int id)
        {
            var donacion = await _donacionService.ObtenerPorIdAsync(id);

            if (donacion == null)
            {
                return NotFound();
            }

            return Ok(donacion);
        }

        // POST: api/DonacionApi
        [HttpPost]
        public async Task<ActionResult> Agregar(Donacion donacion)
        {
            if (string.IsNullOrWhiteSpace(donacion.NombreDonante) || donacion.Monto <= 0)
            {
                return BadRequest("Nombre y monto mayor a cero son obligatorios.");
            }

            // Si quien dona tiene sesión, la donación queda ligada a su cuenta (tabla Usuarios); sin sesión queda en NULL.
            // Se toma de la sesión, nunca del JSON que manda el navegador.
            donacion.UsuarioId = HttpContext.Session.GetInt32("UsuarioId");

            await _donacionService.AgregarAsync(donacion);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = donacion.DonacionId },
                donacion
            );
        }
    }
}

using Aplicacion.Interfaces;
using Dominio.Entidades;
using AdopcionWeb.Filters;
using Microsoft.AspNetCore.Mvc;

namespace AdopcionWeb.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class FotoAnimalApiController : ControllerBase
    {
        private readonly IFotoAnimalService _fotoService;

        public FotoAnimalApiController(IFotoAnimalService fotoService)
        {
            _fotoService = fotoService;
        }

        [HttpGet]
        public async Task<ActionResult<List<FotoAnimal>>> ObtenerTodos()
        {
            var fotos = await _fotoService.ObtenerTodosAsync();
            return Ok(fotos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FotoAnimal>> ObtenerPorId(int id)
        {
            var foto = await _fotoService.ObtenerPorIdAsync(id);

            if (foto == null)
            {
                return NotFound();
            }

            return Ok(foto);
        }

        [RequiereSesion(Rol = "Admin")]

        [HttpPost]
        public async Task<ActionResult> Agregar(FotoAnimal foto)
        {
            await _fotoService.AgregarAsync(foto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = foto.FotoAnimalId },
                foto
            );
        }

        [RequiereSesion(Rol = "Admin")]

        [HttpPut("{id}")]
        public async Task<ActionResult> Actualizar(int id, FotoAnimal foto)
        {
            if (id != foto.FotoAnimalId)
            {
                return BadRequest("El Id de la URL no coincide con el Id de la foto.");
            }

            var existente = await _fotoService.ObtenerPorIdAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            await _fotoService.ActualizarAsync(foto);

            return NoContent();
        }

        [RequiereSesion(Rol = "Admin")]

        [HttpDelete("{id}")]
        public async Task<ActionResult> Eliminar(int id)
        {
            var existente = await _fotoService.ObtenerPorIdAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            await _fotoService.EliminarAsync(id);

            return NoContent();
        }
    }
}
using Aplicacion.Interfaces;
using Dominio.Entidades;
using AdopcionWeb.Filters;
using Microsoft.AspNetCore.Mvc;

namespace AdopcionWeb.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnimalApiController : ControllerBase
    {
        private readonly IAnimalService _animalService;

        public AnimalApiController(IAnimalService animalService)
        {
            _animalService = animalService;
        }

        // GET: api/AnimalApi
        [HttpGet]
        public async Task<ActionResult<List<Animal>>> ObtenerTodos()
        {
            var animales = await _animalService.ObtenerTodosAsync();
            return Ok(animales);
        }

        // GET: api/AnimalApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Animal>> ObtenerPorId(int id)
        {
            var animal = await _animalService.ObtenerPorIdAsync(id);

            if (animal == null)
            {
                return NotFound();
            }

            return Ok(animal);
        }

        // POST: api/AnimalApi
        [RequiereSesion(Rol = "Admin")]
        [HttpPost]
        public async Task<ActionResult> Agregar(Animal animal)
        {
            await _animalService.AgregarAsync(animal);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = animal.AnimalId },
                animal
            );
        }

        // PUT: api/AnimalApi/5
        [RequiereSesion(Rol = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Actualizar(int id, Animal animal)
        {
            if (id != animal.AnimalId)
            {
                return BadRequest("El Id de la URL no coincide con el Id del animal.");
            }

            var existente = await _animalService.ObtenerPorIdAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            await _animalService.ActualizarAsync(animal);

            return NoContent();
        }

        // DELETE: api/AnimalApi/5
        [RequiereSesion(Rol = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Eliminar(int id)
        {
            var existente = await _animalService.ObtenerPorIdAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            await _animalService.EliminarAsync(id);

            return NoContent();
        }
    }
}
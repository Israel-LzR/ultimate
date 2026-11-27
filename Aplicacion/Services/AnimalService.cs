using Aplicacion.Interfaces;
using Dominio.Entidades;
using Dominio.Enumeraciones;
using Dominio.Interfaces;

namespace Aplicacion.Services
{
    public class AnimalService : IAnimalService
    {
        private readonly IAnimalRepository _animalRepository;

        public AnimalService(IAnimalRepository animalRepository)
        {
            _animalRepository = animalRepository;
        }

        public async Task<List<Animal>> ObtenerTodosAsync()
        {
            return await _animalRepository.ObtenerTodosAsync();
        }

        public async Task<Animal?> ObtenerPorIdAsync(int id)
        {
            return await _animalRepository.ObtenerPorIdAsync(id);
        }

        public async Task AgregarAsync(Animal animal)
        {
            Validar(animal);
            animal.FechaPublicacion = DateTime.Now; // la fecha la decide el servidor
            await _animalRepository.AgregarAsync(animal);
        }

        public async Task ActualizarAsync(Animal animal)
        {
            Validar(animal);
            await _animalRepository.ActualizarAsync(animal);
        }

        public async Task EliminarAsync(int id)
        {
            await _animalRepository.EliminarAsync(id);
        }

        // Reglas de negocio: se lanza ArgumentException con un mensaje claro para el usuario
        private static void Validar(Animal animal)
        {
            animal.Nombre = (animal.Nombre ?? "").Trim();
            animal.Ciudad = (animal.Ciudad ?? "").Trim();

            if (animal.Nombre.Length < 2 || animal.Nombre.Length > 60)
                throw new ArgumentException("El nombre debe tener entre 2 y 60 caracteres.");

            if (animal.Ciudad.Length == 0 || animal.Ciudad.Length > 60)
                throw new ArgumentException("La ciudad es obligatoria (máximo 60 caracteres).");

            if (animal.EdadMeses < 0 || animal.EdadMeses > 360)
                throw new ArgumentException("La edad en meses debe estar entre 0 y 360.");

            if (!Enum.IsDefined(animal.Especie) || !Enum.IsDefined(animal.Sexo) ||
                !Enum.IsDefined(animal.Tamano) || !Enum.IsDefined(animal.Estado))
                throw new ArgumentException("Especie, sexo, tamaño o estado no son válidos.");

            if (animal.Descripcion != null && animal.Descripcion.Length > 500)
                throw new ArgumentException("La descripción no puede pasar de 500 caracteres.");

            foreach (var foto in animal.Fotos)
            {
                var url = (foto.Url ?? "").Trim();
                var valida = url.StartsWith("/") || url.StartsWith("http://") || url.StartsWith("https://");
                if (!valida || url.Length > 300)
                    throw new ArgumentException("La URL de la foto debe empezar con /, http:// o https://.");
                foto.Url = url;
            }
        }
    }
}

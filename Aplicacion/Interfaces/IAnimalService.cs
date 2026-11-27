using System;
using System.Collections.Generic;
using System.Text;
using Dominio.Entidades;

namespace Aplicacion.Interfaces
{
    public interface IAnimalService
    {
        Task<List<Animal>> ObtenerTodosAsync();

        Task<Animal?> ObtenerPorIdAsync(int id);

        Task AgregarAsync(Animal animal);

        Task ActualizarAsync(Animal animal);

        Task EliminarAsync(int id);
    }
}

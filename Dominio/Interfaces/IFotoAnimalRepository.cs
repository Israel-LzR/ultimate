using System;
using System.Collections.Generic;
using System.Text;
using Dominio.Entidades;

namespace Dominio.Interfaces
{
    public interface IFotoAnimalRepository
    {
        Task<List<FotoAnimal>> ObtenerTodosAsync();

        Task<FotoAnimal?> ObtenerPorIdAsync(int id);

        Task AgregarAsync(FotoAnimal foto);

        Task ActualizarAsync(FotoAnimal foto);

        Task EliminarAsync(int id);
    }
}

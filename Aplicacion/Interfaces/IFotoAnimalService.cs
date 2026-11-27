using System;
using System.Collections.Generic;
using System.Text;
using Dominio.Entidades;

namespace Aplicacion.Interfaces
{
    public interface IFotoAnimalService
    {
        Task<List<FotoAnimal>> ObtenerTodosAsync();

        Task<FotoAnimal?> ObtenerPorIdAsync(int id);

        Task AgregarAsync(FotoAnimal foto);

        Task ActualizarAsync(FotoAnimal foto);

        Task EliminarAsync(int id);
    }
}

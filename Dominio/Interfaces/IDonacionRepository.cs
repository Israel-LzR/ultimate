using Dominio.Entidades;

namespace Dominio.Interfaces
{
    public interface IDonacionRepository
    {
        Task<List<Donacion>> ObtenerTodosAsync();

        Task<Donacion?> ObtenerPorIdAsync(int id);

        Task AgregarAsync(Donacion donacion);
    }
}

using Aplicacion.Modelos;
using Dominio.Entidades;

namespace Aplicacion.Interfaces
{
    public interface IDonacionService
    {
        Task<List<Donacion>> ObtenerTodosAsync();

        Task<Donacion?> ObtenerPorIdAsync(int id);

        Task AgregarAsync(Donacion donacion);

        Task<DonacionResumen> ObtenerResumenAsync(decimal saldoInicialRespaldo);
    }
}

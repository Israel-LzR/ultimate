using Dominio.Entidades;

namespace Dominio.Interfaces
{
    public interface IFondoRepository
    {
        // Devuelve el fondo del refugio, o null si la tabla todavía no tiene fila
        Task<Fondo?> ObtenerAsync();
    }
}

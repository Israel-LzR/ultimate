using Dominio.Entidades;

namespace Dominio.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ObtenerPorNombreAsync(string nombreUsuario);

        Task<bool> ExisteNombreOCorreoAsync(string nombreUsuario, string correo);

        Task<bool> ExisteAdminAsync();

        Task AgregarAsync(Usuario usuario);
    }
}

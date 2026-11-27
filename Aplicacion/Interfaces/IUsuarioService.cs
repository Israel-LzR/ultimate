using Dominio.Entidades;

namespace Aplicacion.Interfaces
{
    public interface IUsuarioService
    {
        // Crea una cuenta con rol Adoptante
        Task<Usuario> RegistrarAsync(string nombreUsuario, string correo, string contrasena);

        // Devuelve el usuario si las credenciales son correctas; si no, null
        Task<Usuario?> IniciarSesionAsync(string nombreUsuario, string contrasena);

        // Crea el administrador inicial solo si todavía no existe ninguno
        Task CrearAdminSiNoExisteAsync(string? nombreUsuario, string? correo, string? contrasena);
    }
}

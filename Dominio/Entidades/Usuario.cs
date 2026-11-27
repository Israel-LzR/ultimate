namespace Dominio.Entidades
{
    public class Usuario
    {
        public int UsuarioId { get; set; }

        public string NombreUsuario { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        // Nunca se guarda la contraseña: solo su hash (PBKDF2 con sal)
        public string ContrasenaHash { get; set; } = string.Empty;

        // "Admin" o "Adoptante"
        public string Rol { get; set; } = "Adoptante";

        public DateTime FechaRegistro { get; set; }
    }
}

using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Aplicacion.Interfaces;
using Dominio.Entidades;
using Dominio.Interfaces;

namespace Aplicacion.Services
{
    public class UsuarioService : IUsuarioService
    {
        private const int Iteraciones = 100_000;
        private readonly IUsuarioRepository _repositorio;

        public UsuarioService(IUsuarioRepository repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<Usuario> RegistrarAsync(string nombreUsuario, string correo, string contrasena)
        {
            nombreUsuario = (nombreUsuario ?? "").Trim();
            correo = (correo ?? "").Trim().ToLowerInvariant();

            if (!Regex.IsMatch(nombreUsuario, @"^[A-Za-z0-9_.]{3,30}$"))
                throw new ArgumentException("El usuario debe tener de 3 a 30 caracteres: letras, números, punto o guion bajo.");

            if (!MailAddress.TryCreate(correo, out var mail) || mail.Address != correo || !correo.Contains('.') || correo.Length > 100)
                throw new ArgumentException("El correo no tiene un formato válido.");

            ValidarContrasena(contrasena);

            if (await _repositorio.ExisteNombreOCorreoAsync(nombreUsuario, correo))
                throw new ArgumentException("Ese usuario o correo ya está registrado.");

            var usuario = new Usuario
            {
                NombreUsuario = nombreUsuario,
                Correo = correo,
                ContrasenaHash = GenerarHash(contrasena),
                Rol = "Adoptante", // nadie puede registrarse como administrador desde la página
                FechaRegistro = DateTime.Now
            };

            await _repositorio.AgregarAsync(usuario);
            return usuario;
        }

        public async Task<Usuario?> IniciarSesionAsync(string nombreUsuario, string contrasena)
        {
            var usuario = await _repositorio.ObtenerPorNombreAsync((nombreUsuario ?? "").Trim());

            if (usuario == null)
            {
                // Se hace el mismo trabajo de hash para que no se note si el usuario existe o no
                VerificarHash(contrasena ?? "", GenerarHash("relleno"));
                return null;
            }

            return VerificarHash(contrasena ?? "", usuario.ContrasenaHash) ? usuario : null;
        }

        public async Task CrearAdminSiNoExisteAsync(string? nombreUsuario, string? correo, string? contrasena)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(correo) || string.IsNullOrEmpty(contrasena))
                return;

            if (await _repositorio.ExisteAdminAsync())
                return;

            ValidarContrasena(contrasena);

            await _repositorio.AgregarAsync(new Usuario
            {
                NombreUsuario = nombreUsuario.Trim(),
                Correo = correo.Trim().ToLowerInvariant(),
                ContrasenaHash = GenerarHash(contrasena),
                Rol = "Admin",
                FechaRegistro = DateTime.Now
            });
        }

        private static void ValidarContrasena(string contrasena)
        {
            if (string.IsNullOrEmpty(contrasena) || contrasena.Length < 8 || contrasena.Length > 100)
                throw new ArgumentException("La contraseña debe tener entre 8 y 100 caracteres.");

            if (!contrasena.Any(char.IsLetter) || !contrasena.Any(char.IsDigit))
                throw new ArgumentException("La contraseña debe incluir al menos una letra y un número.");
        }

        // Formato guardado: pbkdf2$iteraciones$sal$hash (todo en Base64)
        private static string GenerarHash(string contrasena)
        {
            var sal = RandomNumberGenerator.GetBytes(16);
            var hash = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, Iteraciones, HashAlgorithmName.SHA256, 32);
            return $"pbkdf2${Iteraciones}${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
        }

        private static bool VerificarHash(string contrasena, string guardado)
        {
            var partes = guardado.Split('$');
            if (partes.Length != 4 || partes[0] != "pbkdf2") return false;

            var iteraciones = int.Parse(partes[1]);
            var sal = Convert.FromBase64String(partes[2]);
            var esperado = Convert.FromBase64String(partes[3]);
            var calculado = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, iteraciones, HashAlgorithmName.SHA256, esperado.Length);

            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
    }
}

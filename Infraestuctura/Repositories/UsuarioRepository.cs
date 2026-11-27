using Dominio.Entidades;
using Dominio.Interfaces;
using Infraestructura.Data;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AdopcionDbContext _context;

        public UsuarioRepository(AdopcionDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> ObtenerPorNombreAsync(string nombreUsuario)
        {
            return await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);
        }

        public async Task<bool> ExisteNombreOCorreoAsync(string nombreUsuario, string correo)
        {
            return await _context.Usuarios
                .AnyAsync(u => u.NombreUsuario == nombreUsuario || u.Correo == correo);
        }

        public async Task<bool> ExisteAdminAsync()
        {
            return await _context.Usuarios.AnyAsync(u => u.Rol == "Admin");
        }

        public async Task AgregarAsync(Usuario usuario)
        {
            await _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();
        }
    }
}

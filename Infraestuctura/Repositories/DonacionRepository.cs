using Dominio.Entidades;
using Dominio.Interfaces;
using Infraestructura.Data;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Repositories
{
    public class DonacionRepository : IDonacionRepository
    {
        private readonly AdopcionDbContext _context;

        public DonacionRepository(AdopcionDbContext context)
        {
            _context = context;
        }

        public async Task<List<Donacion>> ObtenerTodosAsync()
        {
            return await _context.Donaciones
                .AsNoTracking()
                .OrderByDescending(d => d.Fecha)
                .ToListAsync();
        }

        public async Task<Donacion?> ObtenerPorIdAsync(int id)
        {
            return await _context.Donaciones
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DonacionId == id);
        }

        public async Task AgregarAsync(Donacion donacion)
        {
            await _context.Donaciones.AddAsync(donacion);
            await _context.SaveChangesAsync();
        }
    }
}

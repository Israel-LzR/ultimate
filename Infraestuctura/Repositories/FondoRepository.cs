using Dominio.Entidades;
using Dominio.Interfaces;
using Infraestructura.Data;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Repositories
{
    public class FondoRepository : IFondoRepository
    {
        private readonly AdopcionDbContext _context;

        public FondoRepository(AdopcionDbContext context)
        {
            _context = context;
        }

        public async Task<Fondo?> ObtenerAsync()
        {
            return await _context.Fondos
                .AsNoTracking()
                .OrderBy(f => f.FondoId)
                .FirstOrDefaultAsync();
        }
    }
}

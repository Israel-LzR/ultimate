using System;
using System.Collections.Generic;
using System.Text;
using Dominio.Entidades;
using Dominio.Interfaces;
using Infraestructura.Data;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Repositories
{
    public class FotoAnimalRepository : IFotoAnimalRepository
    {
        private readonly AdopcionDbContext _context;

        public FotoAnimalRepository(AdopcionDbContext context)
        {
            _context = context;
        }

        public async Task<List<FotoAnimal>> ObtenerTodosAsync()
        {
            return await _context.Fotos.ToListAsync();
        }

        public async Task<FotoAnimal?> ObtenerPorIdAsync(int id)
        {
            return await _context.Fotos
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.FotoAnimalId == id);
        }

        public async Task AgregarAsync(FotoAnimal foto)
        {
            await _context.Fotos.AddAsync(foto);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(FotoAnimal foto)
        {
            _context.Fotos.Update(foto);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var foto = await _context.Fotos
                .FirstOrDefaultAsync(f => f.FotoAnimalId == id);

            if (foto != null)
            {
                _context.Fotos.Remove(foto);
                await _context.SaveChangesAsync();
            }
        }
    }
}
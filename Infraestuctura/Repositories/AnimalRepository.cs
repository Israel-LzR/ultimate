using System;
using System.Collections.Generic;
using System.Text;
using Dominio.Entidades;
using Dominio.Interfaces;
using Infraestructura.Data;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Repositories
{
    public class AnimalRepository : IAnimalRepository
    {
        private readonly AdopcionDbContext _context;

        public AnimalRepository(AdopcionDbContext context)
        {
            _context = context;
        }

        public async Task<List<Animal>> ObtenerTodosAsync()
        {
            return await _context.Animales
                .Include(a => a.Fotos)
                .ToListAsync();
        }

        public async Task<Animal?> ObtenerPorIdAsync(int id)
        {
            return await _context.Animales
                .Include(a => a.Fotos)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AnimalId == id);
        }

        public async Task AgregarAsync(Animal animal)
        {
            await _context.Animales.AddAsync(animal);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Animal animal)
        {
            _context.Animales.Update(animal);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var animal = await _context.Animales
                .FirstOrDefaultAsync(a => a.AnimalId == id);

            if (animal != null)
            {
                _context.Animales.Remove(animal);
                await _context.SaveChangesAsync();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using Dominio.Entidades;
using Dominio.Interfaces;
using Infraestructura.Data;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Repositories
{
    public class SolicitudAdopcionRepository : ISolicitudAdopcionRepository
    {
        private readonly AdopcionDbContext _context;

        public SolicitudAdopcionRepository(AdopcionDbContext context)
        {
            _context = context;
        }

        public async Task<List<SolicitudAdopcion>> ObtenerTodosAsync()
        {
            return await _context.Solicitudes.ToListAsync();
        }

        public async Task<SolicitudAdopcion?> ObtenerPorIdAsync(int id)
        {
            return await _context.Solicitudes
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SolicitudAdopcionId == id);
        }

        public async Task AgregarAsync(SolicitudAdopcion solicitud)
        {
            await _context.Solicitudes.AddAsync(solicitud);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(SolicitudAdopcion solicitud)
        {
            _context.Solicitudes.Update(solicitud);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var solicitud = await _context.Solicitudes
                .FirstOrDefaultAsync(s => s.SolicitudAdopcionId == id);

            if (solicitud != null)
            {
                _context.Solicitudes.Remove(solicitud);
                await _context.SaveChangesAsync();
            }
        }
    }
}

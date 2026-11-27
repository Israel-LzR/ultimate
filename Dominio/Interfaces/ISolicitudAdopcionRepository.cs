using System;
using System.Collections.Generic;
using System.Text;
using Dominio.Entidades;

namespace Dominio.Interfaces
{
    public interface ISolicitudAdopcionRepository
    {
        Task<List<SolicitudAdopcion>> ObtenerTodosAsync();

        Task<SolicitudAdopcion?> ObtenerPorIdAsync(int id);

        Task AgregarAsync(SolicitudAdopcion solicitud);

        Task ActualizarAsync(SolicitudAdopcion solicitud);

        Task EliminarAsync(int id);
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Dominio.Entidades;

namespace Aplicacion.Interfaces
{
    public interface ISolicitudAdopcionService
    {
        Task<List<SolicitudAdopcion>> ObtenerTodosAsync();

        Task<SolicitudAdopcion?> ObtenerPorIdAsync(int id);

        Task AgregarAsync(SolicitudAdopcion solicitud);

        Task ActualizarAsync(SolicitudAdopcion solicitud);

        Task EliminarAsync(int id);

        Task<List<SolicitudAdopcion>> BuscarAsync(string? texto, string? estado);
    }
}
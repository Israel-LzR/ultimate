using System.Net.Mail;
using System.Text.RegularExpressions;
using Aplicacion.Interfaces;
using Dominio.Entidades;
using Dominio.Enumeraciones;
using Dominio.Interfaces;

namespace Aplicacion.Services
{
    public class SolicitudAdopcionService : ISolicitudAdopcionService
    {
        private readonly ISolicitudAdopcionRepository _solicitudRepository;

        public SolicitudAdopcionService(ISolicitudAdopcionRepository solicitudRepository)
        {
            _solicitudRepository = solicitudRepository;
        }

        public async Task<List<SolicitudAdopcion>> ObtenerTodosAsync()
        {
            return await _solicitudRepository.ObtenerTodosAsync();
        }

        public async Task<SolicitudAdopcion?> ObtenerPorIdAsync(int id)
        {
            return await _solicitudRepository.ObtenerPorIdAsync(id);
        }

        public async Task AgregarAsync(SolicitudAdopcion solicitud)
        {
            Validar(solicitud);
            solicitud.Fecha = DateTime.Now;               // lo decide el servidor
            solicitud.Estado = EstadoSolicitud.Pendiente; // toda solicitud nueva inicia pendiente
            await _solicitudRepository.AgregarAsync(solicitud);
        }

        public async Task ActualizarAsync(SolicitudAdopcion solicitud)
        {
            Validar(solicitud);
            if (!Enum.IsDefined(solicitud.Estado))
                throw new ArgumentException("El estado de la solicitud no es válido.");
            await _solicitudRepository.ActualizarAsync(solicitud);
        }

        public async Task EliminarAsync(int id)
        {
            await _solicitudRepository.EliminarAsync(id);
        }

        // Búsqueda para el administrador: texto libre (nombre, correo, teléfono, mensaje, resultado del cuestionario o id)
        // y, opcionalmente, un estado (Pendiente, Aprobada, Rechazada).
        public async Task<List<SolicitudAdopcion>> BuscarAsync(string? texto, string? estado)
        {
            var solicitudes = await _solicitudRepository.ObtenerTodosAsync();
            IEnumerable<SolicitudAdopcion> resultado = solicitudes;

            if (!string.IsNullOrWhiteSpace(estado))
            {
                if (!Enum.TryParse<EstadoSolicitud>(estado, true, out var estadoBuscado) || !Enum.IsDefined(estadoBuscado))
                    throw new ArgumentException("El estado de búsqueda no es válido.");
                resultado = resultado.Where(s => s.Estado == estadoBuscado);
            }

            texto = (texto ?? "").Trim();
            if (texto.Length > 100)
                throw new ArgumentException("El texto de búsqueda es demasiado largo.");

            if (texto.Length > 0)
            {
                resultado = resultado.Where(s =>
                    Contiene(s.NombreSolicitante, texto) ||
                    Contiene(s.Correo, texto) ||
                    Contiene(s.Telefono, texto) ||
                    Contiene(s.Mensaje, texto) ||
                    Contiene(s.ResultadoCuestionario, texto) ||
                    s.SolicitudAdopcionId.ToString() == texto ||
                    s.AnimalId.ToString() == texto);
            }

            return resultado.OrderByDescending(s => s.Fecha).ToList();
        }

        private static bool Contiene(string? campo, string texto)
        {
            return campo != null && campo.Contains(texto, StringComparison.OrdinalIgnoreCase);
        }

        private static void Validar(SolicitudAdopcion s)
        {
            s.NombreSolicitante = (s.NombreSolicitante ?? "").Trim();
            s.Correo = (s.Correo ?? "").Trim();

            if (s.AnimalId <= 0)
                throw new ArgumentException("Debes indicar el animal que te interesa.");

            if (s.NombreSolicitante.Length < 2 || s.NombreSolicitante.Length > 80)
                throw new ArgumentException("El nombre debe tener entre 2 y 80 caracteres.");

            if (!MailAddress.TryCreate(s.Correo, out var correo) || correo.Address != s.Correo || !s.Correo.Contains('.'))
                throw new ArgumentException("El correo no tiene un formato válido.");

            if (!string.IsNullOrWhiteSpace(s.Telefono) && !Regex.IsMatch(s.Telefono.Trim(), @"^[0-9+\-\s]{7,15}$"))
                throw new ArgumentException("El teléfono solo puede llevar números, +, - y espacios (7 a 15 caracteres).");

            if (s.Mensaje != null && s.Mensaje.Length > 500)
                throw new ArgumentException("El mensaje no puede pasar de 500 caracteres.");
        }
    }
}

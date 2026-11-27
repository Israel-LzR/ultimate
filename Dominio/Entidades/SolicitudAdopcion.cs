using System;
using System.Collections.Generic;
using System.Text;
using Dominio.Enumeraciones;

namespace Dominio.Entidades
{
    public class SolicitudAdopcion
    {
        public int SolicitudAdopcionId { get; set; }

        public int AnimalId { get; set; }

        public string NombreSolicitante { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public string? Telefono { get; set; }

        public string? Mensaje { get; set; }

        public DateTime Fecha { get; set; }

        public EstadoSolicitud Estado { get; set; }

        // Resultado del cuestionario de aptitud que contestó el solicitante, ej. "Apto 88%".
        // Lo escribe el servidor (nunca el navegador).
        public string? ResultadoCuestionario { get; set; }

        // Cuenta (tabla Usuarios) que envió la solicitud. Llave foránea; la pone el servidor desde la sesión.
        public int? UsuarioId { get; set; }

        public Animal? Animal { get; set; }
    }
}

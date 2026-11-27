using System;
using System.Collections.Generic;
using System.Text;
using Dominio.Enumeraciones;

namespace Dominio.Entidades
{
    public class Animal
    {
        public int AnimalId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public Especie Especie { get; set; }

        public string? Raza { get; set; }

        public int EdadMeses { get; set; }

        public Sexo Sexo { get; set; }

        public Tamano Tamano { get; set; }

        public string? Color { get; set; }

        public string? Descripcion { get; set; }

        public bool Esterilizado { get; set; }

        public bool Vacunado { get; set; }

        public string Ciudad { get; set; } = string.Empty;

        public string? ContactoPublicador { get; set; }

        public EstadoAdopcion Estado { get; set; }

        public DateTime FechaPublicacion { get; set; }

        public ICollection<FotoAnimal> Fotos { get; set; } = new List<FotoAnimal>();

        public ICollection<SolicitudAdopcion> Solicitudes { get; set; } = new List<SolicitudAdopcion>();
    }
}

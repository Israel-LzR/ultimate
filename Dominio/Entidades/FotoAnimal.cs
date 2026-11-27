using System;
using System.Collections.Generic;
using System.Text;

namespace Dominio.Entidades
{
    public class FotoAnimal
    {
        public int FotoAnimalId { get; set; }

        public int AnimalId { get; set; }

        public string Url { get; set; } = string.Empty;

        public bool EsPrincipal { get; set; }

        public Animal? Animal { get; set; }
    }
}

namespace Aplicacion.Modelos
{
    // Lo que ve el navegador: la pregunta y sus opciones (sin puntos ni consejos)
    public class PreguntaCuestionario
    {
        public string Id { get; set; } = string.Empty;
        public string Texto { get; set; } = string.Empty;
        public List<OpcionCuestionario> Opciones { get; set; } = new();
    }

    public class OpcionCuestionario
    {
        public string Valor { get; set; } = string.Empty;
        public string Texto { get; set; } = string.Empty;
    }

    // Lo que envía el navegador: { "respuestas": { "q1": "a", "q2": "b", ... } }
    public class EvaluacionCuestionario
    {
        // Animal por el que se contesta el cuestionario (cada solicitud necesita el suyo)
        public int AnimalId { get; set; }

        public Dictionary<string, string>? Respuestas { get; set; }
    }

    public class ResultadoCuestionario
    {
        public int Puntos { get; set; }
        public int PuntosMaximos { get; set; }
        public int Porcentaje { get; set; }
        public string Nivel { get; set; } = string.Empty;   // Apto, Casi, NoApto
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public List<string> Recomendaciones { get; set; } = new();
    }
}

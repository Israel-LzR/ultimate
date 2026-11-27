using Aplicacion.Modelos;

namespace Aplicacion.Interfaces
{
    public interface ICuestionarioService
    {
        List<PreguntaCuestionario> ObtenerPreguntas();

        ResultadoCuestionario Evaluar(EvaluacionCuestionario evaluacion);
    }
}

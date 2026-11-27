using AdopcionWeb.Filters;
using Aplicacion.Interfaces;
using Aplicacion.Modelos;
using Microsoft.AspNetCore.Mvc;

namespace AdopcionWeb.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class CuestionarioApiController : ControllerBase
    {
        private readonly ICuestionarioService _cuestionarioService;

        public CuestionarioApiController(ICuestionarioService cuestionarioService)
        {
            _cuestionarioService = cuestionarioService;
        }

        // GET: api/CuestionarioApi -> las preguntas con sus opciones
        [HttpGet]
        public ActionResult<List<PreguntaCuestionario>> ObtenerPreguntas()
        {
            return Ok(_cuestionarioService.ObtenerPreguntas());
        }

        // POST: api/CuestionarioApi/evaluar -> el servidor calcula el resultado.
        // Si sale Apto, queda guardado en la SESIÓN para ese animal: sin eso no se puede enviar
        // la solicitud de adopción (SolicitudAdopcionApiController lo vuelve a comprobar).
        [RequiereSesion]
        [HttpPost("evaluar")]
        public ActionResult<ResultadoCuestionario> Evaluar(EvaluacionCuestionario evaluacion)
        {
            var resultado = _cuestionarioService.Evaluar(evaluacion);

            if (evaluacion.AnimalId > 0)
            {
                var clave = "Apto_" + evaluacion.AnimalId;
                if (resultado.Nivel == "Apto")
                    HttpContext.Session.SetString(clave, "Apto " + resultado.Porcentaje + "%");
                else
                    HttpContext.Session.Remove(clave); // volver a contestar y no salir apto lo anula
            }

            return Ok(resultado);
        }
    }
}

using Aplicacion.Interfaces;
using Aplicacion.Modelos;

namespace Aplicacion.Services
{
    // Cuestionario de aptitud para adoptar. Las preguntas, los puntos y las reglas viven
    // en el servidor: el navegador solo muestra las preguntas y envía las respuestas.
    public class CuestionarioService : ICuestionarioService
    {
        // Opción interna: además del texto lleva sus puntos, si es una respuesta que impide
        // adoptar por ahora (Critica) y un consejo para quien la elija.
        private record Opcion(string Valor, string Texto, int Puntos, bool Critica = false, string? Consejo = null);
        private record Pregunta(string Id, string Texto, Opcion[] Opciones);

        private static readonly Pregunta[] Preguntas =
        {
            new("q1", "¿Tienes permiso para tener mascotas en el lugar donde vives?", new[]
            {
                new Opcion("a", "Sí, es mi casa o tengo el permiso del dueño", 2),
                new Opcion("b", "Lo estoy consultando", 1, false, "Confirma con el dueño de la vivienda que sí aceptan mascotas antes de adoptar."),
                new Opcion("c", "No se permiten mascotas", 0, true, "Si en tu vivienda no se permiten mascotas, hoy no es posible adoptar; revisa si puedes cambiar esa condición.")
            }),
            new("q2", "¿Tu hogar tiene espacio suficiente para el animal que te interesa?", new[]
            {
                new Opcion("a", "Sí, tiene espacio de sobra", 2),
                new Opcion("b", "Es justo, pero lo sacaría a pasear a diario", 1, false, "Si el espacio es reducido, planea paseos o juego diario para que gaste energía."),
                new Opcion("c", "No, el espacio es muy pequeño", 0, false, "Considera un animal pequeño o tranquilo que se adapte a tu espacio.")
            }),
            new("q3", "¿Cuántas horas al día estaría solo?", new[]
            {
                new Opcion("a", "Menos de 4 horas", 2),
                new Opcion("b", "Entre 4 y 8 horas", 1, false, "Deja agua, juguetes y un lugar cómodo; evita que pase solo muchas horas seguidas."),
                new Opcion("c", "Más de 8 horas", 0, false, "Pasar tantas horas solo afecta su bienestar: busca apoyo de alguien que lo acompañe o lo pasee.")
            }),
            new("q4", "¿Están de acuerdo todas las personas que viven contigo?", new[]
            {
                new Opcion("a", "Sí, todas están de acuerdo", 2),
                new Opcion("b", "Todavía no lo he platicado", 1, false, "Platica con tu familia: todos deben estar de acuerdo y conocer las responsabilidades."),
                new Opcion("c", "No, alguien no está de acuerdo", 0, true, "Si alguien en casa no está de acuerdo, es mejor esperar a que todos acepten la llegada de la mascota.")
            }),
            new("q5", "¿Has cuidado mascotas antes?", new[]
            {
                new Opcion("a", "Sí, tengo o he tenido", 2),
                new Opcion("b", "Un poco, con ayuda de otras personas", 1, false, "Infórmate sobre alimentación, vacunas y comportamiento del animal que elijas."),
                new Opcion("c", "Sería mi primera mascota", 1, false, "Al ser tu primera mascota, lee sobre sus cuidados básicos y pregunta al refugio todo lo que necesites.")
            }),
            new("q6", "¿Puedes cubrir alimento, vacunas y atención veterinaria?", new[]
            {
                new Opcion("a", "Sí, cuento con presupuesto para ello", 2),
                new Opcion("b", "Con un presupuesto ajustado", 1, false, "Haz un cálculo mensual de gastos; un imprevisto veterinario puede ser costoso."),
                new Opcion("c", "No podría cubrirlos", 0, true, "Un animal necesita alimento y atención médica constantes; adopta cuando puedas cubrir esos gastos.")
            }),
            new("q7", "¿Estás dispuesto a vacunarlo y esterilizarlo?", new[]
            {
                new Opcion("a", "Sí, sin problema", 2),
                new Opcion("b", "Lo pensaría", 1, false, "La vacunación y la esterilización protegen su salud y evitan camadas no deseadas."),
                new Opcion("c", "No", 0, false, "El refugio recomienda vacunar y esterilizar; platica con ellos para resolver tus dudas.")
            }),
            new("q8", "Un perro o gato puede vivir de 10 a 15 años o más. ¿Estás listo para ese compromiso?", new[]
            {
                new Opcion("a", "Sí, es un compromiso de por vida", 2),
                new Opcion("b", "Tengo algunas dudas", 1, false, "Resuelve tus dudas con el refugio antes de decidir; adoptar es para toda la vida."),
                new Opcion("c", "No estoy seguro", 0, true, "Adoptar es un compromiso largo: tómate tu tiempo y adopta cuando estés seguro.")
            })
        };

        public List<PreguntaCuestionario> ObtenerPreguntas()
        {
            return Preguntas.Select(p => new PreguntaCuestionario
            {
                Id = p.Id,
                Texto = p.Texto,
                Opciones = p.Opciones.Select(o => new OpcionCuestionario { Valor = o.Valor, Texto = o.Texto }).ToList()
            }).ToList();
        }

        public ResultadoCuestionario Evaluar(EvaluacionCuestionario evaluacion)
        {
            var respuestas = evaluacion?.Respuestas;
            if (respuestas == null || respuestas.Count == 0)
                throw new ArgumentException("Responde todas las preguntas del cuestionario.");

            int puntos = 0;
            int maximo = 0;
            bool hayCritica = false;
            var recomendaciones = new List<string>();

            foreach (var pregunta in Preguntas)
            {
                if (!respuestas.TryGetValue(pregunta.Id, out var valor))
                    throw new ArgumentException("Responde todas las preguntas del cuestionario.");

                var elegida = pregunta.Opciones.FirstOrDefault(o => o.Valor == valor);
                if (elegida == null)
                    throw new ArgumentException("Hay una respuesta que no es válida.");

                puntos += elegida.Puntos;
                maximo += pregunta.Opciones.Max(o => o.Puntos);
                if (elegida.Critica) hayCritica = true;
                if (elegida.Consejo != null) recomendaciones.Add(elegida.Consejo);
            }

            int porcentaje = (int)Math.Round(puntos * 100.0 / maximo);

            var resultado = new ResultadoCuestionario
            {
                Puntos = puntos,
                PuntosMaximos = maximo,
                Porcentaje = porcentaje,
                Recomendaciones = recomendaciones
            };

            if (!hayCritica && porcentaje >= 75)
            {
                resultado.Nivel = "Apto";
                resultado.Titulo = "¡Estás listo para adoptar!";
                resultado.Mensaje = "Tus respuestas muestran que puedes darle un hogar responsable a una mascota. Conoce a los animales y envía tu solicitud.";
            }
            else if (!hayCritica && porcentaje >= 50)
            {
                resultado.Nivel = "Casi";
                resultado.Titulo = "Casi estás listo";
                resultado.Mensaje = "Puedes adoptar si tomas en cuenta las recomendaciones de abajo. Prepárate y vuelve a intentarlo cuando quieras.";
            }
            else
            {
                resultado.Nivel = "NoApto";
                resultado.Titulo = "Por ahora no es el mejor momento";
                resultado.Mensaje = "Hay condiciones que conviene resolver antes de adoptar, para el bienestar del animal y el tuyo. Revisa las recomendaciones.";
            }

            return resultado;
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AdopcionWeb.Filters
{
    // Sobre un endpoint: exige sesión (401) y, si se indica Rol, que el usuario tenga ese rol (403).
    public class RequiereSesionAttribute : ActionFilterAttribute
    {
        public string? Rol { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var sesion = context.HttpContext.Session;

            if (string.IsNullOrEmpty(sesion.GetString("Usuario")))
            {
                context.Result = new ObjectResult(new { mensaje = "Debes iniciar sesión para realizar esta acción." })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            else if (Rol != null && sesion.GetString("Rol") != Rol)
            {
                context.Result = new ObjectResult(new { mensaje = "No tienes permiso para realizar esta acción." })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }
    }
}

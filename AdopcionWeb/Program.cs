using System.Text.Json.Serialization;
using Aplicacion.Interfaces;
using Aplicacion.Services;
using Dominio.Interfaces;
using Infraestructura.Data;
using Infraestructura.Repositories;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Controladores (MVC + API)
builder.Services.AddControllersWithViews()
    .AddJsonOptions(opciones =>
    {
        // Evita el error de "ciclo" Animal -> Fotos -> Animal
        opciones.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        // Muestra "Gato" en vez de 1 en las respuestas JSON
        opciones.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// 2. Documentación de la API
builder.Services.AddOpenApi();

// 3. Base de datos
builder.Services.AddDbContext<AdopcionDbContext>(opciones =>
    opciones.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// 4. Repositorios: cuando alguien pida la interfaz, entrega la clase
builder.Services.AddScoped<IAnimalRepository, AnimalRepository>();
builder.Services.AddScoped<IFotoAnimalRepository, FotoAnimalRepository>();
builder.Services.AddScoped<ISolicitudAdopcionRepository, SolicitudAdopcionRepository>();
builder.Services.AddScoped<IDonacionRepository, DonacionRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IFondoRepository, FondoRepository>();

// 5. Servicios
builder.Services.AddScoped<IAnimalService, AnimalService>();
builder.Services.AddScoped<IFotoAnimalService, FotoAnimalService>();
builder.Services.AddScoped<ISolicitudAdopcionService, SolicitudAdopcionService>();
builder.Services.AddScoped<IDonacionService, DonacionService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ICuestionarioService, CuestionarioService>();

// 5.1 Sesiones: los datos viven en el servidor; el navegador solo guarda una cookie con el id de sesión
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opciones =>
{
    opciones.IdleTimeout = TimeSpan.FromMinutes(30); // expira tras 30 min sin actividad
    opciones.Cookie.HttpOnly = true;                  // JavaScript no puede leer la cookie
    opciones.Cookie.IsEssential = true;
    opciones.Cookie.SameSite = SameSiteMode.Lax;
});

var app = builder.Build();

// Administrador inicial: se crea solo si todavía no existe ninguno (datos en appsettings.json, sección "Admin")
using (var alcance = app.Services.CreateScope())
{
    try
    {
        var usuarios = alcance.ServiceProvider.GetRequiredService<IUsuarioService>();
        await usuarios.CrearAdminSiNoExisteAsync(
            app.Configuration["Admin:Usuario"],
            app.Configuration["Admin:Correo"],
            app.Configuration["Admin:Contrasena"]);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "No se pudo crear el administrador inicial. ¿Ya creaste la tabla Usuarios (tabla-usuarios.sql)?");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();

// Manejo central de errores de la API: respuestas JSON claras en vez de pantallas de error
app.Use(async (contexto, siguiente) =>
{
    try
    {
        await siguiente();
    }
    catch (ArgumentException ex) when (contexto.Request.Path.StartsWithSegments("/api"))
    {
        contexto.Response.StatusCode = StatusCodes.Status400BadRequest;
        await contexto.Response.WriteAsJsonAsync(new { mensaje = ex.Message });
    }
    catch (DbUpdateException) when (contexto.Request.Path.StartsWithSegments("/api"))
    {
        contexto.Response.StatusCode = StatusCodes.Status400BadRequest;
        await contexto.Response.WriteAsJsonAsync(new { mensaje = "No se pudo guardar: revisa que los datos relacionados existan." });
    }
});

app.UseAuthorization();
app.MapStaticAssets();

// 6. Pantalla de pruebas de la API (solo en desarrollo)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// 7. Activa los controladores API
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
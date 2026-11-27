// Utilidades compartidas por todas las páginas (se carga desde _Layout)

// Convierte una respuesta de error de la API en un texto para el usuario
async function obtenerMensajeError(respuesta) {
    try {
        const datos = await respuesta.json();
        if (datos.mensaje) return datos.mensaje;
        if (datos.errors) return Object.values(datos.errors).flat().join(" ");
        if (datos.title) return datos.title;
    } catch (e) { /* la respuesta no era JSON */ }
    return "Error " + respuesta.status;
}

// Refleja el estado de la sesión en la barra de navegación
window.actualizarSesionUI = function (estado) {
    window.sesionActual = estado;
    const enlace = document.getElementById("nav-sesion");
    if (enlace) {
        enlace.textContent = estado.activa
            ? "👤 " + estado.usuario + (estado.rol === "Admin" ? " (admin)" : "")
            : "Iniciar sesión";
    }
    // El enlace Administrar solo se muestra al rol Admin (el servidor además lo exige)
    const admin = document.getElementById("nav-admin");
    if (admin) admin.style.display = (estado.activa && estado.rol === "Admin") ? "" : "none";
};

// Consulta la sesión al cargar cada página
window.sesionPromesa = fetch("/api/SesionApi")
    .then(r => r.json())
    .catch(() => ({ activa: false }))
    .then(function (estado) {
        window.actualizarSesionUI(estado);
        return estado;
    });

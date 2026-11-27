document.addEventListener("DOMContentLoaded", function () {
    const URL = "/api/SesionApi";

    const estado = document.getElementById("sesion-estado");
    const error = document.getElementById("sesion-error");
    const zonaAcceso = document.getElementById("zona-acceso");
    const zonaSesion = document.getElementById("zona-sesion");
    const lista = document.getElementById("lista-mis-solicitudes");

    const campo = (id) => document.getElementById(id);

    function textoRol(rol) {
        return rol === "Admin" ? "Administrador" : "Adoptante";
    }

    function mostrarError(mensaje) {
        error.textContent = mensaje;
        error.style.display = "block";
    }

    function ocultarError() {
        error.textContent = "";
        error.style.display = "none";
    }

    async function cargarMisSolicitudes() {
        lista.replaceChildren();

        try {
            const r = await fetch("/api/SolicitudAdopcionApi/mias");
            if (!r.ok) {
                throw new Error(await obtenerMensajeError(r));
            }

            const solicitudes = await r.json();
            const animales = await (await fetch("/api/AnimalApi")).json();

            if (solicitudes.length === 0) {
                const li = document.createElement("li");
                li.className = "list-group-item";
                li.textContent = "Todavía no has enviado solicitudes.";
                lista.appendChild(li);
                return;
            }

            solicitudes.forEach(function (s) {
                const animal = animales.find((a) => a.animalId === s.animalId);

                const li = document.createElement("li");
                li.className = "list-group-item d-flex justify-content-between";

                const nombre = document.createElement("span");
                nombre.textContent = animal ? animal.nombre : "Animal #" + s.animalId;

                const est = document.createElement("span");
                est.className = "badge bg-secondary";
                est.textContent = s.estado;

                li.append(nombre, est);
                lista.appendChild(li);
            });
        } catch (ex) {
            mostrarError(ex.message);
        }
    }

    function pintar(datos) {
        window.actualizarSesionUI(datos);

        if (datos.activa) {
            const inicioTexto = datos.inicio ? " desde " + datos.inicio : "";
            estado.className = "alert alert-success";
            estado.textContent =
                "Sesión activa como " + datos.usuario + " (" + textoRol(datos.rol) + ")" + inicioTexto + ".";

            zonaAcceso.style.display = "none";
            zonaSesion.style.display = "block";

            // El admin no envía solicitudes: se oculta la sección
            const bloque = document.getElementById("bloque-mis-solicitudes");
            if (datos.rol === "Admin") {
                bloque.style.display = "none";
            } else {
                bloque.style.display = "block";
                cargarMisSolicitudes();
            }
        } else {
            estado.className = "alert alert-secondary";
            estado.textContent = "No hay una sesión activa.";
            zonaAcceso.style.display = "flex";
            zonaSesion.style.display = "none";
        }
    }

    async function enviar(ruta, cuerpo) {
        ocultarError();

        try {
            const r = await fetch(URL + "/" + ruta, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(cuerpo)
            });

            if (!r.ok) {
                throw new Error(await obtenerMensajeError(r));
            }

            // Se vuelve a leer lo guardado en el servidor
            const estadoActual = await (await fetch(URL)).json();
            pintar(estadoActual);
        } catch (ex) {
            mostrarError(ex.message);
        }
    }

    // Eventos de formularios y cierre de sesión
    campo("form-login").addEventListener("submit", function (e) {
        e.preventDefault();
        enviar("iniciar", {
            nombreUsuario: campo("login-usuario").value,
            contrasena: campo("login-contrasena").value
        });
        campo("login-contrasena").value = "";
    });

    campo("form-registro").addEventListener("submit", function (e) {
        e.preventDefault();
        enviar("registrar", {
            nombreUsuario: campo("reg-usuario").value,
            correo: campo("reg-correo").value,
            contrasena: campo("reg-contrasena").value
        });
        campo("reg-contrasena").value = "";
    });

    campo("btn-cerrar-sesion").addEventListener("click", async function () {
        ocultarError();
        await fetch(URL + "/cerrar", { method: "POST" });

        // Comprueba que ya no hay información de sesión
        const estadoActual = await (await fetch(URL)).json();
        pintar(estadoActual);
    });

    window.sesionPromesa.then(pintar);
});
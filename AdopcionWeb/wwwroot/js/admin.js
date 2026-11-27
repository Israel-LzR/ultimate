document.addEventListener("DOMContentLoaded", function () {
    const URL_ANIMALES = "/api/AnimalApi";
    const URL_SOLICITUDES = "/api/SolicitudAdopcionApi";

    const form = document.getElementById("form-animal");
    const msg = document.getElementById("admin-msg");
    const tablaAnimales = document.getElementById("tabla-animales");
    const tablaSolicitudes = document.getElementById("tabla-solicitudes");
    const titulo = document.getElementById("titulo-form");
    const btnCancelar = document.getElementById("btn-cancelar");
    const grupoFoto = document.getElementById("grupo-foto");

    const URL_DONACIONES = "/api/DonacionApi";

    let animales = [];
    let filtro = { texto: "", estado: "" }; // búsqueda activa de solicitudes
    let enEdicion = null; // animal completo que se está editando (null = modo crear)

    const campo = id => document.getElementById(id);

    const pesos = new Intl.NumberFormat("es-MX", { style: "currency", currency: "MXN" });
    const fechaHora = valor => valor ? new Date(valor).toLocaleString("es-MX") : "";

    function avisar(texto, ok) {
        msg.textContent = texto;
        msg.className = "fw-bold " + (ok ? "text-success" : "text-danger");
        msg.style.display = "block";
    }

    function celda(fila, texto) {
        const td = document.createElement("td");
        td.textContent = texto ?? "";
        fila.appendChild(td);
        return td;
    }

    function boton(texto, clase, accion) {
        const b = document.createElement("button");
        b.type = "button";
        b.className = "btn btn-sm " + clase + " me-1";
        b.textContent = texto;
        b.addEventListener("click", accion);
        return b;
    }

    // Una sola función para las llamadas: maneja 401 (sin sesión) y errores de validación
    async function llamar(url, opciones) {
        const r = await fetch(url, opciones);
        if (!r.ok) throw new Error(await obtenerMensajeError(r));
        return r.status === 204 ? null : r.json();
    }

    // ---------- READ ----------
    async function cargar() {
        try {
            animales = await llamar(URL_ANIMALES);
            const solicitudes = await llamar(urlSolicitudes());
            pintarAnimales();
            pintarSolicitudes(solicitudes);
            mostrarInfoBusqueda(solicitudes.length);
        } catch (e) {
            avisar("No se pudo cargar la información: " + e.message, false);
        }
        await cargarDonaciones();
    }

    // ---------- Búsqueda de solicitudes ----------
    // Sin filtro pide todas; con filtro usa GET /api/SolicitudAdopcionApi/buscar?texto=...&estado=...
    function urlSolicitudes() {
        if (!filtro.texto && !filtro.estado) return URL_SOLICITUDES;
        return URL_SOLICITUDES + "/buscar?texto=" + encodeURIComponent(filtro.texto) +
            "&estado=" + encodeURIComponent(filtro.estado);
    }

    function mostrarInfoBusqueda(cantidad) {
        const info = campo("buscar-info");
        if (!filtro.texto && !filtro.estado) {
            info.textContent = cantidad + " solicitud(es) en total.";
        } else {
            info.textContent = cantidad + " resultado(s) para la búsqueda.";
        }
    }

    async function buscarSolicitudes() {
        try {
            const solicitudes = await llamar(urlSolicitudes());
            pintarSolicitudes(solicitudes);
            mostrarInfoBusqueda(solicitudes.length);
        } catch (e) {
            avisar("No se pudo buscar: " + e.message, false);
        }
    }

    campo("form-buscar").addEventListener("submit", function (e) {
        e.preventDefault();
        filtro = { texto: campo("buscar-texto").value.trim(), estado: campo("buscar-estado").value };
        buscarSolicitudes();
    });

    campo("btn-limpiar").addEventListener("click", function () {
        campo("buscar-texto").value = "";
        campo("buscar-estado").value = "";
        filtro = { texto: "", estado: "" };
        buscarSolicitudes();
    });

    // ---------- Administrador de donaciones ----------
    // Fondo = saldo inicial (appsettings.json) + todo lo donado; cada donación nueva lo aumenta
    async function cargarDonaciones() {
        try {
            const resumen = await llamar(URL_DONACIONES + "/resumen");
            const donaciones = await llamar(URL_DONACIONES);

            campo("don-fondo").textContent = pesos.format(resumen.fondoTotal);
            campo("don-inicial").textContent = pesos.format(resumen.saldoInicial);
            campo("don-total").textContent = pesos.format(resumen.totalDonado);
            campo("don-cantidad").textContent = resumen.cantidadDonaciones;

            const tabla = campo("tabla-donaciones");
            tabla.replaceChildren();
            donaciones.forEach(d => {
                const tr = document.createElement("tr");
                celda(tr, d.donacionId);
                celda(tr, d.nombreDonante);
                celda(tr, pesos.format(d.monto));
                celda(tr, fechaHora(d.fecha));
                celda(tr, d.idTransaccion);
                celda(tr, d.estado);
                tabla.appendChild(tr);
            });
        } catch (e) {
            avisar("No se pudieron cargar las donaciones: " + e.message, false);
        }
    }

    campo("btn-actualizar-donaciones").addEventListener("click", cargarDonaciones);

    function pintarAnimales() {
        tablaAnimales.replaceChildren();
        animales.forEach(a => {
            const tr = document.createElement("tr");
            celda(tr, a.animalId);
            celda(tr, a.nombre);
            celda(tr, a.especie);
            celda(tr, a.edadMeses);
            celda(tr, a.estado);
            const acciones = document.createElement("td");
            acciones.append(
                boton("Editar", "btn-outline-primary", () => editar(a)),
                boton("Eliminar", "btn-outline-danger", () => eliminarAnimal(a))
            );
            tr.appendChild(acciones);
            tablaAnimales.appendChild(tr);
        });
    }

    function pintarSolicitudes(solicitudes) {
        tablaSolicitudes.replaceChildren();
        solicitudes.forEach(s => {
            const animal = animales.find(a => a.animalId === s.animalId);
            const tr = document.createElement("tr");
            celda(tr, s.solicitudAdopcionId);
            celda(tr, animal ? animal.nombre : "#" + s.animalId);
            celda(tr, s.nombreSolicitante);
            celda(tr, s.correo);
            celda(tr, s.telefono);
            celda(tr, s.mensaje);
            celda(tr, s.resultadoCuestionario || "Sin cuestionario");
            celda(tr, fechaHora(s.fecha));
            celda(tr, s.estado);
            const acciones = document.createElement("td");
            acciones.append(
                boton("Aprobar", "btn-outline-success", () => cambiarEstado(s, "Aprobada")),
                boton("Rechazar", "btn-outline-warning", () => cambiarEstado(s, "Rechazada")),
                boton("Eliminar", "btn-outline-danger", () => eliminarSolicitud(s))
            );
            tr.appendChild(acciones);
            tablaSolicitudes.appendChild(tr);
        });
    }

    // ---------- CREATE / UPDATE ----------
    function leerFormulario() {
        return {
            nombre: campo("a-nombre").value,
            especie: campo("a-especie").value,
            raza: campo("a-raza").value.trim() || null,
            edadMeses: parseInt(campo("a-edad").value, 10),
            sexo: campo("a-sexo").value,
            tamano: campo("a-tamano").value,
            color: campo("a-color").value.trim() || null,
            descripcion: campo("a-descripcion").value.trim() || null,
            esterilizado: campo("a-esterilizado").checked,
            vacunado: campo("a-vacunado").checked,
            ciudad: campo("a-ciudad").value,
            estado: campo("a-estado").value
        };
    }

    form.addEventListener("submit", async function (e) {
        e.preventDefault();
        const datos = leerFormulario();

        try {
            if (enEdicion === null) {
                // POST: crea el animal (con su foto principal si se indicó una)
                const foto = campo("a-foto").value.trim();
                if (foto) datos.fotos = [{ url: foto, esPrincipal: true }];
                await llamar(URL_ANIMALES, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(datos)
                });
                avisar("Animal creado correctamente.", true);
            } else {
                // PUT: se reenvía el animal original con los campos cambiados
                const completo = { ...enEdicion, ...datos };
                delete completo.fotos;
                delete completo.solicitudes;
                await llamar(URL_ANIMALES + "/" + enEdicion.animalId, {
                    method: "PUT",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(completo)
                });
                avisar("Animal actualizado correctamente.", true);
            }
            salirDeEdicion();
            await cargar();
        } catch (ex) {
            avisar(ex.message, false);
        }
    });

    function editar(animal) {
        enEdicion = animal;
        campo("a-nombre").value = animal.nombre;
        campo("a-especie").value = animal.especie;
        campo("a-raza").value = animal.raza ?? "";
        campo("a-edad").value = animal.edadMeses;
        campo("a-sexo").value = animal.sexo;
        campo("a-tamano").value = animal.tamano;
        campo("a-color").value = animal.color ?? "";
        campo("a-descripcion").value = animal.descripcion ?? "";
        campo("a-esterilizado").checked = animal.esterilizado;
        campo("a-vacunado").checked = animal.vacunado;
        campo("a-ciudad").value = animal.ciudad;
        campo("a-estado").value = animal.estado;
        titulo.textContent = "Editando: " + animal.nombre;
        btnCancelar.style.display = "inline-block";
        grupoFoto.style.display = "none";
        window.scrollTo({ top: 0, behavior: "smooth" });
    }

    function salirDeEdicion() {
        enEdicion = null;
        form.reset();
        titulo.textContent = "Nuevo animal";
        btnCancelar.style.display = "none";
        grupoFoto.style.display = "block";
    }

    btnCancelar.addEventListener("click", salirDeEdicion);

    // ---------- DELETE ----------
    async function eliminarAnimal(animal) {
        if (!confirm("¿Eliminar a " + animal.nombre + "? También se borrarán sus fotos y solicitudes.")) return;
        try {
            await llamar(URL_ANIMALES + "/" + animal.animalId, { method: "DELETE" });
            avisar("Animal eliminado.", true);
            await cargar();
        } catch (ex) {
            avisar(ex.message, false);
        }
    }

    async function eliminarSolicitud(s) {
        if (!confirm("¿Eliminar la solicitud de " + s.nombreSolicitante + "?")) return;
        try {
            await llamar(URL_SOLICITUDES + "/" + s.solicitudAdopcionId, { method: "DELETE" });
            avisar("Solicitud eliminada.", true);
            await cargar();
        } catch (ex) {
            avisar(ex.message, false);
        }
    }

    // ---------- UPDATE de solicitudes ----------
    async function cambiarEstado(s, nuevoEstado) {
        try {
            const completa = { ...s, estado: nuevoEstado };
            delete completa.animal;
            await llamar(URL_SOLICITUDES + "/" + s.solicitudAdopcionId, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(completa)
            });
            avisar("Solicitud marcada como " + nuevoEstado + ".", true);
            await cargar();
        } catch (ex) {
            avisar(ex.message, false);
        }
    }

    cargar();
});

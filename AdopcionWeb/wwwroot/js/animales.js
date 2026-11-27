document.addEventListener("DOMContentLoaded", function () {
    const URL_ANIMALES = "/api/AnimalApi";
    const URL_SOLICITUDES = "/api/SolicitudAdopcionApi";

    const grid = document.getElementById("grid-animales");
    const botonesFiltro = document.querySelectorAll(".btn-filtro");

    const modalElemento = document.getElementById("modal-solicitud");
    const modal = new bootstrap.Modal(modalElemento);
    const formSolicitud = document.getElementById("form-solicitud");
    const tituloModal = document.getElementById("titulo-modal");
    const inputAnimalId = document.getElementById("solicitud-animal-id");
    const mensajeError = document.getElementById("solicitud-error");
    const mensajeOk = document.getElementById("solicitud-ok");
    const btnEnviar = document.getElementById("btn-enviar-solicitud");

    // Cuestionario de aptitud (paso 1 del modal)
    const URL_CUESTIONARIO = "/api/CuestionarioApi";
    const pasoCuestionario = document.getElementById("paso-cuestionario");
    const pasoFormulario = document.getElementById("paso-formulario");
    const cqLista = document.getElementById("cq-lista");
    const cqError = document.getElementById("cq-error");
    const cqContador = document.getElementById("cq-contador");
    const cqResultado = document.getElementById("cq-resultado");
    const cqContinuar = document.getElementById("cq-continuar");
    const textoApto = document.getElementById("solicitud-apto");

    // Imagen de respaldo si el animal no tiene foto o la URL falla
    const PLACEHOLDER =
        "data:image/svg+xml;utf8," +
        encodeURIComponent(
            "<svg xmlns='http://www.w3.org/2000/svg' width='300' height='220'>" +
            "<rect width='100%' height='100%' fill='#e9e4f5'/>" +
            "<text x='50%' y='50%' font-size='48' text-anchor='middle' dominant-baseline='middle'>🐾</text>" +
            "</svg>"
        );

    let animales = [];
    let preguntas = [];        // preguntas del cuestionario (vienen del servidor)
    let aptoAnimalId = null;   // id del animal para el que ya se salió apto
    let textoResultado = "";   // ej. "Apto 88%"

    // ---------- Utilidades ----------
    function textoEdad(meses) {
        if (meses < 12) return meses + (meses === 1 ? " mes" : " meses");
        const anios = Math.floor(meses / 12);
        return anios + (anios === 1 ? " año" : " años");
    }

    function textoEspecie(especie) {
        if (especie === "Perro") return "Perrito";
        if (especie === "Gato") return "Gatito";
        return "Otro";
    }

    function urlFotoPrincipal(animal) {
        const fotos = animal.fotos || [];
        const principal = fotos.find(f => f.esPrincipal) || fotos[0];
        return principal ? principal.url : PLACEHOLDER;
    }

    // ---------- Pintar tarjetas (con DOM, sin innerHTML, para evitar inyección de HTML) ----------
    function crearTarjeta(animal) {
        const tarjeta = document.createElement("article");
        tarjeta.className = "tarjeta-animal";

        const img = document.createElement("img");
        img.src = urlFotoPrincipal(animal);
        img.alt = animal.nombre;
        img.onerror = function () { this.onerror = null; this.src = PLACEHOLDER; };

        const contenido = document.createElement("div");
        contenido.className = "contenido-animal";

        const titulo = document.createElement("h3");
        titulo.textContent = animal.nombre;

        const etiqueta = document.createElement("span");
        etiqueta.className = "etiqueta-especie";
        etiqueta.textContent = textoEdad(animal.edadMeses) + " - " + textoEspecie(animal.especie);

        const descripcion = document.createElement("p");
        descripcion.textContent = animal.descripcion || "";

        const boton = document.createElement("a");
        boton.href = "#";
        boton.className = "btn-adoptar";
        boton.textContent = "Me interesa";
        boton.addEventListener("click", function (e) {
            e.preventDefault();
            abrirModal(animal);
        });

        contenido.append(titulo, etiqueta, descripcion, boton);
        tarjeta.append(img, contenido);
        return tarjeta;
    }

    function pintar(filtro) {
        grid.replaceChildren();

        const visibles = animales.filter(a =>
            a.estado === "Disponible" && (filtro === "todos" || a.especie === filtro));

        if (visibles.length === 0) {
            const vacio = document.createElement("p");
            vacio.textContent = "No hay animales disponibles en esta categoría por ahora.";
            grid.appendChild(vacio);
            return;
        }

        visibles.forEach(a => grid.appendChild(crearTarjeta(a)));
    }

    // ---------- Cargar desde la API ----------
    async function cargarAnimales() {
        try {
            const respuesta = await fetch(URL_ANIMALES);
            if (!respuesta.ok) throw new Error("Error " + respuesta.status);
            animales = await respuesta.json();
            pintar("todos");
        } catch (error) {
            console.error("No se pudieron cargar los animales:", error);
            grid.replaceChildren();
            const p = document.createElement("p");
            p.textContent = "No se pudieron cargar los animales. Intenta de nuevo más tarde.";
            grid.appendChild(p);
        }
    }

    // ---------- Filtros ----------
    botonesFiltro.forEach(boton => {
        boton.addEventListener("click", function () {
            botonesFiltro.forEach(b => b.classList.remove("activo"));
            boton.classList.add("activo");
            pintar(boton.dataset.filtro);
        });
    });

    // ---------- Solicitud de adopción ----------
    function abrirModal(animal) {
        // Para pedir una adopción hace falta una cuenta con sesión iniciada
        if (!window.sesionActual || !window.sesionActual.activa) {
            window.location.href = "/Home/Sesion?requerido=1";
            return;
        }
        formSolicitud.reset();
        document.getElementById("solicitud-nombre").value = window.sesionActual.usuario;
        document.getElementById("solicitud-correo").value = window.sesionActual.correo || "";
        mensajeError.style.display = "none";
        mensajeOk.style.display = "none";
        btnEnviar.disabled = false;
        inputAnimalId.value = animal.animalId;
        tituloModal.textContent = "Solicitud de adopción: " + animal.nombre;
        mostrarPaso("cuestionario");
        modal.show();
        prepararCuestionario();
    }

    // ---------- Cuestionario de aptitud: obligatorio en cada solicitud ----------
    function mostrarPaso(paso) {
        const enCuestionario = paso === "cuestionario";
        pasoCuestionario.style.display = enCuestionario ? "block" : "none";
        pasoFormulario.style.display = enCuestionario ? "none" : "block";
        btnEnviar.style.display = enCuestionario ? "none" : "";
    }

    function errorCuestionario(texto) {
        cqError.textContent = texto;
        cqError.style.display = texto ? "block" : "none";
    }

    function actualizarContador() {
        const respondidas = preguntas.filter(p =>
            formSolicitud.querySelector('input[name="' + p.id + '"]:checked')).length;
        cqContador.textContent = respondidas + " de " + preguntas.length + " respondidas";
    }

    // Dibuja las preguntas que manda el servidor (con DOM, sin innerHTML)
    function pintarPreguntas() {
        cqLista.replaceChildren();

        preguntas.forEach(function (pregunta, indice) {
            const tarjeta = document.createElement("div");
            tarjeta.className = "pregunta-card";
            tarjeta.dataset.id = pregunta.id;
            tarjeta.setAttribute("role", "group");

            const titulo = document.createElement("div");
            titulo.className = "pregunta-titulo";
            const numero = document.createElement("span");
            numero.className = "pregunta-numero";
            numero.textContent = indice + 1;
            const textoPregunta = document.createElement("span");
            textoPregunta.textContent = pregunta.texto;
            titulo.append(numero, textoPregunta);
            tarjeta.appendChild(titulo);

            const opciones = document.createElement("div");
            opciones.className = "opciones";

            pregunta.opciones.forEach(function (opcion) {
                const etiqueta = document.createElement("label");
                etiqueta.className = "opcion";

                const radio = document.createElement("input");
                radio.type = "radio";
                radio.name = pregunta.id;
                radio.value = opcion.valor;
                radio.addEventListener("change", function () {
                    tarjeta.classList.remove("sin-responder");
                    actualizarContador();
                });

                const texto = document.createElement("span");
                texto.textContent = opcion.texto;

                etiqueta.append(radio, texto);
                opciones.appendChild(etiqueta);
            });

            tarjeta.appendChild(opciones);
            cqLista.appendChild(tarjeta);
        });

        actualizarContador();
    }

    // Se llama cada vez que se abre el modal: siempre se empieza de cero
    async function prepararCuestionario() {
        aptoAnimalId = null;
        textoResultado = "";
        errorCuestionario("");
        cqResultado.style.display = "none";
        document.getElementById("cq-evaluar").style.display = "";
        cqContador.style.display = "";

        if (preguntas.length === 0) {
            try {
                const respuesta = await fetch(URL_CUESTIONARIO);
                if (!respuesta.ok) throw new Error(await obtenerMensajeError(respuesta));
                preguntas = await respuesta.json();
            } catch (error) {
                console.error("No se pudo cargar el cuestionario:", error);
                errorCuestionario("No se pudo cargar el cuestionario. Intenta de nuevo más tarde.");
                return;
            }
        }
        pintarPreguntas();
    }

    function mostrarResultado(r, animalId) {
        const nivelClase = { Apto: "apto", Casi: "casi", NoApto: "no-apto" }[r.nivel] || "no-apto";
        const icono = { Apto: "✅", Casi: "🟡", NoApto: "🛑" }[r.nivel] || "🛑";

        cqResultado.className = "resultado resultado-" + nivelClase;
        document.getElementById("cq-badge").textContent = icono;
        document.getElementById("cq-titulo").textContent = r.titulo;
        document.getElementById("cq-porcentaje").textContent =
            r.porcentaje + "% de aptitud (" + r.puntos + " de " + r.puntosMaximos + " puntos)";
        document.getElementById("cq-mensaje").textContent = r.mensaje;

        const listaRec = document.getElementById("cq-lista-recomendaciones");
        listaRec.replaceChildren();
        r.recomendaciones.forEach(function (texto) {
            const li = document.createElement("li");
            li.textContent = texto;
            listaRec.appendChild(li);
        });
        document.getElementById("cq-recomendaciones").style.display =
            r.recomendaciones.length > 0 ? "block" : "none";

        // Solo si salió APTO se puede continuar a la solicitud
        const apto = r.nivel === "Apto";
        cqContinuar.style.display = apto ? "" : "none";
        if (apto) {
            aptoAnimalId = animalId;
            textoResultado = "Apto " + r.porcentaje + "%";
        } else {
            aptoAnimalId = null;
        }

        cqResultado.style.display = "block";
        cqLista.style.display = "none";
        document.getElementById("cq-evaluar").style.display = "none";
        cqContador.style.display = "none";
        cqResultado.scrollIntoView({ behavior: "smooth", block: "nearest" });
    }

    document.getElementById("cq-evaluar").addEventListener("click", async function () {
        errorCuestionario("");

        // Se arma { animalId, respuestas: { q1: "a", ... } } y se marcan las que faltan
        const respuestas = {};
        let faltantes = 0;
        preguntas.forEach(function (p) {
            const marcada = formSolicitud.querySelector('input[name="' + p.id + '"]:checked');
            const tarjeta = cqLista.querySelector('[data-id="' + p.id + '"]');
            if (marcada) {
                respuestas[p.id] = marcada.value;
            } else {
                faltantes++;
                tarjeta.classList.add("sin-responder");
            }
        });

        if (faltantes > 0) {
            errorCuestionario("Te faltan " + faltantes + " pregunta(s) por responder.");
            const primera = cqLista.querySelector(".sin-responder");
            if (primera) primera.scrollIntoView({ behavior: "smooth", block: "center" });
            return;
        }

        const animalId = parseInt(inputAnimalId.value, 10);
        try {
            const respuesta = await fetch(URL_CUESTIONARIO + "/evaluar", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ animalId: animalId, respuestas: respuestas })
            });
            if (!respuesta.ok) throw new Error(await obtenerMensajeError(respuesta));
            mostrarResultado(await respuesta.json(), animalId);
        } catch (error) {
            console.error("No se pudo evaluar el cuestionario:", error);
            errorCuestionario(error.message || "No se pudo evaluar el cuestionario.");
        }
    });

    document.getElementById("cq-reintentar").addEventListener("click", function () {
        aptoAnimalId = null;
        formSolicitud.reset();
        cqResultado.style.display = "none";
        cqLista.style.display = "";
        document.getElementById("cq-evaluar").style.display = "";
        cqContador.style.display = "";
        pintarPreguntas();
    });

    cqContinuar.addEventListener("click", function () {
        if (aptoAnimalId === null) return;
        textoApto.textContent = "✅ Cuestionario aprobado: " + textoResultado + ". Completa tu solicitud.";
        // reset() pudo vaciar nombre y correo: se vuelven a llenar con los datos de la sesión
        const nombre = document.getElementById("solicitud-nombre");
        const correo = document.getElementById("solicitud-correo");
        if (window.sesionActual) {
            if (!nombre.value) nombre.value = window.sesionActual.usuario || "";
            if (!correo.value) correo.value = window.sesionActual.correo || "";
        }
        mostrarPaso("formulario");
    });

    formSolicitud.addEventListener("submit", async function (evento) {
        evento.preventDefault();

        // Sin cuestionario aprobado para este animal no se envía nada
        // (el servidor también lo exige; esto evita enviar por accidente con Enter)
        if (aptoAnimalId === null || aptoAnimalId !== parseInt(inputAnimalId.value, 10)) return;

        mensajeError.style.display = "none";
        mensajeOk.style.display = "none";

        const solicitud = {
            animalId: parseInt(inputAnimalId.value, 10),
            nombreSolicitante: document.getElementById("solicitud-nombre").value.trim(),
            correo: document.getElementById("solicitud-correo").value.trim(),
            telefono: document.getElementById("solicitud-telefono").value.trim() || null,
            mensaje: document.getElementById("solicitud-mensaje").value.trim() || null,
            fecha: new Date().toISOString(),
            estado: "Pendiente"
        };

        btnEnviar.disabled = true;

        try {
            const respuesta = await fetch(URL_SOLICITUDES, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(solicitud)
            });

            if (!respuesta.ok) throw new Error(await obtenerMensajeError(respuesta));

            mensajeOk.textContent = "¡Solicitud enviada! El refugio se pondrá en contacto contigo.";
            mensajeOk.style.display = "block";
            formSolicitud.reset();
            aptoAnimalId = null; // otra solicitud pide otro cuestionario
        } catch (error) {
            console.error("No se pudo enviar la solicitud:", error);
            mensajeError.textContent = error.message || "No se pudo enviar la solicitud. Intenta otra vez.";
            mensajeError.style.display = "block";
            btnEnviar.disabled = false;
        }
    });

    cargarAnimales();
});

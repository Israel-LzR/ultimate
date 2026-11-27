document.addEventListener("DOMContentLoaded", function () {
    const URL_DONACIONES = "/api/DonacionApi";

    const formDonacion = document.getElementById("form-donacion");
    const mensajeError = document.getElementById("error-donacion");
    const areaConfirmacion = document.getElementById("confirmacion-resumen");
    const resumenTexto = document.getElementById("resumen-texto");

    // Uso de la sesión: sugerir el nombre del usuario identificado
    window.sesionPromesa.then(function (sesion) {
        const campo = document.getElementById("nombre");
        if (sesion.activa && campo.value === "") campo.value = sesion.usuario;
    });

    formDonacion.addEventListener("submit", async function (evento) {
        evento.preventDefault();

        const inputNombre = document.getElementById("nombre").value.trim();
        const selectMonto = document.getElementById("monto").value;

        if (inputNombre === "" || selectMonto === "") {
            mensajeError.textContent = "Por favor, completa todos los campos requeridos.";
            mensajeError.style.display = "block";
            areaConfirmacion.style.display = "none";
            return;
        }

        mensajeError.style.display = "none";

        // Objeto JavaScript que se envía a la API (el servidor completa fecha, estado e id)
        const donacionUsuario = {
            nombreDonante: inputNombre,
            monto: parseFloat(selectMonto),
            moneda: "MXN"
        };

        console.log("Objeto JavaScript enviado:", donacionUsuario);

        try {
            const respuesta = await fetch(URL_DONACIONES, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(donacionUsuario)
            });

            if (!respuesta.ok) throw new Error(await obtenerMensajeError(respuesta));

            // La API devuelve la donación guardada, con su id de transacción y fecha
            const guardada = await respuesta.json();

            resumenTexto.replaceChildren();
            const lineas = [
                ["ID Transacción: ", guardada.idTransaccion],
                ["Donante: ", guardada.nombreDonante],
                ["Monto: ", "$" + guardada.monto + " " + guardada.moneda],
                ["Fecha: ", new Date(guardada.fecha).toLocaleString()]
            ];
            lineas.forEach(function (par, i) {
                const etiqueta = document.createElement("strong");
                etiqueta.textContent = par[0];
                resumenTexto.append(etiqueta, par[1]);
                if (i < lineas.length - 1) resumenTexto.append(document.createElement("br"));
            });

            areaConfirmacion.style.display = "block";
            formDonacion.reset();
        } catch (error) {
            console.error("No se pudo guardar la donación:", error);
            mensajeError.textContent = error.message || "No se pudo guardar la donación. Intenta de nuevo.";
            mensajeError.style.display = "block";
            areaConfirmacion.style.display = "none";
        }
    });
});

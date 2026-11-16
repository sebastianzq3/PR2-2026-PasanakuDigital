const API_ROOT = "https://localhost:7056/api";
let idGrupoActivo = 1;
let usuariosDisponibles = [];
let turnosSeleccionados = [];

document.addEventListener("DOMContentLoaded", () => {
    verificarSesion();
});

// Cabeceras con el Token JWT para todas las peticiones protegidas
function obtenerHeadersAuth() {
    const token = localStorage.getItem("pasanaku_token");
    return {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${token}`
    };
}

// ==========================================
// 1. AUTENTICACIÓN (LOGIN / LOGOUT CON JWT)
// ==========================================
function verificarSesion() {
    const token = localStorage.getItem("pasanaku_token");
    const nombreUsuario = localStorage.getItem("pasanaku_usuario");

    if (token) {
        document.getElementById("vistaLogin").classList.add("oculto");
        document.getElementById("vistaDashboard").classList.remove("oculto");
        document.getElementById("nombreUsuarioActivo").textContent = nombreUsuario || "Participante";
        inicializarDashboard();
    } else {
        document.getElementById("vistaLogin").classList.remove("oculto");
        document.getElementById("vistaDashboard").classList.add("oculto");
    }
}

async function iniciarSesion(event) {
    event.preventDefault();
    const correo = document.getElementById("loginCorreo").value;
    const password = document.getElementById("loginPassword").value;
    const alertaLogin = document.getElementById("alertaLogin");

    try {
        const response = await fetch(`${API_ROOT}/Auth/login`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ correo, password })
        });

        const data = await response.json();
        if (!response.ok) throw new Error(data.mensaje || "Credenciales inválidas.");

        localStorage.setItem("pasanaku_token", data.token);
        localStorage.setItem("pasanaku_usuario", data.usuario.nombreCompleto);
        alertaLogin.classList.add("oculto");
        verificarSesion();
    } catch (error) {
        alertaLogin.textContent = error.message;
        alertaLogin.classList.remove("oculto");
    }
}

function cerrarSesion() {
    localStorage.removeItem("pasanaku_token");
    localStorage.removeItem("pasanaku_usuario");
    verificarSesion();
}

// ==========================================
// 2. CARGA DE GRUPOS Y DASHBOARD
// ==========================================
async function inicializarDashboard() {
    await cargarListaDeGrupos();
    await cargarDashboard();
}

async function cargarListaDeGrupos() {
    try {
        const response = await fetch(`${API_ROOT}/Pasanaku/grupos`, {
            headers: obtenerHeadersAuth()
        });
        if (response.status === 401) return cerrarSesion();

        const grupos = await response.json();
        const selector = document.getElementById("selectorGrupo");
        selector.innerHTML = "";

        grupos.forEach(g => {
            const option = document.createElement("option");
            option.value = g.idGrupo;
            option.textContent = `${g.nombre} (Bs. ${g.montoCuota} - ${g.estado})`;
            selector.appendChild(option);
        });

        if (grupos.length > 0) {
            if (!grupos.some(g => g.idGrupo === idGrupoActivo)) {
                idGrupoActivo = grupos[0].idGrupo;
            }
            selector.value = idGrupoActivo;
        }
    } catch (error) {
        mostrarAlerta("Error al cargar la lista de grupos: " + error.message, true);
    }
}

function cambiarGrupoSeleccionado() {
    const selector = document.getElementById("selectorGrupo");
    idGrupoActivo = parseInt(selector.value);
    cargarDashboard();
}

async function cargarDashboard() {
    try {
        const response = await fetch(`${API_ROOT}/Pasanaku/grupo/${idGrupoActivo}`, {
            headers: obtenerHeadersAuth()
        });
        if (response.status === 401) return cerrarSesion();
        if (!response.ok) throw new Error("No se pudo obtener la información del grupo.");

        const data = await response.json();
        renderizarDashboard(data);
    } catch (error) {
        mostrarAlerta(error.message, true);
    }
}

function renderizarDashboard(data) {
    document.getElementById("nombreGrupo").textContent = data.nombreGrupo;
    document.getElementById("frecuenciaGrupo").textContent = `Pasanaku ${data.frecuencia}`;
    document.getElementById("fechaInicio").textContent = data.fechaInicio;
    document.getElementById("estadoGrupo").textContent = data.estadoGrupo;

    document.getElementById("cicloActual").textContent = `Turno #${data.cicloActual} de ${data.totalMiembros}`;
    document.getElementById("montoCuota").textContent = `Bs. ${data.montoCuota.toFixed(2)}`;
    document.getElementById("pozoTotal").textContent = `Bs. ${data.pozoTotal.toFixed(2)}`;
    document.getElementById("progresoRecaudacion").textContent = 
        `Recaudado en este turno: Bs. ${data.montoRecaudadoCiclo.toFixed(2)}`;

    const btnAvanzar = document.getElementById("btnAvanzarTurno");
    if (data.estadoGrupo === "Finalizado") {
        document.getElementById("nombreReceptor").textContent = "¡Pasanaku Completado!";
        document.getElementById("detalleReceptor").textContent = "Todos los participantes recibieron su pozo.";
        btnAvanzar.disabled = true;
    } else if (data.receptorActual) {
        document.getElementById("nombreReceptor").textContent = data.receptorActual.nombreCompleto;
        document.getElementById("detalleReceptor").textContent = 
            `Teléfono: ${data.receptorActual.telefono} | Recibe el Pozo en el Turno #${data.receptorActual.turnoCobro}`;
    }

    const pagadosCount = data.miembros.filter(m => m.pagoCicloActual).length;
    const porcentaje = data.totalMiembros > 0 ? (pagadosCount / data.totalMiembros) * 100 : 0;
    document.getElementById("barraProgreso").style.width = `${porcentaje}%`;
    document.getElementById("contadorPagos").textContent = `${pagadosCount}/${data.totalMiembros} Pagados`;

    btnAvanzar.disabled = (pagadosCount < data.totalMiembros) || (data.estadoGrupo === "Finalizado");

    const contenedor = document.getElementById("listaMiembros");
    contenedor.innerHTML = "";

    data.miembros.forEach(miembro => {
        const esTurnoActual = miembro.turnoCobro === data.cicloActual && data.estadoGrupo === "Activo";
        const card = document.createElement("article");
        card.className = `card miembro-card ${esTurnoActual ? "es-receptor" : ""}`;

        card.innerHTML = `
            <div>
                <div class="miembro-top">
                    <span class="turno-numero">Turno #${miembro.turnoCobro}</span>
                    <span class="estado-pago-badge ${miembro.pagoCicloActual ? 'pagado' : 'pendiente'}">
                        ${miembro.pagoCicloActual ? 'Cuota Pagada' : 'Pago Pendiente'}
                    </span>
                </div>
                <h4 style="margin-top: 0.75rem; font-size: 1.1rem;">
                    ${miembro.nombreCompleto} ${esTurnoActual ? '★' : ''}
                </h4>
                <p style="font-size: 0.85rem; color: #475569;">Tel: ${miembro.telefono}</p>
                <p style="font-size: 0.8rem; color: #64748b; margin-top: 0.25rem;">
                    Estado de pozo: <strong>${miembro.estadoCobro}</strong>
                </p>
                ${miembro.fechaPagoCicloActual 
                    ? `<p style="font-size: 0.75rem; color: #059669; margin-top: 0.25rem;">Pagado el: ${miembro.fechaPagoCicloActual}</p>` 
                    : ''}
            </div>
            <div>
                <button 
                    class="btn-pagar" 
                    ${miembro.pagoCicloActual || data.estadoGrupo !== "Activo" ? "disabled" : ""}
                    onclick="marcarPago(${miembro.idUsuario})">
                    ${miembro.pagoCicloActual ? "✓ Aporte Registrado" : `Registrar Pago (Bs. ${data.montoCuota})`}
                </button>
            </div>
        `;
        contenedor.appendChild(card);
    });
}

// ==========================================
// 3. PAGOS Y AVANCE DE TURNO
// ==========================================
async function marcarPago(idUsuario) {
    try {
        const response = await fetch(`${API_ROOT}/Pasanaku/pagar`, {
            method: "POST",
            headers: obtenerHeadersAuth(),
            body: JSON.stringify({ idGrupo: idGrupoActivo, idUsuario })
        });

        const result = await response.json();
        if (!response.ok) throw new Error(result.mensaje || "Error al registrar el pago.");

        mostrarAlerta(result.mensaje, false);
        await cargarDashboard();
    } catch (error) {
        mostrarAlerta(error.message, true);
    }
}

async function entregarPozoYAvanzar() {
    try {
        const response = await fetch(`${API_ROOT}/Pasanaku/grupo/${idGrupoActivo}/avanzar-turno`, {
            method: "POST",
            headers: obtenerHeadersAuth()
        });

        const result = await response.json();
        if (!response.ok) throw new Error(result.mensaje || "No se pudo avanzar de turno.");

        mostrarAlerta(result.mensaje, false);
        await cargarListaDeGrupos();
        await cargarDashboard();
    } catch (error) {
        mostrarAlerta(error.message, true);
    }
}

// ==========================================
// 4. CREACIÓN DE PARTICIPANTES Y GRUPOS
// ==========================================
function abrirModalUsuario() {
    document.getElementById("formNuevoUsuario").reset();
    document.getElementById("modalUsuario").classList.remove("oculto");
}

async function guardarNuevoUsuario(event) {
    event.preventDefault();
    const payload = {
        nombre: document.getElementById("usuNombre").value,
        apellido: document.getElementById("usuApellido").value,
        telefono: document.getElementById("usuTelefono").value,
        correo: document.getElementById("usuCorreo").value,
        password: document.getElementById("usuPassword").value
    };

    try {
        const response = await fetch(`${API_ROOT}/Auth/registrar`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });

        const data = await response.json();
        if (!response.ok) throw new Error(data.mensaje || "Error al registrar participante.");

        cerrarModales();
        mostrarAlerta(data.mensaje, false);
    } catch (error) {
        mostrarAlerta(error.message, true);
    }
}

async function abrirModalGrupo() {
    document.getElementById("formNuevoGrupo").reset();
    document.getElementById("grpFecha").valueAsDate = new Date();
    turnosSeleccionados = [];

    try {
        const response = await fetch(`${API_ROOT}/Pasanaku/usuarios`, {
            headers: obtenerHeadersAuth()
        });
        usuariosDisponibles = await response.json();
        actualizarSelectUsuariosDisponibles();
        renderizarListaTurnos();
        document.getElementById("modalGrupo").classList.remove("oculto");
    } catch (error) {
        mostrarAlerta("No se pudieron cargar los usuarios: " + error.message, true);
    }
}

function actualizarSelectUsuariosDisponibles() {
    const select = document.getElementById("selectUsuarioDisponible");
    select.innerHTML = "";

    const restantes = usuariosDisponibles.filter(
        u => !turnosSeleccionados.some(t => t.idUsuario === u.idUsuario)
    );

    restantes.forEach(u => {
        const opt = document.createElement("option");
        opt.value = u.idUsuario;
        opt.textContent = `${u.nombreCompleto} (${u.telefono})`;
        select.appendChild(opt);
    });
}

function agregarParticipanteATurno() {
    const select = document.getElementById("selectUsuarioDisponible");
    const idSeleccionado = parseInt(select.value);
    if (!idSeleccionado) return;

    const usuario = usuariosDisponibles.find(u => u.idUsuario === idSeleccionado);
    if (usuario) {
        turnosSeleccionados.push(usuario);
        actualizarSelectUsuariosDisponibles();
        renderizarListaTurnos();
    }
}

function quitarParticipanteDeTurno(idUsuario) {
    turnosSeleccionados = turnosSeleccionados.filter(u => u.idUsuario !== idUsuario);
    actualizarSelectUsuariosDisponibles();
    renderizarListaTurnos();
}

function renderizarListaTurnos() {
    const ul = document.getElementById("listaTurnosAsignados");
    ul.innerHTML = "";

    if (turnosSeleccionados.length === 0) {
        ul.innerHTML = `<li class="turno-item" style="color:#64748b;">Aún no has asignado participantes a los turnos.</li>`;
        return;
    }

    turnosSeleccionados.forEach((u, index) => {
        const li = document.createElement("li");
        li.className = "turno-item";
        li.innerHTML = `
            <span><strong>Turno #${index + 1}:</strong> ${u.nombreCompleto}</span>
            <button type="button" class="btn-quitar" onclick="quitarParticipanteDeTurno(${u.idUsuario})">Quitar</button>
        `;
        ul.appendChild(li);
    });
}

async function guardarNuevoGrupo(event) {
    event.preventDefault();
    if (turnosSeleccionados.length < 2) {
        mostrarAlerta("Debes asignar al menos 2 participantes al Pasanaku.", true);
        return;
    }

    const payload = {
        nombre: document.getElementById("grpNombre").value,
        montoCuota: parseFloat(document.getElementById("grpMonto").value),
        frecuencia: document.getElementById("grpFrecuencia").value,
        fechaInicio: document.getElementById("grpFecha").value,
        idsUsuariosOrdenados: turnosSeleccionados.map(u => u.idUsuario)
    };

    try {
        const response = await fetch(`${API_ROOT}/Pasanaku/grupos`, {
            method: "POST",
            headers: obtenerHeadersAuth(),
            body: JSON.stringify(payload)
        });

        const data = await response.json();
        if (!response.ok) throw new Error(data.mensaje || "Error al crear el Pasanaku.");

        cerrarModales();
        idGrupoActivo = data.idGrupo;
        await cargarListaDeGrupos();
        await cargarDashboard();
        mostrarAlerta(data.mensaje, false);
    } catch (error) {
        mostrarAlerta(error.message, true);
    }
}

function cerrarModales() {
    document.getElementById("modalUsuario").classList.add("oculto");
    document.getElementById("modalGrupo").classList.add("oculto");
}

function mostrarAlerta(mensaje, esError = false) {
    const alerta = document.getElementById("alertaMensaje");
    alerta.textContent = mensaje;
    alerta.className = `alerta ${esError ? "error" : ""}`;
    setTimeout(() => {
        alerta.classList.add("oculto");
    }, 4500);
}
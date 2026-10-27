// Chat del asistente con IA. El HTML esta en Site.Master y el servidor es Asistente/Consultar.ashx.
// La conversacion vive solo en esta pagina; se guarda en sessionStorage para no perderla al cambiar de pantalla.
(function () {
    var boton = document.getElementById('asistenteBoton');
    if (!boton) return;   // este usuario no tiene el modulo "Asistente IA"

    var panel = document.getElementById('asistentePanel');
    var lista = document.getElementById('asistenteMensajes');
    var texto = document.getElementById('asistenteTexto');
    var enviar = document.getElementById('asistenteEnviar');
    var sugerencias = document.getElementById('asistenteSugerencias');
    var url = boton.getAttribute('data-url');
    // La charla se guarda por usuario: si en la misma pestaña entra otra persona, no ve la anterior
    var clave = 'asistente-' + boton.getAttribute('data-usuario');

    var historial = [];
    var ocupado = false;

    try { historial = JSON.parse(sessionStorage.getItem(clave) || '[]'); } catch (e) { historial = []; }
    historial.forEach(function (m) { dibujar(m.role === 'user' ? 'yo' : 'ia', m.content); });

    boton.addEventListener('click', function () {
        panel.hidden = !panel.hidden;
        if (!panel.hidden) texto.focus();
    });
    document.getElementById('asistenteCerrar').addEventListener('click', function () { panel.hidden = true; });

    enviar.addEventListener('click', function () { preguntar(texto.value); });
    // Enter manda la pregunta (y no el formulario de ASP.NET)
    texto.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') { e.preventDefault(); preguntar(texto.value); }
    });
    sugerencias.addEventListener('click', function (e) {
        if (e.target.classList.contains('asistente-chip')) preguntar(e.target.textContent);
    });

    function preguntar(pregunta) {
        pregunta = pregunta.trim();
        if (!pregunta || ocupado) return;
        texto.value = '';
        dibujar('yo', pregunta);
        var pensando = dibujar('ia pensando', 'Pensando…');
        ocupado = true;

        llamar({ accion: 'preguntar', pregunta: pregunta, historial: historial }, function (r) {
            pensando.remove();
            dibujar('ia', r.texto);
            if (r.propuesta) tarjeta(r.propuesta);
            guardar({ role: 'user', content: pregunta }, { role: 'assistant', content: r.texto });
        }, function () { pensando.remove(); });
    }

    // La tarjeta con el turno que propone el asistente: el paciente elige el motivo y confirma
    function tarjeta(p) {
        var div = document.createElement('div');
        div.className = 'asistente-turno';
        div.innerHTML =
            '<strong></strong><span class="asistente-turno-detalle"></span>' +
            '<label>Motivo de consulta <select><option>Primera consulta</option><option>Control</option><option>Estudio</option></select></label>' +
            '<button type="button" class="btn">Confirmar turno</button>';
        div.querySelector('strong').textContent = p.Especialidad + ' · ' + p.FechaTexto + ' · ' + p.Hora + ' hs';
        div.querySelector('.asistente-turno-detalle').textContent = p.Medico;
        lista.appendChild(div);
        lista.scrollTop = lista.scrollHeight;

        var confirmar = div.querySelector('button');
        confirmar.addEventListener('click', function () {
            confirmar.disabled = true;
            llamar({
                accion: 'reservar', especialidad: p.Especialidad, fecha: p.Fecha, hora: p.Hora,
                medico: p.IdMedico > 0 ? p.Medico : '', motivo: div.querySelector('select').value
            }, function (r) {
                div.classList.add(r.reservado ? 'asistente-turno-ok' : 'asistente-turno-no');
                div.querySelector('label').remove();
                confirmar.remove();
                dibujar('ia', r.texto);
                guardar({ role: 'assistant', content: r.texto });
                // Si esta en "Mis turnos", recargamos para que aparezca en la lista
                if (r.reservado && location.pathname.indexOf('PortalPaciente') >= 0) setTimeout(function () { location.reload(); }, 1800);
            }, function () { confirmar.disabled = false; });
        });
    }

    function llamar(datos, ok, error) {
        fetch(url, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(datos), credentials: 'same-origin' })
            .then(function (resp) {
                if (resp.status === 403) throw new Error('No tenés habilitado el asistente.');
                if (!resp.ok) throw new Error('No se pudo consultar al asistente (error ' + resp.status + ').');
                return resp.json();
            })
            .then(function (r) { ocupado = false; ok(r); })
            .catch(function (e) { ocupado = false; dibujar('ia error', e.message); if (error) error(); });
    }

    function dibujar(quien, contenido) {
        var div = document.createElement('div');
        div.className = 'asistente-msj asistente-' + quien.split(' ').join(' asistente-');
        div.textContent = contenido;   // textContent: nunca se interpreta como HTML
        lista.appendChild(div);
        lista.scrollTop = lista.scrollHeight;
        sugerencias.hidden = true;
        return div;
    }

    function guardar() {
        for (var i = 0; i < arguments.length; i++) historial.push(arguments[i]);
        historial = historial.slice(-12);
        try { sessionStorage.setItem(clave, JSON.stringify(historial)); } catch (e) { }
    }
})();

document.addEventListener('DOMContentLoaded', function () {
    var links = document.querySelectorAll('.app-nav-links a[data-permiso]');
    var modal = document.getElementById('modalAcceso');

    // "accesos" lo escribe el master: los modulos que este usuario tiene habilitados
    var misAccesos = (typeof accesos !== 'undefined') ? accesos : [];

    links.forEach(function (link) {
        link.addEventListener('click', function (e) {
            if (misAccesos.indexOf(link.getAttribute('data-permiso')) < 0) {
                e.preventDefault();
                modal.style.display = 'flex';
            }
        });
    });

    // Si entro escribiendo la direccion de un modulo que no tiene, Global.asax lo manda a Inicio con ?sinAcceso=1
    if (modal && window.location.search.indexOf('sinAcceso=1') >= 0)
        modal.style.display = 'flex';
});

function cerrarModalAcceso() {
    document.getElementById('modalAcceso').style.display = 'none';
}

// Boton de modo claro / oscuro. El tema vive en el atributo data-tema de <html>
// (lo pone el script del <head> del master) y se recuerda en el navegador.
(function () {
    var boton = document.getElementById('temaBoton');
    if (!boton) return;

    function actualizarTitulo() {
        var oscuro = document.documentElement.getAttribute('data-tema') === 'oscuro';
        boton.title = oscuro ? 'Cambiar a modo claro' : 'Cambiar a modo oscuro';
    }

    boton.addEventListener('click', function () {
        var nuevo = document.documentElement.getAttribute('data-tema') === 'oscuro' ? 'claro' : 'oscuro';
        document.documentElement.setAttribute('data-tema', nuevo);
        try { localStorage.setItem('tema', nuevo); } catch (e) { }
        actualizarTitulo();
    });
    actualizarTitulo();
})();

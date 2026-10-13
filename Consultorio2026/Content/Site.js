document.addEventListener('DOMContentLoaded', function () {
    var links = document.querySelectorAll('.app-nav-links a[data-rol]');
    var modal = document.getElementById('modalAcceso');

    links.forEach(function (link) {
        link.addEventListener('click', function (e) {
            var rolLink = link.getAttribute('data-rol');
            // data-rol puede tener mas de un rol separado por espacio (ej. Turnos tambien ve Especialistas)
            var tieneAcceso = (rolActual === 'Seguridad' || rolLink.split(' ').indexOf(rolActual) >= 0);

            if (!tieneAcceso) {
                e.preventDefault();
                modal.style.display = 'flex';
            }
        });
    });
});

function cerrarModalAcceso() {
    document.getElementById('modalAcceso').style.display = 'none';
}
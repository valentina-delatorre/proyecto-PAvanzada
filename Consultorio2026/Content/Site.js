document.addEventListener('DOMContentLoaded', function () {
    var links = document.querySelectorAll('.app-nav-links a[data-rol]');
    var modal = document.getElementById('modalAcceso');

    links.forEach(function (link) {
        link.addEventListener('click', function (e) {
            var rolLink = link.getAttribute('data-rol');
            var tieneAcceso = (rolActual === 'Seguridad' || rolActual === rolLink);

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
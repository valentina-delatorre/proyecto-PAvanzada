<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Consultorio2026.Default" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Consultorio de Atención Médica</title>
    <link runat="server" href="~/Content/Theme.css" rel="stylesheet" />
    <link runat="server" href="~/Content/Home.css" rel="stylesheet" />
</head>
<body class="landing-body">
    <form id="form1" runat="server">

        <nav class="landing-nav">
            <a href="#inicio" class="landing-nav-brand">
                <svg class="brand-mark" viewBox="0 0 60 24" xmlns="http://www.w3.org/2000/svg">
                    <path d="M0,12 L18,12 L24,4 L30,20 L36,12 L60,12" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" />
                </svg>
                Consultorio
            </a>

            <ul class="landing-nav-links">
                <li><a href="#servicios">Servicios</a></li>
                <li><a href="#nosotros">Sobre nosotros</a></li>
                <li><a href="#turnos">Sacar turno</a></li>
            </ul>

            <div class="landing-nav-actions">
                <a runat="server" href="~/Seguridad/Login.aspx" class="nav-link-login">Iniciar sesión</a>
                <a runat="server" href="~/Seguridad/Registro.aspx" class="btn btn-sm">Registrarse</a>
            </div>
        </nav>

        <header id="inicio" class="landing-hero">
            <video class="hero-video" autoplay muted loop playsinline>
                <source src="public/hero/backgroundvideo.mp4" type="video/mp4" />
            </video>
            <div class="hero-overlay"></div>

            <div class="hero-content">
                <span class="eyebrow">Sistema de gestión</span>
                <h1>Consultorio<br />de Atención Médica</h1>
                <p>Turnos, historias clínicas, especialistas y reportes, todo en un mismo sistema.</p>

                <svg class="ecg-line" viewBox="0 0 380 70" xmlns="http://www.w3.org/2000/svg">
                    <path d="M0,35 L90,35 L105,10 L120,60 L135,35 L160,35 L172,20 L184,50 L196,35 L380,35" />
                </svg>

                <div class="landing-actions">
                    <a runat="server" href="~/Seguridad/Login.aspx" class="btn btn-lg">Iniciar sesión</a>
                    <a runat="server" href="~/Seguridad/Registro.aspx" class="btn btn-lg btn-outline">Registrarse</a>
                </div>
            </div>
        </header>

                <section id="servicios" class="landing-section">
            <span class="eyebrow eyebrow-dark">Nuestros servicios</span>
            <h2>Atención en las principales especialidades</h2>
            <p class="section-intro">Contamos con profesionales matriculados y equipamiento para el diagnóstico y seguimiento de cada paciente. Hacé clic en un servicio para conocerlo en detalle.</p>

            <div class="servicios-grid">
                <a runat="server" href="~/DetalleServicio.aspx?id=1" class="servicio-card servicio-link">
                    <div class="servicio-img"><img src="public/servicios/servicio1.png" alt="Cardiología" /></div>
                    <h3>Cardiología</h3>
                    <p>Controles, electrocardiogramas y seguimiento de pacientes con afecciones cardíacas.</p>
                </a>
                <a runat="server" href="~/DetalleServicio.aspx?id=2" class="servicio-card servicio-link">
                    <div class="servicio-img"><img src="public/servicios/servicio2.png" alt="Pediatría" /></div>
                    <h3>Pediatría</h3>
                    <p>Atención integral de niños y adolescentes, controles de crecimiento y vacunación.</p>
                </a>
                <a runat="server" href="~/DetalleServicio.aspx?id=3" class="servicio-card servicio-link">
                    <div class="servicio-img"><img src="public/servicios/servicio3.png" alt="Traumatología" /></div>
                    <h3>Traumatología</h3>
                    <p>Diagnóstico y tratamiento de lesiones, rehabilitación y seguimiento kinesiológico.</p>
                </a>
                <a runat="server" href="~/DetalleServicio.aspx?id=4" class="servicio-card servicio-link">
                    <div class="servicio-img"><img src="public/servicios/servicio4.png" alt="Clínica Médica" /></div>
                    <h3>Clínica Médica</h3>
                    <p>Consultas generales, chequeos anuales y seguimiento de enfermedades crónicas.</p>
                </a>
                <a runat="server" href="~/DetalleServicio.aspx?id=5" class="servicio-card servicio-link">
                    <div class="servicio-img"><img src="public/servicios/servicio5.png" alt="Dermatología" /></div>
                    <h3>Dermatología</h3>
                    <p>Control de lunares, tratamiento de afecciones de la piel y dermatología estética.</p>
                </a>
                <a runat="server" href="~/DetalleServicio.aspx?id=6" class="servicio-card servicio-link">
                    <div class="servicio-img"><img src="public/servicios/servicio6.png" alt="Kinesiología" /></div>
                    <h3>Kinesiología</h3>
                    <p>Rehabilitación de lesiones, fisioterapia y planes de recuperación personalizados.</p>
                </a>
            </div>

            <div class="servicios-actions">
                <a runat="server" href="~/Servicios.aspx" class="btn btn-lg">Ver más servicios</a>
            </div>
        </section>

        <section id="nosotros" class="landing-section section-alt">
            <div class="nosotros-grid">
                <div>
                    <span class="eyebrow eyebrow-dark">Sobre nosotros</span>
                    <h2>Un consultorio pensado alrededor del paciente</h2>
                    <p>Desde hace más de 15 años acompañamos a las familias de la zona con atención médica cercana y profesional. Nuestro equipo de especialistas trabaja de forma coordinada: cada consulta queda registrada en tu historia clínica, para que cualquier médico del consultorio tenga tu información completa al momento de atenderte.</p>
                    <p>Con el sistema de turnos online podés solicitar tu consulta en cualquier momento, elegir especialista, y seguir el estado de tus turnos desde tu cuenta.</p>
                </div>
                <div class="nosotros-panel">
                    <svg class="ecg-line" viewBox="0 0 380 70" xmlns="http://www.w3.org/2000/svg">
                        <path d="M0,35 L90,35 L105,10 L120,60 L135,35 L160,35 L172,20 L184,50 L196,35 L380,35" />
                    </svg>
                    <div class="nosotros-datos">
                        <div><strong>+15</strong><span>años de trayectoria</span></div>
                        <div><strong>3</strong><span>especialidades</span></div>
                        <div><strong>100%</strong><span>turnos online</span></div>
                    </div>
                </div>
            </div>
        </section>

        <section id="turnos" class="landing-section">
            <div class="turno-cta">
                <span class="eyebrow">Sacar turno</span>
                <h2>Tu próximo turno, a un clic</h2>
                <p>Para solicitar un turno necesitás una cuenta: iniciá sesión y elegí especialidad, médico, fecha y horario. Si todavía no tenés cuenta, crearla te lleva un minuto.</p>
                <div class="landing-actions">
                    <a runat="server" href="~/Seguridad/Login.aspx" class="btn btn-lg">Iniciar sesión para sacar turno</a>
                    <a runat="server" href="~/Seguridad/Registro.aspx" class="btn btn-lg btn-outline">Crear cuenta</a>
                </div>
            </div>
        </section>

        <footer class="landing-footer">
            <div class="footer-inner">
                <a runat="server" href="~/Seguridad/Login.aspx" class="footer-brand" title="Iniciar sesión">
                    <svg class="brand-mark" viewBox="0 0 60 24" xmlns="http://www.w3.org/2000/svg">
                        <path d="M0,12 L18,12 L24,4 L30,20 L36,12 L60,12" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" />
                    </svg>
                    Consultorio de Atención Médica
                </a>

                <ul class="footer-links">
                    <li><a href="#servicios">Servicios</a></li>
                    <li><a href="#nosotros">Sobre nosotros</a></li>
                    <li><a href="#turnos">Sacar turno</a></li>
                    <li><a runat="server" href="~/Seguridad/Login.aspx">Iniciar sesión</a></li>
                </ul>

                <p class="footer-copy">© 2026 Consultorio de Atención Médica — Proyecto académico USAL</p>
            </div>
        </footer>

    </form>
</body>
</html>
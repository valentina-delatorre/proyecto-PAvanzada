<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="Servicios.aspx.cs" Inherits="Consultorio2026.Servicios" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Consultorio — Servicios</title>
    <link runat="server" href="~/Content/Theme.css" rel="stylesheet" />
    <link runat="server" href="~/Content/Home.css" rel="stylesheet" />
</head>
<body class="landing-body">
    <form id="form1" runat="server">

        <nav class="subnav">
            <a runat="server" href="~/Default.aspx" class="landing-nav-brand">
                <svg class="brand-mark" viewBox="0 0 60 24" xmlns="http://www.w3.org/2000/svg">
                    <path d="M0,12 L18,12 L24,4 L30,20 L36,12 L60,12" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" />
                </svg>
                Consultorio
            </a>
            <div class="landing-nav-actions">
                <a runat="server" href="~/Seguridad/Login.aspx" class="nav-link-login">Iniciar sesión</a>
                <a runat="server" href="~/Seguridad/Registro.aspx" class="btn btn-sm">Registrarse</a>
            </div>
        </nav>

        <div class="landing-section">
            <span class="eyebrow eyebrow-dark">Todos nuestros servicios</span>
            <h2>Conocé todo lo que podemos hacer por vos</h2>
            <p class="section-intro">Hacé clic en cualquier servicio para ver el detalle y agendar tu turno.</p>

            <div class="servicios-grid">
                <asp:Repeater ID="RpServicios" runat="server">
                    <ItemTemplate>
                        <a class="servicio-card servicio-link" href='<%# ResolveUrl("~/DetalleServicio.aspx?id=" + Eval("Id")) %>'>
                            <div class="servicio-img">
                                <img src='<%# ResolveUrl("~/public/servicios/" + Eval("ImagenCard")) %>' alt='<%# Eval("Nombre") %>' />
                            </div>
                            <h3><%# Eval("Nombre") %></h3>
                            <p><%# Eval("Resumen") %></p>
                        </a>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>

    </form>
</body>
</html>
<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="DetalleServicio.aspx.cs" Inherits="Consultorio2026.DetalleServicio" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Consultorio — Servicio</title>
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
                <a runat="server" href="~/Servicios.aspx" class="nav-link-login">← Todos los servicios</a>
            </div>
        </nav>

        <div class="detalle-wrap">
            <span class="eyebrow eyebrow-dark">Servicio</span>
            <h1><asp:Literal ID="LtNombre" runat="server" /></h1>
            <p class="detalle-desc"><asp:Literal ID="LtDetalle" runat="server" /></p>

            <div class="detalle-galeria">
                <asp:Repeater ID="RpImagenes" runat="server">
                    <ItemTemplate>
                        <img src='<%# ResolveUrl("~/public/detallesServicios/" + Container.DataItem) %>' alt="Imagen del servicio" />
                    </ItemTemplate>
                </asp:Repeater>
            </div>

            <div class="detalle-cta">
                <asp:Button ID="BtAgendar" runat="server" Text="Agendar turno" OnClick="BtAgendar_Click" CssClass="btn btn-lg" />
            </div>
        </div>

    </form>
</body>
</html>
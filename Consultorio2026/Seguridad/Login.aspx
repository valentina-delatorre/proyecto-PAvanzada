<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Consultorio2026.Seguridad.Login" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <title>Consultorio — Iniciar sesión</title>
    <link runat="server" href="~/Content/Theme.css" rel="stylesheet" />
    <link runat="server" href="~/Seguridad/Login.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-wrap">

            <div class="login-hero">
                <h1>Consultorio<br />de Atención Médica</h1>
                <p>Gestión de turnos, historias clínicas y especialistas en un solo lugar.</p>

                <svg class="ecg-line" viewBox="0 0 380 70" xmlns="http://www.w3.org/2000/svg">
                    <path d="M0,35 L90,35 L105,10 L120,60 L135,35 L160,35 L172,20 L184,50 L196,35 L380,35" />
                </svg>
            </div>

            <div class="login-form-side">
                <div class="login-card">
                    <h2>Iniciar sesión</h2>
                    <p class="subtitle">Ingresá tus credenciales para continuar</p>

                    <div class="field">
                        <label for="<%= TxtUsuario.ClientID %>">Usuario</label>
                        <asp:TextBox ID="TxtUsuario" runat="server" />
                    </div>

                    <div class="field">
                        <label for="<%= TxtContrasenia.ClientID %>">Contraseña</label>
                        <asp:TextBox ID="TxtContrasenia" runat="server" TextMode="Password" />
                    </div>

                    <asp:Button ID="BtIngresar" runat="server" Text="Ingresar" OnClick="BtIngresar_Click" CssClass="btn btn-ingresar" />

                    <asp:Label ID="LbError" runat="server" CssClass="error-msg" />

                    <a runat="server" href="~/Seguridad/Registro.aspx" class="back-link registro-link">¿No tenés cuenta? <strong>Registrate</strong></a>
                    <a runat="server" href="~/Default.aspx" class="back-link">← Volver al inicio</a>
                </div>
            </div>

        </div>
    </form>
</body>
</html>
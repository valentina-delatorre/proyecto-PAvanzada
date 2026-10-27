<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="Registro.aspx.cs" Inherits="Consultorio2026.Seguridad.Registro" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <title>Consultorio — Registrarse</title>
    <link rel="icon" href="data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 32 32'%3E%3Crect width='32' height='32' rx='7' fill='%231f4b44'/%3E%3Cpath d='M4 17h7l2-6 3 11 2-5h10' fill='none' stroke='%23d7a34e' stroke-width='2.5' stroke-linecap='round' stroke-linejoin='round'/%3E%3C/svg%3E" />
    <link runat="server" href="~/Content/Theme.css" rel="stylesheet" />
    <link runat="server" href="~/Seguridad/Login.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-wrap">

            <div class="login-hero">
                <h1>Creá tu cuenta</h1>
                <p>Registrate para solicitar turnos y seguir tus consultas.</p>
                <svg class="ecg-line" viewBox="0 0 380 70" xmlns="http://www.w3.org/2000/svg">
                    <path d="M0,35 L90,35 L105,10 L120,60 L135,35 L160,35 L172,20 L184,50 L196,35 L380,35" />
                </svg>
            </div>

            <div class="login-form-side">
                <div class="login-card">
                    <h2>Registrarse</h2>
                    <p class="subtitle">Completá tus datos para crear la cuenta</p>

                    <div class="field">
                        <label for="<%= TxtCorreo.ClientID %>">Correo electrónico</label>
                        <asp:TextBox ID="TxtCorreo" runat="server" TextMode="Email" />
                    </div>

                    <div class="field">
                        <label for="<%= TxtContrasenia.ClientID %>">Contraseña</label>
                        <asp:TextBox ID="TxtContrasenia" runat="server" TextMode="Password" />
                    </div>

                    <div class="field">
                        <label for="<%= TxtConfirmar.ClientID %>">Repetir contraseña</label>
                        <asp:TextBox ID="TxtConfirmar" runat="server" TextMode="Password" />
                    </div>

                    <asp:Button ID="BtRegistrarse" runat="server" Text="Crear cuenta" OnClick="BtRegistrarse_Click" CssClass="btn btn-ingresar" />

                    <asp:Label ID="LbError" runat="server" CssClass="error-msg" />

                    <a runat="server" href="~/Seguridad/Login.aspx" class="back-link">¿Ya tenés cuenta? Iniciá sesión</a>
                </div>
            </div>

        </div>
    </form>
</body>
</html>
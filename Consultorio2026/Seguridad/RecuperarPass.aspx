<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="RecuperarPass.aspx.cs" Inherits="Consultorio2026.Seguridad.RecuperarPass" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <title>Consultorio — Recuperar contraseña</title>
    <link runat="server" href="~/Content/Theme.css" rel="stylesheet" />
    <link runat="server" href="~/Seguridad/Login.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-wrap">

            <div class="login-hero">
                <h1>Recuperar<br />contraseña</h1>
                <p>Te mandamos un código a tu correo para que puedas elegir una nueva.</p>
                <svg class="ecg-line" viewBox="0 0 380 70" xmlns="http://www.w3.org/2000/svg">
                    <path d="M0,35 L90,35 L105,10 L120,60 L135,35 L160,35 L172,20 L184,50 L196,35 L380,35" />
                </svg>
            </div>

            <div class="login-form-side">
                <div class="login-card">

                    <asp:Panel ID="PnPaso1" runat="server">
                        <h2>¿Olvidaste tu contraseña?</h2>
                        <p class="subtitle">Escribí el correo con el que entrás y te mandamos un código</p>

                        <div class="field">
                            <label for="<%= TxtCorreo.ClientID %>">Correo electrónico</label>
                            <asp:TextBox ID="TxtCorreo" runat="server" TextMode="Email" />
                        </div>

                        <asp:Button ID="BtEnviarCodigo" runat="server" Text="Enviarme el código" OnClick="BtEnviarCodigo_Click" CssClass="btn btn-ingresar" />
                    </asp:Panel>

                    <asp:Panel ID="PnPaso2" runat="server" Visible="false">
                        <h2>Revisá tu correo</h2>
                        <p class="subtitle">Pegá el código de 6 dígitos y elegí tu contraseña nueva. El código vence en 30 minutos.</p>

                        <div class="field">
                            <label for="<%= TxtCodigo.ClientID %>">Código</label>
                            <asp:TextBox ID="TxtCodigo" runat="server" MaxLength="6" />
                        </div>

                        <div class="field">
                            <label for="<%= TxtPassNueva.ClientID %>">Contraseña nueva</label>
                            <asp:TextBox ID="TxtPassNueva" runat="server" TextMode="Password" />
                        </div>

                        <div class="field">
                            <label for="<%= TxtPassRepetir.ClientID %>">Repetir contraseña</label>
                            <asp:TextBox ID="TxtPassRepetir" runat="server" TextMode="Password" />
                        </div>

                        <asp:Button ID="BtConfirmar" runat="server" Text="Cambiar contraseña" OnClick="BtConfirmar_Click" CssClass="btn btn-ingresar" />
                        <asp:Button ID="BtCancelar" runat="server" Text="Volver a empezar" OnClick="BtCancelar_Click" CssClass="back-link" CausesValidation="false" />
                    </asp:Panel>

                    <asp:Label ID="LbMsj" runat="server" CssClass="ok-msg" />
                    <asp:Label ID="LbError" runat="server" CssClass="error-msg" />

                    <a runat="server" href="~/Seguridad/Login.aspx" class="back-link">← Volver a iniciar sesión</a>
                </div>
            </div>

        </div>
    </form>
</body>
</html>

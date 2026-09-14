<%@ Page Title="" Language="C#" MasterPageFile="~/site.master" AutoEventWireup="true" CodeBehind="MiCuenta.aspx.cs" Inherits="Consultorio2026.Cuenta.MiCuenta" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <h2>Mi cuenta</h2>
    <p class="subtitle-line">Tu información y la configuración de tu acceso al sistema.</p>

    <div class="card">
        <h3>Mis datos</h3>
        <asp:Label ID="LbDatos" runat="server" CssClass="subtitle-line" />
    </div>

    <div class="card">
        <h3>Datos de contacto</h3>
        <p class="subtitle-line">A este mail o teléfono te va a llegar el código cuando quieras cambiar tu contraseña.</p>
        <div class="form-grid">
            <div class="field">
                <label>Email</label>
                <asp:TextBox ID="TxtEmail" runat="server" />
            </div>
            <div class="field">
                <label>Teléfono</label>
                <asp:TextBox ID="TxtTelefono" runat="server" />
            </div>
        </div>
        <asp:Button ID="BtGuardarContacto" runat="server" Text="Guardar contacto" OnClick="BtGuardarContacto_Click" CssClass="btn" />
        <asp:Label ID="LbMsjContacto" runat="server" CssClass="ok-msg" />
    </div>

    <div class="card">
        <h3>Cambiar nombre de usuario</h3>
        <div class="form-grid">
            <div class="field">
                <label>Nuevo nombre de usuario</label>
                <asp:TextBox ID="TxtNuevoUsuario" runat="server" />
            </div>
            <div class="field">
                <label>Tu contraseña actual (por seguridad)</label>
                <asp:TextBox ID="TxtPassParaUsuario" runat="server" TextMode="Password" />
            </div>
        </div>
        <asp:Button ID="BtCambiarUsuario" runat="server" Text="Cambiar nombre de usuario" OnClick="BtCambiarUsuario_Click" CssClass="btn" />
        <asp:Label ID="LbMsjUsuario" runat="server" CssClass="ok-msg" />
        <asp:Label ID="LbErrUsuario" runat="server" CssClass="error-msg" />
    </div>

    <div class="card">
        <h3>Cambiar contraseña</h3>

        <asp:Panel ID="PnPaso1" runat="server">
            <p class="subtitle-line">Paso 1 de 2: ingresá tu contraseña actual y la nueva. Te vamos a enviar un código de verificación.</p>
            <div class="form-grid">
                <div class="field">
                    <label>Contraseña actual</label>
                    <asp:TextBox ID="TxtPassActual" runat="server" TextMode="Password" />
                </div>
                <div class="field">
                    <label>Nueva contraseña</label>
                    <asp:TextBox ID="TxtPassNueva" runat="server" TextMode="Password" />
                </div>
                <div class="field">
                    <label>Repetir nueva contraseña</label>
                    <asp:TextBox ID="TxtPassRepetir" runat="server" TextMode="Password" />
                </div>
            </div>
            <asp:Button ID="BtEnviarCodigo" runat="server" Text="Enviar código de verificación" OnClick="BtEnviarCodigo_Click" CssClass="btn" />
        </asp:Panel>

        <asp:Panel ID="PnPaso2" runat="server" Visible="false">
            <p class="subtitle-line">Paso 2 de 2: ingresá el código de 6 dígitos que te enviamos. Vence en 10 minutos.</p>
            <asp:Panel ID="PnDemoCodigo" runat="server" CssClass="demo-codigo">
                <strong>MODO DEMO (sin servidor de mail configurado):</strong> tu código es
                <span class="demo-codigo-num"><asp:Literal ID="LtCodigoDemo" runat="server" /></span>
            </asp:Panel>
            <div class="form-grid">
                <div class="field">
                    <label>Código de verificación</label>
                    <asp:TextBox ID="TxtCodigo" runat="server" />
                </div>
            </div>
            <asp:Button ID="BtConfirmarCambio" runat="server" Text="Confirmar cambio de contraseña" OnClick="BtConfirmarCambio_Click" CssClass="btn" />
            <asp:Button ID="BtCancelarCambio" runat="server" Text="Cancelar" OnClick="BtCancelarCambio_Click" CssClass="btn btn-secondary" />
        </asp:Panel>

        <asp:Label ID="LbMsjPass" runat="server" CssClass="ok-msg" />
        <asp:Label ID="LbErrPass" runat="server" CssClass="error-msg" />
    </div>

</asp:Content>
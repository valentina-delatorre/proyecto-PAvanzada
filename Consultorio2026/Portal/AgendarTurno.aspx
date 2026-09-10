<%@ Page Title="" Language="C#" MasterPageFile="~/site.master" AutoEventWireup="true" CodeBehind="AgendarTurno.aspx.cs" Inherits="Consultorio2026.Portal.AgendarTurno" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <h2>Agendar turno</h2>
    <p class="subtitle-line">Completá tus datos y elegí el turno que mejor te quede. Todos los campos con * son obligatorios.</p>

    <div class="card">
        <h3>Tus datos personales</h3>
        <div class="form-grid">
            <div class="field">
                <label>Nombre *</label>
                <asp:TextBox ID="TxtNombre" runat="server" />
            </div>
            <div class="field">
                <label>Apellido *</label>
                <asp:TextBox ID="TxtApellido" runat="server" />
            </div>
            <div class="field">
                <label>DNI *</label>
                <asp:TextBox ID="TxtDni" runat="server" />
            </div>
            <div class="field">
                <label>Teléfono</label>
                <asp:TextBox ID="TxtTelefono" runat="server" />
            </div>
            <div class="field">
                <label>Email</label>
                <asp:TextBox ID="TxtEmail" runat="server" />
            </div>
        </div>
    </div>

    <div class="card">
        <h3>Tu turno</h3>
        <div class="form-grid">
            <div class="field">
                <label>Especialidad *</label>
                <asp:DropDownList ID="DdlEspecialidad" runat="server" AutoPostBack="true"
                    OnSelectedIndexChanged="DdlEspecialidad_SelectedIndexChanged" CssClass="input-select" />
            </div>
            <div class="field">
                <label>Profesional</label>
                <asp:DropDownList ID="DdlMedico" runat="server" CssClass="input-select" />
            </div>
            <div class="field">
                <label>Tipo de consulta *</label>
                <asp:DropDownList ID="DdlTipoConsulta" runat="server" CssClass="input-select">
                    <asp:ListItem>Primera consulta</asp:ListItem>
                    <asp:ListItem>Control</asp:ListItem>
                    <asp:ListItem>Estudio</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="field">
                <label>Fecha *</label>
                <asp:TextBox ID="TxtFecha" runat="server" TextMode="Date" />
            </div>
            <div class="field">
                <label>Hora *</label>
                <asp:DropDownList ID="DdlHora" runat="server" CssClass="input-select">
                    <asp:ListItem>09:00</asp:ListItem>
                    <asp:ListItem>10:00</asp:ListItem>
                    <asp:ListItem>11:00</asp:ListItem>
                    <asp:ListItem>12:00</asp:ListItem>
                    <asp:ListItem>15:00</asp:ListItem>
                    <asp:ListItem>16:00</asp:ListItem>
                    <asp:ListItem>17:00</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="field field-full">
                <label>Observaciones</label>
                <asp:TextBox ID="TxtObservaciones" runat="server" TextMode="MultiLine" Rows="2" />
            </div>
        </div>

        <asp:Button ID="BtConfirmar" runat="server" Text="Confirmar solicitud de turno" OnClick="BtConfirmar_Click" CssClass="btn" />
        <asp:HyperLink ID="LkMisTurnos" runat="server" NavigateUrl="~/Portal/PortalPaciente.aspx" Visible="false" CssClass="back-link" Text="Ver mis turnos →" />
        <asp:Label ID="LbMensaje" runat="server" CssClass="ok-msg" />
        <asp:Label ID="LbError" runat="server" CssClass="error-msg" />
    </div>

</asp:Content>
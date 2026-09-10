<%@ Page Title="" Language="C#" MasterPageFile="~/site.master" AutoEventWireup="true" CodeBehind="PortalPaciente.aspx.cs" Inherits="Consultorio2026.Portal.PortalPaciente" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="portal-hero">
        <h2>Consultorio de Atención Médica</h2>
        <p>Atendemos con especialistas en cardiología, pediatría y traumatología. Solicitá tu turno online y seguí el estado de tus consultas desde acá.</p>
    </div>

    <div class="card">
        <h3>Solicitar un turno</h3>

        <div class="form-grid">
            <div class="field">
                <label>Especialidad</label>
                <asp:DropDownList ID="DdlEspecialidad" runat="server" AutoPostBack="true"
                    OnSelectedIndexChanged="DdlEspecialidad_SelectedIndexChanged" CssClass="input-select" />
            </div>

            <div class="field">
                <label>Médico (opcional)</label>
                <asp:DropDownList ID="DdlMedico" runat="server" CssClass="input-select" />
            </div>

            <div class="field">
                <label>Tipo de consulta</label>
                <asp:DropDownList ID="DdlTipoConsulta" runat="server" CssClass="input-select">
                    <asp:ListItem>Primera consulta</asp:ListItem>
                    <asp:ListItem>Control</asp:ListItem>
                    <asp:ListItem>Estudio</asp:ListItem>
                </asp:DropDownList>
            </div>

            <div class="field">
                <label>Fecha</label>
                <asp:TextBox ID="TxtFecha" runat="server" TextMode="Date" />
            </div>

            <div class="field">
                <label>Hora</label>
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

        <asp:Button ID="BtSolicitar" runat="server" Text="Solicitar turno" OnClick="BtSolicitar_Click" CssClass="btn" />
        <asp:Label ID="LbMensaje" runat="server" CssClass="ok-msg" />
        <asp:Label ID="LbError" runat="server" CssClass="error-msg" />
    </div>

    <div class="card">
        <h3>Mis turnos</h3>
        <asp:GridView ID="GvMisTurnos" runat="server" AutoGenerateColumns="false" CssClass="data-table"
            EmptyDataText="Todavía no solicitaste ningún turno.">
            <Columns>
                <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:BoundField DataField="Hora" HeaderText="Hora" />
                <asp:BoundField DataField="Especialidad" HeaderText="Especialidad" />
                <asp:BoundField DataField="NombreMedico" HeaderText="Médico" />
                <asp:BoundField DataField="TipoConsulta" HeaderText="Tipo" />
                <asp:BoundField DataField="Estado" HeaderText="Estado" />
            </Columns>
        </asp:GridView>
    </div>

</asp:Content>
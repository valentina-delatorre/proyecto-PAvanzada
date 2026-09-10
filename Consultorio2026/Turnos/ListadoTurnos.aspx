<%@ Page Title="" Language="C#" MasterPageFile="~/site.master" AutoEventWireup="true" CodeBehind="ListadoTurnos.aspx.cs" Inherits="Consultorio2026.Turnos.ListadoTurnos" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h2>Solicitudes de turno pendientes</h2>
    <p>Seleccioná una solicitud para asignarle médico y confirmarla o rechazarla.</p>

    <asp:GridView ID="GvSolicitudes" runat="server" AutoGenerateColumns="false" CssClass="data-table"
        DataKeyNames="IdTurno" OnSelectedIndexChanged="GvSolicitudes_SelectedIndexChanged"
        EmptyDataText="No hay solicitudes pendientes.">
        <Columns>
            <asp:BoundField DataField="NombrePaciente" HeaderText="Paciente" />
            <asp:BoundField DataField="Especialidad" HeaderText="Especialidad" />
            <asp:BoundField DataField="TipoConsulta" HeaderText="Tipo" />
            <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
            <asp:BoundField DataField="Hora" HeaderText="Hora" />
            <asp:BoundField DataField="NombreMedico" HeaderText="Médico pedido" />
            <asp:CommandField ShowSelectButton="true" SelectText="Gestionar" />
        </Columns>
    </asp:GridView>

    <asp:Panel ID="PnGestion" runat="server" Visible="false" CssClass="card">
        <h3>Gestionar solicitud</h3>
        <asp:Label ID="LbDetalle" runat="server" />

        <div class="field" style="max-width: 320px; margin-top: 1rem;">
            <label>Asignar médico</label>
            <asp:DropDownList ID="DdlMedicoAsignar" runat="server" CssClass="input-select" />
        </div>

        <asp:Button ID="BtConfirmar" runat="server" Text="Confirmar turno" OnClick="BtConfirmar_Click" CssClass="btn" />
        <asp:Button ID="BtRechazar" runat="server" Text="Rechazar" OnClick="BtRechazar_Click" CssClass="btn btn-secondary" />
        <asp:Label ID="LbError" runat="server" CssClass="error-msg" />
    </asp:Panel>
        <div class="card">
        <h3>Historial completo de turnos</h3>
        <asp:GridView ID="GvHistorial" runat="server" AutoGenerateColumns="false" CssClass="data-table"
            EmptyDataText="No hay turnos en el sistema.">
            <Columns>
                <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:BoundField DataField="Hora" HeaderText="Hora" />
                <asp:BoundField DataField="NombrePaciente" HeaderText="Paciente" />
                <asp:BoundField DataField="NombreMedico" HeaderText="Especialista" />
                <asp:BoundField DataField="Especialidad" HeaderText="Especialidad" />
                <asp:BoundField DataField="Estado" HeaderText="Estado" />
                <asp:BoundField DataField="EstadoVisual" HeaderText="Situación" />
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>
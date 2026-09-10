<%@ Page Title="" Language="C#" MasterPageFile="~/site.master" AutoEventWireup="true" CodeBehind="ListadoEspecialistas.aspx.cs" Inherits="Consultorio2026.Especialistas.ListadoEspecialistas" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h2>Módulo de Especialistas</h2>
    <p class="subtitle-line">Cartilla de profesionales del consultorio. Seleccioná uno para ver su agenda completa.</p>

    <div class="card">
        <h3>Profesionales</h3>
        <asp:GridView ID="GvMedicos" runat="server" AutoGenerateColumns="false" CssClass="data-table"
            DataKeyNames="IdMedico" OnSelectedIndexChanged="GvMedicos_SelectedIndexChanged"
            EmptyDataText="No hay especialistas cargados.">
            <Columns>
                <asp:BoundField DataField="NombreCompleto" HeaderText="Profesional" />
                <asp:BoundField DataField="Especialidad" HeaderText="Especialidad" />
                <asp:BoundField DataField="UsuarioVinculado" HeaderText="Usuario del sistema" />
                <asp:TemplateField HeaderText="Activo">
                    <ItemTemplate><%# (bool)Eval("Activo") ? "Sí" : "No" %></ItemTemplate>
                </asp:TemplateField>
                <asp:CommandField ShowSelectButton="true" SelectText="Ver agenda" />
            </Columns>
        </asp:GridView>
    </div>

    <asp:Panel ID="PnAgenda" runat="server" Visible="false" CssClass="card">
        <h3>Agenda del profesional</h3>
        <asp:Label ID="LbDetalleMedico" runat="server" CssClass="subtitle-line" />
        <asp:Button ID="BtCambiarActivoMedico" runat="server" OnClick="BtCambiarActivoMedico_Click" CssClass="btn btn-secondary" />

        <asp:GridView ID="GvAgenda" runat="server" AutoGenerateColumns="false" CssClass="data-table" style="margin-top: 1.2rem;"
            EmptyDataText="Este profesional no tiene turnos asignados.">
            <Columns>
                <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:BoundField DataField="Hora" HeaderText="Hora" />
                <asp:BoundField DataField="NombrePaciente" HeaderText="Paciente" />
                <asp:BoundField DataField="TipoConsulta" HeaderText="Servicio" />
                <asp:BoundField DataField="Estado" HeaderText="Estado" />
                <asp:BoundField DataField="EstadoVisual" HeaderText="Situación" />
            </Columns>
        </asp:GridView>
    </asp:Panel>

    <div class="card">
        <h3>Alta de nuevo especialista</h3>
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
                <label>Especialidad *</label>
                <asp:TextBox ID="TxtEspecialidad" runat="server" />
            </div>
            <div class="field">
                <label>Vincular a usuario (rol Medico)</label>
                <asp:DropDownList ID="DdlUsuarioVincular" runat="server" CssClass="input-select" />
            </div>
        </div>
        <asp:Button ID="BtAgregarMedico" runat="server" Text="Agregar especialista" OnClick="BtAgregarMedico_Click" CssClass="btn" />
        <asp:Label ID="LbMensaje" runat="server" CssClass="ok-msg" />
        <asp:Label ID="LbError" runat="server" CssClass="error-msg" />
    </div>
</asp:Content>
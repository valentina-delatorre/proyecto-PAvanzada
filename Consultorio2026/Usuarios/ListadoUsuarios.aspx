<%@ Page Title="" Language="C#" MasterPageFile="~/site.master" AutoEventWireup="true" CodeBehind="ListadoUsuarios.aspx.cs" Inherits="Consultorio2026.Usuarios.ListadoUsuarios" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h2>Módulo de Usuarios</h2>
    <p class="subtitle-line">Todos los usuarios del sistema, ordenados del más nuevo al más viejo. Seleccioná uno para ver su detalle y sus turnos.</p>

    <div class="card">
        <h3>Usuarios del sistema</h3>
        <asp:GridView ID="GvUsuarios" runat="server" AutoGenerateColumns="false" CssClass="data-table"
            DataKeyNames="IdUsuario" OnSelectedIndexChanged="GvUsuarios_SelectedIndexChanged"
            EmptyDataText="No hay usuarios cargados.">
            <Columns>
                <asp:BoundField DataField="NombreUsuario" HeaderText="Usuario" />
                <asp:BoundField DataField="Rol" HeaderText="Rol" />
                <asp:BoundField DataField="NombreCompleto" HeaderText="Nombre y apellido" />
                <asp:BoundField DataField="Dni" HeaderText="DNI" />
                <asp:BoundField DataField="Especialidad" HeaderText="Especialidad (si es médico)" />
                <asp:TemplateField HeaderText="Activo">
                    <ItemTemplate><%# (bool)Eval("Activo") ? "Sí" : "No" %></ItemTemplate>
                </asp:TemplateField>
                <asp:CommandField ShowSelectButton="true" SelectText="Ver detalle" />
            </Columns>
        </asp:GridView>
    </div>

    <asp:Panel ID="PnDetalle" runat="server" Visible="false" CssClass="card">
        <h3>Detalle del usuario</h3>
        <asp:Label ID="LbDetalleUsuario" runat="server" CssClass="subtitle-line" />

        <asp:Button ID="BtCambiarActivo" runat="server" OnClick="BtCambiarActivo_Click" CssClass="btn btn-secondary" />

        <h3 style="margin-top: 1.8rem;">Turnos de este usuario</h3>
        <asp:GridView ID="GvTurnosUsuario" runat="server" AutoGenerateColumns="false" CssClass="data-table"
            EmptyDataText="Este usuario no tiene turnos en el sistema.">
            <Columns>
                <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:BoundField DataField="Hora" HeaderText="Hora" />
                <asp:BoundField DataField="Especialidad" HeaderText="Especialidad" />
                <asp:BoundField DataField="TipoConsulta" HeaderText="Servicio" />
                <asp:BoundField DataField="NombreMedico" HeaderText="Especialista" />
                <asp:BoundField DataField="Estado" HeaderText="Estado" />
                <asp:BoundField DataField="EstadoVisual" HeaderText="Situación" />
            </Columns>
        </asp:GridView>
    </asp:Panel>
</asp:Content>
<%@ Page Title="" Language="C#" MasterPageFile="~/site.master" AutoEventWireup="true" CodeBehind="ReporteDiario.aspx.cs" Inherits="Consultorio2026.Reportes.ReporteDiario" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h2>Módulo de Reportes</h2>
    <p class="subtitle-line">Resumen de la actividad del consultorio. Los datos se actualizan solos: salen directo de los turnos cargados por pacientes y gestionados por el área de turnos.</p>

    <div class="card">
        <h3>Turnos de hoy — <asp:Literal ID="LtFechaHoy" runat="server" /></h3>
        <asp:GridView ID="GvHoy" runat="server" AutoGenerateColumns="false" CssClass="data-table"
            EmptyDataText="No hay turnos para el día de hoy.">
            <Columns>
                <asp:BoundField DataField="Hora" HeaderText="Hora" />
                <asp:BoundField DataField="NombrePaciente" HeaderText="Paciente" />
                <asp:BoundField DataField="NombreMedico" HeaderText="Especialista" />
                <asp:BoundField DataField="Especialidad" HeaderText="Especialidad" />
                <asp:BoundField DataField="TipoConsulta" HeaderText="Servicio" />
                <asp:BoundField DataField="Estado" HeaderText="Estado" />
            </Columns>
        </asp:GridView>
    </div>

    <div class="card">
        <h3>Totales por estado</h3>
        <asp:GridView ID="GvEstados" runat="server" AutoGenerateColumns="false" CssClass="data-table"
            EmptyDataText="Todavía no hay turnos en el sistema.">
            <Columns>
                <asp:BoundField DataField="Etiqueta" HeaderText="Estado" />
                <asp:BoundField DataField="Cantidad" HeaderText="Cantidad" />
            </Columns>
        </asp:GridView>
    </div>

    <div class="card">
        <h3>Turnos por especialidad</h3>
        <asp:GridView ID="GvEspecialidades" runat="server" AutoGenerateColumns="false" CssClass="data-table"
            EmptyDataText="Todavía no hay turnos en el sistema.">
            <Columns>
                <asp:BoundField DataField="Etiqueta" HeaderText="Especialidad" />
                <asp:BoundField DataField="Cantidad" HeaderText="Turnos" />
            </Columns>
        </asp:GridView>
    </div>

    <div class="card">
        <h3>Próximos turnos por especialista</h3>
        <asp:GridView ID="GvProximos" runat="server" AutoGenerateColumns="false" CssClass="data-table"
            EmptyDataText="No hay turnos confirmados a futuro.">
            <Columns>
                <asp:BoundField DataField="Etiqueta" HeaderText="Especialista" />
                <asp:BoundField DataField="Cantidad" HeaderText="Turnos agendados" />
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>
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
            <asp:DropDownList ID="DdlMedicoAsignar" runat="server" CssClass="input-select"
                AutoPostBack="true" OnSelectedIndexChanged="DdlMedicoAsignar_SelectedIndexChanged" />
        </div>

        <%-- Almanaque del especialista elegido: sus turnos ya confirmados en el mes del turno pedido --%>
        <asp:Panel ID="PnCalendario" runat="server" Visible="false">
            <h4 style="margin-bottom: 0.2rem;">Agenda de <asp:Literal ID="LtMedicoAgenda" runat="server" /> — <asp:Literal ID="LtMesAgenda" runat="server" /></h4>
            <p class="subtitle-line">En verde los días con turnos confirmados; el recuadro naranja es el día que pidió el paciente.</p>
            <div class="calendario">
                <div class="cal-cabecera">Lun</div><div class="cal-cabecera">Mar</div><div class="cal-cabecera">Mié</div>
                <div class="cal-cabecera">Jue</div><div class="cal-cabecera">Vie</div><div class="cal-cabecera">Sáb</div>
                <div class="cal-cabecera">Dom</div>
                <asp:Repeater ID="RpCalendario" runat="server">
                    <ItemTemplate>
                        <div class='<%# Eval("Clase") %>'>
                            <span class="cal-num"><%# Eval("Numero") %></span>
                            <span class="cal-horas"><%# Eval("Horarios") %></span>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </asp:Panel>

        <div style="margin-top: 1rem;">
            <asp:Button ID="BtConfirmar" runat="server" Text="Confirmar turno" OnClick="BtConfirmar_Click" CssClass="btn" />
            <asp:Button ID="BtRechazar" runat="server" Text="Rechazar" OnClick="BtRechazar_Click" CssClass="btn btn-secondary" />
        </div>
        <asp:Label ID="LbError" runat="server" CssClass="error-msg" />
    </asp:Panel>

    <div class="card">
        <h3>Historial completo de turnos</h3>

        <div class="form-grid">
            <div class="field">
                <label>Estado</label>
                <asp:DropDownList ID="DdlFiltroEstado" runat="server" CssClass="input-select" />
            </div>
            <div class="field">
                <label>Especialidad</label>
                <asp:DropDownList ID="DdlFiltroEspecialidad" runat="server" CssClass="input-select" />
            </div>
            <div class="field">
                <label>Paciente</label>
                <asp:TextBox ID="TxtFiltroPaciente" runat="server" placeholder="Nombre de usuario" />
            </div>
            <div class="field">
                <label>Desde</label>
                <asp:TextBox ID="TxtFiltroDesde" runat="server" TextMode="Date" />
            </div>
            <div class="field">
                <label>Hasta</label>
                <asp:TextBox ID="TxtFiltroHasta" runat="server" TextMode="Date" />
            </div>
        </div>
        <asp:Button ID="BtFiltrar" runat="server" Text="Filtrar" OnClick="BtFiltrar_Click" CssClass="btn btn-mini" />
        <asp:Button ID="BtLimpiar" runat="server" Text="Limpiar" OnClick="BtLimpiar_Click" CssClass="btn btn-mini btn-secondary" />
        <asp:Label ID="LbResultado" runat="server" CssClass="subtitle-line" />

        <asp:GridView ID="GvHistorial" runat="server" AutoGenerateColumns="false" CssClass="data-table"
            EmptyDataText="No hay turnos que coincidan con el filtro.">
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

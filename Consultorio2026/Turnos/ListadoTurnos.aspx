<%@ Page Title="" Language="C#" MasterPageFile="~/site.master" AutoEventWireup="true" CodeBehind="ListadoTurnos.aspx.cs" Inherits="Consultorio2026.Turnos.ListadoTurnos" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <section class="cabecera">
        <div>
            <span class="cabecera-eyebrow">Módulo de turnos</span>
            <h2>Gestión de turnos</h2>
            <p>Los pedidos de los pacientes llegan acá. Asignales un médico mirando su agenda y confirmalos o rechazalos.</p>
        </div>
        <dl class="cabecera-cifras">
            <div id="DivPorConfirmar" runat="server"><dt>Por confirmar</dt><dd><asp:Literal ID="LtPorConfirmar" runat="server" /></dd></div>
            <div><dt>Hoy</dt><dd><asp:Literal ID="LtHoy" runat="server" /></dd></div>
            <div><dt>Próx. 7 días</dt><dd><asp:Literal ID="LtSemana" runat="server" /></dd></div>
        </dl>
    </section>

    <div class="card">
        <h3>Esperando confirmación</h3>
        <p class="subtitle-line">Tocá "Gestionar" en un pedido para asignarle médico y resolverlo.</p>
        <div class="tabla-scroll">
            <asp:GridView ID="GvSolicitudes" runat="server" AutoGenerateColumns="false" CssClass="data-table"
                DataKeyNames="IdTurno" OnSelectedIndexChanged="GvSolicitudes_SelectedIndexChanged"
                SelectedRowStyle-CssClass="fila-elegida" EmptyDataText="No hay pedidos pendientes. Todo al día.">
                <Columns>
                    <asp:TemplateField HeaderText="Paciente">
                        <ItemTemplate>
                            <span class="celda-persona"><strong><%# Nombre(Eval("IdUsuarioPaciente")) %></strong><span><%# Eval("TipoConsulta") %></span></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Especialidad">
                        <ItemTemplate><span class="etiqueta"><%# Eval("Especialidad") %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Día y hora">
                        <ItemTemplate>
                            <span class="celda-persona"><strong><%# Eval("Fecha", "{0:dd/MM/yyyy}") %></strong><span><%# Eval("Hora") %> hs</span></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Médico pedido">
                        <ItemTemplate><%# Eval("NombreMedico").ToString() == "-" ? "<span class=\"texto-suave\">Sin preferencia</span>" : Eval("NombreMedico") %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:CommandField ShowSelectButton="true" SelectText="Gestionar" />
                </Columns>
            </asp:GridView>
        </div>
    </div>

    <asp:Panel ID="PnGestion" runat="server" Visible="false" CssClass="card card-destacada">
        <h3>Gestionar el pedido</h3>
        <asp:Literal ID="LtDetalle" runat="server" />

        <div class="field" style="max-width: 320px;">
            <label>Asignar médico</label>
            <asp:DropDownList ID="DdlMedicoAsignar" runat="server" CssClass="input-select"
                AutoPostBack="true" OnSelectedIndexChanged="DdlMedicoAsignar_SelectedIndexChanged" />
        </div>

        <%-- Almanaque del especialista elegido: sus turnos ya ocupados en el mes del turno pedido --%>
        <asp:Panel ID="PnCalendario" runat="server" Visible="false">
            <h4 class="cal-titulo">Agenda de <asp:Literal ID="LtMedicoAgenda" runat="server" /> · <asp:Literal ID="LtMesAgenda" runat="server" /></h4>
            <p class="subtitle-line">En verde los días que ya tienen turnos; el recuadro naranja es el día que pidió el paciente.</p>
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

        <div style="margin-top: 1.2rem;">
            <asp:Button ID="BtConfirmar" runat="server" Text="Confirmar turno" OnClick="BtConfirmar_Click" CssClass="btn" />
            <asp:Button ID="BtRechazar" runat="server" Text="Rechazar" OnClick="BtRechazar_Click" CssClass="btn btn-peligro" />
        </div>
        <asp:Label ID="LbError" runat="server" CssClass="error-msg" />
    </asp:Panel>

    <div class="card">
        <h3>Historial completo</h3>

        <div class="form-grid filtros">
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
                <asp:TextBox ID="TxtFiltroPaciente" runat="server" placeholder="Nombre o usuario" />
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
        <div class="filtros-acciones">
            <asp:Button ID="BtFiltrar" runat="server" Text="Filtrar" OnClick="BtFiltrar_Click" CssClass="btn btn-mini" />
            <asp:Button ID="BtLimpiar" runat="server" Text="Limpiar" OnClick="BtLimpiar_Click" CssClass="btn btn-mini btn-secondary" />
            <asp:Label ID="LbResultado" runat="server" CssClass="texto-suave" />
        </div>

        <div class="tabla-scroll">
            <asp:GridView ID="GvHistorial" runat="server" AutoGenerateColumns="false" CssClass="data-table"
                EmptyDataText="No hay turnos que coincidan con el filtro.">
                <Columns>
                    <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                    <asp:BoundField DataField="Hora" HeaderText="Hora" />
                    <asp:TemplateField HeaderText="Paciente">
                        <ItemTemplate><%# Nombre(Eval("IdUsuarioPaciente")) %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="NombreMedico" HeaderText="Especialista" />
                    <asp:BoundField DataField="Especialidad" HeaderText="Especialidad" />
                    <asp:TemplateField HeaderText="Estado">
                        <ItemTemplate><span class='punto punto-<%# Eval("Estado").ToString().ToLower() %>'><%# Eval("EstadoVisual") %></span></ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>
</asp:Content>

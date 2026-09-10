<%@ Page Title="" Language="C#" MasterPageFile="~/site.master" AutoEventWireup="true" CodeBehind="PanelMedico.aspx.cs" Inherits="Consultorio2026.Medicos.PanelMedico" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <h2>Mi panel</h2>
    <asp:Label ID="LbBienvenida" runat="server" CssClass="subtitle-line" />

    <asp:Panel ID="PnPanel" runat="server">

        <div class="stats-row">
            <div class="stat-chip"><strong><asp:Literal ID="LtHoy" runat="server" /></strong><span>turnos hoy</span></div>
            <div class="stat-chip"><strong><asp:Literal ID="LtProximos" runat="server" /></strong><span>próximos confirmados</span></div>
            <div class="stat-chip"><strong><asp:Literal ID="LtPendientes" runat="server" /></strong><span>solicitudes pendientes</span></div>
        </div>

        <div class="card">
            <h3>Solicitudes pendientes de tu decisión</h3>
            <p class="subtitle-line">Pacientes que pidieron turno con vos. Aceptalo para sumarlo a tu agenda, o rechazalo.</p>
            <asp:GridView ID="GvPendientes" runat="server" AutoGenerateColumns="false" CssClass="data-table"
                OnRowCommand="GvPendientes_RowCommand"
                EmptyDataText="No tenés solicitudes pendientes. 🎉">
                <Columns>
                    <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                    <asp:BoundField DataField="Hora" HeaderText="Hora" />
                    <asp:BoundField DataField="NombrePaciente" HeaderText="Paciente" />
                    <asp:BoundField DataField="TipoConsulta" HeaderText="Servicio" />
                    <asp:BoundField DataField="Observaciones" HeaderText="Observaciones" />
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:Button runat="server" Text="Aceptar" CssClass="btn btn-mini"
                                CommandName="Aceptar" CommandArgument='<%# Eval("IdTurno") %>' />
                            <asp:Button runat="server" Text="Rechazar" CssClass="btn btn-mini btn-secondary"
                                CommandName="Rechazar" CommandArgument='<%# Eval("IdTurno") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <div class="card">
            <h3>Mi agenda</h3>
            <asp:GridView ID="GvAgenda" runat="server" AutoGenerateColumns="false" CssClass="data-table"
                EmptyDataText="Todavía no tenés turnos en tu agenda.">
                <Columns>
                    <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                    <asp:BoundField DataField="Hora" HeaderText="Hora" />
                    <asp:BoundField DataField="NombrePaciente" HeaderText="Paciente" />
                    <asp:BoundField DataField="TipoConsulta" HeaderText="Servicio" />
                    <asp:BoundField DataField="Observaciones" HeaderText="Observaciones" />
                    <asp:TemplateField HeaderText="Situación">
                        <ItemTemplate>
                            <span class='<%# ClaseEstado(Eval("EstadoVisual").ToString()) %>'><%# Eval("EstadoVisual") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

    </asp:Panel>

</asp:Content>
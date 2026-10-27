<%@ Page Title="" Language="C#" MasterPageFile="~/site.master" AutoEventWireup="true" CodeBehind="PanelCardiologia.aspx.cs" Inherits="Consultorio2026.Medicos.PanelCardiologia" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <section class="cabecera">
        <div>
            <span class="cabecera-eyebrow">Panel de Cardiología</span>
            <h2><asp:Label ID="LbBienvenida" runat="server" /></h2>
            <p>Tus controles cardiológicos y estudios del día.</p>
        </div>
        <dl class="cabecera-cifras">
            <div><dt>Turnos hoy</dt><dd><asp:Literal ID="LtHoy" runat="server" /></dd></div>
            <div><dt>Próximos</dt><dd><asp:Literal ID="LtProximos" runat="server" /></dd></div>
            <div class="cifra-alerta"><dt>Por decidir</dt><dd><asp:Literal ID="LtPendientes" runat="server" /></dd></div>
        </dl>
    </section>

    <asp:Panel ID="PnPanel" runat="server">

        <div class="card">
            <h3>Pedidos para decidir</h3>
            <p class="subtitle-line">Pacientes que pidieron turno con vos. Aceptalo para sumarlo a tu agenda, o rechazalo.</p>
            <asp:GridView ID="GvPendientes" runat="server" AutoGenerateColumns="false" CssClass="data-table"
                OnRowCommand="GvPendientes_RowCommand"
                EmptyDataText="No tenés pedidos pendientes. Todo al día.">
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
                            <asp:Button runat="server" Text="Rechazar" CssClass="btn btn-mini btn-peligro"
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
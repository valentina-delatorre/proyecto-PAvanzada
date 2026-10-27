<%@ Page Title="" Language="C#" MasterPageFile="~/site.master" AutoEventWireup="true" CodeBehind="Inicio.aspx.cs" Inherits="Consultorio2026.Inicio" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <%-- Cabecera: el dia de hoy en una frase, y los tres numeros que importan --%>
    <section class="hoy">
        <div class="hoy-texto">
            <span class="hoy-fecha"><asp:Literal ID="LtFecha" runat="server" /></span>
            <h2><asp:Literal ID="LtSaludo" runat="server" /></h2>
            <p><asp:Literal ID="LtResumen" runat="server" /></p>
            <svg class="ecg-line hoy-ecg" viewBox="0 0 380 70" xmlns="http://www.w3.org/2000/svg">
                <path d="M0,35 L90,35 L105,10 L120,60 L135,35 L160,35 L172,20 L184,50 L196,35 L380,35" />
            </svg>
        </div>
        <dl class="hoy-cifras">
            <div><dt>Hoy</dt><dd><asp:Literal ID="LtHoy" runat="server" /></dd></div>
            <div><dt>Esta semana</dt><dd><asp:Literal ID="LtSemana" runat="server" /></dd></div>
            <div class="hoy-cifra-alerta" id="DivPorConfirmar" runat="server"><dt>Por confirmar</dt><dd><asp:Literal ID="LtPorConfirmar" runat="server" /></dd></div>
        </dl>
    </section>

    <div class="inicio-grilla">

        <%-- Agenda de los proximos 7 dias, dia por dia --%>
        <section class="agenda">
            <header class="bloque-cabecera">
                <h3>Agenda de la semana</h3>
                <asp:HyperLink ID="HlTurnos" runat="server" NavigateUrl="~/Turnos/ListadoTurnos.aspx" CssClass="bloque-link" Text="Todos los turnos →" />
            </header>

            <asp:Repeater ID="RpAgenda" runat="server">
                <ItemTemplate>
                    <div class='<%# "agenda-dia" + ((bool)Eval("Vacio") ? " agenda-dia-vacio" : "") %>'>
                        <div class="agenda-dia-nombre">
                            <strong><%# Eval("Titulo") %></strong>
                            <span><%# Eval("Fecha") %></span>
                        </div>
                        <div class="agenda-dia-turnos">
                            <%# (bool)Eval("Vacio") ? "<span class=\"agenda-nada\">Sin turnos</span>" : "" %>
                            <asp:Repeater ID="RpTurnos" runat="server" DataSource='<%# Eval("Turnos") %>'>
                                <ItemTemplate>
                                    <div class="agenda-turno">
                                        <span class="agenda-hora"><%# Eval("Hora") %></span>
                                        <span class="agenda-quien">
                                            <strong><%# Eval("Paciente") %></strong>
                                            <span><%# Eval("Especialidad") %> · <%# Eval("Medico") %></span>
                                        </span>
                                        <span class='<%# "punto punto-" + Eval("Estado").ToString().ToLower() %>'><%# Eval("Estado") %></span>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </section>

        <aside class="lateral">

            <%-- Lo que esta esperando que alguien lo confirme --%>
            <section class="bloque bloque-pendientes">
                <header class="bloque-cabecera">
                    <h3>Esperando confirmación</h3>
                </header>
                <asp:Repeater ID="RpPendientes" runat="server">
                    <ItemTemplate>
                        <div class="pendiente">
                            <span class="pendiente-cuando"><%# Eval("Fecha") %><br /><%# Eval("Hora") %> hs</span>
                            <span class="pendiente-que"><strong><%# Eval("Paciente") %></strong><%# Eval("Especialidad") %> · <%# Eval("Medico") %></span>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:Label ID="LbSinPendientes" runat="server" CssClass="bloque-vacio" Text="No hay pedidos pendientes. Todo al día." Visible="false" />
                <asp:HyperLink ID="HlRevisar" runat="server" NavigateUrl="~/Turnos/ListadoTurnos.aspx" CssClass="bloque-link" Text="Revisar en Turnos →" />
            </section>

            <%-- Que dia de la semana se piden mas turnos --%>
            <section class="bloque">
                <header class="bloque-cabecera">
                    <h3>Demanda por día</h3>
                </header>
                <div class="semana-barras">
                    <asp:Repeater ID="RpDemanda" runat="server">
                        <ItemTemplate>
                            <div class='<%# "barra" + ((bool)Eval("EsMaximo") ? " barra-max" : "") %>' title='<%# Eval("Cantidad") + " turnos" %>'>
                                <span class="barra-num"><%# Eval("Cantidad") %></span>
                                <span class="barra-col"><span style='height:<%# Eval("Altura") %>%'></span></span>
                                <span class="barra-dia"><%# Eval("Dia") %></span>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                <p class="bloque-nota"><asp:Literal ID="LtDemanda" runat="server" /></p>
            </section>

            <%-- Los profesionales y cuanto trabajo tienen esta semana --%>
            <section class="bloque">
                <header class="bloque-cabecera">
                    <h3>Profesionales</h3>
                    <asp:HyperLink ID="HlEspecialistas" runat="server" NavigateUrl="~/Especialistas/ListadoEspecialistas.aspx" CssClass="bloque-link" Text="Ver →" />
                </header>
                <asp:Repeater ID="RpProfesionales" runat="server">
                    <ItemTemplate>
                        <div class="profesional">
                            <span class="profesional-iniciales"><%# Eval("Iniciales") %></span>
                            <span class="profesional-nombre"><strong><%# Eval("Nombre") %></strong><%# Eval("Especialidad") %></span>
                            <span class="profesional-carga"><%# Eval("Semana") %></span>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </section>

            <%-- Quienes usan el sistema --%>
            <section class="bloque">
                <header class="bloque-cabecera">
                    <h3>Usuarios</h3>
                    <asp:HyperLink ID="HlAccesos" runat="server" NavigateUrl="~/Seguridad/Accesos.aspx" CssClass="bloque-link" Text="Accesos →" />
                </header>
                <p class="usuarios-total"><asp:Literal ID="LtUsuarios" runat="server" /></p>
                <p class="bloque-nota"><asp:Literal ID="LtRoles" runat="server" /></p>
            </section>
        </aside>
    </div>
</asp:Content>

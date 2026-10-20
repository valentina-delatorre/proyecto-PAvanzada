<%@ Page Title="" Language="C#" MasterPageFile="~/site.master" AutoEventWireup="true" CodeBehind="PortalPaciente.aspx.cs" Inherits="Consultorio2026.Portal.PortalPaciente" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="portal-hero">
        <h2>Consultorio de Atención Médica</h2>
        <p>Atendemos con especialistas en cardiología, pediatría y traumatología. Solicitá tu turno online y seguí el estado de tus consultas desde acá.</p>
    </div>

    <div class="card">
        <h3>Solicitar un turno</h3>

        <%-- PASO 1: con quién. AutoPostBack="true" significa que al cambiar la opción la página
             vuelve sola al servidor y se ejecuta el método de OnSelectedIndexChanged (en el .cs). --%>
        <div class="paso"><span class="paso-num">1</span> ¿Con quién te querés atender?</div>
        <div class="form-grid">
            <div class="field">
                <label>Especialidad</label>
                <asp:DropDownList ID="DdlEspecialidad" runat="server" AutoPostBack="true"
                    OnSelectedIndexChanged="DdlEspecialidad_SelectedIndexChanged" CssClass="input-select" />
            </div>

            <div class="field">
                <label>Médico (opcional)</label>
                <asp:DropDownList ID="DdlMedico" runat="server" CssClass="input-select" AutoPostBack="true"
                    OnSelectedIndexChanged="DdlMedico_SelectedIndexChanged" />
            </div>

            <div class="field">
                <label>Motivo de consulta</label>
                <%-- Este no tiene AutoPostBack: no cambia nada en pantalla, solo se lee al guardar --%>
                <asp:DropDownList ID="DdlTipoConsulta" runat="server" CssClass="input-select">
                    <asp:ListItem>Primera consulta</asp:ListItem>
                    <asp:ListItem>Control</asp:ListItem>
                    <asp:ListItem>Estudio</asp:ListItem>
                </asp:DropDownList>
            </div>
        </div>

        <div class="paso"><span class="paso-num">2</span> Elegí el día y la hora</div>
        <%-- PASO 2: día y hora.
             HfFecha es un campo oculto (el paciente no lo ve): ahí guardamos el día que tocó en el almanaque,
             porque ya no hay un campo de fecha para escribir. --%>
        <asp:HiddenField ID="HfFecha" runat="server" />

        <%-- EL ALMANAQUE. Arranca invisible (Visible="false") y lo prende CargarCalendario() en el .cs.
             Los datos salen del procedimiento almacenado SP_AGENDA_MES. --%>
        <asp:Panel ID="PnCalendario" runat="server" Visible="false" CssClass="calendario-reserva">
            <p class="subtitle-line">Turnos de <strong><asp:Literal ID="LtMedicoAgenda" runat="server" /></strong>. Tocá un día para elegirlo.</p>

            <%-- Flechas para cambiar de mes. Las dos llaman al mismo método (BtMes_Command);
                 el CommandArgument le dice para dónde ir: -1 = mes anterior, 1 = mes siguiente. --%>
            <div class="cal-nav">
                <asp:LinkButton ID="BtMesAnterior" runat="server" CssClass="cal-flecha" OnCommand="BtMes_Command" CommandArgument="-1" Text="‹" />
                <span class="cal-mes"><asp:Literal ID="LtMesAgenda" runat="server" /></span>
                <asp:LinkButton ID="BtMesSiguiente" runat="server" CssClass="cal-flecha" OnCommand="BtMes_Command" CommandArgument="1" Text="›" />
            </div>

            <%-- La grilla: 7 columnas (lunes a domingo). Primero los nombres de los días
                 y después el Repeater pone un casillero por cada día del mes. --%>
            <div class="calendario">
                <div class="cal-cabecera">Lun</div><div class="cal-cabecera">Mar</div><div class="cal-cabecera">Mié</div>
                <div class="cal-cabecera">Jue</div><div class="cal-cabecera">Vie</div><div class="cal-cabecera">Sáb</div>
                <div class="cal-cabecera">Dom</div>
                <%-- El Repeater repite lo que hay en ItemTemplate una vez por cada "Celda" que le pasa el .cs.
                     Eval("Algo") lee esa propiedad de la Celda. Cada día es un LinkButton (un botón con forma de link):
                     al tocarlo se ejecuta RpCalendario_ItemCommand y le llega la fecha por CommandArgument.
                     Enabled = false en los días pasados o completos, así no se pueden tocar. --%>
                <asp:Repeater ID="RpCalendario" runat="server" OnItemCommand="RpCalendario_ItemCommand">
                    <ItemTemplate>
                        <asp:LinkButton runat="server" CssClass='<%# Eval("Clase") %>' CommandName="Elegir"
                            CommandArgument='<%# Eval("Fecha") %>' Enabled='<%# Eval("Seleccionable") %>'>
                            <span class="cal-num"><%# Eval("Numero") %></span>
                            <span class="cal-disp"><%# Eval("Texto") %></span>
                        </asp:LinkButton>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

            <%-- La referencia de colores que va debajo del almanaque (es solo texto fijo) --%>
            <div class="cal-referencias">
                <span class="cal-ref cal-libre">Todo libre</span>
                <span class="cal-ref cal-parcial">Quedan algunos</span>
                <span class="cal-ref cal-completo">Completo</span>
                <span class="cal-ref cal-elegido">Tu día</span>
            </div>

            <%-- Las horas del día elegido. Aparece recién cuando el paciente toca un día (lo prende el .cs).
                 Es un RadioButtonList común (opciones donde se elige una sola); con CSS escondemos el
                 circulito del radio y pintamos el texto como un botón redondeado ("píldora").
                 Las opciones las carga CargarHoras() con las horas que están libres ese día. --%>
            <asp:Panel ID="PnHoras" runat="server" Visible="false" CssClass="horas-del-dia">
                <asp:Label ID="LbFechaElegida" runat="server" CssClass="fecha-elegida" />
                <asp:RadioButtonList ID="RblHora" runat="server" RepeatLayout="Flow" RepeatDirection="Horizontal" CssClass="horas" />
            </asp:Panel>
        </asp:Panel>

        <%-- Guardar el turno: ejecuta BtSolicitar_Click. Los dos Label muestran el mensaje de OK o el de error. --%>
        <asp:Button ID="BtSolicitar" runat="server" Text="Solicitar turno" OnClick="BtSolicitar_Click" CssClass="btn" />
        <asp:Label ID="LbMensaje" runat="server" CssClass="ok-msg" />
        <asp:Label ID="LbError" runat="server" CssClass="error-msg" />
    </div>

    <div class="card">
        <h3>Mis turnos</h3>
        <%-- MIS TURNOS. Antes era una tabla (GridView); ahora es un Repeater, que repite este HTML por cada turno:
             el día y el mes a la izquierda, los datos en el medio y el estado a la derecha.
             Mes() y Medico() son métodos del .cs que dejan los datos más lindos para mostrar.
             El estado lleva una clase según su valor (estado-confirmado, estado-solicitado...)
             y cada una tiene su color en Site.css. --%>
        <asp:Repeater ID="RpMisTurnos" runat="server">
            <ItemTemplate>
                <div class="turno-item">
                    <div class="turno-fecha">
                        <span class="turno-dia"><%# Eval("Fecha", "{0:dd}") %></span>
                        <span class="turno-mes"><%# Mes(Eval("Fecha")) %></span>
                    </div>
                    <div class="turno-info">
                        <strong><%# Eval("Especialidad") %></strong> · <%# Eval("Hora") %> hs
                        <span class="turno-detalle"><%# Medico(Eval("NombreMedico")) %> · <%# Eval("TipoConsulta") %></span>
                    </div>
                    <span class='estado estado-<%# Eval("Estado").ToString().ToLower() %>'><%# Eval("Estado") %></span>
                </div>
            </ItemTemplate>
        </asp:Repeater>
        <%-- Si el paciente no tiene turnos, el .cs prende este cartel --%>
        <asp:Label ID="LbSinTurnos" runat="server" CssClass="subtitle-line" Text="Todavía no solicitaste ningún turno." Visible="false" />
    </div>

</asp:Content>
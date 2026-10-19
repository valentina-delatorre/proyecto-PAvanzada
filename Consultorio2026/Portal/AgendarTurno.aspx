<%@ Page Title="" Language="C#" MasterPageFile="~/site.master" AutoEventWireup="true" CodeBehind="AgendarTurno.aspx.cs" Inherits="Consultorio2026.Portal.AgendarTurno" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <h2>Agendar turno</h2>
    <p class="subtitle-line">Completá tus datos y elegí el turno que mejor te quede. Todos los campos con * son obligatorios.</p>

    <div class="card">
        <h3>Tus datos personales</h3>
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
                <label>DNI *</label>
                <asp:TextBox ID="TxtDni" runat="server" />
            </div>
            <div class="field">
                <label>Teléfono</label>
                <asp:TextBox ID="TxtTelefono" runat="server" />
            </div>
            <div class="field">
                <label>Email</label>
                <asp:TextBox ID="TxtEmail" runat="server" />
            </div>
        </div>
    </div>

    <div class="card">
        <h3>Tu turno</h3>
        <div class="form-grid">
            <div class="field">
                <label>Especialidad *</label>
                <asp:DropDownList ID="DdlEspecialidad" runat="server" AutoPostBack="true"
                    OnSelectedIndexChanged="DdlEspecialidad_SelectedIndexChanged" CssClass="input-select" />
            </div>
            <div class="field">
                <label>Profesional</label>
                <asp:DropDownList ID="DdlMedico" runat="server" CssClass="input-select" AutoPostBack="true"
                    OnSelectedIndexChanged="DdlMedico_SelectedIndexChanged" />
            </div>
            <div class="field">
                <label>Tipo de consulta *</label>
                <asp:DropDownList ID="DdlTipoConsulta" runat="server" CssClass="input-select">
                    <asp:ListItem>Primera consulta</asp:ListItem>
                    <asp:ListItem>Control</asp:ListItem>
                    <asp:ListItem>Estudio</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="field">
                <label>Fecha *</label>
                <asp:TextBox ID="TxtFecha" runat="server" TextMode="Date" AutoPostBack="true"
                    OnTextChanged="TxtFecha_TextChanged" />
            </div>
            <div class="field">
                <label>Hora *</label>
                <asp:DropDownList ID="DdlHora" runat="server" CssClass="input-select">
                    <asp:ListItem>09:00</asp:ListItem>
                    <asp:ListItem>10:00</asp:ListItem>
                    <asp:ListItem>11:00</asp:ListItem>
                    <asp:ListItem>12:00</asp:ListItem>
                    <asp:ListItem>15:00</asp:ListItem>
                    <asp:ListItem>16:00</asp:ListItem>
                    <asp:ListItem>17:00</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="field field-full">
                <label>Observaciones</label>
                <asp:TextBox ID="TxtObservaciones" runat="server" TextMode="MultiLine" Rows="2" />
            </div>
        </div>

        <%-- Almanaque del profesional elegido (SP_AGENDA_MES), con colores según lo ocupado que está cada día --%>
        <asp:Label ID="LbSinAgenda" runat="server" CssClass="subtitle-line" Text="Elegí un profesional para ver su agenda del mes." />
        <asp:Panel ID="PnCalendario" runat="server" Visible="false">
            <h4 style="margin-bottom: 0.2rem;">Agenda de <asp:Literal ID="LtMedicoAgenda" runat="server" /> — <asp:Literal ID="LtMesAgenda" runat="server" /></h4>
            <div class="cal-referencias">
                <span class="cal-ref cal-libre">Libre</span>
                <span class="cal-ref cal-parcial">Con turnos</span>
                <span class="cal-ref cal-completo">Completo</span>
                <span class="cal-ref cal-pasado">Ya pasó</span>
            </div>
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

        <asp:Button ID="BtConfirmar" runat="server" Text="Confirmar solicitud de turno" OnClick="BtConfirmar_Click" CssClass="btn" />
        <asp:HyperLink ID="LkMisTurnos" runat="server" NavigateUrl="~/Portal/PortalPaciente.aspx" Visible="false" CssClass="back-link" Text="Ver mis turnos →" />
        <asp:Label ID="LbMensaje" runat="server" CssClass="ok-msg" />
        <asp:Label ID="LbError" runat="server" CssClass="error-msg" />
    </div>

</asp:Content>
<%@ Page Title="" Language="C#" MasterPageFile="~/site.master" AutoEventWireup="true" CodeBehind="ListadoEspecialistas.aspx.cs" Inherits="Consultorio2026.Especialistas.ListadoEspecialistas" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <section class="cabecera">
        <div>
            <span class="cabecera-eyebrow">Módulo de especialistas</span>
            <h2>Cartilla de profesionales</h2>
            <p>Los médicos del consultorio y su especialidad. Abrí uno para ver su agenda completa, o sumá uno nuevo abajo.</p>
        </div>
        <dl class="cabecera-cifras">
            <div><dt>Activos</dt><dd><asp:Literal ID="LtActivos" runat="server" /></dd></div>
            <div><dt>Especialidades</dt><dd><asp:Literal ID="LtEspecialidades" runat="server" /></dd></div>
            <div><dt>Turnos 7 días</dt><dd><asp:Literal ID="LtSemana" runat="server" /></dd></div>
        </dl>
    </section>

    <%-- Agenda del profesional elegido --%>
    <asp:Panel ID="PnAgenda" runat="server" Visible="false" CssClass="card">
        <h3><asp:Literal ID="LtNombreMedico" runat="server" /></h3>
        <asp:Literal ID="LtFicha" runat="server" />
        <asp:Button ID="BtCambiarActivoMedico" runat="server" OnClick="BtCambiarActivoMedico_Click" CssClass="btn btn-mini btn-peligro" />

        <h3 style="margin-top: 1.6rem;">Agenda</h3>
        <div class="tabla-scroll">
            <asp:GridView ID="GvAgenda" runat="server" AutoGenerateColumns="false" CssClass="data-table"
                EmptyDataText="Este profesional no tiene turnos asignados.">
                <Columns>
                    <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                    <asp:BoundField DataField="Hora" HeaderText="Hora" />
                    <asp:BoundField DataField="NombrePaciente" HeaderText="Paciente" />
                    <asp:BoundField DataField="TipoConsulta" HeaderText="Motivo" />
                    <asp:TemplateField HeaderText="Estado">
                        <ItemTemplate><span class='punto punto-<%# Eval("Estado").ToString().ToLower() %>'><%# Eval("EstadoVisual") %></span></ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </asp:Panel>

    <div class="card">
        <h3>Profesionales</h3>
        <div class="tabla-scroll">
            <asp:GridView ID="GvMedicos" runat="server" AutoGenerateColumns="false" CssClass="data-table"
                DataKeyNames="IdMedico" OnSelectedIndexChanged="GvMedicos_SelectedIndexChanged"
                SelectedRowStyle-CssClass="fila-elegida" EmptyDataText="No hay especialistas cargados.">
                <Columns>
                    <asp:TemplateField HeaderText="Profesional">
                        <ItemTemplate>
                            <span class="celda-persona"><strong><%# Eval("Nombre") %> <%# Eval("Apellido") %></strong><span><%# Eval("UsuarioVinculado").ToString() == "-" ? "Sin usuario en el sistema" : "Usuario: " + Eval("UsuarioVinculado") %></span></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Especialidad">
                        <ItemTemplate><span class="etiqueta"><%# Eval("Especialidad") %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Estado">
                        <ItemTemplate><span class='<%# (bool)Eval("Activo") ? "punto punto-activo" : "punto punto-baja" %>'><%# (bool)Eval("Activo") ? "Atiende" : "De baja" %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:CommandField ShowSelectButton="true" SelectText="Ver agenda" />
                </Columns>
            </asp:GridView>
        </div>
    </div>

    <div class="card">
        <h3>Sumar un especialista</h3>
        <p class="subtitle-line">Si ya tiene usuario con rol Médico, vinculalo para que pueda entrar a su panel.</p>
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
                <label>Vincular a usuario</label>
                <asp:DropDownList ID="DdlUsuarioVincular" runat="server" CssClass="input-select" />
            </div>
        </div>
        <asp:Button ID="BtAgregarMedico" runat="server" Text="Agregar especialista" OnClick="BtAgregarMedico_Click" CssClass="btn" />
        <asp:Label ID="LbMensaje" runat="server" CssClass="ok-msg" />
        <asp:Label ID="LbError" runat="server" CssClass="error-msg" />
    </div>
</asp:Content>

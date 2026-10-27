<%@ Page Title="" Language="C#" MasterPageFile="~/site.master" AutoEventWireup="true" CodeBehind="ListadoUsuarios.aspx.cs" Inherits="Consultorio2026.Usuarios.ListadoUsuarios" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <section class="cabecera">
        <div>
            <span class="cabecera-eyebrow">Módulo de usuarios</span>
            <h2>Personas en el sistema</h2>
            <p>Todos los usuarios, del más nuevo al más viejo. Abrí uno para ver su ficha, sus turnos y darlo de baja si hace falta.</p>
        </div>
        <dl class="cabecera-cifras">
            <div><dt>Usuarios</dt><dd><asp:Literal ID="LtTotal" runat="server" /></dd></div>
            <div><dt>Pacientes</dt><dd><asp:Literal ID="LtPacientes" runat="server" /></dd></div>
            <div><dt>De baja</dt><dd><asp:Literal ID="LtBajas" runat="server" /></dd></div>
        </dl>
    </section>

    <%-- Detalle del usuario elegido: aparece arriba de la lista para no tener que bajar --%>
    <asp:Panel ID="PnDetalle" runat="server" Visible="false" CssClass="card">
        <h3><asp:Literal ID="LtDetalleNombre" runat="server" /></h3>
        <asp:Literal ID="LtFicha" runat="server" />

        <asp:Button ID="BtCambiarActivo" runat="server" OnClick="BtCambiarActivo_Click" CssClass="btn btn-mini btn-peligro" />
        <asp:Label ID="LbProtegido" runat="server" CssClass="subtitle-line" />

        <h3 style="margin-top: 1.6rem;">Turnos</h3>
        <asp:Label ID="LbTotalTurnos" runat="server" CssClass="subtitle-line" />
        <div class="tabla-scroll">
            <asp:GridView ID="GvTurnosUsuario" runat="server" AutoGenerateColumns="false" CssClass="data-table"
                EmptyDataText="Este usuario no tiene turnos en el sistema.">
                <Columns>
                    <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                    <asp:BoundField DataField="Hora" HeaderText="Hora" />
                    <asp:BoundField DataField="Especialidad" HeaderText="Especialidad" />
                    <asp:BoundField DataField="TipoConsulta" HeaderText="Motivo" />
                    <asp:BoundField DataField="NombreMedico" HeaderText="Especialista" />
                    <asp:TemplateField HeaderText="Estado">
                        <ItemTemplate><span class='punto punto-<%# Eval("Estado").ToString().ToLower() %>'><%# Eval("EstadoVisual") %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Observaciones" HeaderText="Observaciones" />
                </Columns>
            </asp:GridView>
        </div>
    </asp:Panel>

    <div class="card">
        <h3>Usuarios del sistema</h3>
        <div class="tabla-scroll">
            <asp:GridView ID="GvUsuarios" runat="server" AutoGenerateColumns="false" CssClass="data-table"
                DataKeyNames="IdUsuario" OnSelectedIndexChanged="GvUsuarios_SelectedIndexChanged"
                SelectedRowStyle-CssClass="fila-elegida" EmptyDataText="No hay usuarios cargados.">
                <Columns>
                    <asp:TemplateField HeaderText="Persona">
                        <ItemTemplate>
                            <span class="celda-persona"><strong><%# Nombre(Eval("IdUsuario")) %></strong><span><%# Eval("NombreUsuario") %> · <%# Eval("Email") %></span></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Rol">
                        <ItemTemplate><span class="etiqueta"><%# Eval("Rol") %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Dni" HeaderText="DNI" />
                    <asp:BoundField DataField="Especialidad" HeaderText="Especialidad" />
                    <asp:TemplateField HeaderText="Estado">
                        <ItemTemplate><span class='<%# (bool)Eval("Activo") ? "punto punto-activo" : "punto punto-baja" %>'><%# (bool)Eval("Activo") ? "Activo" : "De baja" %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:CommandField ShowSelectButton="true" SelectText="Ver ficha" />
                </Columns>
            </asp:GridView>
        </div>
    </div>
</asp:Content>

<%@ Page Title="" Language="C#" MasterPageFile="~/site.master" AutoEventWireup="true" CodeBehind="Accesos.aspx.cs" Inherits="Consultorio2026.Seguridad.Accesos" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <section class="cabecera">
        <div>
            <span class="cabecera-eyebrow">Seguridad</span>
            <h2>Seguridad y accesos</h2>
            <p>Todos los usuarios y a qué módulos pueden entrar. Marcá o desmarcá un casillero para habilitar o deshabilitar ese módulo a esa persona: se guarda en el momento.</p>
        </div>
        <dl class="cabecera-cifras">
            <div><dt>Usuarios</dt><dd><asp:Literal ID="LtUsuarios" runat="server" /></dd></div>
            <div><dt>Módulos</dt><dd><asp:Literal ID="LtModulos" runat="server" /></dd></div>
            <div class="cifra-alerta"><dt>Cambios a mano</dt><dd><asp:Literal ID="LtCambios" runat="server" /></dd></div>
        </dl>
    </section>

    <div class="card">
        <div class="accesos-referencias">
            <span><span class="acceso-muestra"></span> Lo que le da su rol</span>
            <span><span class="acceso-muestra acceso-muestra-cambiado"></span> Cambiado a mano (distinto a su rol)</span>
        </div>
        <asp:Label ID="LbMensaje" runat="server" CssClass="ok-msg" EnableViewState="false" />

        <div class="accesos-scroll">
            <table class="data-table accesos-tabla">
                <thead>
                    <tr>
                        <th>Usuario</th>
                        <th>Rol</th>
                        <th>Activo</th>
                        <asp:Repeater ID="RpModulos" runat="server">
                            <ItemTemplate><th class="acceso-col"><%# Eval("Corto") %></th></ItemTemplate>
                        </asp:Repeater>
                    </tr>
                </thead>
                <tbody>
                    <asp:Repeater ID="RpUsuarios" runat="server" OnItemDataBound="RpUsuarios_ItemDataBound">
                        <ItemTemplate>
                            <tr>
                                <td>
                                    <strong><%# Eval("NombreUsuario") %></strong>
                                    <div class="acceso-detalle"><%# Nombre(Eval("IdUsuario")) %> · <%# Eval("Email") %></div>
                                    <asp:HiddenField ID="HfUsuario" runat="server" Value='<%# Eval("IdUsuario") %>' />
                                </td>
                                <td><%# Eval("Rol") %></td>
                                <td><%# (bool)Eval("Activo") ? "Sí" : "No" %></td>
                                <asp:Repeater ID="RpAccesos" runat="server">
                                    <ItemTemplate>
                                        <td class='<%# "acceso-col" + ((bool)Eval("Habilitado") != (bool)Eval("PorRol") ? " acceso-cambiado" : "") %>'>
                                            <asp:CheckBox ID="ChkAcceso" runat="server" Checked='<%# Eval("Habilitado") %>'
                                                AutoPostBack="true" OnCheckedChanged="ChkAcceso_CheckedChanged"
                                                ToolTip='<%# Eval("Modulo") %>' />
                                            <asp:HiddenField ID="HfPermiso" runat="server" Value='<%# Eval("IdPermiso") %>' />
                                        </td>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
            </table>
        </div>
    </div>
</asp:Content>

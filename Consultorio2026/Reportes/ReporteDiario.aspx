<%@ Page Title="" Language="C#" MasterPageFile="~/site.master" AutoEventWireup="true" CodeBehind="ReporteDiario.aspx.cs" Inherits="Consultorio2026.Reportes.ReporteDiario" %>

<%-- =====================================================================
     PÁGINA ReporteDiario.aspx - Pantalla del módulo Reportes
     =====================================================================
     Este archivo define cómo se ve la pantalla. No tiene lógica: solo
     declara una cabecera con tres números y cuatro tablas. Los datos de las tablas los carga
     el archivo ReporteDiario.aspx.cs en el evento Page_Load.

     MasterPageFile="~/site.master": la página usa la página maestra del
     sistema, que aporta el menú, el encabezado y los estilos comunes.
     Todo lo que escribimos adentro de <asp:Content> se inserta en el
     espacio que la página maestra reserva para el contenido.

     Los comentarios escritos así (con los signos de porcentaje) son
     comentarios de servidor: no se envían al navegador del usuario.
     ===================================================================== --%>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <%-- CABECERA - El título del módulo, la fecha de hoy y tres números grandes
         (turnos de hoy, total de turnos y confirmados). Los tres asp:Literal los
         llena el .cs. La clase "cabecera" es la misma que usan todos los módulos. --%>
    <section class="cabecera">
        <div>
            <span class="cabecera-eyebrow"><asp:Literal ID="LtFechaHoy" runat="server" /></span>
            <h2>Reporte del consultorio</h2>
            <p>Resumen de la actividad. Se actualiza solo: sale directo de los turnos que piden los pacientes y gestiona el área de turnos.</p>
        </div>
        <dl class="cabecera-cifras">
            <div><dt>Turnos hoy</dt><dd><asp:Literal ID="LtHoy" runat="server" /></dd></div>
            <div><dt>Total</dt><dd><asp:Literal ID="LtTotal" runat="server" /></dd></div>
            <div><dt>Confirmados</dt><dd><asp:Literal ID="LtConfirmados" runat="server" /></dd></div>
        </dl>
    </section>

    <%-- TABLA 1 - Turnos de hoy
         Muestra todos los turnos de la fecha actual.

         Atributos del GridView (la tabla):
           - ID: el nombre con el que la usamos desde el código C# (GvHoy).
           - runat="server": indica que es un control de ASP.NET que se
             maneja desde el servidor.
           - AutoGenerateColumns="false": no dejamos que arme las columnas
             solo; las definimos nosotros abajo, para elegir cuáles mostrar
             y con qué título.
           - CssClass: la clase de estilos con la que se dibuja la tabla.
           - EmptyDataText: el mensaje que se muestra si no hay ningún dato. --%>
    <div class="card">
        <h3>Turnos de hoy</h3>
        <asp:GridView ID="GvHoy" runat="server" AutoGenerateColumns="false" CssClass="data-table"
            EmptyDataText="No hay turnos para el día de hoy.">
            <Columns>
                <%-- Cada BoundField es una columna de la tabla:
                       - DataField: qué dato del turno se muestra (el nombre de la propiedad del objeto Turno).
                       - HeaderText: el título que ve el usuario en el encabezado de la columna. --%>
                <asp:BoundField DataField="Hora" HeaderText="Hora" />
                <asp:BoundField DataField="NombrePaciente" HeaderText="Paciente" />
                <asp:BoundField DataField="NombreMedico" HeaderText="Especialista" />
                <asp:BoundField DataField="Especialidad" HeaderText="Especialidad" />
                <asp:BoundField DataField="TipoConsulta" HeaderText="Servicio" />
                <%-- El estado va con un puntito de color: un TemplateField deja escribir
                     HTML propio en la celda, cosa que un BoundField no permite. --%>
                <asp:TemplateField HeaderText="Estado">
                    <ItemTemplate><span class='punto punto-<%# Eval("Estado").ToString().ToLower() %>'><%# Eval("Estado") %></span></ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>

    <%-- TABLAS 2, 3 y 4 - Resúmenes
         Estas tres tablas reciben el mismo tipo de dato: una lista de
         ResumenFila, que tiene solo "Etiqueta" (qué se cuenta) y "Cantidad"
         (cuántos turnos hay). Por eso las tres tienen las mismas dos columnas;
         lo único que cambia es el título que se muestra en cada una. --%>

    <%-- Las tres tablas de resumen van una al lado de la otra (clase "columnas"). --%>
    <div class="columnas columnas-3">

    <%-- TABLA 2 - Totales por estado: cuántos turnos hay confirmados, cancelados, etc. --%>
    <div class="card">
        <h3>Totales por estado</h3>
        <asp:GridView ID="GvEstados" runat="server" AutoGenerateColumns="false" CssClass="data-table data-table-resumen"
            EmptyDataText="Todavía no hay turnos en el sistema.">
            <Columns>
                <asp:BoundField DataField="Etiqueta" HeaderText="Estado" />
                <asp:BoundField DataField="Cantidad" HeaderText="Cantidad" />
            </Columns>
        </asp:GridView>
    </div>

    <%-- TABLA 3 - Turnos por especialidad: cuántos turnos tiene cada especialidad, de mayor a menor. --%>
    <div class="card">
        <h3>Turnos por especialidad</h3>
        <asp:GridView ID="GvEspecialidades" runat="server" AutoGenerateColumns="false" CssClass="data-table data-table-resumen"
            EmptyDataText="Todavía no hay turnos en el sistema.">
            <Columns>
                <asp:BoundField DataField="Etiqueta" HeaderText="Especialidad" />
                <asp:BoundField DataField="Cantidad" HeaderText="Turnos" />
            </Columns>
        </asp:GridView>
    </div>

    <%-- TABLA 4 - Próximos turnos por especialista: cuántos turnos confirmados
         tiene cada médico desde hoy en adelante. --%>
    <div class="card">
        <h3>Próximos turnos por especialista</h3>
        <asp:GridView ID="GvProximos" runat="server" AutoGenerateColumns="false" CssClass="data-table data-table-resumen"
            EmptyDataText="No hay turnos confirmados a futuro.">
            <Columns>
                <asp:BoundField DataField="Etiqueta" HeaderText="Especialista" />
                <asp:BoundField DataField="Cantidad" HeaderText="Turnos agendados" />
            </Columns>
        </asp:GridView>
    </div>
    </div>
</asp:Content>

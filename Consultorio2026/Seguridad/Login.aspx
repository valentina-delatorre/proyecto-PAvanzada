<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Consultorio2026.Seguridad.Login" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Iniciar sesión</title>
</head>
<body>
    <form id="form1" runat="server">
        <div style="max-width:300px; margin:100px auto;">
            <h3>Iniciar sesión</h3>

            <asp:Label runat="server" Text="Usuario:" /><br />
            <asp:TextBox ID="TxtUsuario" runat="server" /><br /><br />

            <asp:Label runat="server" Text="Contraseña:" /><br />
            <asp:TextBox ID="TxtContrasenia" runat="server" TextMode="Password" /><br /><br />

            <asp:Button ID="BtIngresar" runat="server" Text="Ingresar" OnClick="BtIngresar_Click" />
            <br /><br />
            <asp:Label ID="LbError" runat="server" ForeColor="Red" />
        </div>
    </form>
</body>
</html>
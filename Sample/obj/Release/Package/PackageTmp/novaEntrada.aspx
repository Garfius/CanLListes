<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="novaEntrada.aspx.cs" Inherits="CanLlistes.novaEntrada" %>

<asp:Content ID="Content1" ContentPlaceHolderID="StyleSection" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentSection" runat="server">
    <div class="container">

        <div class="row">
            <asp:Label ID="Label1" runat="server" Text="Label">Nom script entrada</asp:Label>
            <asp:TextBox ID="nomNouScript" runat="server"></asp:TextBox>
            <asp:Button ID="desar" runat="server" Text="Desar" CssClass="btn btn-sm btn-primary" OnClick="desar_Click" />
            <asp:Label ID="labelError" runat="server" ForeColor="#CC0000">Recorda que sobreescriu</asp:Label>
        </div>
        <div class="row">
            <asp:Label ID="Label2" runat="server" Text="Label">Correus que el componen</asp:Label>
        </div>
        <div  class="row">
            <textarea id="correusNouScript" style="width:50%; height:800px;" runat="server"></textarea>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ScriptSection" runat="server">
</asp:Content>

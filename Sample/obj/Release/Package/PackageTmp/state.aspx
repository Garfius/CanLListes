<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="state.aspx.cs" Inherits="CanLlistes.state" %>
<asp:Content ID="Content1" ContentPlaceHolderID="StyleSection" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentSection" runat="server">




    <div class="container-fluid">
        <div class="row">
            <div class="col-sm-2 col-md-2 sidebar">
                <ul class="nav nav-sidebar">
                    <li>
                        <asp:LinkButton runat="server" OnClick="novaLlista">New list</asp:LinkButton></li>
                    <li>
                        <asp:LinkButton runat="server" OnClick="editaLlista">Edit</asp:LinkButton></li>
                    <li>
                        <asp:LinkButton runat="server" OnClick="borraLlista">Delete</asp:LinkButton>
                    </li>
                    <li>
                        <asp:LinkButton runat="server" OnClick="executaFlux">Manual Run</asp:LinkButton></li>
                </ul>
                </div>
                <div class="col-sm-5">
                    <h3>Active Streams</h3>
                    <asp:ListBox ID="llistesActives" runat="server" style="width:100%; height:400px;" AutoPostBack="True" OnSelectedIndexChanged="llistesActives_SelectedIndexChanged"></asp:ListBox>
                </div>
                <div class="col-sm-5">
                    <h3>Last output</h3>
                    <pre id="outList" runat="server"></pre>
                </div>
            
        </div>

    </div>


</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ScriptSection" runat="server">

</asp:Content>

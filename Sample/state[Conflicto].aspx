<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="state.aspx.cs" Inherits="CanLlistes.state" %>
<asp:Content ID="Content1" ContentPlaceHolderID="StyleSection" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentSection" runat="server">

    

    
        <div class="container-fluid">
             <div class="row">
                <div class="col-sm-3 col-md-2 sidebar">
                  <ul class="nav nav-sidebar">
                    <li><asp:LinkButton runat="server" OnClick="novaLlista">Nova llista</asp:LinkButton></li>
                    <li><asp:LinkButton runat="server" OnClick="editaLlista">Editar</asp:LinkButton></li>
                    <li><asp:LinkButton runat="server" OnClick="borraLlista">Borrar</asp:LinkButton></li> 
                    <li><asp:LinkButton runat="server" OnClick="executaLlista">Re-generar previsualització</asp:LinkButton></li>
                  </ul>
                </div>
                <div class="col-sm-7 col-md-8 main">
                    <h3>Llistes actives</h3>
                    <asp:ListBox ID="llistesActives" runat="server" Width="100%" ></asp:ListBox>
                    <asp:RadioButtonList ID="RadioButtonList1" runat="server">
                    </asp:RadioButtonList>
                    <asp:RadioButton ID="RadioButton1" runat="server" />
                </div>
           </div> 
      
       </div>         
    

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ScriptSection" runat="server">

</asp:Content>

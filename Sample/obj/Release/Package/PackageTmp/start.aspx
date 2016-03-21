<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="start.aspx.cs" Inherits="CanLlistes.start" %>
<asp:Content ID="Content1" ContentPlaceHolderID="StyleSection" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentSection" runat="server">
    



        <!-- Main component for a primary marketing message or call to action -->
        <div class="jumbotron">
            <h1>Welcome to Canllistes</h1>
            <p runat="server" id="portal">
                This software will help you to create and manage your mail lists. But you need to know hot to create powershell scripts
            </p>
            <p>
                To start you may go to active lists and edit some sample, which uses semple scripts.
                Or you may create a new one by cliking New-->New list
            </p>
            <p> 
                Keep in mind: This is a script manager and coordinator so, the quality of your scripts is the basement of this castle.
            </p>

        </div>
    
    <script>
        function prova2() {
            
        }

    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ScriptSection" runat="server">
</asp:Content>
<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="editor.aspx.cs" Inherits="CanLlistes.editor" %>

<%@ Register Assembly="ASP.Web.UI.PopupControl" Namespace="ASP.Web.UI.PopupControl" TagPrefix="ASPP" %>

<asp:Content ID="Content1" ContentPlaceHolderID="StyleSection" runat="server">
    <style>
            #voraMaca {
        border-radius: 25px;
        border: 2px solid #73AD21;
        padding: 20px; 
        
    }
    </style>
    
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentSection" runat="server">
    
    <h2>Stream editor</h2>
    <div class="container" id="voraMaca">
        <div class="row">
            <p>Desired target mail</p>
            <div class="col-sm-7">
                <asp:TextBox ID="mailDesti" CssClass="form-control" runat="server" onkeypress="return EnterEvent(event)" clientidmode="Static"></asp:TextBox>
                <asp:Button ID="amagatMail" runat="server" Text="cantseeme" style="display:none" OnClick="desaMailClick" />

            </div>
            <div class="col-sm-3">
                <asp:Button ID="botoOcultEnterTextbox" CssClass="btn btn-sm btn-primary" runat="server" Text="Remember mail name" OnClick="desaMailClick" />
                
                <asp:Button class="btn btn-sm btn-primary" ID="GuardaNom" runat="server" Text="Save ALL" OnClick="desaLlistaClick" />
            </div>
        </div>

        <div class="row" id="voraMaca">
            <div class="col-sm-6" style="background-color: lavender;">
                <p>Input Scripts</p>
                <div class="row">
                    <div>
                        <asp:ListBox ID="scriptsInput" Width="100%" runat="server"></asp:ListBox>
                    </div>
                    <div>
                        <asp:LinkButton ID="inpBtUp" runat="server" CssClass="btn btn-sm btn-primary" OnClick="itemUpClick"><span aria-hidden="true" class="glyphicon glyphicon-arrow-up"></span></asp:LinkButton>
                        <asp:LinkButton ID="inpBtDn" runat="server" CssClass="btn btn-sm btn-primary" OnClick="itemDnClick"><span aria-hidden="true" class="glyphicon glyphicon-arrow-down"></span></asp:LinkButton>
                        <asp:Button ID="addInpScr" CssClass="btn btn-sm btn-primary" runat="server" Text="Add execution" OnClick="addInpScriptClick" />
                        <asp:Button CssClass="btn btn-sm btn-primary" runat="server" Text="Edit execution" OnClick="editInpScriptClick" />
                        <asp:Button CssClass="btn btn-sm btn-primary" runat="server" Text="Remove execution" OnClick="remInpScriptClick" />
                        
                    </div>
                </div>
            </div>

            <div class="col-sm-6" style="background-color: lavender;">
                <p>Output Scripts</p>
                <div class="row">
                    <div>
                        <asp:ListBox ID="scriptsOutput" Width="100%" runat="server" ></asp:ListBox>
                    </div>
                    <div>
                        <asp:LinkButton ID="outBtUp" runat="server" CssClass="btn btn-sm btn-primary" OnClick="itemUpClick"><span aria-hidden="true" class="glyphicon glyphicon-arrow-up"></span></asp:LinkButton>
                        <asp:LinkButton ID="outBtDn" runat="server" CssClass="btn btn-sm btn-primary" OnClick="itemDnClick"><span aria-hidden="true" class="glyphicon glyphicon-arrow-down"></span></asp:LinkButton>
                        <asp:Button ID="addOutScr" CssClass="btn btn-sm btn-primary" runat="server" Text="Add execution" OnClick="addOutScriptClick" />
                        <asp:Button CssClass="btn btn-sm btn-primary" runat="server" Text="Edit execution" OnClick="editOutScriptClick" />
                        <asp:Button CssClass="btn btn-sm btn-primary" runat="server" Text="Remove execution" OnClick="remOutScriptClick" />
                        
                    </div>
                </div>
            </div>
        
    
        
            <ASPP:PopupPanel HeaderText="Choose input script" ID="popupInputs" runat="server" OnCloseWindowClick="closeSelectInputClick">
                <PopupWindow runat="server">
                    <ASPP:PopupWindow ID="popupWindowInputs" runat="server">
                        <div align="center" style="width: 500px; height: 300px">
                            <asp:Label runat="server">Scripts found on the input folder</asp:Label>
                            <asp:ListBox ID="inpScriptsDisponibles" Width="80%" height="80%" runat="server"></asp:ListBox>
                            <p>Choose and close, Thanks</p>
                        </div>
                    </ASPP:PopupWindow>
                </PopupWindow>
            </ASPP:PopupPanel>
            <ASPP:PopupPanel HeaderText="Choose output script" ID="popupOutputs" runat="server" OnCloseWindowClick="closeSelectOutputClick">
                <PopupWindow runat="server">
                    <ASPP:PopupWindow ID="popupWindowOutputs" runat="server">
                        <div align="center" style="width: 500px; height: 300px">
                            <asp:Label runat="server">Scripts found on the output folder</asp:Label>
                            <asp:ListBox ID="outScriptsDisponibles" Width="80%" height="80%" runat="server"></asp:ListBox>
                            <p>Choose and close, Thanks</p>
                        </div>
                    </ASPP:PopupWindow>
                </PopupWindow>
            </ASPP:PopupPanel>
            <h3 id="labelEdicio" runat="server"></h3>

            <div class="container">
                    <div class="row" id="parms" runat="server">
                    </div>
            </div>

            
                <div class="col-sm-6"  style="background-color: lavender;">
                    <h3 id="labelExecucio" runat="server">Run output</h3>
                    <pre id="outScript" runat="server"></pre>
                </div>
                <div class="col-sm-6"  style="background-color: lavender;">
                    <h3>Run errors</h3>
                    <pre id="errScript" runat="server"></pre>
                </div>
            
        </div>
    </div>

<asp:Button ID="deixaPagina" runat="server" Text="cantseeme" style="display:none" OnClick="deixaPaginaClick" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ScriptSection" runat="server">
    <script>

        function EnterEvent(e) {
            if (e.keyCode == 13) {
                __doPostBack('<%=amagatMail.UniqueID%>', "");
            }
        }
        window.onbeforeunload = confirmExit;
        function confirmExit()
        {
            __doPostBack('<%=deixaPagina.UniqueID%>', "");
        }
       
    </script>
</asp:Content>

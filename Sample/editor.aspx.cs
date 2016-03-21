using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.UI.HtmlControls;

namespace CanLlistes
{
    //unsafe
    /// <summary>
    /// classe quen conté els mètodes de crida des del web editor.aspx
    /// </summary>
    public  partial class editor : System.Web.UI.Page
    {
        #region variables de control i configuracio
        /// <summary>
        /// ha d'allotjar el controlador de la sessió que demana la pàgina
        /// </summary>
        private controlSessio Controlador;
        /// <summary>
        /// cariable d'ús general per determinar l'estil dels botons en CSS
        /// </summary>
        private string estilBotoCss = "btn btn-sm btn-primary";
        private string targetMailDefaultMessage = "Target mail here";
        private string ErrMustSelectAny = "Must choose which one ?!";
        private string editingInputScript = "Editing Input Script: ";
        private string editingOutputScript = "Editing Output Script: ";
        private string lastRunOutput = "Output from the last run";
        private string ErrAddingScript = "Error adding script";
        private string ErrStreamCorrupt = "Corrupt stream, does first script subtract?";
        private string ErrSavingProblem = "There was a problem saving";
        private string ErrFileBusy = "File busy ?";
        private string SaveInfo = "Parameters for this script changed. Remember to save everything at the upper button";
        private string ErrTargetMailProblem = "No target mail, or invalid";
        private string MailRemembered = "Mail Remembered";
        private string RunOutputCaption = "Output from the run";
        private string ExecutedString = "String of the run: ";
        private string SaveParmsCaption = "Remember parms.";
        private string RunCaption = "Run";
        private string NoerrorsInfo = "No errors reported";
        #endregion

        #region funcions de crida Web
        /// <summary>
        /// funció que es crida abans de mostrar el web
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session.IsNewSession) Server.Transfer("start.aspx");

            Controlador = (controlSessio)Application.Get(this.Session.SessionID);
            
            if (Controlador == null)
            {
                ((Global)this.Context.ApplicationInstance).renewSession();
                Server.Transfer("start.aspx");
            }
            
            if (Controlador.sessionList == null)
            {
                Controlador.sessionList = new flux();
                Controlador.scriptEditant = null;
                parms.Controls.Clear();
                Controlador.sessionList.targetMail = targetMailDefaultMessage;
            }

            if (!IsPostBack)
            {
                refrescaNomIListBoxes();
            }
            else
            {
                if (Controlador.scriptEditant != null)
                {
                    if (Controlador.scriptEditant.parametresObjectes != null)
                    {
                        mostraObjectesParms(Controlador.scriptEditant.input);
                    }

                }

            }

            
        }
        /// <summary>
        /// funció disparada quan es desitja editar un script d'entrada
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void editInpScriptClick(object sender, EventArgs e)
        {
            if (Controlador == null) { Server.Transfer("start.aspx"); }

            if (scriptsInput.SelectedIndex == -1) {

                Response.Write("<script>alert('" + ErrMustSelectAny + "');</script>");
                errScript.InnerText = ErrMustSelectAny;
            } else {

                Controlador.scriptEditant = Controlador.sessionList.scriptsInput[scriptsInput.SelectedIndex];
                Controlador.scriptEditant.input = true;
                mostraObjectesParms(true);
                labelEdicio.InnerHtml = editingInputScript + scriptsInput.SelectedIndex;
                outScript.InnerText = Controlador.ultimaSortidaScriptEditant(true);
                errScript.InnerHtml = "";
                labelExecucio.InnerText = lastRunOutput;
                //ompleUltimOutScript(true);
            }
            
        }
        /// <summary>
        /// funció disparada quan es desitja editar un script de sortida
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void editOutScriptClick(object sender, EventArgs e)
        {
            if (Controlador == null) { Server.Transfer("start.aspx"); }

            if (scriptsOutput.SelectedIndex == -1) {
                Response.Write("<script>alert('" + ErrMustSelectAny + "');</script>");
                errScript.InnerText = ErrMustSelectAny;
            } else {
                Controlador.scriptEditant = Controlador.sessionList.scriptsOutput[scriptsOutput.SelectedIndex];
                Controlador.scriptEditant.input = false;
                
                mostraObjectesParms( false);

                labelEdicio.InnerHtml = editingOutputScript + scriptsOutput.SelectedIndex;
                outScript.InnerText = Controlador.ultimaSortidaScriptEditant(false);
                errScript.InnerHtml = "";
                labelExecucio.InnerText = lastRunOutput;
            }
        }
        /// <summary>
        /// funció disparada quan es desitja apujar un item d'una llista
        /// </summary>
        /// <param name="sender">el botó del web que el crida, per discriminar la lista de items</param>
        /// <param name="e"></param>
        protected void itemUpClick(object sender, EventArgs e)
        {
            if (Controlador == null) { Server.Transfer("start.aspx"); }
            bool input = true;
            ListBox llistaATocar = null;
            switch (((System.Web.UI.Control)(sender)).ID)
            {
                case "inpBtUp":
                    llistaATocar = scriptsInput;
                    input = true;
                break;
                case "outBtUp":
                    llistaATocar = scriptsOutput;
                    input = false;
                break;
            }

            Controlador.pujaItem(llistaATocar, input);
            
            if (Controlador.scriptEditant != null) {
                if (Controlador.scriptEditant.input == input) netejaCosesScriptEditant();
            }
            refrescaNomIListBoxes();

        }
        /// <summary>
        /// funció disparada quan es desitja abaixar un item d'una llista
        /// </summary>
        /// <param name="sender">el botó del web que el crida, per discriminar la lista de items</param>
        /// <param name="e"></param>
        protected void itemDnClick(object sender, EventArgs e)
        {
            if (Controlador == null) { Server.Transfer("start.aspx"); }
            bool input = true;
            ListBox llistaATocar = null;
            
            switch (((System.Web.UI.Control)(sender)).ID)
            {
                case "inpBtDn":
                    input = true;
                    llistaATocar = scriptsInput;
                break;
                case "outBtDn":
                    input = false;
                    llistaATocar = scriptsOutput;
                break;
            }

            Controlador.baixaItem(llistaATocar, input);

            if (Controlador.scriptEditant != null)
            {
                
                if (Controlador.scriptEditant.input == input) netejaCosesScriptEditant();
            }
            refrescaNomIListBoxes();

        }
        /// <summary>
        /// funció disparada quan es desitja afegir un script d'entrada
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void addInpScriptClick(object sender, EventArgs e)
        {
            if (Controlador == null) { Server.Transfer("start.aspx"); }
            inpScriptsDisponibles.SelectedIndex = -1;
            Controlador.listItemScriptsInput(inpScriptsDisponibles.Items);
            popupInputs.ShowPopupWindow();
        }
        /// <summary>
        /// funció disparada quan es desitja afegir un script se sortida
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void addOutScriptClick(object sender, EventArgs e)
        {
            if (Controlador == null) { Server.Transfer("start.aspx"); }
            outScriptsDisponibles.SelectedIndex = -1;
            Controlador.listItemScriptsOutput( outScriptsDisponibles.Items );
            popupOutputs.ShowPopupWindow();
        }
        /// <summary>
        /// funció disparada quan es tanca la finestra emetgent de scripts d'entrada
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void closeSelectInputClick(object sender, EventArgs e)
        {
            if (Controlador == null) { Server.Transfer("start.aspx"); }
            if (inpScriptsDisponibles.SelectedIndex > -1)
            {
                if (Controlador.afegeixScriptDesDArxiu(inpScriptsDisponibles.SelectedValue, true))
                {
                    scriptsInput.Items.Add(inpScriptsDisponibles.SelectedItem);
                    refrescaNomIListBoxes();
                    scriptsInput.SelectedIndex = (scriptsInput.Items.Count - 1);
                    editInpScriptClick(null, null);
                }
                else {
                    errScript.InnerText = ErrAddingScript;
                }
            }
            popupInputs.HidePopupWindow();
        }
        /// <summary>
        /// funció disparada quan es tanca la finestra emetgent de scripts de sortida
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void closeSelectOutputClick(object sender, EventArgs e)
        {
            if (Controlador == null) { Server.Transfer("start.aspx"); }
            if(outScriptsDisponibles.SelectedIndex > -1){

                if (Controlador.afegeixScriptDesDArxiu(outScriptsDisponibles.SelectedValue, false))
                {
                    scriptsOutput.Items.Add(outScriptsDisponibles.SelectedItem);
                    refrescaNomIListBoxes();
                    scriptsOutput.SelectedIndex = (scriptsOutput.Items.Count - 1);
                    editOutScriptClick(null, null);
                }
                else {
                    errScript.InnerText = ErrAddingScript;
                }
            }
            popupOutputs.HidePopupWindow();
        }
        /// <summary>
        /// funció disparada quan es polsa el botó, que ha d'eliminar un script d'entrada 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void remInpScriptClick(object sender, EventArgs e)
        {
            if (Controlador == null) { Server.Transfer("start.aspx"); }
            if (scriptsInput.SelectedIndex > -1)
            {

                if (Controlador.sessionList.scriptsInput[scriptsInput.SelectedIndex].Equals(Controlador.scriptEditant))
                {
                    netejaCosesScriptEditant();
                } 
                Controlador.sessionList.scriptsInput.RemoveAt(scriptsInput.SelectedIndex);
                scriptsInput.Items.RemoveAt(scriptsInput.SelectedIndex);
               
            }
        }
        /// <summary>
        /// funció disparada quan es polsa el botó, que ha d'eliminar un script de sortida
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void remOutScriptClick(object sender, EventArgs e)
        {
            if (Controlador == null) { Server.Transfer("start.aspx"); }
            if (scriptsOutput.SelectedIndex > -1)
            {

                if (Controlador.sessionList.scriptsOutput[scriptsOutput.SelectedIndex].Equals(Controlador.scriptEditant))
                {
                    netejaCosesScriptEditant();
                }
                Controlador.sessionList.scriptsOutput.RemoveAt(scriptsOutput.SelectedIndex);
                scriptsOutput.Items.RemoveAt(scriptsOutput.SelectedIndex);
                

            }
        }
        /// <summary>
        /// funció disparada quan es polsa el botó de desar llista
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void desaLlistaClick(object sender, EventArgs e)
        {
            if (Controlador == null) { Server.Transfer("start.aspx"); }
            Controlador.sessionList.targetMail = mailDesti.Text;
            
            if (((Global)this.Context.ApplicationInstance).estalliureXML(mailDesti.Text + ".xml", Session.SessionID))
            {
                if (Controlador.desaFluxEditant())
                {
                    if (Controlador.creaScriptDeLlista(Controlador.sessionList))
                    {  // aquest es podria moure a controlador
                        Controlador.sessionList = null;
                        Response.Redirect("state.aspx");
                    }
                }
                else {

                    errScript.InnerText = ErrStreamCorrupt;
                }
            }
            else {
                Response.Write("<script>alert('" + ErrSavingProblem + "');</script>");
                errScript.InnerText = ErrFileBusy;
            }

        }
        /// <summary>
        /// funció disparada quan es polsa el botó de desar paràmetres que es veuen en el web
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void desaParmsEditantClick(object sender, EventArgs e)
        {
            if (Controlador == null) { Server.Transfer("start.aspx"); }
            Panel panelin;
            Label ultimaEtiqueta = null;
            Controlador.scriptEditant.parametresObjectes.Clear();
            
            errScript.InnerText = SaveInfo;

            foreach (var panell in parms.Controls)
            {
                if (panell.GetType().ToString().Equals("System.Web.UI.WebControls.Panel"))
                {
                    panelin = (Panel)panell;
                    foreach (var cosa in panelin.Controls)
                    {

                        if (cosa.GetType().ToString().Equals("System.Web.UI.WebControls.Label")) ultimaEtiqueta = (Label)cosa;// es el nom del parametre

                        switch (cosa.GetType().ToString())
                        {

                            case "System.Web.UI.WebControls.TextBox":
                                Controlador.scriptEditant.parametresObjectes.Add(ultimaEtiqueta.Text,(TextBox)cosa);
                                break;
                            case "System.Web.UI.WebControls.DropDownList":
                                Controlador.scriptEditant.parametresObjectes.Add(ultimaEtiqueta.Text, (DropDownList)cosa);
                                break;
                            case "System.Web.UI.WebControls.RadioButtonList":
                                Controlador.scriptEditant.parametresObjectes.Add(this.Controlador.nomRadioButtonPosaTreu, (RadioButtonList)cosa);
                                break;
                        }
                    }
                }
            }
            refrescaNomIListBoxes();
        }
        /// <summary>
        /// funció disparada quan es polsa el botó d'executar script
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void executaScriptEditant(object sender, EventArgs e)
        {
            if (Controlador == null) { Server.Transfer("start.aspx"); }
            switch (((System.Web.UI.Control)(sender)).ID)
            {
                case "butoExecutaInput":
                    executaIOmpleTextBoxes(true);
                    break;
                case "butoExecutaOutput":
                    executaIOmpleTextBoxes(false);
                    break;
            }
        }
        /// <summary>
        /// de crida per establir el nom de la llista segons objecte web
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void desaMailClick(object sender, EventArgs e)
        {
            if (Controlador.sessionList != null)
            {
                RegexUtilities comprovador = new RegexUtilities();

                if (!comprovador.IsValidEmail(mailDesti.Text))
                {
                    errScript.InnerText = ErrTargetMailProblem;
                    return;
                }
                else {
                    Controlador.sessionList.targetMail = mailDesti.Text;
                    errScript.InnerText = MailRemembered;
                }
            }
        }
        /// <summary>
        /// estableix variables quan es deixa la pàgian d'edició
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void deixaPaginaClick(object sender, EventArgs e)
        {
            Controlador.sessionList = null;
        }
        
        #endregion

        #region funcions privades
        /// <summary>
        /// crida la execució al controlador i estableix els objectes del web amb les dades retornades
        /// </summary>
        /// <param name="input">script d'entrada o sortida?</param>
        private void executaIOmpleTextBoxes(bool input)
        {
            string sortida = "";
            string errors = "";
            string path = "";
            RegexUtilities comprovador = new RegexUtilities();

            if (!comprovador.IsValidEmail(mailDesti.Text))
            {
                if (!input) 
                {
                    errScript.InnerText = ErrTargetMailProblem;
                    return;
                }
                
            }
            else
            {
                Controlador.sessionList.targetMail = mailDesti.Text;
                errScript.InnerText = MailRemembered;
            }

            if (Controlador.executaScriptEditant(out sortida, out errors, out path, input))
            {
                outScript.InnerText = ExecutedString + path + "" + Environment.NewLine;
                outScript.InnerText += "-----------------------------------" + Environment.NewLine;

                outScript.InnerText += sortida;
                errScript.InnerText = NoerrorsInfo;
            }
            else
            {
                outScript.InnerText = ExecutedString + path + "" + Environment.NewLine;
                outScript.InnerText += "-----------------------------------" + Environment.NewLine;

                outScript.InnerText += sortida;
                errScript.InnerText = errors;

            }
            labelExecucio.InnerText = RunOutputCaption;

        }
        /// <summary>
        /// estableix el web i sessió, de manera que no e'edita cap script
        /// </summary>
        private void netejaCosesScriptEditant()
        {
            parms.Controls.Clear();
            Controlador.scriptEditant = null;
            labelEdicio.InnerText = "";
            errScript.InnerText = "";
            outScript.InnerText = "";
        }
        /// <summary>
        /// crea estructura de objectes d'edició de paràmetres de script dins el ID de HTML corresponent.
        /// </summary>
        /// <param name="input">format entrada o sortida?</param>
        private void mostraObjectesParms(bool input)
        {
            int tmpInt = new int();
            parms.Controls.Clear();
            Panel nouDiv;
            Label etiqueta;
            Dictionary<string, object> objectesParms = Controlador.scriptEditant.parametresObjectes;
            Button execButton = new Button();
            execButton.Text = RunCaption;

            if (input)
            {
                execButton.ID = "butoExecutaInput";
            }
            else
            {
                execButton.ID = "butoExecutaOutput";
            }

            execButton.Click += new System.EventHandler(executaScriptEditant);
            execButton.CssClass = estilBotoCss;

            if (objectesParms == null)
            {
                parms.Controls.Add(execButton);
                return;

            }

            Button desaParms = new Button();
            desaParms.Text = SaveParmsCaption;
            desaParms.ID = "butoDesaParms";
            desaParms.Click += new System.EventHandler(desaParmsEditantClick);
            desaParms.CssClass = estilBotoCss;

            foreach (var cosa in objectesParms)
            {
                nouDiv = new Panel();
                nouDiv.Attributes["class"] = "col-sm-2";
                nouDiv.Attributes["runat"] = "server";

                switch (cosa.Value.GetType().ToString())
                {
                    case "System.Web.UI.WebControls.TextBox":
                        etiqueta = new Label();
                        etiqueta.Text = cosa.Key;
                        nouDiv.Controls.Add(etiqueta);
                        nouDiv.Controls.Add((TextBox)cosa.Value);
                        tmpInt++;
                        break;
                    case "System.Web.UI.WebControls.DropDownList":

                        etiqueta = new Label();
                        etiqueta.Text = cosa.Key;
                        nouDiv.Controls.Add(etiqueta);
                        nouDiv.Controls.Add((DropDownList)cosa.Value);
                        tmpInt++;

                        break;
                    case "System.Web.UI.WebControls.RadioButtonList":
                        nouDiv.Controls.Add((RadioButtonList)cosa.Value);

                        break;

                }
                parms.Controls.Add(nouDiv);
            }

            parms.Controls.Add(desaParms);
            parms.Controls.Add(execButton);

        }
        /// <summary>
        /// estabelix o re-estableix els objectes del web tals que corresponguin a el flux corresponent
        /// </summary>
        private void refrescaNomIListBoxes()
        {
            // posa informació gral de la XMLfile en edició per pantalla
            if (Controlador.sessionList != null)
            {
                mailDesti.Text = Controlador.sessionList.targetMail;
                Controlador.ompleListBoxDeScripts(scriptsInput, true);
                Controlador.ompleListBoxDeScripts(scriptsOutput, false);
            }
        }
        /// <summary>
        /// deprecated, trigger per saber si es mou el que edita, integrat ja en altres funcions
        /// </summary>
        /// <param name="input">entrada o sortida?</param>
        /// <returns></returns>
        private bool mouElQueEdita(bool input)
        {
            return false;
        }
        /// <summary>
        /// null
        /// </summary>
        /// <returns></returns>
        private bool sessioActiva()
        {
            return false;
        }

        #endregion

    }
}
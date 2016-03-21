using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CanLlistes
{
    /// <summary>
    /// classe quen conté els mètodes de crida des del web novaEntrada.aspx
    /// </summary>
    public partial class novaEntrada : System.Web.UI.Page
    {
        /// <summary>
        /// ha d'allotjar el controlador de la sessió que demana la pàgina
        /// </summary>
        private controlSessio Controlador;
        /// <summary>
        /// funció que es crida abans de mostrar el web
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            Controlador = (controlSessio)Application.Get(this.Session.SessionID);
            
            if (Controlador == null)
            {
                ((Global)this.Context.ApplicationInstance).renewSession();
                Server.Transfer("start.aspx");
            }
        }
        /// <summary>
        /// funció que es crida en polsar el cotó desar, codi principal de la pàgina
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void desar_Click(object sender, EventArgs e)
        {
            controlSessio controla = new controlSessio();
            if (controla.comprovaValorsPerCrearEstatica(nomNouScript.Text, correusNouScript.InnerText))
            {
                if (!controla.creaLlistaEstatica(nomNouScript.Text, correusNouScript.InnerText))
                {
                    labelError.Text = "Error creating file";
                }
                else
                {
                    labelError.Text = "Created, ok";
                }
            }
            else
            {
                labelError.Text =  "Error, Constrains not satisfied: file name end must be PS1, text must ONLY contain mail names, no empty lines allowed";
            }
        }
    }
}
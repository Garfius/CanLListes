using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;

namespace CanLlistes
{
    /// <summary>
    /// classe quen conté els mètodes de crida des del web start.aspx, la pàgina d'inici
    /// </summary>
    public partial class start : System.Web.UI.Page
    {
        /// <summary>
        /// ha d'allotjar el controlador de la sessió que demana la pàgina
        /// </summary>
        protected controlSessio Controlador;
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


    }
}
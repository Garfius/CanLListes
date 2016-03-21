using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CanLlistes
{
    /// <summary>
    /// classe de pàgina per previsió d'ampliació, per ara no s'usa
    /// </summary>
    public partial class tools : System.Web.UI.Page
    {
        private controlSessio Controlador;
        protected void Page_Load(object sender, EventArgs e)
        {
            Controlador = (controlSessio)Application.Get(this.Session.SessionID);
            
            if (Controlador == null)
            {
                ((Global)this.Context.ApplicationInstance).renewSession();
                Server.Transfer("start.aspx");
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            
            
        }
        protected void MycloseWindow(object sender, EventArgs e)
        {

        }
    }
}
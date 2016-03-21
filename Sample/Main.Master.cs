using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;

namespace CanLlistes
{
    /// <summary>
    /// classe de la pàgina mestra, que alberga el menú superior
    /// </summary>
    public partial class Main : System.Web.UI.MasterPage
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
        /// deprecated a referència d'antic etxtbox en menú sup.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void TextBox1_TextChanged(object sender, EventArgs e)
        {
            
        }
        /// <summary>
        /// funció cridada per el menú superior
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void clickInici(object sender, EventArgs e)
        {
            Response.Redirect("start.aspx");
        }
        /// <summary>
        /// funció cridada per el menú superior
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void clickLlistesActives(object sender, EventArgs e)
        {
            Response.Redirect("state.aspx");
        }
        /// <summary>
        /// funció cridada per el menú superior
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void clickNovaLlista(object sender, EventArgs e)
        {
            if (Controlador == null) { Server.Transfer("start.aspx"); }
            Controlador.sessionList = null;
            Response.Redirect("editor.aspx");
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

namespace CanLlistes
{
    /// <summary>
    /// classe quen conté els mètodes de crida des del web state.aspx
    /// </summary>
    public partial class state : System.Web.UI.Page
    {
        /// <summary>
        /// ha d'allotjar el controlador de la sessió que demana la pàgina
        /// </summary>
        controlSessio Controlador;
        private string ErrLoadingXML = "There was an error loading XML file";
        private string ErrStreamBusy = "Stream in use on another web session";
        private string ErrSavingXML = "There was an error saving the XML file";
        private string ErrRunning = "There was a problem executing this stream, are you already running another?";
        /// <summary>
        /// funció que es crida abans de mostrar el web
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            bool refrescant;
            Controlador = (controlSessio)Application.Get(this.Session.SessionID);
            //Controlador = ((Global)this.Context.ApplicationInstance).ObjectesDeDessio[Session.SessionID];
            
            //si ha caducat la session
            // el redireccionament per session nova ha de fer invalida thisSession part
            if (Controlador == null)
            {

                //Controlador = ((Global)this.Context.ApplicationInstance).renewSession();
                ((Global)this.Context.ApplicationInstance).renewSession();
                Server.Transfer("start.aspx");
            }
            else {
                Controlador.sessionList = null;
            }

            //carrega les llistes que ara existeixen per pantalla
            if (!IsPostBack)
            {
                refrescant = Controlador.listItemDeXMLs(llistesActives.Items, this.Context);

            }
            else {
                refrescant = tocarefrescar();
                
            }
            if (refrescant) {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "refresh", "window.setTimeout('var url = window.location.href;window.location.href = url',5000);", true);
            }
            
        }
        /// <summary>
        /// determina si toca refrescar automàticament el web
        /// </summary>
        /// <returns>true si cal refrescar</returns>
        private bool tocarefrescar()
        {
            ListBox listboxTemporal = new ListBox();
            return Controlador.listItemDeXMLs(listboxTemporal.Items, this.Context);

            
        }
        /// <summary>
        /// quan es polsa de crear una nova llista del menú lateral, redirigeix
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void novaLlista(object sender, EventArgs e) {
            Controlador.sessionList = null;
            Response.Redirect("editor.aspx");
            
        }
        /// <summary>
        /// quan es polsa d'editar un flux seleccionat al menú lateral
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void editaLlista(object sender, EventArgs e) {
            if (llistesActives.SelectedIndex > -1) {
                if (((Global)this.Context.ApplicationInstance).estalliureXML(llistesActives.SelectedValue, Session.SessionID))
                {

                    if (Controlador.carregaLlista(llistesActives.SelectedValue))
                    {
                        Response.Redirect("editor.aspx");
                    }
                    else {
                        outList.InnerText = ErrLoadingXML;
                    }
                }
                else {
                    outList.InnerText = ErrStreamBusy;
                    Server.Transfer(this.Context.Request.Path);
                }
            } 
        }
        /// <summary>
        /// quan es polsa d'eliminar un flux seleccionat al menú lateral
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void borraLlista(object sender, EventArgs e) {
            if (llistesActives.SelectedIndex > -1)
            {
                if (((Global)this.Context.ApplicationInstance).estalliureXML(llistesActives.SelectedValue))
                {
                    if (Controlador.borraFlux(llistesActives.SelectedValue))
                    {
                        Server.Transfer(this.Context.Request.Path);// no es vol un postback
                    }
                    else
                    {
                        outList.InnerText = ErrSavingXML;
                    }
                }
            } 
        }
        /// <summary>
        /// quan es polsa d'executar un flux seleccionat al menú lateral
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void executaFlux(object sender, EventArgs e) {
            if (llistesActives.SelectedIndex > -1)
            {
                if (((Global)this.Context.ApplicationInstance).estalliureXML(llistesActives.SelectedValue))
                {
                    Controlador.executaXMLAsincron(llistesActives.SelectedValue);
                    Server.Transfer(this.Context.Request.Path);
                }
                else {
                    outList.InnerText = ErrRunning;
                }
            } 
        }
        /// <summary>
        /// funció cridada quan es selecciona un flux, per a que es refresqui el text del web.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void llistesActives_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (llistesActives.SelectedIndex > -1)
            {
                outList.InnerHtml = Controlador.ultimaSortidaDeLlista(llistesActives.SelectedValue);
            } 
        }
    }
}
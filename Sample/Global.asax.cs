using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Optimization;
using System.Web.Security;
using System.Web.SessionState;
using System.Collections;
using System.Globalization;

namespace CanLlistes
{
    public class Global : System.Web.HttpApplication
    {
        /// <summary>
        /// for developing propouses enables multiple sessions on a single computer
        /// </summary>
        private bool developing = false;

        #region session and IIS events 

        protected void Application_Start(object sender, EventArgs e)
        {
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            
        }
        protected void Session_Start(object sender, EventArgs e)
        {
            controlSessio nou = newObjectForTheActualSession();
            
            if (!developing) { killOtherSessionsOfThisIP(nou.IpRemota, nou.sessionID); }

            Application[nou.sessionID] = nou;
            
        }
        protected void Session_End(object sender, EventArgs e)
        {
            Application.Remove(Session.SessionID);
        }
        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }
        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }
        protected void Application_Error(object sender, EventArgs e)
        {

        }
        protected void Application_End(object sender, EventArgs e)
        {

        }

        #endregion


        #region control functions
        /// <summary>
        /// Refreshes session object on IIS identified by session id
        /// </summary>
        public void renewSession()
        {
            controlSessio nou = newObjectForTheActualSession();

            if (!developing) { killOtherSessionsOfThisIP(nou.IpRemota, nou.sessionID); }

            Application[nou.sessionID] = nou;

        }
        /// <summary>
        /// checks whether any session is executing a stream related to an xml
        /// </summary>
        /// <param name="XMLfile">xml name to check</param>
        /// <remarks> checks runningXML on every session
        /// </remarks>
        /// <returns> true si algu esta executant
        /// </returns>
        public bool anybodyExecuting(string arxiuXML)
        {
            // ha de rebre XML
            bool found = false;
            controlSessio session;

            foreach (var storedSession in Application)
            {
                session = (controlSessio)Application.Get((string)storedSession);
                if (!string.IsNullOrEmpty(session.xmlExecutant))
                {
                    if (arxiuXML.Contains(session.xmlExecutant)) found = true;
                }
            }
            return found;
        }
        /// <summary>
        /// comprova si algú esta editant un xml, per nom de correu destinació
        /// </summary>
        /// <param name="XMLfile">arxiu xml a cercar en sessions</param>
        /// <param name="askingSession">hash de la sessió que ho demana perque no es detecti a si mateixa</param>
        /// <returns>true si algu edita</returns>
        public bool anybodyEditing(string XMLfile, string askingSession = "")
        {
            bool found = false;
            controlSessio session;
            foreach (var storedSession in Application)
            {
                
                session = (controlSessio)Application.Get((string)storedSession);
                
                if (session.sessionList != null)
                {
                    if (session.sessionList.targetMail.ToUpper().Equals(XMLfile.ToUpper().Substring(0, XMLfile.Length - 4)) && (!session.sessionID.Equals(askingSession))) found = true;
                }

            }
            return found;
        }

        /// <summary>
        /// elimina del servidor la sessió de la que es dona el hash
        /// </summary>
        /// <param name="sessionToKill">hash de sessió a matar</param>
        /// <returns>true si tot ha anat be</returns>
        public bool killSession(string sessionToKill)
        {

            controlSessio thisSession = new controlSessio();
            if (thisSession != null)
            {
                if (thisSession.xmlExecutant != "") thisSession.despresScriptAsincron();
                return true;
            }
            return false;

        }
        /// <summary>
        /// comprova si cap sessió es potencialment manipulador d'un xml donat
        /// </summary>
        /// <param name="XMLAmirar">nom de l'arxiu</param>
        /// <param name="askingSession">hash de sessió que ho demana per no trobar-se a ella</param>
        /// <returns>true si esta lliure</returns>
        public bool estalliureXML(string XMLAmirar, string sessioQueHoDemana = "")
        {
            return !(anybodyEditing(XMLAmirar, sessioQueHoDemana) || anybodyExecuting(XMLAmirar));
        }
        /// <summary>
        /// crea un objecte controlsessió nou segons la sessió actual
        /// </summary>
        /// <returns>objecte controlsessió</returns>
        private controlSessio newObjectForTheActualSession()
        {
            controlSessio nouObjecteSessio = new controlSessio();
            nouObjecteSessio.idioma = HttpContext.Current.Request.UserLanguages[0];
            nouObjecteSessio.sessionList = null;
            nouObjecteSessio.IpRemota = HttpContext.Current.Request.UserHostAddress;
            nouObjecteSessio.sessionID = HttpContext.Current.Session.SessionID;
            return nouObjecteSessio;
        }
        /// <summary>
        /// elimina de memória les altres sessions d'una mateixa ip
        /// </summary>
        /// <param name="ip">ip de la que cercar sessions</param>
        /// <param name="sessioBona">la sessió que ho demana, no es borra a si mateixa</param>
        private void killOtherSessionsOfThisIP(string ip, string sessioBona)
        {
            List<string> sessionsAMatar = new List<string>();
            controlSessio sessio;

            foreach (var sessioDesada in Application)
            {
                sessio = (controlSessio)Application.Get((string)sessioDesada);
                if (sessio.IpRemota.Equals(ip) && (!sessio.sessionID.Equals(sessioBona)))
                {

                    if (sessio.xmlExecutant != "") sessio.despresScriptAsincron();
                    sessionsAMatar.Add(sessio.sessionID);

                }
                if (sessio.sessionID.Equals(sessioBona))
                {
                    //error , ja hi ha una session valida per thisSession ip amb el mateix nom?
                }
            }
            foreach (var sessioDesada in sessionsAMatar)
            {

                Application.Remove(sessioDesada);

            }

        }

        #endregion

    }
}
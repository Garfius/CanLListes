using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Serialization;
using System.Management.Automation;
using System.Management.Automation.Language;
using Microsoft.VisualBasic.FileIO;
using System.Threading;

namespace CanLlistes
{
    /// <summary>
    /// Classe que representa una sessió, amb totes les funcionalitats que necessita per funcionar
    /// </summary>
    public class controlSessio
    {

        #region constants de configuracio
        /// <summary>
        /// Per pre-fixar el codi corresponent a el botó web de posar o treure
        /// </summary>
        /// <remarks>Queda desat en el XML</remarks>

        public String nomRadioButtonPosaTreu = "PosaOTreu";
        /// <summary>
        /// nom carpeta de treball en la que es desen els XML
        /// </summary>
        private string XMLsFolder = "listsXML";
        /// <summary>
        /// nom carpeta de treball en la que es desen els compilats en ps1 de les llistes
        /// </summary>
        private string scriptsLlistesFolder = "listsScripts";
        /// <summary>
        /// nom carpeta de treball on s'han de situar els scripts que actuen com a connectors d'entrada
        /// </summary>
        private string inputScriptsFolder = "inputScripts";
        /// <summary>
        /// nom carpeta de treball on s'han de situar els scripts que actuen com a connectors de sortida
        /// </summary>
        private string outputScriptsFolder = "outputScripts";
        /// <summary>
        /// nom carpeta en la que es desa el arxiu de registre
        /// </summary>
        ///<remarks>ha d'existir, no la crea</remarks>
        private string logFolder = "log";
        
        /// <summary>
        /// annexe amb el que es cerca l'arxiu de valors de paràmetre
        /// </summary>
        private string appendPerCSV = "_parms.csv";
        /// <summary>
        /// caràcter amb el que s'interpreta el csv
        /// </summary>
        private string separadorCSV = ",";
        /// <summary>
        /// Annex que ha de rebre, i que es cerca els resultats d'execució
        /// </summary>
        private string appendPerOUT = "_lastOut.txt";
        /// <summary>
        /// valor de mostra i desat en xml que s'utilitza per a desactivar un paràmetre de valors prefixats
        /// </summary>
        private string valorParmDisabled = "-disabled-";
        /// <summary>
        /// nom del script extern que actua com a restador de línies 
        /// </summary>
        private string scripResta = "substact.ps1";
        /// <summary>
        /// conté el nom de l'arxiu del últim output generat
        /// </summary>
        private string arxiuResultatUltimInput = null;
        /// <summary>
        /// objecte asíncron d'execució, ha de ser null si no s'usa
        /// </summary>
        private Thread fluxeExecucio;
        /// <summary>
        /// llista de tres valors prefixada, amb el conjunt de paràmetres que conformen la interfície sencera d'un script de sortida
        /// </summary>
        private List<string> parametresNecessarisScripsSortida = new List<string>()
	    {// no alterar l'ordre !!
	        "inputMails",// 1st sortida
	        "listMailName",// 2n nom descriptiu
	        "removeList" //3r eliminar
	    };

        private string InfoEditing = "On edition";
        private string InfoRunning = "On execution";
        private string InfoNoLastOutpuFound = "File from the last run not found";
        private string InfoAdd = "Add";
        private string InfoRemove = "Remove";
        private string InfoNoParameters = "-no parameters-";
        private string InfoOutputFileLocked = "Error, cannot open output file";
        private string InfoErrors =  "Errors";
        private string InfoOutput = "Output";
        private string InfoOutputRun = "Output from the run";
        private string addSubstract = "addSubstract";
        private string InfoErrCSV = "Error parsing csv";
        private string InfoErrParm = "There was a problem setting parameter from: ";
        private string InfoIncompleteParms = "Incomplete parameters";

        #endregion

        #region variables d'estat
        /// <summary>
        /// cultura donada per el navegador en thisSession sessió
        /// </summary>
        public String idioma;
        /// <summary>
        /// ip remota de l'usuari
        /// </summary>
        public String IpRemota;
        /// <summary>
        /// en el cas quue s'estigui editant un flux, esta contingut aqui
        /// </summary>
        public flux sessionList;
        /// <summary>
        /// en el cas quue s'estigui editant un script, esta contingut aqui
        /// </summary>
        public script scriptEditant;
        /// <summary>
        /// ID hash de la sessió que representa aquest objecte
        /// </summary>
        public String sessionID;
        /// <summary>
        /// en el cas que s'estigui executant un flux, aqui hi ha d'haver el nom del seu XML
        /// </summary>
        public String xmlExecutant = "";
        /// <summary>
        /// conté el moment d'inici de l'execució
        /// </summary>
        public DateTime iniciExecucioAsincrona;

        #endregion

        #region funcions publiques
        /// <summary>
        /// desa en l'arxiu de registre, amb marca de data
        /// </summary>
        /// <param name="texte">text a registrar</param>
        public void logueja(string texte)
        {
            // posar dins un try per si 2 loguegen al mateix temps
            StreamWriter objecteLog = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + logFolder + @"\log.txt", true);
            objecteLog.WriteLine(DateTime.Now.ToString("MM/dd/yyyy h:mm tt") + texte);
            objecteLog.Close();

        }
        /// <summary>
        /// inicia la creació i execució del script que ha de eliminar un flux i la/es llista corresponents
        /// </summary>
        /// <param name="XMLfile">Arxiu xml del que s'ha de borrar tot</param>
        /// <returns>si ha anat be torna true</returns>
        public bool borraFlux(string arxiuXML)
        {
            bool retorn = true;
            bool scriptSuicidaCreat = false;
            string arxiuAmbRuta = AppDomain.CurrentDomain.BaseDirectory + XMLsFolder + @"\" + arxiuXML;
            string nomCorreu = arxiuXML.Substring(0, arxiuXML.Length - 4);
            //string[] directori = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + scriptsLlistesFolder);

            if (creaScriptSuicida(arxiuXML))//crea ps1 que borra la flux
            {

                try
                {
                    executaXMLAsincron(arxiuXML);
                    File.Delete(arxiuAmbRuta);
                    retorn = true;
                }
                catch
                {
                    retorn = false;
                }
            }
            else
            {
                try
                {
                    File.Delete(arxiuAmbRuta);
                    retorn = true;
                }
                catch
                {
                    retorn = false;
                }
            }

            return retorn;
        }
        /// <summary>
        /// carrega en objecte de sessió l'arxiu xml en forma d'objectes web
        /// </summary>
        /// <param name="arxiu">arxiu xml d'entrada</param>
        /// <returns>true si ha carregat be</returns>
        public bool carregaLlista(string arxiu)
        {
            bool totcorrecte = false;
            // el listBoxDesti es per si falla

            fluxSeriable llistaCarregada = new fluxSeriable();

            totcorrecte = carregaObjecteDesdeXML(ref llistaCarregada, AppDomain.CurrentDomain.BaseDirectory + this.XMLsFolder + @"\" + arxiu);

            this.scriptEditant = null;

            sessionList = creaISeleccionaObjectesDesDeParametres(llistaCarregada);

            return (llistaCarregada.mailDesti != "");




        }
        /// <summary>
        /// retorna un listbox tipus web amb tots els XML's formatat per ser vist
        /// </summary>
        /// <param name="llistaRetorna">objecte Web que retorna per ref.</param>
        /// <param name="contexte">li cal el context per poder consultar el global</param>
        /// <returns></returns>
        public bool listItemDeXMLs(ListItemCollection llistaRetorna, HttpContext contexte)
        {
            bool retorn = false;
            string[] directori = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + XMLsFolder);
            string nom;
            llistaRetorna.Clear();

            foreach (string arxiu in directori)
            {
                if (arxiu.ToUpper().Contains(".XML"))
                {
                    
                    nom = arxiu.Split('\\')[arxiu.Split('\\').Count() - 1];
                    if (((Global)contexte.ApplicationInstance).anybodyEditing(nom)) {
                        nom += " | " + InfoEditing;
                    }
                    if (((Global)contexte.ApplicationInstance).anybodyExecuting(nom))
                    {
                        nom += " | " + InfoRunning;
                        retorn = true;// que refresqui
                    }
                    llistaRetorna.Add(nom);
                }
            }
            return retorn;
        }
        /// <summary>
        /// retorna un listbox tipus web amb tots els scripts d'entrada
        /// </summary>
        /// <param name="llistaRetorna">by ref. la llista a omplir</param>
        public void listItemScriptsInput( ListItemCollection llistaRetorna) {
            listItemDeScripts(AppDomain.CurrentDomain.BaseDirectory + inputScriptsFolder, llistaRetorna);
        }
        /// <summary>
        /// retorna un listbox tipus web amb tots els scripts d'entrada
        /// </summary>
        /// <param name="llistaRetorna">by ref. la llista a omplir</param>
        public void listItemScriptsOutput( ListItemCollection llistaRetorna) {
            listItemDeScripts(AppDomain.CurrentDomain.BaseDirectory + outputScriptsFolder, llistaRetorna);
        }
        /// <summary>
        /// desa en arxiu xml del nom del correu del flux en edició la llista que s'esta editant
        /// </summary>
        /// <returns>true si ha anat be</returns>
        public bool desaFluxEditant() {
            bool retorn;
            // el listBoxDesti es per si falla

            if (llistaCorrecte(sessionList))
            {

                // posa els parms en hash desde objectes
                //listBoxDesti = desaObjecteEnXML(sessionList, AppDomain.CurrentDomain.BaseDirectory +this.XMLsFolder+@"\"+ sessionList.nomDescriptiu+".xml");
                retorn = desaObjecteEnXML(fluxSeriableDeFlux(sessionList), AppDomain.CurrentDomain.BaseDirectory + this.XMLsFolder + @"\" + sessionList.targetMail+ ".xml");

                ;//recupera els parms en objectes
                return retorn;
            }
            else {
                return false;
            }
            
        }
        /// <summary>
        /// carrega i deixa preparat en la llista de la sessió el arxiu script connector
        /// </summary>
        /// <param name="nom">nom del script connector</param>
        /// <param name="input">en les d'entrada o sortida?</param>
        /// <returns>true si ha anat be</returns>
        public bool afegeixScriptDesDArxiu(string nom, bool input)
        {
            //carrega valors de zero a partir d'un nom
            //List<string> parmsTemp = new List<string>();
            int tmpInt = 0;
            string nomArxiu;
            bool correcte = true;
            List<string> paramatresArxiu = new List<string>();
            script scriptCarregant = new script();
            scriptCarregant.nom = nom;

            if (input)
            {
                nomArxiu = AppDomain.CurrentDomain.BaseDirectory + inputScriptsFolder + @"\" + scriptCarregant.nom;
            }
            else
            {
                nomArxiu = AppDomain.CurrentDomain.BaseDirectory + outputScriptsFolder + @"\" + scriptCarregant.nom;
            }

            if (!File.Exists(nomArxiu)) return false;

            if (llistaParmsDeArxiu(nomArxiu, ref paramatresArxiu))
            {
                if (input)
                {
                    //scriptCarregant.objectsParms = diccioParametres(nomArxiu, scriptCarregant.parms, input);
                    scriptCarregant.parametresObjectes = diccioParametres(nomArxiu, paramatresArxiu, input);
                    sessionList.scriptsInput.Add(scriptCarregant);
                }
                else
                {

                    foreach (var parm in parametresNecessarisScripsSortida)
                    {// tru els parametres de tractament intern de la XMLfile dels que s'han de crear com a objectes per interacció de lusuari
                        if (paramatresArxiu.Contains(parm.ToLower()))
                        {
                            // s'ha de mirar i marcar si es complert abans de borrar-los
                            paramatresArxiu.RemoveAt(paramatresArxiu.IndexOf(parm.ToLower()));
                            tmpInt++;
                        }

                    }
                    scriptCarregant.interficieSortidaSencera = (tmpInt == parametresNecessarisScripsSortida.Count);
                    scriptCarregant.parametresObjectes = diccioParametres(nomArxiu, paramatresArxiu, input);

                    sessionList.scriptsOutput.Add(scriptCarregant);


                }
            }
            else
            {
                return false;
            }
            return correcte;
        }
        /// <summary>
        /// dona, si existeix el text del últim output del script corresponent al que s'esta editant i s'allotja en var. d'estat
        /// </summary>
        /// <param name="input">si és d'entrada o sortida</param>
        /// <returns>cadena de text amb el resultat</returns>
        public string ultimaSortidaScriptEditant(bool input)
        {
            string nomArxiu;
            if (input)
            {
                nomArxiu = AppDomain.CurrentDomain.BaseDirectory + inputScriptsFolder + @"\" + scriptEditant.nom;
            }
            else
            {
                nomArxiu = AppDomain.CurrentDomain.BaseDirectory + outputScriptsFolder + @"\" + scriptEditant.nom;
            }
            nomArxiu = nomArxiu.Substring(0, nomArxiu.Length - 4) + appendPerOUT;

            if (input) arxiuResultatUltimInput = nomArxiu;

            if (File.Exists(nomArxiu))
            {
                return File.ReadAllText(nomArxiu);
            }
            else
            {
                return "";
            }
        }
        /// <summary>
        /// crea un script d'entrada que retorna per StdOut els mails passats per paràmetre
        /// </summary>
        /// <param name="nomScript">nom que ha de prendre l'arxiu</param>
        /// <param name="mails">llistat de mails amb salts de línia </param>
        /// <returns>true si ha anat be</returns>
        public bool creaLlistaEstatica(string nomScript, string mails)
        {

            string nomArxiu = AppDomain.CurrentDomain.BaseDirectory + inputScriptsFolder + @"\" + nomScript;

            try
            {
                if (File.Exists(nomArxiu))
                {
                    File.Delete(nomArxiu);
                }

                using (StreamWriter outputFile = new StreamWriter(nomArxiu))
                {
                    foreach (var linia in mails.Split('\n'))
                    {

                        //excepte el ultim caracter que és \r
                        outputFile.WriteLine("write-output " + linia.Trim('\r'));
                    }

                }
                return true;
            }
            catch
            {
                return false;
            }


            return true;
        }
        /// <summary>
        /// crea el script compilat que representa el flux que es dona
        /// </summary>
        /// <param name="llistaACompilar">el flux a compilar en script ps1</param>
        /// <returns>true si ha anat be</returns>
        public bool creaScriptDeLlista(flux llistaACompilar)
        {
            // to-do s'ha de convertir a scriptseriable 
            if (!llistaCorrecte(llistaACompilar)) return false;
            string arxiuScriptACrear;
            string arxiuSortida;
            string contingutScript = "";
            string restador = AppDomain.CurrentDomain.BaseDirectory + scriptsLlistesFolder + @"\" + scripResta;
            string arxiuSostreu = AppDomain.CurrentDomain.BaseDirectory + scriptsLlistesFolder + @"\substractTemp.txt";
            arxiuScriptACrear = AppDomain.CurrentDomain.BaseDirectory + scriptsLlistesFolder + @"\" + llistaACompilar.targetMail + ".ps1";
            arxiuSortida = AppDomain.CurrentDomain.BaseDirectory + scriptsLlistesFolder + @"\" + llistaACompilar.targetMail + appendPerOUT;

            contingutScript += "Remove-Item \"" + arxiuSortida + "\"" + Environment.NewLine;

            foreach (var scriptEntrada in llistaACompilar.scriptsInput)
            {
                if (scriptDesableDeScript(scriptEntrada).addSubstract)
                {
                    //scriptAPassar.addSubstract = (radioTemp.SelectedIndex == 0); aqui s'afegeix

                    contingutScript += "& \"" + AppDomain.CurrentDomain.BaseDirectory + inputScriptsFolder + @"\" + scriptEntrada.nom + "\"" + parametresDeScript(scriptEntrada, true, false) + " >> \"" + arxiuSortida + "\"" + Environment.NewLine;
                }
                else
                {
                    contingutScript += "& \"" + AppDomain.CurrentDomain.BaseDirectory + inputScriptsFolder + @"\" + scriptEntrada.nom + "\"" + parametresDeScript(scriptEntrada, true, false) + " > \"" + arxiuSostreu + "\"" + Environment.NewLine;
                    contingutScript += "& \"" + restador + "\" -main \"" + arxiuSortida + "\" -substract \"" + arxiuSostreu + "\"" + Environment.NewLine;
                }
            }

            foreach (var scriptSortida in llistaACompilar.scriptsOutput)
            {
                contingutScript += "& \"" + AppDomain.CurrentDomain.BaseDirectory + outputScriptsFolder + @"\" + scriptSortida.nom + "\"" + parametresDeScript(scriptSortida, false, false, llistaACompilar.targetMail) + " -input " + "\"" + arxiuSortida + "\"" + Environment.NewLine;
            }

            if (File.Exists(arxiuScriptACrear))
            {
                File.Delete(arxiuScriptACrear);
            }
            File.WriteAllText(arxiuScriptACrear, contingutScript);

            return true;
        }
        /// <summary>
        /// dona, si existeix el text del últim output del flux desitjat
        /// </summary>
        /// <param name="llistaAMirar">nom del xml o script</param>
        /// <returns>true si tot ha anat be</returns>
        public string ultimaSortidaDeLlista(string llistaAMirar)
        {

            string arxiuSortida = AppDomain.CurrentDomain.BaseDirectory + scriptsLlistesFolder + @"\" + llistaAMirar.Substring(0, llistaAMirar.Length - 4) + appendPerOUT;
            if (File.Exists(arxiuSortida))
            {
                return File.ReadAllText(arxiuSortida);
            }
            return InfoNoLastOutpuFound;
        }
        /// <summary>
        /// mou amunt si es pot l'item seleccionat
        /// </summary>
        /// <param name="llistaATocar">listbox a tractar</param>
        /// <param name="input">si és d'entrada o sortida</param>
        public void pujaItem(ListBox llistaATocar, bool input)
        {
            List<script> scriptsAMoure;
            if (input)
            {
                scriptsAMoure = sessionList.scriptsInput;
            }
            else
            {
                scriptsAMoure = sessionList.scriptsOutput;
            }

            if (llistaATocar.SelectedIndex > 0)
            {

                scriptsAMoure.Insert(llistaATocar.SelectedIndex - 1, scriptsAMoure[llistaATocar.SelectedIndex]);
                scriptsAMoure.RemoveAt(llistaATocar.SelectedIndex + 1);
                
            }


        }
        /// <summary>
        /// baixa si es pot l'item seleccionat
        /// </summary>
        /// <param name="llistaATocar">listbox a tractar</param>
        /// <param name="input">si és d'entrada o sortida</param>
        public void baixaItem(ListBox llistaATocar, bool input)
        {
            List<script> scriptsAMoure;
            if (input)
            {
                scriptsAMoure = sessionList.scriptsInput;
            }
            else
            {
                scriptsAMoure = sessionList.scriptsOutput;
            }

            if (llistaATocar.SelectedIndex < (llistaATocar.Items.Count - 1))
            {

                scriptsAMoure.Insert(llistaATocar.SelectedIndex + 2, scriptsAMoure[llistaATocar.SelectedIndex]);
                scriptsAMoure.RemoveAt(llistaATocar.SelectedIndex);
                /*llistaATocar.Items.Insert(llistaATocar.SelectedIndex+2, llistaATocar.SelectedItem.Text);
                llistaATocar.Items.RemoveAt(llistaATocar.SelectedIndex);*/

            }

        }
        /// <summary>
        /// omple l'objecte per ref. amb els scripts de la carpeta corresponent
        /// </summary>
        /// <param name="listBoxDesti">listbox a tractar</param>
        /// <param name="input">si és d'entrada o sortida</param>
        public void ompleListBoxDeScripts(ListBox listBoxDesti, bool input)
        {
            // passa a listbox els scripts 
            string descripccio;
            TextBox texte;
            DropDownList dropu;
            RadioButtonList radial;
            List<script> llistaDeScripts;
            if (input)
            {
                llistaDeScripts = sessionList.scriptsInput;
            }
            else
            {
                llistaDeScripts = sessionList.scriptsOutput;
            }


            listBoxDesti.Items.Clear();
            foreach (var script in llistaDeScripts)
            {
                descripccio = script.nom + @" |" + listBoxDesti.Items.Count + "| ";


                if (!input)
                {
                    if (!script.interficieSortidaSencera)
                    {
                        descripccio +=InfoIncompleteParms;
                    }
                }

                if (script.parametresObjectes != null)// en cas de output no te pq tindre cap objecte
                {
                    foreach (var cosa in script.parametresObjectes)
                    {
                        switch (cosa.Value.GetType().ToString())
                        {
                            case "System.Web.UI.WebControls.TextBox":
                                texte = (TextBox)cosa.Value;
                                descripccio = descripccio + "," + texte.Text;
                                break;
                            case "System.Web.UI.WebControls.DropDownList":
                                dropu = (DropDownList)cosa.Value;
                                //to-do si no te valor seleccionat
                                descripccio = descripccio + "," + dropu.SelectedValue;
                                break;
                            case "System.Web.UI.WebControls.RadioButtonList":
                                radial = ((RadioButtonList)cosa.Value);
                                if (radial.SelectedIndex == 0)
                                {
                                    descripccio = descripccio + ","+InfoAdd;
                                }
                                else
                                {
                                    descripccio = descripccio + "," + InfoRemove;
                                }
                                break;
                        }
                    }
                }
                else
                {
                    descripccio = descripccio + " "+InfoNoParameters;
                }


                listBoxDesti.Items.Add(descripccio);
            }
            //listBoxDesti.ClearSelection();

        }
        /// <summary>
        /// executa sincronament el script que s'està editant
        /// </summary>
        /// <param name="standardOutput">by ref. retorna el resultat de StdOut</param>
        /// <param name="errorOutput">by ref. retorna el resultat de StdErr formatat</param>
        /// <param name="path">retorna la cadena executada</param>
        /// <param name="input">entrada o sortida?</param>
        /// <returns>true si tot ha anat be</returns>
        public bool executaScriptEditant(out string standardOutput, out string errorOutput, out string path, bool input)
        {
            string nomArxiu;

            if (input)
            {
                path = AppDomain.CurrentDomain.BaseDirectory + inputScriptsFolder + @"\" + scriptEditant.nom;
            }
            else
            {
                path = AppDomain.CurrentDomain.BaseDirectory + outputScriptsFolder + @"\" + scriptEditant.nom;
            }

            nomArxiu = path;

            errorOutput = "";
            standardOutput = "";
            using (PowerShell PowerShellInstance = PowerShell.Create())
            {

                // use "AddScript" to add the contents of a script file to the end of the execution pipeline.
                // use "AddCommand" to add individual commands/cmdlets to the end of the execution pipeline.
                PowerShellInstance.Runspace.SessionStateProxy.Path.SetLocation(path.Substring(0, path.LastIndexOf('\\')));

                //path = "\"C:\\Users\\garf\\Documents\\Visual Studio 2013\\Projects\\WebApplication4\\WebApplication4\\jsonDemo.ps1\"";
                //path = System.IO.Path.GetFullPath(path);
                //path = path.Replace(" ", spaceCharacter);
                //path = "& \"" + path.Replace(@"\\",@"\")+ "\"";

                path = "& \"" + path + "\"";
                // use "AddParameter" to add a single parameter to the last command/script on the pipeline.

                path += parametresDeScript(scriptEditant, input, true, sessionList.targetMail);

                PowerShellInstance.AddScript(path, true);

                //PowerShellInstance.AddParameter("param1", "parameter 1 value!");

                //to-do peta si esta obert
                Collection<PSObject> PSOutput = PowerShellInstance.Invoke();

                // check the other output streams (for example, the error stream)
                if (PowerShellInstance.Streams.Error.Count > 0)
                {
                    foreach (var errorItem in PowerShellInstance.Streams.Error)
                    {
                        errorOutput += errorItem.CategoryInfo.Activity.ToString() + " | " + errorItem.ToString() + Environment.NewLine;
                    }

                    // error records were written to the error stream.
                    // do something with the items found.
                }
                foreach (PSObject outputItem in PSOutput)
                {
                    // if null object was dumped to the pipeline during the script then a null
                    // object may be present here. check for null to prevent potential NRE.
                    if (outputItem != null)
                    {
                        //TODO: do something with the output item 
                        /*Console.WriteLine(outputItem.BaseObject.GetType().FullName);
                        Console.WriteLine(outputItem.BaseObject.ToString() + "\n");*/
                        standardOutput += outputItem.BaseObject.ToString() + Environment.NewLine;
                    }
                }
            }

            escriuOutDeScript(standardOutput, nomArxiu);

            return string.IsNullOrWhiteSpace(errorOutput);
        }
        /// <summary>
        /// comprova el nom i mails si son correctes
        /// </summary>
        /// <param name="nomScript">nom desitjat</param>
        /// <param name="correus">correus a posar-hi</param>
        /// <returns>true si tot esta be</returns>
        /// <remarks>per usar abans de crear</remarks>
        public bool comprovaValorsPerCrearEstatica(string nomScript, string correus)
        {
            RegexUtilities comprovador = new RegexUtilities();
            if (nomScript.ToUpper().IndexOf("PS1") != nomScript.Length - 3)
            {
                return false;
            }
            try
            {
                foreach (var linia in correus.Split('\n'))
                {
                    if (!comprovador.IsValidEmail(linia.Trim('\r')))
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// llença la execució asíncrona del ps1 corresponent al xml donat
        /// </summary>
        /// <param name="XMLfile">xml del flux que es vol executar</param>
        public void executaXMLAsincron(string arxiuXML)
        {
            xmlExecutant = arxiuXML;
            StartTheThread(AppDomain.CurrentDomain.BaseDirectory + scriptsLlistesFolder + @"\" + arxiuXML.Substring(0, arxiuXML.Length - 3) + "ps1");
        }
        /// <summary>
        /// inicia el thread
        /// </summary>
        /// <param name="param1">ordre a executar</param>
        /// <remarks>d'ús explusiu per executaXMLAsincron</remarks>
        public void StartTheThread(string param1)
        {

            iniciExecucioAsincrona = DateTime.Now;

            fluxeExecucio = new Thread(() => { execucioSimple(param1); despresScriptAsincron(); });
            fluxeExecucio.Start();

        }
        /// <summary>
        /// neteja les variables s'estat despres de la execució asíncrona
        /// </summary>
        public void despresScriptAsincron()
        {
            this.fluxeExecucio = null;
            this.xmlExecutant = "";
            //fluxeExecucio.Abort();
        }
        /// <summary>
        /// inicia la execució
        /// </summary>
        /// <param name="path">ordre</param>
        /// <remarks>d'ús exclusiu per StartTheThread</remarks>
        public void execucioSimple(string path)
        {

            string arxiuSortida = path.Substring(0, path.Length - 4) + appendPerOUT;

            string resultat = "";

            StreamWriter escriptor;
            try
            {
                if (File.Exists(arxiuSortida))
                {
                    File.Delete(arxiuSortida);

                }
            }
            catch
            {
                resultat = InfoOutputFileLocked + Environment.NewLine;
            }


            using (PowerShell PowerShellInstance = PowerShell.Create())
            {

                PowerShellInstance.Runspace.SessionStateProxy.Path.SetLocation(path.Substring(0, path.LastIndexOf('\\')));

                path = "& \"" + path + "\"";

                PowerShellInstance.AddScript(path, true);

                //PowerShellInstance.AddParameter("param1", "parameter 1 value!");

                //to-do peta si esta obert
                Collection<PSObject> PSOutput = PowerShellInstance.Invoke();
                resultat += "--------------------"+InfoErrors+"------------------" + Environment.NewLine;
                // check the other output streams (for example, the error stream)
                if (PowerShellInstance.Streams.Error.Count > 0)
                {
                    foreach (var errorItem in PowerShellInstance.Streams.Error)
                    {
                        resultat += "ERROR:" + errorItem.CategoryInfo.Activity.ToString() + " | " + errorItem.ToString() + Environment.NewLine;
                    }

                    // error records were written to the error stream.
                    // do something with the items found.
                }
                resultat += "--------------------"+InfoOutput+"------------------" + Environment.NewLine;
                foreach (PSObject outputItem in PSOutput)
                {
                    // if null object was dumped to the pipeline during the script then a null
                    // object may be present here. check for null to prevent potential NRE.
                    if (outputItem != null)
                    {
                        //TODO: do something with the output item 
                        /*Console.WriteLine(outputItem.BaseObject.GetType().FullName);
                        Console.WriteLine(outputItem.BaseObject.ToString() + "\n");*/
                        resultat += outputItem.BaseObject.ToString() + Environment.NewLine;
                    }
                }
            }

            using (Stream stream = new FileStream(arxiuSortida, FileMode.OpenOrCreate))
            {
                if (stream.Length > 0)
                {
                    stream.Seek(stream.Length - 1, 0);
                }
                System.Text.Encoding cudifica = System.Text.Encoding.BigEndianUnicode;
                escriptor = new StreamWriter(stream, cudifica);

                escriptor.WriteLine("---------------"+InfoOutputRun+"-----------------" + Environment.NewLine);
                escriptor.Write(resultat);
                escriptor.Write((char)0);
                escriptor.Close();
            }




        }
        /// <summary>
        /// crea el script que ha d'eliminar el flux i la/es llista associades
        /// </summary>
        /// <param name="XMLfile">xml del que es preté eliminar tot</param>
        /// <returns>true si ha anat be</returns>
        public bool creaScriptSuicida(string arxiuXML)
        {
            bool retorn = false;
            fluxSeriable llistaCarregada = new fluxSeriable();
            string arxiuScriptACrear = AppDomain.CurrentDomain.BaseDirectory + scriptsLlistesFolder + @"\" + arxiuXML.Substring(0, arxiuXML.Length - 3) + "ps1";
            string contingutScript = "";

            if (carregaObjecteDesdeXML(ref llistaCarregada, AppDomain.CurrentDomain.BaseDirectory + this.XMLsFolder + @"\" + arxiuXML))
            {
                // si la ha carregat toca executar els outputs amb removeList
                foreach (var sortida in llistaCarregada.scriptsOutput)
                {
                    if (sortida.interficieSortidaSencera)
                    {
                        contingutScript += "& \"" + AppDomain.CurrentDomain.BaseDirectory + outputScriptsFolder + @"\" + sortida.nom + "\" -" + parametresNecessarisScripsSortida[2] + " $true -" + parametresNecessarisScripsSortida[1] + " " + llistaCarregada.mailDesti + Environment.NewLine;
                        retorn = true;

                    }
                }

            }
            else
            {
                return false;
            }
            try
            {
                if (File.Exists(arxiuScriptACrear))
                {
                    File.Delete(arxiuScriptACrear);
                }
                File.WriteAllText(arxiuScriptACrear, contingutScript);
                retorn = true;
            }
            catch
            {
                retorn = false;
            }


            return retorn;
        }


        #endregion

        #region funcions privades
        /// <summary>
        /// retorna l'objecte seriable corresponent al flux passat
        /// </summary>
        /// <param name="llistaAPassar">flux amb ojectes Web</param>
        /// <returns>flux seriable sense objectes web</returns>
        private fluxSeriable fluxSeriableDeFlux(flux llistaAPassar)
        {
            fluxSeriable surtidaa = new fluxSeriable();
            //surtidaa.nomDescriptiu = llistaAPassar.nomDescriptiu;
            surtidaa.mailDesti = llistaAPassar.targetMail;
            surtidaa.scriptsInput = new List<scriptSeriable>();
            surtidaa.scriptsOutput= new List<scriptSeriable>();

             foreach (script scriptEntrada in llistaAPassar.scriptsInput)
            {
                // per cada script d'entrada
                surtidaa.scriptsInput.Add(scriptDesableDeScript(scriptEntrada));

                

            }
            foreach (script scriptSortida in llistaAPassar.scriptsOutput)
            {
                // per cada script de sortida
                if (scriptSortida.parametresObjectes != null)
                {
                    surtidaa.scriptsOutput.Add(scriptDesableDeScript(scriptSortida));
                }
                
            }
            
            
            return surtidaa;
        }
        /// <summary>
        /// retorna l'objecte seriable corresponent al script passat
        /// </summary>
        /// <param name="scriptAPassar">script amb ojectes Web</param>
        /// <returns>script sense objectes web</returns>
        private scriptSeriable scriptDesableDeScript(script scriptAPassar)
        {
            scriptSeriable surtidaa = new scriptSeriable();
            RadioButtonList radioTemp;
            TextBox textTemp;
            DropDownList dropTemp;
            surtidaa.valorsParms = new List<string>();
            surtidaa.nomsParms = new List<string>();
            surtidaa.input = scriptAPassar.input;
            surtidaa.nom = scriptAPassar.nom;
            surtidaa.interficieSortidaSencera = scriptAPassar.interficieSortidaSencera;
            

            foreach (var cosa in scriptAPassar.parametresObjectes)
            {
                switch (cosa.Value.GetType().ToString())
                {

                    case "System.Web.UI.WebControls.TextBox":
                        textTemp = (TextBox)cosa.Value;
                        surtidaa.nomsParms.Add(cosa.Key);
                        surtidaa.valorsParms.Add((string)textTemp.Text.Replace(';', ' '));// per seguretat no es contempla pnt i coma

                        break;
                    case "System.Web.UI.WebControls.DropDownList":
                        dropTemp = (DropDownList)cosa.Value;
                        surtidaa.nomsParms.Add(cosa.Key);
                        surtidaa.valorsParms.Add((string)dropTemp.SelectedValue);
                        break;
                    case "System.Web.UI.WebControls.RadioButtonList":
                        radioTemp = (RadioButtonList)cosa.Value;
                        surtidaa.addSubstract = (radioTemp.SelectedIndex == 0);
                        break;
                }

            }


            return surtidaa;
        }
        /// <summary>
        /// converteix en objecte de treball web l'objecte de flux seriable
        /// </summary>
        /// <param name="llistaEntrada">flux seriable, de xml potser</param>
        /// <returns>flux tractable en Web</returns>
        private flux creaISeleccionaObjectesDesDeParametres(fluxSeriable llistaEntrada)
        {
            //despres de desar i quan carregues
            // passa els valors dels objectes web no serialitzables en una hashtable que si ho es
            flux surtidaa = new flux();
            script scriptObjTemp;
            //surtidaa.nomDescriptiu = llistaEntrada.nomDescriptiu;
            surtidaa.targetMail = llistaEntrada.mailDesti;
            foreach (scriptSeriable scriptEntrada in llistaEntrada.scriptsInput)
            {
                scriptObjTemp = new script();
                // per cada script d'entrada
                scriptObjTemp.input = scriptEntrada.input;
                scriptObjTemp.nom = scriptEntrada.nom;

                scriptObjTemp.parametresObjectes = diccioParametres(AppDomain.CurrentDomain.BaseDirectory + inputScriptsFolder + @"\" + scriptEntrada.nom, scriptEntrada.nomsParms, true);

                seleccionaValorsEnObjectes(ref scriptObjTemp,scriptEntrada);
                surtidaa.scriptsInput.Add(scriptObjTemp);

            }

            foreach (scriptSeriable scriptSortida in llistaEntrada.scriptsOutput)
            {
                scriptObjTemp = new script();
                scriptObjTemp.input = scriptSortida.input;
                scriptObjTemp.nom = scriptSortida.nom;
                scriptObjTemp.interficieSortidaSencera = scriptSortida.interficieSortidaSencera;
                if (scriptSortida.nomsParms != null)
                {
                    
                    // per cada script de sortida
                    scriptObjTemp.parametresObjectes = diccioParametres(AppDomain.CurrentDomain.BaseDirectory + outputScriptsFolder + @"\" + scriptSortida.nom, scriptSortida.nomsParms, false);
                    // els de sortida poden no tindre objectes
                    seleccionaValorsEnObjectes(ref scriptObjTemp, scriptSortida);

                    
                }
                surtidaa.scriptsOutput.Add(scriptObjTemp);
            }
            return surtidaa;

        }
        /// <summary>
        /// crea el diccionari que ha de residir en cada script per poder-se tractar en web
        /// </summary>
        /// <param name="arxiuScript">nom del script per buscar paràmetres</param>
        /// <param name="parms">paràmetres a buscar</param>
        /// <param name="input">entrada o sortida? Molt diferent</param>
        /// <returns>el diccionari amb objectes web</returns>
        private Dictionary<string, object> diccioParametres(string arxiuScript, List<string> parms, bool input)
        {
            //si el parm existeix crea listbox
            //sino textbox
            int contador = 0;
            string folderDesti;
            string nomarxiu = arxiuScript.Substring(0, arxiuScript.Length - 4) + appendPerCSV;
            Dictionary<string, object> trobats = new Dictionary<string, object>();// diccionari nomparametre --> objecte

            if (input)
            {
                RadioButtonList radiu = new RadioButtonList();
                radiu.Items.Add(new ListItem(InfoAdd));
                radiu.Items.Add(new ListItem(InfoRemove));
                radiu.ID = addSubstract;
                radiu.SelectedIndex = 0;
                trobats.Add(this.nomRadioButtonPosaTreu, radiu);
            }

            if (parms == null)
            {
                return trobats;// si no te paràmetres , aqui s'acaba
            }

            // s'hauria de fraccionar amb objectesdeCSV ?
            if (File.Exists(nomarxiu))
            {

                //if (input) { folderDesti = inputScriptsFolder; } else { folderDesti = outputScriptsFolder; };

                TextFieldParser parser = new TextFieldParser(nomarxiu);
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(separadorCSV);
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    try
                    {
                        if (parms.Contains(fields[0].ToLower()))
                        {
                            // no es poden fer dropdowns de valors csv que no s'especifiquen com a paràmetres
                            if (!trobats.ContainsKey(fields[0].ToLower()))
                            {
                                DropDownList dropd = new DropDownList();
                                dropd.ID = "dropd" + contador.ToString();
                                dropd.Items.Add(valorParmDisabled);
                                dropd.Items.Add(itemDeField(fields));
                                dropd.SelectedIndex = 0;
                                trobats.Add(fields[0].ToLower(), dropd);
                                contador++;
                            }
                            else
                            {
                                // ha de buscar el dropdown i afegir.li
                                object temp;
                                DropDownList dropd;
                                trobats.TryGetValue(fields[0].ToLower(), out temp);
                                dropd = (DropDownList)temp;
                                dropd.Items.Add(itemDeField(fields));
                            }
                        }
                    }
                    catch
                    {
                        logueja(InfoErrCSV + nomarxiu);
                    }



                }
                parser.Close();

            }
            // ara s'han de afegir tants texobox com parametres no tinguin dropdown
            //una vegada acabat cal afegir com a textbox els que no hagin aparegut
            foreach (var valorParm in parms)
            {
                if (!trobats.ContainsKey(valorParm))
                {
                    TextBox txt = new TextBox();
                    txt.ID = "text" + contador;
                    trobats.Add(valorParm, txt);
                    contador++;
                }
            }



            return trobats;
            //return trobats.Values.ToList<object>();
        }
        /// <summary>
        /// selecciona els valors 
        /// </summary>
        /// <param name="scriptASeleccionar">script amb objectes web</param>
        /// <param name="scriptAmbValors">script amb valors dels paràmetres</param>
        private void seleccionaValorsEnObjectes(ref script scriptASeleccionar, scriptSeriable scriptAmbValors)
        {
            string valorTemp;
            int contador = 0;
            RadioButtonList radioTemp;
            TextBox textTemp;
            DropDownList dropTemp;
            object ObjecteTemp = new object();
            // aqui tenim parmsloadsave ok, i parms ok, una list<string i una hashtable amb els valors que corresponen
            // to-do i si no te paràmetres?
            foreach (var cosa in scriptASeleccionar.parametresObjectes)
            {
                //to-do falla perque el addsub no apareix en valors
                try
                {
                    switch (cosa.Value.GetType().ToString())
                    {

                        case "System.Web.UI.WebControls.TextBox":
                            textTemp = (TextBox)cosa.Value;

                            textTemp.Text = scriptAmbValors.valorsParms[scriptAmbValors.nomsParms.IndexOf(cosa.Key)].ToString();

                            break;
                        case "System.Web.UI.WebControls.DropDownList":
                            dropTemp = (DropDownList)cosa.Value;
                            //to-do i si no s'hi troba?
                            dropTemp.SelectedIndex = dropTemp.Items.IndexOf(dropTemp.Items.FindByValue(scriptAmbValors.valorsParms[scriptAmbValors.nomsParms.IndexOf(cosa.Key)].ToString()));
                            break;
                        case "System.Web.UI.WebControls.RadioButtonList":
                            radioTemp = (RadioButtonList)cosa.Value;
                            if (scriptAmbValors.addSubstract)
                            {
                                radioTemp.SelectedIndex = 0;
                            }
                            else
                            {
                                radioTemp.SelectedIndex = 1;
                            }

                            break;
                    }
                }
                catch
                {
                    //
                    logueja(InfoErrParm + contador + " " + scriptASeleccionar.nom);
                }

                contador++;
            }

            //            scriptASeleccionar.parmsLoadSave = null;
        }
        /// <summary>
        /// carrega des de arxiu xml a llista seriable 
        /// </summary>
        /// <param name="objectOut">retorna by ref. l'objecte equivalent al xml</param>
        /// <param name="fileName">arxiu a llegir</param>
        /// <returns>true si tot ha anat be</returns>
        private bool carregaObjecteDesdeXML(ref fluxSeriable objectOut,string fileName )
        {
            if (string.IsNullOrEmpty(fileName)) {
                return false;
            }

            if (!File.Exists(fileName))
            {
                return false;
            }
            
            try
            {
                string attributeXml = string.Empty;

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                string xmlString = xmlDocument.OuterXml;

                using (StringReader read = new StringReader(xmlString))
                {
                    Type outType = typeof(fluxSeriable);

                    XmlSerializer serializer = new XmlSerializer(outType);
                    using (XmlReader reader = new XmlTextReader(read))
                    {
                        objectOut = (fluxSeriable)serializer.Deserialize(reader);
                        reader.Close();
                    }

                    read.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            
            
        }
        /// <summary>
        /// retorna objecte web amb scripts ps1 de la carpeta indicada
        /// </summary>
        /// <param name="folder">carpeta a mirar</param>
        /// <param name="llistaRetorna">by ref. la llista retornada</param>
        private void listItemDeScripts(string folder, ListItemCollection llistaRetorna)
        {
            llistaRetorna.Clear();

            string[] directori = Directory.GetFiles(folder);
            string nom;

            foreach (string arxiu in directori)
            {
                if (arxiu.ToUpper().Contains(".PS1"))
                {
                    nom = arxiu.Split('\\')[arxiu.Split('\\').Count() - 1];
                    llistaRetorna.Add(nom);

                }
            }

        }
        /// <summary>
        /// desa objecte seriable en arxiu indicat
        /// </summary>
        /// <param name="serializableObject">objecte a desar</param>
        /// <param name="fileName">arxiu on fer-ho</param>
        /// <returns>true si tot ha anat be</returns>
        /// <remarks>convé sigui XML</remarks>
        private bool desaObjecteEnXML(fluxSeriable serializableObject, string fileName)
        {


            if (serializableObject == null)
            {
                return false;
            }

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            //Use DataTable or HashTable, they are serializable.
            // si borres els objectes tipo web es pot serialitzar
            // serialize list<object>   <-- no es pot serialitzar
            // pots provar a serialitzar un enter= objectes.count per cada tipus
            // llavors recuperes cada tipus
            //llavors has d'anar serialitzant un per un, especificant tipus


            //borraObjectesDeLlistaActual();// <-- no , ja que et calen

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    //serializer.Serialize(stream, (TextBox)serializableObject.scriptsInput[0].objectsParms[4]);


                    serializer.Serialize(stream, serializableObject);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(fileName);
                    stream.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// detecta els paràmetres d'un arxiu ps1 usant la api de Microsoft
        /// </summary>
        /// <param name="arxiu">arxiu ps1 a analitzar</param>
        /// <param name="parms">by ref. llistat de paràmetres</param>
        /// <returns>true si tot ha anat be</returns>
        private bool llistaParmsDeArxiu(string arxiu, ref List<string> parms)
        {
            bool correcte = true;
            //List<string> copia;
            Token[] tokens = null;
            ParseError[] errors = null;
            //listBoxDesti = Parser.ParseFile();
            //ScriptBlockAst listBoxDesti;
            var retorn = Parser.ParseFile(arxiu, out tokens, out errors);

            if (errors.Length != 0)
            {
                foreach (var error in errors)
                {
                    logueja(error.ToString());
                }

                correcte = false;
            }

            try
            {
                if (retorn.ParamBlock != null)
                {
                    parms = retorn.ParamBlock.Parameters.Select(p => p.Name.ToString().ToLower().Substring(1)).ToList();
                }
                //parms = new List<string>(copia); 
            }
            catch
            {

                correcte = false;
            }
            //comprovar la estructura de dades que no la lii

            /*foreach(var parameter in listBoxDesti.ParamBlock.Parameters){
                parms.Add(parameter.ToString());
            }*/


            return correcte;
        }
        /// <summary>
        /// comprova si l'objecte flux és correcte per poder-hi operar
        /// </summary>
        /// <param name="llistaAComprovar">objecte flux de manipulació web</param>
        /// <returns>true si es vàlid</returns>
        private bool llistaCorrecte(flux llistaAComprovar)
        {
            RegexUtilities comprovador = new RegexUtilities();
            //to-do el primer escipt no pot ser treure
            
            try {
                // comprova que el primer script no resti
                object objectTemp;
                llistaAComprovar.scriptsInput[0].parametresObjectes.TryGetValue(this.nomRadioButtonPosaTreu,out objectTemp);
                RadioButtonList radiotemp = (RadioButtonList)objectTemp;
                if (radiotemp.SelectedIndex > 0) return false;
            }
            catch {
                // no te scripts d'entrada ?
                return false;
            }

            //to-do el nom ha de ser email o per output de script ?
            if ((comprovador.IsValidEmail(llistaAComprovar.targetMail)) && (llistaAComprovar.scriptsOutput.Count > 0) && (llistaAComprovar.scriptsInput.Count > 0))
            {
                foreach (var caracter in Path.GetInvalidFileNameChars())
                { // el nom posar pot ser un arxiu?
                    if (llistaAComprovar.targetMail.Contains(caracter)) return false;
                }
                return true;
            }
            
            return false;
            
            //to-do fer una verificació decent

            
        }
        /// <summary>
        /// escriuel output, en l'arxiu que pertoqui segons constants de configuració 
        /// </summary>
        /// <param name="texte">text a desar</param>
        /// <param name="arxiuScript">arxiu amb path del script</param>
        /// <returns>true si tot ha anat be</returns>
        private bool escriuOutDeScript(string texte, string arxiuScript)
        {
            bool retorn = false;
            string nomarxiu = arxiuScript.Substring(0, arxiuScript.Length - 4) + appendPerOUT;
            try
            {
                if (File.Exists(nomarxiu))
                {
                    File.Delete(nomarxiu);
                }
                File.WriteAllText(nomarxiu, texte);
                retorn = true;
            }
            catch
            {
            }
            return retorn;
        }
        /// <summary>
        /// retorna la cadena que representa els paràmetres que s'han escollit per executar un script
        /// </summary>
        /// <param name="scriptADonarParms">nom del script</param>
        /// <param name="input">entrada o sortida</param>
        /// <param name="useLastInput">si es vol usar d'entrada el últim output de script executat</param>
        /// <param name="correuDesti">opcional,si es de sortida cal el mail de desti</param>
        /// <returns>cadena de paràmetres tipus shell</returns>
        private string parametresDeScript(script scriptADonarParms,bool input,bool useLastInput,string correuDesti = null) {
            // dona els parametres del script prenent com a base la conversió a objecte seriable del objecte enviat
            int i;
            string retorn = "";
            scriptSeriable scriptTemp = scriptDesableDeScript(scriptADonarParms);
            
            if (scriptADonarParms.parametresObjectes != null)
            {

                for (i = 0; i < scriptTemp.nomsParms.Count; i++)
                {
                    if ((!scriptTemp.valorsParms[i].Contains("-") && !scriptTemp.valorsParms[i].Equals("")))
                    {
                        if (scriptTemp.valorsParms[i].Contains(" "))
                        {
                            retorn = retorn + " -" + scriptTemp.nomsParms[i] + " \'" + scriptTemp.valorsParms[i]+"\'";
                        }
                        else
                        {
                            retorn = retorn + " -" + scriptTemp.nomsParms[i] + " " + scriptTemp.valorsParms[i];
                        }
                        
                    }

                }
            }


            if ((!input) && useLastInput && (arxiuResultatUltimInput!= null))
            {
                retorn = retorn + " -" + parametresNecessarisScripsSortida[0] + " \'" + arxiuResultatUltimInput + "\'";
            }

            if (correuDesti != null && !input) {
                retorn = retorn + " -" + parametresNecessarisScripsSortida[1] + " \'" + correuDesti + "\'";
            }
            

            
            return retorn;
        }
        /// <summary>
        /// crea un item per a una llista segons array de cadenes
        /// </summary>
        /// <param name="liniaCSV">Array de cadenes</param>
        /// <returns>item de llista</returns>
        /// <remarks> per ús exclusiu de diccioParametres</remarks>
        private ListItem itemDeField(string[] liniaCSV)
        {
            ListItem nouItem;
            if (liniaCSV.Count() == 3)
            {
                nouItem = new ListItem(liniaCSV[2], liniaCSV[1]);
            }
            else if (liniaCSV.Count() == 2)
            {
                nouItem = new ListItem(liniaCSV[1], liniaCSV[1]);
            }
            else if (liniaCSV.Count() == 1)
            {
                nouItem = new ListItem(liniaCSV[0].ToString());
            }
            else
            {
                nouItem = new ListItem("-errorCSV-");
            }
            return nouItem;

        }

        #endregion

    }
    
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CanLlistes
{
    /// <summary>
    /// objecte seriable que representa un script d'entrada o sortida
    /// </summary>
    public class scriptSeriable
    {
        /// <summary>
        /// nom del script, convé que existeixi en la carpeta corresponent
        /// </summary>
        public string nom;
        /// <summary>
        /// noms dels paràmetres del script
        /// </summary>
        public List<string> nomsParms;
        /// <summary>
        /// valors corresponents als paràmetres de nom, ha de mantenir relació de 1 a 1, bijectiva
        /// </summary>
        public List<string> valorsParms;
        /// <summary>
        /// en el cas de representar un script de sortida, registra si el script disposa tots els paràmetres que conformen la interficie de sortida
        /// </summary>
        public Boolean interficieSortidaSencera = true; // de us en scripts sortida
        /// <summary>
        /// en el cas de correspondre a un script d'entrada, determina si s'ha de sumar o restar del flux
        /// </summary>
        public bool addSubstract =true;// de us en scripts entrada
        /// <summary>
        /// si és d'entrada o sortida
        /// </summary>
        public bool input;
    }
}
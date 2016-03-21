using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;

namespace CanLlistes
{
    /// <summary>
    /// objecte que representa un script d'entrada o sortida, amb els objectes de treball Web
    /// </summary>
    [Serializable]
    public class script
    {
        /// <summary>
        /// nom del script, convé que existeixi en la carpeta corresponent
        /// </summary>
        public string nom;
        /// <summary>
        /// llistat en forma de diccionari dels objectes de treball web que representen els paràmetres al respecte d'aquest script
        /// </summary>
        public Dictionary<string, object> parametresObjectes;
        /// <summary>
        /// en el cas de representar un script de sortida, registra si el script disposa tots els paràmetres que conformen la interficie de sortida
        /// </summary>
        public Boolean interficieSortidaSencera = true;
        /// <summary>
        /// si és d'entrada o sortida
        /// </summary>
        public bool input;
        
    }
}
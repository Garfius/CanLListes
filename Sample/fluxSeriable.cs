using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CanLlistes
{
    /// <summary>
    /// objecte que representa un flux en estat seriable
    /// </summary>
    public class fluxSeriable
    {
        /// <summary>
        /// nom de la adreça de distribució desitjada
        /// </summary>
        public string mailDesti = "";
        /// <summary>
        /// llista de scripts d'entrada
        /// </summary>
        public List<scriptSeriable> scriptsInput = new List<scriptSeriable>();
        /// <summary>
        /// llista de scripts de sortida
        /// </summary>
        public List<scriptSeriable> scriptsOutput = new List<scriptSeriable>();

    }
}
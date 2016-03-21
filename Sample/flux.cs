using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;

namespace CanLlistes
{
    /// <summary>
    /// objecte que representa un flux en estat de treball
    /// </summary>
    [Serializable]
    public class flux
    {
        /// <summary>
        /// la adreça de difució que es preté sigui creada
        /// </summary>
        public string targetMail = "";
        /// <summary>
        /// llista de scripts d'entrada
        /// </summary>
        public List<script> scriptsInput = new List<script>();
        /// <summary>
        /// llista de scripts de sortida
        /// </summary>
        public List<script> scriptsOutput = new List<script>();
        

    }
}
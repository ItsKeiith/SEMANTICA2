using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Semantica
{
    public class Error : Exception
    {
        public Error(string mensaje, StreamWriter log) : base(mensaje)
        {
            log.WriteLine("Error "+mensaje);
        }
        public Error(string mensaje) : base(mensaje)
        {
        }
        public Error(string mensaje, StreamWriter log, int linea) : base(mensaje + " en la linea "+linea)
        {
            log.WriteLine("Error "+mensaje+ " en la linea "+linea);
        }

    }
}
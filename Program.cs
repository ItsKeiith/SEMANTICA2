using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Semantica
{
    class Program
    {
        static void Main(string[] args) 
        {
            try
            {
                using (Lenguaje L = new Lenguaje())
                {
                    L.Programa();
                    /* 
                    while (!L.FinArchivo())
                    {
                        L.nextToken();
                    }
                    */
                    /*Variable v = new Variable("Radio", Variable.TipoDato.Float);
                    v.setValor(100);
                
                    int b=260;
                    Console.WriteLine(b%250);
                    Console.WriteLine((byte)b);
                    */
                }
            } 
            catch (Exception e)
            {
                Console.WriteLine("Error "+e.Message);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
/*
    Requerimento 1: Evalua el "else"
    Requeriminto 2: Incrementar la variable del for (incremento) al final de la ejecución
    Requeriminto 3: Hacer el Do
    Requeriminto 4: Hacer el While 
*/
namespace Semantica
{
    public class Lenguaje : Sintaxis
    {
        List<Variable> variables;
        Stack<float> s;
        public Lenguaje()
        {
            s = new Stack<float>();
            variables = new List<Variable>();
        }
        public Lenguaje(string nombre) : base(nombre)
        {
            s = new Stack<float>();
            variables = new List<Variable>();
        }
        //Programa  -> Librerias? Variables? Main
        public void Programa()
        {
            if (getContenido() == "#")
            {
                Librerias();
            }
            if (getClasificacion() == Tipos.tipoDatos)
            {
                Variables();
            }
            Main();
            ImprimeVariables();
        }
        private bool ExisteVariable(string nombre)
        {
            foreach (Variable v in variables)
            {
                if (v.getNombre() == nombre)
                {
                    return true;
                }
            }
            return false;
        }
        private float ValorVariable(string nombre)
        {
            foreach (Variable v in variables)
            {
                if (nombre == v.getNombre())
                {
                    return v.getValor();
                }
            }
            return 0;
        }
        private void ImprimeVariables()
        {
            log.WriteLine("Variables:");
            log.WriteLine("-------------");
            foreach (Variable v in variables)
            {
                log.WriteLine(v.getNombre() + " = " + v.getTipo() + " = " + v.getValor());
            }
        }
        private void ModificaValor(string nombre, float nuevoValor)
        {
            foreach (Variable v in variables)
            {
                if (v.getNombre() == nombre)
                {
                    if (v.getTipo() == Variable.TipoDato.Char)
                    {
                        if (EsCharValido(nuevoValor))
                        {
                            v.setValor(nuevoValor);
                        }
                        else
                        {
                            throw new Error("de semántica: la variable no es un char", log);
                        }
                    }
                    else if (v.getTipo() == Variable.TipoDato.Int)
                    {
                        if (EsIntValido(nuevoValor))
                        {
                            v.setValor(nuevoValor);
                        }
                        else
                        {
                            throw new Error("de semántica: la variable no es int", log);
                        }
                    }
                    else
                    {
                        v.setValor(nuevoValor);
                    }
                }
            }
        }

        // Método para validar si un valor es un char válido
        private static bool EsCharValido(float valor)
        {
            // Un char válido debe estar en el rango de 0 a 255
            return valor >= 0 && valor <= 255;
        }

        // Método para validar si un valor es un int válido
        private static bool EsIntValido(float valor)
        {
            return valor >= 0 && valor <= 65536;
        }

        //Librerias -> #include<identificador(.h)?> Librerias?
        private void Librerias()
        {
            match("#");
            match("include");
            match("<");
            match(Tipos.Identificador);
            if (getContenido() == ".")
            {
                match(".");
                match("h");
            }
            match(">");
            if (getContenido() == "#")
            {
                Librerias();
            }
        }
        //Variables -> tipoDato listaIdentificadores; Variables?
        private void Variables()
        {
            Variable.TipoDato tipo;
            switch (getContenido())
            {
                case "int":
                    tipo = Variable.TipoDato.Int;
                    break;
                case "float":
                    tipo = Variable.TipoDato.Float;
                    break;
                default:
                    tipo = Variable.TipoDato.Char;
                    break;
            }
            match(Tipos.tipoDatos);
            ListaIdentificadores(tipo);
            match(";");
            if (getClasificacion() == Tipos.tipoDatos)
            {
                Variables();
            }
        }
        //listaIdentificadores -> Identificador (,listaIdentificadores)?
        private void ListaIdentificadores(Variable.TipoDato tipo)
        {
            if (!ExisteVariable(getContenido()))
            {
                variables.Add(new Variable(getContenido(), tipo));
            }
            else
            {
                throw new Error("Error de sintaxis: la variable " + getContenido() + " ya existe.", log);
            }
            match(Tipos.Identificador); //Req 1
            if (getContenido() == ",")
            {
                match(",");
                ListaIdentificadores(tipo);
            }
        }
        //bloqueInstrucciones -> { listaIntrucciones? }
        private void BloqueInstrucciones(bool evalua)
        {
            match("{");
            if (getContenido() != "}")
            {
                ListaInstrucciones(evalua);
            }
            match("}");
        }
        //ListaInstrucciones -> Instruccion ListaInstrucciones?
        private void ListaInstrucciones(bool evalua)
        {
            Instruccion(evalua);
            if (getContenido() != "}")
            {
                ListaInstrucciones(evalua);
            }
        }
        //Instruccion -> Printf | Scanf | If | While | do while | For | Asignacion
        private void Instruccion(bool evalua)
        {
            if (getContenido() == "printf")
            {
                Printf(evalua);
            }
            else if (getContenido() == "scanf")
            {
                Scanf(evalua);
            }
            else if (getContenido() == "if")
            {
                If(evalua);
            }
            else if (getContenido() == "while")
            {
                While(evalua);
            }
            else if (getContenido() == "do")
            {
                Do(evalua);
            }
            else if (getContenido() == "for")
            {
                For(evalua);
            }
            else
            {
                Asignacion(evalua);
            }
        }
        //    Requerimiento 1: Printf -> printf(cadena(, Identificador)?);
        private void Printf(bool eval)
        {
            match("printf");
            match("(");

            string str = getContenido();

            str = str.Replace("\"", "");
            str = str.Replace("\\n", "\n");
            str = str.Replace("\\t", "\t");

            match(Tipos.Cadena);
            if (getContenido() == ",")
            {
                match(",");

                if (str.Contains("%f"))
                {
                    str = str.Replace("%f", ValorVariable(getContenido()).ToString());
                }

                string var = getContenido();

                if (!ExisteVariable(var))
                    throw new Error("de Sintaxis: No existe la variable " + var, log);
                match(Tipos.Identificador);
            }
            if (eval)
            {
                Console.Write(str);
            }

            match(")");
            match(";");

        }
        // Requerimiento 2: Scanf -> scanf(cadena,&Identificador);
        private void Scanf(bool evalua)
        {
            string nombre = getContenido();
            string? valorStr = "";
            float valor;
            match("scanf");
            match("(");
            match(Tipos.Cadena);
            match(",");
            match("&");
            match(Tipos.Identificador); //Req 1
            if (evalua)
            {
                valorStr = Console.ReadLine();
                if (!ExisteVariable(getContenido()))
                {
                    throw new Error("de sintaxis, la siguiente variable no está definida: " + getContenido(), log, linea);
                }
                if (float.TryParse(valorStr, out valor))
                {
                    ModificaValor(nombre, valor);
                }
                else
                {
                    throw new Error("lo introducido no es un numero", log, linea);
                }
            }
            match(")");
            match(";");
        }
        //Asignacion -> Identificador (++ | --) | (+= | -=) Expresion | (= Expresion) ;
        private void Asignacion(bool evalua)
        {
            string nombre = getContenido();
            float valor = ValorVariable(nombre);
            if (!ExisteVariable(nombre))
            {
                throw new Error("de sintaxis, la siguiente variable no está definida: " + nombre, log, linea);
            }
            match(Tipos.Identificador); //Req 1
            if (getClasificacion() == Tipos.IncrementoTermino)
            {
                string operador = getContenido();
                if (operador == "++")
                {
                    match("++");
                    valor++;
                }
                else if (operador == "--")
                {
                    match("--");
                    valor--;
                }
                else
                {
                    Expresion();
                    float valorExpresion = s.Pop();
                    switch (operador)
                    {
                        case "+=":
                        match(Tipos.IncrementoTermino);
                            valor += valorExpresion;
                            break;
                        case "-=":
                        match(Tipos.IncrementoTermino);
                            valor -= valorExpresion;
                            break;
                        case "*=":
                        match(Tipos.IncrementoFactor);
                            valor *= valorExpresion;
                            break;
                        case "/=":
                        match(Tipos.IncrementoFactor);
                            valor /= valorExpresion;
                            break;
                        case "%=":
                        match(Tipos.IncrementoFactor);
                            valor %= valorExpresion;
                            break;
                        default:
                            Expresion();
                            valor = s.Pop();
                            break;
                    }
                }
                if (evalua)
                {
                    ModificaValor(nombre, valor);
                }
            }
            else if (getContenido() == "=")
            {
                match("=");
                Expresion();
                valor = s.Pop();
                ModificaValor(nombre, valor);
            }
            else
            {
                throw new Error("Operador de asignación no reconocido: " + getContenido(), log);
            }
            ModificaValor(nombre, valor);
            match(";");

        }
        //If -> if (Condicion) instruccion | bloqueInstrucciones 
        //      (else instruccion | bloqueInstrucciones)?
        private void If(bool evaluaif)
        {
            match("if");
            match("(");
            bool evalua = Condicion() && evaluaif;
            match(")");
            match("{");
            if (evalua)
            {
                if (getContenido() == "{")
                {
                    BloqueInstrucciones(evalua);
                }
                else
                {
                    Instruccion(evalua);
                }
            }

            if (getContenido() == "else")
            {
                match("else");

                if (!evalua)
                {
                    if (getContenido() == "{")
                    {
                        BloqueInstrucciones(evaluaif);
                    }
                    else
                    {
                        Instruccion(evaluaif);
                    }
                }
                else
                {
                    if (getContenido() == "{")
                    {
                        BloqueInstrucciones(evaluaif);
                    }
                    else
                    {
                        Instruccion(evaluaif);
                    }
                }
            }
        }
        //Condicion -> Expresion operadoRelacional Expresion
        private bool Condicion()
        {
            string Operador = getContenido();
            Expresion();
            match(Tipos.OperadorRelacional);
            Expresion();
            float E2 = s.Pop();
            float E1 = s.Pop();
            switch (Operador)
            {
                case "<": return E1 < E2;
                case ">": return E1 > E2;
                case "<=": return E1 <= E2;
                case ">=": return E1 >= E2;
                case "==": return E1 == E2;
                default: return E1 != E2;
            }
        }
        //While -> while(Condicion) bloqueInstrucciones | Instruccion
        private void While(bool evalua)
        {
            match("while");
            match("(");
            int counttmp = ccount - 1;
            int lineatmp = linea;
            bool evaluaWhile = true;

            do
            {
                evaluaWhile = Condicion() && evalua;
                match(")");
                if (getContenido() == "{")
                {
                    BloqueInstrucciones(evaluaWhile);
                }
                else
                {
                    Instruccion(evaluaWhile);
                }
                if (evaluaWhile)
                {
                    ccount = counttmp;
                    linea = lineatmp;
                    archivo.DiscardBufferedData();
                    archivo.BaseStream.Seek(ccount, SeekOrigin.Begin);
                    nextToken();
                }
            } while (evaluaWhile);
        }
        //Do -> do bloqueInstrucciones | Intruccion while(Condicion);
        private void Do(bool evalua)
        {
            match("do");
            int lineatmp = linea;
            bool evaluaDo = true;
            do
            {
                int counttmp = ccount - 1;
                if (getContenido() == "{")
                {
                    BloqueInstrucciones(evalua && evaluaDo);
                }
                else
                {
                    Instruccion(evalua && evaluaDo);
                }

                match("while");
                match("(");
                evaluaDo = Condicion() && evalua;
                match(")");
                match(";");

                if (evalua && evaluaDo)
                {
                    ccount = counttmp;
                    linea = lineatmp;
                    archivo.DiscardBufferedData();
                    archivo.BaseStream.Seek(ccount, SeekOrigin.Begin);
                    nextToken();
                }
            } while (evalua && evaluaDo);
        }
        //For -> for(Asignacion Condicion; Incremento) BloqueInstruccones | Instruccion 
        private void For(bool evalua)
        {
            match("for");
            match("(");
            Asignacion(evalua);
            int counttmp = ccount - 1;
            int lineatmp = linea;
            bool evaluafor = true;
            string variable = getContenido();
            do
            {
                evaluafor = Condicion() && evalua;
                match(";");
                Incremento(evalua);
                match(")");
                if (getContenido() == "{")
                {
                    BloqueInstrucciones(evaluafor);
                }
                else
                {
                    Instruccion(evaluafor);
                }
                if (evaluafor)
                {
                    ModificaValor(variable, ValorVariable(variable) + 1); //Incremento
                    ccount = counttmp - variable.Length;
                    linea = lineatmp;
                    archivo.DiscardBufferedData();
                    archivo.BaseStream.Seek(ccount, SeekOrigin.Begin);
                    nextToken();
                }
            } while (evaluafor);
        }
        //Incremento -> Identificador ++ | --
        private int Incremento(bool evalua)
        {
            string nombre = getContenido();
            bool incremento = false;
            match(Tipos.Identificador);
            if (!ExisteVariable(nombre))
            {
                throw new Error("de Sintaxis : la variable " + nombre + " no existe", log, linea);
            }
            if (getClasificacion() == Tipos.IncrementoTermino)
            {
                if (getContenido() == "++")
                {
                    incremento = true;
                }
                match(Tipos.IncrementoTermino);
            }
            if (incremento)
                return 1;
            return -1;
        }
        //Main      -> void main() bloqueInstrucciones
        private void Main()
        {
            match("void");
            match("main");
            match("(");
            match(")");
            BloqueInstrucciones(true);
        }
        //Expresion -> Termino MasTermino
        private void Expresion()
        {
            Termino();
            MasTermino();
        }
        //MasTermino -> (OperadorTermino Termino)?
        private void MasTermino()
        {
            string operador = getContenido();
            if (getClasificacion() == Tipos.OperadorTermino)
            {
                match(Tipos.OperadorTermino);
                Termino();
                float N1 = s.Pop();
                float N2 = s.Pop();
                switch (operador)
                {
                    case "+": s.Push(N2 + N1); break;
                    case "-": s.Push(N2 - N1); break;
                }
            }
        }
        //Termino -> Factor PorFactor
        private void Termino()
        {
            Factor();
            PorFactor();
        }
        //PorFactor -> (OperadorFactor Factor)?
        private void PorFactor()
        {
            string operador = getContenido();
            if (getClasificacion() == Tipos.OperadorFactor)
            {
                match(Tipos.OperadorFactor);
                Factor();
                float N1 = s.Pop();
                float N2 = s.Pop();
                switch (operador)
                {
                    case "/": s.Push(N2 / N1); break;
                    case "*": s.Push(N2 * N1); break;
                    case "%": s.Push(N2 % N1); break;
                }
            }
        }
        //Factor -> numero | identificador | (Expresion)
        private void Factor()
        {
            if (getClasificacion() == Tipos.Numero)
            {
                s.Push(ValorVariable(getContenido()));
                match(Tipos.Numero);
            }
            else if (getClasificacion() == Tipos.Identificador)
            {
                if (!ExisteVariable(getContenido()))
                {
                    throw new Error("de sintaxis, la siguiente variable no esta definida: " + getContenido(), log, linea);
                }
                s.Push(ValorVariable(getContenido()));
                match(Tipos.Identificador); //Req 1
            }
            else
            {
                match("(");
                if (getClasificacion() == Tipos.tipoDatos)
                {
                    string tipo = getContenido();
                    match(Tipos.tipoDatos);
                    match(")");
                    Expresion();
                    float s_value = s.Pop();
                    switch (tipo)
                    {
                        case "char": s.Push(s_value % 256); break;
                        case "int": s.Push(s_value % 65536); break;
                    }
                }
                else
                {
                    Expresion();
                    match(")");
                }
            }
        }
    }
}
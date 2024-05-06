using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using static Semantica.Variable;
/*
    Requerimento 1: Colocar el tipo de dato en asm dependiendo del tipo de
    dato de la variable
    Requerimiento 2: crear las operaciones en asignacion
    Requerimiento 3: printf
    Requerimiento 4: scanf
    Requerimiento 5: do
    Requerimiento 6: while
*/
namespace Semantica
{
    public class Lenguaje : Sintaxis
    {
        List<Variable> variables;
        Stack<float> s;
        int countIF;
        int countWHILE;

        int countDO;
        public Lenguaje()
        {
            s = new Stack<float>();
            variables = new List<Variable>();
            countIF = countDO = countWHILE = 0;

        }
        public Lenguaje(string nombre) : base(nombre)
        {
            s = new Stack<float>();
            variables = new List<Variable>();
            countIF = countDO = countWHILE = 0;
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
            asm.WriteLine("org 100h");
            Main();
            asm.WriteLine("ret");
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
            asm.WriteLine("; Variables: ");
            log.WriteLine("-------------");
            foreach (Variable v in variables)
            {
                log.WriteLine(v.getNombre() + " = " + v.getTipo() + " = " + v.getValor());
                TipoDato tipo = v.getTipo();
                if (tipo == TipoDato.Int)
                {
                    asm.WriteLine(v.getNombre() + " db 0");
                }
                else if (tipo == TipoDato.Float)
                {
                    asm.WriteLine(v.getNombre() + " dq 0");
                }
                else if (tipo == TipoDato.Char)
                {
                    asm.WriteLine(v.getNombre() + " dd 0");
                }
                else
                {
                }
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
        private static bool EsCharValido(float valor)
        {
            return valor >= 0 && valor <= 255;
        }
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
            string nombre;
            string? valorStr = "";
            float valor;
            match("scanf");
            match("(");
            match(Tipos.Cadena);
            match(",");
            match("&");
            nombre = getContenido();
            match(Tipos.Identificador); //Req 1
            if (evalua)
            {
                valorStr = Console.ReadLine();
                if (!ExisteVariable(nombre))
                {
                    throw new Error("de sintaxis, la siguiente variable no está definida: " + nombre, log, linea);
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
            float valorExpresion = 0;
            if (!ExisteVariable(nombre))
            {
                throw new Error("de sintaxis, la siguiente variable no está definida: " + nombre, log, linea);
            }
            match(Tipos.Identificador); //Req 1
            if (getClasificacion() == Tipos.IncrementoTermino)
            {
                string operador = getContenido();
                switch (operador)
                {
                    case "++":
                        match("++");
                        asm.WriteLine("INC " + nombre);
                        valor++;
                        break;
                    case "--":
                        match("--");
                        asm.WriteLine("DEC " + nombre);
                        valor--;
                        break;
                    case "+=":
                        match(Tipos.IncrementoTermino);
                        if (getClasificacion() == Tipos.Numero)
                        {
                            valorExpresion = float.Parse(getContenido());
                            match(Tipos.Numero);
                        }
                        else if (getClasificacion() == Tipos.Identificador)
                        {
                            valorExpresion = ValorVariable(getContenido());
                            match(Tipos.Identificador);
                        }
                        else
                        {
                            Expresion();
                            asm.WriteLine("POP AX");
                            valorExpresion = s.Pop();
                        }
                        asm.WriteLine("ADD " + nombre + ", " + valorExpresion);
                        valor += valorExpresion;
                        break;
                    case "-=":
                        if (getClasificacion() == Tipos.Numero)
                        {
                            valorExpresion = float.Parse(getContenido());
                            match(Tipos.Numero);
                        }
                        else if (getClasificacion() == Tipos.Identificador)
                        {
                            valorExpresion = ValorVariable(getContenido());
                            match(Tipos.Identificador);
                        }
                        else
                        {
                            Expresion();
                            asm.WriteLine("POP AX");
                            valorExpresion = s.Pop();
                        }
                        asm.WriteLine("SUB " + nombre + ", " + valorExpresion);
                        valor -= valorExpresion;
                        break;
                }
            }

            else if (getClasificacion() == Tipos.IncrementoFactor)
            {
                string operador = getContenido();
                switch (operador)
                {
                    case "*=":
                        match(Tipos.IncrementoFactor);
                        if (getClasificacion() == Tipos.Numero)
                        {
                            valorExpresion = float.Parse(getContenido());
                            match(Tipos.Numero);
                        }
                        else if (getClasificacion() == Tipos.Identificador)
                        {
                            valorExpresion = ValorVariable(getContenido());
                            match(Tipos.Identificador);
                        }
                        else
                        {
                            Expresion();
                            asm.WriteLine("POP AX");
                            valorExpresion = s.Pop();
                        }
                        asm.WriteLine("IMUL " + nombre + ", " + valorExpresion);
                        valor *= valorExpresion;
                        break;
                    case "/=":
                        match(Tipos.IncrementoFactor);
                        if (getClasificacion() == Tipos.Numero)
                        {
                            valorExpresion = float.Parse(getContenido());
                            match(Tipos.Numero);
                        }
                        else if (getClasificacion() == Tipos.Identificador)
                        {
                            valorExpresion = ValorVariable(getContenido());
                            match(Tipos.Identificador);
                        }
                        else
                        {
                            Expresion();
                            asm.WriteLine("POP AX");
                            valorExpresion = s.Pop();
                        }
                        asm.WriteLine("IDIV " + nombre + ", " + valorExpresion);
                        valor /= valorExpresion;
                        break;

                    case "%=":
                        match(Tipos.IncrementoFactor);
                        if (getClasificacion() == Tipos.Numero)
                        {
                            valorExpresion = float.Parse(getContenido());
                            match(Tipos.Numero);
                        }
                        else if (getClasificacion() == Tipos.Identificador)
                        {
                            valorExpresion = ValorVariable(getContenido());
                            match(Tipos.Identificador);
                        }
                        else
                        {
                            Expresion();
                            asm.WriteLine("POP AX");
                            valorExpresion = s.Pop();
                        }
                        asm.WriteLine("MOV AX, " + nombre);
                        asm.WriteLine("MOV DX, 0");
                        asm.WriteLine("DIV " + valorExpresion);
                        asm.WriteLine("MOV " + nombre + ", DX");
                        valor %= valorExpresion;
                        break;
                }
            }
            else if (getContenido() == "=")
            {
                match("=");
                Expresion();
                asm.WriteLine("POP AX");
                asm.WriteLine("MOV " + nombre + ", AX");
                valor = s.Pop();
            }
            //asm.WriteLine("MOV " + nombre);
            if (evalua)
            {
                ModificaValor(nombre, valor);
            }
            match(";");

        }
        //If -> if (Condicion) instruccion | bloqueInstrucciones 
        //      (else instruccion | bloqueInstrucciones)?
        private void If(bool evaluaif)
        {
            match("if");
            match("(");
            string etiqueta = "etiquetaIF" + (++countIF);
            string etiquetaelse = "etiquetaelse" + countIF;
            bool evalua = Condicion(etiqueta) && evaluaif;
            match(")");
            match("{");


            if (getContenido() == "{")
            {
                BloqueInstrucciones(evalua);
            }
            else
            {
                Instruccion(evalua);
            }
            asm.WriteLine("JMP "+ etiquetaelse);
            asm.WriteLine(etiqueta + ":");
            if (getContenido() == "else")
            {
                match("else");
                if (getContenido() == "{")
                {
                    BloqueInstrucciones(evalua);
                }   
                else 
                {
                    Instruccion(!evalua);
                }

            }
            asm.WriteLine(etiquetaelse + ":");
            
        }
        //Condicion -> Expresion operadoRelacional Expresion
        private bool Condicion(string etiqueta)
        {
            string Operador = getContenido();
            Expresion();
            match(Tipos.OperadorRelacional);
            Expresion();
            asm.WriteLine("POP BX");
            float E2 = s.Pop();
            asm.WriteLine("POP AX");
            asm.WriteLine("CMP AX, BX");
            float E1 = s.Pop();
            switch (Operador)
            {
                case "<": asm.WriteLine("JGE " + etiqueta); return E1 < E2;
                case ">": asm.WriteLine("JLE " + etiqueta); return E1 > E2;
                case "<=": asm.WriteLine("JA " + etiqueta); return E1 <= E2;
                case ">=": asm.WriteLine("JG " + etiqueta); return E1 >= E2;
                case "==": asm.WriteLine("JNE " + etiqueta); return E1 == E2;
                default: asm.WriteLine("JE " + etiqueta); return E1 != E2;
            }
        }
        //While -> while(Condicion) bloqueInstrucciones | Instruccion
        private void While(bool evalua)
        {
            string etiquetawhile = "etiquetaWHILE" + (++countWHILE);
            match("while");
            asm.WriteLine(etiquetawhile + ":");
            match("(");
            int counttmp = ccount - 1;
            int lineatmp = linea;
            bool evaluaWhile = true;
            do
            {
                evaluaWhile = Condicion("") && evalua;
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
            asm.WriteLine("JE " + etiquetawhile);
        }
        //Do -> do bloqueInstrucciones | Intruccion while(Condicion);
        private void Do(bool evalua)
        {
            string etiqueta = "etiquetaDO" + (++countDO);
            asm.WriteLine(etiqueta + ":");
            match("do");
            if (getContenido() == "{")
            {
                BloqueInstrucciones(evalua);
            }
            else
            {
                Instruccion(evalua);
            }
            match("while");
            match("(");
            bool EvaluaDo = Condicion(etiqueta) && evalua;
            asm.WriteLine("CMP BX, 10");
            match(")");
            match(";");
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
                evaluafor = Condicion("") && evalua;
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
                float N2 = s.Pop();
                asm.WriteLine("POP BX");
                float N1 = s.Pop();
                asm.WriteLine("POP AX");
                switch (operador)
                {
                    case "+":
                        asm.WriteLine("ADD AX,BX");
                        s.Push(N1 + N2);
                        asm.WriteLine("POP AX");
                        break;
                    case "-":
                        asm.WriteLine("SUB AX,BX");
                        s.Push(N1 - N2);
                        asm.WriteLine("POP AX");
                        break;
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
                float N2 = s.Pop();
                asm.WriteLine("POP BX");
                float N1 = s.Pop();
                asm.WriteLine("POP AX");
                switch (operador)
                {
                    case "/":
                        asm.WriteLine("DIV BX");
                        asm.WriteLine("PUSH BX");
                        s.Push(N1 / N2); break;
                    case "*":

                        asm.WriteLine("MUL BX");
                        asm.WriteLine("PUSH AX");
                        s.Push(N1 * N2); break;
                    case "%":
                        asm.WriteLine("DIV BX");
                        asm.WriteLine("PUSH DX");
                        s.Push(N1 % N2); break;
                }
            }
        }
        //Factor -> numero | identificador | (Expresion)
        private void Factor()
        {
            if (getClasificacion() == Tipos.Numero)
            {
                asm.WriteLine("MOV AX, " + getContenido());
                asm.WriteLine("PUSH AX");
                s.Push(float.Parse(getContenido()));
                match(Tipos.Numero);
            }
            else if (getClasificacion() == Tipos.Identificador)
            {
                if (!ExisteVariable(getContenido()))
                {
                    throw new Error("de sintaxis, la siguiente variable no esta definida: " + getContenido(), log, linea);
                }
                asm.WriteLine("MOV AX, " + getContenido());
                asm.WriteLine("PUSH AX");
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
                    asm.WriteLine("POP AX");
                    switch (tipo)
                    {
                        case "char": s.Push(s_value % 256); break;
                        case "int": s.Push(s_value % 65536); break;
                    }
                    asm.WriteLine("MOV AX, " + s_value);
                    asm.WriteLine("PUSH AX");
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
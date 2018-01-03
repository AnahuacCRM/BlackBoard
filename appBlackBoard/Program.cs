using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using appBlackBoard.Clases;

namespace appBlackBoard
{
    class Program : VariablesGlobales
    {
        static void Main(string[] args)
        {
            //var a = new string[1];
            //a[0] = "-1";
            //args = a;
            try
            {

                utilerias.newLineBitacora("Inicializando App BlackBoard de armado de archivos, espere... ", true, true);
                if ((args != null && args.Length > 0))//Ejecución Manual.
                {
                    numeroEvento = utilerias.setConvienteAEntero(args[0].ToString());
                    if (numeroEvento == 0)
                    {
                        inicializaBitacora();
                        //utilerias.newLineConsole("Se ha recibido el parámetro" + args[0].ToString() + " que no se ha podido convertir a número entero.");
                        utilerias.newLineBitacora("Se ha recibido el parámetro" + args[0].ToString() + " que no se ha podido convertir a número entero.", true, true);
                        utilerias.getEscribeBitacora();
                        return;
                    }
                    else if (numeroEvento > 0)
                        ejecucionManual = true;
                    else
                        ejecucionManual = false;
                }
                else if (NumeroEventoAmarre != string.Empty && modoDebug)
                {
                    numeroEvento = utilerias.setConvienteAEntero(NumeroEventoAmarre);
                    if (numeroEvento == 0)
                    {
                        //utilerias.newLineConsole("El dato de amarre " + NumeroEventoAmarre + " no se ha podido convertir a numero entero.");
                        return;
                    }
                }
                //else
                //{
                //    inicializaBitacora();
                //    //utilerias.newLineConsole("No se ha podido establecer el número de evento a procesar.");
                //    utilerias.newLineBitacora("No se ha podido establecer el número de evento a procesar.", true, true);
                //    utilerias.getEscribeBitacora();
                //    return;
                //}
                ////Consultas consulta = new Consultas(); 
                if (numeroEvento > 0)
                {
                    origenEjecucion = "GWASNAP";
                    Metodos metodos = new Metodos();
                    inicializaBitacora();
                    utilerias.inicializaConnection();
                    utilerias.inicializaUsrPsw();
                    if (!utilerias.inicializaURL())
                    {
                        utilerias.getEscribeBitacora();
                        return;
                    }
                    metodos.generaArchivos();
                    utilerias.getEscribeBitacora();
                    utilerias.enviaCorreo(vsRutaBitacora);
                }
                else
                {
                    origenEjecucion = "MASIVO";
                    Metodos metodos = new Metodos();
                    inicializaBitacora();
                    utilerias.inicializaConnection();
                    utilerias.inicializaPeriodos();
                    utilerias.inicializaCampus();
                    utilerias.inicializaUsrPsw();
                    utilerias.inicializaDTReferencia();
                    //if (!utilerias.inicializaURL())
                    //{
                    //    utilerias.getEscribeBitacora();
                    //    return;
                    //}
                    metodos.generaArchivosMulticampus();
                    //metodos.generaArchivos();
                    utilerias.getEscribeBitacora();
                    utilerias.enviaCorreo(vsRutaBitacora);
                }

            }
            catch (Exception ex)
            {
                inicializaBitacora();
                //utilerias.newLineConsole(utilerias.controlErrores(ex, "Error en el proceso: ", "Fin del mensaje"));
                utilerias.newLineBitacora("Error en el proceso: " + utilerias.controlErrores(ex, "Error en el proceso: ", "Fin del mensaje"), true, true);
                utilerias.getEscribeBitacora();
            }
        }
    }
}

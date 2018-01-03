using System;
using System.Diagnostics;
using System.Web.Configuration;
using System.Web.Services;
using wsBlackBoard.Clases;

namespace wsBlackBoard
{
    /// <summary>
    /// Descripción breve de wsBlackBoardAmarre
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // Para permitir que se llame a este servicio web desde un script, usando ASP.NET AJAX, quite la marca de comentario de la línea siguiente. 
    // [System.Web.Script.Services.ScriptService]
    public class wsBlackBoardAmarre : System.Web.Services.WebService
    {
        public string vsMensaje { get; set; }
        [WebMethod]
        public string srvGeneraArchivosBlackBoard(int pnNumeroEvento)
        {
            Utilerias utilerias = new Utilerias();
            vsMensaje = "";
            string Ambiente = "";
            vsMensaje += "Inicio de la invocación: " + DateTime.Now.ToString() + (char)10;
            vsMensaje += "Parámetro recibido: " + pnNumeroEvento.ToString() + (char)10;
            try
            {
                if (string.IsNullOrWhiteSpace(Ambiente))
                {
                    string csRutaExe = WebConfigurationManager.AppSettings["csRutaExe"];
                    string csNombreExe = WebConfigurationManager.AppSettings["csNombreExe"];
                    if (pnNumeroEvento > 0)
                    {
                        if (verificarProceso(csNombreExe))
                        {
                            string file = csRutaExe;
                            vsMensaje += "Validación completada, se procede a ejecutar. " + DateTime.Now.ToString() + (char)10;
                            Process.Start(file, pnNumeroEvento.ToString());
                            utilerias.almacenaInvocacion(vsMensaje);
                            return "Recibí el siguiente dato: " + pnNumeroEvento.ToString();
                        }
                        else
                        {
                            vsMensaje += "Validación completada, NO se procede a ejecutar ya que se encontró un proceso en ejecución. " + DateTime.Now.ToString() + (char)10;
                            utilerias.almacenaInvocacion(vsMensaje);
                            return "Ya se está ejecutando un proceso. Reintente en unos minutos.";
                        }
                    }
                    else
                    {
                        csRutaExe = WebConfigurationManager.AppSettings["csRutaExeDiff"];
                        csNombreExe = WebConfigurationManager.AppSettings["csNombreExeDiff"];
                        if (verificarProceso(csNombreExe))
                        {
                            string file = csRutaExe;
                            vsMensaje += "Validación completada, se procede a ejecutar. " + DateTime.Now.ToString() + (char)10;
                            Process.Start(file, pnNumeroEvento.ToString());
                            utilerias.almacenaInvocacion(vsMensaje);
                            return "Recibí el siguiente dato: " + pnNumeroEvento.ToString();
                        }
                        else
                        {
                            vsMensaje += "Validación completada, NO se procede a ejecutar ya que se encontró un proceso en ejecución. " + DateTime.Now.ToString() + (char)10;
                            utilerias.almacenaInvocacion(vsMensaje);
                            return "Ya se está ejecutando un proceso. Reintente en unos minutos.";
                        }
                    }
                }
                else
                {
                    string csRutaExe = WebConfigurationManager.AppSettings["csRutaExeQA"];
                    string csNombreExe = WebConfigurationManager.AppSettings["csNombreExeQA"];
                    if (verificarProceso(csNombreExe))
                    {
                        string file = csRutaExe;
                        Process.Start(file, Ambiente + ":" + pnNumeroEvento.ToString());
                        return "Recibí el siguiente dato: " + pnNumeroEvento.ToString();
                    }
                    else
                        return "Ya se está ejecutando un proceso. Reintente en unos minutos.";
                }
            }
            catch (Exception ex)
            {
                vsMensaje += "Ocurrió un error al intentar ejecutar el EXE: " + (char)10;
                vsMensaje += ex.Message + (char)10;
                vsMensaje += ex.StackTrace + (char)10;
                if (ex.InnerException != null)
                    vsMensaje += ex.InnerException.Message;
                vsMensaje += "Fin del mensaje de error. " + DateTime.Now.ToString() + (char)10;
                utilerias.almacenaInvocacion(vsMensaje);
                return "Ocurrió un error al ejecutar el EXE.";
            }
        }
        private bool verificarProceso(string NombreExe)
        {
            vsMensaje += "Validando estatus del ejecutable..." + (char)10;
            Process[] procesos = Process.GetProcesses();
            // recorrer los procesos existentes
            foreach (Process proceso in procesos)
            {
                // verificar si existe el que buscamos
                if (proceso.ProcessName == NombreExe)
                    return false;
            } // end foreach
            return true;
        } // end verificarProceso
    }
}

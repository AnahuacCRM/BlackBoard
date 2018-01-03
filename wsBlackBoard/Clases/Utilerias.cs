using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace wsBlackBoard.Clases
{
    public class Utilerias
    {
        public void almacenaInvocacion(string mensaje)
        {
            mensaje += "_________________________________________________";
            StreamWriter log;

            if (File.Exists("C:\\LogBB\\log.txt"))
            {
                log = File.AppendText("C:\\LogBB\\log.txt");
            }
            else
                return;
            log.WriteLine(mensaje);
            log.Close();
        }
    }
}
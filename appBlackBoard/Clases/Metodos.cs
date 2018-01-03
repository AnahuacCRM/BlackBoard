using System;
using System.Data;
using System.IO;
using System.Text;

namespace appBlackBoard.Clases
{
    class Metodos : VariablesGlobales
    {
        public void generaArchivos()
        {
            try
            {
                utilerias.newLineBitacora("Iniciando la consulta a B.D.", true, true);
                Consultas consulta = new Consultas();
                DataTable dtResultados = new DataTable();
                dtResultados = consulta.getConsultaDatos();
                utilerias.newLineBitacora("Consulta terminada para el evento " + numeroEvento.ToString(), true, true);
                if (dtResultados.Columns.Contains("Error"))
                {
                    utilerias.newLineBitacora("La consulta ha generado el siguiente error: " + dtResultados.Rows[0]["Error"].ToString(), true, true);
                    utilerias.getEscribeBitacora();
                    return;
                }
                else if (dtResultados.Rows.Count <= 0)
                {
                    utilerias.newLineBitacora("La consulta no ha arrojado registros. Favor de validar la información.", true, true);
                    utilerias.getEscribeBitacora();
                    return;
                }
                else
                {
                    //Respalda(dtResultados);
                    generaCarpeta();
                    utilerias.newLineBitacora("Leyendo los datos de la consulta, espere...", true, true);
                    int countOrden = 1;
                    while (dtResultados.Rows.Count > 0)
                    {
                        nombreArchivo = archivoAProcesar(countOrden);
                        string vsTextoLineaDelete = "";
                        //Bitácora 1 Insert: Generar Layout
                        if (!string.IsNullOrWhiteSpace(nombreArchivo))
                            consulta.getReporteBitacora(1, "I", false, null, null);
                        DataRow[] Registros = null;
                        try//Try para controlar el error para reportarlo a BD en bitácora. Se da por hecho que si no puedo generar un archivo debo pasarme al siguiente paso o archivo.
                        {
                            if (string.IsNullOrWhiteSpace(nombreArchivo))
                            {
                                if (countOrden > dtURL.Rows.Count)
                                {
                                    utilerias.newLineBitacora("Se ha excedido el límite de interacciones con los nombres de los registros. Favor de validar.", true, true);
                                    if (dtResultados.Rows.Count > 0)
                                        utilerias.newLineBitacora("No se procesaron " + dtResultados.Rows.Count.ToString() + " registros para el número de evento " + numeroEvento.ToString() + ".", true, true);
                                    break;
                                }
                                utilerias.newLineBitacora("WARNING: No se encontró ningún nombre de archivo en la posición " + countOrden.ToString() + ".", true, true);
                                countOrden++;
                                continue;
                            }

                            Registros = dtResultados.Select("FILL = '" + nombreArchivo + "'");
                            if (Registros.Length <= 0)
                            {
                                utilerias.newLineBitacora("WARNING: No se encontró ningún registro en el cursor para el nombre de archivo " + nombreArchivo + ".", true, true);

                            }
                            else
                            {
                                int countLineas = 0;
                                string vsTextoLinea = "";

                                Console.Write("Escribiendo líneas en archivo, espere...");
                                foreach (DataRow drLinea in Registros)
                                {
                                    if (countLineas == 0)
                                    {
                                        generaArchivoLayout(nombreArchivo);
                                        if (nombreArchivo.Contains("ENROLLMENTSTUDENT"))
                                        {
                                            vsTextoLinea += drLinea["DETL"].ToString() + (char)10;
                                            vsTextoLineaDelete += drLinea["DETL"].ToString() + (char)10;
                                            countLineas++;
                                            continue;
                                        }
                                        else
                                        {
                                            vsTextoLinea += drLinea["DETL"].ToString() + (char)10;
                                            countLineas++;
                                            continue;
                                        }
                                    }

                                    //Validar si Enrollment contiene datos de eliminación de relaciones Cursos-Estudiantes.
                                    if (nombreArchivo.Contains("ENROLLMENTSTUDENT") && countLineas > 0)
                                    {
                                        //var a = drLinea["DETL"].ToString();
                                        if (drLinea["DETL"].ToString().Contains("Student|Y|ENROLLMENTSTUDENT"))
                                        {
                                            vsTextoLinea += drLinea["DETL"].ToString() + (char)10;
                                            countLineas++;
                                        }
                                        else
                                        {
                                            var status = drLinea["StaCur"].ToString().ToUpper();
                                            switch (status)
                                            {
                                                //(Borrado Físico)
                                                case "DD":
                                                case "DW":
                                                case "CC":
                                                case "DA":
                                                    vsTextoLineaDelete += drLinea["DETL"].ToString() + (char)10;
                                                    countLineas++;
                                                    break;

                                                //(Inhabilitación del registro)
                                                case "B1":
                                                case "B2":
                                                case "BC":
                                                case "BE":
                                                case "BR":
                                                case "B3":
                                                case "B6":
                                                    vsTextoLinea += drLinea["DETL"].ToString() + (char)10;
                                                    countLineas++;
                                                    break;

                                                //(Casos no especificados)
                                                default:
                                                    break;
                                            }
                                            ////DD y DW (Borrado Físico)
                                            //if (contieneEstatus && (drLinea["StaCur"].ToString().Contains("DD")|| drLinea["StaCur"].ToString().Contains("DW")))
                                            //{
                                            //    vsTextoLineaDelete += drLinea["DETL"].ToString() + (char)10;
                                            //    countLineas++;
                                            //}
                                            ////B1 y B2 (Inhabilitación del registro).

                                            //else if (contieneEstatus && (drLinea["StaCur"].ToString().Contains("B1")|| drLinea["StaCur"].ToString().Contains("B2")))
                                            //{
                                            //    vsTextoLinea += drLinea["DETL"].ToString() + (char)10;
                                            //    countLineas++;
                                            //}                                            
                                        }
                                        continue;
                                    }
                                    else if (!nombreArchivo.Contains("ENROLLMENTSTUDENT") && countLineas > 0)
                                    {
                                        vsTextoLinea += drLinea["DETL"].ToString() + (char)10;
                                        countLineas++;
                                    }
                                }
                                grabaLayout(vsTextoLinea);
                                utilerias.newLineBitacora("Número de líneas procesadas para el archivo " + nombreArchivo + ": " + countLineas.ToString(), false, true);
                                countLineasGlobal = countLineas;
                            }
                        }
                        catch (Exception ex)
                        {
                            string vsError = "Error al generar el layout: " + utilerias.controlErrores(ex, "Inicio de traza.", "Fin de Traza");
                            utilerias.newLineBitacora(vsError, true, true);
                            //Bitácora 1 Update: Error al generar Layout.
                            consulta.getReporteBitacora(1, "U", true, vsError, null);
                            //Bitácora 2 Update: Error al generar Layout.
                            consulta.getReporteBitacora(2, "I", true, "Cierre del proceso ya que mandó error en la generación del archivo y no hay archivo que cargar.", null);
                            //Bitácora 3 Update: Error al generar Layout.
                            consulta.getReporteBitacora(3, "I", true, "Cierre del proceso ya que mandó error en la generación del archivo y no hay archivo que monitorear.", null);
                            //Quito los registros de drRegistros
                            foreach (DataRow dr in dtResultados.Rows)
                            {
                                if (dr["FILL"].ToString() == nombreArchivo)
                                    dr.Delete();
                            }
                            dtResultados.AcceptChanges();
                            continue;
                        }
                        //Bitácora 1 Update: Terminé de Generar Layout de manera correcta
                        consulta.getReporteBitacora(1, "U", true, null, null);
                        if (Registros.Length > 0)
                        {
                            foreach (DataRow dr in dtResultados.Rows)
                            {
                                if (dr["FILL"].ToString() == nombreArchivo)
                                    dr.Delete();
                            }
                            dtResultados.AcceptChanges();
                            EnviaDatosBB enviaDatos = new EnviaDatosBB();
                            enviaDatos.enviaArchivo(vsRutaArchivoLayouts, nombreArchivo);
                            //validaReferenciasIndefinidas();
                        }
                        utilerias.newLineBitacora(vsSeparador, false, false);
                        utilerias.getEscribeBitacora();
                        //countOrden++;
                        utilerias.getEscribeBitacora();
                        //Console.Clear();
                        if (nombreArchivo.Contains("ENROLLMENTSTUDENT"))
                        {
                            //Debo procesar el archivo de delete (Y nonitorearlo)
                            utilerias.newLineBitacora("Procesando archivo de baja de Enrollment...", true, true);
                            nombreArchivo = "BAJA" + nombreArchivo;
                            generaArchivoLayout(nombreArchivo);
                            //vsRutaArchivoLayouts = vsRutaArchivoLayouts.Replace("ENROLLMENTSTUDENT", "BAJAENROLLMENTSTUDENT");
                            //Bitácora 1 Insert: Generar Layout
                            if (!string.IsNullOrWhiteSpace(nombreArchivo))
                                consulta.getReporteBitacora(1, "I", false, null, null);
                            grabaLayout(vsTextoLineaDelete);
                            //Bitácora 1 Update: Terminé de Generar Layout de manera correcta
                            consulta.getReporteBitacora(1, "U", true, null, null);
                            EnviaDatosBB enviaDatos = new EnviaDatosBB();
                            enviaDatos.enviaArchivo(vsRutaArchivoLayouts, nombreArchivo);
                            utilerias.newLineBitacora(vsSeparador, false, false);
                            utilerias.getEscribeBitacora();
                            //countOrden++;
                            utilerias.getEscribeBitacora();
                        }
                        countOrden++;
                    }
                }
            }
            catch (Exception ex)
            {
                utilerias.newLineBitacora("Error en el método principal: " + utilerias.controlErrores(ex, "Error en el proceso: ", "Fin del mensaje"), true, true);
                utilerias.getEscribeBitacora();
            }
        }
        private void grabaLayout(string vsTextoLinea)
        {
            try
            {
                StreamWriter log;
                if (File.Exists(vsRutaArchivoLayouts))
                {
                    log = File.AppendText(vsRutaArchivoLayouts);
                }
                else
                    return;
                log.WriteLine(vsTextoLinea);
                log.Close();
            }
            catch (Exception ex)
            {
                utilerias.controlErrores(ex, null, null);
            }
        }
        private void generaArchivoLayout(string nombreArchivo)
        {
            string pathArchivo = Path.Combine(vsDirectorioLayouts, nombreArchivo);
            using (FileStream fs = File.Create(pathArchivo))
            {
                var byteBitacora = Encoding.Unicode.GetBytes(string.Empty);
                foreach (byte b in byteBitacora)
                {
                    fs.WriteByte(b);
                }
            }
            vsRutaArchivoLayouts = pathArchivo;
            utilerias.newLineBitacora("Archivo " + nombreArchivo + " creado.", true, true);
        }
        private void generaCarpeta()
        {
            try
            {
                string pathEjecuta = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace(@"file:\", "");
                string pathBitacora = Path.Combine(pathEjecuta, numeroEvento.ToString());
                if (!Directory.Exists(pathBitacora))
                    Directory.CreateDirectory(pathBitacora);
                vsDirectorioLayouts = pathBitacora;
                utilerias.newLineBitacora("Ruta de la carpeta de layouts creada", true, true);
            }
            catch (Exception ex)
            {
                utilerias.newLineBitacora("Error en el proceso: " + utilerias.controlErrores(ex, "Error en el Método de Generar Carpeta: ", "Fin del mensaje"), true, true);
                utilerias.getEscribeBitacora();
            }
        }
        private string archivoAProcesar(int countOrden)
        {
            DataRow[] Registros;
            Registros = dtURL.Select("ORDN = " + countOrden.ToString());
            foreach (DataRow drArchivo in Registros)
            {
                return drArchivo["FILL"].ToString();
            }
            return "";
        }
        public void generaArchivosMulticampus()
        {
            if (dtPeriodos.Rows.Count <= 0)
            {
                utilerias.newLineBitacora("Error Fatal: No se inicializaron los periodos a procesar y el proceso no puede continuar.", true, true);
                return;
            }
            else
            {

                foreach (DataRow drPeriodo in dtPeriodos.Rows)
                {
                    vsgPeriodo = drPeriodo["CODE"].ToString();
                    utilerias.newLineBitacora("Periodo a procesar: " + vsgPeriodo + ".", true, true);
                    utilerias.newLineBitacora("Generando la sabana correspondiente al periodo mencionado, espere...", true, true);
                    if (utilerias.inicializaEventos(vsgPeriodo))
                    {

                        foreach (DataRow drEvento in dtEventos.Rows)
                        {
                            numeroEvento = utilerias.setConvienteAEntero(drEvento["EVEN"].ToString());
                            if (!utilerias.inicializaURL())
                            {
                                utilerias.getEscribeBitacora();
                                continue; ;
                            }
                            generaArchivos();
                        }
                    }
                    else
                    {
                        utilerias.newLineBitacora("Sin los eventos establecidos el proceso no puede continuar.", true, true);
                    }
                }
                if (dtReferenciasIndefinidas.Rows.Count > 0)
                    validaReferenciasIndefinidas();
            }
        }
        public void Respalda(DataTable dtResultado)
        {
            string vsLinea = "";
            utilerias.newLineBitacora("Respaldando tabla...", true, true);
            foreach (DataRow dr in dtResultado.Rows)
            {
                vsLinea += dr[0].ToString() + ";" + dr[1].ToString() + (char)10;
            }
            try
            {

                string nameBitacora = "";
                string pathArchivo = "";
                string pathEjecuta = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace(@"file:\", "");
                string pathBitacora = Path.Combine(pathEjecuta, "Bitacoras");
                nameBitacora = "LayoutRespaldo_" + numeroEvento.ToString() + ".csv";
                if (!Directory.Exists(pathBitacora))
                    Directory.CreateDirectory(pathBitacora);
                pathArchivo = Path.Combine(pathBitacora, nameBitacora);
                using (FileStream fs = File.Create(pathArchivo))
                {
                    var byteBitacora = Encoding.Unicode.GetBytes(vsLinea);
                    foreach (byte b in byteBitacora)
                    {
                        fs.WriteByte(b);
                    }
                }
            }
            catch (Exception ex)
            {
                utilerias.controlErrores(ex, "", "");
            }
        }

        public void validaReferenciasIndefinidas()
        {
            utilerias.newLineBitacora("Iniciando validaciones para los archivos marcados como indefinidos.", true, true);
            foreach (DataRow dr in dtReferenciasIndefinidas.Rows)
            {
                //Seteando Datos...
                vsIdArchivoCargado = dr["Referencia"].ToString();
                vsgPeriodo = dr["Periodo"].ToString();
                nombreArchivo = dr["Archivo"].ToString();
                numeroEvento = utilerias.setConvienteAEntero(dr["Evento"].ToString());
                EnviaDatosBB enviaDatosBB = new EnviaDatosBB();
                enviaDatosBB.rastreoArchivoIndefinido();
            }
        }
    }
}

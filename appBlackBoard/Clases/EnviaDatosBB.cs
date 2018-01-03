using System;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace appBlackBoard.Clases
{
    class EnviaDatosBB : VariablesGlobales
    {
        Consultas consulta = new Consultas();
        public void enviaArchivo(string pathArchivo, string fileName)
        {
            string respuesta = "";
            //bool errorCargaArchivo = /*false*/;
            try
            {
                if (dtURL.Rows.Count <= 0)
                    return;
                else
                {
                    //Bitácora 2 Insert: Iniciar la carga del layout a BB.
                    consulta.getReporteBitacora(2, "I", false, null, null);
                    string url = "";
                    DataRow[] Registros;
                    if (!fileName.Contains("BAJA"))
                    {
                        Registros = dtURL.Select("FILL = '" + fileName + "'");
                        foreach (DataRow drLinea in Registros)
                        {
                            url = drLinea["URLL"].ToString();
                        }
                    }
                    else
                    {
                        Registros = dtURL.Select("FILL = '" + (fileName.Replace("BAJA", "")) + "'");
                        foreach (DataRow drLinea in Registros)
                        {
                            url = drLinea["URLL"].ToString();
                        }
                        url = url.Replace("/store", "/delete");
                    }

                    HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url); //sVal is id for the webService
                    wr.ContentType = "Content-Type:text/plain";
                    wr.Method = "POST";
                    wr.KeepAlive = true;
                    wr.Credentials = CredentialCache.DefaultCredentials;

                    string sAuthorization = vsUsrPsw;//AUTHENTIFICATION BEGIN
                    byte[] toEncodeAsBytes = Encoding.ASCII.GetBytes(sAuthorization);
                    string returnValue = Convert.ToBase64String(toEncodeAsBytes);
                    wr.Headers.Add("Authorization: Basic " + returnValue); //AUTHENTIFICATION END
                    Stream rs = wr.GetRequestStream();
                    byte[] formitembytes = Encoding.UTF8.GetBytes(pathArchivo);
                    string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
                    string header = string.Format(headerTemplate, "file", fileName, wr.ContentType);
                    byte[] headerbytes = Encoding.UTF8.GetBytes(header);

                    FileStream fileStream = new FileStream(pathArchivo, FileMode.Open, FileAccess.Read);
                    byte[] buffer = new byte[4096];
                    int bytesRead = 0;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        rs.Write(buffer, 0, bytesRead);
                    }
                    fileStream.Close();
                    rs.Close();
                    rs = null;

                    WebResponse wresp = null;
                    try
                    {
                        utilerias.newLineBitacora("Cargando archivo " + nombreArchivo + " en BlackBoard, espere...", true, true);
                        wresp = wr.GetResponse();
                        Stream stream2 = wresp.GetResponseStream();
                        StreamReader reader2 = new StreamReader(stream2);
                        respuesta = reader2.ReadToEnd();
                        if (countLineasGlobal <= 1)
                            Thread.Sleep(1000);
                        else
                            Thread.Sleep(3000);
                        utilerias.newLineBitacora("Se cargó correctamente el archivo " + nombreArchivo + " en BlackBoard. Iniciando monitoreo...", true, true);
                        obtieneIdArchivoCargado(respuesta);
                        if (!nombreArchivo.Contains("BAJA"))
                            indiceProcesoOrden++;
                        //Bitácora 2 Update: Terminé la carga del layout a BB sin error
                        consulta.getReporteBitacora(2, "U", true, null, null);
                    }
                    catch (Exception ex)
                    {
                        string vsError = utilerias.controlErrores(ex, "Error en el proceso de carga de archivos [Invocación].", "Fin de la traza.");
                        utilerias.newLineBitacora(vsError, true, true);
                        //Bitácora 2 Update: Terminé la carga del layout a BB sin error
                        consulta.getReporteBitacora(2, "U", true, vsError, null);
                        //OJO: Si no cargué el archivo, no hay nada que monitorear. Debo "cerrar" el proceso 3.
                        consulta.getReporteBitacora(3, "I", true, "Cierre del proceso 3 debido a que la carga del archivo generó error.", null);
                        return;
                    }
                    finally
                    {
                        if (wresp != null)
                        {
                            wresp.Close();
                            wresp = null;
                        }
                        wr = null;
                    }
                }
            }
            catch (Exception ex)
            {
                string vsError = utilerias.controlErrores(ex, "Error en el proceso de carga de archivos [Armado CURL].", "Fin de la traza.");
                utilerias.newLineBitacora(vsError, true, true);
                //Bitácora 2 Update: Terminé la carga del layout a BB sin error
                consulta.getReporteBitacora(2, "U", true, vsError, null);
                //OJO: Si no cargué el archivo, no hay nada que monitorear. Debo cerrar el proceso 3.
                consulta.getReporteBitacora(3, "I", true, "Cierre del proceso 3 debido a que la carga del archivo generó error.", null);
                return;
            }

            //Inicio de rastreo
            rastrearArchivo();
        }
        public void obtieneIdArchivoCargado(string response)
        {
            int lengthResponse = response.Length;


            if (response.Contains("referencia "))
            {
                int ubSeparador = response.IndexOf("referencia ");
                dtURL.Rows[indiceProcesoOrden]["idArchivoCargado"] = response.Substring(ubSeparador + 11, 32);
                vsIdArchivoCargado = response.Substring(ubSeparador + 11, 32);
            }
            else if (response.Contains("code "))
            {
                int ubSeparador = response.IndexOf("code ");
                dtURL.Rows[indiceProcesoOrden]["idArchivoCargado"] = response.Substring(ubSeparador + 5, 32);
                vsIdArchivoCargado = response.Substring(ubSeparador + 5, 32);
            }
            else
                return;
        }
        public void rastrearArchivos()
        {
            utilerias.newLineBitacora("Iniciando el rastreo del archivo cargado...", true, true);
            bool cargaCompletada = false;
            int count = 1;


            foreach (DataRow dr in dtURL.Rows)
            {
                cargaCompletada = false;
                while (!cargaCompletada)
                {
                    string vsIdArchivoCargado = dr["idArchivoCargado"].ToString();
                    if (!string.IsNullOrWhiteSpace(vsIdArchivoCargado))
                    {
                        utilerias.newLineBitacora("La referencia a monitorear es: " + vsIdArchivoCargado + ".", true, true);
                        HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(@"https://anahuac-test.blackboard.com/webapps/bb-data-integration-flatfile-BBLEARN/endpoint/dataSetStatus/" + vsIdArchivoCargado); //sVal is id for the webService
                        wr.ContentType = "Content-Type:text/plain";
                        wr.Method = "POST";
                        wr.KeepAlive = true;
                        wr.Credentials = CredentialCache.DefaultCredentials;

                        string sAuthorization = vsUsrPsw;//AUTHENTIFICATION BEGIN
                        byte[] toEncodeAsBytes = Encoding.ASCII.GetBytes(sAuthorization);
                        string returnValue = Convert.ToBase64String(toEncodeAsBytes);
                        wr.Headers.Add("Authorization: Basic " + returnValue); //AUTHENTIFICATION END
                        Stream rs = wr.GetRequestStream();
                        DataSetStatus status = new DataSetStatus();
                        WebResponse wresp = null;
                        try
                        {
                            //Get the response
                            while (!cargaCompletada)
                            {
                                wresp = wr.GetResponse();
                                Stream stream2 = wresp.GetResponseStream();
                                StreamReader reader2 = new StreamReader(stream2);
                                string respuesta = reader2.ReadToEnd();
                                XmlSerializer serializer = new XmlSerializer(typeof(DataSetStatus));
                                using (TextReader reader = new StringReader(respuesta))
                                {
                                    status = (DataSetStatus)serializer.Deserialize(reader);
                                }
                                if (status.QueuedCount > 0)
                                {
                                    utilerias.newLineBitacora("Registros procesados: " + status.CompletedCount.ToString(), false, true);
                                    //utilerias.newLineConsole("La validación " + count.ToString() + " fue indicada que aún hay registros pendientes de procesar. Se procede a esperar y a revalidar en 10 segundos.");
                                    utilerias.newLineBitacora("La validación " + count.ToString() + " fue indicada que aún hay registros pendientes de procesar. Se procede a esperar y a revalidar en 10 segundos.", true, true);
                                    Thread.Sleep(13000);
                                    count++;
                                }
                                else
                                {
                                    //utilerias.newLineConsole("Validación contemplada, continuando con el proceso...");
                                    utilerias.newLineBitacora("BlackBoard indicó que los registros han sido procesados con éxito. Estas son sus estadísticas:", true, true);
                                    utilerias.newLineBitacora("Registros procesados total: " + status.CompletedCount.ToString(), false, false);
                                    utilerias.newLineBitacora("Registros procesados correctamente: " + status.CompletedCount.ToString(), false, false);
                                    utilerias.newLineBitacora("Registros procesados con error: " + status.ErrorCount.ToString(), false, false);
                                    cargaCompletada = true;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            utilerias.newLineBitacora(utilerias.controlErrores(ex, "Error en el proceso de carga de archivos.", "Fin de la traza."), true, true);
                            Thread.Sleep(10000);
                        }
                        finally
                        {
                            if (wresp != null)
                            {
                                wresp.Close();
                                wresp = null;
                            }
                            wr = null;
                        }

                    }
                    else
                        return;
                }

            }
        }
        public void rastrearArchivo()
        {
            int countError500 = 0;
            try
            {
                //Bitácora 3 Insert: Monitoreo de la carga de archivo.
                consulta.getReporteBitacora(3, "I", false, null, null);
                utilerias.newLineBitacora("Iniciando el rastreo del archivo cargado...", true, true);
                bool cargaCompletada = false;
                int count = 1;


                if (!string.IsNullOrWhiteSpace(vsIdArchivoCargado))
                {

                    utilerias.newLineBitacora("Se encontró el código de referencia " + vsIdArchivoCargado + " para rastrear la carga del archivo " + vsIdArchivoCargado + ".", true, true);
                    while (!cargaCompletada)
                    {
                        HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(@"https://anahuac-test.blackboard.com/webapps/bb-data-integration-flatfile-BBLEARN/endpoint/dataSetStatus/" + vsIdArchivoCargado);
                        wr.ContentType = "Content-Type:text/plain";
                        wr.Method = "POST";
                        wr.KeepAlive = true;
                        wr.Credentials = CredentialCache.DefaultCredentials;

                        string sAuthorization = vsUsrPsw;//AUTHENTIFICATION BEGIN
                        byte[] toEncodeAsBytes = Encoding.ASCII.GetBytes(sAuthorization);
                        string returnValue = Convert.ToBase64String(toEncodeAsBytes);
                        wr.Headers.Add("Authorization: Basic " + returnValue); //AUTHENTIFICATION END
                        Stream rs = wr.GetRequestStream();
                        DataSetStatus status = new DataSetStatus();
                        WebResponse wresp = null;

                        //Get the response

                        try
                        {
                            wresp = wr.GetResponse();
                            Stream stream2 = wresp.GetResponseStream();
                            StreamReader reader2 = new StreamReader(stream2);
                            string respuesta = reader2.ReadToEnd();
                            XmlSerializer serializer = new XmlSerializer(typeof(DataSetStatus));
                            using (TextReader reader = new StringReader(respuesta))
                            {
                                status = (DataSetStatus)serializer.Deserialize(reader);
                            }
                            if (status.QueuedCount > 0)
                            {
                                utilerias.newLineBitacora("Registros procesados: " + status.CompletedCount.ToString(), false, true);
                                utilerias.newLineBitacora("La validación " + count.ToString() + " fue indicada que aún hay registros pendientes de procesar. Se procede a esperar y a revalidar en 10 segundos.", true, true);
                                Thread.Sleep(9000);
                                count++;
                                countError500 = 0;
                            }
                            else
                            {
                                utilerias.newLineBitacora("BlackBoard indicó que los registros han sido procesados con éxito. Estas son sus estadísticas:", true, true);
                                utilerias.newLineBitacora("Registros procesados total: " + status.CompletedCount.ToString(), false, false);
                                utilerias.newLineBitacora("Registros procesados correctamente: " + status.CompletedCount.ToString(), false, false);
                                utilerias.newLineBitacora("Registros procesados con error: " + status.ErrorCount.ToString(), false, false);
                                cargaCompletada = true;
                                //Bitácora 3 Update: Terminé el monitoreo del layout a BB sin error
                                if (status.ErrorCount <= 0)
                                    consulta.getReporteBitacora(3, "U", true, null, null);
                                else
                                    consulta.getReporteBitacora(3, "U", true, null, vsIdArchivoCargado);
                            }

                        }
                        catch (Exception ex)
                        {
                            countError500++;
                            utilerias.newLineBitacora(utilerias.controlErrores(ex, "Error número " + countError500.ToString() + " en el proceso de carga de archivos. Pausando 10 segundos...", "Fin de la traza."), true, true);
                            if (countError500 >= 5)
                            {
                                string vsError = utilerias.controlErrores(ex, "Se ha llegado al límite de invocaciones permitidas para el monitoreo. Se reportará a B.D. como Indefinido.", "Fin de la traza.");
                                utilerias.newLineBitacora(vsError, true, true);
                                //Bitácora 3 Update: Terminé la carga del layout a BB sin error
                                consulta.getReporteBitacora(3, "U", true, "Indefinido", null);
                                //Ajuste: Guardar las referencias y datos necesarios para que pueda monitorearlos despues.

                                DataRow drReferencia;
                                drReferencia = dtReferenciasIndefinidas.NewRow();
                                drReferencia["Referencia"] = vsIdArchivoCargado;
                                drReferencia["Periodo"] = vsgPeriodo;
                                drReferencia["Archivo"] = nombreArchivo;
                                drReferencia["Evento"] = numeroEvento;
                                dtReferenciasIndefinidas.Rows.Add(drReferencia);

                                return;
                            }
                            Thread.Sleep(15000);
                        }

                        finally
                        {
                            if (wresp != null)
                            {
                                wresp.Close();
                                wresp = null;
                            }
                            wr = null;
                        }
                    }
                }
                else
                    return;

            }
            catch (Exception ex)
            {
                utilerias.newLineBitacora(utilerias.controlErrores(ex, "Error en el proceso de carga de archivos.", "Fin de la traza."), true, true);
                string vsError = utilerias.controlErrores(ex, "Error en el proceso de carga de archivos [Preparación URL Monitoreo].", "Fin de la traza.");
                utilerias.newLineBitacora(vsError, true, true);
                //Bitácora 2 Update: Terminé la carga del layout a BB sin error
                consulta.getReporteBitacora(3, "U", true, vsError, null);
            }
        }

        public void rastreoArchivoIndefinido()
        {
            utilerias.newLineBitacora("Iniciando el rastreo del archivo marcado previamente como indefinido: " + nombreArchivo, true, true);
            utilerias.newLineBitacora("La referencia a monitorear es: " + vsIdArchivoCargado + ".", true, true);
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(@"https://anahuac-test.blackboard.com/webapps/bb-data-integration-flatfile-BBLEARN/endpoint/dataSetStatus/" + vsIdArchivoCargado); //sVal is id for the webService
            wr.ContentType = "Content-Type:text/plain";
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = CredentialCache.DefaultCredentials;

            string sAuthorization = vsUsrPsw;//AUTHENTIFICATION BEGIN
            byte[] toEncodeAsBytes = Encoding.ASCII.GetBytes(sAuthorization);
            string returnValue = Convert.ToBase64String(toEncodeAsBytes);
            wr.Headers.Add("Authorization: Basic " + returnValue); //AUTHENTIFICATION END
            Stream rs = wr.GetRequestStream();
            DataSetStatus status = new DataSetStatus();
            WebResponse wresp = null;
            try
            {
                //Get the response
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                string respuesta = reader2.ReadToEnd();
                XmlSerializer serializer = new XmlSerializer(typeof(DataSetStatus));
                using (TextReader reader = new StringReader(respuesta))
                {
                    status = (DataSetStatus)serializer.Deserialize(reader);
                }
                if (status.QueuedCount > 0)//Registros pendientes, debo marcarlo como indefinido
                {
                    utilerias.newLineBitacora("Se reintentó el monitoreo, pero Blackboard indicó que tiene registros pendientes de procesar.", true, true);
                    consulta.getReporteBitacora(3, "U", true, "Indefinido", vsIdArchivoCargado);
                }
                else
                {
                    utilerias.newLineBitacora("BlackBoard indicó que los registros han sido procesados con éxito. Estas son sus estadísticas:", true, true);
                    utilerias.newLineBitacora("Registros procesados total: " + status.CompletedCount.ToString(), false, false);
                    utilerias.newLineBitacora("Registros procesados correctamente: " + status.CompletedCount.ToString(), false, false);
                    utilerias.newLineBitacora("Registros procesados con error: " + status.ErrorCount.ToString(), false, false);
                    if (status.ErrorCount > 0)
                        consulta.getReporteBitacora(3, "U", true, null, vsIdArchivoCargado);
                    else
                        consulta.getReporteBitacora(3, "U", true, null, null);
                }

            }
            catch (Exception ex)
            {
                utilerias.newLineBitacora(utilerias.controlErrores(ex, "Error en el monitoreo de Archivos indefinidos.", "Fin de la traza."), true, true);
                consulta.getReporteBitacora(3, "U", true, "Indefinido", vsIdArchivoCargado);
            }
            finally
            {
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
                wr = null;
            }
        }
    }
    #region Clases para deserializar el XML
    public class DataIntegrationId
    {
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "dataSetStatus")]
    public class DataSetStatus
    {
        [XmlElement(ElementName = "completedCount")]
        public int CompletedCount { get; set; }
        [XmlElement(ElementName = "dataIntegrationId")]
        public DataIntegrationId DataIntegrationId { get; set; }
        [XmlElement(ElementName = "dataSetUid")]
        public string DataSetUid { get; set; }
        [XmlElement(ElementName = "errorCount")]
        public int ErrorCount { get; set; }
        [XmlElement(ElementName = "lastEntryDate")]
        public string LastEntryDate { get; set; }
        [XmlElement(ElementName = "queuedCount")]
        public int QueuedCount { get; set; }
        [XmlElement(ElementName = "startDate")]
        public string StartDate { get; set; }
        [XmlElement(ElementName = "warningCount")]
        public int WarningCount { get; set; }
    }
    #endregion
}

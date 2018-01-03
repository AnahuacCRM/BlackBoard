using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Data;
using System.Net.Mail;

using System.Linq;
using System.Globalization;
using Data;
using System.Configuration;
using Ionic.Zip;

namespace appBlackBoard.Clases
{
    class Utilerias : VariablesGlobales
    {

        private const string csValor = "456^%43:;2323'32-0{][843";
        private const string csVector = "4587hst'3smd(@#&";
        private const string csAlgoritmo = "SHA1";
        private const int cnInteracciones = 2;
        private const int cnLlave = 256;
        private MailMessage Mensaje;
        private SmtpClient smtpClient;
        public int numeroServicioNotificacion { get; set; }
        Consultas consulta = new Consultas();
        public int setConvienteAEntero(object objeto)
        {
            try
            {
                return int.Parse(objeto.ToString());
            }
            catch
            {
                return 0;
            }
        }
        public string Desencriptar(string pscadena, string key)
        {
            // Generamos los arrays de Bytes de nuestras cadenas. Como csVector y csValor son cadenas
            // normales solo usamos Encoding.ASCII
            byte[] aVectorInicial = Encoding.ASCII.GetBytes(csVector);
            byte[] aValorRand = Encoding.ASCII.GetBytes(csValor);
            // Convertimos nuesta cadena encriptada (cipher) a un arrar
            byte[] aCipher = Convert.FromBase64String(pscadena);

            // Generemos la contraseña
            PasswordDeriveBytes cont = new PasswordDeriveBytes(key, aValorRand, csAlgoritmo, cnInteracciones);
            // Obtengamos el array de la llave. Dividido en Bytes. (8 bits)
            byte[] aLlave = cont.GetBytes(cnLlave / 8);

            // Usemos la clase Rijndael para la llave simetrica y usemos el modo Cipher Block Chaining (CBC)
            RijndaelManaged llaveSimetrica = new RijndaelManaged() { Mode = CipherMode.CBC };

            // Generemos el desencriptador
            ICryptoTransform desenc = llaveSimetrica.CreateDecryptor(aLlave, aVectorInicial);

            // Definamos donde tendremos los datos encriptados
            MemoryStream ms = new MemoryStream(aCipher);
            CryptoStream cs = new CryptoStream(ms, desenc, CryptoStreamMode.Read);

            // Definamos el arrar donde se colocaran nuestros datos desencriptados
            byte[] aCadena = new byte[aCipher.Length];

            // Comenzamos a desencriptar
            int tamB = cs.Read(aCadena, 0, aCadena.Length);

            // Liberemos la memoria
            ms.Close();
            cs.Close();

            // regresemos nuestra cadena desecriptada usando UTF8
            return Encoding.UTF8.GetString(aCadena, 0, tamB);

        }

        public DataTable obtenerErrorDataTable(string psError)
        {
            DataTable dtRetorno = new DataTable();
            DataColumn dcError = new DataColumn("Error");
            dtRetorno.Columns.Add(dcError);
            DataRow drResultado;
            drResultado = dtRetorno.NewRow();
            drResultado["Error"] = psError;
            dtRetorno.Rows.Add(drResultado);
            return dtRetorno;
        }

        public bool enviaCorreo(string pathBitacora)
        {
            newLineConsole("Enviando correo electrónico, espere...");
            bool Enviado = false;
            string Comentarios = "SIN COMENTARIOS.";
            var listaCredenciales = Desencriptar(MailCredential, KeyBB).Split(new[] { ";" }, StringSplitOptions.None).ToList();
            string Correos = MailTo;
            if (soloCorreosDesarrollo)
                Correos = CorreosDesarrollo;
            var listaCorreoDestinatario = Desencriptar(Correos, KeyBB).Split(new[] { ";" }, StringSplitOptions.None).ToList();

            SmtpClient server = new SmtpClient(Desencriptar(MailServer, KeyBB), setConvienteAEntero(Port));
            server.UseDefaultCredentials = false;
            server.Credentials = new System.Net.NetworkCredential(listaCredenciales[0], listaCredenciales[1]);
            server.EnableSsl = true;
            server.TargetName = Desencriptar(SPN, KeyBB);
            try
            {
                server.Credentials.GetCredential(Desencriptar(MailServer, KeyBB), setConvienteAEntero(Port), AuthenticationType);
                MailMessage mensajeCorreo = new MailMessage();
                getParametrosCorreo();
                mensajeCorreo.Subject = SubjectMail;
                mensajeCorreo.From = new MailAddress(listaCredenciales[0], "Correo para integración de desarrollos.");
                for (int i = 0; i < listaCorreoDestinatario.Count; i++)
                    mensajeCorreo.To.Add(listaCorreoDestinatario[i]);
                mensajeCorreo.Attachments.Add(new Attachment(pathBitacora));
                mensajeCorreo.Body = BodyMail;
                Mensaje = mensajeCorreo;
                smtpClient = server;
                smtpClient.Timeout = 60000;
                /* Enviar */
                ////utilerias.newLineConsole("Enviando correo de notificación...");
                smtpClient.Send(Mensaje);
                Enviado = true;
            }
            catch (Exception ex2)
            {
                Enviado = false;
                Comentarios = controlErrores(ex2, "", "");
                //utilerias.newLineConsole(Comentarios);
            }
            finally
            {
                smtpClient.Dispose();
            }

            return Enviado;
        }

        public void almacenaBitacora(bool sinError, int numeroEvento, string vsBitacora)
        {
            //utilerias.newLineConsole("Escribiendo bitácora...");
            string nameBitacora = "";
            try
            {
                string pathEjecuta = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace(@"file:\", "");
                string pathBitacora = Path.Combine(pathEjecuta, "Bitacoras");
                nameBitacora = getNombreBitacora(numeroEvento);
                if (!Directory.Exists(pathBitacora))
                    Directory.CreateDirectory(pathBitacora);
                string pathArchivo = Path.Combine(pathBitacora, nameBitacora);
                using (FileStream fs = File.Create(pathArchivo))
                {
                    var byteBitacora = Encoding.Unicode.GetBytes(vsBitacora);
                    foreach (byte b in byteBitacora)
                    {
                        fs.WriteByte(b);
                    }
                }
                if (!sinError)
                {
                    //Validaciones para ver que archivo envío.
                    try
                    {
                        FileInfo fileInfo = new FileInfo(pathArchivo);
                        if (fileInfo.Length < 4198400)//4198400 son aprox 4MB
                        {
                            enviaCorreo(pathArchivo);
                        }
                        else
                        {
                            using (ZipFile zip = new ZipFile())
                            {
                                zip.AddFile(pathArchivo, "");
                                zip.Save(pathArchivo + ".zip");
                            }

                            enviaCorreo(pathArchivo + ".zip");
                        }
                    }
                    catch
                    {
                        enviaCorreo(pathArchivo);
                    }
                }
            }
            catch (Exception ex)
            {
                //utilerias.newLineConsole("Error durante el almacenaje de la bitácora: " + ex.Message);
            }

        }

        private string getNombreBitacora(int numeroEvento)
        {
            string vsNombreBitacora = "";
            string vsAmbienteBitacora = "";
            switch (vsAmbiente)
            {
                case "QA":
                    vsAmbienteBitacora = "DE QA";
                    break;
                case "DEV":
                    vsAmbienteBitacora = "DE PRUEBAS";
                    break;
                case "PROD":
                default:
                    vsAmbienteBitacora = "DE PRODUCCION";
                    break;
            }


            vsNombreBitacora = "Bitacora " + vsAmbienteBitacora + " " + getDateTimeToString().Replace("/", "-").Replace(":", ".") + " Evento " + numeroEvento.ToString() + ".txt";


            return vsNombreBitacora;
        }

        private void getParametrosCorreo()
        {
            SubjectMail = ConfigurationManager.AppSettings["csSubjectMail"];
            BodyMail = ConfigurationManager.AppSettings["csBodyMail"].Replace("#SALTO#", Environment.NewLine);
            switch (vsAmbiente)
            {
                case "QA":
                    SubjectMail += "QA.";
                    BodyMail += Environment.NewLine + "Aplicación ejecutada desde el ambiente QA.";
                    break;
                case "DEV":
                    SubjectMail += "de Pruebas.";
                    BodyMail += Environment.NewLine + "Aplicación ejecutada desde el ambiente de Pruebas.";
                    break;
                case "PROD":
                default:
                    SubjectMail += "Productivo.";
                    BodyMail += Environment.NewLine + "Aplicación ejecutada desde el ambiente Productivo.";
                    break;
            }
            if (numeroServicioNotificacion != 0)
            {
                BodyMail += Environment.NewLine + "Esta bitácora incluye la información hasta del servicio: " + numeroServicioNotificacion.ToString(); ;
            }
        }

        public string getTestingConexionBD()
        {
            try
            {
                //Consultas consulta = new Consultas();
                return consulta.validaConexionBD();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        /// <summary>
        /// Método que estandariza el control de errores
        /// </summary>       
        /// <param name="ex">Variable de tipo Excepcion(Usualmete es ex)</param>
        /// <param name="MensajeInicio">Mensaje personalizable para referenciar el error.</param>
        /// <param name="MensajeFin">Mensaje personalizable para dar cierre del error.</param>
        /// <returns></returns>
        public string controlErrores(Exception ex, string MensajeInicio, string MensajeFin)
        {

            string vsRetorno = "";
            if (!string.IsNullOrWhiteSpace(MensajeInicio))
                vsRetorno += MensajeInicio + (char)10;
            try
            {
                if (ex != null)
                {
                    vsRetorno += "Error Interno, mensaje general: " + ex.Message + (char)10;
                    if (ex.InnerException.Message != null)
                        vsRetorno += "Error Interno, Detalle 1: " + ex.InnerException.Message + (char)10;
                    if (ex.InnerException.InnerException.Message != null)
                        vsRetorno += "Error Interno, Detalle 2: " + ex.InnerException.InnerException.Message + (char)10;
                    if (ex.InnerException.InnerException.InnerException.Message != null)
                        vsRetorno += "Error Interno, Detalle 3: " + ex.InnerException.InnerException.InnerException.Message + (char)10;
                    if (ex.InnerException.InnerException.InnerException.InnerException.Message != null)
                        vsRetorno += "Error Interno, Detalle 4: " + ex.InnerException.InnerException.InnerException.InnerException.Message + (char)10;
                    if (ex.InnerException.InnerException.InnerException.InnerException.InnerException.Message != null)
                        vsRetorno += "Error Interno, Detalle 5: " + ex.InnerException.InnerException.InnerException.InnerException.InnerException.Message + (char)10;
                }
            }
            catch
            {

            }
            if (!string.IsNullOrWhiteSpace(MensajeFin))
                vsRetorno += MensajeFin + (char)10;
            return vsRetorno;
        }

        public string getDateTimeToString()
        {
            DateTime dateTime = DateTime.Now;
            CultureInfo ci = new CultureInfo("es-MX");
            string vsFechaRetorno = "";
            try
            {
                vsFechaRetorno = dateTime.ToString(ci);
            }
            catch
            {
                vsFechaRetorno = dateTime.ToString();
            }
            return vsFechaRetorno;

        }

        public string getComprimeBitatoras()
        {
            string vsRetorno = "";
            try
            {
                string pathEjecuta = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace(@"file:\", "");
                string pathBitacora = Path.Combine(pathEjecuta, "Bitacoras");
                DirectoryInfo directorioBitacoras = new DirectoryInfo(pathBitacora);
                //utilerias.newLineConsole("Obteniendo las bitácoras a comprimir.");
                var listadoBitacoras = directorioBitacoras.GetFiles("*.txt", SearchOption.TopDirectoryOnly);
                if (listadoBitacoras.Length > 0)
                {
                    vsRetorno += "Listado de Bitácoras a comprimir y eliminar: " + (char)10;
                    using (ZipFile zip = new ZipFile())
                    {
                        for (int i = 0; i < listadoBitacoras.Length; i++)
                        {
                            //utilerias.newLineConsole("Agregando " + (i + 1).ToString() + " de " + listadoBitacoras.Length + " archivos a comprimir.");
                            vsRetorno += "Archivo " + (i + 1).ToString() + ": " + listadoBitacoras[i].Name + (char)10;
                            zip.AddFile(listadoBitacoras[i].FullName, "");
                        }
                        //utilerias.newLineConsole("Comprimiendo...");
                        zip.Save(pathBitacora + "\\Zip_Bitacoras_" + getDateTimeToString().Replace("/", "-").Replace(":", ".") + ".zip");
                        for (int i = 0; i < listadoBitacoras.Length; i++)
                        {
                            //utilerias.newLineConsole("Eliminando " + (i + 1).ToString() + " de " + listadoBitacoras.Length + " archivos comprimidos.");
                            File.Delete(listadoBitacoras[i].FullName);
                        }
                    }
                }
                else
                {
                    vsRetorno += "No se detectaron archivos a eliminar." + (char)10;
                }
            }
            catch (Exception ex)
            {
                vsRetorno += controlErrores(ex, "", "");
            }
            return vsRetorno;
        }

        public string getCreaBitacora(int numeroEvento)
        {
            string pathArchivo = "";
            try
            {
                string nameBitacora = "";
                string pathEjecuta = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace(@"file:\", "");
                string pathBitacora = Path.Combine(pathEjecuta, "Bitacoras");
                nameBitacora = getNombreBitacora(numeroEvento);
                if (!Directory.Exists(pathBitacora))
                    Directory.CreateDirectory(pathBitacora);
                pathArchivo = Path.Combine(pathBitacora, nameBitacora);
                using (FileStream fs = File.Create(pathArchivo))
                {
                    var byteBitacora = Encoding.Unicode.GetBytes(string.Empty);
                    foreach (byte b in byteBitacora)
                    {
                        fs.WriteByte(b);
                    }
                }
            }
            catch (Exception ex)
            {
                controlErrores(ex, "", "");
            }
            return pathArchivo;
        }

        public void getEscribeBitacora()
        {
            try
            {
                StreamWriter log;
                if (File.Exists(vsRutaBitacora))
                {
                    log = File.AppendText(vsRutaBitacora);
                }
                else
                    return;
                log.WriteLine(vsBitacora);
                log.Close();
                vsBitacora = "";
            }
            catch (Exception ex)
            {
                controlErrores(ex, null, null);
            }
        }

        public void getEscribeBitacora(string pathArchivo, string psTexto)
        {
            try
            {
                StreamWriter log;
                if (File.Exists(pathArchivo))
                {
                    log = File.AppendText(pathArchivo);
                }
                else
                    return;
                log.WriteLine(psTexto);
                log.Close();
            }
            catch (Exception ex)
            {
                controlErrores(ex, null, null);
            }
        }

        public string getDatosJson(int numeroServicio, string idBanner, string periodo, string vpdi, string programa)
        {
            string retorno = "Json enviado para el servicio " + numeroServicio.ToString();
            if (!string.IsNullOrWhiteSpace(idBanner))
                retorno += ", con el idBanner " + idBanner;
            if (!string.IsNullOrWhiteSpace(periodo))
                retorno += ", para el periodo " + periodo;
            if (!string.IsNullOrWhiteSpace(vpdi))
                retorno += ", con la VPDI " + vpdi;
            if (!string.IsNullOrWhiteSpace(programa))
                retorno += " y con el programa " + programa;
            retorno += "." + (char)10;
            return retorno;
        }

        public bool inicializaConnection()
        {
            try
            {
                voConection = ClassData.getOracleConnection(conexion.cadenaConexion, KeyBB);
            }
            catch (Exception ex)
            {
                vsBitacora += controlErrores(ex, "Error al inicializar la conexión.", "") + (char)10;// "Error al inicializar la conexión: " + ex.Message + (char)10;
                return false;
            }
            return true;
        }
        public bool inicializaUsrPsw()
        {
            try
            {
                vsUsrPsw = Desencriptar(usuario.cadenaUsrPsw, KeyBB);
            }
            catch (Exception ex)
            {
                vsBitacora += controlErrores(ex, "Error al inicializar el usuario.", "") + (char)10;
                return false;
            }
            return true;
        }

        private void newLineConsole(string Texto)
        {
            Console.WriteLine(Texto);
            getEscribeBitacora(vsRutaBitacoraConsola, Texto);
        }
        public string getCreaBitacoraConsole(int numeroEvento)
        {
            string pathArchivo = "";
            try
            {
                string nameBitacora = "";
                string pathEjecuta = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace(@"file:\", "");
                string pathBitacora = Path.Combine(pathEjecuta, "Bitacoras");
                nameBitacora = getNombreBitacora(numeroEvento);
                if (!Directory.Exists(pathBitacora))
                    Directory.CreateDirectory(pathBitacora);
                pathArchivo = Path.Combine(pathBitacora, " - CONSOLA " + nameBitacora);
                using (FileStream fs = File.Create(pathArchivo))
                {
                    var byteBitacora = Encoding.Unicode.GetBytes(string.Empty);
                    foreach (byte b in byteBitacora)
                    {
                        fs.WriteByte(b);
                    }
                }
            }
            catch (Exception ex)
            {
                controlErrores(ex, "", "");
            }
            return pathArchivo;
        }

        public bool inicializaURL()
        {
            //Consultas consulta = new Consultas();
            dtURL = consulta.getURL();
            if(dtURL.Columns.Contains("Error"))
            {
                newLineBitacora("Error al obtener los Endpoints de BlackBoard: " +  dtURL.Rows[0][0].ToString(), false, true);
                return false;
            }
            else if (dtURL.Columns.Contains("SinRegistros") || dtURL.Rows.Count == 0)
            {
                newLineBitacora("No se obtuvieron los Endpoints de BlackBoard y el proceso no puede continuar.", false, true);
                return false;
            }
            indiceProcesoOrden = 0;//Establezco en 0 para cuando se vuelva a obtener las url poder reiniciar.
            return true;
        }
        public void newLineBitacora(string lineaBitacora, bool conFecha, bool aConsola)
        {
            vsBitacora += lineaBitacora;
            if (conFecha)
                vsBitacora += " " + getDateTimeToString();
            vsBitacora += (char)10;
            if (aConsola)
                newLineConsole(lineaBitacora);
        }

        public bool inicializaPeriodos()
        {
            dtPeriodos = consulta.getPeriodos();
            if (dtPeriodos.Columns.Contains("Error"))
            {
                newLineBitacora("Error al obtener los periodos a procesar: " + dtPeriodos.Rows[0][0].ToString(), false, false);
                return false;
            }
            else if (dtPeriodos.Columns.Contains("SinRegistros") || dtPeriodos.Rows.Count == 0)
            {
                newLineBitacora("No se obtuvieron los periodos a procesar y el proceso no puede continuar.", false, false);
                return false;
            }

            return true;
        }
        public bool inicializaEventos(string periodo)
        {
            dtEventos = consulta.getEventos(periodo);
            if (dtEventos.Columns.Contains("Error"))
            {
                newLineBitacora("Error al obtener los periodos a procesar: " + dtEventos.Rows[0][0].ToString(), false, false);
                return false;
            }
            else if (dtEventos.Columns.Contains("SinRegistros") || dtEventos.Rows.Count == 0)
            {
                newLineBitacora("No se obtuvieron los periodos a procesar y el proceso no puede continuar.", false, false);
                return false;
            }

            return true;
        }
        public bool inicializaCampus()
        {
            dtCampus = consulta.getCampus();
            if (dtCampus.Columns.Contains("Error"))
            {
                newLineBitacora("Error al obtener los periodos a procesar: " + dtCampus.Rows[0][0].ToString(), false, false);
                return false;
            }
            else if (dtCampus.Columns.Contains("SinRegistros") || dtCampus.Rows.Count == 0)
            {
                newLineBitacora("No se obtuvieron los periodos a procesar y el proceso no puede continuar.", false, false);
                return false;
            }

            return true;
        }
        public void inicializaDTReferencia()
        {
            DataColumn dcReferencia = new DataColumn("Referencia");
            DataColumn dcPeriodo = new DataColumn("Periodo");
            DataColumn dcArchivo = new DataColumn("Archivo");
            DataColumn dcEvento = new DataColumn("Evento");
            dtReferenciasIndefinidas.Columns.Add(dcReferencia);
            dtReferenciasIndefinidas.Columns.Add(dcPeriodo);
            dtReferenciasIndefinidas.Columns.Add(dcArchivo);
            dtReferenciasIndefinidas.Columns.Add(dcEvento);
        }
    }

    public class Conexion
    {
        public string cadenaConexion { get; set; }
        public Conexion()
        {
            getParametrosConexion();
        }
        public void getParametrosConexion()
        {
            switch (VariablesGlobales.vsAmbiente)
            {
                case "QA":
                    cadenaConexion = VariablesGlobales.ConnectionQA;
                    break;
                case "DEV":
                    cadenaConexion = VariablesGlobales.ConnectionDEV;
                    break;
                case "PROD":
                default:
                    cadenaConexion = VariablesGlobales.ConnectionPROD;
                    break;
            }
        }
    }
    public class Usuario
    {
        public string cadenaUsrPsw { get; set; }
        public Usuario()
        {
            getParametrosUsrPsw();
        }
        public void getParametrosUsrPsw()
        {
            switch (VariablesGlobales.vsAmbiente)
            {
                case "QA":
                    cadenaUsrPsw = VariablesGlobales.LogUsrPswQA;
                    break;
                case "DEV":
                    cadenaUsrPsw = VariablesGlobales.LogUsrPswDEV;
                    break;
                case "PROD":
                default:
                    cadenaUsrPsw = VariablesGlobales.LogUsrPswPROD;
                    break;
            }
        }
    }
}
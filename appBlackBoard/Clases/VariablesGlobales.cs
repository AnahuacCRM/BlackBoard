using System.Configuration;
using System;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace appBlackBoard.Clases
{
    class VariablesGlobales
    {

        #region Constructores de Clases
        #endregion

        #region Variables Int
        public static int numeroRegistrosProcesados = 0;
        public static int numeroEvento = 0;
        #endregion

        #region Variables String
        public static string vsBitacora;
        public static string vsFinBitacora = (char)10 + "________________________________________________________________________________________________________________________________________________________________" + (char)10 + "                                                             FIN DE LA EJECUCIÓN DE LA APLICACIÓN";

        #endregion

        #region Variables Bool

        #endregion

        #region Variables que son leidas de AppConfig
        public static bool soloCorreosDesarrollo = bool.Parse(ConfigurationManager.AppSettings["csSoloCorreosDesarrollo"]);
        public static string KeyBB = ConfigurationManager.AppSettings["csKeyBB"];
        public static string vsAmbiente = ConfigurationManager.AppSettings["csAmbiente"];
        public static string BodyMail = ConfigurationManager.AppSettings["csBodyMail"].Replace("#SALTO#", Environment.NewLine);
        public static string SubjectMail = ConfigurationManager.AppSettings["csSubjectMail"];
        public static string MailServer = ConfigurationManager.AppSettings["csMailServer"];
        public static string MailCredential = ConfigurationManager.AppSettings["csMailCredential"];
        public static string Port = ConfigurationManager.AppSettings["csPort"];
        public static string SPN = ConfigurationManager.AppSettings["csSPN"];
        public static string AuthenticationType = ConfigurationManager.AppSettings["csAuthenticationType"];
        public static string MailTo = ConfigurationManager.AppSettings["csMailTo"];
        public static string CorreosDesarrollo = ConfigurationManager.AppSettings["csCorreosDesarrollo"];
        public static string ConnectionQA = ConfigurationManager.AppSettings["csConnectionQA"];
        public static string ConnectionDEV = ConfigurationManager.AppSettings["csConnectionDEV"];
        public static string ConnectionPROD = ConfigurationManager.AppSettings["csConnectionPROD"];

        public static string LogUsrPswDEV = ConfigurationManager.AppSettings["csLogUsrPswDEV"];
        public static string LogUsrPswQA = ConfigurationManager.AppSettings["csLogUsrPswQA"];
        public static string LogUsrPswPROD = ConfigurationManager.AppSettings["csLogUsrPswPROD"];

        public static Utilerias utilerias = new Utilerias();

        public static string vsRutaBitacora = null;// utilerias.getCreaBitacora(numeroEvento);
        public static string vsRutaBitacoraConsola = null;// utilerias.getCreaBitacoraConsole(numeroEvento);
        public static Conexion conexion = new Conexion();
        public static Usuario usuario = new Usuario();
        public static OracleConnection voConection = null;
        public static string NumeroEventoAmarre = ConfigurationManager.AppSettings["csNumeroEventoAmarre"];
        public static string Token = null;
        public static int numeroRegistrosProcesadosGlobal = 0;
        public static string vsDirectorioLayouts = null;
        public static string vsRutaArchivoLayouts = null;
        public static string vsUsrPsw = string.Empty;
        public static DataTable dtURL = null;
        public static string vsSeparador = "________________________________________________________________________________________________________________________________________________________________";
        public static int indiceProcesoOrden = 0;
        public static DataTable dtPeriodos = null;
        public static bool modoDebug = bool.Parse(ConfigurationManager.AppSettings["csModoDebug"]);
        public static DataTable dtEventos = null;
        public static string vsIdArchivoCargado = null;
        public static DataTable dtCampus = null;
        public static string nombreArchivo = null;
        public static string vsgPeriodo = null;
        public static DataTable dtReferenciasIndefinidas = new DataTable();
        public static bool ejecucionManual = false;
        public static string origenEjecucion = null;
        public static bool contieneEstatus = false;
        public static int countLineasGlobal = 0;
        #endregion
        public static void inicializaBitacora()
        {
            vsRutaBitacora = utilerias.getCreaBitacora(numeroEvento);
            vsRutaBitacoraConsola = utilerias.getCreaBitacoraConsole(numeroEvento);
        }
       
    }
}

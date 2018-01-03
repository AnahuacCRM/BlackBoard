using System;
using System.Data;
using Data;
using Oracle.ManagedDataAccess.Client;

namespace appBlackBoard.Clases
{
    class Consultas:Program
    {
        public DataTable getConsultaDatos()
        {
            DataTable dtRetorno = new DataTable();
            string vsRetorno = "";
            DataSet ds = new DataSet();
            try
            {
                ClassData ClassData = new ClassData();
                vsRetorno = ClassData.getOpenOracleConnection(voConection);
                if (vsRetorno.Length > 0) return utilerias.obtenerErrorDataTable(vsRetorno);
                OracleCommand voCommand = ClassData.getOracleCommand(voConection);
                voCommand.CommandType = CommandType.StoredProcedure;
                if (modoDebug || ejecucionManual)
                {
                    voCommand.CommandText = "kwablbo.getArchivos";
                    voCommand.Parameters.Add(ClassData.getOracleParameter(OracleDbType.Decimal, ParameterDirection.Input, 0, "pnNumEven", numeroEvento));
                    voCommand.Parameters.Add(ClassData.getOracleParameter(OracleDbType.RefCursor, ParameterDirection.Output, 0, "cuFiles", null));
                }
                else
                {
                    voCommand.CommandText = "kwcblbo.GetCarBB";
                    voCommand.Parameters.Add(ClassData.getOracleParameter(OracleDbType.RefCursor, ParameterDirection.Output, 0, "CuProCarBB", null));
                    voCommand.Parameters.Add(ClassData.getOracleParameter(OracleDbType.Decimal, ParameterDirection.Input, 0, "pnEveNum", numeroEvento));
                    voCommand.Parameters.Add(ClassData.getOracleParameter(OracleDbType.Varchar2, ParameterDirection.Output, 0, "psError", null));
                    contieneEstatus = true;
                }
                try
                {
                    var datos = new OracleDataAdapter(voCommand);
                    datos.TableMappings.Add("Tabla", "Resultado");
                    datos.Fill(ds);
                    dtRetorno = ds.Tables[0];
                }
                catch (Exception ex)
                {
                    try
                    {
                        if ((ex.Message.Contains("ORA-04068") || ex.Message.Contains("ORA-04061")))
                        {
                            var datos = new OracleDataAdapter(voCommand);
                            datos.TableMappings.Add("Tabla", "Resultado");
                            datos.Fill(ds);
                            dtRetorno = ds.Tables[0];
                        }
                        else
                            dtRetorno = utilerias.obtenerErrorDataTable("Error en la consulta: " + vsRetorno + " " + ex.Message + ", Mensaje de Base de Datos: " + voCommand.Parameters["psError"].Value.ToString());
                    }
                    catch (Exception ex2)
                    {
                        dtRetorno = utilerias.obtenerErrorDataTable("Error en la consulta: " + vsRetorno + " " + ex2.Message + ", Mensaje de Base de Datos: " + voCommand.Parameters["psError"].Value.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                dtRetorno = utilerias.obtenerErrorDataTable("Error en la consulta: " + vsRetorno + " " + ex.Message);
            }
            
            return dtRetorno;
        }
        public DataTable getURL()
        {
            DataTable dtRetorno = new DataTable();
            string vsRetorno = "";
            DataSet ds = new DataSet();
            try
            {
                ClassData ClassData = new ClassData();
                vsRetorno = ClassData.getOpenOracleConnection(voConection);
                if (vsRetorno.Length > 0) return utilerias.obtenerErrorDataTable(vsRetorno);
                OracleCommand voCommand = ClassData.getOracleCommand(voConection);
                voCommand.CommandType = CommandType.StoredProcedure;
                voCommand.CommandText = "kwablbo.getURL";
                voCommand.Parameters.Add(ClassData.getOracleParameter(OracleDbType.Decimal, ParameterDirection.Input, 0, "pnNumEven", numeroEvento));
                voCommand.Parameters.Add(ClassData.getOracleParameter(OracleDbType.RefCursor, ParameterDirection.Output, 0, "cuFiles", null));
                try
                {
                    var datos = new OracleDataAdapter(voCommand);
                    datos.TableMappings.Add("Tabla", "Resultado");
                    datos.Fill(ds);
                    dtRetorno = ds.Tables[0];
                }
                catch (Exception ex)
                {
                    try
                    {
                        if ((ex.Message.Contains("ORA-04068") || ex.Message.Contains("ORA-04061")))
                        {
                            var datos = new OracleDataAdapter(voCommand);
                            datos.TableMappings.Add("Tabla", "Resultado");
                            datos.Fill(ds);
                            dtRetorno = ds.Tables[0];
                        }
                        else
                            dtRetorno = utilerias.obtenerErrorDataTable("Error en la consulta: " + vsRetorno + " " + ex.Message + ", Mensaje de Base de Datos: " + voCommand.Parameters["psError"].Value.ToString());
                    }
                    catch (Exception ex2)
                    {
                        dtRetorno = utilerias.obtenerErrorDataTable("Error en la consulta: " + vsRetorno + " " + ex2.Message + ", Mensaje de Base de Datos: " + voCommand.Parameters["psError"].Value.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                dtRetorno = utilerias.obtenerErrorDataTable("Error en la consulta: " + vsRetorno + " " + ex.Message);
            }
            if (!dtRetorno.Columns.Contains("Error"))
            {
                DataColumn dcParametro = new DataColumn("idArchivoCargado");
                dtRetorno.Columns.Add(dcParametro);
            }
            return dtRetorno;
        }
        public DataTable getPeriodos()
        {
            DataTable dtRetorno = new DataTable();
            string vsRetorno = "";
            //string vsQuery = "SELECT '201760' AS PERIODO FROM DUAL WHERE 1 = 1";
            string vsQuery = "KWCBLBO.GetPer";
            DataSet ds = new DataSet();
            try
            {
                ClassData classData = new ClassData();
                vsRetorno = ClassData.getOpenOracleConnection(voConection);
                if (vsRetorno.Length > 0) return utilerias.obtenerErrorDataTable(vsRetorno);
                OracleCommand voCommand = ClassData.getOracleCommand(voConection);
                //voCommand.CommandType = CommandType.Text;
                voCommand.CommandType = CommandType.StoredProcedure;
                voCommand.CommandText = vsQuery;
                voCommand.Parameters.Add(ClassData.getOracleParameter(OracleDbType.RefCursor, ParameterDirection.Output, 0, "cuPer", null));
                voCommand.Parameters.Add(ClassData.getOracleParameter(OracleDbType.Varchar2, ParameterDirection.Output, 0, "psError ", null));

                try
                {
                    dtRetorno = ClassData.getExecuteCommandToTable3(voCommand, null);
                }
                catch (Exception ex)
                {
                    try
                    {
                        if ((ex.Message.Contains("ORA-04068") || ex.Message.Contains("ORA-04061")))
                        {
                            var datos = new OracleDataAdapter(voCommand);
                            datos.TableMappings.Add("Tabla", "Resultado");
                            datos.Fill(ds);
                            dtRetorno = ds.Tables[0];
                        }
                        else
                            dtRetorno = utilerias.obtenerErrorDataTable("Error en la consulta: " + vsRetorno + " " + ex.Message + ", Mensaje de Base de Datos: " + voCommand.Parameters["psError"].Value.ToString());
                    }
                    catch (Exception ex2)
                    {
                        dtRetorno = utilerias.obtenerErrorDataTable("Error en la consulta: " + vsRetorno + " " + ex2.Message + ", Mensaje de Base de Datos: " + voCommand.Parameters["psError"].Value.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                dtRetorno = utilerias.obtenerErrorDataTable("Error en la consulta: " + vsRetorno + " " + ex.Message);
            }

            return dtRetorno;
        }
        public DataTable getEventos(string periodo)
        {
            DataTable dtRetorno = new DataTable();
            string vsRetorno = "";
            DataSet ds = new DataSet();
            try
            {
                ClassData ClassData = new ClassData();
                vsRetorno = ClassData.getOpenOracleConnection(voConection);
                if (vsRetorno.Length > 0) return utilerias.obtenerErrorDataTable(vsRetorno);
                OracleCommand voCommand = ClassData.getOracleCommand(voConection);
                voCommand.CommandType = CommandType.StoredProcedure;
                voCommand.CommandText = "kwablbo.setSabana";
                voCommand.Parameters.Add(ClassData.getOracleParameter(OracleDbType.Varchar2, ParameterDirection.Input, 0, "psTerm", periodo));
                voCommand.Parameters.Add(ClassData.getOracleParameter(OracleDbType.RefCursor, ParameterDirection.Output, 0, "cuFiles", null));
                try
                {
                    var datos = new OracleDataAdapter(voCommand);
                    datos.TableMappings.Add("Tabla", "Resultado");
                    datos.Fill(ds);
                    dtRetorno = ds.Tables[0];
                }
                catch (Exception ex)
                {
                    try
                    {
                        if ((ex.Message.Contains("ORA-04068") || ex.Message.Contains("ORA-04061")))
                        {
                            var datos = new OracleDataAdapter(voCommand);
                            datos.TableMappings.Add("Tabla", "Resultado");
                            datos.Fill(ds);
                            dtRetorno = ds.Tables[0];
                        }
                        else
                            dtRetorno = utilerias.obtenerErrorDataTable("Error en la consulta: " + vsRetorno + " " + ex.Message + ", Mensaje de Base de Datos: " + voCommand.Parameters["psError"].Value.ToString());
                    }
                    catch (Exception ex2)
                    {
                        dtRetorno = utilerias.obtenerErrorDataTable("Error en la consulta: " + vsRetorno + " " + ex2.Message + ", Mensaje de Base de Datos: " + voCommand.Parameters["psError"].Value.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                dtRetorno = utilerias.obtenerErrorDataTable("Error en la consulta: " + vsRetorno + " " + ex.Message);
            }

            return dtRetorno;
        }
        public DataTable getCampus()
        {
            DataTable dtRetorno = new DataTable();
            string vsRetorno = "";
            string vsQuery = "";
            vsQuery += "SELECT 'UAC' AS CAMP FROM DUAL UNION  ";
            vsQuery += "SELECT 'UAM' AS CAMP FROM DUAL UNION  ";
            vsQuery += "SELECT 'UAN' AS CAMP FROM DUAL UNION  ";
            vsQuery += "SELECT 'UAO' AS CAMP FROM DUAL UNION  ";
            vsQuery += "SELECT 'UAP' AS CAMP FROM DUAL UNION  ";
            vsQuery += "SELECT 'UAQ' AS CAMP FROM DUAL UNION  ";
            vsQuery += "SELECT 'UAS' AS CAMP FROM DUAL UNION  ";
            vsQuery += "SELECT 'UAX' AS CAMP FROM DUAL";
            //string vsQuery = "SELECT STVCAMP_CODE AS CAMP FROM STVCAMP WHERE STVCAMP_CODE <> 'ISF' AND STVCAMP_CODE <> '000' AND STVCAMP_CODE <> 'UAT' AND STVCAMP_CODE <> 'PRA' AND STVCAMP_CODE <> 'A'";
            DataSet ds = new DataSet();
            try
            {
                ClassData classData = new ClassData();
                vsRetorno = ClassData.getOpenOracleConnection(voConection);
                if (vsRetorno.Length > 0) return utilerias.obtenerErrorDataTable(vsRetorno);
                OracleCommand voCommand = ClassData.getOracleCommand(voConection);
                voCommand.CommandType = CommandType.Text;
                voCommand.CommandText = vsQuery;
                try
                {
                    dtRetorno = ClassData.getExecuteCommandToTable3(voCommand, null);
                }
                catch (Exception ex)
                {
                    try
                    {
                        if ((ex.Message.Contains("ORA-04068") || ex.Message.Contains("ORA-04061")))
                        {
                            var datos = new OracleDataAdapter(voCommand);
                            datos.TableMappings.Add("Tabla", "Resultado");
                            datos.Fill(ds);
                            dtRetorno = ds.Tables[0];
                        }
                        else
                            dtRetorno = utilerias.obtenerErrorDataTable("Error en la consulta: " + vsRetorno + " " + ex.Message + ", Mensaje de Base de Datos: " + voCommand.Parameters["psError"].Value.ToString());
                    }
                    catch (Exception ex2)
                    {
                        dtRetorno = utilerias.obtenerErrorDataTable("Error en la consulta: " + vsRetorno + " " + ex2.Message + ", Mensaje de Base de Datos: " + voCommand.Parameters["psError"].Value.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                dtRetorno = utilerias.obtenerErrorDataTable("Error en la consulta: " + vsRetorno + " " + ex.Message);
            }

            return dtRetorno;
        }
        public string getReporteBitacora(int numeroProceso, string accion, bool llevaFechaFinProceso, string errorProceso, string referenciaBB)
        {
            DateTime fechaFinProceso = DateTime.Now;
            string vsMensajeBitacora = "registro en Monitoreo para el número de evento " + numeroEvento.ToString() + " y para el archivo " + nombreArchivo;
            if (accion == "I")
                vsMensajeBitacora = "Insertando nuevo " + vsMensajeBitacora;
            else
                vsMensajeBitacora = "Actualizando " + vsMensajeBitacora;
            string vsRetorno = "";
            DataSet ds = new DataSet();
            try
            {
                utilerias.newLineBitacora(vsMensajeBitacora, true, true);
                ClassData ClassData = new ClassData();
                vsRetorno = ClassData.getOpenOracleConnection(voConection);
                if (vsRetorno.Length > 0) return vsRetorno;
                OracleCommand voCommand = ClassData.getOracleCommand(voConection);
                voCommand.CommandType = CommandType.StoredProcedure;

                voCommand.CommandText = "kwablbo.setAvance";
                voCommand.Parameters.Add(ClassData.getOracleParameter(OracleDbType.Decimal, ParameterDirection.Input, 0, "pnNumEven", numeroEvento));
                voCommand.Parameters.Add(ClassData.getOracleParameter(OracleDbType.Decimal, ParameterDirection.Input, 0, "psStep", numeroProceso));
                voCommand.Parameters.Add(ClassData.getOracleParameter(OracleDbType.Varchar2, ParameterDirection.Input, 0, "psNameFile", nombreArchivo));
                voCommand.Parameters.Add(ClassData.getOracleParameter(OracleDbType.Varchar2, ParameterDirection.Input, 0, "psAccion", accion));
                if (llevaFechaFinProceso)
                    voCommand.Parameters.Add(ClassData.getOracleParameter(OracleDbType.Date, ParameterDirection.Input, 0, "psEndDate", fechaFinProceso));
                else
                    voCommand.Parameters.Add(ClassData.getOracleParameter(OracleDbType.Date, ParameterDirection.Input, 0, "psEndDate", null));
                voCommand.Parameters.Add(ClassData.getOracleParameter(OracleDbType.Varchar2, ParameterDirection.Input, 0, "psError", errorProceso));
                voCommand.Parameters.Add(ClassData.getOracleParameter(OracleDbType.Varchar2, ParameterDirection.Input, 0, "psRef", referenciaBB));
                voCommand.Parameters.Add(ClassData.getOracleParameter(OracleDbType.Varchar2, ParameterDirection.Input, 0, "psOrigen", origenEjecucion));
                //psOrigen
                voCommand.Parameters.Add(ClassData.getOracleParameter(OracleDbType.Varchar2, ParameterDirection.Output, 0, "psErrorBD", null));
                try
                {
                    ClassData.getExecuteCommand(voCommand);
                    if(voCommand.Parameters["psErrorBD"].Value != null && voCommand.Parameters["psErrorBD"].Value.ToString() != "null" && voCommand.Parameters["psErrorBD"].Value.ToString() != string.Empty)
                    {
                        utilerias.newLineBitacora("Error al realizar la operación " + accion + " en monitoreo: " + voCommand.Parameters["psErrorBD"].Value, true, false);
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        if ((ex.Message.Contains("ORA-04068") || ex.Message.Contains("ORA-04061")))
                        {
                            var datos = new OracleDataAdapter(voCommand);
                            datos.TableMappings.Add("Tabla", "Resultado");
                            datos.Fill(ds);
                            //dtRetorno = ds.Tables[0];
                        }
                        else { }
                        //dtRetorno = utilerias.obtenerErrorDataTable("Error en la consulta: " + vsRetorno + " " + ex.Message + ", Mensaje de Base de Datos: " + voCommand.Parameters["psError"].Value.ToString());
                    }
                    catch (Exception ex2)
                    {
                        //dtRetorno = utilerias.obtenerErrorDataTable("Error en la consulta: " + vsRetorno + " " + ex2.Message + ", Mensaje de Base de Datos: " + voCommand.Parameters["psError"].Value.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                //dtRetorno = utilerias.obtenerErrorDataTable("Error en la consulta: " + vsRetorno + " " + ex.Message);
            }

            return vsRetorno;
        }
        public string validaConexionBD()
        {
            return "";
        }       
    }
}

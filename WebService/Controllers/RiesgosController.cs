using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.Http;
using Oracle.ManagedDataAccess.Client;
using WebService.Models;

namespace WebService.Controllers
{
    public class RiesgosController : ApiController
    {
        List<Riesgo> listaRiesgos = new List<Riesgo>();
        List<ContratoConcentrado> listaContratosConcentrados = new List<ContratoConcentrado>();
        List<ContratoDetalle> listaContratoDetalles = new List<ContratoDetalle>();
        List<ClienteImx> listaClientesImx = new List<ClienteImx>();

        [HttpGet]
        [Route("api/cuentas")]
        [Authorize]
        // GET: Cuentas
        public List<Riesgo> GetCuentas(string RFC)
        {
            try
            {
                var path = AppDomain.CurrentDomain.BaseDirectory;

                // create connection
                OracleConnection oracleConexion = new OracleConnection();

                // create connection string using builder
                OracleConnectionStringBuilder sqlConexionString = new OracleConnectionStringBuilder();
                sqlConexionString.ConnectionString = ConfigurationManager.ConnectionStrings["ABCLeasingOracle"].ConnectionString;

                // connect
                oracleConexion.ConnectionString = sqlConexionString.ConnectionString;
                oracleConexion.Open();
                Console.WriteLine("Connection established (" + oracleConexion.ServerVersion + ")");

                OracleCommand oracleComando = oracleConexion.CreateCommand();
                oracleComando.CommandType = CommandType.Text;
                oracleComando.Connection = oracleConexion;

                string ConsultaSQL = "Select " +
                    "CASE WHEN IND.moralphy = 'P' THEN IND.PRENOM || ' ' || IND.NOMCOMPL || ' ' ELSE '' END || IND.NOM || CASE WHEN IND.moralphy = 'P' THEN ' ' || IND.STR33 ELSE '' END || ' ' || CASE WHEN IND.moralphy = 'M' THEN IND.GENRE ELSE '' END Cliente, " +
                    "IND.TVA RFC, " +
                    "Sum(tabla.Rentas_Devengar) Rentas_Devengar, " +
                    "Sum(tabla.RENTAS_DEVENGAR + ta.RESIDUAL_VALUE) Rentas_Devengar_Residual, " +
                    "Sum(tabla.SALDO_INS_CAPITAL + ta.RESIDUAL_VALUE) Riesgo_Expuesto, " +
                    "NVL(Sum(OCon.Otros_Conceptos), 0) Otros_Conceptos_Vencidos, " +
                    "NVL(Sum(Venc.Rentas_Vencidas), 0) Total_Exigible " +
                "from imxdb.G_DOSSIER DOSS " +
                    "inner join(Select REFTYPE, REFDOSS, REFINDIVIDU From imxdb.T_INTERVENANTS Where NVL(reftype,'DB') In('DB')) T on t.refdoss = DOSS.refdoss " +
                    "inner join(Select MORALPHY, PRENOM, NOMCOMPL, NOM, STR33, GENRE, TVA, REFINDIVIDU From imxdb.G_INDIVIDU) IND on t.refindividu = IND.refindividu " +
                        "inner join(select c.refdoss_reqst, max(c.imx_un_id) imx_un_id from imxdb.T_FIN_AMO c inner join (" +
                            "Select IMX_UN_ID, TYPE, STR3, DT04_DT From IMXDB.g_piecedet Where TYPE = 'ANNEXE_DEMANDE_FIN' AND STR3 = 'ACT')det on det.imx_un_id = c.ANNEX_ID " +
                                "where det.DT04_DT < (TRUNC(sysdate, 'DD') + 1) and(flag_active is null or flag_active = 'O')" +
                            "group by c.refdoss_reqst) uvt on uvt.refdoss_reqst = DOSS.refdoss " +
                    "inner join(Select IMX_UN_ID, REFDOSS_REQST, ANNEX_ID, RESIDUAL_VALUE From imxdb.T_FIN_AMO) ta on ta.refdoss_reqst = DOSS.refdoss and ta.imx_un_id = uvt.imx_un_id " +
                    "inner join(select distinct his.fin_amort_id" +
                        ", SUM(case when his.FEC_FACTURADO >= (TRUNC(sysdate, 'DD') + 1) THEN ROUND(his.Capital, 2) ELSE 0 END) SALDO_INS_CAPITAL" +
                        ", count(his.instal_number) PLAZO" +
                        ", MIN(his.instal_due_dt) VEN_PRIMER_RENTA" +
                        ", SUM(case when his.FEC_FACTURADO >= (TRUNC(sysdate, 'DD') + 1) THEN ROUND((his.Capital + his.Interes), 2) ELSE 0 END) RENTAS_DEVENGAR " +
                            "from(" +
                            "select amort.fin_amort_id, amort.instal_due_dt, amort.instal_number, amort.capital_installment Capital, amort.interest_installment interes, nvl(ent.ER_DAT_DT, amort.instal_due_dt) FEC_FACTURADO " +
                            "from imxdb.t_amort_histo amort " +
                            "left join(" +
                                "select elem.refelem, entx.er_dat_dt from imxdb.g_elemfi elem " +
                                "inner join (select d.df_num, d.df_rel from imxdb.f_detfac d) det on det.df_num = elem.LIBELLE_20_2 " +
                                "inner join(select e.er_num, e.ER_DAT_DT from imxdb.f_entrel e) entx on entx.er_num = det.df_rel " +
                                "where elem.libelle_20_3 = 'LOY'" +
                            ") ent on ent.refelem = amort.refelem_fi_inst " +
                        "union all " +
                        "select amort.fin_amort_id, amort.instal_due_dt, amort.instal_number, amort.capital_installment Capital, amort.interest_installment interes, nvl(ent.ER_DAT_DT, amort.instal_due_dt) FEC_FACTURADO " +
                        "from imxdb.T_AMORT_INSTAL amort " +
                        "left join (select elem.refelem, entx.er_dat_dt from imxdb.g_elemfi elem " +
                            "inner join (select d.df_num, d.df_rel from imxdb.f_detfac d) det on det.df_num = elem.LIBELLE_20_2 " +
                            "inner join(select e.er_num, e.ER_DAT_DT from imxdb.f_entrel e) entx on entx.er_num = det.df_rel " +
                        "where elem.libelle_20_3 = 'LOY' " +
                        ") ent on ent.refelem = amort.refelem_fi_inst " +
                        ") his " +
                    "group by his.fin_amort_id " +
                    ") tabla on tabla.fin_amort_id = uvt.imx_un_id " +
                "Left Join(" +
                    "(Select df_dos, Rentas_Vencidas, Saldo_Vencido_Capital, ER_REG_DT from" +
                        "(Select df_dos, F.ER_TDB Rentas_Vencidas, F.ER_TDB Saldo_Vencido_Capital, er_reg_dt ER_REG_DT from imxdb.F_ENTREL F " +
                           "inner join (Select LIBELLE From imxdb.G_ELEMFI) g on f.er_refext1 = g.libelle " +
                           "inner join(Select DF_REL, DF_NOM, DF_DOS, DF_INV_GROUP From imxdb.F_DETFAC) d on f.er_num = d.df_rel " +
                           "where F.ER_TDB_MVT > NVL(F.ER_PAYE_MVT, 0) and upper(d.df_nom) = 'LOY' " +
                           "order by er_reg_dt) where rownum = 1)" +
                    ") Venc On Venc.df_dos = DOSS.REFDOSS " +
                "Left Join((Select df_dos, Otros_Conceptos from " +
                    "(Select df_dos, F.ER_TDB Otros_Conceptos from imxdb.F_ENTREL F " +
                        "inner join (Select LIBELLE From imxdb.G_ELEMFI) g on f.er_refext1 = g.libelle " +
                        "inner join(Select DF_REL, DF_NOM, DF_DOS, DF_INV_GROUP From imxdb.F_DETFAC) d on f.er_num = d.df_rel " +
                    "where F.ER_TDB_MVT > NVL(F.ER_PAYE_MVT, 0) and d.DF_INV_GROUP = 'C' " +
                    "order by er_reg_dt) where rownum = 1)" +
                ") OCon On OCon.df_dos = DOSS.REFDOSS " +
                "where doss.categdoss LIKE 'FINANCING REQUEST%' And IND.TVA = '" + RFC +
                "' Group By IND.TVA, IND.moralphy, IND.PRENOM, IND.NOMCOMPL, IND.NOM, IND.STR33, IND.GENRE";

                oracleComando.CommandText = ConsultaSQL;

                OracleDataReader dataReader = oracleComando.ExecuteReader();

                Riesgo r = new Riesgo();
                while (dataReader.Read())
                {
                    r = new Riesgo
                    {
                        cliente = dataReader["Cliente"].ToString(),
                        rfc = dataReader["RFC"].ToString(),
                        rentas_devengar = Convert.ToDouble(dataReader["Rentas_Devengar"]),
                        rentas_devengar_residual = Convert.ToDouble(dataReader["Rentas_Devengar_Residual"]),
                        riesgo_expuesto = Convert.ToDouble(dataReader["Riesgo_Expuesto"]),
                        otros_conceptos_vencidos = Convert.ToDouble(dataReader["Otros_Conceptos_Vencidos"]),
                        total_exigible = Convert.ToDouble(dataReader["Total_Exigible"]),
                    };
                    this.listaRiesgos.Add(r);
                }

                dataReader.Close();
                oracleComando.Dispose();
                oracleConexion.Close();
            }
            catch (Exception)
            {
                Exception newEx = new Exception("Error al consultar.");
                throw newEx;
            }

            return listaRiesgos;
        }

        [HttpGet]
        [Route("api/arrendamientos")]
        [Authorize]
        // GET: Arrendamientos
        public List<Riesgo> GetArrendamientos(string Contrato)
        {
            try
            {
                var path = AppDomain.CurrentDomain.BaseDirectory;

                // create connection
                OracleConnection oracleConexion = new OracleConnection();

                // create connection string using builder
                OracleConnectionStringBuilder sqlConexionString = new OracleConnectionStringBuilder();
                sqlConexionString.ConnectionString = ConfigurationManager.ConnectionStrings["ABCLeasingOracle"].ConnectionString;

                // connect
                oracleConexion.ConnectionString = sqlConexionString.ConnectionString;
                oracleConexion.Open();
                Console.WriteLine("Connection established (" + oracleConexion.ServerVersion + ")");

                OracleCommand oracleComando = oracleConexion.CreateCommand();
                oracleComando.CommandType = CommandType.Text;
                oracleComando.Connection = oracleConexion;

                string ConsultaSQL = "select CASE WHEN IND.moralphy = 'P' THEN IND.PRENOM || ' ' || IND.NOMCOMPL || ' ' ELSE '' END || IND.NOM || CASE WHEN IND.moralphy = 'P' THEN ' ' || IND.STR33 ELSE '' END || ' ' || CASE WHEN IND.moralphy = 'M' THEN IND.GENRE ELSE '' END Cliente, " +
                        "IND.TVA RFC, tabla.Rentas_Devengar Rentas_Devengar, tabla.RENTAS_DEVENGAR + ta.RESIDUAL_VALUE Rentas_Devengar_Residual, " +
                        "tabla.SALDO_INS_CAPITAL + ta.RESIDUAL_VALUE Riesgo_Expuesto, NVL(OCon.Otros_Conceptos, 0) Otros_Conceptos_Vencidos, " +
                        "NVL(Venc.Rentas_Vencidas, 0) Total_Exigible " +
                    "from imxdb.G_DOSSIER DOSS " +
                    "inner join(Select REFTYPE, REFDOSS, REFINDIVIDU From imxdb.T_INTERVENANTS Where NVL(reftype,'DB') In('DB')) T on t.refdoss = DOSS.refdoss " +
                    "inner join(Select MORALPHY, PRENOM, NOMCOMPL, NOM, STR33, GENRE, TVA, REFINDIVIDU From imxdb.G_INDIVIDU) IND on t.refindividu = IND.refindividu " +
                    "inner join(select c.refdoss_reqst, max(c.imx_un_id) imx_un_id from imxdb.T_FIN_AMO c inner join (" +
                        "Select IMX_UN_ID, TYPE, STR3, DT04_DT From IMXDB.g_piecedet Where TYPE = 'ANNEXE_DEMANDE_FIN' AND STR3 = 'ACT')det on det.imx_un_id = c.ANNEX_ID " +
                    "where det.DT04_DT < (TRUNC(sysdate, 'DD') + 1) and(flag_active is null or flag_active = 'O') " +
                    "group by c.refdoss_reqst) uvt on uvt.refdoss_reqst = DOSS.refdoss " +
                    "inner join(Select IMX_UN_ID, REFDOSS_REQST, ANNEX_ID, RESIDUAL_VALUE From imxdb.T_FIN_AMO) ta on ta.refdoss_reqst = DOSS.refdoss and ta.imx_un_id = uvt.imx_un_id " +
                    "inner join(select distinct his.fin_amort_id, SUM(case when his.FEC_FACTURADO >= (TRUNC(sysdate, 'DD') + 1) THEN ROUND(his.Capital, 2) ELSE 0 END) SALDO_INS_CAPITAL" +
                        ", count(his.instal_number) PLAZO, MIN(his.instal_due_dt) VEN_PRIMER_RENTA" +
                        ", SUM(case when his.FEC_FACTURADO >= (TRUNC(sysdate, 'DD') + 1) THEN ROUND((his.Capital + his.Interes), 2) ELSE 0 END) RENTAS_DEVENGAR " +
                            "from(select amort.fin_amort_id, amort.instal_due_dt, amort.instal_number, amort.capital_installment Capital, amort.interest_installment interes, nvl(ent.ER_DAT_DT, amort.instal_due_dt) FEC_FACTURADO " +
                                "from imxdb.t_amort_histo amort " +
                                "left join(select elem.refelem, entx.er_dat_dt from imxdb.g_elemfi elem " +
                                    "inner join (select d.df_num, d.df_rel from imxdb.f_detfac d) det on det.df_num = elem.LIBELLE_20_2 " +
                                    "inner join(select e.er_num, e.ER_DAT_DT from imxdb.f_entrel e) entx on entx.er_num = det.df_rel " +
                                "where elem.libelle_20_3 = 'LOY') ent on ent.refelem = amort.refelem_fi_inst " +
                            "union all " +
                                "select amort.fin_amort_id, amort.instal_due_dt, amort.instal_number, amort.capital_installment Capital, amort.interest_installment interes, nvl(ent.ER_DAT_DT, amort.instal_due_dt) FEC_FACTURADO " +
                                "from imxdb.T_AMORT_INSTAL amort " +
                                "left join(select elem.refelem, entx.er_dat_dt from imxdb.g_elemfi elem " +
                                    "inner join (select d.df_num, d.df_rel from imxdb.f_detfac d) det on det.df_num = elem.LIBELLE_20_2 " +
                                    "inner join(select e.er_num, e.ER_DAT_DT from imxdb.f_entrel e) entx on entx.er_num = det.df_rel " +
                                "where elem.libelle_20_3 = 'LOY') ent on ent.refelem = amort.refelem_fi_inst" +
                            ") his " +
                            "group by his.fin_amort_id" +
                        ") tabla on tabla.fin_amort_id = uvt.imx_un_id " +
                    "Left Join((Select df_dos, Rentas_Vencidas, Saldo_Vencido_Capital, ER_REG_DT from" +
                        "(Select df_dos, F.ER_TDB Rentas_Vencidas, F.ER_TDB Saldo_Vencido_Capital, er_reg_dt ER_REG_DT " +
                        "from imxdb.F_ENTREL F " +
                            "inner join (Select LIBELLE From imxdb.G_ELEMFI) g on f.er_refext1 = g.libelle " +
                            "inner join(Select DF_REL, DF_NOM, DF_DOS, DF_INV_GROUP From imxdb.F_DETFAC) d on f.er_num = d.df_rel " +
                        "where F.ER_TDB_MVT > NVL(F.ER_PAYE_MVT, 0) and upper(d.df_nom) = 'LOY' " +
                    "order by er_reg_dt) where rownum = 1)) Venc On Venc.df_dos = DOSS.REFDOSS " +
                    "Left Join((Select df_dos, Otros_Conceptos from" +
                        "(Select df_dos, F.ER_TDB Otros_Conceptos " +
                        "from imxdb.F_ENTREL F " +
                            "inner join (Select LIBELLE From imxdb.G_ELEMFI) g on f.er_refext1 = g.libelle " +
                            "inner join(Select DF_REL, DF_NOM, DF_DOS, DF_INV_GROUP From imxdb.F_DETFAC) d on f.er_num = d.df_rel " +
                        "where F.ER_TDB_MVT > NVL(F.ER_PAYE_MVT, 0) and d.DF_INV_GROUP = 'C' " +
                        "order by er_reg_dt) where rownum = 1)" +
                    ") OCon On OCon.df_dos = DOSS.REFDOSS " +
                "where doss.categdoss LIKE 'FINANCING REQUEST%' And DOSS.ANCREFDOSS = '" + Contrato + "'";

                oracleComando.CommandText = ConsultaSQL;

                OracleDataReader dataReader = oracleComando.ExecuteReader();

                Riesgo r = new Riesgo();
                while (dataReader.Read())
                {
                    r = new Riesgo
                    {
                        cliente = dataReader["Cliente"].ToString(),
                        rfc = dataReader["RFC"].ToString(),
                        rentas_devengar = Convert.ToDouble(dataReader["Rentas_Devengar"]),
                        rentas_devengar_residual = Convert.ToDouble(dataReader["Rentas_Devengar_Residual"]),
                        riesgo_expuesto = Convert.ToDouble(dataReader["Riesgo_Expuesto"]),
                        otros_conceptos_vencidos = Convert.ToDouble(dataReader["Otros_Conceptos_Vencidos"]),
                        total_exigible = Convert.ToDouble(dataReader["Total_Exigible"]),
                    };
                    this.listaRiesgos.Add(r);
                }

                dataReader.Close();
                oracleComando.Dispose();
                oracleConexion.Close();
            }
            catch (Exception)
            {
                Exception newEx = new Exception("Error al consultar.");
                throw newEx;
            }

            return listaRiesgos;
        }

        [HttpPut]
        [Route("api/update_client")]
        [Authorize]
        public IHttpActionResult PutClientes([FromBody] ClienteImx put_cliente)
        {
            try
            {
                AES aES = new AES();

                var sqlConexionString = ConfigurationManager.ConnectionStrings["ABCLeasingABC"].ConnectionString;
                using (SqlConnection sqlConexion = new SqlConnection(sqlConexionString))
                {
                    //Guarda la solicitud de Modficación de Clientes
                    string sqlConsulta = "INSERT INTO ActualizarClientesImx (rfc, email_notificacion, email_facturacion, telefono, celular, revisar, enviar) " +
                        "VALUES (@rfc, @email_notificacion, @email_facturacion, @telefono, @celular, @revisar, @enviar)";

                    sqlConexion.Open();
                    SqlCommand sqlComandoInsert = new SqlCommand(sqlConsulta, sqlConexion);

                    sqlComandoInsert.Parameters.AddWithValue("@rfc", put_cliente.rfc.ToUpper());
                    sqlComandoInsert.Parameters.AddWithValue("@email_notificacion", put_cliente.email_notificacion.ToLower());
                    sqlComandoInsert.Parameters.AddWithValue("@email_facturacion", put_cliente.email_facturacion.ToLower());
                    sqlComandoInsert.Parameters.AddWithValue("@telefono", put_cliente.telefono.ToString());
                    sqlComandoInsert.Parameters.AddWithValue("@celular", put_cliente.celular.ToString());
                    sqlComandoInsert.Parameters.AddWithValue("@revisar", false);
                    sqlComandoInsert.Parameters.AddWithValue("@enviar", false);

                    sqlComandoInsert.ExecuteNonQuery();
                    //#end Guardar
                    sqlConexion.Close();
                    return Json(new
                    {
                        message = "Actualización del Cliente Registrada Correctamente"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        [Route("api/get_clients_authorize")]
        [Authorize]
        public List<ClienteImx> GetClientAuthorize()
        {
            try
            {
                var sqlConexionString = ConfigurationManager.ConnectionStrings["ABCLeasingABC"].ConnectionString;
                using (SqlConnection sqlConexion = new SqlConnection(sqlConexionString))
                {
                    string sqlConsulta = "SELECT * FROM ActualizarClientesImx WHERE revisar = 0 And enviar = 0";

                    sqlConexion.Open();

                    SqlCommand sqlComandoSelect = new SqlCommand(sqlConsulta, sqlConexion);

                    SqlDataReader sqlDatos = sqlComandoSelect.ExecuteReader();

                    ClienteImx c = new ClienteImx();

                    while (sqlDatos.Read())
                    {
                        c = new ClienteImx
                        {
                            rfc = sqlDatos["rfc"].ToString(),
                            email_notificacion = sqlDatos["email_notificacion"].ToString(),
                            email_facturacion = sqlDatos["email_facturacion"].ToString(),
                            telefono = sqlDatos["telefono"].ToString(),
                            celular = sqlDatos["celular"].ToString()
                        };
                        this.listaClientesImx.Add(c);
                    }

                    sqlDatos.Close();
                    sqlComandoSelect.Dispose();
                    sqlConexion.Close();
                }
            }
            catch (Exception)
            {
                Exception newEx = new Exception("Error al consultar");
                throw newEx;
            }

            return listaClientesImx;
        }

        [HttpPut]
        [Route("api/update_client/authorize")]
        [Authorize]
        public IHttpActionResult PutClientesRevisar(string RFC)
        {
            try
            {
                AES aES = new AES();

                var sqlConexionString = ConfigurationManager.ConnectionStrings["ABCLeasingABC"].ConnectionString;
                using (SqlConnection sqlConexion = new SqlConnection(sqlConexionString))
                {
                    //Buscar la solicitud de Modificación de Clientes
                    string sqlConsulta = "SELECT * FROM ActualizarClientesImx WHERE UPPER(RFC) = '" + RFC.ToUpper() + "'";

                    sqlConexion.Open();

                    SqlCommand sqlComandoSelect = new SqlCommand(sqlConsulta, sqlConexion);

                    SqlDataReader sqlDatos = sqlComandoSelect.ExecuteReader();

                    //Si Existe
                    if (sqlDatos.Read())
                    {
                        sqlConexion.Close();
                        //Autoriza la Modificación de Clientes
                        sqlConsulta = "UPDATE ActualizarClientesImx Set revisar=1 Where rfc = @rfc";

                        sqlConexion.Open();
                        SqlCommand sqlComandoInsert = new SqlCommand(sqlConsulta, sqlConexion);

                        sqlComandoInsert.Parameters.AddWithValue("@rfc", RFC.ToUpper());

                        sqlComandoInsert.ExecuteNonQuery();
                        //#end Update
                        sqlConexion.Close();
                        return Json(new
                        {
                            message = "Modificación del Cliente Autorizada"
                        });
                    }
                    else
                    {
                        sqlConexion.Close();
                        return Json(new
                        {
                            message = "No Existe algún registro de Modificación"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        [Route("api/get_clients_send")]
        [Authorize]
        public List<ClienteImx> GetClientSend()
        {
            try
            {
                var sqlConexionString = ConfigurationManager.ConnectionStrings["ABCLeasingABC"].ConnectionString;
                using (SqlConnection sqlConexion = new SqlConnection(sqlConexionString))
                {
                    string sqlConsulta = "SELECT * FROM ActualizarClientesImx WHERE revisar = 1 And enviar = 0";

                    sqlConexion.Open();

                    SqlCommand sqlComandoSelect = new SqlCommand(sqlConsulta, sqlConexion);

                    SqlDataReader sqlDatos = sqlComandoSelect.ExecuteReader();

                    ClienteImx c = new ClienteImx();

                    while (sqlDatos.Read())
                    {
                        c = new ClienteImx
                        {
                            rfc = sqlDatos["rfc"].ToString(),
                            email_notificacion = sqlDatos["email_notificacion"].ToString(),
                            email_facturacion = sqlDatos["email_facturacion"].ToString(),
                            telefono = sqlDatos["telefono"].ToString(),
                            celular = sqlDatos["celular"].ToString()
                        };
                        this.listaClientesImx.Add(c);
                    }

                    sqlDatos.Close();
                    sqlComandoSelect.Dispose();
                    sqlConexion.Close();
                }
            }
            catch (Exception)
            {
                Exception newEx = new Exception("Error al consultar");
                throw newEx;
            }

            return listaClientesImx;
        }

        [HttpPut]
        [Route("api/update_client/send")]
        [Authorize]
        public IHttpActionResult PutClientesEnviar(string RFC)
        {
            try
            {
                AES aES = new AES();

                var sqlConexionString = ConfigurationManager.ConnectionStrings["ABCLeasingABC"].ConnectionString;
                using (SqlConnection sqlConexion = new SqlConnection(sqlConexionString))
                {
                    //Buscar la solicitud de Modificación de Clientes
                    string sqlConsulta = "SELECT * FROM ActualizarClientesImx WHERE UPPER(RFC) = '" + RFC.ToUpper() + "'";

                    sqlConexion.Open();

                    SqlCommand sqlComandoSelect = new SqlCommand(sqlConsulta, sqlConexion);

                    SqlDataReader sqlDatos = sqlComandoSelect.ExecuteReader();

                    //Si Existe
                    if (sqlDatos.Read())
                    {
                        sqlConexion.Close();
                        //Autoriza la Modificación de Clientes
                        sqlConsulta = "UPDATE ActualizarClientesImx Set enviar=1 Where rfc = @rfc";

                        sqlConexion.Open();
                        SqlCommand sqlComandoInsert = new SqlCommand(sqlConsulta, sqlConexion);

                        sqlComandoInsert.Parameters.AddWithValue("@rfc", RFC.ToUpper());

                        sqlComandoInsert.ExecuteNonQuery();
                        //#end Update
                        sqlConexion.Close();
                        return Json(new
                        {
                            message = "Modificación del Cliente Enviada"
                        });
                    }
                    else
                    {
                        sqlConexion.Close();
                        return Json(new
                        {
                            message = "No Existe algún registro para enviar"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        [Route("api/concentrado_contratos")]
        [Authorize]
        // GET: Concentrado_Contratos
        public List<ContratoConcentrado> GetConcentradoContratos(string RFC)
        {
            try
            {
                var path = AppDomain.CurrentDomain.BaseDirectory;

                // create connection
                OracleConnection oracleConexion = new OracleConnection();

                // create connection string using builder
                OracleConnectionStringBuilder sqlConexionString = new OracleConnectionStringBuilder();
                sqlConexionString.ConnectionString = ConfigurationManager.ConnectionStrings["ABCLeasingOracle"].ConnectionString;

                // connect
                oracleConexion.ConnectionString = sqlConexionString.ConnectionString;
                oracleConexion.Open();
                Console.WriteLine("Connection established (" + oracleConexion.ServerVersion + ")");

                OracleCommand oracleComando = oracleConexion.CreateCommand();
                oracleComando.CommandType = CommandType.Text;
                oracleComando.Connection = oracleConexion;

                string ConsultaSQL = "Select CONTRATO," +
                    "    SUM(CASE WHEN FEC_FACTURADO < TO_DATE('01/01/19', 'DD/MM/YY') THEN DIAS_ATRASO ELSE 0 END) RENTAS_MENORES_2018," +
                    "    SUM(CASE WHEN FEC_YEAR = 2019 AND(FEC_MONTH = 1 OR FEC_MONTH = 2 OR FEC_MONTH = 3 OR FEC_MONTH = 4 OR FEC_MONTH = 5 OR FEC_MONTH = 6) THEN ROUND(DIAS_ATRASO / 6, 0) ELSE 0 END) RENTAS_SEMESTRE_1_2019," +
                    "    SUM(CASE WHEN FEC_YEAR = 2019 AND(FEC_MONTH = 7 OR FEC_MONTH = 8 OR FEC_MONTH = 9 OR FEC_MONTH = 10 OR FEC_MONTH = 11 OR FEC_MONTH = 13) THEN ROUND(DIAS_ATRASO / 6, 0) ELSE 0 END) RENTAS_SEMESTRE_2_2019," +
                    "    SUM(CASE WHEN FEC_YEAR = 2020 AND(FEC_MONTH = 1 OR FEC_MONTH = 2 OR FEC_MONTH = 3 OR FEC_MONTH = 4 OR FEC_MONTH = 5 OR FEC_MONTH = 6) THEN ROUND(DIAS_ATRASO / 6, 0) ELSE 0 END) RENTAS_SEMESTRE_1_2020," +
                    "    SUM(CASE WHEN FEC_YEAR = 2020 AND(FEC_MONTH = 7 OR FEC_MONTH = 8 OR FEC_MONTH = 9 OR FEC_MONTH = 10 OR FEC_MONTH = 11 OR FEC_MONTH = 13) THEN ROUND(DIAS_ATRASO / 6, 0) ELSE 0 END) RENTAS_SEMESTRE_2_2020," +
                    "    SUM(CASE WHEN FEC_YEAR = 2021 AND(FEC_MONTH = 1 OR FEC_MONTH = 2 OR FEC_MONTH = 3 OR FEC_MONTH = 4 OR FEC_MONTH = 5 OR FEC_MONTH = 6) THEN ROUND(DIAS_ATRASO / 6, 0) ELSE 0 END) RENTAS_SEMESTRE_1_2021," +
                    "    SUM(CASE WHEN FEC_YEAR = 2021 AND(FEC_MONTH = 7 OR FEC_MONTH = 8 OR FEC_MONTH = 9 OR FEC_MONTH = 10 OR FEC_MONTH = 11 OR FEC_MONTH = 13) THEN ROUND(DIAS_ATRASO / 6, 0) ELSE 0 END) RENTAS_SEMESTRE_2_2021," +
                    "    SUM(CASE WHEN FEC_YEAR = 2022 AND(FEC_MONTH = 1 OR FEC_MONTH = 2 OR FEC_MONTH = 3 OR FEC_MONTH = 4 OR FEC_MONTH = 5 OR FEC_MONTH = 6) THEN ROUND(DIAS_ATRASO / 6, 0) ELSE 0 END) RENTAS_SEMESTRE_1_2022," +
                    "    SUM(CASE WHEN FEC_YEAR = 2022 AND(FEC_MONTH = 7 OR FEC_MONTH = 8 OR FEC_MONTH = 9 OR FEC_MONTH = 10 OR FEC_MONTH = 11 OR FEC_MONTH = 13) THEN ROUND(DIAS_ATRASO / 6, 0) ELSE 0 END) RENTAS_SEMESTRE_2_2022," +
                    "    SUM(CASE WHEN FEC_YEAR = 2023 AND(FEC_MONTH = 1 OR FEC_MONTH = 2 OR FEC_MONTH = 3 OR FEC_MONTH = 4 OR FEC_MONTH = 5 OR FEC_MONTH = 6) THEN ROUND(DIAS_ATRASO / 6, 0) ELSE 0 END) RENTAS_SEMESTRE_1_2023," +
                    "    SUM(CASE WHEN FEC_YEAR = 2023 AND(FEC_MONTH = 7 OR FEC_MONTH = 8 OR FEC_MONTH = 9 OR FEC_MONTH = 10 OR FEC_MONTH = 11 OR FEC_MONTH = 13) THEN ROUND(DIAS_ATRASO / 6, 0) ELSE 0 END) RENTAS_SEMESTRE_2_2023," +
                    "    SUM(CASE WHEN FEC_YEAR = 2024 AND(FEC_MONTH = 1 OR FEC_MONTH = 2 OR FEC_MONTH = 3 OR FEC_MONTH = 4 OR FEC_MONTH = 5 OR FEC_MONTH = 6) THEN ROUND(DIAS_ATRASO / 6, 0) ELSE 0 END) RENTAS_SEMESTRE_1_2024," +
                    "    SUM(CASE WHEN FEC_YEAR = 2024 AND(FEC_MONTH = 7 OR FEC_MONTH = 8 OR FEC_MONTH = 9 OR FEC_MONTH = 10 OR FEC_MONTH = 11 OR FEC_MONTH = 13) THEN ROUND(DIAS_ATRASO / 6, 0) ELSE 0 END) RENTAS_SEMESTRE_2_2024," +
                    "    SUM(CASE WHEN FEC_YEAR = 2025 AND(FEC_MONTH = 1 OR FEC_MONTH = 2 OR FEC_MONTH = 3 OR FEC_MONTH = 4 OR FEC_MONTH = 5 OR FEC_MONTH = 6) THEN ROUND(DIAS_ATRASO / 6, 0) ELSE 0 END) RENTAS_SEMESTRE_1_2025," +
                    "    SUM(CASE WHEN FEC_YEAR = 2025 AND(FEC_MONTH = 7 OR FEC_MONTH = 8 OR FEC_MONTH = 9 OR FEC_MONTH = 10 OR FEC_MONTH = 11 OR FEC_MONTH = 13) THEN ROUND(DIAS_ATRASO / 6, 0) ELSE 0 END) RENTAS_SEMESTRE_2_2025," +
                    "    SUM(CASE WHEN FEC_YEAR = 2026 AND(FEC_MONTH = 1 OR FEC_MONTH = 2 OR FEC_MONTH = 3 OR FEC_MONTH = 4 OR FEC_MONTH = 5 OR FEC_MONTH = 6) THEN ROUND(DIAS_ATRASO / 6, 0) ELSE 0 END) RENTAS_SEMESTRE_1_2026," +
                    "    SUM(CASE WHEN FEC_YEAR = 2026 AND(FEC_MONTH = 7 OR FEC_MONTH = 8 OR FEC_MONTH = 9 OR FEC_MONTH = 10 OR FEC_MONTH = 11 OR FEC_MONTH = 13) THEN ROUND(DIAS_ATRASO / 6, 0) ELSE 0 END) RENTAS_SEMESTRE_2_2026" +
                    " From (" +
                    "SELECT DOSS.ANCREFDOSS CONTRATO," +
                    "    NVL((SELECT MAX(TRUNC(V2.DTJOUR_DT)) MONTOPAGO FROM IMXDB.G_VENELEM V2 WHERE V2.REFELEM = EL.REFELEM), TRUNC(SYSDATE)) - TRUNC(FE.ER_DAT_DT)  DIAS_ATRASO," +
                    "    TRIM(to_char(FE.ER_REG_DT, 'MM')) FEC_MONTH," +
                    "    TRIM(to_char(FE.ER_REG_DT, 'YYYY')) FEC_YEAR," +
                    "    FE.ER_REG_DT FEC_FACTURADO" +
                    " FROM IMXDB.F_ENTREL FE" +
                    "    INNER JOIN IMXDB.F_DETFAC DE ON FE.ER_NUM = DE.DF_REL" +
                    "    INNER JOIN IMXDB.G_ELEMFI EL ON(EL.LIBELLE_20_2 = DE.DF_NUM AND  EL.REFDOSS = DE.DF_DOS)" +
                    "    INNER JOIN IMXDB.G_DOSSIER DOSS ON EL.REFDOSS = DOSS.REFDOSS" +
                    "    INNER JOIN(Select refdoss, reftype, refindividu From imxdb.T_INTERVENANTS) T on t.refdoss = DOSS.refdoss and T.reftype = 'DB'" +
                    "    INNER JOIN(Select TVA, refindividu From imxdb.G_INDIVIDU) IND on t.refindividu = IND.refindividu" +
                    "    LEFT JOIN(SELECT TFA.REFDOSS_REQST, TFA.IMX_UN_ID, TFA.FLAG_ACTIVE, TAI.REFELEM_FI_INT," +
                    "            TAI.FIN_AMORT_ID, TAI.INTEREST_INSTALLMENT, TAI.CAPITAL_INSTALLMENT, TAI.INSTAL_DUE_DT, TAI.REFELEM_FI_INST, TAI.REFELEM_FI_INST_ADM, TAI.REFELEM_FI_INST_LEAS, TAI.FG_IS_INVOICED, 'I' TABLA_ORIG,TAI.REFELEM_FI_CAP, TAI.INSTAL_NUMBER" +
                    "        FROM IMXDB.T_FIN_AMO TFA" +
                    "            INNER JOIN IMXDB.T_AMORT_INSTAL TAI ON TFA.IMX_UN_ID = TAI.FIN_AMORT_ID" +
                    "        WHERE TFA.FLAG_ACTIVE = 'O') TA ON(DE.DF_DOS = TA.REFDOSS_REQST AND TA.REFELEM_FI_INST = EL.REFELEM)" +
                    "    LEFT JOIN(SELECT TFA.REFDOSS_REQST, TFA.IMX_UN_ID, TFA.FLAG_ACTIVE," +
                    "            TAI.FIN_AMORT_ID, TAI.INTEREST_INSTALLMENT, TAI.CAPITAL_INSTALLMENT, TAI.INSTAL_DUE_DT, TAI.REFELEM_FI_INST, TAI.REFELEM_FI_INST_ADM, TAI.REFELEM_FI_INST_LEAS, TAI.FG_IS_INVOICED, TAI.REFELEM_FI_CAP, TAI.REFELEM_FI_INT, TAI.INSTAL_NUMBER" +
                    "        FROM IMXDB.T_FIN_AMO TFA" +
                    "            INNER JOIN IMXDB.T_AMORT_HISTO TAI ON TFA.IMX_UN_ID = TAI.FIN_AMORT_ID) TA_H ON(DE.DF_DOS = TA_H.REFDOSS_REQST AND TA_H.REFELEM_FI_INST = EL.REFELEM)" +
                    "    LEFT JOIN(SELECT DDD.ER_REFEXT1 AS FOLIO_FACT, DDD.FEC_CANCELACION AS FECHA_CANC" +
                    "            FROM (SELECT DISTINCT CCC.ER_REFEXT1, CCC.FEC_CANCELACION, RANK() OVER (PARTITION BY  CCC.ER_REFEXT1 ORDER BY CCC.FEC_CANCELACION ) NUM_CANCELACION" +
                    "                FROM(SELECT DISTINCT F.ER_REFEXT1, max(DET.DF_ANN_DT) FEC_CANCELACION" +
                    "                    FROM imxdb.F_ENTREL f, imxdb.F_ENTREL e, imxdb.f_detfac det" +
                    "                    WHERE F.er_num = e.ER_ORG_INVOICE_REF and det.df_rel = e.er_num and e.ER_ORG_INVOICE_REF IS NOT NULL and E.ER_TCR > 0" +
                    "                    GROUP BY F.ER_REFEXT1" +
                    "                    UNION ALL" +
                    "                    SELECT DISTINCT ER.ER_REFEXT1, MIN(DF.DF_ANN_DT) FEC_CANCELACION" +
                    "                    FROM IMXDB.F_ENTREL ER INNER JOIN IMXDB.F_DETFAC DF ON(ER.ER_NUM = DF.DF_REL)" +
                    "                    WHERE DF.DF_ANN IS NOT NULL AND DF.DF_SEN = 'D' AND DF.DF_ANN_DT > '31/12/2019' AND ER.ER_REFEXT1 IS NOT NULL" +
                    "                    GROUP BY ER.ER_REFEXT1" +
                    "                ) CCC" +
                    "            ) DDD" +
                    "            WHERE DDD.NUM_CANCELACION = 1" +
                    "                AND FEC_CANCELACION<TRUNC(sysdate,'DD')    ) CANC ON CANC.FOLIO_FACT = FE.ER_REFEXT1" +
                    " WHERE FE.ER_NUM NOT IN(SELECT FE.ER_NUM" +
                    "    FROM IMXDB.F_ENTREL FE, imxdb.F_ENTREL E" +
                    "    WHERE FE.ER_NUM = E.ER_ORG_INVOICE_REF" +
                    "        AND E.ER_ORG_INVOICE_REF IS NOT NULL AND E.ER_TCR > 0)" +
                    "    AND CANC.FECHA_CANC IS NULL" +
                    "    AND FE.ER_REFEXT1 IS NOT NULL" +
                    "    AND DE.DF_INV_GROUP = 'A'" +
                    "    AND DE.DF_NOM IN('LOY','FRAD','FRLO')" +
                    "    AND IND.TVA = '" + RFC + "'" +
                    " ORDER BY NVL(TA.INSTAL_NUMBER, TA_H.INSTAL_NUMBER)" +
                    ")" +
                    " GROUP BY CONTRATO";
                oracleComando.CommandText = ConsultaSQL;

                OracleDataReader dataReader = oracleComando.ExecuteReader();

                ContratoConcentrado c = new ContratoConcentrado();
                while (dataReader.Read())
                {
                    c = new ContratoConcentrado
                    {
                        contrato = dataReader["CONTRATO"].ToString(),
                        periodo01 = Convert.ToInt32(dataReader["RENTAS_MENORES_2018"]),
                        periodo02 = Convert.ToInt32(dataReader["RENTAS_SEMESTRE_1_2019"]),
                        periodo03 = Convert.ToInt32(dataReader["RENTAS_SEMESTRE_2_2019"]),
                        periodo04 = Convert.ToInt32(dataReader["RENTAS_SEMESTRE_1_2020"]),
                        periodo05 = Convert.ToInt32(dataReader["RENTAS_SEMESTRE_2_2020"]),
                        periodo06 = Convert.ToInt32(dataReader["RENTAS_SEMESTRE_1_2021"]),
                        periodo07 = Convert.ToInt32(dataReader["RENTAS_SEMESTRE_2_2021"]),
                        periodo08 = Convert.ToInt32(dataReader["RENTAS_SEMESTRE_1_2022"]),
                        periodo09 = Convert.ToInt32(dataReader["RENTAS_SEMESTRE_2_2022"]),
                        periodo10 = Convert.ToInt32(dataReader["RENTAS_SEMESTRE_1_2023"]),
                        periodo11 = Convert.ToInt32(dataReader["RENTAS_SEMESTRE_2_2023"]),
                        periodo12 = Convert.ToInt32(dataReader["RENTAS_SEMESTRE_1_2024"]),
                        periodo13 = Convert.ToInt32(dataReader["RENTAS_SEMESTRE_2_2024"]),
                        periodo14 = Convert.ToInt32(dataReader["RENTAS_SEMESTRE_1_2025"]),
                        periodo15 = Convert.ToInt32(dataReader["RENTAS_SEMESTRE_2_2025"]),
                        periodo16 = Convert.ToInt32(dataReader["RENTAS_SEMESTRE_1_2026"]),
                        periodo17 = Convert.ToInt32(dataReader["RENTAS_SEMESTRE_2_2026"]),
                    };
                    this.listaContratosConcentrados.Add(c);
                }

                dataReader.Close();
                oracleComando.Dispose();
                oracleConexion.Close();
            }
            catch (Exception)
            {
                Exception newEx = new Exception("Error al consultar.");
                throw newEx;
            }

            return listaContratosConcentrados;
        }

        [HttpGet]
        [Route("api/detalle_contrato")]
        [Authorize]
        // GET: Detalle_Contrato
        public List<ContratoDetalle> GetDetalleContrato(string Contrato)
        {
            try
            {
                var path = AppDomain.CurrentDomain.BaseDirectory;

                // create connection
                OracleConnection oracleConexion = new OracleConnection();

                // create connection string using builder
                OracleConnectionStringBuilder sqlConexionString = new OracleConnectionStringBuilder();
                sqlConexionString.ConnectionString = ConfigurationManager.ConnectionStrings["ABCLeasingOracle"].ConnectionString;

                // connect
                oracleConexion.ConnectionString = sqlConexionString.ConnectionString;
                oracleConexion.Open();
                Console.WriteLine("Connection established (" + oracleConexion.ServerVersion + ")");

                OracleCommand oracleComando = oracleConexion.CreateCommand();
                oracleComando.CommandType = CommandType.Text;
                oracleComando.Connection = oracleConexion;

                string ConsultaSQL = "SELECT DOSS.ANCREFDOSS CONTRATO, DE.DF_MONTTC_DOS IMPORTE_FACTURA, " +
                        "NVL((SELECT SUM(V2.MONTANT) MONTOPAGO FROM IMXDB.G_VENELEM V2 WHERE V2.REFELEM = EL.REFELEM),0.00) IMPORTE_PAGADO, " +
                        "NVL((SELECT MAX(TRUNC(V2.DTJOUR_DT)) MONTOPAGO FROM IMXDB.G_VENELEM V2 WHERE V2.REFELEM = EL.REFELEM),TRUNC(SYSDATE)) -TRUNC(FE.ER_DAT_DT)  DIAS_ATRASO, " +
                        "(SELECT VALEUR_ES FROM IMXDB.V_DOMAINE where ABREV = DE.DF_NOM AND TYPE = 'FACTURE') || ' ' || NVL(NVL(TA.INSTAL_NUMBER, TA_H.INSTAL_NUMBER), 0) || '/' || NVL(NVL(UTAV.MAX_INSTAL_NUMBER, UTAV_H.MAX_INSTAL_NUMBER), 0) CONCEPTO, " +
                        "TRIM(to_char(FE.ER_REG_DT, 'Month', 'nls_date_language=spanish')) || '-' || to_char(FE.ER_REG_DT, 'YYYY') PERIODO, " +
                        "FE.ER_REFEXT1 FOLIO, FE.ER_DAT_DT Fecha_emision_factura, FE.ER_REG_DT Fecha_Vencimiento_factura, " +
                        "(SELECT MAX(V2.DTJOUR_DT) FROM IMXDB.G_VENELEM V2 WHERE V2.REFELEM= EL.REFELEM) FECHA_APL_PAGO " +
                    "FROM IMXDB.F_ENTREL FE " +
                        "INNER JOIN IMXDB.F_DETFAC DE ON FE.ER_NUM = DE.DF_REL " +
                        "INNER JOIN IMXDB.G_ELEMFI EL ON(EL.LIBELLE_20_2 = DE.DF_NUM AND  EL.REFDOSS = DE.DF_DOS) " +
                        "INNER JOIN IMXDB.G_DOSSIER DOSS ON EL.REFDOSS = DOSS.REFDOSS " +
                        "LEFT JOIN(SELECT TFA.REFDOSS_REQST, TFA.IMX_UN_ID, TFA.FLAG_ACTIVE, TAI.REFELEM_FI_INT, " +
                                "TAI.FIN_AMORT_ID, TAI.INTEREST_INSTALLMENT, TAI.CAPITAL_INSTALLMENT, TAI.INSTAL_DUE_DT," +
                                " TAI.REFELEM_FI_INST, TAI.REFELEM_FI_INST_ADM, TAI.REFELEM_FI_INST_LEAS, TAI.FG_IS_INVOICED," +
                                " 'I' TABLA_ORIG,TAI.REFELEM_FI_CAP, TAI.INSTAL_NUMBER " +
                            "FROM IMXDB.T_FIN_AMO TFA INNER JOIN IMXDB.T_AMORT_INSTAL TAI ON TFA.IMX_UN_ID = TAI.FIN_AMORT_ID " +
                            "WHERE TFA.FLAG_ACTIVE = 'O') TA ON(DE.DF_DOS = TA.REFDOSS_REQST AND TA.REFELEM_FI_INST = EL.REFELEM) " +
                        "LEFT JOIN(SELECT TFA.REFDOSS_REQST, TFA.IMX_UN_ID, TFA.FLAG_ACTIVE, TAI.FIN_AMORT_ID, TAI.INTEREST_INSTALLMENT, " +
                                "TAI.CAPITAL_INSTALLMENT, TAI.INSTAL_DUE_DT, TAI.REFELEM_FI_INST, TAI.REFELEM_FI_INST_ADM, TAI.REFELEM_FI_INST_LEAS, " +
                                "TAI.FG_IS_INVOICED, TAI.REFELEM_FI_CAP, TAI.REFELEM_FI_INT, TAI.INSTAL_NUMBER " +
                            "FROM IMXDB.T_FIN_AMO TFA " +
                                "INNER JOIN IMXDB.T_AMORT_HISTO TAI ON TFA.IMX_UN_ID = TAI.FIN_AMORT_ID) TA_H ON(DE.DF_DOS = TA_H.REFDOSS_REQST AND TA_H.REFELEM_FI_INST = EL.REFELEM) " +
                        "LEFT JOIN(SELECT FIN_AMORT_ID, max(TAI.INSTAL_NUMBER) MAX_INSTAL_NUMBER " +
                            "FROM IMXDB.T_AMORT_INSTAL TAI " +
                            "GROUP BY FIN_AMORT_ID) UTAV ON(TA.IMX_UN_ID = UTAV.FIN_AMORT_ID) " +
                        "LEFT JOIN(SELECT FIN_AMORT_ID, max(TAI.INSTAL_NUMBER) MAX_INSTAL_NUMBER " +
                            "FROM IMXDB.T_AMORT_HISTO TAI " +
                            "GROUP BY FIN_AMORT_ID)UTAV_H ON(TA_H.IMX_UN_ID = UTAV_H.FIN_AMORT_ID) " +
                        "LEFT JOIN(SELECT DDD.ER_REFEXT1 AS FOLIO_FACT, DDD.FEC_CANCELACION AS FECHA_CANC " +
                            "FROM (SELECT DISTINCT CCC.ER_REFEXT1, CCC.FEC_CANCELACION, RANK() OVER (PARTITION BY  CCC.ER_REFEXT1 ORDER BY CCC.FEC_CANCELACION ) NUM_CANCELACION " +
                                "FROM(SELECT DISTINCT F.ER_REFEXT1, max(DET.DF_ANN_DT) FEC_CANCELACION " +
                                    "FROM imxdb.F_ENTREL f, imxdb.F_ENTREL e, imxdb.f_detfac det " +
                                    "WHERE F.er_num = e.ER_ORG_INVOICE_REF and det.df_rel = e.er_num and e.ER_ORG_INVOICE_REF IS NOT NULL and E.ER_TCR > 0 " +
                                    "GROUP BY F.ER_REFEXT1 " +
                                    "UNION ALL " +
                                    "SELECT DISTINCT ER.ER_REFEXT1, MIN(DF.DF_ANN_DT) FEC_CANCELACION " +
                                    "FROM IMXDB.F_ENTREL ER INNER JOIN IMXDB.F_DETFAC DF ON(ER.ER_NUM = DF.DF_REL) " +
                                    "WHERE DF.DF_ANN IS NOT NULL AND DF.DF_SEN = 'D' AND DF.DF_ANN_DT > '31/12/2019' AND ER.ER_REFEXT1 IS NOT NULL " +
                                    "GROUP BY ER.ER_REFEXT1" +
                                ") CCC" +
                            ") DDD " +
                            "WHERE DDD.NUM_CANCELACION = 1 AND FEC_CANCELACION<TRUNC(sysdate,'DD')    ) CANC ON CANC.FOLIO_FACT = FE.ER_REFEXT1 " +
                        "WHERE FE.ER_NUM NOT IN(SELECT FE.ER_NUM " +
                    "FROM IMXDB.F_ENTREL FE, imxdb.F_ENTREL E " +
                    "WHERE FE.ER_NUM = E.ER_ORG_INVOICE_REF " +
                    "AND E.ER_ORG_INVOICE_REF IS NOT NULL AND E.ER_TCR > 0) " +
                    "AND CANC.FECHA_CANC IS NULL " +
                    "AND FE.ER_REFEXT1 IS NOT NULL " +
                    "AND DE.DF_INV_GROUP = 'A' " +
                    "AND DE.DF_NOM IN('LOY','FRAD','FRLO') " +
                    "AND DOSS.ANCREFDOSS = '" + Contrato + "' ORDER BY NVL(TA.INSTAL_NUMBER, TA_H.INSTAL_NUMBER)";

                oracleComando.CommandText = ConsultaSQL;

                OracleDataReader dataReader = oracleComando.ExecuteReader();

                ContratoDetalle c = new ContratoDetalle();
                while (dataReader.Read())
                {
                    c = new ContratoDetalle
                    {
                        contrato = dataReader["Contrato"].ToString(),
                        importe_factura = Convert.ToDouble(dataReader["Importe_Factura"]),
                        importe_pagado = Convert.ToDouble(dataReader["Importe_Pagado"]),
                        dias_atraso = Convert.ToDouble(dataReader["Dias_Atraso"]),
                        concepto = dataReader["Concepto"].ToString(),
                        periodo = dataReader["Periodo"].ToString()
                    };
                    this.listaContratoDetalles.Add(c);
                }

                dataReader.Close();
                oracleComando.Dispose();
                oracleConexion.Close();
            }
            catch (Exception)
            {
                Exception newEx = new Exception("Error al consultar.");
                throw newEx;
            }

            return listaContratoDetalles;
        }

        [HttpGet]
        [Route("api/clabe_stp1")]
        [Authorize]
        // GET: Clabe_STP
        public ClabeSTP GetClabeSTP1(string RFC)
        {
            try
            {
                var path = AppDomain.CurrentDomain.BaseDirectory;

                // create connection
                OracleConnection oracleConexion = new OracleConnection();

                // create connection string using builder
                OracleConnectionStringBuilder sqlConexionString = new OracleConnectionStringBuilder();
                sqlConexionString.ConnectionString = ConfigurationManager.ConnectionStrings["ABCLeasingOracle"].ConnectionString;

                // connect
                oracleConexion.ConnectionString = sqlConexionString.ConnectionString;
                oracleConexion.Open();
                Console.WriteLine("Connection established (" + oracleConexion.ServerVersion + ")");

                OracleCommand oracleComando = oracleConexion.CreateCommand();
                oracleComando.CommandType = CommandType.Text;
                oracleComando.Connection = oracleConexion;

                var fileStart = Path.Combine(path, "GetClabeStart.sql");

                string ConsultaSQL = File.ReadAllText(fileStart) + RFC + "'";

                oracleComando.CommandText = ConsultaSQL;

                OracleDataReader dataReader = oracleComando.ExecuteReader();

                ClabeSTP c = new ClabeSTP();
                while (dataReader.Read())
                {
                    c.clabe = dataReader["CLABE_STP"].ToString();
                }

                dataReader.Close();
                oracleComando.Dispose();
                oracleConexion.Close();

                return c;
            }
            catch (Exception)
            {
                Exception newEx = new Exception("Error al consultar.");
                throw newEx;
            }
        }

        [HttpGet]
        [Route("api/clabe_stp")]
        [Authorize]
        // GET: Clabe_STP
        public ClabeSTP GetClabeSTP(string RFC)
        {
            try
            {
                var path = AppDomain.CurrentDomain.BaseDirectory;

                // create connection
                OracleConnection oracleConexion = new OracleConnection();

                // create connection string using builder
                OracleConnectionStringBuilder sqlConexionString = new OracleConnectionStringBuilder();
                sqlConexionString.ConnectionString = ConfigurationManager.ConnectionStrings["ABCLeasingOracle"].ConnectionString;

                // connect
                oracleConexion.ConnectionString = sqlConexionString.ConnectionString;
                oracleConexion.Open();
                Console.WriteLine("Connection established (" + oracleConexion.ServerVersion + ")");

                OracleCommand oracleComando = oracleConexion.CreateCommand();
                oracleComando.CommandType = CommandType.Text;
                oracleComando.Connection = oracleConexion;

                string ConsultaSQL = "SELECT t.REFEXT CLABE_STP" +
                    " FROM T_INDIVIDU T" +
                        " inner join g_individu G on t.REFINDIVIDU = g.REFINDIVIDU" +
                    " WHERE t.SOCIETE = 'CLABE STP' and g.tva = '" + RFC + "'";

                oracleComando.CommandText = ConsultaSQL;

                OracleDataReader dataReader = oracleComando.ExecuteReader();

                ClabeSTP c = new ClabeSTP();
                while (dataReader.Read())
                {
                    c.clabe = dataReader["CLABE_STP"].ToString();
                }

                dataReader.Close();
                oracleComando.Dispose();
                oracleConexion.Close();

                return c;
            }
            catch (Exception)
            {
                Exception newEx = new Exception("Error al consultar.");
                throw newEx;
            }
        }

        [HttpPost]
        [Route("api/add_edo_cuenta")]
        [Authorize]
        public IHttpActionResult Post([FromBody] EdoCuenta new_edo_cuenta)
        {
            try
            {
                // create connection
                OracleConnection oracleConexion = new OracleConnection();

                // create connection string using builder
                OracleConnectionStringBuilder sqlConexionString = new OracleConnectionStringBuilder();
                sqlConexionString.ConnectionString = ConfigurationManager.ConnectionStrings["ABCLeasingOracle"].ConnectionString;

                // connect
                oracleConexion.ConnectionString = sqlConexionString.ConnectionString;
                oracleConexion.Open();
                Console.WriteLine("Connection established (" + oracleConexion.ServerVersion + ")");

                OracleCommand oracleComando = oracleConexion.CreateCommand();
                oracleComando.CommandType = CommandType.Text;
                oracleComando.Connection = oracleConexion;

                string ConsultaSQL = "Insert Into t_elements (refdoss, libelle, typeelem, dtsaisie, dtassoc, dt_envoi_dt) " +
                    "Values (:Prefdoss, :Plibelle, :Ptypeelem, to_char(sysdate,'j'), to_char(sysdate,'j'), :Pdt_envoi_dt)";

                oracleComando.CommandText = ConsultaSQL;

                oracleComando.Parameters.Add("Prefdoss", OracleDbType.Varchar2).Value = new_edo_cuenta.solicitud_financiacion;
                oracleComando.Parameters.Add("Plibelle", OracleDbType.Varchar2).Value = "GENERAR ESTADO DE CUENTA";
                oracleComando.Parameters.Add("Ptypeelem", OracleDbType.Varchar2).Value = "in";
                oracleComando.Parameters.Add("Pdt_envoi_dt", OracleDbType.Varchar2).Value = new_edo_cuenta.fecha_ejecucion;

                oracleComando.ExecuteNonQuery();

                oracleComando.Dispose();
                oracleConexion.Dispose();
                oracleConexion.Close();

                return Json(new
                {
                    message = "Estado de Cuenta Agregado Correctamente"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        [Route("api/poliza_seguro")]
        [Authorize]
        public ClabeSTP GetPolizaSeguro(string RFC)
        {
            try
            {
                var path = AppDomain.CurrentDomain.BaseDirectory;

                OracleConnection oracleConexion = new OracleConnection();
                OracleConnectionStringBuilder sqlConexionString = new OracleConnectionStringBuilder();
                sqlConexionString.ConnectionString = ConfigurationManager.ConnectionStrings["ABCLeasingOracle"].ConnectionString;

                oracleConexion.ConnectionString = sqlConexionString.ConnectionString;
                oracleConexion.Open();
                Console.WriteLine("Connection established (" + oracleConexion.ServerVersion + ")");

                OracleCommand oracleComando = oracleConexion.CreateCommand();
                oracleComando.CommandType = CommandType.Text;
                oracleComando.Connection = oracleConexion;

                string ConsultaSQL = "'" + RFC + "'";

                oracleComando.CommandText = ConsultaSQL;

                OracleDataReader dataReader = oracleComando.ExecuteReader();

                ClabeSTP c = new ClabeSTP();
                while (dataReader.Read())
                {
                    c.clabe = dataReader["CLABE_STP"].ToString();
                }

                dataReader.Close();
                oracleComando.Dispose();
                oracleConexion.Close();

                return c;
            }
            catch (Exception)
            {
                Exception newEx = new Exception("Error al consultar.");
                throw newEx;
            }
        }
    }
}
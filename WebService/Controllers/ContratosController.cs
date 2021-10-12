using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using WebService.Models;

namespace WebService.Controllers
{
    public class ContratosController : ApiController
    {
        List<Contrato> listaContratos = new List<Contrato>();

        [Route("api/contratosSQL")]
        [Authorize]
        public List<Contrato> GetContratosSQLServer(string RFC)
        {
            try
            {
                var sqlConexionString = ConfigurationManager.ConnectionStrings["ABCLeasingRiesgos"].ConnectionString;
                using (SqlConnection sqlConexion = new SqlConnection(sqlConexionString))
                {
                    SqlCommand sqlComandoSelect = new SqlCommand("SP_Riesgos_Clientes", sqlConexion);

                    sqlComandoSelect.CommandType = CommandType.StoredProcedure;

                    sqlComandoSelect.Parameters.AddWithValue("@RFC", RFC);

                    sqlConexion.Open();

                    SqlDataReader sqlDatos = sqlComandoSelect.ExecuteReader();

                    Contrato c = new Contrato();

                    while (sqlDatos.Read())
                    {
                        c = new Contrato
                        {
                            rfc_user = sqlDatos["RFC"].ToString(),
                            tipo_relacion = sqlDatos["Tipo_Relacion"].ToString(),
                            contrato = sqlDatos["Contrato"].ToString(),
                            monto_financiado = Convert.ToDouble(sqlDatos["Monto_Financiado"]),
                            residual_sin_iva = Convert.ToDouble(sqlDatos["Residual_Sin_Iva"]),
                            saldo_ins_inv_neta = Convert.ToDouble(sqlDatos["Saldo_Ins_Inv_Neta"]),
                            importe_rentas_vencidas = Convert.ToDouble(sqlDatos["Importe_Rentas_Vencidas"]),
                            otros_conceptos_vencidos = Convert.ToDouble(sqlDatos["Otros_Conceptos_Vencidos"]),
                            intereses_moratorios = Convert.ToDouble(sqlDatos["Intereses_Moratorios"]),
                            fecha_activacion = sqlDatos["Fecha_Activacion"].ToString(),
                            rentas_devengar = Convert.ToDouble(sqlDatos["Rentas_Devengar"]),
                            plazo = Convert.ToInt32(sqlDatos["Plazo"]),
                            meses_pagados = Convert.ToInt32(sqlDatos["Meses_Pagados"]),
                            saldo_vigente_capital = Convert.ToDouble(sqlDatos["Saldo_Vigente_Capital"]),
                            proxima_renta = sqlDatos["Proxima_Renta"].ToString(),
                            num_rentas_vencidas = Convert.ToInt32(sqlDatos["Num_Rentas_Vencidas"])
                        };
                        this.listaContratos.Add(c);
                    }

                    sqlDatos.Close();
                    sqlComandoSelect.Dispose();
                    sqlConexion.Close();
                }
            }
            catch (Exception)
            {
                Exception newEx = new Exception("Error al consultar.");
                throw newEx;
            }

            return listaContratos;
        }

        [Route("api/contratos")]
        [Authorize]
        public List<Contrato> GetContratosOracle(string RFC)
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

                string ConsultaSQL = "select" +
                    "    IND.TVA RFC," +
                    "    CASE T.reftype WHEN 'DB' THEN 'TITULAR'" +
                    "        WHEN 'COL' THEN 'COARRENDATARIO'" +
                    "        WHEN 'CO2' THEN 'COARRENDATARIO_2'" +
                    "        WHEN 'CA' THEN 'OBLIGADO_SOLIDARIO'" +
                    "        WHEN 'CA2' THEN 'DEPOSITARIO'" +
                    "        ELSE 'TITULAR' END TIPO_RELACION," +
                    "    DOSS.ANCREFDOSS CONTRATO," +
                    "    ROUND(nvl(GP.MT10, 0), 2) MONTO_FINANCIADO," +
                    "	ROUND(UVT.RESIDUAL_VALUE * TC.DBL_TC, 2) RESIDUAL_SIN_IVA," +
                    "	ROUND((tabla.SALDO_INS_CAPITAL + UVT.RESIDUAL_VALUE) * TC.DBL_TC, 2) SALDO_INS_INV_NETA," +
                    "	NVL((Select" +
                    "        sum(F.ER_TDB_MVT)" +
                    "        from(select df_dos, df_rel from imxdb.F_DETFAC where upper(df_nom) = 'LOY' group by df_dos, df_rel) d" +
                    "            inner join imxdb.F_ENTREL F on d.df_rel = f.er_num" +
                    "        where F.ER_TDB_MVT > NVL(F.ER_PAYE_MVT, 0)" +
                    "            and d.df_dos = DOSS.refdoss), 0) IMPORTE_RENTAS_VENCIDAS," +
                    "	NVL((Select" +
                    "        sum(F.ER_TDB_MVT - NVL(F.ER_PAYE_MVT, 0))" +
                    "        from(select df_dos, df_rel from imxdb.F_DETFAC where DF_INV_GROUP in ('C', 'F') group by df_dos, df_rel) d" +
                    "            inner join imxdb.F_ENTREL F on d.df_rel = f.er_num" +
                    "        where F.ER_TDB_MVT > NVL(F.ER_PAYE_MVT, 0)" +
                    "            and d.df_dos = DOSS.refdoss), 0) OTROS_CONCEPTOS_VENCIDOS," +
                    "	NVL((Select" +
                    "        sum(F.ER_TDB_MVT)" +
                    "        from(select df_dos, df_rel from imxdb.F_DETFAC where upper(df_nom) = 'LPI' group by df_dos, df_rel) d" +
                    "            inner join imxdb.F_ENTREL F on d.df_rel = f.er_num" +
                    "        where F.ER_TDB_MVT > NVL(F.ER_PAYE_MVT, 0)" +
                    "            and d.df_dos = DOSS.refdoss), 0) INTERESES_MORATORIOS," +
                    "	to_char(GP.DT02_DT, 'dd/mm/yyyy')  FECHA_ACTIVACION," +
                    "	tabla.Rentas_Devengar * (1 + tabla.TASA_IVA) RENTAS_DEVENGAR," +
                    "	tabla.PLAZO PLAZO," +
                    "    NVL((Round(MONTHS_BETWEEN(sysdate, tabla.VEN_PRIMER_RENTA), 0) + 1), 0) Meses_Pagados," +
                    "	trunc(sysdate) - (to_number(to_char(sysdate, 'DD')) - 1) - 1 FEC_CORTE," +
                    "	NVL((Select" +
                    "        max(to_char(F.er_reg_dt, 'dd/mm/yyyy')) er_reg_dt" +
                    "        from(select df_dos, df_rel from imxdb.F_DETFAC where upper(df_nom) = 'LOY' group by df_dos, df_rel) d" +
                    "           inner join imxdb.F_ENTREL F on d.df_rel = f.er_num" +
                    "        where F.ER_TDB_MVT > NVL(F.ER_PAYE_MVT, 0)" +
                    "            and d.df_dos = DOSS.refdoss), '01/01/1990') er_reg_dt," +
                    "	tabla.Rentas_Devengar * (1 + tabla.TASA_IVA) SALDO_VIGENTE_CAPITAL," +
                    "	NVL((Select max(to_char(t_i.INSTAL_DUE_DT, 'dd/mm/yyyy'))" +
                    "        FROM IMXDB.T_FIN_AMO t_a" +
                    "        Inner Join(" +
                    "            Select INSTAL_DUE_DT, FIN_AMORT_ID" +
                    "            From IMXDB.T_AMORT_INSTAL) t_i On t_i.FIN_AMORT_ID = t_a.imx_un_id" +
                    "        Where t_i.FIN_AMORT_ID = uvt.imx_un_id" +
                    "            And to_char(t_i.INSTAL_DUE_DT, 'dd/mm/yyyy') Between to_char(ADD_MONTHS(TRUNC(SYSDATE, 'MM'), +1), 'dd/mm/yyyy')" +
                    "            And to_char(ADD_MONTHS(LAST_DAY(TRUNC(SYSDATE)), +1), 'dd/mm/yyyy')),'01/01/1990') PROXIMA_RENTA" +
                    " from imxdb.G_PIECE GP" +
                    "    inner join imxdb.T_INTERVENANTS T on GP.refdoss = T.refdoss" +
                    "    inner join imxdb.G_DOSSIER DOSS on T.refdoss = DOSS.refdoss" +
                    "    inner join imxdb.G_INDIVIDU IND on IND.refindividu = t.refindividu" +
                    "    inner join imxdb.T_FIN_AMO uvt on DOSS.refdoss = uvt.refdoss_reqst" +
                    "    inner join(" +
                    "        select distinct his.fin_amort_id" +
                    "           , SUM(case when his.FEC_FACTURADO >= (TRUNC(sysdate, 'DD') + 1) THEN ROUND(his.Capital,2) ELSE 0 END) SALDO_INS_CAPITAL" +
                    "		    , count(his.instal_number) PLAZO" +
                    "			, MIN(his.instal_due_dt) VEN_PRIMER_RENTA" +
                    "			, SUM(case when his.FEC_FACTURADO >= (TRUNC(sysdate, 'DD') + 1) THEN ROUND((his.Capital + his.Interes), 2) ELSE 0 END) RENTAS_DEVENGAR" +
                    "			, MAX(his.DF_TAUX_TVA) TASA_IVA" +
                    "        FROM" +
                    "            (select amort.fin_amort_id," +
                    "                amort.instal_due_dt," +
                    "                amort.instal_number," +
                    "                amort.Capital," +
                    "                amort.interes," +
                    "                nvl(ent.ER_DAT_DT, amort.instal_due_dt) FEC_FACTURADO," +
                    "                CASE WHEN ent.DF_TAUX_TVA = 0 THEN 0.16 ELSE NVL(ent.DF_TAUX_TVA, 0.16) END DF_TAUX_TVA" +
                    "            from" +
                    "                (select a.fin_amort_id, a.instal_due_dt, a.instal_number, a.capital_installment Capital, a.interest_installment interes, a.refelem_fi_inst" +
                    "                from imxdb.t_amort_histo a" +
                    "                union all" +
                    "                select b.fin_amort_id, b.instal_due_dt, b.instal_number, b.capital_installment Capital, b.interest_installment interes, b.refelem_fi_inst" +
                    "                from imxdb.T_AMORT_INSTAL b ) amort" +
                    "            left join" +
                    "                (select elem.refelem, entx.er_dat_dt, DF_TAUX_TVA" +
                    "                from imxdb.g_elemfi elem" +
                    "                    inner join imxdb.f_detfac det on det.df_num = elem.LIBELLE_20_2" +
                    "          inner join imxdb.f_entrel entx on entx.er_num = det.df_rel" +
                    "                where elem.libelle_20_3 = 'LOY'" +
                    "                ) ent on amort.refelem_fi_inst = ent.refelem" +
                    "            )his" +
                    "        group by his.fin_amort_id" +
                    "	) tabla on tabla.fin_amort_id = uvt.imx_un_id" +
                    "    inner join(" +
                    "        select TTC.origine TXT_MONEDA, round(1 / NVL(TTC.\"TAUX\", 1.0),4) as DBL_TC" +
                    "        from imxdb.t_devise TTC" +
                    "        where type = 'MR' and PLACE = 'MEX'" +
                    "	) TC on TC.TXT_MONEDA = DOSS.devise" +
                    " where SUBSTR(doss.categdoss,1,17) = 'FINANCING REQUEST'" +
                    "    and uvt.flag_active = 'O'" +
                    "    and NVL(t.reftype,'DB') In('DB', 'COL', 'CO2', 'CA', 'CA2')" +
                    "    and GP.typpiece = 'FINANCING REQUEST'" +
                    "    and GP.str_20_1 in ('ACT', 'ECT', 'TER')" +
                    "    and IND.TVA = '" + RFC + "'";

                oracleComando.CommandText = ConsultaSQL;

                OracleDataReader dataReader = oracleComando.ExecuteReader();

                Contrato c = new Contrato();

                while (dataReader.Read())
                {
                    int daysDiff = ((TimeSpan)(Convert.ToDateTime(dataReader["Fec_Corte"]) - Convert.ToDateTime(dataReader["er_reg_dt"]))).Days;
                    int monthDiff;
                    if (daysDiff <= 0 || dataReader["er_reg_dt"].ToString() == "01/01/1990")
                        monthDiff = 0;
                    else
                        monthDiff = daysDiff / 30;

                    c = new Contrato
                    {
                        rfc_user = dataReader["RFC"].ToString(),
                        tipo_relacion = dataReader["Tipo_Relacion"].ToString(),
                        contrato = dataReader["Contrato"].ToString(),
                        monto_financiado = Convert.ToDouble(dataReader["Monto_Financiado"]),
                        residual_sin_iva = Convert.ToDouble(dataReader["Residual_Sin_Iva"]),
                        saldo_ins_inv_neta = Convert.ToDouble(dataReader["Saldo_Ins_Inv_Neta"]),
                        importe_rentas_vencidas = Convert.ToDouble(dataReader["Importe_Rentas_Vencidas"]),
                        otros_conceptos_vencidos = Convert.ToDouble(dataReader["Otros_Conceptos_Vencidos"]),
                        intereses_moratorios = Convert.ToDouble(dataReader["Intereses_Moratorios"]),
                        fecha_activacion = dataReader["Fecha_Activacion"].ToString(),
                        rentas_devengar = Convert.ToDouble(dataReader["Rentas_Devengar"]),
                        plazo = Convert.ToInt32(dataReader["Plazo"]),
                        meses_pagados = Convert.ToInt32(dataReader["Meses_Pagados"]) - monthDiff,
                        saldo_vigente_capital = Convert.ToDouble(dataReader["Saldo_Vigente_Capital"]),
                        proxima_renta = dataReader["Proxima_Renta"].ToString(),
                        num_rentas_vencidas = monthDiff,
                    };
                    this.listaContratos.Add(c);
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

            return listaContratos;
        }
    }
}

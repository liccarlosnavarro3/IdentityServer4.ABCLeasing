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

        [Route("api/contratos")]
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

        [Route("api/contratosOracle")]
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

                string ConsultaSQL = "select DOSS.ANCREFDOSS Contrato, to_char(p.DT02_DT, 'dd/mm/yyyy')  Fecha_Activacion, IND.TVA RFC, T.reftype Tipo_Relacion, ROUND(nvl(p.MT10,0),2) Monto_Financiado, ROUND(ta.RESIDUAL_VALUE * TC.DBL_TC,2) Residual_Sin_Iva," +
                    "    ROUND((tabla.SALDO_INS_CAPITAL + ta.RESIDUAL_VALUE) * TC.DBL_TC, 2) Saldo_Ins_Inv_Neta, NVL(Venc.Rentas_Vencidas, 0) Importe_Rentas_Vencidas, NVL(Venc.Otros_Conceptos, 0) Otros_Conceptos_Vencidos," +
                    "    NVL(Venc.Intereses_Moratorios, 0) Intereses_Moratorios, tabla.Rentas_Devengar, tabla.PLAZO Plazo," +
                    "    NVL((Round(MONTHS_BETWEEN(sysdate, tabla.VEN_PRIMER_RENTA), 0) + 1) - (Case When TC.FEC_CORTE - Venc.ER_REG_DT < 0 then 0 Else Round((TC.FEC_CORTE - Venc.ER_REG_DT) / 30, 0) End), 0) Meses_Pagados," +
                    "	tabla.RENTAS_DEVENGAR Saldo_Vigente_Capital," +
                    "    NVL((Select max(to_char(t_i.INSTAL_DUE_DT, 'dd/mm/yyyy')) FROM IMXDB.T_FIN_AMO t_a Inner Join(" +
                    "        Select INSTAL_DUE_DT, FIN_AMORT_ID From IMXDB.T_AMORT_INSTAL) t_i On t_i.FIN_AMORT_ID = t_a.imx_un_id Where t_i.FIN_AMORT_ID = uvt.imx_un_id And to_char(t_i.INSTAL_DUE_DT, 'dd/mm/yyyy') Between to_char(ADD_MONTHS(TRUNC(SYSDATE, 'MM'), +1), 'dd/mm/yyyy') And to_char(ADD_MONTHS(LAST_DAY(TRUNC(SYSDATE)), +1), 'dd/mm/yyyy')),'01/01/1990') PROXIMA_RENTA," +
                    "    NVL(Case When TC.FEC_CORTE - Venc.ER_REG_DT < 0 Then 0 Else Round((TC.FEC_CORTE - Venc.ER_REG_DT) / 30, 0) End, 0) NUM_RENTAS_VENCIDAS" +
                    "  from imxdb.G_DOSSIER DOSS" +
                    "    inner join(Select DT02_DT, MT10, TX02, REFDOSS, TYPPIECE From imxdb.G_PIECE Where typpiece = 'FINANCING REQUEST') P on DOSS.REFDOSS = P.REFDOSS" +
                    "    inner join(Select REFDOSS, ANCREFDOSS, DEVISE, REFLOT, CATEGDOSS From imxdb.G_DOSSIER) DF on DOSS.reflot = DF.refdoss" +
                    "    inner join(Select REFTYPE, REFDOSS, REFINDIVIDU From imxdb.T_INTERVENANTS Where NVL(reftype,'DB') = 'DB') T on t.refdoss = DOSS.refdoss" +
                    "    inner join(Select MORALPHY, PRENOM, NOMCOMPL, NOM, STR33, GENRE, TVA, DIVISION, STR44, REFINDIVIDU, ADR2 From imxdb.G_INDIVIDU) IND on t.refindividu = IND.refindividu" +
                    "    left join(Select REFTYPE, REFDOSS, REFINDIVIDU From imxdb.T_INTERVENANTS Where NVL(reftype,'DB') = 'EPB') PROMOTOR_T on PROMOTOR_T.refdoss = DOSS.refdoss" +
                    "    inner join(" +
                    "        select c.refdoss_reqst, max(c.imx_un_id) imx_un_id" +
                    "        from imxdb.T_FIN_AMO c inner join (Select IMX_UN_ID, TYPE, STR3, DT04_DT From IMXDB.g_piecedet Where TYPE = 'ANNEXE_DEMANDE_FIN' AND STR3 = 'ACT')det on det.imx_un_id = c.ANNEX_ID" +
                    "        where det.DT04_DT < (TRUNC(sysdate, 'DD') + 1) and(flag_active is null or flag_active = 'O')" +
                    "        group by c.refdoss_reqst) uvt on uvt.refdoss_reqst = DOSS.refdoss" +
                    "    inner join(Select IMX_UN_ID, REFDOSS_REQST, ANNEX_ID, RESIDUAL_VALUE From imxdb.T_FIN_AMO) ta on ta.refdoss_reqst = DOSS.refdoss and ta.imx_un_id = uvt.imx_un_id" +
                    "    inner join(" +
                    "        select distinct his.fin_amort_id, SUM(case when his.FEC_FACTURADO >= (TRUNC(sysdate, 'DD') + 1) THEN ROUND(his.Capital,2) ELSE 0 END) SALDO_INS_CAPITAL, count(his.instal_number) PLAZO" +
                    "            , MIN(his.instal_due_dt) VEN_PRIMER_RENTA, SUM(case when his.FEC_FACTURADO >= (TRUNC(sysdate, 'DD') + 1) THEN ROUND((his.Capital + his.Interes), 2) ELSE 0 END) RENTAS_DEVENGAR" +
                    "        from(" +
                    "            select amort.fin_amort_id, amort.instal_due_dt, amort.instal_number, amort.capital_installment Capital, amort.interest_installment interes, nvl(ent.ER_DAT_DT, amort.instal_due_dt) FEC_FACTURADO" +
                    "            from imxdb.t_amort_histo amort" +
                    "            left join" +
                    "       (select elem.refelem, entx.er_dat_dt from imxdb.g_elemfi elem" +
                    "           inner join (select d.df_num, d.df_rel from imxdb.f_detfac d) det on det.df_num = elem.LIBELLE_20_2" +
                    "                inner join(select e.er_num, e.ER_DAT_DT from imxdb.f_entrel e) entx on entx.er_num = det.df_rel" +
                    "            where elem.libelle_20_3 = 'LOY'" +
                    "            ) ent on ent.refelem = amort.refelem_fi_inst" +
                    "        union all" +
                    "            select amort.fin_amort_id, amort.instal_due_dt, amort.instal_number, amort.capital_installment Capital, amort.interest_installment interes, nvl(ent.ER_DAT_DT, amort.instal_due_dt) FEC_FACTURADO" +
                    "             from imxdb.T_AMORT_INSTAL amort" +
                    "            left join" +
                    "            (select elem.refelem, entx.er_dat_dt from imxdb.g_elemfi elem" +
                    "                inner join (select d.df_num, d.df_rel from imxdb.f_detfac d) det on det.df_num = elem.LIBELLE_20_2" +
                    "                inner join(select e.er_num, e.ER_DAT_DT from imxdb.f_entrel e) entx on entx.er_num = det.df_rel" +
                    "            where elem.libelle_20_3 = 'LOY'" +
                    "            ) ent on ent.refelem = amort.refelem_fi_inst" +
                    "        ) his" +
                    "        group by his.fin_amort_id" +
                    "    ) tabla on tabla.fin_amort_id = uvt.imx_un_id" +
                    "    inner join(" +
                    "        select TTC.origine TXT_MONEDA, round(1 / NVL(TTC.TAUX, 1.0),4) as DBL_TC, dtdebut_dt FEC_TC, FEC_CORTE" +
                    "       from imxdb.t_devise TTC, (" +
                    "           select origine, max(dtdebut_dt) last_dt , FEC_CORTE" +
                    "           from imxdb.t_devise , (" +
                    "               select trunc(sysdate) - (to_number(to_char(sysdate, 'DD')) - 1) - 1 FEC_CORTE from dual) PARAMS where type = 'MR' and PLACE = 'MEX' and dtdebut_dt <= PARAMS.FEC_CORTE" +
                    "            group by origine) MAX_TC" +
                    "       where TTC.origine = MAX_TC.origine and TTC.dtdebut_dt = MAX_TC.last_dt" +
                    "	) TC on TC.TXT_MONEDA = DOSS.devise" +
                    "   Inner Join(" +
                    "       Select Distinct df_dos, Case When upper(d.df_nom) = 'LOY' Then F.ER_TDB Else 0 End Rentas_Vencidas, Case When d.DF_INV_GROUP = 'C' Then F.ER_TDB Else 0 End Otros_Conceptos" +
                    "           , Case When upper(d.df_nom) = 'LPI' Then F.ER_TDB Else 0 End Intereses_Moratorios, Case When upper(d.df_nom) = 'LOY' Then F.ER_TDB Else 0 End Saldo_Vencido_Capital, Case When upper(d.df_nom) = 'LOY' Then er_reg_dt End ER_REG_DT" +
                    "       from imxdb.F_ENTREL F" +
                    "           inner join (Select LIBELLE From imxdb.G_ELEMFI) g on f.er_refext1 = g.libelle" +
                    "           inner join(Select DF_REL, DF_NOM, DF_DOS, DF_INV_GROUP From imxdb.F_DETFAC) d on f.er_num = d.df_rel" +
                    "       where F.ER_TDB_MVT > NVL(F.ER_PAYE_MVT, 0) and(upper(d.df_nom) = 'LOY' Or d.DF_INV_GROUP = 'C' Or upper(d.df_nom) = 'LPI')" +
                    "       order by er_reg_dt" +
                    "   ) Venc On Venc.df_dos = DOSS.REFDOSS" +
                    " where doss.categdoss LIKE 'FINANCING REQUEST%' And IND.TVA = '" + RFC + "'" +
                    " Order By IND.TVA";

                oracleComando.CommandText = ConsultaSQL;

                OracleDataReader dataReader = oracleComando.ExecuteReader();

                Contrato c = new Contrato();

                while (dataReader.Read())
                {
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
                        meses_pagados = Convert.ToInt32(dataReader["Meses_Pagados"]),
                        saldo_vigente_capital = Convert.ToDouble(dataReader["Saldo_Vigente_Capital"]),
                        proxima_renta = dataReader["Proxima_Renta"].ToString(),
                        num_rentas_vencidas = Convert.ToInt32(dataReader["Num_Rentas_Vencidas"])
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

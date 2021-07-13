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
        public List<Contrato> GetSQLServer(string RFC)
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
                            residual_iva = Convert.ToDouble(sqlDatos["Residual_Iva"]),
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

        [Route("api/contratosoracle")]
        [Authorize]
        public List<Contrato> GetOracle(string RFC)
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
                oracleComando.CommandType = CommandType.StoredProcedure;
                oracleComando.Connection = oracleConexion;
                oracleComando.CommandText = "SP_RIESGOS_CLIENTES";

                OracleParameter var_rfc = new OracleParameter();
                var_rfc.OracleDbType = OracleDbType.Varchar2;
                var_rfc.Direction = ParameterDirection.Input;
                var_rfc.Value = RFC;

                OracleParameter var_result = new OracleParameter();
                var_result.OracleDbType = OracleDbType.RefCursor;
                var_result.Direction = ParameterDirection.Output;

                oracleComando.Parameters.Add(var_rfc);
                oracleComando.Parameters.Add(var_result);

                oracleComando.ExecuteNonQuery();

                OracleDataReader dataReader = ((OracleRefCursor)oracleComando.Parameters[1].Value).GetDataReader();

                Contrato c = new Contrato();
                while (dataReader.Read())
                {
                    c = new Contrato
                    {
                        rfc_user = dataReader["RFC"].ToString(),
                        tipo_relacion = dataReader["Tipo_Relacion"].ToString(),
                        contrato = dataReader["Contrato"].ToString(),
                        monto_financiado = Convert.ToDouble(dataReader["Monto_Financiado"]),
                        residual_iva = Convert.ToDouble(dataReader["Residual_Sin_Iva"]),
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

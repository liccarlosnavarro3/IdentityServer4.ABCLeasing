using System;
using System.Web;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ABCLeasing.ABCPocket.Models;
using System.Configuration;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ABCLeasing.ABCPocket.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult FindRFC(string RFC)
        {
            try
            {
                int Creado = 1;
                int Intento = 0;
                int Verificado = 0;

                RFC = RFC ?? "ZAXX010101000";
                //#Sección Buscar RFC
                var sqlConexionString = _configuration.GetConnectionString("ABCConnectionString");
                //var sqlConexionString = "Data Source=srvboston\\bostonprod;Min Pool Size=0;Max Pool Size=10024;Pooling=true;Initial Catalog=auth_api.abcleasing.com.mx;User ID=sa;Password=Bases2015.#; Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
                SqlConnection sqlConexion = new SqlConnection(sqlConexionString);

                var sqlConsulta = "Select * From UsersPocketABC Where RFC = @RFC";
                sqlConexion.Open();
                SqlCommand sqlComando = new SqlCommand(sqlConsulta, sqlConexion);

                sqlComando.Parameters.AddWithValue("@RFC", RFC.ToUpper());

                SqlDataReader dataReader = sqlComando.ExecuteReader();

                while (dataReader.Read())
                {
                    if ((bool)dataReader["Creado"])
                    {
                        Creado = 3;
                    }
                    else
                    {
                        Creado = 2;
                    }
                    Intento = (int)dataReader["Intento"];
                }

                ViewBag.Creado = Creado;
                ViewBag.Intento = Intento;
                ViewBag.Verificado = Verificado;
                ViewBag.RFC = RFC.ToUpper();

                dataReader.Close();
                sqlComando.Dispose();
                sqlConexion.Close();

                //#end Buscar
                //return RedirectToAction("Index");
                return View("Index");
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }
        }

        public IActionResult VerifyUser(string RFC, int Intento, string Contrato, string Telefono, string Email)
        {
            try
            {
                Telefono = Telefono ?? "0000000000";
                RFC = RFC ?? "ZAXX010101000";
                int Verificado = 1;

                if (Intento <= 3) {
                    Intento++;
                    //#Sección Buscar RFC
                    var sqlConexionString = _configuration.GetConnectionString("ABCConnectionString");
                    //var sqlConexionString = "Data Source=srvboston\\bostonprod;Min Pool Size=0;Max Pool Size=10024;Pooling=true;Initial Catalog=auth_api.abcleasing.com.mx;User ID=sa;Password=Bases2015.#; Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
                    SqlConnection sqlConexion = new SqlConnection(sqlConexionString);

                    //var sqlConsulta = "Select * From UsersPocketABC Where RFC = @RFC";
                    var sqlConsulta = "Select Isnull(Contratos, '')Contratos, Isnull(Telefonos, '')Telefonos, Isnull(Email, '')Email From UsersPocketABC Where RFC = @RFC";
                    sqlConexion.Open();
                    SqlCommand sqlComando = new SqlCommand(sqlConsulta, sqlConexion);

                    sqlComando.Parameters.AddWithValue("@RFC", RFC.ToUpper());

                    SqlDataReader dataReader = sqlComando.ExecuteReader();

                    while (dataReader.Read())
                    {
                        if (string.IsNullOrEmpty(dataReader["Telefonos"].ToString()))
                        {
                            if (dataReader["Contratos"].ToString().ToUpper().Contains(Contrato.ToUpper())
                                && dataReader["Email"].ToString().ToLower().Contains(Email.ToLower()))
                            {
                                Verificado = 3;
                            }
                        } else
                        {
                            if (dataReader["Contratos"].ToString().ToUpper().Contains(Contrato.ToUpper())
                                && dataReader["Email"].ToString().ToLower().Contains(Email.ToLower())
                                && dataReader["Telefonos"].ToString().Contains(Telefono))
                            {
                                Verificado = 3;
                            }
                        }
                    }

                    dataReader.Close();
                    sqlComando.Dispose();
                    sqlConexion.Close();
                    //#end Buscar

                    //Actualiza Intento
                    sqlConsulta = "Update UsersPocketABC Set Intento = @Intento Where RFC = @RFC";
                    sqlConexion.Open();
                    SqlCommand sqlComando2 = new SqlCommand(sqlConsulta, sqlConexion);

                    sqlComando2.Parameters.AddWithValue("@Intento", Intento);
                    sqlComando2.Parameters.AddWithValue("@RFC", RFC.ToUpper());

                    sqlComando2.ExecuteNonQuery();
                    sqlComando2.Dispose();
                    sqlConexion.Close();
                    //#end Actualizar
                }
                else
                {
                    Verificado = 2;
                }

                ViewBag.Creado = 2;
                ViewBag.Verificado = Verificado;
                ViewBag.Intento = Intento;
                ViewBag.RFC = RFC.ToUpper();
                ViewBag.Contrato = Contrato.ToUpper();
                ViewBag.Telefono = (Telefono == "0000000000") ? "" : Telefono;
                ViewBag.Email = Email.ToLower();

                return View("Index");
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }
        }

        public IActionResult SavePass(string RFC, string Email, string Pass, string RePass)
        {
            try
            {
                int Creado = 2;
                int Verificado = 4;

                if (Pass == RePass) {
                    AES aES = new AES();
                    string PassEncrypt = aES.OpenSSLEncrypt(Pass);

                    //#Sección Actualizar Contraseña
                    var sqlConexionString = _configuration.GetConnectionString("ABCConnectionString");
                    //var sqlConexionString = "Data Source=srvboston\\bostonprod;Min Pool Size=0;Max Pool Size=10024;Pooling=true;Initial Catalog=auth_api.abcleasing.com.mx;User ID=sa;Password=Bases2015.#; Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
                    SqlConnection sqlConexion = new SqlConnection(sqlConexionString);

                    var sqlConsulta = "Update ClientesABC Set updated_at = @updated_at, password = @Pass Where RFC = @RFC";
                    sqlConexion.Open();
                    SqlCommand sqlComando = new SqlCommand(sqlConsulta, sqlConexion);

                    sqlComando.Parameters.AddWithValue("@updated_at", DateTime.Now);
                    sqlComando.Parameters.AddWithValue("@Pass", PassEncrypt);
                    sqlComando.Parameters.AddWithValue("@RFC", RFC.ToUpper());

                    sqlComando.ExecuteNonQuery();
                    sqlComando.Dispose();
                    sqlConexion.Close();
                    //#end Actualizar

                    //Actualiza Intento
                    sqlConsulta = "Update UsersPocketABC Set Creado = 1 Where RFC = @RFC";
                    sqlConexion.Open();
                    SqlCommand sqlComando2 = new SqlCommand(sqlConsulta, sqlConexion);

                    sqlComando2.Parameters.AddWithValue("@RFC", RFC.ToUpper());

                    sqlComando2.ExecuteNonQuery();
                    sqlComando2.Dispose();
                    sqlConexion.Close();
                    //#end Actualizar
                    Creado = 4;
                }
                ViewBag.RFC = "";
                ViewBag.Contrato = "";
                ViewBag.Telefono = "";
                ViewBag.Email = "";
                ViewBag.Creado = Creado;
                ViewBag.Verificado = Verificado;

                return View("Index");
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }
        }
        
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

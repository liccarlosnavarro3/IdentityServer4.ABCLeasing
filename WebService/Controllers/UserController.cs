using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web.Http;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Newtonsoft.Json;
using WebService.Models;
using System;

namespace WebService.Controllers
{
    [Route("api/user")]
    public class UserController : ApiController
    {
        [Authorize]
        public IHttpActionResult Get()
        {
            //#Sección de Buscar Usuario
            string user_id;
            string device_type;
            string updated_at;
            string created_at;
            string no_cliente;
            string RFC;
            string nombre;

            string Url_User = Request.RequestUri.Query.ToString();
            string Parameter_User = Url_User.Substring(1, 3);

            if (Parameter_User == "RFC")
            {
                string RFC_User = Url_User.Substring(5);
                var sqlConexionString = ConfigurationManager.ConnectionStrings["ABCLeasingABC"].ConnectionString;
                
                SqlConnection sqlConexion = new SqlConnection(sqlConexionString);
                string sqlConsulta = "SELECT user_id, device_type, updated_at, created_at, no_cliente, RFC, nombre FROM ClientesABC WHERE UPPER(RFC) = '" + RFC_User.ToUpper() + "'";

                sqlConexion.Open();

                SqlCommand sqlComando = new SqlCommand(sqlConsulta, sqlConexion);

                SqlDataReader sqlDatos = sqlComando.ExecuteReader();
                if (sqlDatos.Read())
                {
                    user_id = sqlDatos["user_id"].ToString();
                    device_type = sqlDatos["device_type"].ToString();
                    updated_at = string.Format("{0:d}", sqlDatos["updated_at"]);
                    created_at = string.Format("{0:d}", sqlDatos["created_at"]);
                    no_cliente = sqlDatos["no_cliente"].ToString();
                    RFC = sqlDatos["RFC"].ToString();
                    nombre = sqlDatos["nombre"].ToString();

                    sqlConexion.Close();

                    //#Sección de Claims
                    var claims = new List<Claim>();
                    claims.Add(new Claim("user_id", user_id));
                    claims.Add(new Claim("device_type", device_type));
                    claims.Add(new Claim("updated_at", updated_at));
                    claims.Add(new Claim("created_at", created_at));
                    claims.Add(new Claim("no_cliente", no_cliente));
                    claims.Add(new Claim("RFC", RFC));
                    claims.Add(new Claim(ClaimTypes.Surname, nombre));

                    var claimsIdentity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    Thread.CurrentPrincipal = claimsPrincipal;
                    //#Next Claims

                    var identity = ((ClaimsIdentity)Thread.CurrentPrincipal.Identity);

                    var c_user_id = identity.Claims.Where(x => x.Type == "user_id").FirstOrDefault();
                    var UserId = c_user_id.Value;

                    var c_device_type = identity.Claims.Where(x => x.Type == "device_type").FirstOrDefault();
                    var DeviceType = c_device_type.Value;

                    var c_updated_at = identity.Claims.Where(x => x.Type == "updated_at").FirstOrDefault();
                    var UpdatedAt = c_updated_at.Value;

                    var c_created_at = identity.Claims.Where(x => x.Type == "created_at").FirstOrDefault();
                    var CreatedAt = c_created_at.Value;

                    var c_no_cliente = identity.Claims.Where(x => x.Type == "no_cliente").FirstOrDefault();
                    var NoCliente = c_no_cliente.Value;

                    var c_RFC = identity.Claims.Where(x => x.Type == "RFC").FirstOrDefault();
                    var RFCCliente = c_RFC.Value;

                    var c_nombre = identity.Claims.Where(x => x.Type == ClaimTypes.Surname).FirstOrDefault();
                    var Nombre = c_nombre.Value;
                    //#endif Claims

                    var caller = User as ClaimsPrincipal;

                    var subjectClaim = caller.FindFirst("sub");

                    if (subjectClaim != null)
                    {
                        return Json(new
                        {
                            No_Cliente = NoCliente,
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            message = "OK computer",
                            client = caller.FindFirst("client_id").Value
                        });
                    }
                }
                else
                {
                    sqlConexion.Close();
                    return Json(new
                    {
                        message = "User Not Found"
                    });
                }
            }
            else if (Url_User.Substring(1, 6) == "Nombre")
            {
                string Nombre_User = Url_User.Substring(8);

                var sqlConexionString = ConfigurationManager.ConnectionStrings["ABCLeasingABC"].ConnectionString;

                SqlConnection sqlConexion = new SqlConnection(sqlConexionString);
                string sqlConsulta = "SELECT user_id, device_type, updated_at, created_at, no_cliente, RFC, nombre FROM ClientesABC WHERE UPPER(nombre) LIKE '%" + Nombre_User.ToUpper() + "%'";

                sqlConexion.Open();

                SqlCommand sqlComando = new SqlCommand(sqlConsulta, sqlConexion);

                SqlDataReader sqlDatos = sqlComando.ExecuteReader();
                if (sqlDatos.Read())
                {
                    var listClientes = new List<Cliente>();

                    while (sqlDatos.Read())
                    {
                        user_id = sqlDatos["user_id"].ToString();
                        device_type = sqlDatos["device_type"].ToString();
                        updated_at = string.Format("{0:d}", sqlDatos["updated_at"]);
                        created_at = string.Format("{0:d}", sqlDatos["created_at"]);
                        no_cliente = sqlDatos["no_cliente"].ToString();
                        RFC = sqlDatos["RFC"].ToString();
                        nombre = sqlDatos["nombre"].ToString();

                        //#Sección de Claims
                        var claims = new List<Claim>();
                        claims.Add(new Claim("user_id", user_id));
                        claims.Add(new Claim("device_type", device_type));
                        claims.Add(new Claim("updated_at", updated_at));
                        claims.Add(new Claim("created_at", created_at));
                        claims.Add(new Claim("no_cliente", no_cliente));
                        claims.Add(new Claim("RFC", RFC));
                        claims.Add(new Claim(ClaimTypes.Surname, nombre));

                        var claimsIdentity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                        Thread.CurrentPrincipal = claimsPrincipal;
                        //#Next Claims

                        var identity = ((ClaimsIdentity)Thread.CurrentPrincipal.Identity);

                        var c_user_id = identity.Claims.Where(x => x.Type == "user_id").FirstOrDefault();
                        var UserId = c_user_id.Value;

                        var c_device_type = identity.Claims.Where(x => x.Type == "device_type").FirstOrDefault();
                        var DeviceType = c_device_type.Value;

                        var c_updated_at = identity.Claims.Where(x => x.Type == "updated_at").FirstOrDefault();
                        var UpdatedAt = c_updated_at.Value;

                        var c_created_at = identity.Claims.Where(x => x.Type == "created_at").FirstOrDefault();
                        var CreatedAt = c_created_at.Value;

                        var c_no_cliente = identity.Claims.Where(x => x.Type == "no_cliente").FirstOrDefault();
                        var NoCliente = c_no_cliente.Value;

                        var c_RFC = identity.Claims.Where(x => x.Type == "RFC").FirstOrDefault();
                        var RFCCliente = c_RFC.Value;

                        var c_nombre = identity.Claims.Where(x => x.Type == ClaimTypes.Surname).FirstOrDefault();
                        var Nombre = c_nombre.Value;
                        //#endif Claims

                        var caller = User as ClaimsPrincipal;

                        var subjectClaim = caller.FindFirst("sub");

                        listClientes.Add(new Cliente {
                            ClienteNumero = NoCliente,
                            ClienteNombre = Nombre,
                            ClienteRFC = RFCCliente
                        });
                    }

                    sqlConexion.Close();

                    return Json(listClientes);
                }
                else
                {
                    sqlConexion.Close();
                    return Json(new
                    {
                        message = "User Not Found"
                    });
                }
            }
            else
            {
                return Json(new
                {
                    message = "Option Not Validate"
                });
            }
        }

        [HttpGet]
        [Route("api/user_client")]
        [Authorize]
        public IHttpActionResult NumberClient(string no_cliente)
        {
            try
            {
                var sqlConexionString = ConfigurationManager.ConnectionStrings["ABCLeasingABC"].ConnectionString;
                using (SqlConnection sqlConexion = new SqlConnection(sqlConexionString))
                {
                    string sqlConsulta = "SELECT user_id, device_type, updated_at, created_at, no_cliente, RFC, nombre FROM ClientesABC WHERE UPPER(no_cliente) = '" + no_cliente.ToUpper() + "'";

                    sqlConexion.Open();

                    SqlCommand sqlComandoSelect = new SqlCommand(sqlConsulta, sqlConexion);

                    SqlDataReader sqlDatos = sqlComandoSelect.ExecuteReader();

                    var ClienteRFC = "No Existe El Cliente";
                    if (sqlDatos.Read())
                    {
                        ClienteRFC = sqlDatos["rfc"].ToString();
                    }

                    sqlDatos.Close();
                    sqlComandoSelect.Dispose();
                    sqlConexion.Close();

                    return Json(new
                    {
                        RFC = ClienteRFC,
                    });
                }
            }
            catch (Exception)
            {
                Exception newEx = new Exception("Error al consultar");
                throw newEx;
            }
        }

        [HttpPost]
        [Route("api/add_cliente/abc")]
        [Authorize]
        public IHttpActionResult Post([FromBody] ClienteABC new_user)
        {
            try
            {
                AES aES = new AES();

                var sqlConexionString = ConfigurationManager.ConnectionStrings["ABCLeasingABC"].ConnectionString;
                using (SqlConnection sqlConexion = new SqlConnection(sqlConexionString))
                {
                    //Confirmar que no exista el Cliente
                    string sqlConsulta = "SELECT * FROM ClientesABC WHERE UPPER(RFC) = '" + new_user.RFC.ToUpper() + "' OR no_cliente = '" + new_user.no_cliente.ToString() + "'";

                    sqlConexion.Open();

                    SqlCommand sqlComandoSelect = new SqlCommand(sqlConsulta, sqlConexion);

                    SqlDataReader sqlDatos = sqlComandoSelect.ExecuteReader();

                    //Si NO existe
                    if (!sqlDatos.Read())
                    {
                        sqlConexion.Close();
                        string PassDecrypt = new_user.password.ToString();
                        string PassEncrypt = aES.OpenSSLEncrypt(PassDecrypt);
                        //Guarda al nuevo usuario
                        sqlConsulta = "INSERT INTO ClientesABC (created_at, no_cliente, RFC, password, nombre) VALUES (@created_at, @no_cliente, @RFC, @password, @nombre)";

                        sqlConexion.Open();
                        SqlCommand sqlComandoInsert = new SqlCommand(sqlConsulta, sqlConexion);

                        sqlComandoInsert.Parameters.AddWithValue("@created_at", DateTime.Now);
                        sqlComandoInsert.Parameters.AddWithValue("@no_cliente", new_user.no_cliente.ToString());
                        sqlComandoInsert.Parameters.AddWithValue("@RFC", new_user.RFC.ToUpper());
                        sqlComandoInsert.Parameters.AddWithValue("@password", PassEncrypt);
                        sqlComandoInsert.Parameters.AddWithValue("@nombre", new_user.nombre.ToUpper());

                        sqlComandoInsert.ExecuteNonQuery();
                        //#end Guardar
                        sqlConexion.Close();
                        return Json(new
                        {
                            message = "Cliente ABC Agregado Correctamente"
                        });
                    }
                    else
                    {
                        sqlConexion.Close();
                        return Json(new
                        {
                            message = "El Cliente ABC Ya Existe"
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

        [HttpPost]
        [Route("api/add_cliente/pocket")]
        [Authorize]
        public IHttpActionResult Post2([FromBody] Cliente new_user)
        {
            try
            {
                AES aES = new AES();

                var sqlConexionString = ConfigurationManager.ConnectionStrings["ABCLeasingABC"].ConnectionString;
                using (SqlConnection sqlConexion = new SqlConnection(sqlConexionString))
                {
                    //Confirmar que no exista el Cliente en UsersPocketABC
                    string sqlConsulta = "SELECT * FROM UsersPocketABC WHERE UPPER(RFC) = '" + new_user.RFC.ToUpper() + "'";

                    sqlConexion.Open();

                    SqlCommand sqlComandoSelect = new SqlCommand(sqlConsulta, sqlConexion);

                    SqlDataReader sqlDatos = sqlComandoSelect.ExecuteReader();

                    //Si NO existe
                    if (!sqlDatos.Read())
                    {
                        sqlConexion.Close();
                        //Guarda al nuevo usuario
                        sqlConsulta = "INSERT INTO UsersPocketABC (RFC, contratos, telefonos, email, intento, creado) VALUES (@RFC, @contratos, @telefonos, @email, @intento, @creado)";

                        sqlConexion.Open();
                        SqlCommand sqlComandoInsert = new SqlCommand(sqlConsulta, sqlConexion);

                        sqlComandoInsert.Parameters.AddWithValue("@RFC", new_user.RFC.ToUpper());
                        sqlComandoInsert.Parameters.AddWithValue("@contratos", new_user.contratos.ToUpper());
                        sqlComandoInsert.Parameters.AddWithValue("@telefonos", new_user.telefonos.ToString());
                        sqlComandoInsert.Parameters.AddWithValue("@email", new_user.email.ToUpper()); 
                        sqlComandoInsert.Parameters.AddWithValue("@intento", 0);           
                        sqlComandoInsert.Parameters.AddWithValue("@creado", false);

                        sqlComandoInsert.ExecuteNonQuery();
                        //#end Guardar
                        sqlConexion.Close();
                        return Json(new
                        {
                            message = "Cliente ABC Pocket Agregado Correctamente"
                        });
                    }
                    else
                    {
                        sqlConexion.Close();
                        return Json(new
                        {
                            message = "El Cliente ABC Pocket Ya Existe"
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


        // PUT api/values/5
        public void Put(int id, [FromBody] string value)
        {

        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
    public class Cliente
    {
        public string ClienteNumero { get; set; }
        public string ClienteNombre { get; set; }
        public string ClienteRFC { get; set; }
        public string RFC { get; set; }
        public string contratos { get; set; }
        public string telefonos { get; set; }
        public string email { get; set; }
    }
}
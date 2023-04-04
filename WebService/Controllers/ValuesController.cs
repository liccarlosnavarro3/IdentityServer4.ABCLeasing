using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Http;

using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Http.Cors;

namespace WebService.Controllers
{
    //[EnableCors(origins: "https://authfacttest.abcleasing.com.mx", headers: "*", methods: "*")]
    //[EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ValuesController : ApiController
    {
        [Authorize]
        public IHttpActionResult Get()
        {

            System.Net.Http.Headers.HttpRequestHeaders headers = this.Request.Headers;

            string authorization = string.Empty;
            string token = string.Empty;

            if (headers.Contains("Authorization"))
            {
                authorization = headers.GetValues("Authorization").First();
                token = authorization.Substring(7);
            }

            //#Sección de Buscar Usuario
            string user_id;
            string device_type;
            string updated_at;
            string created_at;
            string no_cliente;
            string RFC;
            string nombre;

            var sqlConexionString = ConfigurationManager.ConnectionStrings["ABCLeasingABC"].ConnectionString;
            SqlConnection sqlConexion = new SqlConnection(sqlConexionString);
            
            string sqlConsulta = "SELECT user_id, device_type, updated_at, created_at, no_cliente, RFC, nombre FROM ClientesABC WHERE token = @token";

            sqlConexion.Open();

            SqlCommand sqlComando = new SqlCommand(sqlConsulta, sqlConexion);

            sqlComando.Parameters.AddWithValue("@token", token.ToString());

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
                //#end Buscar

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
                //#endif Claims

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
                        message = "OK User",
                        IdClient = subjectClaim.Value,
                        Client = caller.FindFirst("client_id").Value,
                        User_Id = UserId,
                        Device_Type = DeviceType,
                        Updated_At = UpdatedAt,
                        Created_At = CreatedAt,
                        No_Cliente = NoCliente,
                        RFC_Cliente = RFCCliente,
                        Nombre_Cliente = Nombre,
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
            // Sino existe
            else
            {
                sqlConexion.Close();
                return Json(new
                {
                    message = "Token no válido"
                });
            }
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
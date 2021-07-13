using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ABCLeasing.OAuth
{
    public class ABCLeasingUserService : UserServiceBase 
    {
        ABCLeasingHelper _SQLHelper = new ABCLeasingHelper();

        public class CustomUser
        {
            //public string Username { get; set; }
            //public string Password { get; set; }
            //public int UsuarioId { get; set; }
            //public string Email { get; set; }
            //public string Role { get; set; }
            //public string Nombre { get; set; }
            //public string RFC { get; set; }
            //public string NumCliente { get; set; }
            public int user_id { get; set; }
            //public string device_type { get; set; }
            public DateTime created_at { get; set; }
            public string no_cliente { get; set; }
            public string RFC { get; set; }

            public string password { get; set; }
            public string nombre { get; set; }
            public List<Claim> Claims { get; set; }
        }


        public ABCLeasingUserService()
        {
            _SQLHelper.Connect();
        }

        public static List<CustomUser> Users = new List<CustomUser>();
        public override Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            List<CustomUser> users = _SQLHelper.GetClassList<CustomUser>("SELECT user_id, created_at, no_cliente, RFC, password, nombre FROM ClientesABC WHERE UPPER(RFC) = '" + context.UserName.ToUpper() + "'");
            if (users.Count > 0)
            {
                AES aES = new AES();
                CustomUser user = users[0];
                //Línea para Encriptar contraseñas
                //context.AuthenticateResult = new AuthenticateResult(aES.OpenSSLEncrypt(context.Password));
                //if (aES.OpenSSLDecrypt(user.Password) == aES.OpenSSLDecrypt(context.Password))
                if (aES.OpenSSLDecrypt(user.password) == context.Password)
                //if (user.Password == aES.OpenSSLEncrypt(context.Password))
                {
                    //context.AuthenticateResult = new AuthenticateResult(aES.OpenSSLEncrypt(context.Password));
                    //context.AuthenticateResult = new AuthenticateResult(context.Password, context.UserName);
                    //UserCredential cred = new UserCredential(context.UserName, context.Password);
                    //return _context.AcquireToken(_config.ServiceUrl, _config.ClientId, cred);


                    //context.AuthenticateResult = new AuthenticateResult(user.user_id.ToString(), user.RFC);
                    //#Sección de Guardar Usuario
                    var sqlConexionString = ConfigurationManager.ConnectionStrings["ABCLeasingABC"].ConnectionString;
                    SqlConnection sqlConexion = new SqlConnection(sqlConexionString);

                    //Borra tabla temporal
                    string sqlConsulta = "TRUNCATE TABLE ClientesABCtmp";
                    sqlConexion.Open();
                    SqlCommand sqlComando2 = new SqlCommand(sqlConsulta, sqlConexion);
                    sqlComando2.ExecuteNonQuery();
                    sqlConexion.Close();

                    //Guarda al nuevo usuario
                    sqlConsulta = "INSERT INTO ClientesABCtmp (user_id, updated_at, created_at, no_cliente, RFC, nombre) VALUES (@user_id, @updated_at, @created_at, @no_cliente, @RFC, @nombre)";
                    sqlConexion.Open();
                    SqlCommand sqlComando = new SqlCommand(sqlConsulta, sqlConexion);

                    sqlComando.Parameters.AddWithValue("@user_id", user.user_id);
                    //sqlComando.Parameters.AddWithValue("@device_type", user.device_type);
                    sqlComando.Parameters.AddWithValue("@updated_at", user.created_at);
                    sqlComando.Parameters.AddWithValue("@created_at", user.created_at);
                    sqlComando.Parameters.AddWithValue("@no_cliente", user.no_cliente);
                    sqlComando.Parameters.AddWithValue("@RFC", user.RFC);
                    sqlComando.Parameters.AddWithValue("@nombre", user.nombre);

                    sqlComando.ExecuteNonQuery();
                    sqlConexion.Close();
                    //#end Guardar
                    _SQLHelper.Close();
                    context.AuthenticateResult = new AuthenticateResult(user.user_id.ToString(), user.RFC);
                }
                else
                {
                    context.AuthenticateResult = new AuthenticateResult("Incorrect Credentials: Password incorrect");
                    //context.AuthenticateResult = new AuthenticateResult(aES.OpenSSLEncrypt(context.Password));
                }
                //return;
            }
            else
            {
                context.AuthenticateResult = new AuthenticateResult("Incorrect Credentials: Username not found");
                //return;
            }
            _SQLHelper.Close();

            return Task.FromResult(0);
        }

        public override Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var identity = new ClaimsIdentity();

            List<CustomUser> users = _SQLHelper.GetClassList<CustomUser>("SELECT user_id, created_at, no_cliente, RFC, password, nombre FROM ClientesABC WHERE UPPER(RFC) = '" + context.Subject.Identity.Name.ToUpper() + "'");
            if (users.Count > 0)
            {
                CustomUser user = users[0];
                //#Sección de Claims
                identity.AddClaims(new[]
                {
                    new Claim("NoCliente", user.no_cliente)
                });
            }
            _SQLHelper.Close();
            context.IssuedClaims = identity.Claims; //<- MAKE SURE you add the claims here

            return Task.FromResult(identity.Claims);
        }
    }
}
using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Cliente
{
    public class Program
    {
        public static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();
        //{
        //    //var response = GetClientToken(); 
        //    var response = GetUserToken();
        //    CallApi(response);
        //}
        private static async Task MainAsync()
        {
            var disco = await DiscoveryClient.GetAsync("http://localhost:55503");

            if (disco.IsError)
            {
                throw new Exception("Error al acceder al servidor");
            }

            var TokenCliente = new TokenClient(
                "http://localhost:55503/connect/token",
                "ABCLeasing",
                "785BFB36-9A18-4D29-94A5-18FE176F2E6E");

            var TokenResponse = TokenCliente.RequestResourceOwnerPasswordAsync("Admin", "abcadmin", "ABCLeasingAPI").Result;

            if (TokenResponse.IsError)
            {
                throw new Exception("Error al obtener el token");
            }

            var TokenJson = TokenResponse.Json;

            var client = new HttpClient();
            client.SetBearerToken(TokenResponse.AccessToken);

            var response = await client.GetAsync("http://localhost:59655/api/Values");

            //Console.WriteLine(client.GetStringAsync("http://localhost:59655/api/Values").Result);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error al acceder a la API");
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
            }
        }
    }
}
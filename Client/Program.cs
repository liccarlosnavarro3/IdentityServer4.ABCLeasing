using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            //var response = GetClientToken(); 
            var response = GetUserToken();
            CallApi(response);
        }

        static TokenResponse GetClientToken()
        {
            var client = new TokenClient(
                "http://localhost:55503/connect/token",
                "ABCLeasing",
                "785BFB36-9A18-4D29-94A5-18FE176F2E6E");

            return client.RequestClientCredentialsAsync("ABCLeasingAPI").Result;
        }

        static TokenResponse GetUserToken()
        {
            var client = new TokenClient(
                "http://localhost:55503/connect/token",
                "ABCLeasing",
                "785BFB36-9A18-4D29-94A5-18FE176F2E6E");

            return client.RequestResourceOwnerPasswordAsync("Admin", "abcadmin", "ABCLeasingAPI").Result;
        }
        static void CallApi(TokenResponse response)
        {
            var client = new HttpClient();
            client.SetBearerToken(response.AccessToken);

            var Respuesta = client.GetStringAsync("http://localhost:59655/api/Values").Result;
            Console.WriteLine(Respuesta);
        }
    }
}

using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;

namespace IdentityServer4Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var disco = DiscoveryClient.GetAsync("http://localhost:49814").Result;
            TokenClient tokenClient = new TokenClient(disco.TokenEndpoint, "client", "secret");
            var tokenResponse = tokenClient.RequestClientCredentialsAsync("api1").Result;

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            Console.WriteLine(tokenResponse.Json);

            var client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);

            var response = client.GetAsync("http://localhost:50003/identity").Result;
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(JArray.Parse(content));
            }

            tokenClient = new TokenClient(disco.TokenEndpoint, "ro.client", "secret");
            tokenResponse = tokenClient.RequestResourceOwnerPasswordAsync("nick", "password", "api1").Result;

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            Console.WriteLine(tokenResponse.Json);

            client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);

            response = client.GetAsync("http://localhost:50003/identity").Result;
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(JArray.Parse(content));
            }
        }
    }
}

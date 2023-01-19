using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            GetAuthorizationToken();
            createResourceGroup().GetAwaiter().GetResult();
            getAllResourceGroupDetails();
            Console.WriteLine(AzureDetails.Response);
        }

        public static void GetAuthorizationToken()
        {
            ClientCredential cc = new ClientCredential(AzureDetails.ClientID, AzureDetails.ClientSecret);
            var context = new AuthenticationContext("https://login.microsoftonline.com/" + AzureDetails.TenantID);
            var result = context.AcquireTokenAsync("https://management.azure.com/", cc);
            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the Access token");
            }
            AzureDetails.AccessToken = result.Result.AccessToken;
        }

        public static async Task getAllResourceGroupDetails()
        {
            try
            {
                HttpClient client = new HttpClient();
                //  client.BaseAddress = new Uri("https://management.azure.com/subscriptions/" + AzureDetails.SubscriptionID + "/resourcegroups?api-version=2019-10-01");
                client.BaseAddress = new Uri("https://api.businesscentral.dynamics.com/v2.0/4dfedb10-35ca-4e46-9c2a-0fa40d6968c0/Sandbox2/WS/NGO%20Demo/Codeunit/WebPortal");
                client.DefaultRequestHeaders.Accept.Clear();

                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + AzureDetails.token);

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, client.BaseAddress);

                var response = await MakeRequestAsync(request, client);
                AzureDetails.Response = response;
            }
            catch(Exception es)
            {
                Console.WriteLine(es.Message);
            }
        }

        public static async Task createResourceGroup()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://management.azure.com/subscriptions/" + AzureDetails.SubscriptionID + "/resourcegroups/" + AzureDetails.ResourceGroupName + "?api-version=2019-10-01");
            client.DefaultRequestHeaders.Accept.Clear();

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + AzureDetails.AccessToken);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, client.BaseAddress);
            var body = $"{{\"location\": \"{AzureDetails.Location}\"}}";
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            request.Content = content;
            var response = await MakeRequestAsync(request, client);
            AzureDetails.Response = response;
        }

        public static async Task<string> MakeRequestAsync(HttpRequestMessage getRequest, HttpClient client)
        {
            var response = await client.SendAsync(getRequest).ConfigureAwait(false);
            var responseString = string.Empty;
            try
            {
                response.EnsureSuccessStatusCode();
                responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (HttpRequestException)
            {
                
            }

            return responseString;
        }
    }
}

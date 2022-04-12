using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using BTS.AzAD.WCFAdapaterEx;
using System.ServiceModel.Channels;
using System.Text;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace BTS.AzAD.WCFAdapterEx.Test
{
    [TestClass]
    public class AdapterExTest
    {
        const string clientId = "2b1d9843-99f8-41f9-b098-d1b9610008f1"; //"ec09904f-0164-4176-a0ce-02749513c8e1";
        const string tenantId = "544d9264-3ce2-4aec-aa71-34d87c705442"; //"8614f00a-7ea9-4b2a-bba6-160b29e5a7a7";
        const string clientSecret = "NTS7Q~LlAfjaFRoIPAEssHey3bM67ipQI287p"; //"ZOA7Q~CedNEUVVXuhkulaceltsidzM4k5r3ki";

        readonly IServiceCollection serviceCollection = new ServiceCollection();
        readonly IHttpClientFactory clientFactory;
        public AdapterExTest()
        {
            var provider =  serviceCollection.AddHttpClient().BuildServiceProvider();
            clientFactory = provider.GetRequiredService<IHttpClientFactory>();
        }

        [TestMethod]
        public void TestBehaviourEx()
        {
            var _adapterBehovior = new WCFAdapaterEx.AzureADSecurityBehaviour(clientFactory, grantType: "client_credentials", tenantId: tenantId, 
                                                                clientId: clientId, clientSecret: clientSecret, 
                                                                resouceId: "https%3A%2F%2Fservicebus.azure.net", userName: "n/a", userPassword: "n/a");
            var msg = Message.CreateMessage(MessageVersion.Default, "");
            _adapterBehovior.BeforeSendRequest(ref msg, null);

        }

        [TestMethod]
        public void TestFetchingToken()
        {
            const string _tenantId = tenantId;
            const string _authorityUri = "https://login.microsoftonline.com/{0}/oauth2/token";
            string body = "grant_type={0}&client_id={1}&client_secret={2}&resource={3}";

            body = String.Format(body, "client_credentials", clientId, clientSecret, "https%3A%2F%2Fservicebus.azure.net");
            var __authUri = String.Format(_authorityUri, _tenantId);

            string result;

            try
            {
                result = HttpPost(__authUri, body).Result.ToString();
            }
            catch (WebException)
            {

                throw;
            }

            JavaScriptSerializer ser = new JavaScriptSerializer();
            var _token = ser.Deserialize<AdapterAuthToken>(result);
        }

        private async Task<string> HttpPost(string authUri, string p)
        {

                HttpContent c = new StringContent(p, Encoding.UTF8, "application/x-www-form-urlencoded");

                var res = await clientFactory.CreateClient().PostAsync(authUri, c);

                return res.Content.ReadAsStringAsync().Result;            
        }
    }
}

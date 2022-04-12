using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Net.Http;

namespace BTS.AzAD.WCFAdapaterEx
{
    public class AzureADSecurityBehaviour : IClientMessageInspector, IEndpointBehavior
    {
        private const string _authorityUri = "https://login.microsoftonline.com/{0}/oauth2/token";
        private string _userName;
        private string _userPwd;
        private string _grantType;
        private string _clientId;
        private string _clientSecret;
        private string _resourceId;
        private string _tenantid;

        private static DateTime? _tokenExpieryTime;
        private readonly object _getTokenThreadLock = new object();

        private readonly HttpClient _httpClient;

        // internal properties
        private static AdapterAuthToken _token;

        public AzureADSecurityBehaviour(IHttpClientFactory httpClientFactory, string grantType,string tenantId, string clientId, string clientSecret, string resouceId, string userName, string userPassword)
        {
            _grantType = grantType;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _resourceId = resouceId;
            _userName = userName;
            _userPwd = userPassword;
            _tenantid = tenantId;

            _httpClient = httpClientFactory.CreateClient();
        }

        #region IClientMessageInspector
        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            //do nothing
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            HttpRequestMessageProperty httpRequest = null;

            if (request.Properties.ContainsKey(HttpRequestMessageProperty.Name))
            {
                httpRequest = request.Properties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
            }

            if (httpRequest == null)
            {
                httpRequest = new HttpRequestMessageProperty()
                {
                    Method = "GET",
                    SuppressEntityBody = true
                };

                request.Properties.Add(HttpRequestMessageProperty.Name, httpRequest);
            }

            WebHeaderCollection headers = httpRequest.Headers;

            if (TokenHasExpired())
            {
                FetchAuthToken();
            }

            headers.Add(HttpRequestHeader.ContentType, "application/json");
            headers.Add(HttpRequestHeader.Authorization, "Bearer " + _token.access_token);

            return null;
        }

        private void FetchAuthToken()
        {
            lock (this._getTokenThreadLock)
            {
                if (!TokenHasExpired())
                {
                    return;
                }

                string body = "grant_type={0}&client_id={1}&client_secret={2}&resource={3}";

                body = String.Format(body, _grantType, _clientId, _clientSecret, _resourceId);

                if (_grantType == "password".ToLower())
                {
                    body = body + "&username={0}&password={1}";
                    body = String.Format(body, _userName, _userPwd);
                }

                var __authUri = String.Format(_authorityUri, _tenantid);

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
                _token = ser.Deserialize<AdapterAuthToken>(result);

                int.TryParse(_token.expires_in, out int expiresInSeconds);

                _tokenExpieryTime = DateTime.Now.AddSeconds(expiresInSeconds);

            }        
        }

        private bool TokenHasExpired()
        {
            if (_tokenExpieryTime == null || !_tokenExpieryTime.HasValue)
                return true;

            if (DateTime.Now.AddSeconds(-300) >= _tokenExpieryTime.Value)
                return true;

            return false;
        }

        private async Task<string> HttpPost(string authUri, string p)
        {

                HttpContent c = new StringContent(p, Encoding.UTF8, "application/x-www-form-urlencoded");

                var res = await _httpClient.PostAsync(authUri, c);

                return res.Content.ReadAsStringAsync().Result;

        }

        #endregion IClientMessageInspector

        #region IEndpointBehaviour
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            // do nothing
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(this);
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            // do nothing
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            // do nothing
        }
        #endregion IEndpointBehaviour
    }
}

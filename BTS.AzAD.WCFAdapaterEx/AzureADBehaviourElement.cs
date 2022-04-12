using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Configuration;
using System.Configuration;
using System.Net.Http;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BTS.AzAD.WCFAdapaterEx
{
    public class AzureADBehaviourElement : BehaviorExtensionElement
    {
        readonly IServiceCollection serviceCollection = new ServiceCollection();
        readonly IHttpClientFactory httpClientFactory;

        public override Type BehaviorType
        {
            get { return typeof(AzureADSecurityBehaviour); }
        }

        public AzureADBehaviourElement()
        {
           var provider = serviceCollection.AddHttpClient().BuildServiceProvider();

            httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
        }

        protected override object CreateBehavior()
        {
           return new AzureADSecurityBehaviour(httpClientFactory, GrantType,TenantId,ClientId,ClientSecret,ResourceId,Username,Password);
        }

        [ConfigurationProperty("granttype", IsRequired = true)]
        public string GrantType
        {
            get { return (string)this["granttype"]; }
            set { this["granttype"] = value; }
        }

        [ConfigurationProperty("tenantId", IsRequired = true)]
        public string TenantId
        {
            get { return (string)this["tenantId"]; }
            set { this["tenantId"] = value; }
        }

        [ConfigurationProperty("username", IsRequired = false)]
        public string Username
        {
            get { return (string)this["username"]; }
            set { this["username"] = value; }
        }

        [ConfigurationProperty("password", IsRequired = false)]
        public string Password
        {
            get { return (string)this["password"]; }
            set { this["password"] = value; }
        }

        [ConfigurationProperty("clientId", IsRequired = true)]
        public string ClientId
        {
            get { return (string)this["clientId"]; }
            set { this["clientId"] = value; }
        }

        [ConfigurationProperty("clientsecret", IsRequired = true)]
        public string ClientSecret
        {
            get { return (string)this["clientsecret"]; }
            set { this["clientsecret"] = value; }
        }

        [ConfigurationProperty("resourceId", IsRequired = true)]
        public string ResourceId
        {
            get { return (string)this["resourceId"]; }
            set { this["resourceId"] = value; }
        }
    }
}

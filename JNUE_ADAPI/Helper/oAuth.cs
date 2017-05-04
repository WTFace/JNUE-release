using JNUE_ADAPI.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace JNUE_ADAPI.Helper
{
    public class oAuth
    {
        static MediaTypeWithQualityHeaderValue Json = new MediaTypeWithQualityHeaderValue("application/json");
        
        public static async Task<string> getSessionToken()
        {
            AuthenticationContext authenticationContext = new AuthenticationContext(Properties.AzADAuthority, false);
            ClientCredential clientCred = new ClientCredential(Conn.AzClientID, Conn.AzClientSecret);
            AuthenticationResult authenticationResult = await authenticationContext.AcquireTokenAsync("https://graph.windows.net", clientCred);
            return authenticationResult.AccessToken;
        }
        
        public static async Task<string> getSessionToken(string resourceid)
        {
            AuthenticationContext authenticationContext = new AuthenticationContext(Properties.AzADAuthority, false);
            ClientCredential clientCred = new ClientCredential(Conn.AzClientID, Conn.AzClientSecret);
            AuthenticationResult authenticationResult = await authenticationContext.AcquireTokenAsync(resourceid, clientCred);
            return authenticationResult.AccessToken;
        }
        
        public static async Task<OfficeUser> getUserInfoAsync(string accessToken)
        {
            OfficeUser myInfo = new OfficeUser();
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me"))
                {
                    request.Headers.Accept.Add(Json);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    using (var response = await client.SendAsync(request))
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
                            myInfo.name = json?["displayName"]?.ToString();
                            myInfo.address = json?["mail"]?.ToString().Trim().Replace(" ", string.Empty);
                        }
                    }
                }
            }
            return myInfo;
        }
        
        public static async Task<string> Authorize()
        {
            AuthenticationContext authenticationContext = new AuthenticationContext(Properties.AzADAuthority, false);
            ClientCredential clientCred = new ClientCredential(Conn.AzClientID, Conn.AzClientSecret);
            AuthenticationResult authenticationResult = await authenticationContext.AcquireTokenAsync("https://graph.microsoft.com/", clientCred);
            return authenticationResult.AccessToken;
        }
        
        public static async Task<string> GetTokenHelperAsync(AuthenticationContext context, string resourceId)
        {
            string accessToken = null;
            AuthenticationResult result = null;
            string myId = Conn.AzClientID;
            string myKey = Conn.AzClientSecret;
            ClientCredential client = new ClientCredential(myId, myKey);

            result = await context.AcquireTokenAsync(resourceId, client);
            accessToken = result.AccessToken;
            return accessToken;
        }

        
        public static async Task<string> getLoginToken(string userid, string passwd)
        {
            string res = "";
            string username = string.Format("{0}@{1}", userid, Properties.AzDomainUrl);
            List<KeyValuePair<string, string>> vals = new List<KeyValuePair<string, string>>();

            vals.Add(new KeyValuePair<string, string>("grant_type", "password"));
            vals.Add(new KeyValuePair<string, string>("scope", "openid"));
            vals.Add(new KeyValuePair<string, string>("resource", "https://outlook.office365.com/"));
            vals.Add(new KeyValuePair<string, string>("client_id", Conn.AzClientID));
            vals.Add(new KeyValuePair<string, string>("client_secret", Conn.AzClientSecret));
            vals.Add(new KeyValuePair<string, string>("username", username));
            vals.Add(new KeyValuePair<string, string>("password", passwd));

            string url = string.Format("https://login.microsoftonline.com/{0}/oauth2/token", Properties.AzDomainUrl);

            HttpClient client = new HttpClient();
            HttpContent content = new FormUrlEncodedContent(vals);
            using (var response = await client.PostAsync(url, content))
            {
                if (response.Content != null)
                {
                    //var headers = response.Headers.ToString();
                    res = await response.Content.ReadAsStringAsync();
                    //res = headers;
                }
            };
            dynamic result = JObject.Parse(res);
            return result.access_token;
        }
        
        public static async Task<string> getChangeCodeToToken(string code)
        {
            string res = "";
            List<KeyValuePair<string, string>> vals = new List<KeyValuePair<string, string>>();
            vals.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            vals.Add(new KeyValuePair<string, string>("code", code));
            vals.Add(new KeyValuePair<string, string>("scope", "openid"));
            vals.Add(new KeyValuePair<string, string>("client_id", Conn.AzClientID));
            vals.Add(new KeyValuePair<string, string>("client_secret", Conn.AzClientSecret));
            vals.Add(new KeyValuePair<string, string>("redirect_uri", "https://portal.office.com"));

            string url = string.Format("https://login.windows.net/{0}/oauth2/token", Properties.AzDomainUrl);

            HttpClient client = new HttpClient();
            HttpContent content = new FormUrlEncodedContent(vals);
            using (var response = await client.PostAsync(url, content))
            {
                if (response.Content != null)
                {
                    res = await response.Content.ReadAsStringAsync();
                }
            };
            dynamic result = JObject.Parse(res);
            return result.access_token;
        }
        
        public static async Task<string> sessionLogin(string userid, string passwd)
        {
            string res = "";
            string username = string.Format("{0}@{1}", userid, Properties.AzDomainUrl);
            List<KeyValuePair<string, string>> vals = new List<KeyValuePair<string, string>>();

            vals.Add(new KeyValuePair<string, string>("UserName", username));
            vals.Add(new KeyValuePair<string, string>("Password", passwd));
            vals.Add(new KeyValuePair<string, string>("userNameInput", username));
            vals.Add(new KeyValuePair<string, string>("passwordInput", passwd));

            // TODO: office365.jnue.kr 주소 확인해야 함
            string url = string.Format("https://office365.jnue.kr/adfs/ls/?lc=1042&wa=wsignin1.0&wtrealm=urn%3afederation%3aMicrosoftOnline&wctx=estsredirect%3d2%26estsrequest%3drQIIAbNSzigpKSi20tcvyC8qSczRy09Ly0xO1UvOz9XLL0rPTAGxioS4BBT4zXep3hVzmy7EZf8lyohlFaMaTp36OYl5KZl56XqJxQUVFxgZu5hYDA1MjDYxsfo6-zp5nmCacFbuFpOgf1G6Z0p4sVtqSmpRYklmft4jJt7Q4tQi_7ycypD87NS8Scx8OfnpmXnxxUVp8Wk5-eVAAaDxBYnJJfElmcnZqSW7mFUsElNTTSyNzHQtDBMNdU2SEs10kyzNLHWNTI1SzY3MzIxNTYwPsGwIucAi8IOFcRenLXHOti9JLEpPLbFVNUpLSU1LLM0pAQsDAA2&popupui=");

            HttpClient client = new HttpClient();
            HttpContent content = new FormUrlEncodedContent(vals);
            using (var response = await client.PostAsync(url, content))
            {
                if (response.Content != null)
                {
                    res = await response.Content.ReadAsStringAsync();
                }
            };
            return res;
        }
        
        /// TODO : 이건 로컬 주소 썼는데, 필요없으면 날려야 함
        public static async Task<string> getPasswords(string userid)
        {
            string pass = "";
            string url = string.Format("http://192.168.8.4/Cert/Admin/az_srv_api/srv_pass.jsp?userid={0}&secure_key={1}", userid, Properties.LDAPSiteKey);
            HttpClient client = new HttpClient();
            using (HttpResponseMessage response = await client.PostAsync(new Uri(url), null))
            {
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                //string responseBody = await response.Content.ReadAsStringAsync();
                var responseBody = JObject.Parse(await response.Content.ReadAsStringAsync());
                var res_mesg = responseBody?["res_mesg"]?.ToString();
                var res_code = responseBody?["res_code"]?.ToString();

                if (res_code.Equals("0"))
                {
                    byte[] data = Convert.FromBase64String(res_mesg);
                    string decodedString = Encoding.UTF8.GetString(data);
                    pass = decodedString;
                }
            };

            return pass;
        }
    }
}
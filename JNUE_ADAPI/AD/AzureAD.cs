using JNUE_ADAPI.Helper;
using JNUE_ADAPI.Models;
using System;
using System.Net.Http;
using System.Web;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace JNUE_ADAPI.AD
{
    /// Azure AD
    public static class AzureAD
    {
        public static async Task getToken()
        {
            var token = await oAuth.getSessionToken();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public static async Task<string> getUser(string userid)
        {
            var token = await oAuth.getSessionToken();
            var res = "";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var request = string.Format(Properties.AzGraphApi + "users/{0}?api-version=1.6", userid);
                using (var response = client.GetAsync(request))
                {
                    response.Wait();
                    res = response.Result. IsSuccessStatusCode.ToString();
                };
            };
            return res;
        }
        
        public static async Task<string> setUsageLocation(string userid)
        {
            var token = await oAuth.getSessionToken();
            var obj = "{\"usageLocation\": \"KR\"}";
            var res = "";
            var json = new StringContent(obj, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var request = string.Format(Properties.AzGraphApi + "users/{0}?api-version=1.6", userid);

                using (var response = PatchAsync(client, request, json)) //Patch
                {
                    response.Wait();
                    res = response.ToString();
                };
            };
            return res;
        }


        public static async Task<string> setLicense(string userid, string skuid1, string skuid2, string disables)
        {
            var token = await oAuth.getSessionToken();
            var res = "";
            var obj = "";

            if (disables.Equals("") != true)
            {
                obj = "{\"addLicenses\": [{\"disabledPlans\": [" + disables + "], \"skuId\": \"" + skuid2 + "\"}, {\"disabledPlans\": [], \"skuId\": \"" + skuid1 + "\"}], \"removeLicenses\": [] }";
            }
            else
            {
                obj = "{\"addLicenses\": [{\"disabledPlans\": [], \"skuId\": \"" + skuid1 + "\"}], \"removeLicenses\": [] }"; //휴
            }

            var json = new StringContent(obj, Encoding.UTF8, "application/json");
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var request = string.Format(Properties.AzGraphApi + "users/{0}/assignLicense?api-version=1.6", userid);

                using (var response = client.PostAsync(request, json))
                {
                    response.Wait();
                    res = response.ToString();
                };
            };
            return res;
        }

        public static async Task<string> removeLicense(string userid, string removes)
        {
            var token = await oAuth.getSessionToken();
            var res = "";
            var obj = "{ \"addLicenses\":[], \"removeLicenses\":[" + removes + "] }";

            var json = new StringContent(obj, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var request = string.Format(Properties.AzGraphApi + "users/{0}/assignLicense?api-version=1.6", userid);

                using (var response = client.PostAsync(request, json))
                {
                    response.Wait();
                    res = response.Result.ToString();
                };
            };
            return res;
        }

        public static Task<HttpResponseMessage> PatchAsync(this HttpClient client, string requestUri, HttpContent content)
        {
            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = new HttpMethod("PATCH"),
                RequestUri = new Uri(client.BaseAddress + requestUri),
                Content = content
            };
            return client.SendAsync(request);
        }
    }
}

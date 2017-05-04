namespace JNUE_ADAPI.Models
{
    /// Properties
    public class Properties
    {
        public static string FacLicense { get { return "94763226-9b3c-4e75-a931-5c89701abe66"; } } // 교수
        public static string PlusLicense { get { return "c32f9321-a627-406d-a114-1f9c81aaafac"; } } // office 설치
        public static string StuLicense { get { return "314c4481-f395-4525-be8b-2ec4bb1e9d91"; } } // 학생
        public static string disables { get { return "\"e03c7e47-402c-463c-ab25-949079bedb21\",\"9b5de886-f035-4ff2-b3d8-c9127bea3620\",\"a23b959c-7ce8-4e57-9140-b90eb88a9e97\""; } }
        

        /// Azure Graph Api
        public static string AzGraphApi { get { return "https://graph.windows.net/365.jnue.ac.kr/"; } }

        /// Azure Sevice Url
        public static string AzDomainUrl { get { return "365.jnue.ac.kr"; } } //365.jnue.ac.kr

        /// TODO: Azure service url 
        public static string AzADAuthority { get { return "https://login.windows.net/5c0cc7ce-1527-4ee0-b026-305965d7f394/oauth2/token"; } }

        /// Azure Sevice Url
        public static string LDAPSiteKey { get { return "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"; } }

        /// AD Federation Server Url
        public static string ADFS_URL
        {
            get { return @"https://mso365adfs.jnue.ac.kr/adfs/ls/?lc=1033&client-request-id=13bdc15f-c955-47bb-9ec5-811ccd466607&username=hong%40365.jnue.ac.kr&wa=wsignin1.0&wtrealm=urn%3afederation%3aMicrosoftOnline&wctx=estsredirect%3d2%26estsrequest%3drQIIAeNisFLOKCkpKLbS1y_ILypJzNHLT0vLTE7VS87P1csvSs9MAbGKhLgE4g_uFQ49udt93tFGmbNuaeyrGNVw6tTPScxLycxL10ssLqi4wMjYxcRiaGBsvImJ1dfZ18nzBNOEs3K3mAT9i9I9U8KL3VJTUosSSzLz8x4x8YYWpxb55-VUhuRnp-btYlYxMzc1SU5MTdE1NUlM1TVJTLLUtTA3NtVNMbAwSkpMTE01MTE_wLIh5AKLwCsWHgNmKw4OLgEGCQYFhh8sjItYgQ7PaNdf0rJRwbW51-5_zA-xx6dY9asqvY1dXbIKzDxT3NPDQyJLDEOygstKfNPSSi3TCrTDM4y03fzSigxDwgxszawMJ7AJTWBj2sVpS5yH7UsSi9JTS2xVjdJSUtMSS3NKwMIA0&popupui="; }
        }
    }
}
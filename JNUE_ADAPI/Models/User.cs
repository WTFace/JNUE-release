namespace JNUE_ADAPI.Models
{
    /// ADAPI.User
    public class User
    {
        /// cn
        public string cn { get; set; }
        /// name
        public string name { get; set; }
        /// sAMAccountName
        public string sAMAccountName { get; set; }
        /// realName
        public string realName { get; set; }
        /// description
        public string description { get; set; }
        /// userPrincipalName
        public string userPrincipalName { get; set; }
        /// userAccountControl
        public string userAccountControl { get; set; }
        /// proxyAddresses
        public string proxyAddresses { get; set; }
        /// msExchHideFromAddressLists
        public string msExchHideFromAddressLists { get; set; }
    }
}
namespace JNUE_ADAPI.AD
{
    using log4net;
    using Models;
    using Oracle.ManagedDataAccess.Client;
    using System;
    using System.Collections.Generic;
    using System.DirectoryServices;
    using System.Linq;
    using System.Reflection;


    /// LocalAD
    public class LocalAD
    {
        readonly static ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static StntNumbCheckViewModel _StntNumbModel = new StntNumbCheckViewModel();
        private const long ADS_OPTION_PASSWORD_PORTNUMBER = 6;
        private const long ADS_OPTION_PASSWORD_METHOD = 7;
        private const int ADS_PASSWORD_ENCODE_REQUIRE_SSL = 0;
        private const int ADS_PASSWORD_ENCODE_CLEAR = 1;
        private static AuthenticationTypes AuthTypes = AuthenticationTypes.Signing | AuthenticationTypes.Sealing | AuthenticationTypes.Secure;
        private static int intPort = 389;

        
        public static bool SecureKey(string secure_key)
        {
            if (Properties.LDAPSiteKey.Equals(secure_key))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// Exists AD가 있는지 체크 
        public static bool Exists()
        {
            bool found = false;
            if (DirectoryEntry.Exists(Conn.LDAPPath))
            {
                found = true;
            }
            return found;
        }

        public static bool ExistAttributeValue(string attributeName, string value)
        {
            try
            {
                bool exist = false;
                using (DirectoryEntry entry = new DirectoryEntry(Conn.LDAPPath, Conn.LDAPUser, Conn.LDAPPassword, AuthTypes))
                {
                    DirectorySearcher ds = new DirectorySearcher(entry);
                    ds.SearchScope = SearchScope.Subtree;
                    ds.Filter = "(&(objectCategory=person)(extensionAttribute1=*))"; //O365users only
                    ds.PropertiesToLoad.Add(attributeName);
                    SearchResultCollection allResult = ds.FindAll();

                    if (allResult != null)
                    {
                        foreach (SearchResult result in allResult)
                        {
                            DirectoryEntry de = result.GetDirectoryEntry();
                            if (de.Properties[attributeName].Count > 0 && value == de.Properties[attributeName][0].ToString())
                            {
                                exist = true;
                                break;
                            }
                        }
                    }
                }
                return exist;
            }
            catch (DirectoryServicesCOMException e)
            {
                Console.Write(e.ToString());
                return false;
            }
        }

        ///  학번을 넣으면 ID 끄집어 온다.
        public static string getUserId(string stnt_numb)
        {
            try
            {
                string attr;
                DirectoryEntry entry = new DirectoryEntry(Conn.LDAPPath, Conn.LDAPUser, Conn.LDAPPassword, AuthTypes);
                DirectorySearcher ds = new DirectorySearcher(entry);
                ds.SearchScope = SearchScope.Subtree;
                ds.Filter = "(&(objectCategory=user) (extensionAttribute1=" + stnt_numb+ "))";
                ds.PropertiesToLoad.Add("userPrincipalname");
                SearchResult result = ds.FindOne();

                if (result != null)
                {                                  
                    if (result.Properties["userPrincipalName"].Count > 0)
                    {
                        attr = result.Properties["userPrincipalName"][0].ToString();
                    }
                    else{
                        attr = "NONE"; }
                }else{
                    attr = "NONE";}
                entry.Close();
                entry.Dispose();
                return attr;
            }
            catch (DirectoryServicesCOMException e){
                Console.Write(e.ToString());
                return e.ToString();
            }
        }

        ///  attributeName 명과 사용자 아이디를 넣으면 값을 끄집어 온다. 1개만
        public static string getSingleAttr(string attributeName, string stnt_numb)
        {
            try
            {
                string attr;
                DirectoryEntry entry = new DirectoryEntry(Conn.LDAPPath, Conn.LDAPUser, Conn.LDAPPassword, AuthTypes);
                DirectorySearcher ds = new DirectorySearcher(entry);
                ds.SearchScope = SearchScope.Subtree;
                ds.Filter = "(&(objectCategory=user) (extensionAttribute1=" + stnt_numb + "))";
                ds.PropertiesToLoad.Add(attributeName);
                SearchResult result = ds.FindOne();

                if (result != null)
                {
                    if (result.Properties[attributeName].Count > 0)
                    {
                        attr = result.Properties[attributeName][0].ToString();
                    }else{
                        attr = "NONE";
                    }
                }else{
                    attr = "NONE";
                } 
                entry.Close();
                entry.Dispose();
                return attr;
            }
            catch (DirectoryServicesCOMException e)
            {
                Console.Write(e.ToString());
                return "NONE";
            }
        }
        
        /// 유저아이디를 GUID로 변환
        public static string ConvertDNtoGUID(string userid)
        {
            DirectoryEntry entry = GetDirectoryEntryByUserId(userid);
            return entry.Guid.ToString();
        }
        
        ///  옷텟 형식으로 변환
        public static string ConvertGuidToOctectString(string objectGuid)
        {
            Guid guid = new Guid(objectGuid);
            byte[] byteGuid = guid.ToByteArray();
            string queryGuid = "";
            foreach (byte b in byteGuid)
            {
                queryGuid += @"\" + b.ToString("x2");
            }
            return queryGuid;
        }
        
        /// Authenticate 인증
        public static string Authenticate(string userid, string passwd)
        {
            string authentic = "false";
            try
            {
                DirectoryEntry entry = new DirectoryEntry(Conn.LDAPPath, userid, passwd, AuthTypes);
                object nativeObject = entry.NativeObject;
                authentic = "true";
            }
            catch (DirectoryServicesCOMException e)
            {
                Console.Write(e.ToString());
                return e.ToString();
            }

            return authentic;
        }
        
        /// 사용자 생성
        public static string CreateUserAccount(string userid, string passwd, string stnt_numb)
        {
            try
            {
                string oGUID = string.Empty;

                DirectoryEntry entry = new DirectoryEntry(Conn.LDAPPath, Conn.LDAPUser, Conn.LDAPPassword, AuthTypes);

                DirectoryEntry entry_ou = entry.Children.Find("OU=O365user");
                DirectoryEntry entry_cn = entry_ou.Children.Find("OU=user");
                DirectoryEntry entry_ur = entry_cn.Children.Add("CN=" + userid, "user");
                string oradb = Conn.connection;
                using (OracleConnection conn = new OracleConnection(oradb))
                {
                    Dictionary<string, string> haksa = new Dictionary<string, string>();
                    try
                    {
                        conn.Open();
                        string sql = "select user_used,role,status,stnt_knam from office365 where stnt_numb= '" + stnt_numb + "'";
                        OracleCommand cmd = new OracleCommand(sql, conn);
                        cmd.CommandType = System.Data.CommandType.Text;
                        OracleDataReader dr = cmd.ExecuteReader();
                        while (dr.Read())
                        {
                            haksa.Add("user_used", dr.GetString(0));
                            haksa.Add("role", dr.GetString(1));
                            haksa.Add("status", dr.GetString(2));
                            haksa.Add("stnt_knam", dr.GetString(3));
                        }
                        conn.Close();
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                    }
                    entry_ur.Properties["displayName"].Add(haksa["stnt_knam"]);
                    if (haksa["role"] == "학생")
                    {
                        entry_ur.Properties["employeeType"].Add("student");
                    }
                    else if (haksa["role"] == "교직원")
                    {
                        entry_ur.Properties["employeeType"].Add("faculty");
                    }
                }
                
                entry_ur.Properties["cn"].Add(userid);
                entry_ur.Properties["name"].Add(userid);
                entry_ur.Properties["sAMAccountName"].Add(userid);  
                entry_ur.Properties["userPrincipalName"].Add(userid + "@" + Properties.AzDomainUrl);
                entry_ur.Properties["description"].Add("z");
                entry_ur.Properties["mail"].Add(userid + "@"+ Properties.AzDomainUrl);
                entry_ur.Properties["proxyAddresses"].Add("SMTP:" + userid + "@" + Properties.AzDomainUrl);
                entry_ur.Properties["msExchHideFromAddressLists"].Add("TRUE");
                entry_ur.Properties["extensionAttribute1"].Add(stnt_numb);
                entry_ur.CommitChanges();

                entry_ur.Invoke("SetOption", new object[] { ADS_OPTION_PASSWORD_PORTNUMBER, intPort });
                entry_ur.Invoke("SetOption", new object[] { ADS_OPTION_PASSWORD_METHOD, ADS_PASSWORD_ENCODE_CLEAR });
                entry_ur.Invoke("SetPassword", new object[] { passwd });
                entry_ur.Invoke("Put", new object[] { "userAccountControl", 0x10000 | 0x0200 });
                entry_ur.CommitChanges();

                oGUID = entry.Guid.ToString();

                entry.Close();
                entry_ou.Close();
                entry_cn.Close();
                entry_ur.Close();

                return oGUID;

            }
            catch (DirectoryServicesCOMException e)
            {
                logger.Error(e.ToString());
                return "NONE";
            }
        }
        
        public static string setPassword(string userid, string passwd)
        {
            try
            {
                var entry = GetDirectoryEntryByUserId(userid);
                entry.Invoke("SetOption", new object[] { ADS_OPTION_PASSWORD_PORTNUMBER, intPort });
                entry.Invoke("SetOption", new object[] { ADS_OPTION_PASSWORD_METHOD, ADS_PASSWORD_ENCODE_CLEAR });
                entry.Invoke("SetPassword", new object[] { passwd });
                entry.CommitChanges();
                entry.Close();

                return "SUCCESS";
            }
            catch (DirectoryServicesCOMException e)
            {
                Console.Write(e.ToString());
                return "NONE";
            }
        }
        
        /// 개인 사용자에 대한 디렉토리 앤트리를 뽑아온다.
        public static DirectoryEntry GetDirectoryEntryByUserId(string stnt_numb)
        {
            DirectoryEntry entry = new DirectoryEntry(Conn.LDAPPath, Conn.LDAPUser, Conn.LDAPPassword, AuthTypes);

            var de = entry;
            var deSearch = new DirectorySearcher(de)
            { SearchRoot = de, Filter = "(&(objectCategory=user)(extensionAttribute1=" + stnt_numb + "))" };

            var results = deSearch.FindOne();
            return results != null ? results.GetDirectoryEntry() : null;
        }
        
        /// 사용자를 삭제
        public static string DeleteUserAccount(string userid)
        {
            try
            {
                DirectoryEntry entry = GetDirectoryEntryByUserId(userid);
                DirectoryEntry entry_ou = entry.Parent;
                entry_ou.Children.Remove(entry);
                entry_ou.CommitChanges();
                return "SUCCESS";
            }
            catch (DirectoryServicesCOMException e)
            {
                Console.Write(e.ToString());
                return "NONE";
            }
        }
        
        public static string UpdateStatus(string stnt_numb, string status)
        {
            try
            {
                DirectoryEntry entry = GetDirectoryEntryByUserId(stnt_numb);
                entry.Properties["description"].Value = status;
                entry.CommitChanges();
                return "SUCCESS";
            }
            catch (DirectoryServicesCOMException e)
            {
                Console.Write(e.ToString());
                return "NONE";
            }
        }

        /// 사용자 익스체인지 히든
        public static string UpdateUserExchage(string userid)
        {
            try
            {
                DirectoryEntry entry = GetDirectoryEntryByUserId(userid);
                entry.Properties["msExchHideFromAddressLists"].Value = "TRUE";
                entry.CommitChanges();
                return "SUCCESS";
            }
            catch (DirectoryServicesCOMException e)
            {
                Console.Write(e.ToString());
                return "NONE";
            }
        }

        /// GetDirectoryEntryByGroup 
        /// <param name="GroupId"></param>
        public static DirectoryEntry GetDirectoryEntryByGroup(string GroupId)
        {

            DirectoryEntry entry = new DirectoryEntry(Conn.LDAPPath, Conn.LDAPUser, Conn.LDAPPassword, AuthTypes);

            var de = entry;
            var deSearch = new DirectorySearcher(de)
            { SearchRoot = de, Filter = "(&(objectCategory=group)(cn=" + GroupId + "))" };

            var results = deSearch.FindOne();
            return results != null ? results.GetDirectoryEntry() : null;
        }

        /// 사용자 언락
        public static string Unlock(string userid)
        {
            try
            {
                DirectoryEntry entry = GetDirectoryEntryByUserId(userid);
                entry.Properties["LockOutTime"].Value = 0; //unlock account
                entry.CommitChanges(); //may not be needed but adding it anyways
                entry.Close();
                return "SUCCESS";
            }
            catch (DirectoryServicesCOMException e)
            {
                Console.Write(e.ToString());
                return "NONE";
            }
        }

        ///  그룹에 추가
        public static string AddToGroup(string userid, string groupname)
        {
            try
            {
                string userDn = getSingleAttr("distinguishedName", userid);
                DirectoryEntry group_entry = GetDirectoryEntryByGroup(groupname);
                group_entry.Properties["member"].Add(userDn);
                group_entry.CommitChanges();
                return "SUCCESS";
            }
            catch (DirectoryServicesCOMException e)
            {
                Console.Write(e.ToString());
                return "NONE";
            }
        }

        /// 그룹에서 삭제
        public static string RemoveUserFromGroup(string userid, string groupname)
        {
            try
            {
                string userDn = getSingleAttr("distinguishedName", userid);
                DirectoryEntry group_entry = GetDirectoryEntryByGroup(groupname);
                group_entry.Properties["member"].Remove(userDn);
                group_entry.CommitChanges();
                return "SUCCESS";
            }
            catch (DirectoryServicesCOMException e)
            {
                Console.Write(e.ToString());
                return "NONE";
            }
        }

        /// 사용자 정보를 끄집어 오자 몇개만
        public static IEnumerable<User> getLocalUserById(string userid)
        {
            string o_cn = "";
            string o_name = "";
            string o_sAMAccountName = "";
            string o_realName = "";
            string o_description = "";
            string o_userPrincipalName = "";
            string o_userAccountControl = "";
            string o_proxyAddresses = "";
            string o_msExchHideFromAddressLists = "";


            DirectoryEntry de = new DirectoryEntry(Conn.LDAPPath);
            de.Username = Conn.LDAPUser;
            de.Password = Conn.LDAPPassword;

            DirectorySearcher ds = new DirectorySearcher(de);
            ds.SearchScope = SearchScope.Subtree;
            ds.Filter = "(&(objectCategory=user) (sAMAccountName=" + userid + "))";

            ds.PropertiesToLoad.Add("cn");
            ds.PropertiesToLoad.Add("name");
            ds.PropertiesToLoad.Add("sAMAccountName");
            ds.PropertiesToLoad.Add("realName");
            ds.PropertiesToLoad.Add("description");
            ds.PropertiesToLoad.Add("userPrincipalName");
            ds.PropertiesToLoad.Add("userAccountControl");
            ds.PropertiesToLoad.Add("proxyAddresses");
            ds.PropertiesToLoad.Add("msExchHideFromAddressLists");

            try
            {
                SearchResult result = ds.FindOne();

                if (result != null)
                {
                    if (result.Properties["cn"].Count > 0) o_cn = result.Properties["cn"][0].ToString();
                    if (result.Properties["name"].Count > 0) o_name = result.Properties["name"][0].ToString();
                    if (result.Properties["sAMAccountName"].Count > 0) o_sAMAccountName = result.Properties["sAMAccountName"][0].ToString();
                    if (result.Properties["realName"].Count > 0) o_realName = result.Properties["realName"][0].ToString();
                    if (result.Properties["description"].Count > 0) o_description = result.Properties["description"][0].ToString();
                    if (result.Properties["userPrincipalName"].Count > 0) o_userPrincipalName = result.Properties["userPrincipalName"][0].ToString();
                    if (result.Properties["userAccountControl"].Count > 0) o_userAccountControl = result.Properties["userAccountControl"][0].ToString();
                    if (result.Properties["proxyAddresses"].Count > 0) o_proxyAddresses = result.Properties["proxyAddresses"][0].ToString();
                    if (result.Properties["msExchHideFromAddressLists"].Count > 0) o_msExchHideFromAddressLists = result.Properties["msExchHideFromAddressLists"][0].ToString();

                    User[] usr = new User[]
                    { new User {cn = o_cn,
                                    name = o_name,
                                    sAMAccountName = o_sAMAccountName,
                                    realName = o_realName,
                                    description = o_description,
                                    userPrincipalName = o_userPrincipalName,
                                    userAccountControl = o_userAccountControl,
                                    proxyAddresses = o_proxyAddresses,
                                    msExchHideFromAddressLists = o_msExchHideFromAddressLists}};
                    return usr;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }
    }
}
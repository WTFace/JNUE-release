using JNUE_ADAPI.Models;
using System.Web.Mvc;
using System;
using JNUE_ADAPI.AD;
using log4net;
using System.Reflection;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;

namespace JNUE_ADAPI.Controllers
{
    public class HomeController : Controller
    {
        #region Private Fields
        readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        //[HttpGet]
        //public ActionResult Index()
        //{
        //    return View(); }

        public ActionResult Index(StntNumbCheckViewModel model)
        {
            if (ModelState.IsValid)
            {
                string oradb = Conn.connection;
                using (OracleConnection conn = new OracleConnection(oradb))
                {
                    Dictionary<string, string> haksa = new Dictionary<string, string>();
                    try
                    {
                        conn.Open();
                        string sql = "select user_used,role,status,stnt_knam from office365 where stnt_numb= '" + model.Stnt_Numb + "'";
                        OracleCommand cmd = new OracleCommand(sql, conn);
                        cmd.CommandType = System.Data.CommandType.Text;
                        OracleDataReader dr = cmd.ExecuteReader();
                        while (dr.Read())
                        {
                            haksa.Add("user_used", dr.GetString(0));
                            haksa.Add("role", dr.GetString(1));
                            haksa.Add("status", dr.GetString(2));
                            haksa.Add("stnt_knam", dr.GetString(3));
                        }conn.Close();
                        if (haksa.Count==0)
                        {
                            ModelState.AddModelError("", "입력하신 학번이 조회되지 않습니다.\n관리자에게 문의하여 주시기 바랍니다.");
                        }

                        if (haksa["user_used"] == "N") // 비활성화된 계정
                        { ModelState.AddModelError("", "입력하신 학번은 현재 사용중이지 않습니다.\n관리자에게 문의하여 주시기 바랍니다."); }

                        if (LocalAD.ExistAttributeValue("extensionAttribute1", model.Stnt_Numb) == true)
                        {
                            string upn = LocalAD.getSingleAttr("userPrincipalName", model.Stnt_Numb);
                            TempData["upn"] = upn; //login시 id 넘겨줄 용도

                            if (AzureAD.getUser(upn).Result.Equals("False")) //클라우드 동기화 끝났는지
                            {
                                TempData["false"] = "1";
                                return RedirectToAction("Index", "Home");
                            }
                            
                            if (haksa["status"] != LocalAD.getSingleAttr("description", model.Stnt_Numb)) //학적변동확인
                            {
                                AzureAD.setUsageLocation(upn); //위치 할당
                                LocalAD.UpdateStatus(model.Stnt_Numb, haksa["status"]);
                                if (haksa["status"] == "2"){
                                    TempData["status"] = "학적 상태가 '휴학'으로 변경되었습니다.";
                                }else if (haksa["status"] == "1"){
                                    TempData["status"] = "학적 상태가 '재학'으로 변경되었습니다.";
                                }else{
                                    TempData["status"] = "상태가 '졸업/퇴직'으로 변경되었습니다.";}

                                if (LocalAD.getSingleAttr("employeeType", model.Stnt_Numb) == "student")
                                {
                                    if (haksa["status"] == "1")
                                    { //재학
                                        var res = AzureAD.setLicense(upn, Properties.StuLicense, Properties.PlusLicense, Properties.disables);
                                    }
                                    else
                                    { //휴2,졸0
                                        var res = AzureAD.setLicense(upn, Properties.StuLicense, "", "");
                                        AzureAD.removeLicense(upn, Properties.PlusLicense);
                                    }
                                }
                                else if (LocalAD.getSingleAttr("employeeType", model.Stnt_Numb) == "faculty")
                                {
                                    if (haksa["status"] == "0") //퇴직0
                                    { 
                                        var res = AzureAD.setLicense(upn, Properties.FacLicense, "", "");
                                        AzureAD.removeLicense(upn, "\"" + Properties.PlusLicense + "\"");
                                    }
                                    else //재직
                                    {
                                        var res = AzureAD.setLicense(upn, Properties.FacLicense, Properties.PlusLicense, Properties.disables);
                                    }
                                }
                            }
                            return RedirectToAction("Alert", "Home");
                        }
                        else
                        {
                            TempData["numb"] = model.Stnt_Numb;
                            // 없으면 회원가입페이지로
                            return RedirectToAction("RegisterJnueO365", "Home");
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "학번 조회에 실패하였습니다.\n관리자에게 문의하여 주시기 바랍니다.");
                        logger.Debug(ex.ToString());
                    }
                }       
            }
            return View(model);
        }
        
        
        public ActionResult RegisterJnueO365()
        {
            return View();
        }

        public ActionResult Alert()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RegisterJnueO365(RegisterViewModel model)
        {
            var numb = TempData["numb"];
            if (ModelState.IsValid)
            {
                string cua = LocalAD.CreateUserAccount(model.ID, model.Password, numb.ToString());
                
                ///정상적으로 생성이 되더라도 어차피 동기화시간때문에 로그인 못하니,
                ///Index로 보내고 "false"=1일때 뜨는 팝업을 보게한다.
                if (cua != "NONE") 
                {
                    TempData["false"] = "1";  
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "사용자를 추가할 수 없습니다.\n관리자에게 문의하여 주시기 바랍니다.");
            }
            return View(model);
        }
    }
}

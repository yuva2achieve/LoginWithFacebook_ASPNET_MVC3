using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.IO;
using Facebook;
using Newtonsoft.Json.Linq;

namespace LoginWithFacebook.Controllers
{
    public class LoginController : Controller
    {
        # region facebooklogin

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LoginWithFacebook()
        {
            string req = "https://www.facebook.com/dialog/oauth?client_id=" + System.Web.Configuration.WebConfigurationManager.AppSettings["facebookappid"] + "&redirect_uri=http://" + System.Web.Configuration.WebConfigurationManager.AppSettings["DomainName"] + "/login/returnfromfb&scope=user_about_me,user_hometown,user_location,email,offline_access";
            return Redirect(req);
        }

        public ActionResult returnfromfb()
        {
            string code = Request.QueryString["code"];
            if (code == null)
            {
                return RedirectToAction("LastWindow", "Login");
            }

            string AccessToken = "";
            try
            {
                if (code != null)
                {
                    string str = "https://graph.facebook.com/oauth/access_token?client_id=" + System.Web.Configuration.WebConfigurationManager.AppSettings["facebookappid"] + "&redirect_uri=http://" + Request.Url.Authority + "/login/returnfromfb&client_secret=" + System.Web.Configuration.WebConfigurationManager.AppSettings["facebookappsecret"] + "&code=" + code;
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(str);
                    req.Method = "POST";
                    req.ContentType = "application/x-www-form-urlencoded";
                    byte[] Param = Request.BinaryRead(System.Web.HttpContext.Current.Request.ContentLength);
                    string strRequest = System.Text.Encoding.ASCII.GetString(Param);

                    req.ContentLength = strRequest.Length;

                    StreamWriter streamOut = new StreamWriter(req.GetRequestStream(), System.Text.Encoding.ASCII);
                    streamOut.Write(strRequest);
                    streamOut.Close();
                    StreamReader streamIn = new StreamReader(req.GetResponse().GetResponseStream());
                    string strResponse = streamIn.ReadToEnd();
                    if (strResponse.Contains("&expires"))
                        strResponse = strResponse.Substring(0, strResponse.IndexOf("&expires"));
                    AccessToken = strResponse.Replace("access_token=", "");
                    streamIn.Close();
                }

                Facebook.FacebookAPI api = new Facebook.FacebookAPI(AccessToken);
                string request = "/me";

                JSONObject fbobject = api.Get(request);
                try
                {
                    ViewBag.FacebookID = fbobject.Dictionary["id"].String;
                    ViewBag.FirstName = fbobject.Dictionary["first_name"].String;
                    ViewBag.LastName = fbobject.Dictionary["last_name"].String;
                    ViewBag.Email = fbobject.Dictionary["email"].String;

                    //You can get all data using fbobject.Dictionary

                    string str = "http://graph.facebook.com/" + fbobject.Dictionary["id"].String + "?fields=picture&type=normal";
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(str);
                    StreamReader streamIn = new StreamReader(req.GetResponse().GetResponseStream());
                    string strResponse = streamIn.ReadToEnd();
                    JObject je = new JObject();
                    je = JObject.Parse(strResponse);
                    string ProfilePicURL = je["picture"].ToString().Replace("\"", "");

                }
                catch (Exception ex)
                {
                    //errorLog.setError(ex, "LoginController.SaveFacebookData");
                }
            }
            catch (Exception ex)
            {
                //errorLog.setError(ex, "LoginController.returnfromfb");
            }
            return View();
        }

        # endregion facebooklogin

    }
}

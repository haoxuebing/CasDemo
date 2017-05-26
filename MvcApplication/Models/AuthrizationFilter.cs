using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Net.Http;
using System.Threading;
using System.Xml;
using System.Net;
using System.Text;
using HuaYue.Framework.Core.Utility;
using System.Configuration;
using System.Web.Mvc;

namespace MvcApplication.Models
{
    public class AuthrizationFilter : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {

            //如果注册了全局变量，一些界面不想做验证可在这里筛选

            HttpCookie cookie = filterContext.HttpContext.Request.Cookies["UserContext"];
            if (cookie == null)
            {
                if (filterContext.RequestContext.HttpContext.Request["ticket"] == null)
                {
                    string redirectUrl = ConfigurationManager.AppSettings["Account_ServiceBase"].ToString() + "/partner?service=" + filterContext.HttpContext.Request.Url.AbsoluteUri;//Request.Url.AbsoluteUri;
                    filterContext.HttpContext.Response.Write(string.Format("<script>top.window.location.href = '{0}';</script>", redirectUrl));
                    filterContext.HttpContext.Response.End();
                }
                else
                {
                    string redirectUrl = string.Format("{0}/serviceValidate?service={1}&ticket={2}", ConfigurationManager.AppSettings["Account_ServiceBase"].ToString(), filterContext.HttpContext.Request.Url.AbsoluteUri, filterContext.HttpContext.Request["ticket"]);
                    WebClient client = new WebClient();
                    client.Encoding = Encoding.UTF8;
                    string htmlStr = client.DownloadString(redirectUrl).Replace("cas:", "");
                    CasMolde casModel = GetCasModel(htmlStr);
                    HttpCookie newCookie = new HttpCookie("UserContext");
                    newCookie.Value = CryptographicUtility.EncryptString(casModel.loginName);
                    string strUrl = filterContext.HttpContext.Request.Url.Host;
                    string[] domails = strUrl.Split('.');
                    if (domails.Length >= 3)
                    {
                        newCookie.Domain = "." + domails[domails.Length - 3] + "." + domails[domails.Length - 2] + "." + domails[domails.Length - 1]; //+ context.Request.Url.AbsoluteUri;
                    }
                    filterContext.HttpContext.Response.Cookies.Add(newCookie);
                    filterContext.HttpContext.Response.Write(string.Format("<script>top.window.location.href = '{0}';</script>", filterContext.HttpContext.Request.Url.AbsolutePath));
                    return;
                }
            } 
            //base.OnAuthorization(filterContext);
        }
        private CasMolde GetCasModel(string xml)
        {
            CasMolde casmodel = new CasMolde();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            foreach (XmlNode item in doc.DocumentElement.ChildNodes)
            {
                foreach (XmlNode item2 in item.ChildNodes)
                {
                    switch (item2.Name)
                    {
                        case "userId":
                            casmodel.userId = item2.InnerXml;
                            break;
                        case "user":
                            casmodel.user = item2.InnerXml;
                            break;
                        case "loginName":
                            casmodel.loginName = item2.InnerXml;
                            break;
                        case "email":
                            casmodel.email = item2.InnerXml;
                            break;
                        case "domain":
                            casmodel.domain = item2.InnerXml;
                            break;
                        default:
                            break;
                    }
                }
            }
            return casmodel;
        }

    }
    //public class CasMolde
    //{
    //    public string userId { get; set; }
    //    public string user { get; set; }
    //    public string domain { get; set; }
    //    public string email { get; set; }
    //    public string loginName { get; set; }
    //}
}
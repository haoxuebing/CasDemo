using HuaYue.Framework.Core.ContextManager;
using HuaYue.Framework.Core.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;

namespace MvcApplication.Models
{
    public class AuthrizationModule : IHttpModule
    {


        public void Init(HttpApplication context)
        {
            context.PostAcquireRequestState += new EventHandler(context_PostAcquireRequestState);
            context.Error += new EventHandler(context_Error);
        }

        private void context_PostAcquireRequestState(object sender, EventArgs e)
        {
            HttpApplication application = (HttpApplication)sender;
            HttpContext context = application.Context;
            string requestPath = context.Request.Path.ToLower();
            string fileName = System.IO.Path.GetFileName(context.Request.Path);
            string fileExt = System.IO.Path.GetExtension(context.Request.Path);

            if (context.Request.Cookies["UserContext"] != null)//context.Request["UserContext"] != null)
            {
                //从cookie自动登录
                string userName = CryptographicUtility.DecryptString(context.Request.Cookies["UserContext"].Value);
                //第三方网站验证 userName
 
            }
            else
            {
                if (context.Request["ticket"] == null)
                {
                    string redirectUrl = ConfigurationManager.AppSettings["Account_ServiceBase"].ToString() + "/partner?service=" + context.Request.Url.AbsoluteUri;
                    context.Response.Write(string.Format("<script>top.window.location.href = '{0}';</script>", redirectUrl));
                    context.Response.End();
                }
                else
                {
                    string redirectUrl = string.Format("{0}/serviceValidate?service={1}&ticket={2}", ConfigurationManager.AppSettings["Account_ServiceBase"].ToString(), context.Request.Url.AbsoluteUri, context.Request["ticket"]);
                    WebClient client = new WebClient();
                    client.Encoding = Encoding.UTF8;
                    string htmlStr = client.DownloadString(redirectUrl).Replace("cas:", "");
                    CasMolde casModel = GetCasModel(htmlStr);
                    HttpCookie cookie = new HttpCookie("UserContext");
                    cookie.Value = CryptographicUtility.EncryptString(casModel.loginName);
                    string strUrl = context.Request.Url.Host;
                    string[] domails = strUrl.Split('.');
                    if (domails.Length >= 3)
                    {
                        cookie.Domain = "." + domails[domails.Length - 3] + "." + domails[domails.Length - 2] + "." + domails[domails.Length - 1]; //+ context.Request.Url.AbsoluteUri;
                    }
                    context.Response.Cookies.Add(cookie);
                    context.Response.Write(string.Format("<script>top.window.location.href = '{0}';</script>", context.Request.Url.AbsolutePath));
                    return;
                }
            }

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
        private void context_Error(object sender, EventArgs e)
        {
            // 统一错误处理
            // 可以调用NLog记录日志
            //throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class CasMolde
    {
        public string userId { get; set; }
        public string user { get; set; }
        public string domain { get; set; }
        public string email { get; set; }
        public string loginName { get; set; }
    }
}
using MvcApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcApplication.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        public ActionResult Index()
        {

            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Quit()
        {
            //退出操作，清楚共享的cookie
            HttpCookie UserContextCookie = Request.Cookies["UserContext"];
            string strUrl = Request.Url.Host;
            string[] domails = strUrl.Split('.');
            UserContextCookie.Domain = "." + domails[domails.Length - 3] + "." + domails[domails.Length - 2] + "." + domails[domails.Length - 1];
            if (UserContextCookie != null)
            {
                UserContextCookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(UserContextCookie);
            }
            return RedirectToAction("Index");
        }
    }
}

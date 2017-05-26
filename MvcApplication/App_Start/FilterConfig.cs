using MvcApplication.Models;
using System.Web;
using System.Web.Mvc;

namespace MvcApplication
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            //filters.Add(new AuthrizationFilter()); //过滤器全局配置
        }
    }
}
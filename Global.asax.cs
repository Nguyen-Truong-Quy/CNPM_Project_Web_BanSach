using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using CNPM_Project_web.Model;

namespace CNPM_Project_web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            var context = HttpContext.Current;
            if (context == null || context.Session == null) return;

            // Nếu session chưa có CustomerId nhưng cookie có
            if (context.Session["CustomerId"] == null && context.Request.Cookies["UserInfo"] != null)
            {
                var cookie = context.Request.Cookies["UserInfo"];
                var userId = cookie["UserId"];
                if (!string.IsNullOrEmpty(userId))
                {
                    using (var db = new Web_Entities())
                    {
                        var kh = db.Khach_Hang.FirstOrDefault(k => k.ID_User == userId);
                        if (kh != null)
                            context.Session["CustomerId"] = kh.MA_KH;
                    }
                }
            }
        }

    }
}

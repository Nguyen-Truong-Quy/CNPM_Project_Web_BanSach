using System.Web.Mvc;

namespace CNPM_Project_web.Areas.QuanLyCuaHang
{
    public class QuanLyCuaHangAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "QuanLyCuaHang";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "QuanLyCuaHang_default",
                "QuanLyCuaHang/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}

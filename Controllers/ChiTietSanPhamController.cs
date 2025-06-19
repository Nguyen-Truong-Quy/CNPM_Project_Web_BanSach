using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CNPM_Project_web.Controllers
{
    public class ChiTietSanPhamController : Controller
    {
        // GET: ChiTietSanPham
        public ActionResult ChiTietSanPham()
        {
            return View();
        }
        public ActionResult danhsachSP()
        {
            return View();
        }
    }
}
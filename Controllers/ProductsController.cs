using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CNPM_Project_web.Model;

namespace CNPM_Project_web.Controllers
{
    public class ProductsController : Controller
    {
        private Web_Entities db = new Web_Entities();
        // GET: Products
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ProductList()
        {
            // Lấy sản phẩm có trạng thái là "Đăng"
            var sanPhamDang = db.San_Pham
                                .Where(sp => sp.Trang_Thai.TEN_TRANG_THAI == "Đăng")
                                .ToList();

            return View(sanPhamDang);
        }
        public ActionResult ProductDetails(string id)
        {
            var sp = db.San_Pham.FirstOrDefault(x => x.MA_SP == id);
            if (sp == null)
                return HttpNotFound();

            return View(sp);
        }

    }
}
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;      // namespace của ToPagedList()
using PagedList.Mvc;
using CNPM_Project_web.Model;

namespace CNPM_Project_web.Controllers
{
    public class HomeController : Controller
    {
        private Web_Entities db = new Web_Entities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult TrangChu(string search, int? page)
        {
            // Lấy queryable để có thể lọc và phân trang trên database
            var items = db.San_Pham.AsQueryable();

            // Nếu có tìm kiếm thì filter theo tên
            if (!string.IsNullOrEmpty(search))
            {
                ViewBag.CurrentFilter = search;
                items = items.Where(s => s.TEN_SP.Contains(search));
            }
            else
            {
                ViewBag.CurrentFilter = "";
            }

            // Sắp xếp (tuỳ bạn)
            items = items.OrderBy(s => s.MA_SP);

            // Thiết lập phân trang
            int pageSize = 12;                 // số mục mỗi trang
            int pageNumber = (page ?? 1);      // trang hiện tại, mặc định 1

            // Chuyển IQueryable --> IPagedList
            IPagedList<CNPM_Project_web.Model.San_Pham> pagedList =
                items.ToPagedList(pageNumber, pageSize);

            return View(pagedList);
        }
    }
}
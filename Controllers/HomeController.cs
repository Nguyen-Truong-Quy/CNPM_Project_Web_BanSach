using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
        public ActionResult TrangChu(string search, int? page, int? danhMuc, int? nhaXuatBan, int? tacGia, int? theLoai)
        {
            var items = db.San_Pham
                          .Where(sp => sp.Trang_Thai.TEN_TRANG_THAI == "Bình thường"); // Không dùng ToList()

            // Lọc theo các filter nếu có
            if (danhMuc.HasValue)
            {
                items = items.Where(s => s.ID_DANH_MUC == danhMuc.Value);
                var danhMucTen = db.Danh_Muc.FirstOrDefault(d => d.ID_DANH_MUC == danhMuc.Value)?.TEN_DANH_MUC;
                ViewBag.TenDanhMuc = danhMucTen;
            }
            if (nhaXuatBan.HasValue)
            {
                items = items.Where(s => s.ID_NXB == nhaXuatBan.Value);
                var tenNXB = db.Nha_Xuat_Ban
                               .FirstOrDefault(n => n.ID_NXB == nhaXuatBan.Value)?.TEN_NXB ?? "";
                ViewBag.TenNXB = HttpUtility.HtmlEncode(tenNXB);
            }
            if (tacGia.HasValue)
            {
                items = items.Where(s => s.ID_TAC_GIA == tacGia.Value);
                var tenTG = db.Tac_Gia
                              .FirstOrDefault(t => t.ID_TAC_GIA == tacGia.Value)?.TEN_TAC_GIA ?? "";
                ViewBag.TenTacGia = HttpUtility.HtmlEncode(tenTG);
            }
            if (theLoai.HasValue)
            {
                items = items.Where(s => s.ID_The_Loai == theLoai.Value);
                var tenTL = db.The_Loai
                              .FirstOrDefault(t => t.ID_The_Loai == theLoai.Value)?.Ten_The_Loai ?? "";
                ViewBag.TenTheLoai = HttpUtility.HtmlEncode(tenTL);
            }



            // Tìm kiếm theo tên
            if (!string.IsNullOrEmpty(search))
            {
                ViewBag.CurrentFilter = search;
                items = items.Where(s => s.TEN_SP.Contains(search));
            }
            else
            {
                ViewBag.CurrentFilter = "";
            }

            // Sắp xếp theo mã sản phẩm
            items = items.OrderBy(s => s.MA_SP);

            // Thiết lập phân trang
            int pageSize = 12;
            int pageNumber = (page ?? 1);
            IPagedList<CNPM_Project_web.Model.San_Pham> pagedList = items.ToPagedList(pageNumber, pageSize);

            // 🔥 Sản phẩm bán chạy (cũng lọc trạng thái là "Đăng")
            var sanPhamBanChay = db.San_Pham
                .Where(sp => sp.Trang_Thai.TEN_TRANG_THAI == "Bình thường" && sp.Chi_Tiet_Don_Hang.Any())
                .OrderByDescending(sp => sp.Chi_Tiet_Don_Hang.Sum(ct => (int?)ct.SO_LUONG) ?? 0)
                .Take(8)
                .ToList();

            // 🆕 Sản phẩm mới (dựa vào MA_SP, cũng lọc "Đăng")
            var sanPhamMoi = db.San_Pham
                .Where(sp => sp.Trang_Thai.TEN_TRANG_THAI == "Bình thường")
                .ToList()
                .OrderByDescending(sp =>
                {
                    string so = new string(sp.MA_SP.Where(char.IsDigit).ToArray());
                    return int.TryParse(so, out int result) ? result : 0;
                })
                .Take(8)
                .ToList();

            ViewBag.SanPhamBanChay = sanPhamBanChay;
            ViewBag.SanPhamMoi = sanPhamMoi;

            ViewBag.DanhMucList = db.Danh_Muc.ToList();
            ViewBag.NhaXuatBanList = db.Nha_Xuat_Ban.ToList();
            ViewBag.TacGiaList = db.Tac_Gia.ToList();
            ViewBag.TheLoaiList = db.The_Loai.ToList();

            return View(pagedList);
        }



    }
}
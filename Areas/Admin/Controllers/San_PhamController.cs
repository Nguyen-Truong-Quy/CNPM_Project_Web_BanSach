using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CNPM_Project_web.Model;
using PagedList;

namespace CNPM_Project_web.Areas.Admin.Controllers
{
    public class San_PhamController : Controller
    {
        private Web_Entities db = new Web_Entities();

        // GET: Admin/San_Pham
        public ActionResult Index(string searchString, string sortOrder, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentFilter = searchString;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.PriceSortParm = sortOrder == "Price" ? "price_desc" : "Price";
            ViewBag.CategorySortParm = sortOrder == "Category" ? "category_desc" : "Category";

            var products = db.San_Pham
                .Include(s => s.Danh_Muc)
                .Include(s => s.Nha_Xuat_Ban)
                .Include(s => s.Tac_Gia)
                .Include(s => s.The_Loai)
                .Include(s => s.Trang_Thai)
                .AsQueryable();

            // Tìm kiếm
            if (!String.IsNullOrEmpty(searchString))
            {
                products = products.Where(s => 
                    s.TEN_SP.Contains(searchString) ||
                    s.MA_SP.Contains(searchString) ||
                    s.Danh_Muc.TEN_DANH_MUC.Contains(searchString) ||
                    s.The_Loai.Ten_The_Loai.Contains(searchString) ||
                    s.Tac_Gia.TEN_TAC_GIA.Contains(searchString) ||
                    s.Nha_Xuat_Ban.TEN_NXB.Contains(searchString)
                );
            }

            // Sắp xếp
            switch (sortOrder)
            {
                case "name_desc":
                    products = products.OrderByDescending(s => s.TEN_SP);
                    break;
                case "Price":
                    products = products.OrderBy(s => s.GIA_BAN);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(s => s.GIA_BAN);
                    break;
                case "Category":
                    products = products.OrderBy(s => s.Danh_Muc.TEN_DANH_MUC);
                    break;
                case "category_desc":
                    products = products.OrderByDescending(s => s.Danh_Muc.TEN_DANH_MUC);
                    break;
                default:
                    products = products.OrderBy(s => s.TEN_SP);
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(products.ToPagedList(pageNumber, pageSize));
        }

        // GET: Admin/San_Pham/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            San_Pham san_Pham = db.San_Pham
                .Include(s => s.Danh_Muc)
                .Include(s => s.Nha_Xuat_Ban)
                .Include(s => s.Tac_Gia)
                .Include(s => s.The_Loai)
                .Include(s => s.Trang_Thai)
                .FirstOrDefault(s => s.MA_SP == id);
            if (san_Pham == null)
            {
                return HttpNotFound();
            }
            return View(san_Pham);
        }

        // GET: Admin/San_Pham/Create
        public ActionResult Create()
        {
            // Tạo mã SP tiếp theo
            string newMaSP = "SP001";
            var lastSP = db.San_Pham
                .Where(s => s.MA_SP.StartsWith("SP"))
                .OrderByDescending(s => s.MA_SP)
                .FirstOrDefault();

            if (lastSP != null)
            {
                string lastCode = lastSP.MA_SP.Substring(2);
                if (int.TryParse(lastCode, out int lastNumber))
                {
                    newMaSP = "SP" + (lastNumber + 1).ToString("D3");
                }
            }

            // Gán sẵn vào model
            var sp = new San_Pham
            {
                MA_SP = newMaSP
            };

            // ViewBag setup
            ViewBag.ID_DANH_MUC = new SelectList(db.Danh_Muc, "ID_DANH_MUC", "TEN_DANH_MUC");
            ViewBag.ID_NXB = new SelectList(db.Nha_Xuat_Ban, "ID_NXB", "TEN_NXB");
            ViewBag.ID_TAC_GIA = new SelectList(db.Tac_Gia, "ID_TAC_GIA", "TEN_TAC_GIA");
            ViewBag.ID_The_Loai = new SelectList(db.The_Loai, "ID_The_Loai", "Ten_The_Loai");
            ViewBag.ID_TRANG_THAI = db.Trang_Thai
            .Where(t => t.ID_TRANG_THAI == 2 || t.ID_TRANG_THAI == 3)
            .Select(t => new SelectListItem
             {
                Value = t.ID_TRANG_THAI.ToString(),
                Text = t.TEN_TRANG_THAI
            })
            .ToList();

            return View(sp);
        }

        // POST: Admin/San_Pham/Create
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MA_SP,TEN_SP,MO_TA,GIA_GOC,DISCOUNT,GIA_BAN,TON_KHO,ID_TRANG_THAI,ID_DANH_MUC,ID_TAC_GIA,ID_NXB,ID_The_Loai")]
        San_Pham san_Pham,
        HttpPostedFileBase ImageFile,
        string ImageUrl)
        {
            if (ModelState.IsValid)
            {
                // 🔸 Tạo mã sản phẩm tự động (SP001, SP002,...)
                string newMaSP = "SP001";
                var lastSP = db.San_Pham
                    .Where(s => s.MA_SP.StartsWith("SP"))
                    .OrderByDescending(s => s.MA_SP)
                    .FirstOrDefault();

                if (lastSP != null)
                {
                    string lastCode = lastSP.MA_SP.Substring(2);
                    if (int.TryParse(lastCode, out int lastNumber))
                    {
                        newMaSP = "SP" + (lastNumber + 1).ToString("D3");
                    }
                }
                san_Pham.MA_SP = newMaSP;

                // 🔸 Xử lý ảnh
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(ImageFile.FileName);
                    var path = Path.Combine(Server.MapPath("~/Images/SanPham"), fileName);
                    Directory.CreateDirectory(Server.MapPath("~/Images/SanPham"));
                    ImageFile.SaveAs(path);
                    san_Pham.HINH_ANH = "/Images/SanPham/" + fileName;
                }
                else if (!string.IsNullOrEmpty(ImageUrl))
                {
                    san_Pham.HINH_ANH = ImageUrl;
                }

                try
                {
                    db.San_Pham.Add(san_Pham);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Sản phẩm đã được tạo thành công!";
                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var eve in ex.EntityValidationErrors)
                    {
                        foreach (var ve in eve.ValidationErrors)
                        {
                            ModelState.AddModelError(ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    TempData["ErrorMessage"] = "Lỗi khi lưu dữ liệu. Vui lòng kiểm tra lại.";
                }
            }
            else
            {
                 TempData["ErrorMessage"] = "Dữ liệu nhập không hợp lệ. Vui lòng kiểm tra các trường bắt buộc.";
            }

            // Reload lại dropdown nếu model có lỗi
            ViewBag.ID_DANH_MUC = new SelectList(db.Danh_Muc, "ID_DANH_MUC", "TEN_DANH_MUC", san_Pham.ID_DANH_MUC);
            ViewBag.ID_NXB = new SelectList(db.Nha_Xuat_Ban, "ID_NXB", "TEN_NXB", san_Pham.ID_NXB);
            ViewBag.ID_TAC_GIA = new SelectList(db.Tac_Gia, "ID_TAC_GIA", "TEN_TAC_GIA", san_Pham.ID_TAC_GIA);
            ViewBag.ID_The_Loai = new SelectList(db.The_Loai, "ID_The_Loai", "Ten_The_Loai", san_Pham.ID_The_Loai);
            ViewBag.ID_TRANG_THAI = db.Trang_Thai
    .       Where(t => t.ID_TRANG_THAI == 2 || t.ID_TRANG_THAI == 3)
            .Select(t => new SelectListItem
            {
                Value = t.ID_TRANG_THAI.ToString(),
                Text = t.TEN_TRANG_THAI
            })
            .ToList();
            return View(san_Pham);
        }

        // GET: Admin/San_Pham/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            San_Pham san_Pham = db.San_Pham.Find(id);
            if (san_Pham == null)
            {
                return HttpNotFound();
            }

            ViewBag.ID_DANH_MUC = new SelectList(db.Danh_Muc, "ID_DANH_MUC", "TEN_DANH_MUC", san_Pham.ID_DANH_MUC);
            ViewBag.ID_NXB = new SelectList(db.Nha_Xuat_Ban, "ID_NXB", "TEN_NXB", san_Pham.ID_NXB);
            ViewBag.ID_TAC_GIA = new SelectList(db.Tac_Gia, "ID_TAC_GIA", "TEN_TAC_GIA", san_Pham.ID_TAC_GIA);
            ViewBag.ID_The_Loai = new SelectList(db.The_Loai, "ID_The_Loai", "Ten_The_Loai", san_Pham.ID_The_Loai);
            ViewBag.ID_TRANG_THAI = db.Trang_Thai
                .Where(t => t.ID_TRANG_THAI == 2 || t.ID_TRANG_THAI == 3)
                .Select(t => new SelectListItem
                {
                    Value = t.ID_TRANG_THAI.ToString(),
                    Text = t.TEN_TRANG_THAI
                })
                .ToList();
            // Thêm logic kiểm tra ảnh
            if (!string.IsNullOrEmpty(san_Pham.HINH_ANH))
            {
                ViewBag.IsImageUrl = san_Pham.HINH_ANH.StartsWith("http");
            }

            return View(san_Pham);
        }

        // POST: Admin/San_Pham/Edit/5
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "MA_SP,TEN_SP,MO_TA,GIA_GOC,DISCOUNT,GIA_BAN,TON_KHO,ID_TRANG_THAI,ID_DANH_MUC,ID_TAC_GIA,ID_NXB,ID_The_Loai")]
            San_Pham san_Pham,
            HttpPostedFileBase ImageFile,
            string ImageUrl)
        {
            if (ModelState.IsValid)
            {
                // Xử lý ảnh
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(ImageFile.FileName);
                    var path = Path.Combine(Server.MapPath("~/Images/SanPham"), fileName);
                    Directory.CreateDirectory(Server.MapPath("~/Images/SanPham"));
                    ImageFile.SaveAs(path);
                    san_Pham.HINH_ANH = "/Images/SanPham/" + fileName;
                }
                else if (!string.IsNullOrEmpty(ImageUrl))
                {
                    san_Pham.HINH_ANH = ImageUrl;
                }

                db.Entry(san_Pham).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Sản phẩm đã được cập nhật thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.ID_DANH_MUC = new SelectList(db.Danh_Muc, "ID_DANH_MUC", "TEN_DANH_MUC", san_Pham.ID_DANH_MUC);
            ViewBag.ID_NXB = new SelectList(db.Nha_Xuat_Ban, "ID_NXB", "TEN_NXB", san_Pham.ID_NXB);
            ViewBag.ID_TAC_GIA = new SelectList(db.Tac_Gia, "ID_TAC_GIA", "TEN_TAC_GIA", san_Pham.ID_TAC_GIA);
            ViewBag.ID_The_Loai = new SelectList(db.The_Loai, "ID_The_Loai", "Ten_The_Loai", san_Pham.ID_The_Loai);
            ViewBag.ID_TRANG_THAI = db.Trang_Thai
                .Where(t => t.ID_TRANG_THAI == 2 || t.ID_TRANG_THAI == 3)
                .Select(t => new SelectListItem
                {
                    Value = t.ID_TRANG_THAI.ToString(),
                    Text = t.TEN_TRANG_THAI
                })
                .ToList();

            return View(san_Pham);
        }

        // GET: Admin/San_Pham/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            San_Pham san_Pham = db.San_Pham
                .Include(s => s.Danh_Muc)
                .Include(s => s.Nha_Xuat_Ban)
                .Include(s => s.Tac_Gia)
                .Include(s => s.The_Loai)
                .Include(s => s.Trang_Thai)
                .FirstOrDefault(s => s.MA_SP == id);
            if (san_Pham == null)
            {
                return HttpNotFound();
            }
            return View(san_Pham);
        }

        // POST: Admin/San_Pham/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            San_Pham san_Pham = db.San_Pham.Find(id);
            if (san_Pham == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra ràng buộc trước khi xóa
            var hasOrders = db.Chi_Tiet_Don_Hang.Any(ct => ct.MA_SP == id);
            if (hasOrders)
            {
                TempData["ErrorMessage"] = "Không thể xóa sản phẩm này vì đã có đơn hàng liên quan!";
                return RedirectToAction("Index");
            }

            try
            {
                db.San_Pham.Remove(san_Pham);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Sản phẩm đã được xóa thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa sản phẩm: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // AJAX: Kiểm tra tên sản phẩm trùng lặp
        [HttpPost]
        public JsonResult CheckProductName(string tenSP, string maSP = null)
        {
            var exists = db.San_Pham.Any(p => 
                p.TEN_SP == tenSP && 
                (maSP == null || p.MA_SP != maSP));
            
            return Json(new { exists = exists });
        }

        // AJAX: Lấy thông tin sản phẩm
        [HttpGet]
        public JsonResult GetProductInfo(string id)
        {
            var product = db.San_Pham
                .Include(p => p.Danh_Muc)
                .Include(p => p.Nha_Xuat_Ban)
                .Include(p => p.Tac_Gia)
                .Include(p => p.The_Loai)
                .Include(p => p.Trang_Thai)
                .FirstOrDefault(p => p.MA_SP == id);

            if (product != null)
            {
                return Json(new
                {
                    maSP = product.MA_SP,
                    tenSP = product.TEN_SP,
                    moTa = product.MO_TA,
                    giaGoc = product.GIA_GOC,
                    discount = product.DISCOUNT,
                    giaBan = product.GIA_BAN,
                    tonKho = product.TON_KHO,
                    hinhAnh = product.HINH_ANH,
                    danhMuc = product.Danh_Muc?.TEN_DANH_MUC,
                    tacGia = product.Tac_Gia?.TEN_TAC_GIA,
                    nhaXuatBan = product.Nha_Xuat_Ban?.TEN_NXB,
                    theLoai = product.The_Loai?.Ten_The_Loai,
                    trangThai = product.Trang_Thai?.TEN_TRANG_THAI
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

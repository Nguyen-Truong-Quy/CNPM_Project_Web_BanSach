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

namespace CNPM_Project_web.Areas.Admin.Controllers
{
    public class San_PhamController : Controller
    {
        private Web_Entities db = new Web_Entities();

        // GET: Admin/San_Pham
        public ActionResult Index()
        {
            var san_Pham = db.San_Pham.Include(s => s.Danh_Muc).Include(s => s.Nha_Xuat_Ban).Include(s => s.Tac_Gia).Include(s => s.The_Loai).Include(s => s.Trang_Thai);
            return View(san_Pham.ToList());
        }

        // GET: Admin/San_Pham/Details/5
        public ActionResult Details(string id)
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

            return View(sp); // ✅ BẮT BUỘC truyền model vào đây
        }


        // POST: Admin/San_Pham/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
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
            San_Pham san_Pham = db.San_Pham.Find(id);
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
            db.San_Pham.Remove(san_Pham);
            db.SaveChanges();
            return RedirectToAction("Index");
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

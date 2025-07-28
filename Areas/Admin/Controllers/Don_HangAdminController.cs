using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CNPM_Project_web.Model;

namespace CNPM_Project_web.Areas.Admin.Controllers
{
    public class Don_HangController : Controller
    {
        private Web_Entities db = new Web_Entities();


        // GET: Admin/Don_Hang
        public ActionResult Index(string status)
        {
            // Nếu status là null hoặc rỗng, coi như là "Tất cả" ngay từ đầu.
            if (string.IsNullOrEmpty(status))
            {
                status = "Tất cả";
            }

            ViewBag.CurrentStatus = status; // Gửi trạng thái hiện tại về View để làm active tab

            IQueryable<Don_Hang> don_hang_query = db.Don_Hang;

            // Chỉ lọc khi status có giá trị và KHÁC "Tất cả"
            if (status != "Tất cả")
            {
                don_hang_query = don_hang_query.Where(d => d.Trang_Thai.TEN_TRANG_THAI == status);
            }

            // Luôn Include và OrderBy cho kết quả cuối cùng
            var final_list = don_hang_query
                .Include(d => d.Trang_Thai)
                .Include(d => d.Khach_Hang)
                .OrderByDescending(d => d.TG_DAT_HANG)
                .ToList();

            return View(final_list);
        }

        public ActionResult _DonHangDetailsPartial(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // Dùng Include để lấy tất cả dữ liệu liên quan trong một lần truy vấn
            Don_Hang don_Hang = db.Don_Hang
                                  .Include(d => d.Chi_Tiet_Don_Hang.Select(ct => ct.San_Pham)) // Lấy chi tiết và sản phẩm
                                  .Include(d => d.Khach_Hang) // Lấy thông tin khách hàng
                                  .Include(d => d.Trang_Thai) // Lấy thông tin trạng thái
                                  .FirstOrDefault(d => d.ID_DON_HANG == id);
            if (don_Hang == null)
            {
                return HttpNotFound();
            }
            return PartialView("_DonHangDetailsPartial", don_Hang);
        }

        // ACTION MỚI: Xử lý việc cập nhật trạng thái tiếp theo của đơn hàng
        [HttpPost]
        public ActionResult UpdateOrderStatus(int id)
        {
            Don_Hang donHang = db.Don_Hang.Find(id);
            if (donHang == null)
            {
                return HttpNotFound();
            }

            // Logic xác định trạng thái tiếp theo
            // Giả sử ID trạng thái trong DB của bạn theo thứ tự:
            // 2: Chờ xác nhận TT -> 3: Chờ giao hàng -> 4: Đang giao -> 5: Hoàn thành
            int currentStatusId = donHang.ID_TRANG_THAI;
            int nextStatusId = currentStatusId;

            switch (currentStatusId)
            {
                case 2: // Chờ xác nhận thanh toán
                    nextStatusId = 3; // -> Chờ giao hàng
                    break;
                case 3: // Chờ giao hàng
                    nextStatusId = 4; // -> Đang giao hàng
                    break;
                case 4: // Đang giao hàng
                    nextStatusId = 5; // -> Hoàn thành
                    break;
                // Các trạng thái khác (Mới, Hoàn thành, Đã hủy) không có hành động tiếp theo
                default:
                    return Json(new { success = false, message = "Đơn hàng ở trạng thái không thể cập nhật." });
            }

            donHang.ID_TRANG_THAI = nextStatusId;
            db.Entry(donHang).State = EntityState.Modified;

            // (Tùy chọn) Ghi lại lịch sử thay đổi trạng thái
            db.Lich_Su_Don_Hang.Add(new Lich_Su_Don_Hang
            {
                ID_DON_HANG = donHang.ID_DON_HANG,
                ID_TRANG_THAI = nextStatusId,
                NGAY_CAP_NHAT = DateTime.Now,
                GHI_CHU = "Admin cập nhật trạng thái."
            });

            db.SaveChanges();
            return Json(new { success = true, message = "Cập nhật trạng thái thành công!" });
        }



        // GET: Admin/Don_Hang/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Don_Hang don_Hang = db.Don_Hang.Find(id);
            if (don_Hang == null)
            {
                return HttpNotFound();
            }
            return View(don_Hang);
        }

        // GET: Admin/Don_Hang/Create
        public ActionResult Create()
        {
            ViewBag.ID_TRANG_THAI = new SelectList(db.Trang_Thai, "ID_TRANG_THAI", "TEN_TRANG_THAI");
            ViewBag.MA_KH = new SelectList(db.Khach_Hang, "MA_KH", "HO_TEN_KH");
            return View();
        }

        // POST: Admin/Don_Hang/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_DON_HANG,MA_KH,TG_DAT_HANG,ID_TRANG_THAI,TONG_TIEN")] Don_Hang don_Hang)
        {
            if (ModelState.IsValid)
            {
                db.Don_Hang.Add(don_Hang);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ID_TRANG_THAI = new SelectList(db.Trang_Thai, "ID_TRANG_THAI", "TEN_TRANG_THAI", don_Hang.ID_TRANG_THAI);
            ViewBag.MA_KH = new SelectList(db.Khach_Hang, "MA_KH", "HO_TEN_KH", don_Hang.MA_KH);
            return View(don_Hang);
        }

        // GET: Admin/Don_Hang/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Don_Hang don_Hang = db.Don_Hang.Find(id);
            if (don_Hang == null)
            {
                return HttpNotFound();
            }
            ViewBag.ID_TRANG_THAI = new SelectList(db.Trang_Thai, "ID_TRANG_THAI", "TEN_TRANG_THAI", don_Hang.ID_TRANG_THAI);
            ViewBag.MA_KH = new SelectList(db.Khach_Hang, "MA_KH", "HO_TEN_KH", don_Hang.MA_KH);
            return View(don_Hang);
        }

        // POST: Admin/Don_Hang/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_DON_HANG,MA_KH,TG_DAT_HANG,ID_TRANG_THAI,TONG_TIEN")] Don_Hang don_Hang)
        {
            if (ModelState.IsValid)
            {
                db.Entry(don_Hang).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ID_TRANG_THAI = new SelectList(db.Trang_Thai, "ID_TRANG_THAI", "TEN_TRANG_THAI", don_Hang.ID_TRANG_THAI);
            ViewBag.MA_KH = new SelectList(db.Khach_Hang, "MA_KH", "HO_TEN_KH", don_Hang.MA_KH);
            return View(don_Hang);
        }

        // GET: Admin/Don_Hang/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Don_Hang don_Hang = db.Don_Hang.Find(id);
            if (don_Hang == null)
            {
                return HttpNotFound();
            }
            return View(don_Hang);
        }

        // POST: Admin/Don_Hang/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Don_Hang don_Hang = db.Don_Hang.Find(id);
            db.Don_Hang.Remove(don_Hang);
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

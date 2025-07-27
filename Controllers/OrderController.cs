using System;
using System.Linq;
using System.Web.Mvc;
using CNPM_Project_web.Model;

namespace CNPM_Project_web.Controllers
{
    public class OrderController : Controller
    {
        private Web_Entities db = new Web_Entities();

        [HttpPost]
        public JsonResult PlaceOrder(string maSP, int soLuong)
        {
            try
            {
                // 1. Lấy khách hàng hiện tại (giả sử lưu trong session)
                var userId = Session["UserID"] as string;
                var kh = db.Khach_Hang.SingleOrDefault(k => k.ID_User == userId);
                if (kh == null) throw new Exception("Chưa đăng nhập");

                // 2. Lấy thông tin sản phẩm
                var sp = db.San_Pham.Find(maSP);
                if (sp == null) throw new Exception("Sản phẩm không tồn tại");

                // 3. Tạo Don_Hang
                var dh = new Don_Hang
                {
                    MA_KH = kh.MA_KH,
                    TG_DAT_HANG = DateTime.Now,
                    ID_TRANG_THAI = 1,           // trạng thái “Mới”
                   
                };
                db.Don_Hang.Add(dh);
                db.SaveChanges();  // để lấy dh.ID_DON_HANG

                // 4. Tạo Chi_Tiet_Don_Hang
                var ct = new Chi_Tiet_Don_Hang
                {
                    ID_DON_HANG = dh.ID_DON_HANG,
                    MA_SP = maSP,
                    SO_LUONG = soLuong,
                    
                };
                db.Chi_Tiet_Don_Hang.Add(ct);
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    orderId = dh.ID_DON_HANG
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}

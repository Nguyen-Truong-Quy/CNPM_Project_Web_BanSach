using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CNPM_Project_web.Model;

namespace CNPM_Project_web.Controllers
{
    public class UsersController : Controller
    {
        private Web_Entities db = new Web_Entities();

        // GET: Users/Register
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra username đã tồn tại chưa
                var existingUser = db.USERS.Find(model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Tên đăng nhập đã tồn tại.");
                    return View(model);
                }

                // Tạo mã khách hàng mới
                string maKH = GenerateCustomerCode();

                // Tạo bản ghi Khach_Hang
                var khachHang = new Khach_Hang
                {
                    MA_KH = maKH,
                    HO_TEN_KH = model.Username,
                    EMAIL = model.Email
                };
                db.Khach_Hang.Add(khachHang);

                // Tạo bản ghi USER
                var newUser = new USER
                {
                    USERNAME = model.Username,
                    PASSWORD = model.Password,
                    EMAIL = model.Email,
                    ID_ROLE = 2, // Khách hàng
                    MA_KH = maKH
                };
                db.USERS.Add(newUser);

                // Lưu cả hai vào database
                db.SaveChanges();

                ViewBag.SuccessMessage = "Đăng ký thành công!";
                return View();
            }

            return View(model);
        }


        private string GenerateCustomerCode()
        {
            int count = db.USERS.Count(u => u.ID_ROLE == 2) + 1;
            return "KH" + count.ToString("D4"); // VD: KH0001
        }
    }
}

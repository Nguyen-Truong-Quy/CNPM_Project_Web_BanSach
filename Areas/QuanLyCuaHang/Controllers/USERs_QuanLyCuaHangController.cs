using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CNPM_Project_web.Model;
using CNPM_Project_web.ViewModel;

namespace CNPM_Project_web.Areas.Quản_lý_cửa_hàng.Controllers
{
    public class USERs_QuanLyCuaHangController : Controller
    {
        private Web_Entities db = new Web_Entities();

        // GET: Quản_lý_cửa_hàng/USERs_QuanLyCuaHang
        public ActionResult Index()
        {
            var users = from u in db.USERS
                        join k in db.Khach_Hang on u.ID_User equals k.ID_User
                        where u.ID_ROLE == 1
                        select new UserKhachHangViewModel1
                        {
                            ID_User = u.ID_User,
                            EMAIL = u.EMAIL,
                            PASSWORD = u.PASSWORD,
                            ID_ROLE = u.ID_ROLE,
                            TEN_ROLE = u.Role.TEN_ROLE,
                            MA_KH = k.MA_KH,
                            HO_TEN_KH = k.HO_TEN_KH,
                            SDT_KH = k.SDT_KH,
                            DIA_CHI = k.DIA_CHI,
                            ANH_DAI_DIEN = k.ANH_DAI_DIEN
                        };
            return View(users.ToList());
        }

        // GET: Quản_lý_cửa_hàng/USERs_QuanLyCuaHang/Create
        [HttpGet]
        public ActionResult Create()
        {
            return View(new UserCreateViewModel());
        }

        // POST: .../Create
        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult Create(UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 1) Sinh ID_User mới một cách “số học”
            var allIds = db.USERS
                           .Where(u => u.ID_User.StartsWith("USER_"))
                           .Select(u => u.ID_User.Substring(5))
                           .ToList();

            int maxNum = 0;
            foreach (var s in allIds)
                if (int.TryParse(s, out var n) && n > maxNum)
                    maxNum = n;

            int nextNum = maxNum + 1;
            string newIdUser = $"USER_{nextNum:D3}";

            // 2) Sinh MA_KH tương tự
            var allMa = db.Khach_Hang
                          .Where(k => k.MA_KH.StartsWith("KH"))
                          .Select(k => k.MA_KH.Substring(2))
                          .ToList();

            int maxKH = 0;
            foreach (var s in allMa)
                if (int.TryParse(s, out var n) && n > maxKH)
                    maxKH = n;

            int nextKH = maxKH + 1;
            string newMaKH = $"KH{nextKH:D3}";

            // 3) Thêm USER
            var user = new USER
            {
                ID_User = newIdUser,
                EMAIL = model.EMAIL.Trim(),
                PASSWORD = model.PASSWORD.Trim(),
                ID_ROLE = 1
            };
            db.USERS.Add(user);

            // 4) Thêm Khach_Hang liên kết
            var kh = new Khach_Hang
            {
                MA_KH = newMaKH,
                ID_User = newIdUser,
                EMAIL = model.EMAIL.Trim(),
                HO_TEN_KH = "",
                SDT_KH = "",
                DIA_CHI = "",
                ANH_DAI_DIEN = null
            };
            db.Khach_Hang.Add(kh);

            db.SaveChanges();  // bây giờ chắc chắn không đụng khóa chính
            return RedirectToAction("Index");
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
        public ActionResult Details(string id)
        {
            if (id == null) return HttpNotFound();

            var data = (from u in db.USERS
                        join k in db.Khach_Hang on u.ID_User equals k.ID_User
                        join r in db.Roles on u.ID_ROLE equals r.ID_ROLE
                        where u.ID_User == id
                        select new UserKhachHangViewModel1
                        {
                            ID_User = u.ID_User,
                            EMAIL = u.EMAIL,
                            PASSWORD = u.PASSWORD,
                            ID_ROLE = u.ID_ROLE,
                            TEN_ROLE = r.TEN_ROLE,
                            MA_KH = k.MA_KH,
                            HO_TEN_KH = k.HO_TEN_KH,
                            SDT_KH = k.SDT_KH,
                            DIA_CHI = k.DIA_CHI,
                            ANH_DAI_DIEN = k.ANH_DAI_DIEN
                        }).FirstOrDefault();

            if (data == null) return HttpNotFound();

            return View(data);
        }
        [HttpGet]
        public ActionResult Edit(string id)
        {
            if (id == null) return HttpNotFound();

            var data = (from u in db.USERS
                        join k in db.Khach_Hang on u.ID_User equals k.ID_User
                        where u.ID_User == id
                        select new UserKhachHangViewModel1
                        {
                            ID_User = u.ID_User,
                            EMAIL = u.EMAIL,
                            PASSWORD = u.PASSWORD,
                            ID_ROLE = u.ID_ROLE,
                            MA_KH = k.MA_KH,
                            HO_TEN_KH = k.HO_TEN_KH,
                            SDT_KH = k.SDT_KH,
                            DIA_CHI = k.DIA_CHI,
                            ANH_DAI_DIEN = k.ANH_DAI_DIEN
                        }).FirstOrDefault();

            if (data == null) return HttpNotFound();

            ViewBag.RoleList = new SelectList(db.Roles.ToList(), "ID_ROLE", "TEN_ROLE", data.ID_ROLE);
            return View(data);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserKhachHangViewModel1 model, HttpPostedFileBase AnhUpload)
        {
            CheckValues(model);
            if (!ModelState.IsValid)
            {
                ViewBag.RoleList = new SelectList(db.Roles.ToList(), "ID_ROLE", "TEN_ROLE", model.ID_ROLE);
                return View(model);
            }


            if (ModelState.IsValid)
            {
                var user = db.USERS.Find(model.ID_User);
                if (user != null)
                {
                    user.EMAIL = model.EMAIL;
                    user.PASSWORD = model.PASSWORD;
                    user.ID_ROLE = model.ID_ROLE;
                }

                var khach = db.Khach_Hang.FirstOrDefault(k => k.ID_User == model.ID_User);
                if (khach != null)
                {
                    khach.HO_TEN_KH = model.HO_TEN_KH;
                    khach.SDT_KH = model.SDT_KH;
                    khach.DIA_CHI = model.DIA_CHI;
                    khach.EMAIL = model.EMAIL;

                    if (AnhUpload != null && AnhUpload.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(AnhUpload.FileName);
                        string folderPath = Server.MapPath("/Content/Uploads/KhachHang/");
                        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                        string filePath = Path.Combine(folderPath, fileName);
                        AnhUpload.SaveAs(filePath);

                        khach.ANH_DAI_DIEN = "/Content/Uploads/KhachHang/" + fileName;
                    }
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(model);
        }
        private void CheckValues(UserKhachHangViewModel1 model)
        {
            if (string.IsNullOrWhiteSpace(model.EMAIL))
                ModelState.AddModelError("EMAIL", "Email không được để trống");
            else if (!model.EMAIL.EndsWith("@gmail.com"))
                ModelState.AddModelError("EMAIL", "Email phải có đuôi @gmail.com");

            if (string.IsNullOrWhiteSpace(model.PASSWORD))
                ModelState.AddModelError("PASSWORD", "Mật khẩu không được để trống");
            else if (model.PASSWORD.Length < 8 || !System.Text.RegularExpressions.Regex.IsMatch(model.PASSWORD, @"\d"))
                ModelState.AddModelError("PASSWORD", "Mật khẩu phải có ít nhất 8 ký tự và chứa ít nhất 1 số");

            if (string.IsNullOrWhiteSpace(model.HO_TEN_KH))
                ModelState.AddModelError("HO_TEN_KH", "Họ tên không được để trống");

            if (string.IsNullOrWhiteSpace(model.SDT_KH))
                ModelState.AddModelError("SDT_KH", "Số điện thoại không được để trống");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(model.SDT_KH, @"^0\d{9}$"))
                ModelState.AddModelError("SDT_KH", "Số điện thoại phải gồm 10 chữ số và bắt đầu bằng số 0");

            if (string.IsNullOrWhiteSpace(model.DIA_CHI))
                ModelState.AddModelError("DIA_CHI", "Địa chỉ không được để trống");
        }


    }
}

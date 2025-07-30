
﻿using CNPM_Project_web.Model;
using System;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Web;
using System.Data.Entity.Infrastructure;
using System.Web.Mvc;
using CNPM_Project_web.Model;

namespace CNPM_Project_web.Areas.Admin.Controllers
{
    public class User_KhachHangController : Controller
    {
        private Web_Entities db = new Web_Entities();

        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.RoleList = new SelectList(db.Roles.ToList(), "ID_ROLE", "TEN_ROLE");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserKhachHangViewModel model, HttpPostedFileBase AnhUpload)
        {
            CheckValues(model);

            ViewBag.RoleList = new SelectList(db.Roles.ToList(), "ID_ROLE", "TEN_ROLE");

            if (ModelState.IsValid)
            {
                // Xử lý lưu ảnh nếu có
                string filePath = null;
                if (AnhUpload != null && AnhUpload.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(AnhUpload.FileName);
                    string serverPath = Server.MapPath("~/Content/Uploads/KhachHang/");
                    if (!Directory.Exists(serverPath)) Directory.CreateDirectory(serverPath);

                    string fullPath = Path.Combine(serverPath, fileName);
                    AnhUpload.SaveAs(fullPath);

                    filePath = "/Content/Uploads/KhachHang/" + fileName;
                }

                // Tạo mã USER_...
                string lastUserId = db.USERS.OrderByDescending(u => u.ID_User).Select(u => u.ID_User).FirstOrDefault();
                int nextUserNumber = 1;
                if (!string.IsNullOrEmpty(lastUserId) && lastUserId.StartsWith("USER_"))
                {
                    int.TryParse(lastUserId.Substring(5), out nextUserNumber);
                    nextUserNumber++;
                }
                string newIdUser = $"USER_{nextUserNumber:D3}";

                // Tạo mã KH...
                string lastMaKH = db.Khach_Hang.OrderByDescending(k => k.MA_KH).Select(k => k.MA_KH).FirstOrDefault();
                int nextMaKHNumber = 1;
                if (!string.IsNullOrEmpty(lastMaKH) && lastMaKH.StartsWith("KH"))
                {
                    int.TryParse(lastMaKH.Substring(2), out nextMaKHNumber);
                    nextMaKHNumber++;
                }
                string newMaKH = $"KH{nextMaKHNumber:D3}";

                // Thêm user
                var user = new USER
                {
                    ID_User = newIdUser,
                    EMAIL = model.EMAIL,
                    PASSWORD = model.PASSWORD,
                    ID_ROLE = model.ID_ROLE
                };
                db.USERS.Add(user);

                // Thêm khách hàng
                var khach = new Khach_Hang
                {
                    MA_KH = newMaKH,
                    HO_TEN_KH = model.HO_TEN_KH,
                    SDT_KH = model.SDT_KH,
                    DIA_CHI = model.DIA_CHI,
                    EMAIL = model.EMAIL,
                    ANH_DAI_DIEN = filePath, // ảnh đã upload
                    ID_User = newIdUser
                };
                db.Khach_Hang.Add(khach);

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public ActionResult Index(string searchString, string sortOrder, string roleFilter, int? pageSize)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.EmailSortParm = sortOrder == "email" ? "email_desc" : "email";

            // Get all data first
            var query = from u in db.USERS
                        join k in db.Khach_Hang on u.ID_User equals k.ID_User
                        join r in db.Roles on u.ID_ROLE equals r.ID_ROLE
                        select new UserKhachHangViewModel
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
                        };

            // Apply search filter
            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                query = query.Where(u => 
                    (u.HO_TEN_KH != null && u.HO_TEN_KH.ToLower().Contains(searchString)) ||
                    (u.EMAIL != null && u.EMAIL.ToLower().Contains(searchString)) ||
                    (u.SDT_KH != null && u.SDT_KH.ToLower().Contains(searchString)) ||
                    (u.DIA_CHI != null && u.DIA_CHI.ToLower().Contains(searchString))
                );
            }

            // Apply role filter
            if (!String.IsNullOrEmpty(roleFilter))
            {
                if (roleFilter == "admin")
                {
                    query = query.Where(u => u.TEN_ROLE != null && u.TEN_ROLE.ToLower().Contains("admin"));
                }
                else if (roleFilter == "user")
                {
                    query = query.Where(u => u.TEN_ROLE != null && u.TEN_ROLE.ToLower().Contains("user"));
                }
            }

            // Apply sorting
            switch (sortOrder)
            {
                case "name_desc":
                    query = query.OrderByDescending(u => u.HO_TEN_KH);
                    break;
                case "email":
                    query = query.OrderBy(u => u.EMAIL);
                    break;
                case "email_desc":
                    query = query.OrderByDescending(u => u.EMAIL);
                    break;
                default:
                    query = query.OrderBy(u => u.HO_TEN_KH);
                    break;
            }

            // Set ViewBag values
            ViewBag.SearchString = searchString;
            ViewBag.RoleFilter = roleFilter;
            ViewBag.PageSize = pageSize ?? 10;

            // Execute query and return
            var result = query.ToList();
            return View(result);
        }

        public ActionResult Details(string id)
        {
            if (id == null) return HttpNotFound();

            var data = (from u in db.USERS
                        join k in db.Khach_Hang on u.ID_User equals k.ID_User
                        join r in db.Roles on u.ID_ROLE equals r.ID_ROLE
                        where u.ID_User == id
                        select new UserKhachHangViewModel
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
                        select new UserKhachHangViewModel
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
        public ActionResult Edit(UserKhachHangViewModel model, HttpPostedFileBase AnhUpload)
        {
            if (ModelState.IsValid)
            {
                var user = db.USERS.Find(model.ID_User);
                var kh = db.Khach_Hang.Find(model.MA_KH);

                if (user != null && kh != null)
                {
                    user.PASSWORD = model.PASSWORD;
                    user.ID_ROLE = model.ID_ROLE;

                    kh.HO_TEN_KH = model.HO_TEN_KH;
                    kh.SDT_KH = model.SDT_KH;
                    kh.DIA_CHI = model.DIA_CHI;

                    // Xử lý ảnh đại diện mới nếu có upload
                    if (AnhUpload != null && AnhUpload.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(AnhUpload.FileName);
                        string path = Path.Combine(Server.MapPath("~/Content/Upload/KhachHang"), fileName);
                        AnhUpload.SaveAs(path);
                        kh.ANH_DAI_DIEN = "~/Content/Upload/KhachHang" + fileName;
                    }

                    db.SaveChanges();
                    TempData["Success"] = "Cập nhật người dùng thành công!";
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError("", "Không tìm thấy người dùng.");
            }

         
            return View(model);
        }





     // thêm ở đầu file

        public ActionResult Delete(string id)
            {
        var khach = db.Khach_Hang.FirstOrDefault(k => k.ID_User == id);
        var user = db.USERS.Find(id);

        try
        {
            if (khach != null) db.Khach_Hang.Remove(khach);
            if (user != null) db.USERS.Remove(user);
            db.SaveChanges();
            TempData["Success"] = "Xóa khách hàng thành công.";
        }
        catch (DbUpdateException)
        {
            // vài trường hợp: nếu còn Don_Hang tham chiếu tới MA_KH
            TempData["Error"] = "Không thể xóa khách hàng này vì còn đơn hàng liên quan.";

        }

        return RedirectToAction("Index");
    }


    private void CheckValues(UserKhachHangViewModel model)
        {
            if (string.IsNullOrEmpty(model.HO_TEN_KH))
            {
                ModelState.AddModelError("HO_TEN_KH", "Họ tên không được để trống");
            }

            if (string.IsNullOrEmpty(model.EMAIL))
            {
                ModelState.AddModelError("EMAIL", "Email không được để trống");
            }
            else if (!IsValidEmail(model.EMAIL))
            {
                ModelState.AddModelError("EMAIL", "Email không hợp lệ");
            }

            if (string.IsNullOrEmpty(model.SDT_KH))
            {
                ModelState.AddModelError("SDT_KH", "Số điện thoại không được để trống");
            }

            if (string.IsNullOrEmpty(model.DIA_CHI))
            {
                ModelState.AddModelError("DIA_CHI", "Địa chỉ không được để trống");
            }

            if (string.IsNullOrEmpty(model.PASSWORD))
            {
                ModelState.AddModelError("PASSWORD", "Mật khẩu không được để trống");
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
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
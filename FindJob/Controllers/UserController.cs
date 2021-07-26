using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using FindJob.Models;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.IO;
using System.Net.Mail;
using System.Net;

namespace FindJob.Controllers
{
    public class UserController : Controller
    {


        FindJobDataContext db = new FindJobDataContext();
        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult DangNhap()
        {
            if (Session["TaiKhoan"] != null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View();
        }

        [HttpPost]
        public ActionResult DangNhap(FormCollection collection,string JobsDetail)
        {
            var sTenDN = collection["TenDN"];
            var sMatKhau = collection["MatKhau"];
            var iType = collection["type"];
            if (String.IsNullOrEmpty(sTenDN))
            {
                ViewData["Err1"] = "Bạn chưa điền tên đăng nhập";

            }
            else if (String.IsNullOrEmpty(sMatKhau))
            {
                ViewData["Err2"] = "Phải nhập mật khẩu";
            }
            else
            {
                if(iType == "0")
                {
                    KHACHHANG kh = db.KHACHHANGs.SingleOrDefault(n => n.TaiKhoan == sTenDN && n.MatKhau == GetMD5(sMatKhau));
                    if (kh != null)
                    {
                        if (kh.TrangThai != 1)
                        {
                            ViewBag.ThongBao = "Tài khoản bị tạm khóa";
                            return View();
                        }
                        Session["Taikhoan"] = kh;
                        //remember
                        if (collection["Remember"].Contains("true"))
                        {
                            Response.Cookies["TenDN"].Value = sTenDN;
                            Response.Cookies["MatKhau"].Value = sMatKhau;
                            Response.Cookies["TenDN"].Expires = DateTime.Now.AddDays(1);
                            Response.Cookies["MatKhau"].Expires = DateTime.Now.AddDays(1);
                        }
                        else
                        {
                            Response.Cookies["TenDN"].Expires = DateTime.Now.AddDays(-1);
                            Response.Cookies["MatKhau"].Expires = DateTime.Now.AddDays(-1);
                        }
                        if (string.IsNullOrEmpty(JobsDetail))
                        {
                            return RedirectToAction("Index", "Home");
                        }else
                        return RedirectToAction("JobsDetail", "Jobs",new { id= JobsDetail});
                    }
                    else
                    {
                        ViewBag.ThongBao = "Tên đăng nhập hoặc mật khẩu không đúng";
                    }
                }
                else
                {
                    CONGTY cty = db.CONGTies.SingleOrDefault(n => n.TaiKhoan == sTenDN && n.MatKhau == GetMD5(sMatKhau));
                    if (cty != null)
                    {
                        if(cty.TrangThai != 1)
                        {
                            ViewBag.ThongBao = "Tài khoản bị tạm khóa";
                            return View();
                        }

                        Session["CongTy"] = cty;
                        //remember
                        if (collection["Remember"].Contains("true"))
                        {
                            Response.Cookies["TenDN"].Value = sTenDN;
                            Response.Cookies["MatKhau"].Value = sMatKhau;
                            Response.Cookies["TenDN"].Expires = DateTime.Now.AddDays(1);
                            Response.Cookies["MatKhau"].Expires = DateTime.Now.AddDays(1);
                        }
                        else
                        {
                            Response.Cookies["TenDN"].Expires = DateTime.Now.AddDays(-1);
                            Response.Cookies["MatKhau"].Expires = DateTime.Now.AddDays(-1);
                        }
 
                            return RedirectToAction("Index", "Companys");
                    }
                    else
                    {
                        ViewBag.ThongBao = "Tên đăng nhập hoặc mật khẩu không đúng";
                    }
                }
                
                
            }

            return View();
        }


        public ActionResult DangXuat()
        {
            Session["TaiKhoan"] = null;
            Session["CongTy"] = null;
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(FormCollection f)
        {
            var user = db.KHACHHANGs.Where(n => n.Email == f["Email"] && n.TaiKhoan == f["User"]);
            if (user != null)
            {
                var kh = db.KHACHHANGs.SingleOrDefault(n => n.TaiKhoan == f["User"]);
                string str_build = RandomStr(7);
                var mailClient = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("websitedoban@gmail.com", "22023794A"),
                    EnableSsl = true
                };

                var message = new MailMessage();
                message.From = new MailAddress("websitedoban@gmail.com");
                message.To.Add(new MailAddress(f["Email"]));
                message.Subject = "Reset Password";
                message.Body = "New Password: " + str_build;
                mailClient.Send(message);
                kh.MatKhau = GetMD5(str_build.ToString());
                db.SubmitChanges();
                return RedirectToAction("DangNhap", "User");
            }
            return View();
        }

        [HttpGet]
        public ActionResult DangKy()
        {
            
            return View();
        }

        [HttpPost]
        public ActionResult DangKy(FormCollection collection)
        {

            var sTaiKhoan = collection["TaiKhoan"];
            var sMatKhau = collection["MatKhau"];
            var sMatKhauNhapLai = collection["MatKhauNL"];
            var sHoTen = collection["HoTen"];
            var sEmail = collection["Email"];
            var sDienThoai = collection["DienThoai"];
            var iType = collection["type"];
            if (String.IsNullOrEmpty(sMatKhauNhapLai))
            {
                ViewBag.ThongBao = "Phải nhập lại mật khẩu";
            }
            else if (sMatKhau != sMatKhauNhapLai)
            {
                ViewBag.ThongBao = "Mật khẩu nhập lại không khớp";
            }
            else if ((db.KHACHHANGs.SingleOrDefault(n => n.TaiKhoan == sTaiKhoan) != null && iType == "0") || (db.CONGTies.SingleOrDefault(n => n.TaiKhoan == sTaiKhoan) != null && iType == "1"))
            {
                ViewBag.ThongBao = "Tên đăng nhập đã tồn tại";
            }
            else if ((db.KHACHHANGs.SingleOrDefault(n => n.Email == sEmail) != null && iType == "0") || (db.CONGTies.SingleOrDefault(n => n.Email == sEmail) != null && iType == "1"))
            {
                ViewBag.ThongBao = "Email không được trùng";
            }
            else if (ValidatePassword(sMatKhau) == false)
            {
                ViewBag.ThongBao = "Mật khẩu không đủ mạnh!";
            }
            else if (String.IsNullOrEmpty(sEmail))
            {
                ViewBag.ThongBao = "Email không đươc rỗng";
            }
            else if (!isValidEmail(sEmail))
            {
                ViewBag.ThongBao = "Email không đúng định dạng";
            }
            else if (String.IsNullOrEmpty(sDienThoai))
            {
                ViewBag.ThongBao = "Số điện thoại không được rỗng";
            }
            else if (!IsValidPhone(sDienThoai))
            {
                ViewBag.ThongBao = "Số điện thoại sai định dạng";
            }
            else
            {
                if(iType == "0")
                {
                    KHACHHANG kh = new KHACHHANG();
                    kh.TaiKhoan = sTaiKhoan;
                    kh.MatKhau = GetMD5(sMatKhau);
                    kh.HoTen = sHoTen;
                    kh.Email = sEmail;
                    kh.DienThoai = sDienThoai;
                    kh.AnhDaiDien = "default.jpg";
                    kh.MaNganh = 1;
                    kh.NamKinhNghiem = 0;
                    kh.GioiTinh = 1;
                    kh.TrangThai = 1;
                    db.KHACHHANGs.InsertOnSubmit(kh);
                    db.SubmitChanges();
                }
                else
                {
                    CONGTY cty = new CONGTY();
                    cty.TaiKhoan = sTaiKhoan;
                    cty.MatKhau = GetMD5(sMatKhau);
                    cty.TenCty = sHoTen;
                    cty.Email = sEmail;
                    cty.TrangThai = 1;
                    cty.Logo = "default.png";
                    cty.Background = "default.jpg";
                    cty.DienThoai = sDienThoai;
                    db.CONGTies.InsertOnSubmit(cty);
                    db.SubmitChanges();
                }

                return RedirectToAction("DangNhap");
            }
            return this.DangKy();

        }

        [HttpGet]
        public ActionResult Profile(int? id)
        {
            if (id > 0)
            {
                var profile = db.KHACHHANGs.Where(n => n.MaKH == id).SingleOrDefault();
                ViewBag.NganhNghe = new SelectList(db.NganhNghes.ToList().OrderBy(n => n.MaNganh), "MaNganh", "TenNganh", profile.MaNganh);
                return View(profile);
            }
            else
            {
                if (Session["TaiKhoan"] == null || Session["TaiKhoan"] == "")
                {
                    return RedirectToAction("DangNhap");
                }
                var kh = (KHACHHANG)Session["TaiKhoan"];
                var profile = db.KHACHHANGs.Where(n => n.MaKH == kh.MaKH).SingleOrDefault();
                ViewBag.NganhNghe = new SelectList(db.NganhNghes.ToList().OrderBy(n => n.MaNganh), "MaNganh", "TenNganh", profile.MaNganh);
                return View(profile);
            }
        }

        [HttpPost]
        public ActionResult Profile(FormCollection collection , HttpPostedFileBase AnhDaiDien, HttpPostedFileBase fileCv)
        {
            var user = (KHACHHANG)Session["TaiKhoan"];
            var kh = db.KHACHHANGs.SingleOrDefault(n => n.MaKH == user.MaKH);
            var sHoTen = collection["HoTen"];
            var sTrinhDo = collection["TrinhDo"];
            var iKinhNghiem = collection["KinhNghiem"];
            var iNganhNghe = collection["NganhNghe"];
            var iGioiTinh = collection["GioiTinh"];
            var sEmail = collection["Email"];
            var sDiaChi = collection["DiaChi"];
            var sDienThoai = collection["DienThoai"];
            var dNgaySinh = String.Format("{0:MM/dd/yyyy}", collection["NgaySinh"]);
            var sMatKhau_old = collection["oldPW"];
            var sMatKhau_new = collection["newPW"];
            
            if (sEmail != user.Email && db.KHACHHANGs.SingleOrDefault(n => n.Email == sEmail) != null)
            {
                    ViewBag.ThongBao = "Email đã tồn tại";
                
            }
            else if (!String.IsNullOrEmpty(sMatKhau_new) && String.IsNullOrEmpty(sMatKhau_old))
            {
                ViewBag.ThongBao = "Phải điền mật khẩu cũ";
            }
            else if (!String.IsNullOrEmpty(sMatKhau_old) && GetMD5(sMatKhau_old) != kh.MatKhau)
            {
                ViewBag.ThongBao = "Mật khẩu cũ không đúng";
            }
            else if (!String.IsNullOrEmpty(sMatKhau_new) && ValidatePassword(sMatKhau_new) == false )
            {
                ViewBag.ThongBao = "Mật khẩu không đủ mạnh!";
            }
            else if (String.IsNullOrEmpty(sEmail))
            {
                ViewBag.ThongBao = "Email không đươc rỗng";
            }
            else if (!isValidEmail(sEmail))
            {
                ViewBag.ThongBao = "Email không đúng định dạng";
            }
            else if (String.IsNullOrEmpty(sDienThoai))
            {
                ViewBag.ThongBao = "Số điện thoại không được rỗng";
            }
            else if (!IsValidPhone(sDienThoai))
            {
                ViewBag.ThongBao = "Số điện thoại sai định dạng";
            }
            else if (DateTime.Parse(dNgaySinh) >= DateTime.Now)
            {
                ViewBag.ThongBao = "Ngày sinh phải bé hơn ngày hiện tại";
            }
            else
            {
                if (!string.IsNullOrEmpty(sMatKhau_new))
                    kh.MatKhau = GetMD5(sMatKhau_new);

                if (AnhDaiDien != null)
                {
                    var sFileName = Path.GetFileName(AnhDaiDien.FileName);

                    var path = Path.Combine(Server.MapPath("~/assets/img/avatar"), sFileName);

                    while (System.IO.File.Exists(path))
                    {
                        sFileName = RandomStr(2) + sFileName;
                        path = Path.Combine(Server.MapPath("~/assets/img/avatar"), sFileName);
                    }
                    AnhDaiDien.SaveAs(path);
                    kh.AnhDaiDien = sFileName;
                }

                if (fileCv != null)
                {
                    var sFileCv = Path.GetFileName(fileCv.FileName);

                    var path = Path.Combine(Server.MapPath("~/assets/img/cv"), sFileCv);

                    while (System.IO.File.Exists(path))
                    {
                        sFileCv = RandomStr(2)+ sFileCv;
                        path = Path.Combine(Server.MapPath("~/assets/img/cv"), sFileCv);
                    }
                    fileCv.SaveAs(path);
                    kh.FileCV = sFileCv;
                }

                kh.HoTen = sHoTen;
                kh.GioiTinh = int.Parse(iGioiTinh);
                kh.Email = sEmail;
                kh.DiaChi = sDiaChi.Trim();
                kh.DienThoai = sDienThoai;
                kh.NgaySinh = DateTime.Parse(dNgaySinh);
                kh.TrinhDo = sTrinhDo;
                kh.MaNganh = int.Parse(iNganhNghe);
                kh.NamKinhNghiem = int.Parse(iKinhNghiem);
                db.SubmitChanges();
                Session["TaiKhoan"] = kh;
                return RedirectToAction("Profile");
            }
            return this.Profile(0);
        }




        public static string GetMD5(string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] formData = Encoding.UTF8.GetBytes(str);
            byte[] targetData = md5.ComputeHash(formData);
            string byte2String = null;

            for (int i = 0; i < targetData.Length; i++)
            {
                byte2String += targetData[i].ToString("x2");

            }
            return byte2String;
        }
        private bool ValidatePassword(string password)
        {
            var input = password;
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new Exception("Password should not be empty");
            }

            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasMiniMaxChars = new Regex(@".{8,15}");
            var hasLowerChar = new Regex(@"[a-z]+");
            if (!hasLowerChar.IsMatch(input))
            {
                ViewBag.ThongBao = "Mật khẩu phải chứa ít nhất một chữ cái thường.";
                return false;
            }
            else if (!hasUpperChar.IsMatch(input))
            {
                ViewBag.ThongBao = "Mật khẩu phải chứa ít nhất một chữ cái viết hoa.";
                return false;
            }
            else if (!hasMiniMaxChars.IsMatch(input))
            {
                ViewBag.ThongBao = "Mật khẩu không được nhỏ hơn 8 hoặc lớn hơn 15 ký tự.";
                return false;
            }
            else if (!hasNumber.IsMatch(input))
            {
                ViewBag.ThongBao = "Mật khẩu phải chứa ít nhất một giá trị số.";
                return false;
            }
            else
            {
                return true;
            }
        }

        public static string RandomStr(int length)
        {
            StringBuilder str_build = new StringBuilder();
            Random random = new Random();

            char letter;

            for (int i = 0; i < length; i++)
            {
                double flt = random.NextDouble();
                int shift = Convert.ToInt32(Math.Floor(25 * flt));
                letter = Convert.ToChar(shift + 65);
                str_build.Append(letter);
            }
            return str_build.ToString();
        }
        private static bool isValidEmail(string inputEmail)
        {
            string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                  @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                  @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            Regex re = new Regex(strRegex);
            if (re.IsMatch(inputEmail))
                return (true);
            else
                return (false);
        }
        private bool IsValidPhone(string Phone)
        {
            try
            {
                if (string.IsNullOrEmpty(Phone))
                    return false;
                var r = new Regex(@"(84|0[3|5|7|8|9])+([0-9]{8})\b");
                return r.IsMatch(Phone);

            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
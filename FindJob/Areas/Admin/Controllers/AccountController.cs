using System;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FindJob.Models;
using System.Security.Cryptography;

namespace FindJob.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {

        FindJobDataContext db = new FindJobDataContext();
        // GET: Admin/Account
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(FormCollection f)
        {
            var sTenDN = f["TenDN"];
            var sMatKhau = f["MatKhau"];
            var iType = f["type"];
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

                ADMIN adm = db.ADMINs.SingleOrDefault(n => n.TaiKhoan == sTenDN && n.MatKhau == GetMD5(sMatKhau));
                if (adm != null)
                {
                    Session["Admin"] = adm;
                    //remember
                    if (f["Remember"].Contains("true"))
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
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    ViewBag.ThongBao = "Tên đăng nhập hoặc mật khẩu không đúng";
                }
            }
            return View();
        }


        public ActionResult Logout()
        {
            Session["Admin"] = null;
            return RedirectToAction("Login", "Account");
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



    }
}
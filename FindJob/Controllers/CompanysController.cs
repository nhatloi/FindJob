using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;
using PagedList.Mvc;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using FindJob.Models;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.Net.Mime;

namespace FindJob.Controllers
{
    public class CompanysController : BaseController
    {

        FindJobDataContext db = new FindJobDataContext();
        // GET: Companys
        public ActionResult Index(int? page)
        {
            var cty = (CONGTY)Session["CongTy"];
            int iPageNum = (page ?? 1);
            int iPageSize = 3;
            var post = db.TinTimViecs.Where(n => n.MaCty == cty.MaCTy).ToList();
            return View(post.ToPagedList(iPageNum, iPageSize));
        }

        public ActionResult LeftMenu()
        {
            var cty = (CONGTY)Session["CongTy"];
            ViewBag.Cty = cty.TenCty;
            return PartialView();
        }

        public ActionResult Navbar()
        {
            return PartialView();
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var cty = (CONGTY)Session["CongTy"];
            var cv = db.TinTimViecs.SingleOrDefault(n => n.MaTin == id);
            if (cv.MaCty != cty.MaCTy)
                return RedirectToAction("Index");
            if (cv == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            ViewBag.NganhNghe = new SelectList(db.NganhNghes.ToList().OrderBy(n => n.MaNganh), "MaNganh", "TenNganh", cv.MaNganh);
            return View(cv);
        }


        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(FormCollection f)
        {
            var cv = db.TinTimViecs.SingleOrDefault(n => n.MaTin == int.Parse(f["iMaTin"]));
            if (ModelState.IsValid)
            {
                cv.MaNganh = int.Parse(f["NganhNghe"]);
                cv.ViTriLamViec = f["sViTri"];
                cv.KieuCV = f["sKieuCV"];
                cv.LuongToiThieu = int.Parse(f["iLuongTT"]);
                cv.LuongToiDa = int.Parse(f["iLuongTD"]);
                cv.KinhNghiem = int.Parse(f["iKinhNghiem"]);
                cv.MoTa = f["sMoTa"].Replace("<p>", "").Replace("</p>", "\n");
                cv.YeuCauCV = f["sYeuCauCV"].Replace("<p>", "").Replace("</p>", "\n");
                cv.NgayDangTin = Convert.ToDateTime(f["dNgayDangTin"]);
                cv.HanNopHoSo = Convert.ToDateTime(f["dHanNopHoSo"]);
                db.SubmitChanges();

                return RedirectToAction("Index");

            }
            return View(cv);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {

            var cty = (CONGTY)Session["CongTy"];
            var cv = db.TinTimViecs.SingleOrDefault(n => n.MaTin == id);
            if (cv == null || cv.MaCty != cty.MaCTy)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(cv);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConFirm(int id, FormCollection f)
        {
            var cty = (CONGTY)Session["CongTy"];
            var cv = db.TinTimViecs.SingleOrDefault(n => n.MaTin == id);
            if (cv == null || cv.MaCty != cty.MaCTy)
            {
                Response.StatusCode = 404;
                return null;
            }

            var donUT = db.DonUngTuyens.Where(ut => ut.MaTin == id);
            if (donUT.Count() > 0)
            {
                var don = db.DonUngTuyens.Where(vs => vs.MaTin == id).ToList();
                db.DonUngTuyens.DeleteAllOnSubmit(don);
                db.SubmitChanges();
            }
            db.TinTimViecs.DeleteOnSubmit(cv);
            db.SubmitChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Create()
        {
            var cty = (CONGTY)Session["CongTy"];
            ViewBag.NganhNghe = new SelectList(db.NganhNghes.ToList().OrderBy(n => n.TenNganh), "MaNganh", "TenNganh");
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(FormCollection f)
        {

            ViewBag.NganhNghe = new SelectList(db.NganhNghes.ToList().OrderBy(n => n.TenNganh), "MaNganh", "TenNganh");
            if (ModelState.IsValid)
            {
                var cv = new TinTimViec();
                cv.MaCty = int.Parse(f["iMaCty"]);
                cv.MaNganh = int.Parse(f["NganhNghe"]);
                cv.ViTriLamViec = f["sViTri"];
                cv.KieuCV = f["sKieuCV"];
                cv.LuongToiThieu = int.Parse(f["iLuongTT"]);
                cv.LuongToiDa = int.Parse(f["iLuongTD"]);
                cv.KinhNghiem = int.Parse(f["iKinhNghiem"]);
                cv.MoTa = f["sMoTa"].Replace("<p>", "").Replace("</p>", "\n");
                cv.YeuCauCV = f["sYeuCauCV"].Replace("<p>", "").Replace("</p>", "\n");
                cv.NgayDangTin = Convert.ToDateTime(f["dNgayDangTin"]);
                cv.HanNopHoSo = Convert.ToDateTime(f["dHanNopHoSo"]);
                cv.TrangThai = 0;
                db.TinTimViecs.InsertOnSubmit(cv);
                db.SubmitChanges();

                return RedirectToAction("Index");
            }
            else
            {
                string text = f["iMaCty"] + f["NganhNghe"] + f["sViTri"] + f["sKieuCV"] + f["iLuongTT"] + f["iLuongTD"] + f["iKinhNghiem"] + f["sMoTa"] + f["sYeuCauCV"] + f["dNgayDangTin"] + f["dHanNopHoSo"];
                ViewBag.ThongBao = text;
                return View();
            }
        }

        [HttpGet]
        public ActionResult Profile()
        {
            var cty = (CONGTY)Session["CongTy"];
            ViewBag.KhuVuc = new SelectList(db.KhuVucs.ToList().OrderBy(n => n.MaKV), "MaKV", "TenKhuVuc",cty.MaKV);
            return View(cty);
        }

        [HttpPost]
        public ActionResult Profile(FormCollection f, HttpPostedFileBase sLogo, HttpPostedFileBase sBg)
        {
            var user = (CONGTY)Session["CongTy"];
            var Cty = db.CONGTies.SingleOrDefault(n => n.MaCTy == user.MaCTy);
            var sTenCty = f["TenCty"];
            var sLinhVuc = f["LinhVuc"];
            var iKhuVuc = f["KhuVuc"];
            var sWebsite = f["Website"];
            var sEmail = f["Email"];
            var sDiaChi = f["DiaChi"];
            var sDienThoai = f["DienThoai"];
            var sMoTa = f["Mota"];

            if (ModelState.IsValid)
            {
                if (sLogo != null)
                {
                    var sFileName = Path.GetFileName(sLogo.FileName);

                    var path = Path.Combine(Server.MapPath("~/assets/img/icon"), sFileName);

                    while (System.IO.File.Exists(path))
                    {
                        sFileName = UserController.RandomStr(2) + sFileName;
                        path = Path.Combine(Server.MapPath("~/assets/img/icon"), sFileName);
                    }
                    sLogo.SaveAs(path);
                    Cty.Logo = sFileName;
                }
                

                if (sBg != null)
                {
                    var sFileBG = Path.GetFileName(sBg.FileName);

                    var path = Path.Combine(Server.MapPath("~/assets/img/banner"), sFileBG);

                    if (!System.IO.File.Exists(path))
                    {
                        sFileBG = UserController.RandomStr(2) + sFileBG;
                        path = Path.Combine(Server.MapPath("~/assets/img/banner"), sFileBG);
                    }
                    sBg.SaveAs(path);
                    Cty.Background = sFileBG;
                }

                Cty.TenCty = sTenCty;
                Cty.LinhVuc = sLinhVuc;
                Cty.MaKV = int.Parse(iKhuVuc);
                Cty.Website = sWebsite;
                Cty.Email = sEmail;
                Cty.DiaChi = sDiaChi;
                Cty.DienThoai = sDienThoai;
                Cty.MoTa = sMoTa.Replace("<p>", "").Replace("</p>", "\n");
                db.SubmitChanges();
                Session["CongTy"] = Cty;
                return RedirectToAction("Profile");
            }
            return View();
        }

        public ActionResult Details(int id)
        {
            var cty = (CONGTY)Session["CongTy"];
            var cv = db.TinTimViecs.SingleOrDefault(n => n.MaTin == id);
            if (cv == null || cv.MaCty != cty.MaCTy)
            {
                Response.StatusCode = 404;
                return null;
            }

            var listDon = db.DonUngTuyens.Where(n => n.MaTin == id);
            return View(listDon);
        }
        [HttpGet]
        public ActionResult SendMail(int user,int post)
        {
            var kh = db.KHACHHANGs.Where(n=> n.MaKH == user).SingleOrDefault();
            ViewBag.Email = kh.Email;
            ViewBag.Post = post;
            return View();
        }

        [HttpPost]
        public ActionResult SendMail(FormCollection f)
        {
            var cty = (CONGTY)Session["CongTy"];
            var mail = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("websitedoban@gmail.com", "22023794A"),
                EnableSsl = true
            };

            var message = new MailMessage();
            message.From = new MailAddress("websitedoban@gmail.com");
            message.To.Add(new MailAddress(f["Email"]));
            message.Subject = "Reply Mail from company " + cty.TenCty;
            message.Body = f["message"];
            mail.Send(message);
            return View();
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(FormCollection f)
        {
            var user = (CONGTY)Session["CongTy"];
            var sNewPW = f["newPW"];
            var sOldPW = f["oldPW"];
            var sCfPW = f["CfPW"];
            var cty = db.CONGTies.SingleOrDefault(n => n.MaCTy == user.MaCTy);
            if(cty.MatKhau != UserController.GetMD5(sOldPW))
            {
                ViewBag.ThongBao = "Mật khẩu cũ không trùng khớp!";
                return View();
            }
            if (ValidatePassword(sNewPW) == false)
            {
                return View();
            }
            if (sCfPW != sNewPW)
            {
                ViewBag.ThongBao = "Mật khẩu nhập lại không trùng khớp!";
                return View();
            }
            cty.MatKhau = UserController.GetMD5(sNewPW);
            db.SubmitChanges();
            ViewBag.ThongBao = "Thay đổi mật khẩu thành công!";
            return View();
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


    }
}
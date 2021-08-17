using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;
using PagedList.Mvc;
using System.Web.Mvc;
using FindJob.Models;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.Net.Mime;

namespace FindJob.Controllers
{
    public class JobsController : Controller
    {

        FindJobDataContext db = new FindJobDataContext();
        // GET: Jobs

        private List<TinTimViec> Laytin(int count,string sort)
        {
            if(count == -1)
            {
                var listCv = db.TinTimViecs.Where(n => n.TrangThai == 1);
                if (sort == "salary")
                    return listCv.OrderByDescending(n => n.LuongToiDa).ToList();
                if (sort == "expire")
                    return listCv.OrderByDescending(n => n.KinhNghiem).ToList();
                return db.TinTimViecs.Where(n=>n.TrangThai==1).ToList();
            }
            return db.TinTimViecs.Where(n=>n.TrangThai == 1).OrderByDescending(a => a.NgayDangTin).Take(count).ToList();
        }

        public ActionResult Index(int? page, string sort)
        {
            int iSize = 5;
            int iPageNum = (page ?? 1);
            var listCV = Laytin(-1,sort);
            ViewBag.Nganh = db.NganhNghes.ToList().OrderBy(n => n.TenNganh);
            ViewBag.KhuVuc = db.KhuVucs.ToList().OrderBy(n => n.MaKV);
            return View(listCV.ToPagedList(iPageNum, iSize));
        }

        public ActionResult JobsDetail(int id)
        {
            ViewBag.Apply = "true";
            if (Session["TaiKhoan"] != null)
            {
                KHACHHANG kh = (KHACHHANG)Session["TaiKhoan"];
                var don = db.DonUngTuyens.Where(n => n.MaTin == id && n.MaKH == kh.MaKH).SingleOrDefault();
                if(don!= null)
                {
                    ViewBag.Apply = "false";
                }
            }
            var viec = (from v in db.TinTimViecs
                    where v.MaTin == id
                    select v).SingleOrDefault();
            return View(viec);
        }

        public ActionResult LeftContent()
        {
            ViewBag.Nganh = db.NganhNghes.ToList().OrderBy(n => n.TenNganh);
            ViewBag.KhuVuc = db.KhuVucs.ToList().OrderBy(n => n.MaKV);
            return PartialView();
        }


        public ActionResult Search(int? page, FormCollection f)
        {

            int iSize = 10;
            int iPageNum = (page ?? 1);

            var kq = from v in db.TinTimViecs where v.TrangThai == 1 select v;
            if (f["strSearch"] != null)
                kq = from v in kq where v.ViTriLamViec.Contains(f["strSearch"]) select v;
            if (f["Category"] != "all")
                kq = from v in kq where v.CONGTY.LinhVuc.Contains(f["Category"]) select v;
            if (f["JobType"] != null)
            {
                if (f["JobType"].Split(',').Length == 1)
                    kq = from v in kq where v.KieuCV.Contains(f["JobType"]) select v;
                else kq = from v in kq
                          where v.KieuCV.Contains(f["JobType"].Split(',')[0])
                           || v.KieuCV.Contains(f["JobType"].Split(',')[1])
                          select v;

            }
            if (f["Location"] != "all")
                kq = from v in kq where v.CONGTY.MaKV == int.Parse(f["Location"]) select v;
            if(f["ex-start"] != null && f["ex-end"] != null)
            {
                kq = from v in kq where (v.KinhNghiem >= int.Parse(f["ex-start"]) && v.KinhNghiem <= int.Parse(f["ex-end"])) select v;
            }

            if (f["salary-start"] != null && f["salary-end"] != null)
            {
                kq = from v in kq where (v.LuongToiThieu >= int.Parse(f["salary-start"]) && v.LuongToiDa <= int.Parse(f["salary-end"])) select v;
            }


            return View(kq.ToPagedList(iPageNum, iSize));
        }


        [HttpGet]
        public ActionResult ApplyCV(int id)
        {

            if (Session["TaiKhoan"] == null)
            {
                return RedirectToAction("DangNhap", "User", new { JobsDetail = id });
            }

            var Congty = from ct in db.CONGTies
                       where ct.MaCTy == id
                       select ct;

            ViewBag.cV = id;
            ViewBag.Kh = (KHACHHANG)Session["Taikhoan"];

            return View(Congty.SingleOrDefault());
        }

        [HttpPost]
        public ActionResult ApplyCV(Mail model,DonUngTuyen don, HttpPostedFileBase attachment)
        {
            var tin = db.TinTimViecs.SingleOrDefault(n => n.MaTin == int.Parse(Request["id-jobs"]));
            var mail = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("websitedoban@gmail.com", "22023794A"),
                EnableSsl = true
            };

            var message = new MailMessage();
            message.From = new MailAddress("websitedoban@gmail.com");
            message.To.Add(new MailAddress(Request["company-email"]));
            message.Subject = "Mail Apply the job for "+ tin.ViTriLamViec;
            message.Body = "This is Email of " + tin.ViTriLamViec + " post in " + tin.NgayDangTin
                + "\nName :" + Request["name"] + "\n"
                + " Email :" + Request["email"] + "\n"
                + " Phone :" + Request["phone"] + "\n"
                + " Introduction : " + model.Introduction;

            if (attachment != null)
            {
                var sFileName = Path.GetFileName(attachment.FileName);
                var path = Path.Combine(Server.MapPath("~/UploadFile"), sFileName);
                if (!System.IO.File.Exists(path))
                {
                    attachment.SaveAs(path);
                }
                Attachment data = new Attachment(Server.MapPath("~/UploadFile/" + sFileName), MediaTypeNames.Application.Octet);
                message.Attachments.Add(data);
            }
            don.MaKH = int.Parse(Request["id-kh"]) ;
            don.MaTin = int.Parse(Request["id-jobs"]);
            don.TrangThai = 1;
            db.DonUngTuyens.InsertOnSubmit(don);
            db.SubmitChanges();
            mail.Send(message);
            return RedirectToAction("Index", "Jobs");
        }

        public ActionResult ListCompany(int? page, string strSearch = "")
        {
            int iSize = 10;
            int iPageNum = (page ?? 1);
            var listCty = from cty in db.CONGTies select cty;
            strSearch = strSearch.Trim();
            if (strSearch != null || strSearch != "")
            {
                listCty = from cty in listCty
                          where (cty.TenCty.Contains(strSearch)
                              || cty.LinhVuc.Contains(strSearch)
                                || cty.KhuVuc.TenKhuVuc.Contains(strSearch)
                                || cty.Website.Contains(strSearch)
                              )
                          select cty;
            }
            ViewBag.Category = db.CONGTies.GroupBy(cty => cty.LinhVuc).Select(g => g.Key);
            ViewBag.Location = db.CONGTies.GroupBy(cty => cty.KhuVuc).Select(g => g.Key);
            ViewBag.Search = strSearch;
            return View(listCty.ToPagedList(iPageNum, iSize));

        }

        public ActionResult DetailCompany(int id)
        {
            var Cty = db.CONGTies.Where(n => n.MaCTy == id).SingleOrDefault();
            ViewBag.Posts = db.TinTimViecs.Where(n => n.MaCty == id).ToList();
            ViewBag.Apply = db.DonUngTuyens.Where(n => n.TinTimViec.CONGTY.MaCTy == id);
            return View(Cty);
        }
    }
}
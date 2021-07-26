using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FindJob.Models;
using System.Net.Mail;
using PagedList;
using System.Net;

namespace FindJob.Areas.Admin.Controllers
{
    public class AdminController : BaseAdminController
    {
        FindJobDataContext db = new FindJobDataContext();
        // GET: Admin/Admin



        public ActionResult Index()
        {
            ViewBag.ThongBao = (RouteData.Values["action"]).ToString().ToLower();
            ViewBag.Customer = db.KHACHHANGs.Count();
            ViewBag.CustomerNotActive = db.KHACHHANGs.Where(n=> n.TrangThai == 0).Count();
            ViewBag.Company = db.CONGTies.Count();
            ViewBag.Post = db.TinTimViecs.Count();
            ViewBag.Location = db.KhuVucs.Count();
            ViewBag.Career = db.NganhNghes.Count();
            var post = db.TinTimViecs.OrderByDescending(n => n.LuongToiDa).Take(4);
            return View(post);
        }
        public ActionResult User(int? page, string search)
        {
            var listUser = from v in db.KHACHHANGs select v;
            if (!string.IsNullOrEmpty(search))
            {
                listUser = from user in listUser where 
                           user.HoTen.Contains(search) 
                           || user.TaiKhoan.Contains(search)
                           || user.Email.Contains(search)
                           select user;
            }
            int iPageNum = (page ?? 1);
            int iPageSize = 7;
            ViewBag.ThongBao = (RouteData.Values["action"]).ToString().ToLower();
            return View(listUser.ToPagedList(iPageNum, iPageSize));
        }

        [HttpGet]
        public ActionResult UserDetails(int id)
        {
            var User = db.KHACHHANGs.Where(n=>n.MaKH == id).SingleOrDefault();
            return View(User);
        }

        [HttpPost]
        public ActionResult UserDetails(int update,int id)
        {
            var User = db.KHACHHANGs.Where(n => n.MaKH == id).SingleOrDefault();
            if(User!= null)
            {
                User.TrangThai = update;
                db.SubmitChanges();
               
            }
            return RedirectToAction("UserDetails");
        }

        public ActionResult LeftMenu()
        {
            return PartialView();
        }

        public ActionResult Companys(int?page, string search)
        {

            var listCty = from v in db.CONGTies select v;
            if (!string.IsNullOrEmpty(search))
            {
                listCty = from cty in listCty
                          where
                             cty.TenCty.Contains(search)
                             || cty.TaiKhoan.Contains(search)
                             || cty.Email.Contains(search)
                             || cty.DiaChi.Contains(search)
                          select cty;
            }
            int iPageNum = (page ?? 1);
            int iPageSize = 7;
            ViewBag.ThongBao = (RouteData.Values["action"]).ToString().ToLower();
            return View(listCty.ToPagedList(iPageNum, iPageSize));
        }

        [HttpGet]
        public ActionResult CompanyDetails(int id)
        {
            var cty = db.CONGTies.Where(n=>n.MaCTy == id).SingleOrDefault();
            return View(cty);
        }

        [HttpPost]
        public ActionResult CompanyDetails(int id,int update)
        {
            var cty = db.CONGTies.Where(n => n.MaCTy == id).SingleOrDefault();
            if (cty != null)
            {
                cty.TrangThai = update;
                db.SubmitChanges();

            }
            return RedirectToAction("CompanyDetails");
        }

        public ActionResult Post(int ?page, string search)
        {
            var listPost = from v in db.TinTimViecs select v;
            if (!string.IsNullOrEmpty(search))
            {
                listPost = from post in listPost
                           where
                             post.CONGTY.TenCty.Contains(search)
                             || post.KieuCV.Contains(search)
                             || post.ViTriLamViec.Contains(search)
                             || post.NganhNghe.TenNganh.Contains(search)
                          select post;
            }

            int iPageNum = (page ?? 1);
            int iPageSize = 7;
            ViewBag.ThongBao = (RouteData.Values["action"]).ToString().ToLower();
            return View(listPost.ToPagedList(iPageNum, iPageSize));
        }


        [HttpGet]
        public ActionResult PostDetails(int id)
        {
            var Post = db.TinTimViecs.Where(n => n.MaTin == id).SingleOrDefault();
            return View(Post);
        }


        [HttpPost]
        public ActionResult PostDetails(int id,int update)
        {
            var post = db.TinTimViecs.Where(n => n.MaTin == id).SingleOrDefault();
            if (post != null)
            {
                post.TrangThai = update;
                db.SubmitChanges();
            }
            return RedirectToAction("PostDetails");
        }


        [HttpGet]
        public ActionResult FeedBackPost(int id)
        {
            var Post = db.TinTimViecs.Where(n => n.MaTin == id).SingleOrDefault();
            ViewBag.Email = Post.CONGTY.Email;
            ViewBag.Post = Post.MaTin;
            return View();
        }


        [HttpPost]
        public ActionResult FeedBackPost(FormCollection f)
        {
            var post = db.TinTimViecs.Where(n => n.MaTin == int.Parse(f["id"])).SingleOrDefault();
            if(post != null)
            {
                var mail = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("websitedoban@gmail.com", "22023794A"),
                    EnableSsl = true
                };

                var message = new MailMessage();
                message.From = new MailAddress("websitedoban@gmail.com");
                message.To.Add(new MailAddress(f["Email"]));
                message.Subject = "feedback email about " + post.ViTriLamViec + " is post in "+post.NgayDangTin;
                message.Body = f["message"];
                mail.Send(message);
                return RedirectToAction("PostDetails",new { id = post.MaTin});
            }

            return View();
        }
        


        [HttpGet]
        public ActionResult PostDelete(int id)
        {
            var Post = db.TinTimViecs.Where(n=> n.MaTin == id).SingleOrDefault();
            return View(Post);
        }

        [HttpPost]
        public ActionResult PostDelete(FormCollection f)
        {

            var Post = db.TinTimViecs.SingleOrDefault(n => n.MaTin == int.Parse(f["id"]));
            if (Post == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            var donUT = db.DonUngTuyens.Where(don => don.MaTin == Post.MaTin);
            if (donUT.Count() > 0)
            {
                db.DonUngTuyens.DeleteAllOnSubmit(donUT);
                db.SubmitChanges();
            }
            db.TinTimViecs.DeleteOnSubmit(Post);
            db.SubmitChanges();
            return RedirectToAction("Post");
        }


        public ActionResult Career(int? page, string search)
        {
            var listCareer = from v in db.NganhNghes select v;
            if (!string.IsNullOrEmpty(search))
            {
                listCareer = from career in listCareer
                             where
                             career.TenNganh.Contains(search)
                             select career;
            }
            int iPageNum = (page ?? 1);
            int iPageSize = 7;
            ViewBag.ThongBao = (RouteData.Values["action"]).ToString().ToLower();
            return View(listCareer.ToPagedList(iPageNum, iPageSize));
        }

        [HttpGet]
        public ActionResult CreateCareer()
        {
            return View();
        }


        [HttpPost]
        public ActionResult CreateCareer(FormCollection f)
        {
            var sTenNganh = f["TenNganh"];
            if (db.NganhNghes.SingleOrDefault(n => n.TenNganh == sTenNganh) != null)
            {
                ViewBag.ThongBao = "Tên chuyên ngành đã tồn tại";
            }
            else if (string.IsNullOrEmpty(sTenNganh))
            {
                ViewBag.ThongBao = "Phải điền tên Ngành nghề";
            }
            else
            {
                NganhNghe newNganh = new NganhNghe();
                newNganh.TenNganh = sTenNganh;
                db.NganhNghes.InsertOnSubmit(newNganh);
                db.SubmitChanges();
                return RedirectToAction("Career");
            }
            return View();
        }


        [HttpGet]
        public ActionResult DeleteCareer(int id)
        {
            var career = db.NganhNghes.Where(n => n.MaNganh == id).SingleOrDefault();
            return View(career);
        }


        [HttpPost]
        public ActionResult DeleteCareer(FormCollection f)
        {
            var career = db.NganhNghes.SingleOrDefault(n => n.MaNganh == int.Parse(f["id"]));
            if (career == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            var post = db.TinTimViecs.Where(p => p.MaNganh == career.MaNganh);
            var kh = db.KHACHHANGs.Where(k => k.MaNganh == career.MaNganh);
            if (post.Count() > 0 || kh.Count() > 0)
            {
                if(post.Count() > 0)
                    ViewBag.ThongBao = "Nghề này đang có trong bảng Tin tìm việc. Nếu muốn xóa thì phải xóa hết mã Ngành này trong bảng Tin Tìm việc.";
                if (kh.Count() > 0)
                    ViewBag.ThongBao = "Nghề này đang có trong bảng Khách hàng. Nếu muốn xóa thì phải xóa hết mã Ngành này trong bảng Khách hàng.";
                return View(career);

            }

            db.NganhNghes.DeleteOnSubmit(career);
            db.SubmitChanges();
            return RedirectToAction("Career");
        }

        [HttpGet]
        public ActionResult EditCareer(int id)
        {
            var career = db.NganhNghes.Where(n => n.MaNganh == id).SingleOrDefault();
            return View(career);
        }


        [HttpPost]
        public ActionResult EditCareer(FormCollection f)
        {
            var career = db.NganhNghes.SingleOrDefault(n => n.MaNganh == int.Parse(f["id"]));
            if (career == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            career.TenNganh = f["TenNganh"];            
            db.SubmitChanges();
            return RedirectToAction("Career");
        }

        public ActionResult Location(int? page, string search)
        {
            var listLocation = from v in db.KhuVucs select v;
            if (!string.IsNullOrEmpty(search))
            {
                listLocation = from loca in listLocation
                               where
                             loca.TenKhuVuc.Contains(search)
                             select loca;
            }
            int iPageNum = (page ?? 1);
            int iPageSize = 7;
            ViewBag.ThongBao = (RouteData.Values["action"]).ToString().ToLower();
            return View(listLocation.ToPagedList(iPageNum, iPageSize));
        }

        [HttpGet]
        public ActionResult CreateLocation()
        {
            return View();
        }


        [HttpPost]
        public ActionResult CreateLocation(FormCollection f)
        {
            var sTenKhuVuc = f["TenKhuVuc"];
            if (db.KhuVucs.SingleOrDefault(n => n.TenKhuVuc == sTenKhuVuc) != null)
            {
                ViewBag.ThongBao = "Tên Khu Vực đã tồn tại";
            }
            else if (string.IsNullOrEmpty(sTenKhuVuc))
            {
                ViewBag.ThongBao = "Phải điền tên  Khu Vực";
            }
            else
            {
                KhuVuc newKV = new KhuVuc();
                newKV.TenKhuVuc = sTenKhuVuc;
                db.KhuVucs.InsertOnSubmit(newKV);
                db.SubmitChanges();
                return RedirectToAction("Location");
            }
            return View();
        }


        [HttpGet]
        public ActionResult DeleteLocation(int id)
        {
            var location = db.KhuVucs.Where(n => n.MaKV == id).SingleOrDefault();
            return View(location);
        }


        [HttpPost]
        public ActionResult DeleteLocation(FormCollection f)
        {
            var location = db.KhuVucs.SingleOrDefault(n => n.MaKV == int.Parse(f["id"]));
            if (location == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            var cty = db.CONGTies.Where(p => p.MaKV == location.MaKV);
            if (cty.Count() > 0)
            {
                ViewBag.ThongBao = "Khu Vực này đang có trong bảng Công ty. Nếu muốn xóa thì phải xóa hết mã Khu Vực này trong bảng Công ty.";
                return View(location);

            }

            db.KhuVucs.DeleteOnSubmit(location);
            db.SubmitChanges();
            return RedirectToAction("Location");
        }

        [HttpGet]
        public ActionResult EditLocation(int id)
        {
            var location = db.KhuVucs.Where(n => n.MaKV == id).SingleOrDefault();
            return View(location);
        }


        [HttpPost]
        public ActionResult EditLocation(FormCollection f)
        {
            var location = db.KhuVucs.SingleOrDefault(n => n.MaKV == int.Parse(f["id"]));
            if (location == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            location.TenKhuVuc = f["TenKV"];
            db.SubmitChanges();
            return RedirectToAction("Location");
        }

    }
}
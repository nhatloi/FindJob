using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FindJob.Models;
using System.Web.Mvc;

namespace FindJob.Controllers
{
    public class HomeController : Controller
    {
        FindJobDataContext db = new FindJobDataContext();

        private List<TinTimViec> Laytin(int count)
        {
            return db.TinTimViecs.Where(n => n.TrangThai == 1).OrderByDescending(a => a.NgayDangTin).Take(count).ToList();
        }

        public ActionResult Index()
        {
            ViewBag.KhuVuc = from s in db.KhuVucs select s;
            ViewBag.Job = Laytin(4);
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

    }
}
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using FindJob.Models;
using System.Net.Mail;
using System.Net;
using System.Net.Mime;
using FindJob.Models;


namespace SachOnline.Controllers
{
    public class FileAndMailController : Controller
    {
        // GET: FileAndEmail
        FindJobDataContext db = new FindJobDataContext();
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult SendMail()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SendMail(Mail model)
        {

            var mail = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("websitedoban@gmail.com", "22023794A"),
                EnableSsl = true
            };

            var message = new MailMessage();
            message.From = new MailAddress("websitedoban@gmail.com");
            message.To.Add(new MailAddress(Request["company-email"]));
            message.Subject = "Mail Apply the job";
            message.Body = "Name :" + Request["name"] + "\n"
                + " Email :" + Request["email"] + "\n"
                + " Phone :" + Request["phone"] + "\n"
                + " Introduction : " + model.Introduction;

            if(Request.Files["attachment"] != null)
            {
                var f = Request.Files["attachment"];
                var path = Path.Combine(Server.MapPath("~/UploadFile"), f.FileName);
                if (!System.IO.File.Exists(path))
                {
                    f.SaveAs(path);

                }
                Attachment data = new Attachment(Server.MapPath("~/UploadFile/" + f.FileName), MediaTypeNames.Application.Octet);
                message.Attachments.Add(data);
            }
            mail.Send(message);
            return RedirectToAction("Index","Jobs");
        }


        [HttpGet]
        public ActionResult SendCodePW(string mail,string user)
        {
            var kh = db.KHACHHANGs.Where(n => n.Email == mail && n.TaiKhoan == user);
            if(kh != null)
            {
                int length = 7;
                // creating a StringBuilder object()
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
                var mailClient = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("websitedoban@gmail.com", "22023794A"),
                    EnableSsl = true
                };

                var message = new MailMessage();
                message.From = new MailAddress("websitedoban@gmail.com");
                message.To.Add(new MailAddress(mail));
                message.Subject = "Reset Password";
                message.Body = "Code: " + str_build;
                mailClient.Send(message);
            }
            return View();
        }
    }
}
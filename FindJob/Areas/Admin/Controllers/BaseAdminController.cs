using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace FindJob.Areas.Admin.Controllers
{
    public class BaseAdminController : Controller
    {
        // GET: Admin/BaseAdmin
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = Session["Admin"];
            if (session == null)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Account", action = "Login", Area = "Admin" }));
            }
            base.OnActionExecuting(filterContext);

        }
    }
}
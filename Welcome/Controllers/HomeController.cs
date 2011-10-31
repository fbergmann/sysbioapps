using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Mvc;

namespace Welcome.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";

            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Software()
        {
            return View();
        }

        public ActionResult Services()
        {
            return View();
        }

        public string Upload(HttpPostedFileBase fileData)
        {
            var fileName = this.Server.MapPath("~/Uploads/" + System.IO.Path.GetFileName(fileData.FileName));
            fileData.SaveAs(fileName);
            SBML = System.IO.File.ReadAllText(fileName);
            
            return "ok";
        }

        public string SBML
        {
            get { return (string)Session["sbml"]; }
            set
            {
                Session["sbml"] = value;
            }
        }
    }
}

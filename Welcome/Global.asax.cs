using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using SBMLExtension;
using SBMLExtension.LayoutExtension;

namespace Welcome
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            Util.SVG_FileDirectory = @"C:\Program Files (x86)\KGI\SBW\Layout";
            SBML2TikZ.Converter.LatexFileName = @"c:\pdflatex.bat";//@"c:\Program Files (x86)\MiKTeX 2.8\miktex\bin\pdflatex.exe";
            //Welcome.Controllers.LayoutController.LatexFileName = @"c:\Program Files (x86)\MiKTeX 2.8\miktex\bin\pdflatex.exe";
            Welcome.Controllers.LayoutController.LatexFileName = @"c:\pdflatex.bat";

        }

        protected void Session_Start()
        {
            //var session = HttpContext.Current.Session;
            //Debug.WriteLine("Adding Session: " + session.SessionID);
            //session["sbml"] = "";
            //session["layout"] = new Layout();

            //var sessions = (Dictionary<string, HttpSessionState>)Application.Get("sessions");
            //if (sessions == null)
            //    sessions = new Dictionary<string, HttpSessionState>();
            //sessions[session.SessionID] = session;
            //Application.Set("sessions", sessions);
        }

        protected void Session_End()
        {
            //var sessions = (Dictionary<string, HttpSessionState>)Application.Get("sessions");
            //if (sessions == null)
            //    sessions = new Dictionary<string, HttpSessionState>();
            //string sessionId = HttpContext.Current.Session.SessionID;
            //Debug.WriteLine("Remove Session: " + sessionId);
            //sessions.Remove(sessionId);
            //Application.Set("sessions", sessions);
        }



    }
}
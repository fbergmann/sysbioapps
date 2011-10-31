using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using SBML2TikZ;
using SBMLExtension;
using SBMLExtension.LayoutExtension;

namespace Welcome.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.Required)]
    public class LayoutController : Controller
    {

        public class ViewDataUploadFilesResult
        {
            public string thumbnail_url { get; set; }
            public string name { get; set; }
            public int length { get; set; }
            public string type { get; set; }
            public string url { get; set; }
        }

        public Layout CurrentLayout 
        {
            get
            {                
                if (Session["layout"] == null)
                {
                    Session["layout"] = new Layout();
                }
                return (Layout)Session["layout"];
            }
            set
            {
                Session["layout"] = value;
            }
        }

        public string SBML
        {
            get 
            {
                return (string)Session["sbml"]; 
            }
            set
            {
                Session["sbml"] = value;
            }
        }

        public JsonResult JQueryUpload()
        {
            var r = new List<ViewDataUploadFilesResult>();            
             try
             {
                 for (int i = 0; i < Request.Files.Count; i++)
                 {
                     var hpf = Request.Files[i];
                     if (hpf == null || hpf.ContentLength == 0) continue;
                     string FileName = hpf.FileName;
                     var savedFileName = this.Server.MapPath("~/Uploads/" + Path.GetFileName(FileName));
                     hpf.SaveAs(savedFileName);

                     if (Path.GetExtension(savedFileName).ToLowerInvariant().EndsWith("xml"))
                         SBML = System.IO.File.ReadAllText(savedFileName);

                     r.Add(new ViewDataUploadFilesResult
                               {
                                   thumbnail_url = "/Layout/PreviewImage?file=" + Path.GetFileName(FileName),
                                   url = "javascript:loadFile('" + Path.GetFileName(FileName) + "')",
                                   name = FileName,
                                   length = hpf.ContentLength,
                                   type = hpf.ContentType
                               });
                 }
             }
             catch (Exception ex)
             {
                 Debug.WriteLine(ex.Message);
             }
             Response.ContentType = "text/plain";
             return Json(r.ToArray(),"text/html", JsonRequestBehavior.AllowGet);

        }


        public FileResult PreviewImage(string file)
        {
            string fileName = Server.MapPath("~/Uploads/" + file);
            if (!System.IO.File.Exists(fileName)) return null;
            var layout = Util.readLayoutFromFile(fileName);
            using (var image = layout.ToImage(120, 60))
            {
                return ToFileResult(image, Path.GetFileNameWithoutExtension(file) + "_min.png");
            }
        }

        //
        // GET: /Layout/

        public ActionResult Index()
        {
            return View();
        }

        private static string GenerateLayout(string sbml, double gravity = 15, double stiffness = 30, bool magnetism = false, bool boundary = false, bool grid = false, string type = "many")
        {
            var network = new AutoLayout.DrawNetwork();
            network.loadSBML(sbml);
            sbml = network.generateLayout(
                    gravity,
                    stiffness,
                    magnetism,
                    boundary,
                    grid,
                    type == "sourcesink",
                    type == "one",
                    type == "many"
                    );
            return sbml;
        }
        public ContentResult DoLayout(bool sbgn = false, double gravity = 15, double stiffness = 30, bool magnetism = false, bool boundary = false, bool grid = false, string type = "many")
        {
            string sbml = Util.writeLayout(CurrentLayout);
            CurrentLayout = Util.readLayout(GenerateLayout(sbml, gravity, stiffness, magnetism, boundary, grid, type), sbgn);
            return Content(String.Format("<a target='_blank' style='border: none;' href='/Layout/ImageWithScale?scale=1&{0}'><img src='/Layout/ImageWithDimensions?height=300&width=500&{0}' alt='Image' /></a>", DateTime.Now.Ticks));
        }

        private string GetCurrentSBML(string file = "")
        {
            string sbml;

            string fileName = Server.MapPath("~/Uploads/" + file);
            if (string.IsNullOrEmpty(file) || !System.IO.File.Exists(fileName))
            {
                sbml = SBML;
            }
            else
            {
                sbml = System.IO.File.ReadAllText(fileName);
            }
            return sbml;
        }

        public ContentResult UpdateImage(string file = "", bool sbgn = false)
        {
                
            string sbml = GetCurrentSBML(file);
            CurrentLayout = Util.readLayout(sbml, sbgn);

            if (!CurrentLayout.hasLayout())
                CurrentLayout = Util.readLayout(GenerateLayout(sbml), sbgn);

            return Content(String.Format("<a target='_blank' style='border: none;' href='/Layout/ImageWithScale?scale=1&{0}'><img src='/Layout/ImageWithDimensions?height=300&width=500&{0}' alt='Image' /></a>", DateTime.Now.Ticks));
        }

        private static FileResult ToFileResult(Image image, string fileName = "layout.png")
        {
            if (image == null) return null;
            using (var stream = new MemoryStream())
            {
                image.Save(stream, ImageFormat.Png);
                return new FileContentResult(stream.GetBuffer(), "image/png") { FileDownloadName = fileName };
            }
        }

        public FileResult ImageWithScale(float scale = 1)
        {
            return ToFileResult(CurrentLayout.ToImage(scale));
        }

        public FileResult ImageWithDimensions(double width, double height)
        {            
            return ToFileResult(CurrentLayout.ToImage(width, height));
        }

        public FileResult SVG()
        {
            using (var stream =new MemoryStream())
            {
                var writer = new StreamWriter(stream);
                writer.WriteLine(Util.ToSVG(CurrentLayout));
                return new FileContentResult(stream.GetBuffer(), "image/svg+xml") { FileDownloadName = "layout.svg" };
            }
        }

        public FileResult TikZ()
        {
            var converter = new Converter {layout = CurrentLayout, specs = new RenderSpecs(CurrentLayout)};
            var tex = converter.ToTex(CurrentLayout);

            return new FileContentResult(System.Text.Encoding.UTF8.GetBytes(tex), "application/x-tex")
            {
                 FileDownloadName="layout.tex"
            };
        }

        public FileResult PDF()
        {
            var converter = new Converter { layout = CurrentLayout };

            var pdf = converter.ToPDF();

            return new FileContentResult(pdf, "application/pdf")
            {
                FileDownloadName = "layout.pdf"
            };
        }
    }
}

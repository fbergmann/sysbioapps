using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public Collection<Layout> AllLayouts 
        {
            get
            {
                if (Session["layouts"] == null)
                {
                    Session["layouts"] = new Collection<Layout>();
                }
                return (Collection<Layout>)Session["layouts"];
            }
            set
            {
                Session["layouts"] = value;
            }
        }


        public string SelectedLayoutId
        {
            get
            {
                if (Session["selected"] == null)
                {
                    Session["selected"] = "";
                }
                return (string)Session["selected"];
            }
            set
            {
                Session["selected"] = value;
            }
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
                    string fileName = hpf.FileName;
                    var savedFileName = this.Server.MapPath("~/Uploads/" + Path.GetFileName(fileName));
                    hpf.SaveAs(savedFileName);

                    string lowerCasefileName = Path.GetExtension(savedFileName).ToLowerInvariant();
                    if (lowerCasefileName.EndsWith("xml") || lowerCasefileName.EndsWith("sbml") || lowerCasefileName.EndsWith("sbgn"))
                    {
                        SBML = System.IO.File.ReadAllText(savedFileName);                        
                    }

                    r.Add(new ViewDataUploadFilesResult
                              {
                                  thumbnail_url = String.Format("/Layout/PreviewImage?file={0}", Path.GetFileName(fileName)),
                                  url = String.Format("javascript:loadFile('{0}')", Path.GetFileName(fileName)),
                                  name = fileName,
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
            return Json(r.ToArray(), "text/html", JsonRequestBehavior.AllowGet);

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

        public ActionResult Index(string selected = null)
        {
            var result = new List<string>();
            foreach (Layout layout in AllLayouts )
            {
            	result.Add(layout.ID);
            }
            if (selected == null && result.Count > 0)
                selected = result[0];

            SelectedLayoutId = selected;
            ViewBag.selected = new SelectList(result, selected);
            ViewBag.NumLayouts = result.Count;
            return View();
        }

        public Layout SelectedLayout
        {
            get
            {
                var selected = SelectedLayoutId;
                var all = AllLayouts;
                foreach (var item in all)
                {
                    if (item.ID == selected)
                        return item;
                }
                if (all.Count > 0)
                return all[0];
                return new Layout();
                
            }
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
            AllLayouts = Util.Layouts;
            SelectedLayoutId = CurrentLayout.ID;
            return Content(String.Format("<a target='_blank' style='border: none;' href='/Layout/ImageWithScale?scale=1&{0}'><img src='/Layout/ImageWithDimensions?height=300&width=500&{0}' alt='Image' /></a>", DateTime.Now.Ticks));
        }

        private string GetCurrentSBML(string file = "")
        {
            string sbml;

            if (string.IsNullOrEmpty(file) )
            {
                sbml = SBML;
            }
            else
            {
                string fileName = Server.MapPath("~/Uploads/" + Path.GetFileName(file));
                if (!System.IO.File.Exists(fileName))
                {
                    sbml = SBML;
                }
                else
                {
                    sbml = System.IO.File.ReadAllText(fileName);
                }
            }
            return sbml;
        }

        public ContentResult UpdateImage(string file = "", bool sbgn = false)
        {
                
            string sbml = GetCurrentSBML(file);
            CurrentLayout = Util.readLayout(sbml, sbgn);
            AllLayouts = Util.Layouts;

            if (!CurrentLayout.hasLayout())
            {
                CurrentLayout = Util.readLayout(GenerateLayout(sbml), sbgn);
                AllLayouts = Util.Layouts;
                SelectedLayoutId = CurrentLayout.ID;
            }

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
            return ToFileResult(SelectedLayout.ToImage(scale));
        }

        public FileResult ImageWithDimensions(double width, double height)
        {
            return ToFileResult(SelectedLayout.ToImage(width, height));
        }

        public FileResult SVG()
        {
            string svgContent = Util.ToSVG(SelectedLayout);
            return new FileContentResult(System.Text.Encoding.UTF8.GetBytes(svgContent) , "image/svg+xml") { FileDownloadName = "layout.svg" };
        }

        public FileResult GetSBML()
        {
            string svgContent = Util.writeLayout(SelectedLayout);
            if (string.IsNullOrWhiteSpace(svgContent))
                svgContent = "";
            return new FileContentResult(System.Text.Encoding.UTF8.GetBytes(svgContent), "application/sbml+xml") { FileDownloadName = "layout.xml" };
        }

        public FileResult TikZ()
        {
            var converter = new Converter {Layout = CurrentLayout, specs = new RenderSpecs(CurrentLayout)};
            var tex = converter.ToTex(SelectedLayout);

            return new FileContentResult(System.Text.Encoding.UTF8.GetBytes(tex), "application/x-tex")
            {
                 FileDownloadName="layout.tex"
            };
        }


        private static string _LatexFileName = "pdflatex";
        //private static string _LatexFileName =@"c:\pdflatex.bat";

        /// <summary>
        /// Gets / Sets the filename of the pdf latex compiler.
        /// </summary>
        public static string LatexFileName
        {
            get
            {
                return _LatexFileName;
            }
            set
            {
                _LatexFileName = value;
            }
        }

        private static string _LatexArguments = " -disable-installer -interaction nonstopmode";
        /// <summary>
        /// Gets / Sets the default arguments used for the compilation mode. 
        /// By default the converter will invoke the LatexFileName with the 
        /// tex  file followed by  -interaction nonstopmode
        /// </summary>
        public static string LatexArguments
        {
            get
            {
                return _LatexArguments;
            }
            set
            {
                _LatexArguments = value;
            }
        }

        public static string LastMessage { get; set; }

        public static void compiletoPDF(out Boolean compiled, string texfilename)
        {
            compiled = true;
            try
            {
                string oldDir = Directory.GetCurrentDirectory();
                ProcessStartInfo pdfLaTeXinfo = new ProcessStartInfo();
                pdfLaTeXinfo.CreateNoWindow = true;
                pdfLaTeXinfo.UseShellExecute = false;
                pdfLaTeXinfo.RedirectStandardOutput = true;
                pdfLaTeXinfo.RedirectStandardError = true;
                Directory.SetCurrentDirectory(Path.GetDirectoryName(texfilename));

                pdfLaTeXinfo.Arguments = Path.GetFileName(texfilename) + " " + LatexArguments;
                pdfLaTeXinfo.FileName = LatexFileName;

                Process p = Process.Start(pdfLaTeXinfo);

                LastMessage = p.StandardOutput.ReadToEnd();
                LastMessage += p.StandardError.ReadToEnd();
                p.WaitForExit();
                p.Close();
                Directory.SetCurrentDirectory(oldDir);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                compiled = false;
            }
        }


        private static byte[] CompileTikZToPDF(string TikZstrings)
        {
            //Create a temp directory to generate the pdf and tex files
            string tempDir = (Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
            Directory.CreateDirectory(tempDir);

            string tempFileName = Path.GetRandomFileName();
            string TeXfilename = Path.Combine(tempDir, Path.GetFileNameWithoutExtension(tempFileName) + ".tex");
            string PDFfilename = Path.Combine(tempDir, Path.GetFileNameWithoutExtension(tempFileName) + ".pdf");
            //string TeXfilename = tempDir + "\\" + Path.GetFileNameWithoutExtension(tempFileName) + ".tex";
            //string PDFfilename = tempDir + "\\" + Path.GetFileNameWithoutExtension(tempFileName) + ".pdf";

            // write TikZstrings into TeXfilename
            using (StreamWriter writer = new StreamWriter(TeXfilename))
            {
                writer.WriteLine(TikZstrings);
            }

            //Now convert the TeX file to PDF
            Boolean compiled;
            compiletoPDF(out compiled, TeXfilename);

            //if the compilation was successful, we convert the PDF to a byte buffer
            if (compiled)
            {
                try
                {
                    byte[] PDFdata = System.IO.File.ReadAllBytes(PDFfilename);
                    //delete the tempDir
                    try
                    {
                        Directory.Delete(tempDir, true);
                    }
                    catch (Exception )
                    {
                        
                    }
                    return PDFdata;
                    
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                    try
                    {
                        Directory.Delete(tempDir, true);
                    }
                    catch (Exception )
                    {

                    }
                    return new byte[] { };
                }
            }
            try
            {
                Directory.Delete(tempDir, true);
            }
            catch (Exception )
            {

            }
            return new byte[] { }; //return an empty array
        }

        public FileResult PDF()
        {
            var converter = new Converter { Layout = CurrentLayout };
            Converter.LastCompileException = null;
            var tex = converter.ToTex(CurrentLayout);
            //var pdf  = CompileTikZToPDF(tex);
            //if (pdf == null || pdf.Length == 0)
            //{
            //    return new FileContentResult(System.Text.Encoding.UTF8.GetBytes(LastMessage), "text/plain");
            //}
            var pdf = converter.ToPDF();
            if (Converter.LastCompileException != null)
            {
                var errorMessage = Converter.LastCompileException.Message;
                errorMessage += Environment.NewLine + Environment.NewLine + Converter.LastCompileException.StackTrace;
                return new FileContentResult(System.Text.Encoding.UTF8.GetBytes(errorMessage), "text/plain");
            }
            return new FileContentResult(pdf, "application/pdf")
            {
                FileDownloadName = "layout.pdf"
            };
        }
    }
}

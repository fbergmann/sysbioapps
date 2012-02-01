using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Welcome.Models;

namespace Welcome.Controllers
{
    

    public class TranslatorController : Controller
    {
        public static string TempDir = @"c:\Temp";
        public static string ExecuteProgram(string program, 
            string sbml, 
            string preArgs = " -f ",
            string postArgs = "")
        {
            if (string.IsNullOrWhiteSpace(sbml))
                return "No Model loaded ... ";

            try
            {
                string fileName = Path.Combine(TempDir, Path.GetRandomFileName());
                System.IO.File.WriteAllText(fileName, sbml);
                var info = new ProcessStartInfo { 
                    RedirectStandardOutput = true, 
                    RedirectStandardError = true, 
                                                    CreateNoWindow = true, 
                                                    UseShellExecute = false, 
                                                    FileName = program, 
                                                    Arguments = preArgs + fileName + postArgs };
                string result; string error = "";
                using (var process = new Process() { StartInfo = info })
                {
                    process.Start();
                    result = process.StandardOutput.ReadToEnd();
                    error = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                    process.Close();
                }
                return result + "\n\n" + error;
            }
            catch (Exception)
            {
                return "Translation Failed!";
            }
        }

        private static System.Security.SecureString CreatePassword(string p)
        {
            var result = new System.Security.SecureString();
            var buffer = p.ToCharArray();
            for (int i = 0; i < p.Length; i++)
                result.AppendChar(buffer[i]);
            return result;
        }

        static string TranslatorDir =  @"C:\Program Files (x86)\KGI\SBW\bin\";

        static Translator[] Translators = new Translator[] { 
            new Translator { 
                Key="matlab", 
                Name="Matlab Translator", 
                Extension=".m", 
                MimeType="text/x-matlab", 
                Translate = (sbml) => ExecuteProgram(TranslatorDir+@"MatlabTranslator.exe", sbml)
            },
            new Translator { 
                Key="xpp", 
                Name="XPP Translator", 
                Extension=".ode", 
                MimeType="application/xpp", 
                Translate = (sbml) => ExecuteProgram(TranslatorDir+@"XPPTranslator.exe", sbml)
            },
            new Translator { 
                Key="jan", 
                Name="JarnacLite Translator", 
                Extension=".jan", 
                MimeType="application/jarnac", 
                Translate = (sbml) => ExecuteProgram(@"C:\Program Files (x86)\KGI\SBW\JarnacLite\JarnacLiteConsole.exe", sbml, "", "")                
            },
            new Translator { 
                Key="java", 
                Name="Java Translator", 
                Extension=".java", 
                MimeType="text/x-java-source", 
                Translate = (sbml) => ExecuteProgram(TranslatorDir+@"JavaTranslator.exe", sbml)
            },
            new Translator { 
                Key="mathematica", 
                Name="Mathematica Translator", 
                Extension=".nb", 
                MimeType="application/mathematica", 
                Translate = (sbml) => ExecuteProgram(TranslatorDir+@"MathematicaTranslator.exe", sbml)
            },
            new Translator { 
                Key="csharp", 
                Name="C# Translator", 
                Extension=".cs", 
                MimeType="text/x-csharp", 
                Translate = (sbml) => ExecuteProgram(TranslatorDir+@"CSharpTranslator.exe", sbml)
            },
            new Translator { 
                Key="cpp", 
                Name="C/C++ Translator", 
                Extension=".cpp", 
                MimeType="text/x-c", 
                Translate = (sbml) => ExecuteProgram(TranslatorDir+@"CTranslator.exe", sbml)
            },
            new Translator { 
                Key="java", 
                Name="Java Translator", 
                Extension=".java", 
                MimeType="text/x-java-source", 
                Translate = (sbml) => ExecuteProgram(TranslatorDir+@"JavaTranslator.exe", sbml)
            }
        }; 


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

        public string FileName
        {
            get
            {
                return (string)Session["fileName"];
            }
            set
            {
                Session["fileName"] = value;
            }
        }


        public string CurrentTranslator
        {
            get
            {
                if (string.IsNullOrWhiteSpace((string)Session["CurrentTranslator"]))
                    Session["CurrentTranslator"] = "matlab";
                return (string)Session["CurrentTranslator"];
            }
            set
            {
                Session["CurrentTranslator"] = value;
            }
        }

        
        //
        // GET: /Translator/

        public ActionResult Index()
        {

            var sbml = GetCurrentSBML(FileName);
            var trans = GetCurrentTranslator();
            sbml = trans.Translate(sbml);


            return View(new CurrentTranslation { 
                CurrentTranslator = CurrentTranslator, 
                Translators = Translators, 
                FileName = FileName, 
                Translation = trans.WrapResult(sbml)
            });
        }

        private void SetSBML(string rawSBML, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "upload.xml";
            var localName = Server.MapPath("~/Uploads/" + fileName);
            System.IO.File.WriteAllText(localName, rawSBML);
            SBML = rawSBML;
            FileName = fileName;
        }
        
        [ValidateInput(false)]
        public RedirectToRouteResult UploadSBML(string rawSBML, string fileName = "upload.xml")
        {
            SetSBML(rawSBML, fileName);

            return RedirectToAction("Index");
        }

        public RedirectToRouteResult UploadURL(string url)
        {
            var request = WebRequest.Create(url);
            var response = request.GetResponse(); ;
            string urlContent;
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                urlContent = reader.ReadToEnd();
            }
            var baseName = "upload.xml";
            string content = response.Headers["Content-Disposition"];
            if (!string.IsNullOrWhiteSpace(content))
                baseName = content.Substring(content.IndexOf('=')+1);

            SetSBML(urlContent, baseName);

            return RedirectToAction("Index");

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
                    FileName = hpf.FileName;
                    string fileName = Path.GetFileName(FileName);
                    string extension = Path.GetExtension(fileName);
                    var savedFileName = this.Server.MapPath("~/Uploads/" + fileName);
                    hpf.SaveAs(savedFileName);

                    if (extension.ToLowerInvariant().EndsWith("xml"))
                        SBML = System.IO.File.ReadAllText(savedFileName);

                    r.Add(new ViewDataUploadFilesResult
                    {
                        url = "javascript:loadFile('" + fileName + "')",
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
            return Json(r.ToArray(), "text/html", JsonRequestBehavior.AllowGet);

        }

        public ContentResult UpdateTranslation(string translator = "matlab", string file = "")
        {
            string sbml = GetCurrentSBML(file);
            var trans = GetCurrentTranslator(translator);
            CurrentTranslator = trans.Key;
            sbml = trans.Translate(sbml);
            //return Content(String.Format("<script type='syntaxhighlighter' class='brush:xml'>\n{0}\n</script>", CleanString(sbml)));
            return Content(trans.WrapResult(sbml));
        }

        public FileResult DownloadTranslation(string translator="matlab", string file = "")
        {
            var sbml = GetCurrentSBML(file);
            if (string.IsNullOrWhiteSpace(sbml)) return null;
            var trans = GetCurrentTranslator(translator);
            sbml = trans.Translate(sbml);

            return new FileContentResult(System.Text.Encoding.UTF8.GetBytes(sbml), trans.MimeType)
            {
                FileDownloadName = Path.GetFileNameWithoutExtension(FileName) + trans.Extension
            };
        }


        private string GetCurrentSBML(string file = "")
        {
            string sbml;

            if (string.IsNullOrEmpty(file))
            {
                sbml = SBML;
            }
            else
            {
                string fileName = Server.MapPath("~/Uploads/" + Path.GetFileName(file));
                if ( !System.IO.File.Exists(fileName))
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


        private static Translator GetCurrentTranslator(string key = "matlab")
        {
            foreach (var item in Translators)
            {
                if (item.Key == key)
                {               
                    return item;
                }
            }
            return Translators[0];
        }

    }
}

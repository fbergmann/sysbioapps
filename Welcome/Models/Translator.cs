using System;

namespace Welcome.Models
{
    public class Translator
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string MimeType { get; set; }
        public string Extension { get; set; }
        public Func<string, string> Translate { get; set; }
        
        public string WrapResult(string sbml)
        {
             return String.Format("<pre class='brush: {0}'>\n{1}\n</pre>", 
                 GetBrush(Extension), 
                 CleanString(sbml));
        }

        private static string GetBrush(string extension)
        {
            switch (extension.ToLowerInvariant())
            {
                case ".cs":
                    return "csharp";
                case ".cpp":
                    return "cpp";
                case ".java":
                    return "java";
                case ".m":
                    return "matlabkey";
                case ".ode":
                    return "xpp";
                case ".jan":
                    return "jan";
                default:
                    return "text";
            }
        }

        private static string CleanString(string content)
        {
            var result = content.Replace("<", "&lt;");
            result = result.Replace(">", "&gt;");
            result = result.Replace("\r\n", "\n");
            return result;
        }

    }
}
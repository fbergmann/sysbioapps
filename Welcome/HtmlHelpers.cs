using System;
using System.IO;
using System.Net;
using System.Web.Mvc;

namespace Welcome
{
    public static class HtmlHelpers
    {
        public static MvcHtmlString WebPage(this HtmlHelper htmlHelper, string url)
        {
            try
            {

                url = htmlHelper.ViewContext.HttpContext.ApplicationInstance.Server.MapPath(url);
                return MvcHtmlString.Create(File.ReadAllText(url));
            }
            catch (Exception ex)
            {
                return MvcHtmlString.Create(new WebClient().DownloadString(url));
            }
        }
    }
}
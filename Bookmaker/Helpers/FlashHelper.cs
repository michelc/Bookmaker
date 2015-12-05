using System.Web.Mvc;

namespace Bookmaker.Helpers
{
    public static class FlashHelper
    {
        public static void Flash(this Controller controller, object text, params object[] args)
        {
            controller.TempData["FlashKey"] = string.Format(text.ToString(), args);
        }

        public static MvcHtmlString Flash(this HtmlHelper helper)
        {
            var flash = (string)helper.ViewContext.TempData["FlashKey"];
            if (flash == null) return null;

            var css = "flash";
            if (flash.StartsWith("!"))
            {
                flash = flash.Substring(1);
                css = "flash_alert";
            }

            var tag = new TagBuilder("div");
            tag.AddCssClass(css);
            tag.InnerHtml = flash;

            return new MvcHtmlString(tag.ToString());
        }
    }
}

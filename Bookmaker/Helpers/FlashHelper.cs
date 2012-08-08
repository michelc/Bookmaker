using System.Web.Mvc;

namespace Bookmaker.Helpers
{
    public static class FlashHelper
    {
        public static void Flash(this Controller controler, string Value)
        {
            controler.TempData["FlashKey"] = Value;
        }

        public static MvcHtmlString Flash(this HtmlHelper helper)
        {
            var flash = (string)helper.ViewContext.TempData["FlashKey"];
            if (flash == null) return null;

            var tag = new TagBuilder("div");
            tag.AddCssClass("flash");
            tag.InnerHtml = flash;

            return new MvcHtmlString(tag.ToString());
        }
    }
}
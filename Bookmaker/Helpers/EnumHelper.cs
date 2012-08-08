using System;
using System.Linq;
using System.Web.Mvc;

namespace Bookmaker.Helpers
{
    public static class EnumHelper
    {
        public static SelectList Enums<TEnum>(this System.Data.Entity.DbContext db)
        {
            var values = from TEnum e in Enum.GetValues(typeof(TEnum))
                         select new { Id = e, Name = e.ToString() };

            return new SelectList(values, "Id", "Name");
        }
    }
}
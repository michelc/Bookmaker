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

        public static T ToEnum<T>(this string @string)
        {
            // http://matthewmanela.com/blog/i-finally-got-fed-up-with-enum-parse/
            return (T)Enum.Parse(typeof(T), @string);
        }

    }
}
using System;
using System.Linq;
using System.Web.Mvc;

namespace Bookmaker.Helpers
{
    public static class EnumHelper
    {
        public static T ToEnum<T>(this string @string)
        {
            // http://matthewmanela.com/blog/i-finally-got-fed-up-with-enum-parse/
            return (T)Enum.Parse(typeof(T), @string);
        }

    }
}
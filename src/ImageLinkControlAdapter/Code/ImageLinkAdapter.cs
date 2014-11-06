using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.Adapters;

namespace ImageLinkControlAdapter.Code
{
    public class ImageLinkAdapter : ControlAdapter
    {
        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            /// First we get the control's planned HTML that is emmitted...
            using (StringWriter baseStringWriter = new StringWriter())
            using (HtmlTextWriter baseWriter = new HtmlTextWriter(baseStringWriter))
            {
                base.Render(baseWriter);
                baseWriter.Flush();
                baseWriter.Close();
                /// Now we have an HTML element...
                string baseHtml = baseStringWriter.ToString();
                /// now fixit up...
                writer.Write(RebuildImgTag(baseHtml));
            }
        }


        internal string RebuildImgTag(string existingTagHtml)
        {
            var pattern = @"<img\s[^>]*>";
            var rv = Regex.Replace(existingTagHtml, pattern, this.InsertAlt);
            
            return rv;

        }

        internal string InsertAlt(Match match)
        {
            return this.InsertAlt(match.ToString());
        }

        internal string InsertAlt(string existingTag)
        {
            if (!existingTag.StartsWith("<img", StringComparison.InvariantCultureIgnoreCase))
                return existingTag;

            if (existingTag.Contains("alt=", StringComparison.InvariantCultureIgnoreCase))
                return existingTag;

            var insertPoint = existingTag.IndexOf("/>");
            var rv = existingTag.Insert(insertPoint, "alt=\"\"");
            return rv;
        }

    }

    internal static class StringExtensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }
    }
}

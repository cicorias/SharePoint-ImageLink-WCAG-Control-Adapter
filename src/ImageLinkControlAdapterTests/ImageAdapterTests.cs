using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using ImageLinkControlAdapter.Code;

namespace ImageLinkControlAdapterTests
{
    [TestClass]
    public class ImageAdapterTests
    {
        /*
        <img src="/_layouts/15/images/favicon.ico?rev=23" />
        <img src="/_layouts/15/images/spcommon.png?rev=23" />
        <img src="/_layouts/15/images/siteIcon.png?rev=23" />
        */


        [TestMethod]
        public void ActualStuffFromPage()
        {
            var HTML = "<div id=\"imgPrefetch\" style=\"display:none\">\r\n<img src=\"/_layouts/15/images/spcommon.png?rev=23\" />\r\n<img src=\"/_layouts/15/images/favicon.ico?rev=23\" />\r\n<img src=\"/_layouts/15/images/spcommon.png?rev=23\" />\r\n<img src=\"/_layouts/15/images/siteIcon.png?rev=23\" />\r\n</div>\r\n";

            Regex reImg = new Regex(@"<img\s[^>]*>", RegexOptions.IgnoreCase);
            MatchCollection mc = reImg.Matches(HTML);
            foreach (var match in mc)
            {
                Console.WriteLine(match);
            }

            ImageLinkAdapter adapter = new ImageLinkAdapter();
            var newHtml = adapter.RebuildImgTag(HTML);

            Console.WriteLine("Old HTML");
            Console.WriteLine(HTML);
            Console.WriteLine("New HTML");
            Console.WriteLine(newHtml);
        }


        [TestMethod]
        public void TestSimpleSPImageStandardLinksNotTheSame()
        {
            //arange
            string[] tests = new string[] {
            "<img src=\"/_layouts/15/images/favicon.ico?rev=23\" />",
            "<img src=\"/_layouts/15/images/spcommon.png?rev=23\" />",
            "<img src=\"/_layouts/15/images/siteIcon.png?rev=23\" />"
            };           
            //act
            ImageLinkAdapter adapter = new ImageLinkAdapter();

            //assert

            foreach (var item in tests)
            {
                var rv = adapter.RebuildImgTag(item);
                Assert.AreNotEqual(item, rv, true, "They are equal and shouldn't be");
            }

        }


        [TestMethod]
        public void ValidateAltWasAdded()
        {
            //arange
            string[] tests = new string[] {
            "<img src=\"/_layouts/15/images/favicon.ico?rev=23\" />",
            "<img src=\"/_layouts/15/images/spcommon.png?rev=23\" />",
            "<img src=\"/_layouts/15/images/siteIcon.png?rev=23\" />"
            };
            //act
            ImageLinkAdapter adapter = new ImageLinkAdapter();

            //assert

            foreach (var item in tests)
            {
                var rv = adapter.RebuildImgTag(item);
                Console.WriteLine(rv);
                var contains = rv.Contains("alt=\"\"");
                Assert.IsTrue(contains, "Missing the IMG alt tag in the img");
            }

        }

        [TestMethod]
        public void DontMessWithNonImgTags()
        {
            //arange
            string[] tests = new string[] {
            "<zimg src=\"/_layouts/15/images/favicon.ico?rev=23\" />",
            "<z src=\"/_layouts/15/images/spcommon.png?rev=23\" />",
            "<zimg src=\"/_layouts/15/images/siteIcon.png?rev=23\" />"
            };
            //act
            ImageLinkAdapter adapter = new ImageLinkAdapter();

            //assert

            foreach (var item in tests)
            {
                var rv = adapter.RebuildImgTag(item);
                Assert.AreEqual(item, rv, true, "They are equal and shouldn't be");
            }

        }


        [TestMethod]
        public void AlreadyHasAltDontChange()
        {
            //arange
            string[] tests = new string[] {
            "<img alt=\"\" src=\"/_layouts/15/images/favicon.ico?rev=23\" />",
            "<img src=\"/_layouts/15/images/siteIcon.png?rev=23\" alt=\"\" />"
            };
            //act
            ImageLinkAdapter adapter = new ImageLinkAdapter();

            //assert

            foreach (var item in tests)
            {
                var rv = adapter.RebuildImgTag(item);
                Assert.AreEqual(item, rv, true, "They are equal and shouldn't be");
            }

        }
    }
}


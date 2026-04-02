    public class LinkController : Controller
            if (!Request.Query.TryGetValue("maxlinks", out StringValues maxLinksSV) || !Int32.TryParse(maxLinksSV.FirstOrDefault(), out int maxLinks) || maxLinks < 1)
                maxLinks = 3;
            if (!Request.Query.TryGetValue("linknumber", out StringValues linkNumberSV) || !Int32.TryParse(linkNumberSV.FirstOrDefault(), out int linkNumber) || linkNumber < 1)
                linkNumber = 1;
            string baseUri = Regex.Replace(UriHelper.GetDisplayUrl(Request), "\\?.*", string.Empty);
            string type = Request.Query.TryGetValue("type", out StringValues typeSV) ? typeSV.FirstOrDefault() : "default";
            string whitespace = " ";
            if (type.ToUpper() == "EXTRAWHITESPACE")
                whitespace = "  ";
            else if (type.ToUpper() == "NOWHITESPACE")
                whitespace = string.Empty;
            var linkList = new List<string>();
            if (maxLinks > 1 && linkNumber > 1)
                linkList.Add(GetLink(baseUri: baseUri, maxLinks: maxLinks, linkNumber: linkNumber - 1, type: type, whitespace: whitespace, rel: "prev"));
            linkList.Add(GetLink(baseUri: baseUri, maxLinks: maxLinks, linkNumber: maxLinks, type: type, whitespace: whitespace, rel: "last"));
            linkList.Add(GetLink(baseUri: baseUri, maxLinks: maxLinks, linkNumber: 1, type: type, whitespace: whitespace, rel: "first"));
            linkList.Add(GetLink(baseUri: baseUri, maxLinks: maxLinks, linkNumber: linkNumber, type: type, whitespace: whitespace, rel: "self"));
            bool sendMultipleHeaders = false;
            bool skipNextLink = false;
            switch (type.ToUpper())
                case "NOURL":
                    linkList.Add(Constants.NoUrlLinkHeader);
                    skipNextLink = true;
                case "MALFORMED":
                    linkList.Add(Constants.MalformedUrlLinkHeader);
                case "NOREL":
                    linkList.Add(Constants.NoRelLinkHeader);
                case "MULTIPLE":
                    sendMultipleHeaders = true;
            if (!skipNextLink && maxLinks > 1 && linkNumber < maxLinks)
                linkList.Add(GetLink(baseUri: baseUri, maxLinks: maxLinks, linkNumber: linkNumber + 1, type: type, whitespace: whitespace, rel: "next"));
            StringValues linkHeader;
            if (sendMultipleHeaders)
                linkHeader = linkList.ToArray();
                linkHeader = string.Join(',', linkList);
            Response.Headers.Append("Link", linkHeader);
            // Generate /Get/ result and append linknumber, maxlinks, and type
            var result = getController.Index();
            var output = result.Value as Hashtable;
            output.Add("linknumber", linkNumber);
            output.Add("maxlinks", maxLinks);
            output.Add("type", type.FirstOrDefault());
        private static string GetLink(string baseUri, int maxLinks, int linkNumber, string whitespace, string type, string rel)
            return string.Format(Constants.LinkUriTemplate, baseUri, maxLinks, linkNumber, type, whitespace, rel);

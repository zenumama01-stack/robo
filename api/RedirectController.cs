    public class RedirectController : Controller
        public IActionResult Index(int count)
            string url = Regex.Replace(input: Request.GetDisplayUrl(), pattern: "\\/Redirect.*", replacement: string.Empty, options: RegexOptions.IgnoreCase);
            var destinationIsPresent = Request.Query.TryGetValue("destination", out StringValues destination);
            if (count <= 1)
                url = destinationIsPresent ? destination.FirstOrDefault() : $"{url}/Get/";
                int nextHop = count - 1;
                url = $"{url}/Redirect/{nextHop}";
            var typeIsPresent = Request.Query.TryGetValue("type", out StringValues type);
            if (typeIsPresent && Enum.TryParse(type.FirstOrDefault(), out HttpStatusCode status))
                Response.StatusCode = (int)status;
                url = $"{url}?type={type.FirstOrDefault()}";
                Response.Headers.Append("Location", url);
            else if (typeIsPresent && string.Equals(type.FirstOrDefault(), "relative", StringComparison.InvariantCultureIgnoreCase))
                url = new Uri($"{url}?type={type.FirstOrDefault()}").PathAndQuery;
            else if (destinationIsPresent)
                Response.StatusCode = 302;

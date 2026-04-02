    public class DosController : Controller
        public string Index()
            string contentType = Constants.ApplicationJson;
            Response.StatusCode = 200;
            StringValues dosType;
            if (Request.Query.TryGetValue("dosType", out dosType))
                output = dosType.FirstOrDefault();
            StringValues dosLengths;
            Int32 dosLength = 1;
            if (Request.Query.TryGetValue("dosLength", out dosLengths))
                Int32.TryParse(dosLengths.FirstOrDefault(), out dosLength);
            string body = string.Empty;
            switch (dosType)
                case "img":
                    contentType = "text/html; charset=utf8";
                    body = "<img" + (new string(' ', dosLength));
                // This is not really a DOS test, but this is the best place for it at present.
                case "img-attribute":
                    body = "<img src=\"https://fakesite.org/image.png\" id=\"mainImage\" class=\"lightbox\">";
                case "charset":
                    contentType = "text/html; charset=melon";
                    body = "<meta " + (new string('.', dosLength));
                    throw new InvalidOperationException("Invalid dosType: " + dosType);
            // Content-Type must be applied right before it is sent to the client or MVC will overwrite.
            Response.OnStarting(state =>
                    var httpContext = (HttpContext)state;
                    httpContext.Response.ContentType = contentType;
                    return Task.FromResult(0);
                }, HttpContext);
            Response.ContentLength = Encoding.UTF8.GetBytes(body).Length;
            return body;

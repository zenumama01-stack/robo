    public class ResponseHeadersController : Controller
                headers.Add(key, string.Join(Constants.HeaderSeparator, (string)Request.Query[key]));
                if (string.Equals("Content-Type", key, StringComparison.InvariantCultureIgnoreCase))
                    string contentType = Request.Query[key];
                    Response.Headers.TryAdd(key, Request.Query[key]);
            return JsonConvert.SerializeObject(headers);

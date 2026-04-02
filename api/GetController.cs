    public class GetController : Controller
            Hashtable args = new Hashtable();
            foreach (var key in Request.Query.Keys)
                args.Add(key, string.Join(Constants.HeaderSeparator, (string)Request.Query[key]));
            Hashtable headers = new Hashtable();
            foreach (var key in Request.Headers.Keys)
                headers.Add(key, string.Join(Constants.HeaderSeparator, (string)Request.Headers[key]));
                { "args", args },
                { "headers", headers },
                { "origin", Request.HttpContext.Connection.RemoteIpAddress.ToString() },
                { "url", UriHelper.GetDisplayUrl(Request) },
                { "query", Request.QueryString.ToUriComponent() },
                { "method", Request.Method },
                { "protocol", Request.Protocol }
            if (Request.HasFormContentType)
                Hashtable form = new Hashtable();
                foreach (var key in Request.Form.Keys)
                    form.Add(key, Request.Form[key]);
                output["form"] = form;
            string data = new StreamReader(Request.Body).ReadToEnd();
            if (!string.IsNullOrEmpty(data))
                output["data"] = data;

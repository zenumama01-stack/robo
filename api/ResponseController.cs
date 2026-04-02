using Microsoft.AspNetCore.Http.Features;
    public class ResponseController : Controller
            StringValues contentTypes;
            if (Request.Query.TryGetValue("contenttype", out contentTypes))
                contentType = contentTypes.FirstOrDefault();
            StringValues statusCodes;
            Int32 statusCode;
            if (Request.Query.TryGetValue("statuscode", out statusCodes) &&
                Int32.TryParse(statusCodes.FirstOrDefault(), out statusCode))
                Response.StatusCode = statusCode;
            StringValues responsePhrase;
            if (Request.Query.TryGetValue("responsephrase", out responsePhrase))
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = responsePhrase.FirstOrDefault();
            StringValues body;
            if (Request.Query.TryGetValue("body", out body))
                output = body.FirstOrDefault();
            StringValues headers;
            if (Request.Query.TryGetValue("headers", out headers))
                    Response.Headers.Clear();
                    JObject jobject = JObject.Parse(headers.FirstOrDefault());
                    foreach (JProperty property in (JToken)jobject)
                        // Only set Content-Type through contenttype field.
                        if (string.Equals(property.Name, "Content-Type", StringComparison.InvariantCultureIgnoreCase))
                        foreach (string entry in GetSingleOrArray<string>(property.Value))
                            Response.Headers.Append(property.Name, entry);
                    output = JsonConvert.SerializeObject(ex);
                    Response.StatusCode = StatusCodes.Status500InternalServerError;
                    contentType = Constants.ApplicationJson;
            Response.ContentLength = Encoding.UTF8.GetBytes(output).Length;
        private static List<T> GetSingleOrArray<T>(JToken token)
            if (token.HasValues)
                return token.ToObject<List<T>>();
                return new List<T> { token.ToObject<T>() };

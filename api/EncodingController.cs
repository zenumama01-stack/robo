using Microsoft.Net.Http.Headers;
    public class EncodingController : Controller
            string url = "/Encoding/Utf8";
        public ActionResult Utf8()
            MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue("text/html");
            mediaType.Encoding = Encoding.UTF8;
            return View();
        public async void Cp936()
            var encoding = Encoding.GetEncoding(936);
            mediaType.Encoding = encoding;
            Response.ContentType = mediaType.ToString();
            var body = new byte[]
                178,
                226,
                202,
                212,
                49,
                50,
                51
            await Response.Body.WriteAsync(body, 0, body.Length);

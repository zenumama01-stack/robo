    public class CompressionController : Controller
        public ActionResult Index()
            string url = "/Compression/Gzip";
            ViewData["Url"] = url;
            Response.Redirect(url, false);
            return View("~/Views/Redirect/Index.cshtml");
        [GzipFilter]
        public JsonResult Gzip()
        [DeflateFilter]
        public JsonResult Deflate()
        [BrotliFilter]
        public JsonResult Brotli()

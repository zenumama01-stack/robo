    public class MultipartController : Controller
        private readonly IWebHostEnvironment _environment;
        public MultipartController(IWebHostEnvironment environment)
            _environment = environment;
        [HttpPost]
        public JsonResult Index(IFormCollection collection)
            if (!Request.HasFormContentType)
                Response.StatusCode = 415;
                Hashtable error = new Hashtable { { "error", "Unsupported media type" } };
                return  Json(error);
            List<Hashtable> fileList = new List<Hashtable>();
            foreach (var file in collection.Files)
                if (file.Length > 0)
                    using (var reader = new StreamReader(file.OpenReadStream()))
                        result = reader.ReadToEnd();
                Hashtable fileHash = new Hashtable
                    {"ContentDisposition", file.ContentDisposition},
                    {"ContentType", file.ContentType},
                    {"FileName", file.FileName},
                    {"Length", file.Length},
                    {"Name", file.Name},
                    {"Content", result},
                    {"Headers", file.Headers}
                fileList.Add(fileHash);
            Hashtable itemsHash = new Hashtable();
            foreach (var key in collection.Keys)
                itemsHash.Add(key, collection[key]);
            MediaTypeHeaderValue mediaContentType = MediaTypeHeaderValue.Parse(Request.ContentType);
                {"Files", fileList},
                {"Items", itemsHash},
                {"Boundary", HeaderUtilities.RemoveQuotes(mediaContentType.Boundary).Value},
                {"Headers", headers}

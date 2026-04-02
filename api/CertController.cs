    public class CertController : Controller
        public JsonResult Index()
            // X509Certificate2 objects do not serialize as JSON. Create a HashTable instead
            Hashtable output = new Hashtable
                {"Status", "FAILED"}
            if (HttpContext.Connection.ClientCertificate != null)
                output = new Hashtable
                    {"Status", "OK"},
                    {"Thumbprint", HttpContext.Connection.ClientCertificate.Thumbprint},
                    {"Subject", HttpContext.Connection.ClientCertificate.Subject},
                    {"SubjectName", HttpContext.Connection.ClientCertificate.SubjectName.Name},
                    {"Issuer", HttpContext.Connection.ClientCertificate.Issuer},
                    {"IssuerName", HttpContext.Connection.ClientCertificate.IssuerName.Name},
                    {"NotAfter", HttpContext.Connection.ClientCertificate.NotAfter},
                    {"NotBefore", HttpContext.Connection.ClientCertificate.NotBefore}
            return Json(output);

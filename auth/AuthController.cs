using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;
using mvc.Models;
    public class AuthController : Controller
        public JsonResult Basic()
            StringValues authorization;
            if (Request.Headers.TryGetValue("Authorization", out authorization))
                var getController = new GetController();
                getController.ControllerContext = this.ControllerContext;
                return getController.Index();
                Response.Headers.Append("WWW-Authenticate", "Basic realm=\"WebListener\"");
                Response.StatusCode = 401;
                return Json("401 Unauthorized");
        public JsonResult Negotiate()
                Response.Headers.Append("WWW-Authenticate", "Negotiate");
        public JsonResult Ntlm()
                Response.Headers.Append("WWW-Authenticate", "NTLM");
        public IActionResult Error()
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

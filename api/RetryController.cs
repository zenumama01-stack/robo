    public class RetryController : Controller
        // Dictionary for sessionId as key and failureCode, failureCount and failureResponsesSent as the value.
        private static Dictionary<string, Tuple<int, int, int>> retryInfo;
        public JsonResult Retry(string sessionId, int failureCode, int failureCount)
            retryInfo ??= new Dictionary<string, Tuple<int, int, int>>();
            if (retryInfo.TryGetValue(sessionId, out Tuple<int, int, int> retry))
                // if failureResponsesSent is less than failureCount
                if (retry.Item3 < retry.Item2)
                    Response.StatusCode = retry.Item1;
                    retryInfo[sessionId] = Tuple.Create(retry.Item1, retry.Item2, retry.Item3 + 1);
                    Hashtable error = new Hashtable { { "error", $"Error: HTTP - {retry.Item1} occurred." } };
                    return Json(error);
                    retryInfo.Remove(sessionId);
                    // echo back sessionId for POST test.
                    var resp = new Hashtable { { "failureResponsesSent", retry.Item3 }, { "sessionId", sessionId } };
                    return Json(resp);
                // initialize the failureResponsesSent as 1.
                var newRetryInfoItem = Tuple.Create(failureCode, failureCount, 1);
                retryInfo.Add(sessionId, newRetryInfoItem);
                Response.StatusCode = failureCode;
                Hashtable error = new Hashtable { { "error", $"Error: HTTP - {failureCode} occurred." } };

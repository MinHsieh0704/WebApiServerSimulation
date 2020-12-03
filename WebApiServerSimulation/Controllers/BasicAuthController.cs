using Min_Helpers;
using Min_Helpers.PrintHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace WebApiServerSimulation.Controllers
{
    public class BasicAuthController : ApiController
    {
        [HttpGet, HttpPost, HttpPut, HttpDelete]
        public IHttpActionResult Handle()
        {
            try
            {
                HttpRequestMessage req = Request;
                Uri reqUri = req.RequestUri;
                HttpMethod reqMethod = req.Method;
                HttpContent reqContent = req.Content;
                HttpRequestHeaders reqHeaders = req.Headers;

                var auth = reqHeaders.Authorization;
                if (auth == null)
                {
                    HttpResponseMessage res = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                    res.Headers.Add("WWW-Authenticate", "Basic realm=\"User Visible Realm\"");
                    res.Content = new StringContent("This action requires login.");

                    return ResponseMessage(res);
                }
                else
                {
                    string authSource = Encoding.UTF8.GetString(Convert.FromBase64String(auth.Parameter));
                    string account = authSource.Substring(0, authSource.IndexOf(":"));
                    string password = authSource.Substring(authSource.IndexOf(":") + 1, authSource.Length - authSource.IndexOf(":") - 1);

                    if (account != Program.basicAuth?.account || password != Program.basicAuth?.password)
                    {
                        HttpResponseMessage res = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                        res.Content = new StringContent("Login failed.");

                        return ResponseMessage(res);
                    }
                }

                List<KeyValuePair<string, IEnumerable<string>>> headers = reqHeaders.ToList().Concat(reqContent.Headers.ToList()).ToList();

                JObject content = new JObject();
                if (reqMethod == HttpMethod.Get || reqMethod == HttpMethod.Delete)
                {
                    NameValueCollection input = HttpUtility.ParseQueryString(reqUri.Query);

                    foreach (var key in input)
                    {
                        List<string> keys = Regex.Split(key.ToString(), @"\.").ToList();
                        JToken _JToken = content;
                        for (int i = 0; i < keys.Count(); i++)
                        {
                            if (i == keys.Count() - 1) _JToken[keys[i]] = input[key.ToString()];
                            if (_JToken[keys[i]] == null) _JToken[keys[i]] = new JObject();
                            _JToken = _JToken[keys[i]];
                        }
                    }
                }
                else if (reqMethod == HttpMethod.Post || reqMethod == HttpMethod.Put)
                {
                    string input = reqContent.ReadAsStringAsync().Result;
                    if (!string.IsNullOrEmpty(input)) content = JsonConvert.DeserializeObject<JObject>(input);
                }

                string info = "";
                info += $"\r\n    Method: {reqMethod.Method}";
                info += $"\r\n    Path: {reqUri.LocalPath}";
                info += $"\r\n    Headers:";
                foreach(var header in headers)
                {
                    if (header.Value == null) continue;
                    info += $"\r\n        - {header.Key}: {JsonConvert.SerializeObject(header.Value)}";
                }
                info += $"\r\n    Content:";
                info += IndexController.ContentInfo(content, 0);

                Program.PrintService.Log($"{info}", Print.EMode.message);

                return Json(content);
            }
            catch (Exception ex)
            {
                ex = ExceptionHelper.GetReal(ex);
                Program.PrintService.Log($"{ex.Message}", Print.EMode.error);

                return Content(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}

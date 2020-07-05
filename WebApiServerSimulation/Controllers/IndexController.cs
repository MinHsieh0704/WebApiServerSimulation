using Min_Helpers;
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
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace WebApiServerSimulation.Controllers
{
    public class IndexController : ApiController
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

                List<KeyValuePair<string, IEnumerable<string>>> headers = reqHeaders.ToList().Concat(reqContent.Headers.ToList()).ToList();

                JObject _input = new JObject();
                if (reqMethod == HttpMethod.Get || reqMethod == HttpMethod.Delete)
                {
                    NameValueCollection input = HttpUtility.ParseQueryString(reqUri.Query);

                    foreach (var key in input)
                        _input.Add(new JProperty(key.ToString(), input[key.ToString()]));
                }
                else if (reqMethod == HttpMethod.Post || reqMethod == HttpMethod.Put)
                {
                    string input = reqContent.ReadAsStringAsync().Result;

                    _input = JsonConvert.DeserializeObject<JObject>(input);
                }

                string info = "";
                info += $"\r\n{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}";
                info += $"\r\n    {reqMethod.Method}, {reqUri.LocalPath}";
                info += "\r\n    Headers";
                foreach(var header in headers)
                {
                    if (header.Value == null) continue;
                    info += $"\r\n        {header.Key}: {JsonConvert.SerializeObject(header.Value)}";
                }
                info += "\r\n    Content";
                foreach (var input in _input)
                {
                    if (input.Value == null) continue;
                    info += $"\r\n        {input.Key}: {input.Value}";
                }

                ConsoleHelper.WriteLine(info, ConsoleHelper.EMode.message);

                return Json(_input);
            }
            catch (Exception ex)
            {
                ex = ExceptionHelper.GetReal(ex);
                ConsoleHelper.Log($"{ex.Message}", ConsoleHelper.EMode.error);
                Program.log.Error(ex);

                return Content(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}

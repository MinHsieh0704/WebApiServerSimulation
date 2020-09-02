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

                    content = JsonConvert.DeserializeObject<JObject>(input);
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
                info += this.ContentInfo(content, 0);

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

        /// <summary>
        /// Content Info
        /// </summary>
        /// <param name="content"></param>
        /// <param name="deep"></param>
        /// <returns></returns>
        private string ContentInfo(JObject content, int deep)
        {
            try
            {
                string info = "";
                foreach (var input in content.Properties())
                {
                    JToken jToken = content[input.Name];
                    switch (jToken.Type)
                    {
                        case JTokenType.Null:
                            info += $"\r\n{"".PadLeft((deep + 2) * 4, ' ')}- {input.Name}: null";
                            break;
                        case JTokenType.Object:
                            info += $"\r\n{"".PadLeft((deep + 2) * 4, ' ')}- {input.Name}:";
                            info += this.ContentInfo(jToken.ToObject<JObject>(), deep + 1);
                            break;
                        case JTokenType.Array:
                            info += $"\r\n{"".PadLeft((deep + 2) * 4, ' ')}- {input.Name}:";
                            for (int i = 0; i < jToken.Count(); i++)
                            {
                                info += $"\r\n{"".PadLeft((deep + 3) * 4, ' ')}{i}:";
                                info += this.ContentInfo(jToken[i].ToObject<JObject>(), deep + 2);
                            }
                            break;
                        case JTokenType.Date:
                            DateTime dt = (DateTime)input.Value;
                            var a = TimeZoneInfo.ConvertTimeToUtc(dt);
                            info += $"\r\n{"".PadLeft((deep + 2) * 4, ' ')}- {input.Name}: {a.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}";
                            break;
                        default:
                            info += $"\r\n{"".PadLeft((deep + 2) * 4, ' ')}- {input.Name}: {input.Value}";
                            break;
                    }
                }

                return info;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

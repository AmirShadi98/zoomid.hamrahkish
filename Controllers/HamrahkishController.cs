using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace zoomid.hamrahkish.API.Controllers
{
    public class HamrahkishController : ApiController
    {
        [HttpPost]
        [Route("api/Hamrahkish/HamrahkishData")]
        public IHttpActionResult HamrahkishData([FromBody] dynamic data)
        {
            if (data == null)
            {
                return BadRequest("Invalid data format");
            }

            string userName = data.username;
            string password = data.password;

            string privateKeyPath = HttpContext.Current.Server.MapPath("~/Keys/Private.pem");
            string publicKeyPath = HttpContext.Current.Server.MapPath("~/Keys/public.pem");

            string privateKey = File.ReadAllText(privateKeyPath);
            string publicKey = File.ReadAllText(publicKeyPath); 
            
            privateKey = RemoveNonPrintableCharacters(privateKey);
            publicKey = RemoveNonPrintableCharacters(publicKey);

            var postData = new
            {
                username = userName,
                password = password
            };

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(postData);

            using (HttpClient client = new HttpClient())
            {
                string baseUrl = "https://zoomid.hamrahkish.com";
                client.BaseAddress = new Uri(baseUrl);

                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                content.Headers.Add("private-key", privateKey);
                content.Headers.Add("public-key", publicKey);

                try
                {
                    HttpResponseMessage response = client.PostAsync("/hrbox", content).Result; 
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = response.Content.ReadAsStringAsync().Result;
                        return Ok(responseContent);
                    }
                    else
                    {
                        return BadRequest("Failed to call the API. Status code: " + response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }
            }
        }

            private string RemoveNonPrintableCharacters(string input)
            {
                return Regex.Replace(input, @"\p{C}+", string.Empty);
            }
    }
}

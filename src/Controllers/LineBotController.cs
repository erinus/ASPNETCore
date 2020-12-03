using System;
using System.IO;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace app.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LineBotController : ControllerBase
    {
        private readonly ILogger<LineBotController> _logger;

        public LineBotController(ILogger<LineBotController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public Object Post([FromBody] dynamic content)
        {
            Console.WriteLine($"LINE: {content}");
            foreach (dynamic e in content.GetProperty("events").EnumerateArray())
            {
                if (e.GetProperty("type").ToString().Equals("message"))
                {
                    string text = e.GetProperty("message").GetProperty("text").ToString();
                    string token = e.GetProperty("replyToken").ToString();
                    Console.WriteLine($"      {token}");
                    Console.WriteLine($"      {text}");

                    HttpWebRequest req = WebRequest.Create("https://api.line.me/v2/bot/message/reply") as HttpWebRequest;
                    req.Method = "POST";
                    req.Headers["Content-Type"] = "application/json";
                    req.Headers["Authorization"] = "Bearer t5TwfaBQnhwqFnOpEI510zNnFjLmwMs6+hNGQuW4tHKuRvZREezBsu/y5oq/6HKSDg1js2769TUhpgzsO/SpAjKlQoMzHuWeG/icYmVaTO+NKWY5f2QCddBjSY8aNRN4pn9t8dTEJOL3e3METCwHYQdB04t89/1O/w1cDnyilFU=";
                    using (StreamWriter writer = new StreamWriter(req.GetRequestStream()))
                    {
                        var data = new
                        {
                            replyToken = token,
                            messages = new []
                            {
                                new
                                {
                                    type = "text",
                                    text = "test"
                                }
                            }
                        };
                        writer.WriteLine(JsonSerializer.Serialize(data));
                    }
                    HttpWebResponse rsp = req.GetResponse() as HttpWebResponse;
                    using (StreamReader reader = new StreamReader(rsp.GetResponseStream()))
                    {
                        Console.WriteLine(reader.ReadToEnd());
                    }
                }
            }
            return new {};
        }
    }
}
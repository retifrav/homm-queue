using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;
using homm_queue.Models;

namespace homm_queue.Controllers
{   
    public class HomeController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly string queueFile = "queue.json";

        public HomeController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            queueFile = Path.Combine(_hostingEnvironment.WebRootPath, queueFile);
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Queue";

            List<Player> queue = new List<Player>();

            JObject queueJson = ReadQueueFile();
            
            foreach(var player in queueJson["players"])
            {
                if (Convert.ToBoolean(player["playing"]))
                {
                    //Console.WriteLine($"[DEBUG] {player["name"]}");
                    queue.Add(new Player()
                    {
                        Name = Convert.ToString(player["name"]),
                        CurrentTurn = Convert.ToBoolean(player["currentTurn"])
                    });
                }
            }

            int playersWithTurn = queueJson.SelectTokens(
                "$.players[?(@.currentTurn == true)]"
                ).Count();

            if (playersWithTurn > 1)
            {
                ViewData["Error"] = "more than one player with current turn";
                return View(null);
            }
            if (playersWithTurn < 1)
            {
                ViewData["Error"] = "no players with current turn";
                return View(null);
            }

            return View(queue);
        }

        [HttpGet("/StatusCode/{statusCode}")]
        public IActionResult Index(int statusCode)
        {
            return View("StatusCode", statusCode);
        }

        [HttpPost("/MakeTurn")]
        public JsonResult MakeTurn()
        {
            JObject queueJson = ReadQueueFile();

            string nextPlayerID = "unknown";

            // if (!string.IsNullOrEmpty(nextPlayer))
            // {
            //     // TODO set turn on provided player
            // }
            // else
            {
                bool setCurrentPlayerTurn = false;
                foreach(var player in queueJson["players"])
                {
                    if (Convert.ToBoolean(player["playing"]))
                    {
                        if (setCurrentPlayerTurn)
                        {
                            player["currentTurn"] = true;
                            nextPlayerID = Convert.ToString(player["slackID"]);
                            setCurrentPlayerTurn = false;
                            break;
                        }
                        if (Convert.ToBoolean(player["currentTurn"]))
                        {
                            player["currentTurn"] = false;
                            setCurrentPlayerTurn = true;
                        }
                    }
                }
                if (setCurrentPlayerTurn)
                {
                    queueJson["players"][0]["currentTurn"] = true;
                    nextPlayerID = Convert.ToString(queueJson["players"][0]["slackID"]);
                }
            }

            using (StreamWriter file = new StreamWriter(queueFile, false))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                queueJson.WriteTo(writer);
            }

            try // send message to Slack
            {
                PostToSlackChannel(nextPlayerID);
            }
            catch (Exception ex)
            {
                // TODO log the error
            }

            //Response.StatusCode = 200;
            return Json("OK");
        }

        [HttpGet("/GetCurrentTurn")]
        public JsonResult GetCurrentTurn()
        {
            JObject queueJson = ReadQueueFile();
            try
            {
                return Json(
                    queueJson.SelectToken("$.players[?(@.currentTurn == true)]")["name"]
                    );
            }
            catch
            {
                return Json("unknown");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                    });
        }

        private JObject ReadQueueFile()
        {
            using (StreamReader reader = new StreamReader(queueFile))
            {
                return (JObject)JToken.ReadFrom(new JsonTextReader(reader));
            }
        }

        //async Task<Tuple<int, string>> PostToSlackChannel(string userSlackID)
        async void PostToSlackChannel(string userSlackID)
        {
            // var textData = new Dictionary<string, string>
            // {
            //     { "text", $"<@{userSlackID}>'s turn!" }
            // };

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri("https://hooks.slack.com/");

                HttpRequestMessage request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "services/T4DCCB6O2/BNJ1VMEKW/FSzIk0ZbqkFvUiF2Jf21PEow"
                    );
                StringBuilder msg = new StringBuilder()
                    .Append("{\"text\":\"")
                    .Append($"<@{userSlackID}>'s turn!")
                    .Append("\"}");
                request.Content = new StringContent(
                    msg.ToString(),
                    Encoding.UTF8,
                    "application/json"
                    );

                try
                {
                    var httpResponse = await httpClient.SendAsync(request);
                    var httpContent = await httpResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"[DEBUG] {(int)httpResponse.StatusCode} - {httpContent}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}

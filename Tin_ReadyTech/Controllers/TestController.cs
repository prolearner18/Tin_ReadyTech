using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using Newtonsoft.Json;
using System.Net.Http;

namespace Tin_ReadyTech.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : Controller
    {
        int month = DateTime.Now.Month;
        int day = DateTime.Now.Day;
        int requestcount; decimal aucklandtemperature;
        [HttpGet]
        public async Task<ActionResult> BrewCoffee()
        {
            requestcount = 0;
            aucklandtemperature = 0;
            string currentdate = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss+ffff", CultureInfo.InvariantCulture);

            Response weather = await City("Auckland");
            if (weather != null)
            {
                aucklandtemperature = weather.Temp;
            }

            RememberRequest();

            if (requestcount == 5)
            {
                HttpContext.Session.SetString("Key", "0");
                return StatusCode(503, "");
            }
            else if (month == 4 && day == 1)
            {
                return StatusCode(418, "");
            }
            else if (aucklandtemperature > 30)
            {
                var data = new Coffee
                {
                    message = "Your refreshing iced coffee is ready",
                    prepared = currentdate
                };
                return Ok(data);
            }
            else
            {
                var data = new Coffee
                {
                    message = "Your piping hot coffee is ready",
                    prepared = currentdate
                };
                return Ok(data);
            }
        }

        private void RememberRequest()
        {
            if (HttpContext.Session.GetString("Key") != null)
            {
                requestcount = Convert.ToInt32(HttpContext.Session.GetString("Key").ToString()) + 1;
                HttpContext.Session.SetString("Key", requestcount.ToString());
            }
            else
            {
                HttpContext.Session.SetString("Key", "1");
            }
        }

        [HttpGet("[action]/{city}")]
        public async Task<Response> City(string city)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://api.openweathermap.org");
                var response = await client.GetAsync($"/data/2.5/weather?q={city}&appid=e6027afe2ff777422fdfb01d1ecf4af7&units=metric");
                response.EnsureSuccessStatusCode();

                var stringResult = await response.Content.ReadAsStringAsync();
                var rawWeather = JsonConvert.DeserializeObject<OpenWeatherResponse>(stringResult);
                decimal tempearature = string.IsNullOrEmpty(rawWeather.Main.Temp) ? 0 : Convert.ToDecimal(rawWeather.Main.Temp);
                Response wresponse = new Response
                {
                    Temp = tempearature,
                    //Summary = string.Join(",", rawWeather.Weather.Select(x => x.Main)),
                    City = rawWeather.Name
                };
                return wresponse;
            }
        }


    }
}

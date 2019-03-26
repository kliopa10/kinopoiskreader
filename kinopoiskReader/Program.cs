using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Console;
using Newtonsoft.Json;

namespace kinopoiskReader
{
    class Program
    {
        static string BASE_ADDRESS = "https://ma.kinopoisk.ru";
        static string UUID = "6730382b7a236cd964264b49413ed00f";
        static string KINOPOISK_API_SALT = "IDATevHDS7";
        static string CLIENT_ID = "56decdcf6d4ad1bcaa1b3856";

        static  void Main(string[] args)
        {
            
            Resp();
            Console.ReadLine();
        }
        static async void Resp()
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };

            using (HttpClient client = new HttpClient(handler))
            {
                try
                {
                    var request = PrepareRequest();
                    var task = await client.SendAsync(request);
                    task.EnsureSuccessStatusCode();
                    var responce = await task.Content.ReadAsStringAsync();
                    var json = JsonConvert.DeserializeObject<Result>(responce);
                    foreach(Film film in json.data.items)
                    {
                        WriteLine(film.title);
                    }

                } catch(HttpRequestException ex)
                {
                    Console.WriteLine("EXCEPTION!!!");
                    Console.WriteLine($"{ex.Message}");
                }
            }
        }
        static HttpRequestMessage PrepareRequest()
        {
            var date = DateTime.Now;
            string releaseDate = date.Month < 10 ?
                $"0{date.Month.ToString()}.{date.Year.ToString()}" 
                : $"{date.Month.ToString()}.{date.Year.ToString()}";
            string ADDRESS_RELEASES = $"/k/v1/films/releases/digital?digitalReleaseMonth={releaseDate}&limit=1000&offset=0&uuid={UUID}";
            string url = $"{BASE_ADDRESS}{ADDRESS_RELEASES}";
            string timestamp = Math.Round(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds).ToString();
            
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept-encoding", "gzip");
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("User-Agent", "Android client (6.0.1 / api23) ru.kinopoisk/4.6.5 (86)"); 
            request.Headers.Add("Image-Scale", "3");
            request.Headers.Add("device", "android");
            request.Headers.Add("ClientId", CLIENT_ID);
            request.Headers.Add("countryID", "2");
            request.Headers.Add("cityID", "1");
            request.Headers.Add("Android-Api-Version", "23");
            request.Headers.Add("clientDate", date.ToString("hh:mm dd.MM.yyyy"));
            request.Headers.Add("X-TIMESTAMP", timestamp);
            request.Headers.Add("X-SIGNATURE", GetHash(url, timestamp));
            return request;
        }
        static string GetHash(string url, string timestamp)
        {
            string hashString = url + timestamp + KINOPOISK_API_SALT;
            byte[] encoded = new UTF8Encoding().GetBytes(hashString);
            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encoded);
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
        }
        public class Result
        {
            public bool success;
            public Item data;
        }
        public class Item
        {
            public Film[] items;
        }
        public class Film
        {
            public int id;
            public string slug;
            public string title;
            public string originalTitle;
            public int year;
            public Poster poster;
            public Genres[] genres;
            public Countrie[] countries;
            public Rating rating;
            public Expectation expectation;
            public string currentRating;
            public bool serial;
            public int duration;
            public int trailerId;
            public ContextData contextData;
        }
        public class ContextData
        {
            public bool isDigital;
            public string releaseDate;
        }
        public class Countrie
        {
            public int id;
            public string name;
        }
        public class Rating
        {
            public double value;
            public int count;
            public bool ready;
        }
        public class Expectation
        {
            public double value;
            public int count;
            public bool ready;
        }
        public class Genres
        {
            public int id;
            public string name;
        }
        public class Poster
        {
            public string baseUrl;
            public string url;
        }
    }
    
}
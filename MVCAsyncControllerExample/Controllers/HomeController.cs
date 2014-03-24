using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace MVCAsyncControllerExample.Controllers
{
    //an example of an asynchronous controller
    public class HomeController : AsyncController
    {
        public ActionResult Index()
        {
            return View();
        }


        //For async controller methods we impelement two methods, one with Async suffix and one with Completed suffix
        public void getDataAsync(string term)
        {
            AsyncManager.OutstandingOperations.Increment();
            Fetcher fetcher = new Fetcher();
            fetcher.fetcherCompleted += (sender, e) =>
            {
                AsyncManager.Parameters["json"] = e.jsonData;
                AsyncManager.OutstandingOperations.Decrement();
            };
            fetcher.getJSONDataAsync(term);
        }

        public ActionResult getDataCompleted(string jsonarg)
        {
            //string json = jsona[0];
            string json = AsyncManager.Parameters["json"].ToString();
            ContentResult result = Content(json, "application/json");

            return result;
        }
    }

    public class Fetcher
    {
        public delegate void FetcherCompleted(object sender, FetcherEventArgs e);
        public FetcherCompleted fetcherCompleted;

        public async void getJSONDataAsync(string term)
        {
            HttpClient client = new HttpClient(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip });
            string url = string.Format("http://api.stackexchange.com/2.2/search?order=desc&sort=activity&site=stackoverflow&intitle={0}", term);
            var uri = new Uri(url);
            string json = "";

            await client.GetAsync(uri).ContinueWith(async t =>
            {
                json = await t.Result.Content.ReadAsStringAsync();
            });
            FetcherEventArgs e = new FetcherEventArgs() { jsonData = json };

            fetcherCompleted(this, e);
            //fetcherCompleted.Invoke(this, e);
        }
    }

    public class FetcherEventArgs : EventArgs
    {
        public string jsonData { get; set; }
    }
}

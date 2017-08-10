using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RelicTool {
    public static class MarketPuller {
        private const string url = "https://warframe.market/api/get_orders/Blueprint/";

        public static async Task<long> GetCheapest(string name) {

            var textInfo = new CultureInfo("en-US", false).TextInfo;

            name = textInfo.ToTitleCase(name.ToLower());

            if (name.EndsWith(" Blueprint") && !name.EndsWith("Prime Blueprint"))
                name = name.Replace(" Blueprint", "");

            Console.WriteLine(name);

            string jsonData;
            using (var client = new WebClient()) {
                var uri = new Uri(url + Uri.EscapeDataString(name));

                try {
                    jsonData = await client.DownloadStringTaskAsync(uri);

                    dynamic result = JsonConvert.DeserializeObject(jsonData);

                    if (result.code != 200) {
                        return 0;
                    }

                    IEnumerable<dynamic> sellOrders = result.response.sell;
                    long smallestPrice = sellOrders.Where(order => order.online_status).Min(order => order.price);
                    return smallestPrice;
                }
                catch {
                    return 0;
                }
            }
        }
    }
}

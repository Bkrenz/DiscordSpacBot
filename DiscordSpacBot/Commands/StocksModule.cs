using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.Json;

using AlphaVantage.Net.Core.Client;
using AlphaVantage.Net.Stocks;
using AlphaVantage.Net.Stocks.Client;
using AlphaVantage.Net.Core;
using AlphaVantage.Net.Common;

using Discord.Commands;

namespace DiscordSpacBot.Commands
{
    public class StocksModule :  ModuleBase<SocketCommandContext>
    {

        private readonly StocksClient _stocksClient;
        private readonly AlphaVantageClient _client;
        public StocksModule(AlphaVantageClient avClient)
        {
            _stocksClient = avClient.Stocks();
            _client = avClient;
        }

        [Command("stock")]
        [Summary("Get information about a stock.")]
        public async Task GetStockAsync([Summary("The stock's ticker.")] string ticker)
        {
            // Add error handling
            var globalQuote = _stocksClient.GetGlobalQuoteAsync(ticker).Result;

            var timeSeries = _stocksClient.GetTimeSeriesAsync(ticker, AlphaVantage.Net.Common.Intervals.Interval.Monthly).Result;
            long totalVolume = 0;
            foreach(var data in timeSeries.DataPoints)
            {
                totalVolume += data.Volume;
            }
            var avgVolume = totalVolume / timeSeries.DataPoints.Count;

            string response = ">>> __**" + globalQuote.Symbol + "**__\n";
            
            response += string.Format("__**{0}**__\n {1}\n \n", "Price:", globalQuote.Price.ToString("c"));
            response += string.Format("__**{0}**__\n {1}\n \n", "Open:", globalQuote.OpeningPrice.ToString("c"));
            response += string.Format("__**{0}**__\n {1}\n \n", "High:", globalQuote.HighestPrice.ToString("c"));
            response += string.Format("__**{0}**__\n {1}\n \n", "Low:", globalQuote.LowestPrice.ToString("c"));
            response += string.Format("__**{0}**__\n {1}\n \n", "Volume:", globalQuote.Volume.ToString("N0"));
            response += string.Format("__**{0}**__\n {1}\n \n", "Average Volume (Last Month):", avgVolume.ToString("N0"));

            await Context.Channel.SendMessageAsync(response);
        }


        [Command("chart")]
        [Summary("Get a chart of the stock.")]
        public async Task GetChartAsync([Summary("The stock's ticker.")] string ticker)
        {
            string script = @"G:\Spacs\Charts\scripts\create_charts.py " + ticker;
            string python = @"C:\Python38\python.exe";

            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(python, script)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            p.Start();

            string output = p.StandardOutput.ReadToEnd();
            string error = p.StandardError.ReadToEnd();

            p.WaitForExit();
            Console.WriteLine(error);

            await Context.Channel.SendFileAsync(@"G:\Spacs\Charts\" + ticker + @"\daily_chart.png", "Daily Info Chart for " + ticker);
        }

        [Command("quote")]
        [Summary("Get a quote of the specified stock for today.")]
        public async Task GetStockQuote([Summary("The stock's symbol.")] string symbol)
        {
            try
            {
                var globalQuote = await _stocksClient.GetGlobalQuoteAsync(symbol);
                var query = new Dictionary<string, string>() { { "symbol", symbol } };
                var overviewResult = await _client.RequestParsedJsonAsync(ApiFunction.OVERVIEW, query);
                var fundamentals = overviewResult.RootElement;

                var price = globalQuote.Price;
                var open = globalQuote.OpeningPrice;
                var high = globalQuote.HighestPrice;
                var low = globalQuote.LowestPrice;
                var volume = globalQuote.Volume;
                var change = (price / open - 1);

                var name = symbol;
                JsonElement value;
                if (fundamentals.TryGetProperty("Name", out value))
                    name = value.GetString();

                string response = "```" + fundamentals.GetProperty("Name")  + " \n";
                response += string.Format("{0,-10} | {1,-12} {2,-8}\n", "Price", price.ToString("c"), " (" + change.ToString("P") + ")");
                response += string.Format("{0,-10} | {1, -12} \n", "Open", open.ToString("c"));
                response += string.Format("{0,-10} | {1, -12} \n", "High", high.ToString("c"));
                response += string.Format("{0,-10} | {1, -12} \n", "Low", low.ToString("c"));
                response += string.Format("{0,-10} | {1, -12} \n", "Volume", volume.ToString("N0"));
                response += "```";

                await Context.Channel.SendMessageAsync(response);

            }
            catch (Exception e) 
            {
                Console.WriteLine(e.ToString());
                await Context.Channel.SendMessageAsync("ERROR: Could not find quote for symbol " + symbol + ".");
            }
        }

        [Command("info")]
        [Summary("Get the fundamental information for the stock.")]
        public async Task GetStockInfo([Summary("The stock's symbol")] string symbol)
        {
            try
            {
                var globalQuote = await _stocksClient.GetGlobalQuoteAsync(symbol);
                var query = new Dictionary<string, string>() { { "symbol", symbol } };
                var overviewResult = await _client.RequestParsedJsonAsync(ApiFunction.OVERVIEW, query);
                var fundamentals = overviewResult.RootElement;

                var price = globalQuote.Price;

                var name = symbol;
                JsonElement value;
                if (fundamentals.TryGetProperty("Name", out value))
                    name = value.GetString();

                string response = "```" + fundamentals.GetProperty("Name") + " \n";
                response += string.Format("{0,-16} | {1,-32}\n", "Price", price.ToString("c"));
                response += string.Format("{0,-16} | {1,-32}\n", "Exchange", fundamentals.GetProperty("Exchange"));
                response += string.Format("{0,-16} | {1,-32}\n", "Sector" , fundamentals.GetProperty("Sector"));
                response += string.Format("{0,-16} | ${1,-31}\n", "52 Week High", fundamentals.GetProperty("52WeekHigh"));
                response += string.Format("{0,-16} | ${1,-31}\n", "52 Week Low", fundamentals.GetProperty("52WeekLow"));
                response += "```";

                await Context.Channel.SendMessageAsync(response);

                response = fundamentals.GetProperty("Description") + "";
                int sizeLimit = 1500;
                while (response.Length > sizeLimit)
                {
                    while (response.Substring(sizeLimit, 1) != " ")
                        sizeLimit--;
                    await Context.Channel.SendMessageAsync("```" + response.Substring(0, sizeLimit) + "```");
                    response = response.Substring(sizeLimit, response.Length-sizeLimit);
                }
                await Context.Channel.SendMessageAsync("```" + response + "```");

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                await Context.Channel.SendMessageAsync("ERROR: Error finding info for " + symbol + ".");
            }
        }

    }
}

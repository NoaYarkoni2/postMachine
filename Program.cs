using HttpCaller;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System;
namespace MyProject;
class Program
{
    static async Task Main(string[] args)
    {
        var config = await ReadConfigFromFileAsync("config.json");

        HttpClient client = new HttpClient();
        int totalCalls = 0;
        int successfulCalls = 0;
        int failedCalls = 0;

        CancellationTokenSource cts = new CancellationTokenSource();
        cts.CancelAfter(config.Duration);

        var startTime = DateTime.Now;

        var tasks = new Task[config.NumberOfThreads];

        for (int i = 0; i < config.NumberOfThreads; i++)
        {
            tasks[i] = Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    Interlocked.Increment(ref totalCalls);
                    try
                    {
                        var response = await SendHttpRequest(client, config.Url, config.Method, config.JsonPayload);
                        if (response.IsSuccessStatusCode)
                        {
                            Interlocked.Increment(ref successfulCalls);
                            Console.WriteLine($"Success: {response.StatusCode}");
                        }
                        else
                        {
                            Interlocked.Increment(ref failedCalls);
                            Console.WriteLine($"Failure: {response.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref failedCalls);
                        Console.WriteLine($"Exception: {ex.Message}");

                    }

                    await Task.Delay(config.Interval);
                }
            });
        }

        await Task.WhenAll(tasks);

        Console.WriteLine($"Start Time: {startTime}");
        Console.WriteLine($"End Time: {DateTime.Now}");
        Console.WriteLine("Report:");
        Console.WriteLine($"Total Calls: {totalCalls}");
        Console.WriteLine($"Successful Calls: {successfulCalls}");
        Console.WriteLine($"Failed Calls: {failedCalls}");
        Console.WriteLine($"Configuration Used: Duration: {(config.Duration)/1000} sec, Interval: {config.Interval}, NumberOfThreads: {config.NumberOfThreads}");
    }

    static async Task<HttpResponseMessage> SendHttpRequest(HttpClient client, string url, string method, string jsonPayload)
    {
        HttpRequestMessage request = new HttpRequestMessage
        {
            RequestUri = new Uri(url),
            Method = new HttpMethod(method),
            Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
        };

        return await client.SendAsync(request);
    }

    static async Task<Config> ReadConfigFromFileAsync(string filePath)
    {
        string jsonString = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<Config>(jsonString);
    }
}






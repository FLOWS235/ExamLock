using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var http = new HttpClient();
        http.BaseAddress = new Uri("http://localhost:5000");

        string hwid = HardwareId.Get();
        Console.WriteLine($"HWID: {hwid}");

        while (true)
        {
            Console.Write("Username: ");
            string username = Console.ReadLine() ?? "";
            Console.Write("Password: ");
            string password = ReadPassword();

            var loginReq = new { Hwid = hwid, Username = username, Password = password };
            var resp = await http.PostAsync("/login", JsonContent(loginReq));
            if (resp.IsSuccessStatusCode)
            {
                Console.WriteLine("Login successful.");
                break;
            }
            var text = await resp.Content.ReadAsStringAsync();
            if (text.Contains("Device not registered"))
            {
                Console.Write("Device not registered. Enter admin key: ");
                string adminKey = Console.ReadLine() ?? "";
                var regReq = new { Hwid = hwid, AdminKey = adminKey };
                var regResp = await http.PostAsync("/register", JsonContent(regReq));
                if (regResp.IsSuccessStatusCode)
                {
                    Console.WriteLine("Device registered. Try logging in again.");
                }
                else
                {
                    Console.WriteLine("Registration failed.");
                }
            }
            else
            {
                Console.WriteLine("Login failed.");
            }
        }

        var questionsResp = await http.GetAsync("/questions");
        var questionsJson = await questionsResp.Content.ReadAsStringAsync();
        var questions = JsonSerializer.Deserialize<string[]>(questionsJson) ?? Array.Empty<string>();
        var answers = new string[questions.Length];

        var start = DateTime.UtcNow;
        var minDuration = TimeSpan.FromHours(1);
        var maxDuration = TimeSpan.FromHours(4);

        for (int i = 0; i < questions.Length; i++)
        {
            Console.WriteLine($"Question {i + 1}: {questions[i]}");
            Console.Write("Answer: ");
            answers[i] = Console.ReadLine() ?? "";
        }

        while ((DateTime.UtcNow - start) < minDuration)
        {
            Console.WriteLine("Minimum exam time not reached. Please wait...");
            await Task.Delay(5000);
        }

        Console.WriteLine("Exam finished. Answers submitted.");
        // In a real implementation, submit answers to server and log events.
    }

    static HttpContent JsonContent(object obj) =>
        new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

    static string ReadPassword()
    {
        var password = new StringBuilder();
        ConsoleKeyInfo key;
        while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
        {
            if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password.Remove(password.Length - 1, 1);
                Console.Write("\b \b");
            }
            else
            {
                password.Append(key.KeyChar);
                Console.Write("*");
            }
        }
        Console.WriteLine();
        return password.ToString();
    }
}

static class HardwareId
{
    public static string Get()
    {
        return Environment.MachineName; // Placeholder for real hardware ID
    }
}

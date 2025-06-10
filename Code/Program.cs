using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Monitor de Temperatura");
        Console.WriteLine("---------------------\n");

        // Solicitar unidade de temperatura
        string unidade;
        do
        {
            Console.Write("Escolha a unidade de temperatura (celsius/kelvin/fahrenheit): ");
            unidade = Console.ReadLine()?.ToLower();
        } while (unidade != "celsius" && unidade != "kelvin" && unidade != "fahrenheit");

        // Solicitar intervalo
        int intervalo;
        do
        {
            Console.Write("Informe o intervalo em segundos para cada leitura: ");
        } while (!int.TryParse(Console.ReadLine(), out intervalo) || intervalo <= 0);

        Console.WriteLine("\nIniciando monitoramento...");
        Console.WriteLine("Pressione Ctrl+C para encerrar.\n");

        double? ultimaTemperatura = null;
        var httpClient = new HttpClient();
        
        // Configurar tratamento de Ctrl+C
        Console.CancelKeyPress += (sender, e) =>
        {
            Console.WriteLine("\nEncerrando monitoramento de temperatura.");
            Environment.Exit(0);
        };

        while (true)
        {
            try
            {
                string url = $"http://localhost:5000/temperatura/{unidade}";
                HttpResponseMessage response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var temperaturaAtual = JsonSerializer.Deserialize<double>(jsonResponse);

                string horario = DateTime.Now.ToString("HH:mm:ss");
                Console.Write($"[{horario}] Temperatura: {temperaturaAtual:F2} °{unidade[0].ToString().ToUpper()} → ");

                if (ultimaTemperatura.HasValue)
                {
                    if (temperaturaAtual > ultimaTemperatura)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("SUBIU");
                    }
                    else if (temperaturaAtual < ultimaTemperatura)
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("DESCEU");
                    }
                    else
                    {
                        Console.ResetColor();
                        Console.WriteLine("SEM ALTERAÇÃO");
                    }
                }
                else
                {
                    Console.ResetColor();
                    Console.WriteLine("PRIMEIRA LEITURA");
                }

                ultimaTemperatura = temperaturaAtual;
                Console.ResetColor();
            }
            catch (HttpRequestException)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Erro ao obter leitura");
                Console.ResetColor();
            }
            catch (JsonException)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Resposta inválida");
                Console.ResetColor();
            }

            await Task.Delay(intervalo * 1000);
        }
    }
}

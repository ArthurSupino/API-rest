using System;
using System.Net;
using System.Text.Json;

class Server
{
    public static void Start(string[] args)
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:5000/temperatura/");
        listener.Start();
        Console.WriteLine("Servidor iniciado em http://localhost:5000. Aguardando requisições...");

        while (true)
        {
            try
            {
                var context = listener.GetContext();
                var request = context.Request;
                var response = context.Response;

                string unidade = request.Url?.Segments[^1].TrimEnd('/').ToLower();
                double temperaturaCelsius = new Random().Next(-10, 40); // Temperatura simulada

                double temperatura = unidade switch
                {
                    "celsius" => temperaturaCelsius,
                    "kelvin" => temperaturaCelsius + 273.15,
                    "fahrenheit" => (temperaturaCelsius * 9 / 5) + 32,
                    _ => double.NaN
                };

                if (double.IsNaN(temperatura))
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    byte[] errorMessage = System.Text.Encoding.UTF8.GetBytes("Unidade inválida.");
                    response.OutputStream.Write(errorMessage, 0, errorMessage.Length);
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.OK;
                    string jsonResponse = JsonSerializer.Serialize(temperatura);
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(jsonResponse);
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                }

                response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no servidor: {ex.Message}");
            }
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;

class Program
{
    private static IConfiguration _configuration;
    private static string speechKey = "API_KEY";
    private static string speechRegion = "brazilsouth";

    private static readonly string[] Voices = new[]
    {
        "pt-BR-FranciscaNeural",
        "pt-BR-AntonioNeural",
        "pt-BR-BrendaNeural",
        "pt-BR-DonatoNeural",
        "pt-BR-ElzaNeural",
        "pt-BR-FabioNeural",
        "pt-BR-GiovannaNeural",
        "pt-BR-HumbertoNeural",
        "pt-BR-JulioNeural",
        "pt-BR-LeilaNeural",
        "pt-BR-LeticiaNeural",
        "pt-BR-ManuelaNeural",
        "pt-BR-NicolauNeural",
        "pt-BR-ThalitaNeural",
        "pt-BR-ValerioNeural",
        "pt-BR-YaraNeural"
    };

    static async Task Main(string[] args)
    {
        // Carregar as configurações
        LoadConfiguration();

        var apiKey = _configuration["ApiKey"];
        var serialPortName = _configuration["SerialPortName"];
        var callSign = _configuration["CallSign"];
        var cities = File.ReadAllLines("cities.txt");

        // Carregar mensagem personalizada
        var customMessage = LoadCustomMessage();

        Console.WriteLine("Configurações carregadas:");
        Console.WriteLine($"API Key: {apiKey}");
        Console.WriteLine($"Serial Port Name: {serialPortName}");
        Console.WriteLine($"Call Sign: {callSign}");

        using (var serialPort = new SerialPort(serialPortName, 9600))
        {
            try
            {
                serialPort.Open();
                Console.WriteLine($"Porta serial {serialPortName} aberta.");

                var city = cities[new Random().Next(cities.Length)];
                var parts = city.Split(',');
                var cityName = parts[0];
                var lat = parts[1];
                var lon = parts[2];

                Console.WriteLine($"Consultando o tempo para {cityName}...");
                Console.WriteLine($"Latitude: {lat}, Longitude: {lon}");

                var weatherData = await GetWeatherDataAsync(lat, lon, apiKey);
                var forecastData = await GetForecastDataAsync(lat, lon, apiKey);

                if (weatherData != null)
                {
                    Console.WriteLine("Dados do tempo recebidos com sucesso.");

                    var tempKelvin = weatherData["main"]["temp"]?.Value<double>() ?? double.NaN;
                    var description = weatherData["weather"]?[0]["description"]?.Value<string>() ?? "sem descrição disponível";
                    var humidity = weatherData["main"]["humidity"]?.Value<int>() ?? 0;
                    var pressure = weatherData["main"]["pressure"]?.Value<int>() ?? 0;
                    var windSpeed = weatherData["wind"]["speed"]?.Value<double>() ?? 0;
                    var windDeg = weatherData["wind"]["deg"]?.Value<int>() ?? 0;
                    var clouds = weatherData["clouds"]["all"]?.Value<int>() ?? 0;
                    var rain = forecastData?["list"]?[0]["rain"]?["3h"]?.Value<double>() ?? 0;

                    var tempCelsius = tempKelvin - 273.15;
                    var tempFormatted = tempCelsius.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture).Replace('.', ',');

                    var voice = Voices[new Random().Next(Voices.Length)]; // Seleciona uma voz aleatória

                    var message = $"{callSign} Informa: A temperatura atual em {cityName} é de {tempFormatted} graus Celsius. " +
                                $"Condição Atual: {description}. A umidade está em {humidity}%. " +
                                $"A pressão atmosférica é de {pressure} hPa. A velocidade do vento é de {windSpeed} m/s, com uma direção de {windDeg} graus. " +
                                $"As condições de nuvens são de {clouds}%. A previsão indica uma possível chuva de {rain} mm nas próximas horas. " +
                                $"{customMessage} " +
                                $"Emissão Piloto de {callSign}, localizada em Golf Golf Cinquenta e Dois Quebec Éco, Palhoça, Santa Catarina. " +
                                $"Geração da previsão do tempo com Tecnologia Microsoft Azure e OpenWeatherMap.";

                    var tempAudioFile = "temp.wav";
                    Console.WriteLine("Gerando áudio com Azure Speech...");

                    // Gerar o áudio usando a API de fala do Azure via API REST
                    await GenerateSpeechAzure(message, tempAudioFile, voice);

                    Console.WriteLine("Áudio gerado.");

                    // Acionar o RTS
                    serialPort.RtsEnable = true;
                    Console.WriteLine("Sinal RTS ativado.");

                    // Reproduzir o áudio com aplay no Linux
                    var process = PlayAudioLinux(tempAudioFile);
                    
                    Console.WriteLine("Áudio reproduzido.");

                    // Aguardar até o áudio terminar de tocar
                    while (!process.HasExited)
                    {
                        await Task.Delay(500); // Verifica a cada 500ms
                    }

                    // Desativar o RTS após o áudio ter terminado
                    serialPort.RtsEnable = false;
                    Console.WriteLine("Sinal RTS desativado.");
                }
                else
                {
                    Console.WriteLine("Não foi possível obter os dados do tempo.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao abrir a porta serial ou processar os dados: {ex.Message}");
            }
            finally
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                    Console.WriteLine($"Porta serial {serialPortName} fechada.");
                }
            }
        }
    }

    private static void LoadConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        _configuration = builder.Build();
    }

    private static async Task<JObject> GetWeatherDataAsync(string lat, string lon, string apiKey)
    {
        using (var httpClient = new HttpClient())
        {
            var url = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={apiKey}&lang=pt_br";
            Console.WriteLine($"URL da API de tempo: {url}");
            try
            {
                var response = await httpClient.GetStringAsync(url);
                return JObject.Parse(response);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erro ao obter dados do tempo: {ex.Message}");
                return null;
            }
        }
    }

    private static async Task<JObject> GetForecastDataAsync(string lat, string lon, string apiKey)
    {
        using (var httpClient = new HttpClient())
        {
            var url = $"https://api.openweathermap.org/data/2.5/forecast?lat={lat}&lon={lon}&appid={apiKey}&lang=pt_br";
            Console.WriteLine($"URL da API de previsão: {url}");
            try
            {
                var response = await httpClient.GetStringAsync(url);
                return JObject.Parse(response);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erro ao obter dados de previsão: {ex.Message}");
                return null;
            }
        }
    }

    private static string LoadCustomMessage()
    {
        string customMessageFilePath = "custom_message.txt";
        if (File.Exists(customMessageFilePath))
        {
            return File.ReadAllText(customMessageFilePath);
        }
        else
        {
            Console.WriteLine("Arquivo de mensagem personalizada não encontrado. Usando mensagem padrão.");
            return string.Empty;
        }
    }

    private static async Task GenerateSpeechAzure(string message, string outputFile, string voice)
    {
        var token = await FetchTokenAsync();
        var url = $"https://{speechRegion}.tts.speech.microsoft.com/cognitiveservices/v1";

        using (var httpClient = new HttpClient())
        {
            // Configuração dos headers
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "YourApplicationName");

            var ssml = $@"
                <speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='pt-BR'>
                    <voice name='{voice}'>{message}</voice>
                </speak>";

            var content = new StringContent(ssml, System.Text.Encoding.UTF8, "application/ssml+xml");

            // Adicionar o cabeçalho X-MICROSOFT-OutputFormat
            content.Headers.Add("X-MICROSOFT-OutputFormat", "riff-8khz-16bit-mono-pcm");

            var response = await httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            using (var audioStream = await response.Content.ReadAsStreamAsync())
            using (var fileStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
            {
                await audioStream.CopyToAsync(fileStream);
            }
        }
    }

    private static async Task<string> FetchTokenAsync()
    {
        var url = $"https://{speechRegion}.api.cognitive.microsoft.com/sts/v1.0/issueToken";
        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", speechKey);
            var response = await httpClient.PostAsync(url, null);
            return await response.Content.ReadAsStringAsync();
        }
    }

    private static Process PlayAudioLinux(string filePath)
    {
        var process = new Process();
        process.StartInfo.FileName = "aplay";
        process.StartInfo.Arguments = filePath;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();
        return process;
    }
}

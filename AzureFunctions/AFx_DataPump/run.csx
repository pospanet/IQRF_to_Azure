using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

private const string BaseUrl = @"https://cloud.iqrf.org/";
private const string Path = @"api/api.php?";
private const string Parameters = @"ver=2&uid=<user ID>&gid=<Gateway ID>&cmd=dnld&new=1";
private const string Signature = @"&signature=";
private const string ApiKey = "<API key>";

public static void Run(TimerInfo myTimer, ICollector<string> outputQueueItems, TraceWriter log)
{
    log.Info($"DataPump function executed at: {DateTime.Now}"); 
    string ipAddress = GetPublicIpAddress();  
    log.Info($"DataPump function IP address: {ipAddress}");  
    string data = GetData(ipAddress);
    log.Info($"DataPump function get data: {data}");   
    string[] allLines = data.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
    allLines = allLines.Select(line => line.Trim()).ToArray();
    string[] allDataLines = allLines.Where(s => s.Split(';').Length.Equals(4) && (s.Split(';')[2]).Length.Equals(56)).ToArray();
    log.Info($"DataPump function data lines parsed: {allDataLines.Length}");
    foreach(string line in allDataLines)
    {
        outputQueueItems.Add(line);
    }
}

private static string GetData(string ip)
{
    HttpClient client = new HttpClient {BaseAddress = new Uri(BaseUrl)};
    int unixTimestamp = (int) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds/600;
    MD5 hash = MD5.Create();
    string stringData = string.Concat(Parameters, '|', ApiKey, '|', ip, '|', unixTimestamp);
    byte[] hashData = hash.ComputeHash(Encoding.ASCII.GetBytes(stringData));


    StringBuilder sb = new StringBuilder();
    foreach (byte t in hashData)
    {
        sb.Append(t.ToString("X2"));
    }
    string hashString = sb.ToString();

    byte[] asciiBytes = Encoding.ASCII.GetBytes(stringData);
    byte[] hashedBytes = hash.ComputeHash(asciiBytes);
    string hashedString = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();

    string reguestPath = string.Concat(Path, Parameters, Signature, hashedString);

    Task<HttpResponseMessage> task = client.GetAsync(reguestPath);
    Task.WaitAll(task);
    HttpResponseMessage response = task.Result;
    Task<string> task2 = response.Content.ReadAsStringAsync();
    Task.WaitAll(task2);
    return task2.Result;
}

private const string CheckIpBaseAddress = "http://checkip.dyndns.org";

public static string GetPublicIpAddress()
{
    HttpClient client = new HttpClient();
    client.BaseAddress = new Uri(CheckIpBaseAddress);
    Task<HttpResponseMessage> task = client.GetAsync(string.Empty);
    Task.WaitAll(task);
    Task<string> task2 = task.Result.Content.ReadAsStringAsync();
    string[] a = task2.Result.Split(':');
    string a2 = a[1].Substring(1);
    string[] a3 = a2.Split('<');
    return a3[0];
}
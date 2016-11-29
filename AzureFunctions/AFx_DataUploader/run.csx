using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Globalization;

static string[] dateTimeFormats = {"yyyy-M-d H:mm:ss", "yyyy-MM-dd HH:mm:ss"};

public static void Run(string myQueueItem, out string outputEventHubMessage, TraceWriter log)
{
    log.Info($"DataUploader function processed: {myQueueItem}");
    CustomerData customerData = new CustomerData(myQueueItem);
    outputEventHubMessage = JsonConvert.SerializeObject(customerData);
}

public class CustomerData
{
    public CustomerData(string rawData)
    {
    }
}
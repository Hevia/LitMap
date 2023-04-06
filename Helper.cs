using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;


public class Helpers
{
    public List<string> ParseString(string input)
    {
        List<string> output = new List<string>();
        string[] items = input.Split(',');

        foreach (string item in items)
        {
            output.Add(item);
        }

        return output;
    }

    public async Task<JObject> FetchPapersAsync(string query)
    {
        string baseUrl = "https://api.semanticscholar.org/graph/v1/paper/search?query=";
        string fields = "&fields=title,abstract";
        string url = $"{baseUrl}{Uri.EscapeDataString(query)}{fields}";

        using HttpClient httpClient = new HttpClient();
        string json = await httpClient.GetStringAsync(url);
        JObject jsonData = JObject.Parse(json);

        return jsonData;
    }

    public string ConcatenateStrings(List<string> stringList)
    {
        return string.Join('+', stringList);
    }

    public string ConcatenateStringsWithNewlines(List<string> stringList)
    {
        return string.Join(Environment.NewLine, stringList);
    }
}
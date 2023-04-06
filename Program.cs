using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.KernelExtensions;
using Microsoft.SemanticKernel.Orchestration;
using Newtonsoft.Json.Linq;

var kernel = Kernel.Builder.Build();
var helpers = new Helpers();

kernel.Config.AddOpenAITextCompletionService("davinci", "text-davinci-003", "");

// Import our Semantic Skills
var keywordSkills = kernel.ImportSemanticSkillFromDirectory("Skills", "Keywords");
var paperSkills = kernel.ImportSemanticSkillFromDirectory("Skills", "Papers");
var generateSkills = kernel.ImportSemanticSkillFromDirectory("Skills", "Generate");

// Init our context with the research question
string userQuestion = @"
NLP interfaces for scientific chatbots
";

string userQuestion2 = @"
Population genetics for brown rockfish
";

var userContext = new ContextVariables();
userContext.Set("INPUT", userQuestion2);

var keywords = await kernel.RunAsync(userContext, keywordSkills["IdentifyKeywords"]);
userContext.Set("KEYWORDS", keywords.ToString());
Console.WriteLine($"Keywords: {keywords}\r\n");

// var relatedKeywords = await kernel.RunAsync(userContext, keywordSkills["RelatedKeywords"]);
// userContext.Set("RELATED_KEYWORDS", relatedKeywords.ToString());
// Console.WriteLine($"Related Keywords: {relatedKeywords}\r\n");

List<string> paperSummaries = new List<string>();

List<string> keywords_parsed = helpers.ParseString(userContext["KEYWORDS"]);
string keyword_query = helpers.ConcatenateStrings(keywords_parsed);

var jsonData = await helpers.FetchPapersAsync(keyword_query);
JArray? results = jsonData["data"] as JArray;

//JToken paperJsonList;

//if (jsonData.TryGetValue("data", out paperJsonList))
//{
//    results = (JArray?)paperJsonList;
//    Console.WriteLine($"Abstract: {}");
//}
//else
//{
//    Console.WriteLine($"Abstract not found");
//}

//for (int i = 0; i < results.Count; i++)
for (int i = 0; i < 4; i++)
{
    JObject paperData = (JObject)results[i];
    JToken abstractToken;

    if (paperData.TryGetValue("abstract", out abstractToken))
    {
        string abstractText = abstractToken.ToString();

        if (string.IsNullOrEmpty(abstractText))
        {
            Console.WriteLine("Abstract is empty or null.");
        }
        else
        {
            userContext.Set("ABSTRACT", abstractText);
            var summary = await kernel.RunAsync(userContext, paperSkills["SummarizePaper"]);            
            paperSummaries.Add(summary.ToString());

            // Console.WriteLine("====================================");
            // Console.WriteLine("Abstract: " + abstractText);
            // Console.WriteLine("====================================");
            // Console.WriteLine("Abstract Summary: " + summary);
        }
    }
    else
    {
        Console.WriteLine($"Abstract not found");
    }
}

var paperSummariesString = helpers.ConcatenateStringsWithNewlines(paperSummaries);
userContext.Set("PAPER_SUMMARIES", paperSummariesString);
var followUp = await kernel.RunAsync(userContext, paperSkills["FollowUpQuestions"]);
Console.WriteLine("====================================");
Console.WriteLine("Follow-Up Questions: " + followUp);
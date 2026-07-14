using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System.ClientModel;
using dotnet_ai_chat;
using dotnet_ai_chat.Data;
using dotnet_ai_chat.Models;

IConfigurationRoot config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var credential = new ApiKeyCredential(config["GitHubModels:Token"]) ?? throw new InvalidDataException();
var options = new OpenAIClientOptions()
{
    Endpoint = new Uri("https://models.github.ai/inference")
};
options.AddPolicy(new HeaderCapturePolicy(), System.ClientModel.Primitives.PipelinePosition.PerCall);

IChatClient chatClient =
    new OpenAIClient(credential, options).GetChatClient("openai/gpt-4.1-mini").AsIChatClient();

using var db = new ChatDbContext();

var conversation = new Conversation { Title = "New Chat" };
db.conversations.Add(conversation);
try
{
    await db.SaveChangesAsync();
}
catch (Exception ex)
{
    Console.WriteLine("ERROR: " + ex.Message);
    if (ex.InnerException != null)
        Console.WriteLine("INNER: " + ex.InnerException.Message);
}

Console.WriteLine($"Started new conversation {conversation.Id}");

Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine("Chat Started. Type 'exit' to quit \n");
long totalInput = 0;
long totalOutput = 0;
List<ChatMessage> history = new List<ChatMessage>();
while (true)
{
    string text = "\nYou >>> ";
    WriteColored(text, ConsoleColor.Cyan);
    string prompt = Console.ReadLine();

    if (prompt.ToLower() == "exit") return;

    history.Add(new ChatMessage(ChatRole.User, prompt));

    try
    {
        ChatResponse chatResponse = await chatClient.GetResponseAsync(history);
        history.Add(new ChatMessage(ChatRole.Assistant, chatResponse.ToString()));

        totalInput += chatResponse.Usage?.InputTokenCount ?? 0;
        totalOutput += chatResponse.Usage?.OutputTokenCount ?? 0;

        text = $"\nAI >>> {chatResponse} ";
        WriteColored(text, ConsoleColor.Green);
        text = $"\n\n[Input : {chatResponse.Usage?.InputTokenCount} | Output: {chatResponse.Usage?.OutputTokenCount} | Remaining : {HeaderCapturePolicy.RemainingTokens}]\n";
        WriteColored(text, ConsoleColor.DarkYellow);
        text = $"\n [Sessions totals - Input : {totalInput}, Output: {totalOutput}]\n";
        WriteColored(text, ConsoleColor.DarkYellow);
    }
    catch (Exception exp)
    {
        Console.WriteLine($"Error : {exp.Message}");
        return;
    }


    static void WriteColored(string message, ConsoleColor color)
    {
        Console.ResetColor();
        Console.ForegroundColor = color;
        Console.Write(message);

    }
}






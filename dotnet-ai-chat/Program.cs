using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System.ClientModel;
using dotnet_ai_chat;
using DotnetAiChat.Core.Data;
using DotnetAiChat.Core.Models;
using Microsoft.EntityFrameworkCore;

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

//Resuming recent conversation if it does exist creating new one
Conversation? conversation = await db.conversations
    .Include(c => c.Messages)
    .OrderByDescending(c => c.UpdatedAt)
    .FirstOrDefaultAsync();
List<ChatMessage> history = new List<ChatMessage>();
if(conversation != null)
{
    //looping through every message and adding to history as well as displaying them
    Console.WriteLine($"Resuming Conversation : {conversation.Title} ({conversation.Title})");
    foreach(var message in conversation.Messages.OrderBy(m => m.Created))
    {
        var role = message.Role == "user" ? ChatRole.User : ChatRole.Assistant;
        history.Add(new ChatMessage(role, message.Content));

        var color = message.Role == "user" ? ConsoleColor.Cyan : ConsoleColor.Green;
        var chatMessage = message.Role == "user" ? "You >>> " : "AI >>> ";
        chatMessage += message.Content+"\n";
        WriteColored(chatMessage, color);
    }
}
else
{
    StartNewConversation(db);
}




Console.WriteLine($"Started new conversation {conversation.Id}");

Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine("Chat Started. Type 'exit' to quit \n");
long totalInput = 0;
long totalOutput = 0;
while (true)
{
    string text = "\nYou >>> ";
    WriteColored(text, ConsoleColor.Cyan);
    string prompt = Console.ReadLine();

    if (prompt.ToLower() == "exit") return;

    if(prompt.ToLower() == "new")
    {
        StartNewConversation(db);
        history.Clear();
        Console.WriteLine($"\n Started New Conversation: {conversation.Id}");
        continue;
    }

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

        //Saving User and AI Messages
        var UserMessage = new Message()
        {
            ConversationId = conversation.Id,
            Role = "user",
            Content = prompt!
        };

        var AIMessage = new Message()
        {
            ConversationId = conversation.Id,
            Role = "AI",
            ModelUsed = "openai/gpt-4.1-mini",
            Content = chatResponse.ToString()!,
            InputTokens = chatResponse.Usage?.InputTokenCount is long i ? (int)i : null,
            OutputTokens = chatResponse.Usage?.OutputTokenCount is long j ? (int)j : null,
        };

        db.messages.AddRange(UserMessage, AIMessage);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while saving messages in database" + ex.Message);
        }
        
    }
    catch (Exception exp)
    {
        Console.WriteLine($"Error : {exp.Message}");
        return;
    }


    
}

static void WriteColored(string message, ConsoleColor color)
{
    Console.ResetColor();
    Console.ForegroundColor = color;
    Console.Write(message);

}

async void StartNewConversation(ChatDbContext db)
{
    conversation = new Conversation { Title = "New Chat" };
    db.conversations.Add(conversation);
    try
    {
        await db.SaveChangesAsync();
    }
    catch (Exception ex) {
        Console.WriteLine($"Error occured while saving conversation data {ex.Message}");
    }
}






using DotnetAiChat.Core.Data;
using DotnetAiChat.Web.Components;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<ChatDbContext>();
builder.Services.AddSingleton<IChatClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var credential = new ApiKeyCredential(config["GitHubModels:Token"]
        ?? throw new InvalidOperationException("GitHub token not found in user secrets"));

    var options = new OpenAIClientOptions()
    {
        Endpoint = new Uri("https://models.github.ai/inference")
    };

    return new OpenAIClient(credential, options)
        .GetChatClient("openai/gpt-4o-mini")
        .AsIChatClient();
}

);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

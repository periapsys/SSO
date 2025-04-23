using PERI.SK.Application.Conversations.Queries;
using PERI.SK.Infrastructure;
using PERI.SK.Infrastructure.AzureOpenAi;
using PERI.SK.Infrastructure.Data;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

#if DEBUG
if (File.Exists("appsettings.Development.json"))
    builder.Configuration.AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true);
else
    builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
#else
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
#endif

builder.Services.ApplyInfrastructureServiceCollection(builder.Configuration);
builder.Services.ApplyDataServiceCollection(builder.Configuration);
builder.Services.ApplyAzureOpenAiServiceCollection(builder.Configuration);

builder.Services.AddMediatR(x => x.RegisterServicesFromAssemblyContaining<GetResponseQuery>());

builder.Services.AddMemoryCache();

// Build and run the application
var app = builder.Build();

var mediator = app.Services.GetRequiredService<IMediator>();

// Start the interactive loop
Console.WriteLine("Hi! Please enter you query.");
Console.WriteLine("Type 'exit' to close the application.\n");

while (true)
{
    // Read user input
    string userInput = Console.ReadLine()!;

    // Check if the input is "exit"
    if (userInput?.ToLower() == "exit")
    {
        break;
    }

    // Process the input (e.g., Converse with OpenAI)
    // Assuming you want to use OpenAiService to handle the query
    string response = await mediator.Send(new GetResponseQuery { Query = userInput!, Requestor = "user" });

    // Output the response
    Console.WriteLine("\nResponse: " + response + "\n");
}

// Exiting the program gracefully after 'exit' is typed
Console.WriteLine("Goodbye!");
Environment.Exit(0);  // This will immediately close the console application

await app.RunAsync();
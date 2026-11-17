using System.ClientModel;
using Microsoft.Extensions.AI;
using OpenAI;
using Stockroom.Core;
using Stockroom.Core.Services;
using Stockroom.Questions;

namespace Stockroom.Web.Questions;

public static class QuestionRegistration
{
    public static IServiceCollection AddOrderQuestions(
        this IServiceCollection services, IConfiguration config)
    {
        var options = config.GetSection("Questions")
            .Get<QuestionOptions>() ?? new QuestionOptions();
        services.AddSingleton(options);

        // The switch decides, not whether an address is filled in.
        if (!options.Enabled)
        {
            return services;
        }

        services.AddChatClient(_ => CreateChatClient(options));

        // "Last month" is a month of the business, not of the server.
        services.AddSingleton(sp => new QuestionReader(
            sp.GetRequiredService<IChatClient>(),
            new BusinessTimeProvider(
                sp.GetRequiredService<BusinessClock>().Zone),
            sp.GetRequiredService<ILogger<QuestionReader>>()));
        services.AddScoped<OrderQuestionService>();
        return services;
    }

    private static IChatClient CreateChatClient(
        QuestionOptions options)
    {
        OpenAIClientOptions clientOptions = new()
        {
            Endpoint = new Uri(options.Endpoint),
            // A local model on a CPU may first have to load.
            NetworkTimeout = TimeSpan.FromMinutes(3),
        };

        // A local server ignores the key, but the client needs one.
        ApiKeyCredential key = new(options.ApiKey ?? "not-used");
        return new OpenAIClient(key, clientOptions)
            .GetChatClient(options.Model)
            .AsIChatClient();
    }
}

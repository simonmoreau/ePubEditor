using Azure.AI.OpenAI;
using EpubCore;
using ePubEditor.Core.Models;
using ePubEditor.Core.Services;
using FuzzySharp;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.ClientModel;
using System.Diagnostics;
using System.Reflection;

namespace ePubEditor.Core
{
    public class Main
    {
        private readonly ServiceProvider _serviceProvider;

        public Main()
        {
            _serviceProvider = ConfigureServices(new ServiceCollection());
        }

        private ServiceProvider ConfigureServices(IServiceCollection services)
        {
            string baseUrl = "https://www.googleapis.com/books/v1/";
            services.AddHttpClient<IGoogleBook, GoogleBook>
                (client => client.BaseAddress = new Uri(baseUrl));

            services = AddAIServices(services);

            return services.BuildServiceProvider();
        }

        public IServiceCollection AddAIServices(IServiceCollection services)
        {
            string executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string configPath = Path.Combine(executingDirectory, "appsettings.json");

            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile(configPath, optional: true)
                .Build();

            AzureOpenAI? azureOpenAISettigns = config.GetSection(nameof(AzureOpenAI)).Get<AzureOpenAI>();

            string deploymentName = azureOpenAISettigns.ModelId;
            Uri endpoint = new Uri(azureOpenAISettigns.Endpoint);
            ApiKeyCredential apiKey = new ApiKeyCredential(azureOpenAISettigns.Key);

            IChatClient chatClient = new AzureOpenAIClient(
                        endpoint,
                        apiKey)
                    .AsChatClient(deploymentName)
                    .AsBuilder()
                    .UseFunctionInvocation()
                    .Build();

            services.AddChatClient(chatClient);

            return services;
        }

        public async Task Start()
        {
            //await GoogleSearch();

            EpubLister epubLister = new EpubLister();
            await epubLister.ListEpub();
        }




    }
}

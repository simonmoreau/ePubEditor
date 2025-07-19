using Azure.AI.OpenAI;
using ePubEditor.Core.Comics;
using ePubEditor.Core.Models;
using ePubEditor.Core.Services;
using FuzzySharp;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mscc.GenerativeAI;
using OpenAI.Chat;
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

        public T GetService<T>() where T : class
        {
            T service = _serviceProvider.GetRequiredService<T>();

            if (service == null)
            {
                throw new InvalidOperationException($"Service of type {typeof(T).Name} is not registered.");
            }

            return service;
        }

        private ServiceProvider ConfigureServices(IServiceCollection services)
        {
            string baseUrl = "https://www.googleapis.com/books/v1/";
            services.AddHttpClient<IGoogleBook, GoogleBook>
                (client => client.BaseAddress = new Uri(baseUrl));

            services = AddAIServices(services);

            services.AddSingleton<AIMetadataFetcher>();
            services.AddSingleton<ComicsRenamer>();
            services.AddSingleton<ComicVineWriter>();

            return services.BuildServiceProvider();
        }

        private IServiceCollection AddAIServices(IServiceCollection services)
        {
            // Build a configuration object from JSON file

            string? executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (string.IsNullOrEmpty(executingDirectory))
            {
                throw new InvalidOperationException("Executing directory cannot be determined.");
            }

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
                    .SetBasePath(executingDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

            IConfigurationRoot config = configurationBuilder.Build();

            ConfigureAIServices(services, config);

            services.Configure<Gemini>(options => config.GetSection(nameof(Gemini)).Bind(options));

            return services;
        }

        private void ConfigureAIServices(IServiceCollection services, IConfigurationRoot config)
        {
            // Configure AI services here
            AzureOpenAI? azureOpenAISettigns = config.GetSection(nameof(AzureOpenAI)).Get<AzureOpenAI>();

            string deploymentName = azureOpenAISettigns.ModelId;
            Uri endpoint = new Uri(azureOpenAISettigns.Endpoint);
            ApiKeyCredential apiKeyCredential = new ApiKeyCredential(azureOpenAISettigns.Key);

            IChatClient openAIClient = new AzureOpenAIClient(endpoint, apiKeyCredential)
                    .GetChatClient(deploymentName).AsIChatClient();

            IChatClient client = new ChatClientBuilder(openAIClient)
                .UseFunctionInvocation()
                .Build();

            services.AddSingleton<IChatClient>(client);
        }

        public async Task Start()
        {
            //await GoogleSearch();

            //EpubLister epubLister = new EpubLister();
            //await epubLister.ListEpub();

            AIMetadataFetcher? metatadataFetcher = _serviceProvider.GetService<AIMetadataFetcher>();
            await metatadataFetcher.FetchMetadataWithOpenAI();
        }




    }
}

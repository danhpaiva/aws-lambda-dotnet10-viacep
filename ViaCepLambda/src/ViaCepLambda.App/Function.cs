using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using ViaCepLambda.Domain.Interfaces;
using ViaCepLambda.Infrastructure.Services;
using ViaCepLambda.Domain.Models;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ViaCepLambda.App;

public class Function
{
    private readonly IViaCepService _viaCepService;

    // Construtor padrão que a AWS Lambda chama. Ele configura o container de DI.
    public Function()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        _viaCepService = serviceProvider.GetRequiredService<IViaCepService>();
    }

    // Construtor utilizado para testes unitários (Injeção de Mock)
    public Function(IViaCepService viaCepService)
    {
        _viaCepService = viaCepService;
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient<IViaCepService, ViaCepService>();
    }

    public async Task<Endereco?> FunctionHandler(string cepInput, ILambdaContext context)
    {
        context.Logger.LogLine($"Processando consulta de CEP: {cepInput}");

        if (string.IsNullOrWhiteSpace(cepInput))
        {
            context.Logger.LogLine("Aviso: CEP recebido está vazio ou nulo.");
            return null;
        }

        try
        {
            var endereco = await _viaCepService.ObterEnderecoPorCepAsync(cepInput);

            if (endereco == null)
                context.Logger.LogLine($"Nenhum endereço encontrado para o CEP: {cepInput}");

            return endereco;
        }
        catch (Exception ex)
        {
            context.Logger.LogLine($"Erro ao processar requisição: {ex.Message}");
            throw;
        }
    }
}
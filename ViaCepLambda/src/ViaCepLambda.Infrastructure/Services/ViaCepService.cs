using System.Net.Http.Json;
using ViaCepLambda.Domain.Interfaces;
using ViaCepLambda.Domain.Models;
using ViaCepLambda.Infrastructure.DTOS;

namespace ViaCepLambda.Infrastructure.Services
{
    public class ViaCepService : IViaCepService
    {
        private readonly HttpClient _httpClient;

        public ViaCepService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Endereco?> ObterEnderecoPorCepAsync(string cep)
        {
            // Higieniza o CEP para enviar apenas números
            var cepLimpo = new string(cep.Where(char.IsDigit).ToArray());

            if (cepLimpo.Length != 8)
                throw new ArgumentException("O CEP deve conter exatamente 8 dígitos.");

            var url = $"https://viacep.com.br/ws/{cepLimpo}/json/";

            try
            {
                var response = await _httpClient.GetFromJsonAsync<ViaCepResponse>(url);

                if (response == null || response.Erro == "true")
                    return null;

                return new Endereco
                {
                    Cep = response.Cep,
                    Logradouro = response.Logradouro,
                    Bairro = response.Bairro,
                    Localidade = response.Localidade,
                    Uf = response.Uf
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
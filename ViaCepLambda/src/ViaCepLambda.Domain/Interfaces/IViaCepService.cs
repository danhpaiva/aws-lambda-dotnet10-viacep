using ViaCepLambda.Domain.Models;

namespace ViaCepLambda.Domain.Interfaces
{
    public interface IViaCepService
    {
        Task<Endereco?> ObterEnderecoPorCepAsync(string cep);
    }
}
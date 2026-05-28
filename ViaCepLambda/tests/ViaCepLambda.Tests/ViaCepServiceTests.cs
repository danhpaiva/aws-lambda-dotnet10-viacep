using Amazon.Lambda.Core;
using Moq;
using ViaCepLambda.App;
using ViaCepLambda.Domain.Models;
using ViaCepLambda.Domain.Interfaces;

namespace ViaCepLambda.Tests
{
    public class ViaCepServiceTests
    {
        [Fact]
        public async Task FunctionHandler_DeveRetornarEndereco_QuandoCepForValido()
        {
            // Arrange
            var mockService = new Mock<IViaCepService>();
            var mockContext = new Mock<ILambdaContext>();
            var mockLogger = new Mock<ILambdaLogger>();

            // Configura o contexto da AWS para não estourar NullReferenceException ao usar o Logger
            mockContext.Setup(c => c.Logger).Returns(mockLogger.Object);

            var enderecoEsperado = new Endereco
            {
                Cep = "01001-000",
                Logradouro = "Praça da Sé",
                Bairro = "Sé",
                Localidade = "São Paulo",
                Uf = "SP"
            };

            mockService.Setup(s => s.ObterEnderecoPorCepAsync("01001000"))
                       .ReturnsAsync(enderecoEsperado);

            // Injeta o Mock pelo construtor de teste da Lambda
            var lambda = new Function(mockService.Object);

            // Act
            var resultado = await lambda.FunctionHandler("01001000", mockContext.Object);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal("Praça da Sé", resultado.Logradouro);
            Assert.Equal("SP", resultado.Uf);
        }
    }
}
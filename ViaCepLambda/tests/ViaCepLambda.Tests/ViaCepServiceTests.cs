using Amazon.Lambda.Core;
using Moq;
using ViaCepLambda.App;
using ViaCepLambda.Domain.Models;
using ViaCepLambda.Domain.Interfaces;

namespace ViaCepLambda.Tests
{
    public class ViaCepServiceTests
    {
        private readonly Mock<IViaCepService> _mockService;
        private readonly Mock<ILambdaContext> _mockContext;
        private readonly Mock<ILambdaLogger> _mockLogger;

        // Construtor do XUnit roda antes de CADA teste. Excelente para limpar os mocks.
        public ViaCepServiceTests()
        {
            _mockService = new Mock<IViaCepService>();
            _mockContext = new Mock<ILambdaContext>();
            _mockLogger = new Mock<ILambdaLogger>();

            // Configura o logger padrão para todos os cenários
            _mockContext.Setup(c => c.Logger).Returns(_mockLogger.Object);
        }

        [Fact]
        public async Task FunctionHandler_DeveRetornarEndereco_QuandoCepForValido()
        {
            // Arrange
            var enderecoEsperado = new Endereco
            {
                Cep = "01001-000",
                Logradouro = "Praça da Sé",
                Bairro = "Sé",
                Localidade = "São Paulo",
                Uf = "SP"
            };

            _mockService.Setup(s => s.ObterEnderecoPorCepAsync("01001000"))
                       .ReturnsAsync(enderecoEsperado);

            var lambda = new Function(_mockService.Object);

            // Act
            var resultado = await lambda.FunctionHandler("01001000", _mockContext.Object);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal("Praça da Sé", resultado.Logradouro);
            Assert.Equal("SP", resultado.Uf);
            _mockLogger.Verify(l => l.LogLine(It.Is<string>(s => s.Contains("Processando consulta"))), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task FunctionHandler_DeveRetornarNulo_QuandoCepForVazioOuNulo(string? cepInvalido)
        {
            // Arrange
            var lambda = new Function(_mockService.Object);

            // Act
            var resultado = await lambda.FunctionHandler(cepInvalido!, _mockContext.Object);

            // Assert
            Assert.Null(resultado);
            _mockLogger.Verify(l => l.LogLine(It.Is<string>(s => s.Contains("Aviso: CEP recebido está vazio"))), Times.Once);

            // Garante que o serviço sequer foi chamado (Economia de IO/Rede)
            _mockService.Verify(s => s.ObterEnderecoPorCepAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task FunctionHandler_DeveRetornarNulo_QuandoCepNaoForEncontradoNaApi()
        {
            // Arrange
            // Simulando o serviço retornando null (CEP não existe na base do ViaCep)
            _mockService.Setup(s => s.ObterEnderecoPorCepAsync("99999999"))
                       .ReturnsAsync((Endereco?)null);

            var lambda = new Function(_mockService.Object);

            // Act
            var resultado = await lambda.FunctionHandler("99999999", _mockContext.Object);

            // Assert
            Assert.Null(resultado);
            _mockLogger.Verify(l => l.LogLine(It.Is<string>(s => s.Contains("Nenhum endereço encontrado"))), Times.Once);
        }

        [Fact]
        public async Task FunctionHandler_DeveRelancarExcecao_QuandoServicoFalhar()
        {
            // Arrange
            // Simulando uma falha crítica de infraestrutura (Ex: Timeout ou queda da API externa)
            _mockService.Setup(s => s.ObterEnderecoPorCepAsync("01001000"))
                       .ThrowsAsync(new HttpRequestException("Erro de conexão com o servidor ViaCep"));

            var lambda = new Function(_mockService.Object);

            // Act & Assert
            // Garante que a Lambda não engole o erro e repassa a exceção para a AWS lidar/retentar
            var excecao_lancada = await Assert.ThrowsAsync<HttpRequestException>(() =>
                lambda.FunctionHandler("01001000", _mockContext.Object)
            );

            Assert.Equal("Erro de conexão com o servidor ViaCep", excecao_lancada.Message);
            _mockLogger.Verify(l => l.LogLine(It.Is<string>(s => s.Contains("Erro ao processar requisição"))), Times.Once);
        }
    }
}
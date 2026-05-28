# ⚡ AWS Lambda ViaCEP Integration - Clean Architecture

[![.NET 10](https://img.shields.io/badge/.NET-10.0-blueviolet.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![AWS Lambda](https://img.shields.io/badge/AWS-Lambda-orange.svg)](https://aws.amazon.com/lambda/)
[![Clean Architecture](https://img.shields.io/badge/Design-Clean%20Architecture-brightgreen.svg)]()
[![Tests](https://img.shields.io/badge/Tests-xUnit%20%2F%20Moq-blue.svg)]()

Este repositório serve como um guia prático e um modelo de inicialização (boilerplate) para o desenvolvimento de funções **AWS Lambda nativas em .NET 10**, estruturado sob os princípios da **Arquitetura Limpa (Clean Architecture)** e **Clean Code**. 

A aplicação consome a API externa do ViaCEP de forma totalmente desacoplada, utilizando inversão de dependência, injeção de dependência nativa no ciclo de vida da Lambda e testes unitários automatizados.

---

## 🏗️ Visão Geral da Arquitetura

Diferente de scripts monolíticos tradicionais de funções Serverless, este ecossistema foi dividido em camadas para garantir testabilidade isolada, facilidade de manutenção e blindagem do domínio contra mudanças em serviços de terceiros.

```text
       [ViaCepLambda.App] (Ponto de entrada AWS / Container DI)
               │
               ▼
  [ViaCepLambda.Infrastructure] ──(Implementa)──► [ViaCepLambda.Domain]
  (HTTP Client / DTOs do ViaCep)                  (Entidades e Contratos)

```

### 🧠 Decisões de Design Sênior Incorporadas

* **Null-Safety & Robusteza Local:** Para evitar falhas internas de serialização do *AWS .NET Mock Lambda Test Tool* (que lança `NullReferenceException` ao tentar processar retornos explicitamente nulos), a Lambda adota o padrão de retornar uma instância limpa/vazia do objeto `Endereco` em cenários de falha ou validação.
* **Desacoplamento de Contrato:** A infraestrutura mapeia o JSON externo via `ViaCepResponse` (DTO) e o traduz para a entidade de negócio `Endereco`, garantindo que se o ViaCEP mudar o nome de um campo amanhã, o Core da sua aplicação continuará intacto.
* **Injeção de Dependência Eficiente:** O construtor parameterizado permite que 100% da lógica de negócio seja mockada nos testes unitários, enquanto o construtor padrão resolve a árvore de dependências HTTP usando o ciclo de vida nativo do `Microsoft.Extensions.DependencyInjection`.

---

## 🛠️ Pré-requisitos

Para rodar e modificar este projeto, você precisará de:

1. **.NET 10 SDK** instalado.
2. **AWS .NET Mock Lambda Test Tool** (versão para .NET 10).
3. Uma IDE de sua preferência (VS Code, Visual Studio ou Rider).

Se ainda não tem a ferramenta de testes instalada globalmente, execute:

```bash
dotnet tool install --global Amazon.Lambda.TestTool --version 0.10.3

```

---

## 🚀 Como Executar e Testar Localmente

### 1. Clonar e Buildar

Primeiro, clone o repositório e compile a solução para gerar os artefatos necessários:

```bash
git clone [https://github.com/danhpaiva/aws-lambda-dotnet10-viacep.git](https://github.com/danhpaiva/aws-lambda-dotnet10-viacep.git)
cd aws-lambda-dotnet10-viacep
dotnet build

```

### 2. Iniciar a Ferramenta de Teste

Navegue até a pasta do projeto executável e inicialize o Mock Test Tool para o runtime do .NET 10:

```bash
cd src/ViaCepLambda.App
dotnet-lambda-test-tool-10.0

```

Isso abrirá uma interface web em seu navegador padrão no endereço `http://localhost:5050`.

### 3. Executar o Payload de Teste

Na interface gráfica do browser, configure os seguintes campos:

* **Function Handler:** `ViaCepLambda.App::ViaCepLambda.App.Function::FunctionHandler`
* **Function Input:** Selecione `Custom` e envie uma string contendo o CEP desejado:

```json
"01001000"

```

> 💡 **Dica de teste:** Experimente enviar uma string vazia `" "` para validar o mecanismo de *Null-Safety* retornando um objeto estruturado vazio com sucesso, em vez de quebrar a ferramenta com uma exceção de ponteiro nulo.

---

## 🧪 Executando os Testes Unitários

Para validar todos os comportamentos de forma isolada, sem realizar chamadas de rede reais na API do ViaCEP, execute na raiz do projeto:

```bash
dotnet test

```

A suíte de testes utiliza **xUnit** e **Moq** cobrindo:

* Retorno de endereço sob CEP válido.
* Resiliência com `[Theory]` e `[InlineData]` contra payloads nulos, vazios ou com espaços.
* Lançamento e tratamento correto de exceções de rede (`HttpRequestException`).

---

## 🚀 Deploy na AWS (via CLI)

Se você tiver o `Amazon.Lambda.Tools` instalado no seu CLI do .NET, o deploy pode ser feito com um único comando a partir da raiz do projeto:

```bash
dotnet lambda deploy-function NomeDaSuaLambda

```

Este comando lerá o arquivo `aws-lambda-tools-defaults.json` automaticamente para definir o runtime (`dotnet10`), a memória allocated (`512MB`), o timeout (`30s`) e o handler.

---

## 🧠 Otimizações de Compilação Incorporadas

* **`PublishReadyToRun = true`**: Reduz drasticamente o tempo de **Cold Start** da Lambda na AWS, compilando o código em formato nativo antes do upload.
* **`CopyLocalLockFileAssemblies = true`**: Garante que o Mock Test Tool local consiga rastrear todas as dependências do NuGet entre as camadas da Clean Architecture sem falhas de carregamento de tipo.

---

Feito com .NET 10 e 💜 por [Daniel](https://github.com/danhpaiva)
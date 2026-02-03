# Correção da Pasta .azurefunctions Ausente em Aplicação Azure Functions .NET Isolated

## Descrição do Objetivo
Os artefatos implantados não contêm a pasta `.azurefunctions`, que é obrigatória para o modelo de worker isolado do .NET. Isso foi causado por uma configuração incorreta do projeto que misturava os modelos in-process e isolated worker, além da falta do ponto de entrada `Program.cs`. O objetivo foi converter totalmente o projeto para uma configuração válida de .NET isolated worker.

> [!IMPORTANT]
> **Motivo das Alterações (Bug no Debian)**
> Estas alterações e a migração completa para o modelo Isolated Worker foram estritamente necessárias devido a um bug conhecido ao executar Azure Functions (modelo In-Process) no ambiente Debian. A mudança garante a compatibilidade do runtime e a geração correta dos artefatos de implantação necessários.

## Alterações Realizadas

### Configuração
#### [MODIFICADO] [validaCPF.csproj](validaCPF.csproj)
- Removido `Microsoft.NET.Sdk.Functions` (SDK In-process).
- Adicionado `<OutputType>Exe</OutputType>` ao grupo de propriedades.
- Adicionadas referências para `Microsoft.Azure.Functions.Worker`, `Microsoft.Azure.Functions.Worker.Sdk` e pacotes do Application Insights.

#### [MODIFICADO] [local.settings.json](local.settings.json)
- Removido a configuração `FUNCTIONS_INPROC_NET8_ENABLED`.
- Garantido que `FUNCTIONS_WORKER_RUNTIME` esteja definido como `dotnet-isolated`.

### Código
#### [NOVO] [Program.cs](Program.cs)
- Criado o ponto de entrada (`Main`) para o host do worker isolado.

#### [MODIFICADO] [validaCPF.cs](validaCPF.cs)
- Atualizado para usar injeção de dependência e tipos do worker isolado.
- Corrigido erro de compilação renomeando o método de validação para `IsCpfValid`.
- Corrigido lógica de validação de CPF e erro de desserialização JSON (substituindo `dynamic` por classe tipada).

## Plano de Verificação

### Testes Automatizados
1. Executar `dotnet build` no diretório do projeto.
2. Verificar a existência da pasta `.azurefunctions` no diretório de saída:
   ```bash
   ls -R bin/Debug/net8.0/.azurefunctions
   ```
3. Executar a função localmente para garantir que ela inicie sem erros:
   ```bash
   func start
   ```

# Specification — AssistenteDB MCP Server

## 1. Objetivo

Implementar um servidor **Model Context Protocol (MCP)** que exponha cada endpoint da API REST (`AssistenteDB.Api`) como uma **tool MCP**, permitindo que agentes de IA interajam diretamente com o sistema via protocolo padronizado.

---

## 2. Escopo

- Novo projeto: `AssistenteDB.Mcp` (.NET 10, console app)
- Transporte: **stdio** (padrão MCP para integração com clientes como Claude Desktop, VS Code, etc.)
- Estratégia de acesso a dados: **acesso direto à camada de domínio/repositórios** (sem proxy HTTP)
- Total de tools: **31 tools** mapeadas 1:1 com os endpoints existentes

---

## 3. Arquitetura

```
AssistenteDB.sln
├── src/
│   ├── Domain/            ← sem alterações
│   ├── Application/       ← sem alterações
│   ├── Data/              ← sem alterações
│   ├── Presentation/
│   │   ├── AssistenteDB.Api/    ← sem alterações
│   │   └── AssistenteDB.Mcp/   ← NOVO
│       ├── AssistenteDB.Mcp.csproj
│       ├── Program.cs
│       └── Tools/
│           ├── ArquivoTools.cs
│           ├── TipoArquivoTools.cs
│           ├── ProdutoTools.cs
│           ├── TipoProdutoTools.cs
│           ├── ProdutoVersaoTools.cs
│           ├── ItemTools.cs
│           └── ItemCustoTools.cs
```

### 3.1 Decisões Arquiteturais

| Decisão | Escolha | Justificativa |
|---|---|---|
| Transporte MCP | stdio | Compatível com todos os clientes MCP (Claude Desktop, VS Code, etc.) |
| Acesso a dados | Repositórios diretos | Evita hop de rede, reutiliza toda a camada Data existente |
| Organização de tools | Arquivo por controller | Espelha a estrutura da API, facilita manutenção |
| Serialização | System.Text.Json | Nativo no .NET 10, sem dependências extras |
| Configuração | `appsettings.json` + env vars | Mesmo padrão do projeto Api |

---

## 4. Novo Projeto — `AssistenteDB.Mcp.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="ModelContextProtocol" Version="0.*" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="10.0.*" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.*" />
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.*" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\Application\AssistenteDB.Application.csproj" />
    <ProjectReference Include="..\..\Data\AssistenteDB.Data.csproj" />
  </ItemGroup>
</Project>
```

---

## 5. Program.cs

```csharp
using AssistenteDB.Data.Context;
using AssistenteDB.Data.Repositories;
using AssistenteDB.Domain.Interfaces;
using AssistenteDB.Mcp.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;

var builder = Host.CreateApplicationBuilder(args);

// PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositórios
builder.Services.AddScoped<ITipoArquivoRepository, TipoArquivoRepository>();
builder.Services.AddScoped<ITipoProdutoRepository, TipoProdutoRepository>();
builder.Services.AddScoped<IArquivoRepository, ArquivoRepository>();
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IProdutoVersaoRepository, ProdutoVersaoRepository>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IItemCustoRepository, ItemCustoRepository>();
builder.Services.AddScoped<IItemArquivoRepository, ItemArquivoRepository>();
builder.Services.AddScoped<IProdutoArquivoRepository, ProdutoArquivoRepository>();

// MCP Server — registra todas as tools por varredura de assembly
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
```

---

## 6. Catálogo de Tools

### 6.1 Arquivos — `ArquivoTools.cs`

| Tool | Método | Parâmetros | Retorno |
|---|---|---|---|
| `download_arquivo` | GET `/api/arquivos/{id}/download` | `id: long` | base64 dos bytes + tipo |
| `delete_arquivo` | DELETE `/api/arquivos/{id}` | `id: long` | confirmação |

> **Nota:** `upload_arquivo` (POST multipart) **não** será exposta no MCP inicial. Upload de arquivos binários via MCP stdio requer tratamento especial de base64. Fica como **fora de escopo** desta versão.

---

### 6.2 Tipos de Arquivo — `TipoArquivoTools.cs`

| Tool | Método | Parâmetros | Retorno |
|---|---|---|---|
| `listar_tipos_arquivo` | GET `/api/tipos-arquivo` | — | `TipoArquivoDto[]` |
| `obter_tipo_arquivo` | GET `/api/tipos-arquivo/{id}` | `id: long` | `TipoArquivoDto` |

---

### 6.3 Produtos — `ProdutoTools.cs`

| Tool | Método | Parâmetros | Retorno |
|---|---|---|---|
| `listar_produtos` | GET `/api/produtos` | `page: int = 1`, `pageSize: int = 20`, `ativado: bool?`, `tipoProdutoId: long?` | paginado |
| `obter_produto` | GET `/api/produtos/{id}` | `id: long` | `ProdutoResponseDto` |
| `criar_produto` | POST `/api/produtos` | `tipoProdutoId: long`, `nome: string`, `descricao: string?`, `ativado: bool = true` | `ProdutoResponseDto` |
| `atualizar_produto` | PUT `/api/produtos/{id}` | `id: long`, `tipoProdutoId: long`, `nome: string`, `descricao: string?`, `ativado: bool` | `ProdutoResponseDto` |
| `deletar_produto` | DELETE `/api/produtos/{id}` | `id: long` | confirmação |

---

### 6.4 Tipos de Produto — `TipoProdutoTools.cs`

| Tool | Método | Parâmetros | Retorno |
|---|---|---|---|
| `listar_tipos_produto` | GET `/api/tipos-produto` | — | `TipoProdutoDto[]` |
| `obter_tipo_produto` | GET `/api/tipos-produto/{id}` | `id: long` | `TipoProdutoDto` |

---

### 6.5 Versões de Produto — `ProdutoVersaoTools.cs`

| Tool | Método | Parâmetros | Retorno |
|---|---|---|---|
| `listar_versoes_produto` | GET `/api/produtos/{produtoId}/versoes` | `produtoId: long` | `ProdutoVersaoResponseDto[]` |
| `obter_versao` | GET `/api/versoes/{id}` | `id: long` | `ProdutoVersaoResponseDto` |
| `criar_versao` | POST `/api/produtos/{produtoId}/versoes` | `produtoId: long`, `nome: string`, `numero: int` | `ProdutoVersaoResponseDto` |
| `atualizar_versao` | PUT `/api/versoes/{id}` | `id: long`, `nome: string`, `numero: int` | `ProdutoVersaoResponseDto` |
| `deletar_versao` | DELETE `/api/versoes/{id}` | `id: long` | confirmação |
| `vincular_arquivo_versao` | PUT `/api/versoes/{id}/arquivo` | `id: long`, `arquivoId: long` | confirmação |
| `desvincular_arquivo_versao` | DELETE `/api/versoes/{id}/arquivo` | `id: long` | confirmação |

---

### 6.6 Itens — `ItemTools.cs`

| Tool | Método | Parâmetros | Retorno |
|---|---|---|---|
| `listar_itens_versao` | GET `/api/versoes/{versaoId}/itens` | `versaoId: long` | `ItemResponseDto[]` |
| `obter_item` | GET `/api/itens/{id}` | `id: long` | `ItemResponseDto` |
| `criar_item` | POST `/api/versoes/{versaoId}/itens` | `versaoId: long`, `nome: string`, `descricao: string?` | `ItemResponseDto` |
| `atualizar_item` | PUT `/api/itens/{id}` | `id: long`, `nome: string`, `descricao: string?` | `ItemResponseDto` |
| `deletar_item` | DELETE `/api/itens/{id}` | `id: long` | confirmação |
| `vincular_arquivo_item` | PUT `/api/itens/{id}/arquivo` | `id: long`, `arquivoId: long` | confirmação |
| `desvincular_arquivo_item` | DELETE `/api/itens/{id}/arquivo` | `id: long` | confirmação |

---

### 6.7 Custos de Item — `ItemCustoTools.cs`

| Tool | Método | Parâmetros | Retorno |
|---|---|---|---|
| `listar_custos_item` | GET `/api/itens/{itemId}/custos` | `itemId: long` | `ItemCustoResponseDto[]` |
| `obter_custo` | GET `/api/custos/{id}` | `id: long` | `ItemCustoResponseDto` |
| `criar_custo` | POST `/api/itens/{itemId}/custos` | `itemId: long`, `peso: decimal?`, `tempo: decimal?`, `quantidade: decimal?`, `perdas: decimal?` | `ItemCustoResponseDto` |
| `atualizar_custo` | PUT `/api/custos/{id}` | `id: long`, `peso: decimal?`, `tempo: decimal?`, `quantidade: decimal?`, `perdas: decimal?` | `ItemCustoResponseDto` |
| `deletar_custo` | DELETE `/api/custos/{id}` | `id: long` | confirmação |

---

## 7. Padrão de Implementação das Tools

Cada tool usa o atributo `[McpServerTool]` com uma description clara para o LLM, e `[McpServerToolParameter]` para cada parâmetro:

```csharp
[McpServerToolType]
public class ProdutoTools(IProdutoRepository repo, ITipoProdutoRepository tipoRepo)
{
    [McpServerTool(Name = "listar_produtos", Description = "Lista produtos com suporte a filtros e paginação.")]
    public async Task<string> ListarProdutos(
        [McpServerToolParameter(Description = "Número da página (padrão: 1)")] int page = 1,
        [McpServerToolParameter(Description = "Tamanho da página (padrão: 20)")] int pageSize = 20,
        [McpServerToolParameter(Description = "Filtrar por ativado/desativado")] bool? ativado = null,
        [McpServerToolParameter(Description = "Filtrar por tipo de produto")] long? tipoProdutoId = null)
    {
        var (items, total) = await repo.GetAllAsync(page, pageSize, ativado, tipoProdutoId);
        return JsonSerializer.Serialize(new { total, page, pageSize, items });
    }
}
```

### 7.1 Tratamento de Erros

Todas as tools devem capturar exceções e retornar JSON estruturado de erro:

```csharp
catch (Exception ex)
{
    return JsonSerializer.Serialize(new { error = ex.Message });
}
```

### 7.2 Retorno de Recursos Não Encontrados

Retornar JSON com campo `error` e mensagem descritiva (não lançar exceção):

```json
{ "error": "Produto com id 42 não encontrado." }
```

---

## 8. Configuração (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=assistentedb;Username=postgres;Password=postgres"
  }
}
```

---

## 9. Adição à Solução

```bash
dotnet new console -n AssistenteDB.Mcp -o src/Presentation/AssistenteDB.Mcp --framework net10.0
dotnet sln AssistenteDB.sln add src/Presentation/AssistenteDB.Mcp/AssistenteDB.Mcp.csproj
```

---

## 10. Integração com Claude Desktop

Após build, adicionar em `claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "assistentedb": {
      "command": "dotnet",
      "args": ["run", "--project", "caminho/para/AssistenteDB.Mcp"],
      "env": {
        "ConnectionStrings__DefaultConnection": "Host=localhost;Database=assistentedb;Username=postgres;Password=postgres"
      }
    }
  }
}
```

---

## 11. Fora de Escopo (v1)

| Item | Motivo |
|---|---|
| `upload_arquivo` (POST multipart) | Binários via stdio requerem encoding base64 end-to-end — complexidade extra |
| Autenticação/autorização | API interna sem auth atualmente |
| MCP Resources | Apenas tools são necessárias para os casos de uso atuais |
| MCP Prompts | Fora do escopo de integração de endpoints |

---

## 12. Resumo

| | |
|---|---|
| **Projeto novo** | `AssistenteDB.Mcp` (console, .NET 10) |
| **Total de tools** | 30 tools (31 endpoints − 1 upload multipart) |
| **Arquivos de tools** | 7 arquivos (`Tools/*.cs`) |
| **Transporte** | stdio |
| **Acesso a dados** | Repositórios diretos (sem proxy HTTP) |
| **Pacote MCP** | `ModelContextProtocol` |

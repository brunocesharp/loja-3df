# Especificação: Migração para .NET 10

**Versão:** 1.0 | **Data:** 2026-03-14

## 1. Visão Geral

Este documento especifica todas as alterações necessárias para migrar o projeto **AssistenteDB** do .NET 8.0 para o .NET 10.0. A migração abrange atualização do target framework, pacotes NuGet, substituição do Swashbuckle pelo OpenAPI nativo do ASP.NET Core e ajustes de configuração.

---

## 2. Escopo

### Dentro do Escopo
- Atualização do `TargetFramework` em todos os `.csproj`
- Atualização de todos os pacotes NuGet para versões compatíveis com .NET 10
- Substituição do Swashbuckle pelo suporte nativo de OpenAPI (ASP.NET Core 10)
- Ajustes no `Program.cs` decorrentes da troca de biblioteca OpenAPI
- Verificação de breaking changes do EF Core 10 que impactem o código existente

### Fora do Escopo
- Mudanças de regras de negócio ou endpoints
- Adição de novas funcionalidades
- Migração de banco de dados

---

## 3. Inventário Atual

| Artefato | Valor Atual |
|---|---|
| TargetFramework (todos os projetos) | `net8.0` |
| Microsoft.EntityFrameworkCore | 8.0.0 |
| Npgsql.EntityFrameworkCore.PostgreSQL | 8.0.0 |
| Microsoft.EntityFrameworkCore.Tools | 8.0.0 |
| Swashbuckle.AspNetCore | 6.5.0 |
| SDK mínimo | .NET 8 |

---

## 4. Alterações Necessárias

### ALT-001 — Atualizar TargetFramework em todos os `.csproj`

**Impacto:** 4 arquivos

| Arquivo | Caminho |
|---|---|
| AssistenteDB.Domain.csproj | `src/Domain/AssistenteDB.Domain.csproj` |
| AssistenteDB.Application.csproj | `src/Application/AssistenteDB.Application.csproj` |
| AssistenteDB.Data.csproj | `src/Data/AssistenteDB.Data.csproj` |
| AssistenteDB.Api.csproj | `src/Presentation/AssistenteDB.Api/AssistenteDB.Api.csproj` |

**Mudança em cada arquivo:**
```xml
<!-- DE -->
<TargetFramework>net8.0</TargetFramework>

<!-- PARA -->
<TargetFramework>net10.0</TargetFramework>
```

---

### ALT-002 — Atualizar pacotes NuGet em `AssistenteDB.Data.csproj`

| Pacote | Versão Atual | Versão Alvo |
|---|---|---|
| `Microsoft.EntityFrameworkCore` | 8.0.0 | 10.0.0 |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 8.0.0 | 10.0.0 |

**Mudança:**
```xml
<!-- DE -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />

<!-- PARA -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0" />
```

---

### ALT-003 — Atualizar e substituir pacotes NuGet em `AssistenteDB.Api.csproj`

O `Swashbuckle.AspNetCore` **não tem suporte oficial para .NET 10**. O ASP.NET Core 10 inclui suporte nativo a OpenAPI via `Microsoft.AspNetCore.OpenApi`. A UI do Swagger pode ser servida com `Scalar.AspNetCore` ou `Swashbuckle.AspNetCore` em versão atualizada (se compatível).

**Opção recomendada: OpenAPI nativo + Scalar UI**

| Pacote | Ação | Versão |
|---|---|---|
| `Microsoft.EntityFrameworkCore.Tools` | Atualizar | 10.0.0 |
| `Swashbuckle.AspNetCore` | **Remover** | — |
| `Microsoft.AspNetCore.OpenApi` | **Adicionar** | 10.0.0 |
| `Scalar.AspNetCore` | **Adicionar** | latest |

**Mudança no `.csproj`:**
```xml
<!-- DE -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0">
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  <PrivateAssets>all</PrivateAssets>
</PackageReference>
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />

<!-- PARA -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.0">
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  <PrivateAssets>all</PrivateAssets>
</PackageReference>
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.0" />
<PackageReference Include="Scalar.AspNetCore" Version="2.*" />
```

---

### ALT-004 — Atualizar `Program.cs` (substituição do Swashbuckle)

**Arquivo:** `src/Presentation/AssistenteDB.Api/Program.cs`

**Mudança:**
```csharp
// DE
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// PARA
builder.Services.AddOpenApi();
```

```csharp
// DE
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// PARA
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                    // expõe /openapi/v1.json
    app.MapScalarApiReference();         // UI em /scalar/v1
}
```

**Observação:** O `using Scalar.AspNetCore;` deve ser adicionado no topo do arquivo.

---

### ALT-005 — Verificar breaking changes do EF Core 10

O EF Core 10 introduz mudanças que devem ser verificadas no código existente:

| Breaking Change | Área Afetada | Ação |
|---|---|---|
| `FromSqlRaw` / `FromSqlInterpolated` com parâmetros nulos passam a lançar exceção | Repositórios que usam SQL raw | Verificar se há uso de SQL raw nos Repositories |
| `ExecuteUpdate` / `ExecuteDelete` requerem transação explícita em alguns cenários | Repositórios com bulk ops | Verificar se há chamadas de ExecuteUpdate/Delete |
| Mudanças no comportamento de `ValueConverter` para tipos primitivos | Entidades com conversores | Verificar AppDbContext |
| `HasConversion` com tipos nullable modificado | AppDbContext | Verificar mapeamentos de tipo |
| Novos warnings de analyzer que viram erros em modo `TreatWarningsAsErrors` | Build | Verificar se existe essa flag nos .csproj |

**Ação obrigatória:** Compilar o projeto e corrigir todos os warnings/erros surfaçados após a atualização.

---

### ALT-006 — Atualizar `launchSettings.json`

**Arquivo:** `src/Presentation/AssistenteDB.Api/Properties/launchSettings.json`

A URL do Swagger UI muda de `/swagger` para a URL do Scalar:

```json
// DE
"launchUrl": "swagger"

// PARA
"launchUrl": "scalar/v1"
```

---

### ALT-007 — Instalar SDK .NET 10

Pré-requisito de ambiente:

- Instalar .NET 10 SDK: https://dot.net
- Verificar instalação: `dotnet --version` deve retornar `10.x.x`
- O Visual Studio 2022 17.14+ tem suporte ao .NET 10

---

## 5. Resumo de Arquivos Alterados

| Arquivo | Tipo de Alteração |
|---|---|
| `src/Domain/AssistenteDB.Domain.csproj` | TargetFramework |
| `src/Application/AssistenteDB.Application.csproj` | TargetFramework |
| `src/Data/AssistenteDB.Data.csproj` | TargetFramework + pacotes EF Core / Npgsql |
| `src/Presentation/AssistenteDB.Api/AssistenteDB.Api.csproj` | TargetFramework + remove Swashbuckle + adiciona OpenAPI + Scalar |
| `src/Presentation/AssistenteDB.Api/Program.cs` | Substituição Swashbuckle → OpenAPI nativo + Scalar |
| `src/Presentation/AssistenteDB.Api/Properties/launchSettings.json` | launchUrl |
| `Properties/launchSettings.json` | launchUrl (cópia raiz) |

---

## 6. Ordem de Execução

1. Instalar SDK .NET 10 (ALT-007)
2. Atualizar `TargetFramework` em todos os `.csproj` (ALT-001)
3. Atualizar pacotes NuGet do Data (ALT-002)
4. Atualizar/substituir pacotes NuGet do Api (ALT-003)
5. Atualizar `Program.cs` (ALT-004)
6. Build e correção de breaking changes (ALT-005)
7. Atualizar `launchSettings.json` (ALT-006)
8. Executar a aplicação e validar Swagger/Scalar UI
9. Rodar testes manuais nos endpoints principais

---

## 7. Riscos

| Risco | Probabilidade | Mitigação |
|---|---|---|
| Npgsql 10.0.0 ainda não lançado | Baixa | Usar 9.0.x com EF Core 10 (compatível) ou aguardar release |
| Breaking changes não mapeados no EF Core 10 | Média | Compilar e testar todos os endpoints após migração |
| Scalar UI com comportamento diferente do Swagger UI | Baixa | Alternativa: Swashbuckle 8.x (se compatível com .NET 10) |

---

## 8. Histórico de Mudanças

| Versão | Data | Descrição |
|---|---|---|
| 1.0 | 2026-03-14 | Versão inicial — análise de migração .NET 8 → .NET 10 |

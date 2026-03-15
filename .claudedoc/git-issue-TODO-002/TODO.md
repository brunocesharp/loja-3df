# TODO — Plano de Execução: git-issue-TODO-002

**Data:** 2026-03-14
**Baseado em:** specification.md

---

## Contexto

Migração mecânica de .NET 8.0 para .NET 10.0. Nenhum endpoint ou regra de negócio é alterada. As únicas mudanças funcionais são a substituição do Swashbuckle pelo OpenAPI nativo do ASP.NET Core 10 e a adição do Scalar como UI de documentação.

---

## Dependências entre tarefas

```
TASK-001 (SDK)
    │
    └──▶ TASK-002 (TargetFramework) ──▶ TASK-003 (pacotes Data)
                                    ──▶ TASK-004 (pacotes Api)
                                              │
                                              └──▶ TASK-005 (Program.cs)
                                                        │
                                                        └──▶ TASK-006 (build + breaking changes)
                                                                  │
                                                                  └──▶ TASK-007 (launchSettings)
                                                                            │
                                                                            └──▶ TASK-008 (validação)
```

---

## TASK-001 — Instalar SDK .NET 10

**Pré-requisito de ambiente — executar antes de qualquer outra tarefa**

- [ ] Baixar e instalar o SDK .NET 10 em https://dot.net
- [ ] Verificar instalação: `dotnet --version` deve retornar `10.x.x`
- [ ] Confirmar Visual Studio 2022 versão 17.14+ (exigido para .NET 10)

---

## TASK-002 — Atualizar TargetFramework em todos os `.csproj`

**Arquivo:** `src/Domain/AssistenteDB.Domain.csproj`
- [ ] Alterar `<TargetFramework>net8.0</TargetFramework>` → `<TargetFramework>net10.0</TargetFramework>`

**Arquivo:** `src/Application/AssistenteDB.Application.csproj`
- [ ] Alterar `<TargetFramework>net8.0</TargetFramework>` → `<TargetFramework>net10.0</TargetFramework>`

**Arquivo:** `src/Data/AssistenteDB.Data.csproj`
- [ ] Alterar `<TargetFramework>net8.0</TargetFramework>` → `<TargetFramework>net10.0</TargetFramework>`

**Arquivo:** `src/Presentation/AssistenteDB.Api/AssistenteDB.Api.csproj`
- [ ] Alterar `<TargetFramework>net8.0</TargetFramework>` → `<TargetFramework>net10.0</TargetFramework>`

---

## TASK-003 — Atualizar pacotes NuGet em `AssistenteDB.Data.csproj`

**Arquivo:** `src/Data/AssistenteDB.Data.csproj`

- [ ] Atualizar `Microsoft.EntityFrameworkCore` de `8.0.0` para `10.0.0`
- [ ] Atualizar `Npgsql.EntityFrameworkCore.PostgreSQL` de `8.0.0` para `10.0.0`

> **Atenção:** Se `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.0 ainda não estiver disponível no NuGet, usar `9.0.x` (compatível com EF Core 10 via `--prerelease` se necessário).

---

## TASK-004 — Atualizar e substituir pacotes NuGet em `AssistenteDB.Api.csproj`

**Arquivo:** `src/Presentation/AssistenteDB.Api/AssistenteDB.Api.csproj`

- [ ] Atualizar `Microsoft.EntityFrameworkCore.Tools` de `8.0.0` para `10.0.0`
- [ ] Remover `<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />`
- [ ] Adicionar `<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.0" />`
- [ ] Adicionar `<PackageReference Include="Scalar.AspNetCore" Version="2.*" />`

---

## TASK-005 — Atualizar `Program.cs`

**Arquivo:** `src/Presentation/AssistenteDB.Api/Program.cs`

- [ ] Adicionar `using Scalar.AspNetCore;` no topo do arquivo
- [ ] Substituir no bloco de serviços:
  ```csharp
  // remover
  builder.Services.AddEndpointsApiExplorer();
  builder.Services.AddSwaggerGen();

  // adicionar
  builder.Services.AddOpenApi();
  ```
- [ ] Substituir no bloco do pipeline:
  ```csharp
  // remover
  if (app.Environment.IsDevelopment())
  {
      app.UseSwagger();
      app.UseSwaggerUI();
  }

  // adicionar
  if (app.Environment.IsDevelopment())
  {
      app.MapOpenApi();              // expõe /openapi/v1.json
      app.MapScalarApiReference();   // UI em /scalar/v1
  }
  ```

---

## TASK-006 — Build e correção de breaking changes

- [ ] Executar `dotnet build` na raiz da solution
- [ ] Corrigir todos os erros de compilação surfaçados pelo EF Core 10
- [ ] Verificar e corrigir warnings que viraram erros no .NET 10 (nullable, analyzers)
- [ ] Verificar se há uso de SQL raw (`FromSqlRaw`, `ExecuteUpdate`, `ExecuteDelete`) nos repositórios — ajustar se necessário
- [ ] Verificar mapeamentos de tipo no `AppDbContext.cs` (`HasConversion`, `ValueConverter`) — confirmar comportamento com EF Core 10
- [ ] Executar `dotnet restore` + `dotnet build --configuration Release` para confirmar build limpo

---

## TASK-007 — Atualizar `launchSettings.json`

**Arquivo:** `src/Presentation/AssistenteDB.Api/Properties/launchSettings.json`
- [ ] Alterar `"launchUrl": "swagger"` → `"launchUrl": "scalar/v1"` nos perfis `http` e `https`

**Arquivo:** `Properties/launchSettings.json` (cópia na raiz)
- [ ] Aplicar a mesma alteração de `launchUrl`

---

## TASK-008 — Validação final

- [ ] Iniciar a aplicação (`dotnet run` ou F5 no Visual Studio)
- [ ] Confirmar que a UI do Scalar está acessível em `https://localhost:9595/scalar/v1`
- [ ] Confirmar que o JSON do contrato OpenAPI está acessível em `https://localhost:9595/openapi/v1.json`
- [ ] Testar ao menos um endpoint de cada tipo (GET lista, GET por ID, POST, PUT, DELETE) via Scalar UI
- [ ] Testar upload de arquivo via `POST /api/arquivos`
- [ ] Confirmar que `dotnet ef` continua funcionando: `dotnet ef dbcontext info --project src/Data --startup-project src/Presentation/AssistenteDB.Api`

---

## Checklist final

- [ ] Todos os 4 projetos com `TargetFramework = net10.0`
- [ ] Zero referências ao `Swashbuckle` nos `.csproj` e `Program.cs`
- [ ] `Microsoft.AspNetCore.OpenApi` + `Scalar.AspNetCore` adicionados e funcionais
- [ ] Build sem erros em Debug e Release
- [ ] Scalar UI acessível e exibindo todos os endpoints
- [ ] EF Core migrations ainda funcionais após atualização

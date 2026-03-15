# Todo — AssistenteDB MCP Server (TODO-003)

**Gerado em:** 2026-03-15
**Caminho crítico:** TASK-001 → TASK-002 → TASK-003 → TASK-004 → TASK-005 → TASK-006 → TASK-007 → TASK-008 → TASK-009

---

## Fase 1 — Setup do Projeto

### TASK-001 · Criar projeto `AssistenteDB.Mcp`
- [ ] Executar `dotnet new console -n AssistenteDB.Mcp -o src/Presentation/AssistenteDB.Mcp --framework net10.0`
- [ ] Executar `dotnet sln AssistenteDB.sln add src/Presentation/AssistenteDB.Mcp/AssistenteDB.Mcp.csproj`
- **Depends on:** nenhuma

### TASK-002 · Configurar `AssistenteDB.Mcp.csproj`
- [ ] Adicionar `<PackageReference Include="ModelContextProtocol" Version="0.*" />`
- [ ] Adicionar `<PackageReference Include="Microsoft.Extensions.Hosting" Version="10.0.*" />`
- [ ] Adicionar `<PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.*" />`
- [ ] Adicionar `<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.*" />`
- [ ] Adicionar `<ProjectReference>` para `AssistenteDB.Application` e `AssistenteDB.Data`
- **Depends on:** TASK-001

### TASK-003 · Criar `appsettings.json`
- [ ] Criar arquivo com `ConnectionStrings:DefaultConnection` apontando para PostgreSQL local
- **Depends on:** TASK-001

### TASK-004 · Implementar `Program.cs`
- [ ] Configurar `Host.CreateApplicationBuilder`
- [ ] Registrar `AppDbContext` com Npgsql
- [ ] Registrar todos os repositórios como `Scoped`
- [ ] Chamar `AddMcpServer().WithStdioServerTransport().WithToolsFromAssembly()`
- [ ] Chamar `await builder.Build().RunAsync()`
- **Depends on:** TASK-002, TASK-003

---

## Fase 2 — Implementação das Tools

### TASK-005 · Criar `Tools/ArquivoTools.cs`
- [ ] `download_arquivo` — busca bytes do arquivo por id, retorna base64 + content type
- [ ] `delete_arquivo` — deleta arquivo por id, retorna confirmação JSON
- [ ] Tratamento de erros com `catch (Exception ex)` retornando `{ "error": "..." }`
- [ ] Retornar `{ "error": "Arquivo com id X não encontrado." }` para 404
- **Depends on:** TASK-004

### TASK-006 · Criar `Tools/TipoArquivoTools.cs`
- [ ] `listar_tipos_arquivo` — retorna lista de `TipoArquivoDto`
- [ ] `obter_tipo_arquivo` — retorna um `TipoArquivoDto` por id
- **Depends on:** TASK-004

### TASK-007 · Criar `Tools/ProdutoTools.cs` e `Tools/TipoProdutoTools.cs`
- [ ] `listar_produtos` — paginado com filtros `page`, `pageSize`, `ativado?`, `tipoProdutoId?`
- [ ] `obter_produto` — por id
- [ ] `criar_produto` — campos: `tipoProdutoId`, `nome`, `descricao?`, `ativado`
- [ ] `atualizar_produto` — campos completos por id
- [ ] `deletar_produto` — por id
- [ ] `listar_tipos_produto` — lista completa
- [ ] `obter_tipo_produto` — por id
- **Depends on:** TASK-004

### TASK-008 · Criar `Tools/ProdutoVersaoTools.cs`
- [ ] `listar_versoes_produto` — por `produtoId`
- [ ] `obter_versao` — por id
- [ ] `criar_versao` — campos: `produtoId`, `nome`, `numero`
- [ ] `atualizar_versao` — por id
- [ ] `deletar_versao` — por id
- [ ] `vincular_arquivo_versao` — PUT com `arquivoId`
- [ ] `desvincular_arquivo_versao` — DELETE
- **Depends on:** TASK-004

### TASK-009 · Criar `Tools/ItemTools.cs` e `Tools/ItemCustoTools.cs`
- [ ] `listar_itens_versao` — por `versaoId`
- [ ] `obter_item`, `criar_item`, `atualizar_item`, `deletar_item`
- [ ] `vincular_arquivo_item`, `desvincular_arquivo_item`
- [ ] `listar_custos_item` — por `itemId`
- [ ] `obter_custo`, `criar_custo`, `atualizar_custo`, `deletar_custo`
- [ ] Campos de custo: `peso?`, `tempo?`, `quantidade?`, `perdas?` (todos `decimal?`)
- **Depends on:** TASK-004

---

## Fase 3 — Validação

### TASK-010 · Build e smoke test
- [ ] `dotnet build` sem erros
- [ ] Testar handshake MCP via stdin com JSON-RPC manual ou Claude Desktop
- [ ] Verificar que as 30 tools aparecem na listagem
- **Depends on:** TASK-005, TASK-006, TASK-007, TASK-008, TASK-009

---

## Resumo

| | |
|---|---|
| **Total de tasks** | 10 |
| **Total de tools** | 30 |
| **Arquivos de tools** | 7 (`Tools/*.cs`) |
| **Fora de escopo** | `upload_arquivo` (multipart), auth, MCP Resources/Prompts |

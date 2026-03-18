# Todo — upload_arquivo MCP Tool (TODO-004)

**Gerado em:** 2026-03-15
**Caminho crítico:** TASK-001 → TASK-002

---

## Fase 1 — Implementação

### TASK-001 · Adicionar method `UploadArquivo` em `ArquivoTools.cs`

Arquivo: `src/Presentation/AssistenteDB.Mcp/Tools/ArquivoTools.cs`

- [ ] Adicionar using `AssistenteDB.Domain.Entities` (se ainda não presente)
- [ ] Implementar method com atributos `[McpServerTool]` e `[Description]` seguindo o padrão do arquivo
- [ ] Parâmetros: `tipoArquivoId: long`, `base64: string`
- [ ] Decodificar base64 com `Convert.FromBase64String` dentro de try/catch separado, retornando `{ "error": "Base64 inválido." }` em caso de falha
- [ ] Criar entidade `Arquivo { TipoArquivoId = tipoArquivoId, Bytes = bytes }`
- [ ] Chamar `await repo.CreateAsync(arquivo)`
- [ ] Retornar JSON: `{ id, tipoArquivoId, message: "Arquivo criado com sucesso." }`
- [ ] Capturar exceções genéricas retornando `{ "error": ex.Message }`
- **Depends on:** nenhuma

---

## Fase 2 — Validação

### TASK-002 · Smoke test manual

- [ ] Executar `dotnet build` sem erros
- [ ] Chamar `upload_arquivo` com base64 válido e verificar retorno com `id`
- [ ] Chamar `upload_arquivo` com base64 inválido e verificar `{ "error": "Base64 inválido." }`
- [ ] Chamar `download_arquivo` com o `id` retornado e verificar que os bytes batem
- **Depends on:** TASK-001

---

## Resumo

| | |
|---|---|
| **Total de tasks** | 2 |
| **Arquivo alterado** | `ArquivoTools.cs` (1 method adicionado) |
| **Novos arquivos** | nenhum |
| **Entidade** | `Arquivo` — sem alterações |
| **Repositório** | `IArquivoRepository.CreateAsync` — já existe |

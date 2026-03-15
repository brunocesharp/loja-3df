# Specification — Tool MCP upload_arquivo

**Versão:** 1.0 | **Data:** 2026-03-15

## 1. Objetivo

Adicionar o tool MCP `upload_arquivo` à classe `ArquivoTools`, permitindo que agentes de IA insiram novos arquivos binários na tabela `arquivo` do banco de dados via base64.

Este tool complementa os dois já existentes (`download_arquivo` e `delete_arquivo`), completando o ciclo CRUD de arquivos na camada MCP.

---

## 2. Contexto

Na spec 003 (git-issue-DONE-003), o `upload_arquivo` foi explicitamente deixado **fora de escopo** com a justificativa:

> *"Upload de arquivos binários via MCP stdio requer tratamento especial de base64 — complexidade extra."*

A solução para remover essa complexidade é **aceitar o conteúdo já em base64 como parâmetro string**, delegando ao chamador a responsabilidade de codificar o arquivo. Isso elimina o problema de binários brutos no transporte stdio e mantém o padrão de implementação existente.

---

## 3. Escopo

### Dentro do Escopo

- Novo method `UploadArquivo` em `ArquivoTools.cs`
- Validação do base64 recebido
- Persistência via `IArquivoRepository.CreateAsync`
- Retorno do `id` gerado

### Fora do Escopo

- Validação de `tipoArquivoId` (se o tipo existe) — responsabilidade do banco via FK
- Endpoint REST equivalente — já existe na API
- Alterações na entidade `Arquivo` ou no repositório

---

## 4. Arquivos Envolvidos

| Arquivo | Ação |
|---|---|
| `src/Presentation/AssistenteDB.Mcp/Tools/ArquivoTools.cs` | Adicionar method `UploadArquivo` |

Nenhuma outra alteração é necessária: a interface `IArquivoRepository` já expõe `CreateAsync(Arquivo arquivo)` e a entidade `Arquivo` já possui as propriedades necessárias.

---

## 5. Requisito Funcional

### RF-001 — Tool `upload_arquivo`

**Descrição:** Recebe um arquivo em base64 e o persiste na tabela `arquivo`, associado ao tipo informado.

**Assinatura do tool:**

| Campo | Valor |
|---|---|
| Nome MCP | `upload_arquivo` |
| Description | `"Faz upload de um arquivo em base64 para a tabela Arquivo, associando ao tipo de arquivo informado."` |

**Parâmetros:**

| Parâmetro | Tipo | Obrigatório | Description para o LLM |
|---|---|---|---|
| `tipoArquivoId` | `long` | Sim | `"Id do tipo do arquivo"` |
| `base64` | `string` | Sim | `"Conteúdo do arquivo codificado em base64"` |

**Retorno — sucesso (HTTP 200 equivalente):**

```json
{
  "id": 42,
  "tipoArquivoId": 1,
  "message": "Arquivo criado com sucesso."
}
```

**Retorno — base64 inválido:**

```json
{
  "error": "Base64 inválido."
}
```

**Retorno — erro genérico:**

```json
{
  "error": "<mensagem da exceção>"
}
```

---

## 6. Implementação

```csharp
[McpServerTool(Name = "upload_arquivo"), Description("Faz upload de um arquivo em base64 para a tabela Arquivo, associando ao tipo de arquivo informado.")]
public async Task<string> UploadArquivo(
    [Description("Id do tipo do arquivo")] long tipoArquivoId,
    [Description("Conteúdo do arquivo codificado em base64")] string base64)
{
    try
    {
        byte[] bytes;
        try { bytes = Convert.FromBase64String(base64); }
        catch { return JsonSerializer.Serialize(new { error = "Base64 inválido." }); }

        var arquivo = new Arquivo
        {
            TipoArquivoId = tipoArquivoId,
            Bytes = bytes
        };

        var criado = await repo.CreateAsync(arquivo);
        return JsonSerializer.Serialize(new
        {
            id = criado.Id,
            tipoArquivoId = criado.TipoArquivoId,
            message = "Arquivo criado com sucesso."
        });
    }
    catch (Exception ex)
    {
        return JsonSerializer.Serialize(new { error = ex.Message });
    }
}
```

**Using necessário:** `AssistenteDB.Domain.Entities` (para `new Arquivo { ... }`).
Verificar se já está presente nos usings do arquivo; caso não esteja, adicionar.

---

## 7. Critérios de Aceitação

- [ ] Tool `upload_arquivo` presente em `ArquivoTools`
- [ ] Base64 inválido retorna `{ "error": "Base64 inválido." }` sem lançar exceção
- [ ] Arquivo criado via `IArquivoRepository.CreateAsync`
- [ ] Retorno de sucesso contém `id`, `tipoArquivoId` e `message`
- [ ] Exceções genéricas capturadas com `{ "error": ex.Message }`
- [ ] Padrão de atributos `[McpServerTool]` e `[Description]` seguido conforme os tools existentes

---

## 8. Rastreabilidade

| Item | Referência |
|---|---|
| Issue | git-issue-TODO-004 |
| Spec origem (MCP server) | `.claudedoc/git-issue-DONE-003/specification.md` — seção 11 (Fora de Escopo v1) |
| Entidade | `src/Domain/Entities/Arquivo.cs` |
| Interface | `src/Domain/Interfaces/IArquivoRepository.cs` |
| Repositório | `src/Data/Repositories/ArquivoRepository.cs` |
| Tool class | `src/Presentation/AssistenteDB.Mcp/Tools/ArquivoTools.cs` |

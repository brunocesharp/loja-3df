# Design REST API — Sistema de Precificação

**Issue:** git-issue-TODO-001
**Data:** 2026-03-14
**Stack:** ASP.NET Core 8 · EF Core · PostgreSQL
**Padrões:** JSON camelCase · rotas kebab-case plural · envelope `{ data }` / `{ error }`

---

## Índice de Recursos

| Recurso        | Rota base                           | Tipo    |
|----------------|-------------------------------------|---------|
| TipoProduto    | `/api/tipos-produto`                | Lookup  |
| Produto        | `/api/produtos`                     | CRUD    |
| ProdutoVersao  | `/api/produtos/{produtoId}/versoes` | CRUD    |
| Item           | `/api/versoes/{versaoId}/itens`     | CRUD    |
| ItemCusto      | `/api/itens/{itemId}/custos`        | CRUD    |
| TipoArquivo    | `/api/tipos-arquivo`                | Lookup  |
| Arquivo        | `/api/arquivos`                     | Upload  |
| Item-Arquivo   | `/api/itens/{id}/arquivo`           | Vínculo |
| Versao-Arquivo | `/api/versoes/{id}/arquivo`         | Vínculo |

---

## Padrões Transversais

### Envelope de resposta

**Sucesso singular:**
```json
{ "data": { ... } }
```

**Sucesso coleção:**
```json
{
  "data": [ ... ],
  "pagination": { "page": 1, "pageSize": 20, "totalPages": 5, "totalItems": 89 }
}
```

**Erro:**
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Dados inválidos.",
    "details": [
      { "field": "nome", "code": "REQUIRED", "message": "Campo obrigatório." }
    ]
  }
}
```

### Status codes

| Código | Uso                                                        |
|--------|------------------------------------------------------------|
| 200    | Sucesso em GET / PUT                                       |
| 201    | Recurso criado (POST)                                      |
| 204    | Sem conteúdo (DELETE / desvincular)                        |
| 400    | Payload malformado                                         |
| 404    | Recurso não encontrado                                     |
| 409    | Conflito de integridade (FK referenciada, já vinculado)    |
| 413    | Arquivo maior que 10 MB                                    |
| 422    | FK inexistente ou regra de negócio violada                 |
| 500    | Erro interno                                               |

### Paginação

```
GET /api/produtos?page=1&pageSize=20
```

Obrigatória em: Produto, Item, ItemCusto.

---

## RF-001 — TipoProduto

> Lookup read-only. Dados gerenciados por seed (`ORGN`, `ACSS`, `MINI`).

### Endpoints

| Método | Rota                       | Descrição             |
|--------|----------------------------|-----------------------|
| GET    | `/api/tipos-produto`       | Listar todos os tipos |
| GET    | `/api/tipos-produto/{id}`  | Buscar tipo por ID    |

### GET /api/tipos-produto — Response 200
```json
{
  "data": [
    { "id": 1, "sigla": "ORGN", "nome": "ORGANIZADORES", "descricao": "ORGANIZADORES" },
    { "id": 2, "sigla": "ACSS", "nome": "ACESSORIOS",    "descricao": "ACESSORIOS"    },
    { "id": 3, "sigla": "MINI", "nome": "MINIATURA",     "descricao": "MINIATURAS"    }
  ]
}
```

### GET /api/tipos-produto/{id} — Response 200
```json
{
  "data": { "id": 1, "sigla": "ORGN", "nome": "ORGANIZADORES", "descricao": "ORGANIZADORES" }
}
```

Response 404: `{ "error": { "code": "TIPO_PRODUTO_NOT_FOUND", "message": "Tipo de produto não encontrado." } }`

---

## RF-002 — Produto

### Endpoints

| Método | Rota                   | Descrição             |
|--------|------------------------|-----------------------|
| GET    | `/api/produtos`        | Listar (paginado)     |
| GET    | `/api/produtos/{id}`   | Buscar por ID         |
| POST   | `/api/produtos`        | Criar                 |
| PUT    | `/api/produtos/{id}`   | Atualizar completo    |
| DELETE | `/api/produtos/{id}`   | Remover               |

### GET /api/produtos

**Query params:** `page`, `pageSize`, `ativado` (bool), `tipoProdutoId` (long)

**Response 200:**
```json
{
  "data": [
    {
      "id": 1,
      "tipoProduto": { "id": 1, "sigla": "ORGN", "nome": "ORGANIZADORES" },
      "nome": "Organizador de Gaveta",
      "descricao": "Separador modular para gavetas",
      "dataCriacao": "2026-03-14T10:00:00Z",
      "dataAtualizacao": null,
      "ativado": true
    }
  ],
  "pagination": { "page": 1, "pageSize": 20, "totalPages": 3, "totalItems": 47 }
}
```

### GET /api/produtos/{id} — Response 200
```json
{
  "data": {
    "id": 1,
    "tipoProduto": { "id": 1, "sigla": "ORGN", "nome": "ORGANIZADORES" },
    "nome": "Organizador de Gaveta",
    "descricao": "Separador modular para gavetas",
    "dataCriacao": "2026-03-14T10:00:00Z",
    "dataAtualizacao": null,
    "ativado": true
  }
}
```

### POST /api/produtos

**Request Body:**
```json
{
  "tipoProdutoId": 1,
  "nome": "Organizador de Gaveta",
  "descricao": "Separador modular para gavetas",
  "ativado": true
}
```

| Campo         | Tipo    | Obrig. | Validação              |
|---------------|---------|--------|------------------------|
| tipoProdutoId | long    | sim    | FK deve existir        |
| nome          | string  | sim    | MaxLength(150)         |
| descricao     | string? | não    | —                      |
| ativado       | bool    | não    | Default: `true`        |

**Response 201:** mesmo schema do GET por ID.

Response 422 (FK inválida): `{ "error": { "code": "TIPO_PRODUTO_NOT_FOUND", "message": "tipoProdutoId inválido." } }`

### PUT /api/produtos/{id}

**Request Body:** mesmos campos do POST, todos obrigatórios incluindo `ativado`.

**Response 200:** mesmo schema do GET por ID.

### DELETE /api/produtos/{id}

Remove em cascata para `produto_versao`.

**Response 204** · **Response 404:** padrão.

---

## RF-003 — ProdutoVersao

### Endpoints

| Método | Rota                                    | Descrição        |
|--------|-----------------------------------------|------------------|
| GET    | `/api/produtos/{produtoId}/versoes`     | Listar versões   |
| GET    | `/api/versoes/{id}`                     | Buscar por ID    |
| POST   | `/api/produtos/{produtoId}/versoes`     | Criar versão     |
| PUT    | `/api/versoes/{id}`                     | Atualizar versão |
| DELETE | `/api/versoes/{id}`                     | Remover versão   |

### GET /api/produtos/{produtoId}/versoes — Response 200
```json
{
  "data": [
    { "id": 1, "produtoId": 1, "nome": "Versão 1.0", "numero": 1 },
    { "id": 2, "produtoId": 1, "nome": "Versão 1.1", "numero": 2 }
  ]
}
```

### GET /api/versoes/{id} — Response 200
```json
{
  "data": {
    "id": 1,
    "produtoId": 1,
    "produto": { "id": 1, "nome": "Organizador de Gaveta" },
    "nome": "Versão 1.0",
    "numero": 1,
    "arquivo": {
      "id": 5,
      "tipoArquivo": { "id": 1, "nome": "Documento PDF", "sigla": "PDF" }
    }
  }
}
```

### POST /api/produtos/{produtoId}/versoes

**Request Body:**
```json
{ "nome": "Versão 1.0", "numero": 1 }
```

| Campo  | Tipo   | Obrig. | Validação                 |
|--------|--------|--------|---------------------------|
| nome   | string | sim    | MaxLength(150)            |
| numero | int    | sim    | >= 1, único por produtoId |

**Response 201:** mesmo schema do GET por ID.

Response 422 (numero duplicado): `{ "error": { "code": "VERSAO_NUMERO_DUPLICADO", "message": "Já existe uma versão com o número 1 para este produto." } }`

### PUT /api/versoes/{id}

**Request Body:** `{ "nome": "Versão 1.0 Rev.", "numero": 1 }`

**Response 200:** mesmo schema do GET por ID.

### DELETE /api/versoes/{id}

Remove em cascata para `item` e `produto_arquivo`. **Response 204.**

---

## RF-004 — Item

### Endpoints

| Método | Rota                             | Descrição              |
|--------|----------------------------------|------------------------|
| GET    | `/api/versoes/{versaoId}/itens`  | Listar itens da versão |
| GET    | `/api/itens/{id}`                | Buscar item (global)   |
| POST   | `/api/versoes/{versaoId}/itens`  | Criar item             |
| PUT    | `/api/itens/{id}`                | Atualizar item         |
| DELETE | `/api/itens/{id}`                | Remover item           |

### GET /api/versoes/{versaoId}/itens — Response 200
```json
{
  "data": [
    { "id": 1, "produtoVersaoId": 1, "nome": "Tampa Superior", "descricao": null },
    { "id": 2, "produtoVersaoId": 1, "nome": "Base",           "descricao": "Base de sustentação" }
  ]
}
```

### GET /api/itens/{id} — Response 200

Retorna custos e arquivo vinculado (quando existirem).

```json
{
  "data": {
    "id": 1,
    "produtoVersao": {
      "id": 1,
      "nome": "Versão 1.0",
      "numero": 1,
      "produto": { "id": 1, "nome": "Organizador de Gaveta" }
    },
    "nome": "Tampa Superior",
    "descricao": null,
    "custos": [
      { "id": 1, "peso": 45.200, "tempo": 12.50, "quantidade": 1.000, "perdas": 2.000 }
    ],
    "arquivo": {
      "id": 3,
      "tipoArquivo": { "id": 2, "nome": "Imagem PNG", "sigla": "PNG" }
    }
  }
}
```

### POST /api/versoes/{versaoId}/itens

**Request Body:**
```json
{ "nome": "Tampa Superior", "descricao": "Tampa de encaixe superior" }
```

| Campo     | Tipo    | Obrig. | Validação      |
|-----------|---------|--------|----------------|
| nome      | string  | sim    | MaxLength(150) |
| descricao | string? | não    | —              |

**Response 201:** mesmo schema do GET por ID.

### PUT /api/itens/{id}

**Request Body:** `{ "nome": "Tampa Superior Rev.", "descricao": "..." }`

**Response 200:** mesmo schema do GET por ID.

### DELETE /api/itens/{id}

Remove em cascata para `item_custo` e `item_arquivo`. **Response 204.**

---

## RF-005 — ItemCusto

### Endpoints

| Método | Rota                              | Descrição       |
|--------|-----------------------------------|-----------------|
| GET    | `/api/itens/{itemId}/custos`      | Listar custos   |
| GET    | `/api/custos/{id}`                | Buscar por ID   |
| POST   | `/api/itens/{itemId}/custos`      | Criar custo     |
| PUT    | `/api/custos/{id}`                | Atualizar custo |
| DELETE | `/api/custos/{id}`                | Remover custo   |

### GET /api/itens/{itemId}/custos — Response 200
```json
{
  "data": [
    { "id": 1, "itemId": 1, "peso": 45.200, "tempo": 12.50, "quantidade": 1.000, "perdas": 2.000 }
  ]
}
```

### POST /api/itens/{itemId}/custos

**Request Body:** todos os campos são opcionais, mas ao menos um deve ser informado.
```json
{ "peso": 45.200, "tempo": 12.50, "quantidade": 1.000, "perdas": 2.000 }
```

| Campo      | Tipo     | Obrig. | Validação                    |
|------------|----------|--------|------------------------------|
| peso       | decimal? | não    | >= 0, precisão NUMERIC(12,3) |
| tempo      | decimal? | não    | >= 0, precisão NUMERIC(12,2) |
| quantidade | decimal? | não    | >= 0, precisão NUMERIC(12,3) |
| perdas     | decimal? | não    | >= 0, precisão NUMERIC(12,3) |

**Response 201:** mesmo schema do GET por ID.

Response 422 (todos nulos): `{ "error": { "code": "CUSTO_SEM_VALORES", "message": "Pelo menos um campo de custo deve ser informado." } }`

### PUT /api/custos/{id}

**Request Body:** mesmo do POST. **Response 200:** mesmo schema do GET por ID.

### DELETE /api/custos/{id}

**Response 204.**

---

## RF-006 — TipoArquivo

> Lookup read-only. Mesma estrutura do TipoProduto.

### Endpoints

| Método | Rota                       | Descrição             |
|--------|----------------------------|-----------------------|
| GET    | `/api/tipos-arquivo`       | Listar todos os tipos |
| GET    | `/api/tipos-arquivo/{id}`  | Buscar tipo por ID    |

### GET /api/tipos-arquivo — Response 200
```json
{
  "data": [
    { "id": 1, "sigla": "PDF", "nome": "Documento PDF" },
    { "id": 2, "sigla": "PNG", "nome": "Imagem PNG"    },
    { "id": 3, "sigla": "STL", "nome": "Modelo 3D STL" }
  ]
}
```

---

## RF-007 — Arquivo

### Endpoints

| Método | Rota                            | Descrição         |
|--------|---------------------------------|-------------------|
| POST   | `/api/arquivos`                 | Upload de arquivo |
| GET    | `/api/arquivos/{id}/download`   | Download          |
| DELETE | `/api/arquivos/{id}`            | Remover arquivo   |

### POST /api/arquivos

**Content-Type:** `multipart/form-data`

| Campo         | Tipo       | Obrig. | Validação         |
|---------------|------------|--------|-------------------|
| tipoArquivoId | form-field | sim    | FK deve existir   |
| file          | file       | sim    | Não vazio, max 10 MB |

**Response 201:**
```json
{
  "data": {
    "id": 7,
    "tipoArquivo": { "id": 1, "sigla": "PDF", "nome": "Documento PDF" }
  }
}
```

Response 413: `{ "error": { "code": "ARQUIVO_MUITO_GRANDE", "message": "Tamanho máximo permitido é 10 MB." } }`

### GET /api/arquivos/{id}/download — Response 200

```
Content-Type: application/octet-stream
Content-Disposition: attachment; filename="arquivo_7.pdf"
[bytes do arquivo]
```

### DELETE /api/arquivos/{id}

**Response 204.**

Response 409 (vinculado): `{ "error": { "code": "ARQUIVO_VINCULADO", "message": "Arquivo está vinculado a um item ou versão e não pode ser removido." } }`

---

## RF-008 — Vínculo Item ↔ Arquivo (1:1)

> O arquivo vinculado é incluído no GET `/api/itens/{id}` (ver RF-004).

### Endpoints

| Método | Rota                       | Descrição                   |
|--------|----------------------------|-----------------------------|
| PUT    | `/api/itens/{id}/arquivo`  | Vincular arquivo ao item    |
| DELETE | `/api/itens/{id}/arquivo`  | Desvincular arquivo do item |

### PUT /api/itens/{id}/arquivo

**Request Body:**
```json
{ "arquivoId": 7 }
```

**Comportamento:**
- Se o item já tiver vínculo, o vínculo anterior é substituído (o arquivo não é deletado)
- Retorna 409 se o arquivo já está vinculado a **outro** item

**Response 200:**
```json
{
  "data": {
    "itemId": 1,
    "arquivo": { "id": 7, "tipoArquivo": { "id": 1, "sigla": "PDF", "nome": "Documento PDF" } }
  }
}
```

Response 409: `{ "error": { "code": "ARQUIVO_JA_VINCULADO", "message": "Arquivo já está vinculado a outro item." } }`

### DELETE /api/itens/{id}/arquivo

Remove somente o vínculo. O arquivo permanece em `arquivo`.

**Response 204.**

Response 404 (sem vínculo): `{ "error": { "code": "VINCULO_NOT_FOUND", "message": "Item não possui arquivo vinculado." } }`

---

## RF-009 — Vínculo ProdutoVersao ↔ Arquivo (1:1)

> O arquivo vinculado é incluído no GET `/api/versoes/{id}` (ver RF-003).

### Endpoints

| Método | Rota                          | Descrição                      |
|--------|-------------------------------|--------------------------------|
| PUT    | `/api/versoes/{id}/arquivo`   | Vincular arquivo à versão      |
| DELETE | `/api/versoes/{id}/arquivo`   | Desvincular arquivo da versão  |

### PUT /api/versoes/{id}/arquivo

**Request Body:**
```json
{ "arquivoId": 5 }
```

**Comportamento:** idêntico ao RF-008 — substitui vínculo existente, 409 se já vinculado a outra versão.

**Response 200:**
```json
{
  "data": {
    "produtoVersaoId": 1,
    "arquivo": { "id": 5, "tipoArquivo": { "id": 1, "sigla": "PDF", "nome": "Documento PDF" } }
  }
}
```

Response 409: `{ "error": { "code": "ARQUIVO_JA_VINCULADO", "message": "Arquivo já está vinculado a outra versão." } }`

### DELETE /api/versoes/{id}/arquivo

**Response 204.**

Response 404 (sem vínculo): `{ "error": { "code": "VINCULO_NOT_FOUND", "message": "Versão não possui arquivo vinculado." } }`

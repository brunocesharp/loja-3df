# Requisitos: Mapeamento de Banco de Dados PostgreSQL para ASP.NET Core 8

**Versão:** 1.0 | **Data:** 2026-03-14

## 1. Visão Geral

Este documento especifica os requisitos para mapear o schema PostgreSQL em uma aplicação ASP.NET Core 8 com Clean Architecture. O sistema gerencia produtos por tipo, versões, itens com custos e arquivos associados, seguindo o padrão CRUD existente (Repository Pattern + EF Core).

---

## 2. Escopo

### Dentro do Escopo
- 8 tabelas mapeadas com relacionamentos corretos no EF Core
- APIs REST CRUD para todas as entidades
- DTOs com validações por DataAnnotations
- Upload/download de arquivos (BYTEA)
- Queries com JOIN (Include) para dados relacionados
- Paginação para listagens

### Fora do Escopo
- Autenticação/autorização
- Interface de usuário
- Migrações de dados históricos

---

## 3. Requisitos Funcionais

### RF-001 — TipoProduto (Lookup Read-Only)

**Descrição:** Tabela de referência pré-populada com tipos de produto. Somente leitura via API.

**Mapeamento da Entidade C#:**

| Coluna SQL        | Propriedade C#  | Tipo C#  | Validação              |
|-------------------|-----------------|----------|------------------------|
| id                | Id              | long     | Chave primária         |
| sigla             | Sigla           | string   | Required, MaxLength(20)|
| nome              | Nome            | string   | Required, MaxLength(100)|
| descricao         | Descricao       | string?  | —                      |

**Endpoints:**

| Método | Rota                        | Resposta              |
|--------|-----------------------------|-----------------------|
| GET    | /api/tipos-produto          | 200 + lista           |
| GET    | /api/tipos-produto/{id}     | 200 + objeto / 404    |

**Regras de Negócio:**
- Sem Create/Update/Delete via API — dados gerenciados por seed ou DBA
- Seed inicial: `ORGN`, `ACSS`, `MINI`

---

### RF-002 — Produto

**Descrição:** Produto principal vinculado a um tipo. Possui campos de auditoria e flag de ativação.

**Mapeamento da Entidade C#:**

| Coluna SQL        | Propriedade C#    | Tipo C#    | Validação                        |
|-------------------|-------------------|------------|----------------------------------|
| id                | Id                | long       | Chave primária                   |
| tipo_produto_id   | TipoProdutoId     | long       | Required, FK para TipoProduto    |
| nome              | Nome              | string     | Required, MaxLength(150)         |
| descricao         | Descricao         | string?    | —                                |
| data_criacao      | DataCriacao       | DateTime   | Auto: `CURRENT_TIMESTAMP`        |
| data_atualizacao  | DataAtualizacao   | DateTime?  | Auto: atualizado em cada UPDATE  |
| ativado           | Ativado           | bool       | Default: true                    |

**CreateProdutoDto:**

| Campo         | Tipo    | Validação                     |
|---------------|---------|-------------------------------|
| TipoProdutoId | long    | Required                      |
| Nome          | string  | Required, MaxLength(150)      |
| Descricao     | string? | —                             |
| Ativado       | bool    | Default: true                 |

**UpdateProdutoDto:**

| Campo         | Tipo    | Validação                     |
|---------------|---------|-------------------------------|
| TipoProdutoId | long    | Required                      |
| Nome          | string  | Required, MaxLength(150)      |
| Descricao     | string? | —                             |
| Ativado       | bool    | Required                      |

**Endpoints:**

| Método | Rota                | Body              | Resposta              |
|--------|---------------------|-------------------|-----------------------|
| GET    | /api/produtos       | — (page, pageSize)| 200 + lista paginada  |
| GET    | /api/produtos/{id}  | —                 | 200 + objeto / 404    |
| POST   | /api/produtos       | CreateProdutoDto  | 201 Created + objeto  |
| PUT    | /api/produtos/{id}  | UpdateProdutoDto  | 200 + objeto / 404    |
| DELETE | /api/produtos/{id}  | —                 | 204 / 404             |

**Regras de Negócio:**
- `TipoProdutoId` deve existir em `tipo_produto`
- DELETE em cascata para `produto_versao`
- GET retorna `TipoProduto` via navegação (Include)

---

### RF-003 — ProdutoVersao

**Descrição:** Versão de um produto. Número deve ser único por produto.

**Mapeamento da Entidade C#:**

| Coluna SQL        | Propriedade C#    | Tipo C#  | Validação                          |
|-------------------|-------------------|----------|------------------------------------|
| id                | Id                | long     | Chave primária                     |
| produto_id        | ProdutoId         | long     | Required, FK para Produto          |
| nome              | Nome              | string   | Required, MaxLength(150)           |
| numero            | Numero            | int      | Required, Unique por ProdutoId     |

**CreateProdutoVersaoDto:**

| Campo     | Tipo   | Validação                |
|-----------|--------|--------------------------|
| ProdutoId | long   | Required                 |
| Nome      | string | Required, MaxLength(150) |
| Numero    | int    | Required, >= 1           |

**UpdateProdutoVersaoDto:**

| Campo  | Tipo   | Validação                |
|--------|--------|--------------------------|
| Nome   | string | Required, MaxLength(150) |
| Numero | int    | Required, >= 1           |

**Endpoints:**

| Método | Rota                                  | Body                   | Resposta               |
|--------|---------------------------------------|------------------------|------------------------|
| GET    | /api/produtos/{produtoId}/versoes     | —                      | 200 + lista            |
| GET    | /api/versoes/{id}                     | —                      | 200 + objeto / 404     |
| POST   | /api/produtos/{produtoId}/versoes     | CreateProdutoVersaoDto | 201 Created + objeto   |
| PUT    | /api/versoes/{id}                     | UpdateProdutoVersaoDto | 200 + objeto / 404     |
| DELETE | /api/versoes/{id}                     | —                      | 204 / 404              |

**Regras de Negócio:**
- Validar unicidade de `(produto_id, numero)` — retornar 422 se duplicado
- DELETE em cascata para `item` e `produto_arquivo`

---

### RF-004 — Item

**Descrição:** Item pertencente a uma versão de produto.

**Mapeamento da Entidade C#:**

| Coluna SQL         | Propriedade C#    | Tipo C#  | Validação                       |
|--------------------|-------------------|----------|---------------------------------|
| id                 | Id                | long     | Chave primária                  |
| produto_versao_id  | ProdutoVersaoId   | long     | Required, FK para ProdutoVersao |
| nome               | Nome              | string   | Required, MaxLength(150)        |
| descricao          | Descricao         | string?  | —                               |

**CreateItemDto:**

| Campo           | Tipo    | Validação                |
|-----------------|---------|--------------------------|
| ProdutoVersaoId | long    | Required                 |
| Nome            | string  | Required, MaxLength(150) |
| Descricao       | string? | —                        |

**UpdateItemDto:**

| Campo     | Tipo    | Validação                |
|-----------|---------|--------------------------|
| Nome      | string  | Required, MaxLength(150) |
| Descricao | string? | —                        |

**Endpoints:**

| Método | Rota                                       | Body          | Resposta             |
|--------|--------------------------------------------|---------------|----------------------|
| GET    | /api/versoes/{versaoId}/itens              | —             | 200 + lista          |
| GET    | /api/itens/{id}                            | —             | 200 + objeto / 404   |
| POST   | /api/versoes/{versaoId}/itens              | CreateItemDto | 201 Created + objeto |
| PUT    | /api/itens/{id}                            | UpdateItemDto | 200 + objeto / 404   |
| DELETE | /api/itens/{id}                            | —             | 204 / 404            |

**Regras de Negócio:**
- GET retorna `ItemCusto` e dados do arquivo vinculado (se existir)
- DELETE em cascata para `item_custo` e `item_arquivo`

---

### RF-005 — ItemCusto

**Descrição:** Dados de custo associados a um item (peso, tempo, quantidade, perdas).

**Mapeamento da Entidade C#:**

| Coluna SQL  | Propriedade C# | Tipo C#    | Validação                   |
|-------------|----------------|------------|-----------------------------|
| id          | Id             | long       | Chave primária              |
| item_id     | ItemId         | long       | Required, FK para Item      |
| peso        | Peso           | decimal?   | Min(0), Precision(12,3)     |
| tempo       | Tempo          | decimal?   | Min(0), Precision(12,2)     |
| quantidade  | Quantidade     | decimal?   | Min(0), Precision(12,3)     |
| perdas      | Perdas         | decimal?   | Min(0), Precision(12,3)     |

**CreateItemCustoDto:**

| Campo      | Tipo     | Validação               |
|------------|----------|-------------------------|
| ItemId     | long     | Required                |
| Peso       | decimal? | Range(0, double.MaxValue)|
| Tempo      | decimal? | Range(0, double.MaxValue)|
| Quantidade | decimal? | Range(0, double.MaxValue)|
| Perdas     | decimal? | Range(0, double.MaxValue)|

**UpdateItemCustoDto:** (mesmos campos sem ItemId)

**Endpoints:**

| Método | Rota                            | Body               | Resposta             |
|--------|---------------------------------|--------------------|----------------------|
| GET    | /api/itens/{itemId}/custos      | —                  | 200 + lista          |
| GET    | /api/custos/{id}                | —                  | 200 + objeto / 404   |
| POST   | /api/itens/{itemId}/custos      | CreateItemCustoDto | 201 Created + objeto |
| PUT    | /api/custos/{id}                | UpdateItemCustoDto | 200 + objeto / 404   |
| DELETE | /api/custos/{id}                | —                  | 204 / 404            |

---

### RF-006 — TipoArquivo (Lookup Read-Only)

**Descrição:** Tabela de referência de tipos de arquivo. Somente leitura via API.

**Mapeamento da Entidade C#:**

| Coluna SQL | Propriedade C# | Tipo C# | Validação               |
|------------|----------------|---------|-------------------------|
| id         | Id             | long    | Chave primária          |
| nome       | Nome           | string  | Required, MaxLength(100)|
| sigla      | Sigla          | string  | Required, MaxLength(20) |

**Endpoints:**

| Método | Rota                       | Resposta           |
|--------|----------------------------|--------------------|
| GET    | /api/tipos-arquivo         | 200 + lista        |
| GET    | /api/tipos-arquivo/{id}    | 200 + objeto / 404 |

---

### RF-007 — Arquivo (Upload/Download)

**Descrição:** Armazenamento de arquivos binários (BYTEA) com tipo associado.

**Mapeamento da Entidade C#:**

| Coluna SQL     | Propriedade C#  | Tipo C#  | Validação                    |
|----------------|-----------------|----------|------------------------------|
| id             | Id              | long     | Chave primária               |
| tipo_arquivo_id| TipoArquivoId   | long     | Required, FK para TipoArquivo|
| byte           | Bytes           | byte[]?  | MaxLength 10MB               |

**CreateArquivoDto:** Recebido como `multipart/form-data`

| Campo         | Tipo    | Validação                  |
|---------------|---------|----------------------------|
| TipoArquivoId | long    | Required                   |
| Arquivo       | IFormFile | Required, MaxSize: 10MB  |

**Endpoints:**

| Método | Rota                       | Body / Params         | Resposta                      |
|--------|----------------------------|-----------------------|-------------------------------|
| POST   | /api/arquivos              | multipart/form-data   | 201 Created + { id, tipo }   |
| GET    | /api/arquivos/{id}/download| —                     | 200 FileResult / 404          |
| DELETE | /api/arquivos/{id}         | —                     | 204 / 404 / 409 se vinculado  |

**Regras de Negócio:**
- Propriedade C# nomeada `Bytes` (evitar palavra reservada `byte`)
- Validar tamanho máximo: 10 MB
- DELETE retorna 409 Conflict se arquivo está vinculado a Item ou ProdutoVersao
- Download retorna `Content-Disposition: attachment; filename="arquivo_{id}"`

---

### RF-008 — ItemArquivo (Vínculo 1:1 Item ↔ Arquivo)

**Descrição:** Tabela pivot que vincula um Item a um Arquivo. Cardinalidade 1:1 em ambas as direções (unique constraints).

**Mapeamento da Entidade C#:**

| Coluna SQL   | Propriedade C# | Tipo C# | Validação             |
|--------------|----------------|---------|-----------------------|
| id           | Id             | long    | Chave primária        |
| id_item      | ItemId         | long    | Required, Unique, FK  |
| id_arquivo   | ArquivoId      | long    | Required, Unique, FK  |

**Endpoints (gerenciados via Item):**

| Método | Rota                         | Body              | Resposta           |
|--------|------------------------------|-------------------|--------------------|
| GET    | /api/itens/{id}              | —                 | Inclui arquivo     |
| PUT    | /api/itens/{id}/arquivo      | `{ arquivoId }`   | 200 / 404 / 409    |
| DELETE | /api/itens/{id}/arquivo      | —                 | 204 / 404          |

**Regras de Negócio:**
- PUT retorna 409 Conflict se o arquivo já está vinculado a outro Item
- PUT substitui o vínculo existente (remove o anterior antes de criar o novo)
- DELETE remove apenas o vínculo, não o arquivo

---

### RF-009 — ProdutoArquivo (Vínculo 1:1 ProdutoVersao ↔ Arquivo)

**Descrição:** Tabela pivot que vincula uma ProdutoVersao a um Arquivo. Mesmo padrão de ItemArquivo.

**Mapeamento da Entidade C#:**

| Coluna SQL         | Propriedade C#  | Tipo C# | Validação             |
|--------------------|-----------------|---------|-----------------------|
| id                 | Id              | long    | Chave primária        |
| id_produto_versao  | ProdutoVersaoId | long    | Required, Unique, FK  |
| id_arquivo         | ArquivoId       | long    | Required, Unique, FK  |

**Endpoints (gerenciados via ProdutoVersao):**

| Método | Rota                              | Body            | Resposta        |
|--------|-----------------------------------|-----------------|-----------------|
| GET    | /api/versoes/{id}                 | —               | Inclui arquivo  |
| PUT    | /api/versoes/{id}/arquivo         | `{ arquivoId }` | 200 / 404 / 409 |
| DELETE | /api/versoes/{id}/arquivo         | —               | 204 / 404       |

**Regras de Negócio:**
- Mesmas regras de RF-008 aplicadas para ProdutoVersao

---

## 4. Requisitos Não-Funcionais

### RNF-001 — Validação de Entrada
- Todos os DTOs usam `System.ComponentModel.DataAnnotations`
- Campos FK são validados no repositório/serviço (verificar existência)
- Retornar `400 Bad Request` para validações de formato
- Retornar `422 Unprocessable Entity` para violações de regra de negócio

### RNF-002 — Paginação
- Parâmetros: `?page=1&pageSize=20` (default pageSize: 20, máximo: 100)
- Resposta inclui: `{ data: [...], total: N, page: 1, pageSize: 20 }`
- Obrigatório para listagens de Produto, Item e ItemCusto

### RNF-003 — Performance de Queries
- Usar `AsNoTracking()` em todas as operações de leitura
- Limitar profundidade de Include() a 2 níveis por padrão
- Criar índices no EF Core correspondendo aos da DDL (`ix_*`)

### RNF-004 — Upload de Arquivos
- Tamanho máximo: 10 MB por arquivo
- Validar MIME type antes de salvar
- Não expor o conteúdo binário em endpoints JSON (apenas no download)

### RNF-005 — Tratamento de Erros
- Todos os endpoints retornam JSON padronizado em erros:
  ```json
  { "error": "Mensagem legível", "code": "PRODUTO_NOT_FOUND" }
  ```
- 404 para entidades não encontradas
- 409 para conflitos de integridade
- 500 com log estruturado para erros inesperados

### RNF-006 — Transações
- Operações em múltiplas tabelas (ex: criar arquivo + criar vínculo) devem usar `using var tx = await _context.Database.BeginTransactionAsync()`

### RNF-007 — Nomenclatura
- Entidades C#: PascalCase (ex: `ProdutoVersao`, `ItemCusto`)
- Propriedades: PascalCase (ex: `DataCriacao`, `TipoProdutoId`)
- Rotas: kebab-case plural (ex: `/api/tipos-produto`, `/api/produto-versoes`)
- JSON: camelCase via `JsonNamingPolicy.CamelCase`

---

## 5. Modelo de Relacionamento (resumido)

```
tipo_produto (1) ──< produto (N)
produto      (1) ──< produto_versao (N)
produto_versao (1) ──< item (N)
produto_versao (1) ──0..1── produto_arquivo ──1── arquivo
item           (1) ──< item_custo (N)
item           (1) ──0..1── item_arquivo ──1── arquivo
tipo_arquivo   (1) ──< arquivo (N)
```

---

## 6. Ordem de Implementação (por dependência)

1. `TipoArquivo` (sem dependências)
2. `TipoProduto` (sem dependências)
3. `Arquivo` (depende de TipoArquivo)
4. `Produto` (depende de TipoProduto)
5. `ProdutoVersao` (depende de Produto)
6. `Item` (depende de ProdutoVersao)
7. `ItemCusto` (depende de Item)
8. `ItemArquivo` (depende de Item + Arquivo)
9. `ProdutoArquivo` (depende de ProdutoVersao + Arquivo)

---

## 7. Histórico de Mudanças

| Versão | Data       | Descrição                                 |
|--------|------------|-------------------------------------------|
| 1.0    | 2026-03-14 | Versão inicial baseada no schema PostgreSQL |

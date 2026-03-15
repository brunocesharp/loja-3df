# TODO — Plano de Execução: git-issue-TODO-001

**Data:** 2026-03-14
**Baseado em:** specification.md + design.md

## Observações sobre o código existente

A entidade `Item` atual (`int Id`, `Name`, `IsActive`) é um **scaffold de exemplo** sem relação com o schema desta issue. As novas entidades usam `long` para IDs e nomenclatura em português conforme o banco.

**Decisão:** remover o scaffold de `Item` e implementar todas as entidades do schema desde o início.

---

## Caminho de dependências

```
TipoArquivo ──┐
              ├──▶ Arquivo ──────────────────────┐
TipoProduto ──┘                                   │
     │                                            │
     └──▶ Produto ──▶ ProdutoVersao ──┬──▶ ProdutoArquivo (pivot)
                           │          │
                           └──▶ Item ─┴──▶ ItemArquivo (pivot)
                                 │
                                 └──▶ ItemCusto
```

---

## Fase 0 — Limpeza do scaffold

- [ ] **TASK-000:** Remover entidade `Item` existente (scaffold de exemplo)
  - Apagar `Domain/Entities/Item.cs`
  - Apagar `Domain/Interfaces/IItemRepository.cs`
  - Apagar `Application/DTOs/ItemDtos.cs`
  - Apagar `Data/Repositories/ItemRepository.cs`
  - Apagar `Presentation/.../Controllers/ItemsController.cs`
  - Remover `DbSet<Item>` e configuração do `AppDbContext.cs`
  - Remover registro do repositório no `Program.cs`

---

## Fase 1 — Lookups (sem dependências)

### TASK-001 — TipoArquivo

- [ ] `Domain/Entities/TipoArquivo.cs` — propriedades: `Id` (long), `Nome` (string), `Sigla` (string)
- [ ] `Domain/Interfaces/ITipoArquivoRepository.cs` — apenas `GetAllAsync` e `GetByIdAsync`
- [ ] `Application/DTOs/TipoArquivoDtos.cs` — somente `TipoArquivoDto` de leitura (sem Create/Update)
- [ ] `Data/Context/AppDbContext.cs` — adicionar `DbSet<TipoArquivo>` + configuração no `OnModelCreating`
  - `sigla`: MaxLength(20), IsRequired, IsUnique
  - `nome`: MaxLength(100), IsRequired
  - Seed: inserir dados via `HasData` (popular após TASK-101 definir IDs)
- [ ] `Data/Repositories/TipoArquivoRepository.cs` — implementar apenas leitura
- [ ] `Presentation/.../Controllers/TiposArquivoController.cs` — `GET /api/tipos-arquivo` e `GET /api/tipos-arquivo/{id}`
- [ ] Registrar `ITipoArquivoRepository` no `Program.cs`

### TASK-002 — TipoProduto

- [ ] `Domain/Entities/TipoProduto.cs` — propriedades: `Id` (long), `Sigla` (string), `Nome` (string), `Descricao` (string?)
- [ ] `Domain/Interfaces/ITipoProdutoRepository.cs` — apenas `GetAllAsync` e `GetByIdAsync`
- [ ] `Application/DTOs/TipoProdutoDtos.cs` — somente `TipoProdutoDto` de leitura
- [ ] `Data/Context/AppDbContext.cs` — adicionar `DbSet<TipoProduto>` + configuração
  - `sigla`: MaxLength(20), IsRequired, IsUnique
  - `nome`: MaxLength(100), IsRequired
- [ ] `Data/Repositories/TipoProdutoRepository.cs` — implementar apenas leitura
- [ ] `Presentation/.../Controllers/TiposProdutoController.cs` — `GET /api/tipos-produto` e `GET /api/tipos-produto/{id}`
- [ ] Registrar `ITipoProdutoRepository` no `Program.cs`

---

## Fase 2 — Arquivo (depende de TipoArquivo)

### TASK-003 — Arquivo

- [ ] `Domain/Entities/Arquivo.cs` — propriedades: `Id` (long), `TipoArquivoId` (long), `Bytes` (byte[]?) + navigation `TipoArquivo`
- [ ] `Domain/Interfaces/IArquivoRepository.cs` — `GetByIdAsync`, `CreateAsync`, `DeleteAsync`, `IsVinculadoAsync`
- [ ] `Application/DTOs/ArquivoDtos.cs`:
  - `ArquivoResponseDto` — retorna apenas `id` e `tipoArquivo` (nunca expõe os bytes no JSON)
  - Sem DTO de criação (upload via `IFormFile` diretamente no controller)
- [ ] `Data/Context/AppDbContext.cs` — adicionar `DbSet<Arquivo>` + configuração
  - `byte` → coluna `byte` (nome reservado: usar `HasColumnName("byte")` ou configurar no `OnModelCreating`)
  - FK para `TipoArquivo` com `ON DELETE RESTRICT`
- [ ] `Data/Repositories/ArquivoRepository.cs`
  - `IsVinculadoAsync(long id)` — verifica se existe `item_arquivo` ou `produto_arquivo` com esse `id`
- [ ] `Presentation/.../Controllers/ArquivosController.cs`
  - `POST /api/arquivos` — `[Consumes("multipart/form-data")]`, recebe `IFormFile` + `tipoArquivoId`, salva bytes no banco, retorna 201
  - `GET /api/arquivos/{id}/download` — retorna `FileContentResult` com `Content-Disposition: attachment`
  - `DELETE /api/arquivos/{id}` — retorna 409 se `IsVinculadoAsync` for true
  - Validar tamanho máximo 10 MB (configurar em `appsettings.json` e checar no controller)
- [ ] Registrar `IArquivoRepository` no `Program.cs`
- [ ] Adicionar `builder.Services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = 10_485_760)` no `Program.cs`

---

## Fase 3 — Produto (depende de TipoProduto)

### TASK-004 — Produto

- [ ] `Domain/Entities/Produto.cs` — propriedades: `Id` (long), `TipoProdutoId` (long), `Nome` (string), `Descricao` (string?), `DataCriacao` (DateTime), `DataAtualizacao` (DateTime?), `Ativado` (bool) + navigation `TipoProduto`
- [ ] `Domain/Interfaces/IProdutoRepository.cs` — `GetAllAsync(page, pageSize, ativado?, tipoProdutoId?)`, `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`
- [ ] `Application/DTOs/ProdutoDtos.cs`:
  - `CreateProdutoDto` — `TipoProdutoId`, `Nome`, `Descricao?`, `Ativado` (default true)
  - `UpdateProdutoDto` — mesmos campos, todos obrigatórios
  - `ProdutoResponseDto` — inclui `TipoProdutoDto` aninhado
- [ ] `Data/Context/AppDbContext.cs` — adicionar `DbSet<Produto>` + configuração
  - `nome`: MaxLength(150), IsRequired
  - `data_criacao`: `HasDefaultValueSql("NOW()")`
  - FK para `TipoProduto` com `ON DELETE RESTRICT`, `ON UPDATE CASCADE`
  - Índice: `ix_produto_tipo_produto_id`
- [ ] `Data/Repositories/ProdutoRepository.cs`
  - `GetAllAsync` usa `.Include(p => p.TipoProduto)`, `AsNoTracking()`, `Skip/Take` para paginação
  - `GetByIdAsync` usa `.Include(p => p.TipoProduto)`
  - `UpdateAsync` seta `DataAtualizacao = DateTime.UtcNow`
- [ ] `Presentation/.../Controllers/ProdutosController.cs` — CRUD completo
  - Validar que `TipoProdutoId` existe antes de Create/Update (retorna 422 se não existir)
- [ ] Registrar `IProdutoRepository` no `Program.cs`

---

## Fase 4 — ProdutoVersao (depende de Produto)

### TASK-005 — ProdutoVersao

- [ ] `Domain/Entities/ProdutoVersao.cs` — propriedades: `Id` (long), `ProdutoId` (long), `Nome` (string), `Numero` (int) + navigation `Produto`, `Arquivo?`
- [ ] `Domain/Interfaces/IProdutoVersaoRepository.cs` — `GetByProdutoIdAsync`, `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `NumeroExisteAsync(produtoId, numero)`
- [ ] `Application/DTOs/ProdutoVersaoDtos.cs`:
  - `CreateProdutoVersaoDto` — `Nome`, `Numero`
  - `UpdateProdutoVersaoDto` — `Nome`, `Numero`
  - `ProdutoVersaoResponseDto` — inclui `ProdutoResponseDto` aninhado e `ArquivoResponseDto?`
- [ ] `Data/Context/AppDbContext.cs` — adicionar `DbSet<ProdutoVersao>` + configuração
  - `nome`: MaxLength(150), IsRequired
  - Unique constraint: `uq_produto_versao_produto_numero` em `(ProdutoId, Numero)`
  - FK para `Produto` com `ON DELETE CASCADE`
  - Índice: `ix_produto_versao_produto_id`
- [ ] `Data/Repositories/ProdutoVersaoRepository.cs`
  - `NumeroExisteAsync` — verifica duplicata antes de criar/atualizar
  - `GetByIdAsync` faz `.Include(v => v.Produto)` e `.Include(v => v.ProdutoArquivo!.Arquivo.TipoArquivo)`
- [ ] `Presentation/.../Controllers/ProdutoVersoesController.cs`
  - `GET /api/produtos/{produtoId}/versoes`
  - `GET /api/versoes/{id}`
  - `POST /api/produtos/{produtoId}/versoes` — validar `NumeroExisteAsync`, retornar 422 se duplicado
  - `PUT /api/versoes/{id}`
  - `DELETE /api/versoes/{id}`
- [ ] Registrar `IProdutoVersaoRepository` no `Program.cs`

---

## Fase 5 — Item (depende de ProdutoVersao)

### TASK-006 — Item

- [ ] `Domain/Entities/Item.cs` — propriedades: `Id` (long), `ProdutoVersaoId` (long), `Nome` (string), `Descricao` (string?) + navigations `ProdutoVersao`, `Custos`, `ItemArquivo?`
- [ ] `Domain/Interfaces/IItemRepository.cs` — `GetByVersaoIdAsync`, `GetByIdAsync` (com includes), `CreateAsync`, `UpdateAsync`, `DeleteAsync`
- [ ] `Application/DTOs/ItemDtos.cs`:
  - `CreateItemDto` — `Nome`, `Descricao?`
  - `UpdateItemDto` — `Nome`, `Descricao?`
  - `ItemResponseDto` — inclui `ProdutoVersaoResponseDto`, lista `ItemCustoDto`, `ArquivoResponseDto?`
- [ ] `Data/Context/AppDbContext.cs` — adicionar `DbSet<Item>` + configuração
  - `nome`: MaxLength(150), IsRequired
  - FK para `ProdutoVersao` com `ON DELETE CASCADE`
  - Índice: `ix_item_produto_versao_id`
- [ ] `Data/Repositories/ItemRepository.cs`
  - `GetByIdAsync` faz `.Include(i => i.ProdutoVersao.Produto)`, `.Include(i => i.Custos)`, `.Include(i => i.ItemArquivo!.Arquivo.TipoArquivo)`
- [ ] `Presentation/.../Controllers/ItensController.cs`
  - `GET /api/versoes/{versaoId}/itens`
  - `GET /api/itens/{id}` — retorna custos e arquivo vinculado
  - `POST /api/versoes/{versaoId}/itens`
  - `PUT /api/itens/{id}`
  - `DELETE /api/itens/{id}`
- [ ] Registrar `IItemRepository` no `Program.cs`

---

## Fase 6 — ItemCusto (depende de Item)

### TASK-007 — ItemCusto

- [ ] `Domain/Entities/ItemCusto.cs` — propriedades: `Id` (long), `ItemId` (long), `Peso` (decimal?), `Tempo` (decimal?), `Quantidade` (decimal?), `Perdas` (decimal?) + navigation `Item`
- [ ] `Domain/Interfaces/IItemCustoRepository.cs` — `GetByItemIdAsync`, `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`
- [ ] `Application/DTOs/ItemCustoDtos.cs`:
  - `CreateItemCustoDto` — todos os campos opcionais (`decimal?`), validação `[Range(0, double.MaxValue)]`
  - `UpdateItemCustoDto` — mesmos campos
  - `ItemCustoResponseDto`
- [ ] `Data/Context/AppDbContext.cs` — adicionar `DbSet<ItemCusto>` + configuração
  - `peso`: `HasColumnType("numeric(12,3)")`
  - `tempo`: `HasColumnType("numeric(12,2)")`
  - `quantidade`: `HasColumnType("numeric(12,3)")`
  - `perdas`: `HasColumnType("numeric(12,3)")`
  - FK para `Item` com `ON DELETE CASCADE`
  - Índice: `ix_item_custo_item_id`
- [ ] `Data/Repositories/ItemCustoRepository.cs`
- [ ] `Presentation/.../Controllers/ItemCustosController.cs`
  - `GET /api/itens/{itemId}/custos`
  - `GET /api/custos/{id}`
  - `POST /api/itens/{itemId}/custos` — validar ao menos um campo não nulo (retorna 422)
  - `PUT /api/custos/{id}`
  - `DELETE /api/custos/{id}`
- [ ] Registrar `IItemCustoRepository` no `Program.cs`

---

## Fase 7 — Pivots de Arquivo (dependem de Item, ProdutoVersao e Arquivo)

### TASK-008 — ItemArquivo (pivot 1:1)

- [ ] `Domain/Entities/ItemArquivo.cs` — propriedades: `Id` (long), `ItemId` (long), `ArquivoId` (long) + navigations `Item`, `Arquivo`
- [ ] `Domain/Interfaces/IItemArquivoRepository.cs` — `GetByItemIdAsync`, `VincularAsync(itemId, arquivoId)`, `DesvincularAsync(itemId)`
- [ ] `Data/Context/AppDbContext.cs` — adicionar `DbSet<ItemArquivo>` + configuração
  - Unique constraint em `ItemId` (`uq_item_arquivo_item`)
  - Unique constraint em `ArquivoId` (`uq_item_arquivo_arquivo`)
  - FK `ItemId` com `ON DELETE CASCADE`
  - FK `ArquivoId` com `ON DELETE CASCADE`
- [ ] `Data/Repositories/ItemArquivoRepository.cs`
  - `VincularAsync` — se já existe vínculo para o `itemId`, substituir (dentro de transação)
  - Retornar 409 se `arquivoId` já está vinculado a outro item
- [ ] Adicionar endpoints no `ItensController.cs`
  - `PUT /api/itens/{id}/arquivo` — `{ "arquivoId": 7 }` → retorna 200 com dados do arquivo
  - `DELETE /api/itens/{id}/arquivo` → retorna 204; 404 se não há vínculo
- [ ] Registrar `IItemArquivoRepository` no `Program.cs`

### TASK-009 — ProdutoArquivo (pivot 1:1)

- [ ] `Domain/Entities/ProdutoArquivo.cs` — propriedades: `Id` (long), `ProdutoVersaoId` (long), `ArquivoId` (long) + navigations `ProdutoVersao`, `Arquivo`
- [ ] `Domain/Interfaces/IProdutoArquivoRepository.cs` — `GetByVersaoIdAsync`, `VincularAsync(versaoId, arquivoId)`, `DesvincularAsync(versaoId)`
- [ ] `Data/Context/AppDbContext.cs` — adicionar `DbSet<ProdutoArquivo>` + configuração
  - Unique constraint em `ProdutoVersaoId` (`uq_produto_arquivo_produto_versao`)
  - Unique constraint em `ArquivoId` (`uq_produto_arquivo_arquivo`)
  - FK `ProdutoVersaoId` com `ON DELETE CASCADE`
  - FK `ArquivoId` com `ON DELETE CASCADE`
- [ ] `Data/Repositories/ProdutoArquivoRepository.cs`
  - Mesma lógica transacional de `ItemArquivoRepository`
  - Retornar 409 se `arquivoId` já está vinculado a outra versão
- [ ] Adicionar endpoints no `ProdutoVersoesController.cs`
  - `PUT /api/versoes/{id}/arquivo`
  - `DELETE /api/versoes/{id}/arquivo`
- [ ] Registrar `IProdutoArquivoRepository` no `Program.cs`

---

## Fase 8 — Seed data e Migration

### TASK-010 — Seed data para lookups

- [ ] Adicionar `HasData` no `OnModelCreating` para `TipoProduto`:
  ```
  id=1 sigla=ORGN nome=ORGANIZADORES
  id=2 sigla=ACSS nome=ACESSORIOS
  id=3 sigla=MINI nome=MINIATURA
  ```
- [ ] Popular `TipoArquivo` com tipos relevantes (ex: PDF, PNG, STL, DXF, STEP)

### TASK-011 — Migration EF Core

- [ ] Executar `dotnet ef migrations add ImplementarSistemaPrecificacao --project src/Data --startup-project src/Presentation/AssistenteDB.Api`
- [ ] Revisar o arquivo de migration gerado:
  - Verificar se todas as 9 tabelas estão presentes
  - Confirmar unique constraints compostos em `produto_versao` e nos pivots
  - Confirmar índices `ix_*` gerados
  - Confirmar seed data nas tabelas lookup
- [ ] Executar `dotnet ef database update`
- [ ] Testar endpoints via Swagger em `https://localhost:9595/swagger`

---

## Checklist final

- [ ] Todos os endpoints documentados no Swagger (via `[ProducesResponseType]` ou anotações XML)
- [ ] Todos os repositórios registrados no `Program.cs` com `AddScoped`
- [ ] Resposta de erro padronizada `{ "error": { "code": "...", "message": "..." } }` em todos os 404/409/422
- [ ] Upload de arquivo limitado a 10 MB configurado no `Program.cs`
- [ ] `AsNoTracking()` aplicado em todas as queries de leitura

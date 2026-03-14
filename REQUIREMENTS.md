# Guia de Requisitos e Fluxo de Trabalho

## Organização das Especificações

Todas as especificações do projeto estão armazenadas na pasta `.claudedoc/`, organizadas em subpastas individuais por issue. Cada subpasta segue o padrão de nomenclatura:

```
.claudedoc/git-issue-{STATUS}-{NNN}/
```

Onde:
- **`{STATUS}`** indica a situação atual da issue:
  - `TODO` — especificação aguardando implementação
  - `DOING` — implementação em andamento
  - `DONE` — implementação concluída
- **`{NNN}`** é o número sequencial da issue com zero-padding (ex: `001`, `002`, `042`)

**Exemplos:**
```
.claudedoc/git-issue-TODO-001/   ← próxima a ser implementada
.claudedoc/git-issue-DOING-002/  ← em andamento
.claudedoc/git-issue-DONE-003/   ← concluída
```

---

## Conteúdo de uma Issue

Cada pasta de issue contém os seguintes arquivos, criados progressivamente conforme o fluxo de trabalho:

| Arquivo            | Criado em     | Descrição                                                    |
|--------------------|---------------|--------------------------------------------------------------|
| `specification.md` | Pré-existente | Detalhamento funcional: entidades, regras de negócio, DTOs   |
| `design.md`        | Etapa Design  | Desenho dos endpoints REST: rotas, contratos, status codes   |
| `TODO.md`          | Etapa Plan    | Tarefas de implementação ordenadas por dependência           |

---

## Fluxo de Trabalho do Agente

Ao receber uma tarefa de implementação, o agente deve seguir este fluxo:

1. **Localizar a próxima issue pendente** — listar as pastas em `.claudedoc/` e selecionar a de menor número com status `TODO`
2. **Ler a especificação** — abrir e interpretar o arquivo `specification.md` da issue selecionada
3. **Design** — criar o arquivo `design.md` dentro da pasta da issue com o detalhamento dos endpoints da API: rotas, verbos HTTP, contratos de request/response e status codes
4. **Plan** — criar o arquivo `TODO.md` dentro da pasta da issue com as tarefas de implementação ordenadas por dependência
5. **Implementar** — executar as tarefas listadas no `TODO.md` seguindo os padrões da codebase, marcando cada tarefa como concluída ao finalizá-la
6. **Atualizar o status** — ao concluir, renomear a pasta trocando `TODO` → `DONE` (ou `DOING` se for uma pausa intermediária)

---

## Padrões da Codebase

A aplicação segue **Clean Architecture** em ASP.NET Core 8 com as seguintes camadas:

| Camada         | Projeto                        | Responsabilidade                            |
|----------------|--------------------------------|---------------------------------------------|
| Domain         | `AssistenteDB.Domain`          | Entidades e interfaces de repositório       |
| Application    | `AssistenteDB.Application`     | DTOs e regras de aplicação                  |
| Data           | `AssistenteDB.Data`            | DbContext EF Core e implementação dos repos |
| Presentation   | `AssistenteDB.Api`             | Controllers REST e configuração da API      |

Ao implementar uma nova entidade, o padrão a seguir é:

1. **Entity** em `Domain/Entities/`
2. **Interface** `IXxxRepository` em `Domain/Interfaces/`
3. **DTOs** `CreateXxxDto` e `UpdateXxxDto` em `Application/DTOs/`
4. **DbSet** registrado em `Data/Context/AppDbContext.cs`
5. **Repository** `XxxRepository` em `Data/Repositories/`
6. **Controller** `XxxsController` em `Presentation/.../Controllers/`

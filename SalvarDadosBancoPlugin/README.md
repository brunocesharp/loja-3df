# MyApi — ASP.NET + PostgreSQL CRUD

API REST em C# com ASP.NET Core 8 e PostgreSQL via Entity Framework Core.

## Estrutura do Projeto

```
MyApi/
├── Controllers/
│   └── ItemsController.cs   # Endpoints CRUD
├── Data/
│   └── AppDbContext.cs       # Contexto EF Core
├── DTOs/
│   └── ItemDtos.cs           # Objetos de transferência
├── Models/
│   └── Item.cs               # Entidade principal
├── Repositories/
│   ├── IItemRepository.cs    # Interface
│   └── ItemRepository.cs     # Implementação
├── appsettings.json          # Connection string
├── MyApi.csproj              # Dependências
└── Program.cs                # Bootstrap da aplicação
```

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL rodando localmente ou em nuvem

## Configuração

1. **Edite a connection string** em `appsettings.json`:
   ```json
   "DefaultConnection": "Host=localhost;Port=5432;Database=mydb;Username=postgres;Password=sua_senha"
   ```

2. **Instale as dependências:**
   ```bash
   dotnet restore
   ```

3. **Crie e aplique a migration:**
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

4. **Rode a aplicação:**
   ```bash
   dotnet run
   ```

5. Acesse o Swagger em: `http://localhost:5000/swagger`

## Endpoints

| Método | Rota            | Descrição             |
|--------|-----------------|-----------------------|
| GET    | /api/items      | Lista todos os itens  |
| GET    | /api/items/{id} | Busca item por ID     |
| POST   | /api/items      | Cria novo item        |
| PUT    | /api/items/{id} | Atualiza item         |
| DELETE | /api/items/{id} | Remove item           |

## Exemplo de Payload

### POST /api/items
```json
{
  "name": "Meu Item",
  "description": "Descrição opcional"
}
```

### PUT /api/items/1
```json
{
  "name": "Nome Atualizado",
  "description": "Nova descrição",
  "isActive": true
}
```

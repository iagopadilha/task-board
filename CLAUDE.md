# CLAUDE.md

Projeto de estudo: mini Kanban para praticar a pirâmide de testes, OpenIddict e OAuth2. Detalhes em `README.md`.

## Idioma

Responder no idioma do usuário (português ou inglês). Código, nomes e commits em inglês.

## Stack

- Backend: ASP.NET Core .NET 8, Minimal API, em `backend/TaskBoard.Api`
- Banco: PostgreSQL 18 via `docker-compose.yaml` (porta host 5433)
- ORM: EF Core 8 Code First, Npgsql, snake_case (`UseSnakeCaseNamingConvention`)
- Frontend (a criar): React + Vite + TypeScript

## Comandos

```bash
docker compose up -d                              # sobe o Postgres
cd backend/TaskBoard.Api
dotnet build
dotnet run
dotnet ef migrations add <Nome>                   # nova migration
dotnet ef database update                         # aplica migrations
```

## Arquitetura do backend

Projeto único `TaskBoard.Api`, organizado por pastas:

- `Domain/`: entidades e regras. Zero dependência de infraestrutura.
- `Data/`: `AppDbContext`, configurações de entidade, extensões de banco.
- `Repositories/`: interfaces e implementações.
- `Services/`: casos de uso.
- `Endpoints/`: Minimal API, agrupados por recurso.
- `Dtos/`, `Middleware/`, `Security/`, `ExtensionMethods/`.

## Convenções de domínio

- Entidades usam **factory method estático `Create`** + construtor privado vazio (para o EF) + setters `private`. Não criar construtor público nem setter público.
- Validação e invariantes ficam na entidade, lançando `DomainValidationException` (dado inválido) ou `BusinessRuleException` (regra de negócio).
- "Agora" sempre entra por parâmetro (`nowUtc`). Não usar `DateTime.UtcNow` dentro do domínio.
- Datas em UTC (`timestamptz` no Npgsql só aceita UTC).
- Regras que precisam do banco (ex.: limite de 5 tarefas `InProgress` por usuário) ficam no service, não na entidade.

## Divisão de trabalho

O Iago faz os testes e a autenticação. **Não criar projetos de teste nem frameworks de teste** sem pedido explícito. Foco da base: domínio, API, banco.

## Segredos e arquivos locais

- Nunca commitar `.env`, `.specs/` nem `PROJECT_CONTEXT.md` (já no `.gitignore`).
- Repo público: não colocar senhas reais em `appsettings*.json`. Para novas credenciais usar user-secrets ou `.env`.

## Manter o README atualizado

Ao concluir uma etapa, marcar o item na seção **Status atual** do `README.md` e ajustar **Em andamento**.

## Git

Commits pequenos e atômicos, mensagens em inglês no formato Conventional Commits (`feat:`, `fix:`, `docs:`, `chore:`).

### Fluxo de branches

- Branch padrão de trabalho é **`developer`**. Todo commit e push vai primeiro para ela.
- **Nunca** commitar nem dar push direto na `main`.
- Para levar mudanças à `main`, abrir **Pull Request `developer` → `main`** (`gh pr create --base main --head developer`).
- Se a `developer` não existir, criar a partir da `main` antes de começar.

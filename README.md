# task-board

Projeto de estudo para aplicar os conceitos da **pirâmide de testes** + **OpenIddict e OAuth2**.
É um mini Kanban (boards e tarefas) com regra de negócio, banco e UI suficientes para exercitar unit, integração e E2E.

## Stack

| Camada | Tecnologia |
|---|---|
| Backend | ASP.NET Core **.NET 8**, Minimal API |
| Banco | PostgreSQL 18 (Docker) |
| ORM | EF Core 8 Code First + Migrations, snake_case (`EFCore.NamingConventions`) |
| Frontend | React + Vite + TypeScript (a criar) |
| Testes | xUnit, Moq, FluentAssertions, Testcontainers, Vitest, React Testing Library, MSW, Playwright (a criar) |

## Como rodar

Pré-requisitos: Docker Desktop, SDK .NET 8, `dotnet-ef` (`dotnet tool install --global dotnet-ef`).

```bash
# 1. variaveis de ambiente
cp .env.example .env          # ajuste PGS_DB_PASSWORD

# 2. banco (porta 5433 no host)
docker compose up -d

# 3. migrations + API
cd backend/TaskBoard.Api
dotnet ef database update
dotnet run
```

A connection string de desenvolvimento fica em `backend/TaskBoard.Api/appsettings.json` e precisa bater com a senha do `.env`.

## Estrutura

```
task-board/
├── backend/
│   └── TaskBoard.Api/     # Domain, Data, Services, Repositories, Endpoints, Middleware
├── docker-compose.yaml    # Postgres
├── CLAUDE.md              # instrucoes para o Claude Code
└── README.md
```

## Regras de negócio (resumo)

- Tarefa: título 3–100 caracteres, prazo não pode estar no passado, status inicial `Todo`, prioridade padrão `Medium`.
- Máquina de estados: `Todo → InProgress → Review → Done` (`Review` pode voltar a `InProgress`). `Done` é imutável.
- `InProgress` exige responsável; máximo de 5 tarefas `InProgress` por usuário (limite WIP).
- Só o dono do board exclui; tarefa `InProgress` não pode ser excluída.

## Status atual

> Atualize esta seção a cada etapa concluída.

- [x] Domínio (`User`, `Board`, `TaskItem`) com regras e máquina de estados
- [x] Minimal API: endpoints de usuário, board e tarefa, DTOs, middleware global de erro
- [x] Postgres via Docker Compose
- [x] `AppDbContext` com Npgsql + snake_case
- [x] Migration inicial (`FirstMigration`)
- [ ] Autenticação (JWT / OpenIddict + OAuth2)
- [ ] Testes de unidade (C#)
- [ ] Testes de integração (WebApplicationFactory + Testcontainers)
- [ ] Frontend React + Vite + TS
- [ ] Testes de frontend (Vitest, RTL, MSW)
- [ ] Testes E2E (Playwright)

## Em andamento

- Versionamento no GitHub e documentação do projeto.

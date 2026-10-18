# task-board

Projeto de estudo para aplicar os conceitos da **pirâmide de testes** + **autenticação simples com BCrypt (padrão de hashing)**.
É um mini Kanban (boards e tarefas) com regra de negócio, banco e UI suficientes para exercitar testes de unidade, integração e E2E — começando **bare-hand** (sem bibliotecas auxiliares, testando e mockando na mão para dominar os fundamentos) e depois evoluindo para bibliotecas do ecossistema.

## Stack

| Camada | Tecnologia |
|---|---|
| Backend | ASP.NET Core **.NET 8**, Minimal API |
| Banco | PostgreSQL 18 (Docker) |
| ORM | EF Core 8 Code First + Migrations, snake_case (`EFCore.NamingConventions`) |
| Auth | Autenticação simples (Email/Senha) + **BCrypt** (padrão de hashing e salting) + JWT |
| Frontend | React + Vite + TypeScript (a criar) |
| Testes | Bare-hand primeiro → xUnit, Moq, FluentAssertions, Testcontainers, Vitest, React Testing Library, MSW, Playwright |

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
│   └── TaskBoard.Api/     # Domain, Data, Services, Repositories, Endpoints, Middleware, Security
├── docker-compose.yaml    # Postgres
├── CLAUDE.md              # instrucoes para o agente
└── README.md
```

## Regras de negócio (resumo)

- Tarefa: título 3–100 caracteres, prazo não pode estar no passado, status inicial `Todo`, prioridade padrão `Medium`.
- Máquina de estados: `Todo → InProgress → Review → Done` (`Review` pode voltar a `InProgress`). `Done` é imutável.
- `InProgress` exige responsável; máximo de 5 tarefas `InProgress` por usuário (limite WIP).
- Só o dono do board exclui; tarefa `InProgress` não pode ser excluída.

## Estratégia de estudo

1. **Autenticação simples e Hashing (BCrypt)**
   - Aplicar o padrão de hashing (`IPasswordHasher`) isolando a criptografia do domínio/serviços.
   - Entender e aplicar **BCrypt** (salt embutido, work factor/cost, verificação segura `Verify` contra timing/brute-force).
   - Fluxo direto de registro (`POST /api/auth/register` ou criação de usuário com senha) e login (`POST /api/auth/login`).
2. **Pirâmide de Testes (Bare-hand → Libraries)**
   - **Fase 1 (Bare-hand):** testar regras de domínio e serviços construindo asserts, fakes/stubs manuais e cenários sem depender de bibliotecas de mock/assertion para entender o que acontece por baixo dos panos.
   - **Fase 2 (Com bibliotecas):** introduzir gradualmente `FluentAssertions`, `Moq`/`NSubstitute`, `WebApplicationFactory` + `Testcontainers` (integração com Postgres real), e no frontend `Vitest`, `RTL`, `MSW` e `Playwright` (E2E).

## Status atual

> Atualize esta seção a cada etapa concluída.

- [x] Domínio (`User`, `Board`, `TaskItem`) com regras e máquina de estados
- [x] Minimal API: endpoints de usuário, board e tarefa, DTOs, middleware global de erro
- [x] Postgres via Docker Compose
- [x] `AppDbContext` com Npgsql + snake_case
- [x] Migration inicial (`FirstMigration`)
- [ ] Autenticação simples com BCrypt (padrão de hashing)
  - [ ] Abstração de hashing (`IPasswordHasher`) e implementação com BCrypt (`Hash` e `Verify`)
  - [ ] Registro de usuário recebendo senha em texto plano, validando política mínima e salvando `PasswordHash`
  - [ ] Endpoint de login validando credenciais com BCrypt e autenticando requisições (`ICurrentUser`)
- [ ] Testes de unidade no Backend (C#)
  - [ ] Fase bare-hand (asserts puros e fakes/mocks manuais)
  - [ ] Evolução com libs (`Moq`, `FluentAssertions`)
- [ ] Testes de integração no Backend (`WebApplicationFactory` + `Testcontainers`)
- [ ] Frontend React + Vite + TS
- [ ] Testes de frontend (`Vitest`, `React Testing Library`, `MSW`)
- [ ] Testes E2E (`Playwright`)

## Em andamento

- Ajustando documentação e preparando base para autenticação simples com BCrypt e primeiros testes bare-hand.

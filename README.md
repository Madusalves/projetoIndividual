# Projeto Individual – Sistema de Biblioteca (C# / Windows Forms)

Aplicação desktop em C# (.NET 8, Windows Forms) para gerenciar acervo, usuários e empréstimos de uma biblioteca.
Os dados ficam em um **PostgreSQL rodando em Docker, somente local** (porta exposta apenas em `127.0.0.1:5433`).

## Executar

```text
docker compose up -d          # sobe o banco (cria as tabelas e dados de exemplo na primeira vez)
cd BibliotecaApp
dotnet run
```

Para parar: `docker compose down` (os dados continuam no volume). Para recriar do zero: `docker compose down -v`.
A conexão pode ser trocada pela variável de ambiente `BIBLIOTECA_DB`.

## Entrega do banco

- [db/init.sql](db/init.sql) – script de criação das tabelas e dados de exemplo
- [db/backup_biblioteca.sql](db/backup_biblioteca.sql) – backup (`pg_dump`)
- [docs/dicionario_de_dados.md](docs/dicionario_de_dados.md) – dicionário de dados

Restaurar o backup: `docker exec -i biblioteca-db psql -U biblioteca biblioteca < db/backup_biblioteca.sql` (em um banco vazio).
Gerar novo backup: `docker exec biblioteca-db pg_dump -U biblioteca --no-owner biblioteca > db/backup_biblioteca.sql`.

## Conceitos de POO aplicados

| Conceito | Onde |
|---|---|
| Classes abstratas e herança | `ItemAcervo` → `Livro`, `Revista`, `Dvd`; `Usuario` → `Aluno`, `Professor` |
| Polimorfismo | `PrazoDias`, `MultaPorDia`, `Detalhes`, `LimiteEmprestimos`, `FatorMulta` sobrescritos em cada subclasse |
| Interface | `IEmprestavel` |
| Encapsulamento | Propriedades com validação (`Titulo`, `Ano`, `Nome`), `set` privado em `Disponivel`, `Multa` |
| Exceções personalizadas | `BibliotecaException`, tratada na interface |
| Eventos / delegates | `Biblioteca.Alterado` atualiza as telas |
| Camada de dados | `Repositorio` (ADO.NET/Npgsql, parâmetros e transações) |
| Coleções e LINQ | `List<T>`, `IReadOnlyList<T>`, `Where/Select/Count` |
| Sobrecarga | `Biblioteca.Adicionar(ItemAcervo)` e `Adicionar(Usuario)` |

## Regras de negócio

- Livro: 14 dias, multa R$ 1,00/dia · Revista: 7 dias, R$ 0,50/dia · DVD: 3 dias, R$ 2,00/dia.
- Aluno: até 3 empréstimos simultâneos · Professor: até 6 e 50% de desconto na multa.
- Não é possível emprestar item já emprestado, remover item emprestado ou remover usuário com empréstimo em aberto.
- Para testar multa, escolha uma data de devolução posterior ao prazo na aba Empréstimos.

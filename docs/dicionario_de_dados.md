# Dicionário de Dados – Banco `biblioteca` (PostgreSQL 16)

## itens_acervo
Livros, revistas e DVDs (uma tabela com coluna discriminadora `tipo`).

| Coluna | Tipo | Nulo | Descrição |
|---|---|---|---|
| id | SERIAL (PK) | não | Identificador do item |
| tipo | VARCHAR(10) | não | `Livro`, `Revista` ou `DVD` |
| titulo | VARCHAR(200) | não | Título |
| ano | INTEGER | não | Ano de publicação (1000–9999) |
| disponivel | BOOLEAN | não | `true` se pode ser emprestado (padrão `true`) |
| autor | VARCHAR(150) | sim | Somente Livro |
| paginas | INTEGER | sim | Somente Livro (> 0) |
| edicao | INTEGER | sim | Somente Revista (> 0) |
| duracao_min | INTEGER | sim | Somente DVD, em minutos (> 0) |

Restrição `ck_campos_tipo`: cada tipo exige os seus campos específicos.

## usuarios

| Coluna | Tipo | Nulo | Descrição |
|---|---|---|---|
| id | SERIAL (PK) | não | Identificador do usuário |
| perfil | VARCHAR(10) | não | `Aluno` (limite 3) ou `Professor` (limite 6) |
| nome | VARCHAR(150) | não | Nome completo |
| email | VARCHAR(150) | não | E-mail (único) |

## emprestimos

| Coluna | Tipo | Nulo | Descrição |
|---|---|---|---|
| id | SERIAL (PK) | não | Identificador do empréstimo |
| item_id | INTEGER (FK → itens_acervo.id) | não | Item emprestado |
| usuario_id | INTEGER (FK → usuarios.id) | não | Quem pegou |
| data_emprestimo | DATE | não | Data da retirada |
| data_prevista | DATE | não | Data limite (retirada + prazo do tipo) |
| data_devolucao | DATE | sim | Preenchida na devolução; nula = em aberto |
| multa | NUMERIC(10,2) | não | Multa calculada na devolução (padrão 0) |

Índices: `ux_emprestimo_aberto_item` (único parcial: um item não pode ter dois empréstimos em aberto) e `ix_emprestimos_usuario`.

## Relacionamentos

```text
usuarios 1 ──── N emprestimos N ──── 1 itens_acervo
```

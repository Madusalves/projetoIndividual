-- Esquema do banco da Biblioteca (executado na primeira subida do container).

CREATE TABLE itens_acervo (
    id           SERIAL PRIMARY KEY,
    tipo         VARCHAR(10)  NOT NULL CHECK (tipo IN ('Livro', 'Revista', 'DVD')),
    titulo       VARCHAR(200) NOT NULL,
    ano          INTEGER      NOT NULL CHECK (ano BETWEEN 1000 AND 9999),
    disponivel   BOOLEAN      NOT NULL DEFAULT TRUE,
    autor        VARCHAR(150),
    paginas      INTEGER CHECK (paginas > 0),
    edicao       INTEGER CHECK (edicao > 0),
    duracao_min  INTEGER CHECK (duracao_min > 0),
    -- cada tipo exige seus próprios campos
    CONSTRAINT ck_campos_tipo CHECK (
        (tipo = 'Livro'   AND autor IS NOT NULL AND paginas IS NOT NULL) OR
        (tipo = 'Revista' AND edicao IS NOT NULL) OR
        (tipo = 'DVD'     AND duracao_min IS NOT NULL)
    )
);

CREATE TABLE usuarios (
    id      SERIAL PRIMARY KEY,
    perfil  VARCHAR(10)  NOT NULL CHECK (perfil IN ('Aluno', 'Professor')),
    nome    VARCHAR(150) NOT NULL,
    email   VARCHAR(150) NOT NULL UNIQUE
);

CREATE TABLE emprestimos (
    id               SERIAL PRIMARY KEY,
    item_id          INTEGER NOT NULL REFERENCES itens_acervo(id),
    usuario_id       INTEGER NOT NULL REFERENCES usuarios(id),
    data_emprestimo  DATE    NOT NULL,
    data_prevista    DATE    NOT NULL,
    data_devolucao   DATE,
    multa            NUMERIC(10,2) NOT NULL DEFAULT 0 CHECK (multa >= 0),
    CHECK (data_prevista >= data_emprestimo),
    CHECK (data_devolucao IS NULL OR data_devolucao >= data_emprestimo)
);

-- um item só pode ter um empréstimo em aberto
CREATE UNIQUE INDEX ux_emprestimo_aberto_item
    ON emprestimos (item_id) WHERE data_devolucao IS NULL;
CREATE INDEX ix_emprestimos_usuario ON emprestimos (usuario_id);

-- dados de exemplo
INSERT INTO itens_acervo (tipo, titulo, ano, autor, paginas) VALUES
    ('Livro', 'Dom Casmurro', 1899, 'Machado de Assis', 256),
    ('Livro', 'O Cortiço', 1890, 'Aluísio Azevedo', 280);
INSERT INTO itens_acervo (tipo, titulo, ano, edicao) VALUES
    ('Revista', 'Superinteressante', 2023, 450);
INSERT INTO itens_acervo (tipo, titulo, ano, duracao_min) VALUES
    ('DVD', 'Cidade de Deus', 2002, 130);
INSERT INTO usuarios (perfil, nome, email) VALUES
    ('Aluno', 'Maria Silva', 'maria@escola.com'),
    ('Professor', 'João Pereira', 'joao@escola.com');

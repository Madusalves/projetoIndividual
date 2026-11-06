--
-- PostgreSQL database dump
--

\restrict bJbmnbIv7sZVn7ndAaH00PWfydgAcCxQz4x2UaXLeEaOV0CRSpO5D003Q4y6nCO

-- Dumped from database version 16.15
-- Dumped by pg_dump version 16.15

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: emprestimos; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.emprestimos (
    id integer NOT NULL,
    item_id integer NOT NULL,
    usuario_id integer NOT NULL,
    data_emprestimo date NOT NULL,
    data_prevista date NOT NULL,
    data_devolucao date,
    multa numeric(10,2) DEFAULT 0 NOT NULL,
    CONSTRAINT emprestimos_check CHECK ((data_prevista >= data_emprestimo)),
    CONSTRAINT emprestimos_check1 CHECK (((data_devolucao IS NULL) OR (data_devolucao >= data_emprestimo))),
    CONSTRAINT emprestimos_multa_check CHECK ((multa >= (0)::numeric))
);


--
-- Name: emprestimos_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.emprestimos_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: emprestimos_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.emprestimos_id_seq OWNED BY public.emprestimos.id;


--
-- Name: itens_acervo; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.itens_acervo (
    id integer NOT NULL,
    tipo character varying(10) NOT NULL,
    titulo character varying(200) NOT NULL,
    ano integer NOT NULL,
    disponivel boolean DEFAULT true NOT NULL,
    autor character varying(150),
    paginas integer,
    edicao integer,
    duracao_min integer,
    CONSTRAINT ck_campos_tipo CHECK (((((tipo)::text = 'Livro'::text) AND (autor IS NOT NULL) AND (paginas IS NOT NULL)) OR (((tipo)::text = 'Revista'::text) AND (edicao IS NOT NULL)) OR (((tipo)::text = 'DVD'::text) AND (duracao_min IS NOT NULL)))),
    CONSTRAINT itens_acervo_ano_check CHECK (((ano >= 1000) AND (ano <= 9999))),
    CONSTRAINT itens_acervo_duracao_min_check CHECK ((duracao_min > 0)),
    CONSTRAINT itens_acervo_edicao_check CHECK ((edicao > 0)),
    CONSTRAINT itens_acervo_paginas_check CHECK ((paginas > 0)),
    CONSTRAINT itens_acervo_tipo_check CHECK (((tipo)::text = ANY ((ARRAY['Livro'::character varying, 'Revista'::character varying, 'DVD'::character varying])::text[])))
);


--
-- Name: itens_acervo_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.itens_acervo_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: itens_acervo_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.itens_acervo_id_seq OWNED BY public.itens_acervo.id;


--
-- Name: usuarios; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.usuarios (
    id integer NOT NULL,
    perfil character varying(10) NOT NULL,
    nome character varying(150) NOT NULL,
    email character varying(150) NOT NULL,
    CONSTRAINT usuarios_perfil_check CHECK (((perfil)::text = ANY ((ARRAY['Aluno'::character varying, 'Professor'::character varying])::text[])))
);


--
-- Name: usuarios_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.usuarios_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: usuarios_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.usuarios_id_seq OWNED BY public.usuarios.id;


--
-- Name: emprestimos id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.emprestimos ALTER COLUMN id SET DEFAULT nextval('public.emprestimos_id_seq'::regclass);


--
-- Name: itens_acervo id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.itens_acervo ALTER COLUMN id SET DEFAULT nextval('public.itens_acervo_id_seq'::regclass);


--
-- Name: usuarios id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.usuarios ALTER COLUMN id SET DEFAULT nextval('public.usuarios_id_seq'::regclass);


--
-- Data for Name: emprestimos; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.emprestimos (id, item_id, usuario_id, data_emprestimo, data_prevista, data_devolucao, multa) FROM stdin;
\.


--
-- Data for Name: itens_acervo; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.itens_acervo (id, tipo, titulo, ano, disponivel, autor, paginas, edicao, duracao_min) FROM stdin;
1	Livro	Dom Casmurro	1899	t	Machado de Assis	256	\N	\N
2	Livro	O Cortiço	1890	t	Aluísio Azevedo	280	\N	\N
3	Revista	Superinteressante	2023	t	\N	\N	450	\N
4	DVD	Cidade de Deus	2002	t	\N	\N	\N	130
\.


--
-- Data for Name: usuarios; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.usuarios (id, perfil, nome, email) FROM stdin;
1	Aluno	Maria Silva	maria@escola.com
2	Professor	João Pereira	joao@escola.com
\.


--
-- Name: emprestimos_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.emprestimos_id_seq', 2, true);


--
-- Name: itens_acervo_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.itens_acervo_id_seq', 4, true);


--
-- Name: usuarios_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.usuarios_id_seq', 2, true);


--
-- Name: emprestimos emprestimos_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.emprestimos
    ADD CONSTRAINT emprestimos_pkey PRIMARY KEY (id);


--
-- Name: itens_acervo itens_acervo_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.itens_acervo
    ADD CONSTRAINT itens_acervo_pkey PRIMARY KEY (id);


--
-- Name: usuarios usuarios_email_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.usuarios
    ADD CONSTRAINT usuarios_email_key UNIQUE (email);


--
-- Name: usuarios usuarios_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.usuarios
    ADD CONSTRAINT usuarios_pkey PRIMARY KEY (id);


--
-- Name: ix_emprestimos_usuario; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_emprestimos_usuario ON public.emprestimos USING btree (usuario_id);


--
-- Name: ux_emprestimo_aberto_item; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ux_emprestimo_aberto_item ON public.emprestimos USING btree (item_id) WHERE (data_devolucao IS NULL);


--
-- Name: emprestimos emprestimos_item_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.emprestimos
    ADD CONSTRAINT emprestimos_item_id_fkey FOREIGN KEY (item_id) REFERENCES public.itens_acervo(id);


--
-- Name: emprestimos emprestimos_usuario_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.emprestimos
    ADD CONSTRAINT emprestimos_usuario_id_fkey FOREIGN KEY (usuario_id) REFERENCES public.usuarios(id);


--
-- PostgreSQL database dump complete
--

\unrestrict bJbmnbIv7sZVn7ndAaH00PWfydgAcCxQz4x2UaXLeEaOV0CRSpO5D003Q4y6nCO


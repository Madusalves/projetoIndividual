using BibliotecaApp.Models;
using Npgsql;

namespace BibliotecaApp.Data;

public class Repositorio
{
    private readonly string _connectionString;

    public Repositorio(string? connectionString = null)
    {
        _connectionString = connectionString
            ?? Environment.GetEnvironmentVariable("BIBLIOTECA_DB")
            ?? "Host=127.0.0.1;Port=5433;Database=biblioteca;Username=biblioteca;Password=biblioteca_local";
    }

    private NpgsqlConnection Abrir()
    {
        var con = new NpgsqlConnection(_connectionString);
        con.Open();
        return con;
    }

    public void TestarConexao()
    {
        using var con = Abrir();
    }

    // ---------- Leitura ----------

    public List<ItemAcervo> ListarItens()
    {
        var lista = new List<ItemAcervo>();
        using var con = Abrir();
        using var cmd = new NpgsqlCommand(
            "SELECT id, tipo, titulo, ano, disponivel, autor, paginas, edicao, duracao_min FROM itens_acervo ORDER BY id", con);
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            string titulo = r.GetString(2);
            int ano = r.GetInt32(3);
            ItemAcervo item = r.GetString(1) switch
            {
                "Livro" => new Livro(titulo, ano, r.GetString(5), r.GetInt32(6)),
                "Revista" => new Revista(titulo, ano, r.GetInt32(7)),
                _ => new Dvd(titulo, ano, r.GetInt32(8)),
            };
            item.Id = r.GetInt32(0);
            item.DefinirDisponivel(r.GetBoolean(4));
            lista.Add(item);
        }
        return lista;
    }

    public List<Usuario> ListarUsuarios()
    {
        var lista = new List<Usuario>();
        using var con = Abrir();
        using var cmd = new NpgsqlCommand("SELECT id, perfil, nome, email FROM usuarios ORDER BY id", con);
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            Usuario u = r.GetString(1) == "Professor"
                ? new Professor(r.GetString(2), r.GetString(3))
                : new Aluno(r.GetString(2), r.GetString(3));
            u.Id = r.GetInt32(0);
            lista.Add(u);
        }
        return lista;
    }

    public List<Emprestimo> ListarEmprestimos(IEnumerable<ItemAcervo> itens, IEnumerable<Usuario> usuarios)
    {
        var porItem = itens.ToDictionary(i => i.Id);
        var porUsuario = usuarios.ToDictionary(u => u.Id);
        var lista = new List<Emprestimo>();
        using var con = Abrir();
        using var cmd = new NpgsqlCommand(
            "SELECT id, item_id, usuario_id, data_emprestimo, data_prevista, data_devolucao, multa FROM emprestimos ORDER BY id", con);
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            lista.Add(new Emprestimo(
                r.GetInt32(0), porItem[r.GetInt32(1)], porUsuario[r.GetInt32(2)],
                r.GetDateTime(3), r.GetDateTime(4),
                r.IsDBNull(5) ? null : r.GetDateTime(5), r.GetDecimal(6)));
        }
        return lista;
    }

    // ---------- Escrita ----------

    public void Inserir(ItemAcervo item)
    {
        using var con = Abrir();
        using var cmd = new NpgsqlCommand(
            @"INSERT INTO itens_acervo (tipo, titulo, ano, disponivel, autor, paginas, edicao, duracao_min)
              VALUES (@tipo, @titulo, @ano, @disp, @autor, @pag, @ed, @dur) RETURNING id", con);
        cmd.Parameters.AddWithValue("tipo", item.Tipo);
        cmd.Parameters.AddWithValue("titulo", item.Titulo);
        cmd.Parameters.AddWithValue("ano", item.Ano);
        cmd.Parameters.AddWithValue("disp", item.Disponivel);
        cmd.Parameters.AddWithValue("autor", (object?)(item as Livro)?.Autor ?? DBNull.Value);
        cmd.Parameters.AddWithValue("pag", (object?)(item as Livro)?.Paginas ?? DBNull.Value);
        cmd.Parameters.AddWithValue("ed", (object?)(item as Revista)?.Edicao ?? DBNull.Value);
        cmd.Parameters.AddWithValue("dur", (object?)(item as Dvd)?.DuracaoMinutos ?? DBNull.Value);
        item.Id = (int)cmd.ExecuteScalar()!;
    }

    public void Inserir(Usuario u)
    {
        using var con = Abrir();
        using var cmd = new NpgsqlCommand(
            "INSERT INTO usuarios (perfil, nome, email) VALUES (@p, @n, @e) RETURNING id", con);
        cmd.Parameters.AddWithValue("p", u.Perfil);
        cmd.Parameters.AddWithValue("n", u.Nome);
        cmd.Parameters.AddWithValue("e", u.Email);
        try { u.Id = (int)cmd.ExecuteScalar()!; }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new BibliotecaException("Já existe um usuário com este e-mail.");
        }
    }

    public void ExcluirItem(int id) => Excluir("DELETE FROM itens_acervo WHERE id = @id", id);
    public void ExcluirUsuario(int id) => Excluir("DELETE FROM usuarios WHERE id = @id", id);

    private void Excluir(string sql, int id)
    {
        using var con = Abrir();
        using var cmd = new NpgsqlCommand(sql, con);
        cmd.Parameters.AddWithValue("id", id);
        try { cmd.ExecuteNonQuery(); }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new BibliotecaException("Registro possui histórico de empréstimos e não pode ser removido.");
        }
    }

    public void RegistrarEmprestimo(Emprestimo e)
    {
        using var con = Abrir();
        using var tx = con.BeginTransaction();
        using (var cmd = new NpgsqlCommand(
            @"INSERT INTO emprestimos (item_id, usuario_id, data_emprestimo, data_prevista)
              VALUES (@i, @u, @de, @dp) RETURNING id", con, tx))
        {
            cmd.Parameters.AddWithValue("i", e.Item.Id);
            cmd.Parameters.AddWithValue("u", e.Usuario.Id);
            cmd.Parameters.AddWithValue("de", e.DataEmprestimo);
            cmd.Parameters.AddWithValue("dp", e.DataPrevista);
            e.Id = (int)cmd.ExecuteScalar()!;
        }
        AtualizarDisponibilidade(con, tx, e.Item.Id, false);
        tx.Commit();
    }

    public void RegistrarDevolucao(Emprestimo e)
    {
        using var con = Abrir();
        using var tx = con.BeginTransaction();
        using (var cmd = new NpgsqlCommand(
            "UPDATE emprestimos SET data_devolucao = @d, multa = @m WHERE id = @id", con, tx))
        {
            cmd.Parameters.AddWithValue("d", e.DataDevolucao!.Value);
            cmd.Parameters.AddWithValue("m", e.Multa);
            cmd.Parameters.AddWithValue("id", e.Id);
            cmd.ExecuteNonQuery();
        }
        AtualizarDisponibilidade(con, tx, e.Item.Id, true);
        tx.Commit();
    }

    private static void AtualizarDisponibilidade(NpgsqlConnection con, NpgsqlTransaction tx, int itemId, bool disp)
    {
        using var cmd = new NpgsqlCommand("UPDATE itens_acervo SET disponivel = @d WHERE id = @id", con, tx);
        cmd.Parameters.AddWithValue("d", disp);
        cmd.Parameters.AddWithValue("id", itemId);
        cmd.ExecuteNonQuery();
    }
}

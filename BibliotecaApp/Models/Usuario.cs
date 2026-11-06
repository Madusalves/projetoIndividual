namespace BibliotecaApp.Models;

public abstract class Usuario
{
    private string _nome = "";

    protected Usuario(string nome, string email)
    {
        Nome = nome;
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new BibliotecaException("E-mail inválido.");
        Email = email.Trim();
    }

    public int Id { get; internal set; }
    public string Email { get; }

    public string Nome
    {
        get => _nome;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BibliotecaException("O nome é obrigatório.");
            _nome = value.Trim();
        }
    }

    public abstract string Perfil { get; }
    public abstract int LimiteEmprestimos { get; }

    public virtual decimal FatorMulta => 1m;

    public override string ToString() => $"{Nome} ({Perfil})";
}

public class Aluno : Usuario
{
    public Aluno(string nome, string email) : base(nome, email) { }
    public override string Perfil => "Aluno";
    public override int LimiteEmprestimos => 3;
}

public class Professor : Usuario
{
    public Professor(string nome, string email) : base(nome, email) { }
    public override string Perfil => "Professor";
    public override int LimiteEmprestimos => 6;
    public override decimal FatorMulta => 0.5m; // 50% de desconto
}

namespace BibliotecaApp.Models;

public interface IEmprestavel
{
    bool Disponivel { get; }
    int PrazoDias { get; }
    void Emprestar();
    void Devolver();
}

public class BibliotecaException : Exception
{
    public BibliotecaException(string mensagem) : base(mensagem) { }
}

public abstract class ItemAcervo : IEmprestavel
{
    private string _titulo = "";
    private int _ano;

    protected ItemAcervo(string titulo, int ano)
    {
        Titulo = titulo;
        Ano = ano;
    }

    public int Id { get; internal set; }

    public string Titulo
    {
        get => _titulo;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BibliotecaException("O título é obrigatório.");
            _titulo = value.Trim();
        }
    }

    public int Ano
    {
        get => _ano;
        set
        {
            if (value < 1000 || value > DateTime.Now.Year)
                throw new BibliotecaException($"Ano inválido (1000 a {DateTime.Now.Year}).");
            _ano = value;
        }
    }

    public bool Disponivel { get; private set; } = true;

    // Cada tipo define suas próprias regras.
    public abstract string Tipo { get; }
    public abstract int PrazoDias { get; }
    protected abstract decimal MultaPorDia { get; }
    public abstract string Detalhes { get; }

    public decimal CalcularMulta(int diasAtraso) =>
        diasAtraso <= 0 ? 0m : diasAtraso * MultaPorDia;

    public void Emprestar()
    {
        if (!Disponivel)
            throw new BibliotecaException($"\"{Titulo}\" já está emprestado.");
        Disponivel = false;
    }

    public void Devolver() => Disponivel = true;

    internal void DefinirDisponivel(bool disponivel) => Disponivel = disponivel;

    public override string ToString() => $"[{Tipo}] {Titulo} ({Ano})";
}

public class Livro : ItemAcervo
{
    public Livro(string titulo, int ano, string autor, int paginas) : base(titulo, ano)
    {
        if (string.IsNullOrWhiteSpace(autor)) throw new BibliotecaException("O autor é obrigatório.");
        if (paginas <= 0) throw new BibliotecaException("Número de páginas inválido.");
        Autor = autor.Trim();
        Paginas = paginas;
    }

    public string Autor { get; }
    public int Paginas { get; }
    public override string Tipo => "Livro";
    public override int PrazoDias => 14;
    protected override decimal MultaPorDia => 1.00m;
    public override string Detalhes => $"{Autor}, {Paginas} páginas";
}

public class Revista : ItemAcervo
{
    public Revista(string titulo, int ano, int edicao) : base(titulo, ano)
    {
        if (edicao <= 0) throw new BibliotecaException("Edição inválida.");
        Edicao = edicao;
    }

    public int Edicao { get; }
    public override string Tipo => "Revista";
    public override int PrazoDias => 7;
    protected override decimal MultaPorDia => 0.50m;
    public override string Detalhes => $"Edição nº {Edicao}";
}

public class Dvd : ItemAcervo
{
    public Dvd(string titulo, int ano, int duracaoMinutos) : base(titulo, ano)
    {
        if (duracaoMinutos <= 0) throw new BibliotecaException("Duração inválida.");
        DuracaoMinutos = duracaoMinutos;
    }

    public int DuracaoMinutos { get; }
    public override string Tipo => "DVD";
    public override int PrazoDias => 3;
    protected override decimal MultaPorDia => 2.00m;
    public override string Detalhes => $"{DuracaoMinutos} min";
}

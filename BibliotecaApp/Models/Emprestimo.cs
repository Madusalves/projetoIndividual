namespace BibliotecaApp.Models;

public class Emprestimo
{
    internal Emprestimo(int id, ItemAcervo item, Usuario usuario, DateTime dataEmprestimo,
        DateTime dataPrevista, DateTime? dataDevolucao, decimal multa)
    {
        Id = id; Item = item; Usuario = usuario;
        DataEmprestimo = dataEmprestimo; DataPrevista = dataPrevista;
        DataDevolucao = dataDevolucao; Multa = multa;
    }

    public Emprestimo(ItemAcervo item, Usuario usuario, DateTime dataEmprestimo)
    {
        Item = item;
        Usuario = usuario;
        DataEmprestimo = dataEmprestimo.Date;
        DataPrevista = DataEmprestimo.AddDays(item.PrazoDias);
    }

    public int Id { get; internal set; }
    public ItemAcervo Item { get; }
    public Usuario Usuario { get; }
    public DateTime DataEmprestimo { get; }
    public DateTime DataPrevista { get; }
    public DateTime? DataDevolucao { get; private set; }
    public decimal Multa { get; private set; }
    public bool Ativo => DataDevolucao is null;

    public int DiasAtraso(DateTime referencia) =>
        Math.Max(0, (referencia.Date - DataPrevista).Days);

    public void Encerrar(DateTime dataDevolucao)
    {
        if (!Ativo) throw new BibliotecaException("Este empréstimo já foi devolvido.");
        if (dataDevolucao.Date < DataEmprestimo)
            throw new BibliotecaException("A devolução não pode ser anterior ao empréstimo.");

        DataDevolucao = dataDevolucao.Date;
        Multa = Item.CalcularMulta(DiasAtraso(dataDevolucao)) * Usuario.FatorMulta;
        Item.Devolver();
    }
}

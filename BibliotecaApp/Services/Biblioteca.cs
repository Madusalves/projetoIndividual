using BibliotecaApp.Data;
using BibliotecaApp.Models;

namespace BibliotecaApp.Services;

public class Biblioteca
{
    private readonly Repositorio _repo;
    private List<ItemAcervo> _itens = new();
    private List<Usuario> _usuarios = new();
    private List<Emprestimo> _emprestimos = new();

    public Biblioteca(Repositorio repo) => _repo = repo;

    public event Action? Alterado;

    public IReadOnlyList<ItemAcervo> Itens => _itens;
    public IReadOnlyList<Usuario> Usuarios => _usuarios;
    public IReadOnlyList<Emprestimo> Emprestimos => _emprestimos;

    public void Carregar()
    {
        _itens = _repo.ListarItens();
        _usuarios = _repo.ListarUsuarios();
        _emprestimos = _repo.ListarEmprestimos(_itens, _usuarios);
        Alterado?.Invoke();
    }

    public void Adicionar(ItemAcervo item)
    {
        _repo.Inserir(item);
        _itens.Add(item);
        Alterado?.Invoke();
    }

    public void Adicionar(Usuario usuario)
    {
        _repo.Inserir(usuario);
        _usuarios.Add(usuario);
        Alterado?.Invoke();
    }

    public void RemoverItem(int id)
    {
        var item = _itens.FirstOrDefault(i => i.Id == id)
            ?? throw new BibliotecaException("Item não encontrado.");
        if (!item.Disponivel)
            throw new BibliotecaException("Não é possível remover um item emprestado.");
        _repo.ExcluirItem(id);
        _itens.Remove(item);
        Alterado?.Invoke();
    }

    public void RemoverUsuario(int id)
    {
        var usuario = _usuarios.FirstOrDefault(u => u.Id == id)
            ?? throw new BibliotecaException("Usuário não encontrado.");
        if (EmprestimosAtivosDe(usuario) > 0)
            throw new BibliotecaException("O usuário possui empréstimos em aberto.");
        _repo.ExcluirUsuario(id);
        _usuarios.Remove(usuario);
        Alterado?.Invoke();
    }

    public int EmprestimosAtivosDe(Usuario u) =>
        _emprestimos.Count(e => e.Ativo && e.Usuario == u);

    public Emprestimo Emprestar(ItemAcervo item, Usuario usuario, DateTime data)
    {
        if (EmprestimosAtivosDe(usuario) >= usuario.LimiteEmprestimos)
            throw new BibliotecaException(
                $"{usuario.Nome} atingiu o limite de {usuario.LimiteEmprestimos} empréstimos.");

        item.Emprestar(); // lança exceção se já estiver emprestado
        var emp = new Emprestimo(item, usuario, data);
        try { _repo.RegistrarEmprestimo(emp); }
        catch { item.Devolver(); throw; } // desfaz a mudança em memória se o banco falhar
        _emprestimos.Add(emp);
        Alterado?.Invoke();
        return emp;
    }

    public Emprestimo Devolver(int emprestimoId, DateTime data)
    {
        var emp = _emprestimos.FirstOrDefault(e => e.Id == emprestimoId)
            ?? throw new BibliotecaException("Empréstimo não encontrado.");
        emp.Encerrar(data);
        _repo.RegistrarDevolucao(emp);
        Alterado?.Invoke();
        return emp;
    }
}

using BibliotecaApp.Models;
using BibliotecaApp.Services;

namespace BibliotecaApp.Forms;

public class MainForm : Form
{
    private readonly Biblioteca _bib;

    // Acervo
    private readonly ComboBox _cbTipo = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 100 };
    private readonly TextBox _txtTitulo = new() { Width = 200 };
    private readonly NumericUpDown _numAno = new() { Minimum = 1000, Maximum = 9999, Value = DateTime.Now.Year, Width = 70 };
    private readonly Label _lblExtra1 = new() { AutoSize = true };
    private readonly TextBox _txtExtra1 = new() { Width = 140 };
    private readonly Label _lblExtra2 = new() { AutoSize = true };
    private readonly NumericUpDown _numExtra2 = new() { Minimum = 1, Maximum = 99999, Value = 100, Width = 70 };
    private readonly DataGridView _gridItens = CriarGrid();

    // Usuários
    private readonly ComboBox _cbPerfil = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 100 };
    private readonly TextBox _txtNome = new() { Width = 200 };
    private readonly TextBox _txtEmail = new() { Width = 200 };
    private readonly DataGridView _gridUsuarios = CriarGrid();

    // Empréstimos
    private readonly ComboBox _cbItem = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
    private readonly ComboBox _cbUsuario = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200 };
    private readonly DateTimePicker _dtEmprestimo = new() { Format = DateTimePickerFormat.Short, Width = 100 };
    private readonly DateTimePicker _dtDevolucao = new() { Format = DateTimePickerFormat.Short, Width = 100 };
    private readonly DataGridView _gridEmprestimos = CriarGrid();

    public MainForm(Biblioteca biblioteca)
    {
        _bib = biblioteca;
        Text = "Biblioteca";
        Size = new Size(900, 560);
        StartPosition = FormStartPosition.CenterScreen;

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(MontarAbaAcervo());
        tabs.TabPages.Add(MontarAbaUsuarios());
        tabs.TabPages.Add(MontarAbaEmprestimos());
        Controls.Add(tabs);

        _bib.Alterado += Atualizar;
        _bib.Carregar();
    }

    private static DataGridView CriarGrid() => new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        RowHeadersVisible = false,
        BackgroundColor = SystemColors.Window,
    };

    private static FlowLayoutPanel Linha(params Control[] controles)
    {
        var p = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(6) };
        foreach (var c in controles)
        {
            c.Margin = new Padding(3, 6, 3, 3);
            p.Controls.Add(c);
        }
        return p;
    }

    private static Label Rotulo(string t) => new() { Text = t, AutoSize = true };

    private static Button Botao(string t, EventHandler h)
    {
        var b = new Button { Text = t, AutoSize = true };
        b.Click += h;
        return b;
    }


    private TabPage MontarAbaAcervo()
    {
        _cbTipo.Items.AddRange(new object[] { "Livro", "Revista", "DVD" });
        _cbTipo.SelectedIndexChanged += (_, _) => AjustarCamposExtras();
        _cbTipo.SelectedIndex = 0;

        var aba = new TabPage("Acervo");
        aba.Controls.Add(_gridItens);
        aba.Controls.Add(Linha(
            Rotulo("Tipo:"), _cbTipo, Rotulo("Título:"), _txtTitulo, Rotulo("Ano:"), _numAno,
            _lblExtra1, _txtExtra1, _lblExtra2, _numExtra2,
            Botao("Adicionar", (_, _) => Executar(AdicionarItem)),
            Botao("Remover selecionado", (_, _) => Executar(RemoverItem))));
        return aba;
    }

    private TabPage MontarAbaUsuarios()
    {
        _cbPerfil.Items.AddRange(new object[] { "Aluno", "Professor" });
        _cbPerfil.SelectedIndex = 0;

        var aba = new TabPage("Usuários");
        aba.Controls.Add(_gridUsuarios);
        aba.Controls.Add(Linha(
            Rotulo("Perfil:"), _cbPerfil, Rotulo("Nome:"), _txtNome, Rotulo("E-mail:"), _txtEmail,
            Botao("Adicionar", (_, _) => Executar(AdicionarUsuario)),
            Botao("Remover selecionado", (_, _) => Executar(RemoverUsuario))));
        return aba;
    }

    private TabPage MontarAbaEmprestimos()
    {
        var aba = new TabPage("Empréstimos");
        aba.Controls.Add(_gridEmprestimos);
        aba.Controls.Add(Linha(
            Rotulo("Data de devolução:"), _dtDevolucao,
            Botao("Devolver selecionado", (_, _) => Executar(Devolver))));
        aba.Controls.Add(Linha(
            Rotulo("Item:"), _cbItem, Rotulo("Usuário:"), _cbUsuario,
            Rotulo("Data:"), _dtEmprestimo,
            Botao("Emprestar", (_, _) => Executar(Emprestar))));
        return aba;
    }

    private void Executar(Action acao)
    {
        try { acao(); }
        catch (BibliotecaException ex)
        {
            MessageBox.Show(ex.Message, "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void AjustarCamposExtras()
    {
        switch (_cbTipo.SelectedItem as string)
        {
            case "Livro": _lblExtra1.Text = "Autor:"; _lblExtra2.Text = "Páginas:"; _txtExtra1.Enabled = true; break;
            case "Revista": _lblExtra1.Text = ""; _lblExtra2.Text = "Edição:"; _txtExtra1.Enabled = false; break;
            default: _lblExtra1.Text = ""; _lblExtra2.Text = "Minutos:"; _txtExtra1.Enabled = false; break;
        }
        _txtExtra1.Visible = _txtExtra1.Enabled;
    }

    private void AdicionarItem()
    {
        var titulo = _txtTitulo.Text;
        var ano = (int)_numAno.Value;
        var n = (int)_numExtra2.Value;

        ItemAcervo item = (_cbTipo.SelectedItem as string) switch
        {
            "Livro" => new Livro(titulo, ano, _txtExtra1.Text, n),
            "Revista" => new Revista(titulo, ano, n),
            _ => new Dvd(titulo, ano, n),
        };
        _bib.Adicionar(item);
        _txtTitulo.Clear();
        _txtExtra1.Clear();
    }

    private void RemoverItem() => _bib.RemoverItem(IdSelecionado(_gridItens));

    private void AdicionarUsuario()
    {
        Usuario u = _cbPerfil.SelectedItem as string == "Professor"
            ? new Professor(_txtNome.Text, _txtEmail.Text)
            : new Aluno(_txtNome.Text, _txtEmail.Text);
        _bib.Adicionar(u);
        _txtNome.Clear();
        _txtEmail.Clear();
    }

    private void RemoverUsuario() => _bib.RemoverUsuario(IdSelecionado(_gridUsuarios));

    private void Emprestar()
    {
        if (_cbItem.SelectedItem is not ItemAcervo item || _cbUsuario.SelectedItem is not Usuario usuario)
            throw new BibliotecaException("Selecione o item e o usuário.");
        var emp = _bib.Emprestar(item, usuario, _dtEmprestimo.Value);
        MessageBox.Show($"Devolução prevista em {emp.DataPrevista:dd/MM/yyyy}.", "Empréstimo realizado");
    }

    private void Devolver()
    {
        var emp = _bib.Devolver(IdSelecionado(_gridEmprestimos), _dtDevolucao.Value);
        var msg = emp.Multa > 0
            ? $"Devolvido com {emp.DiasAtraso(emp.DataDevolucao!.Value)} dia(s) de atraso.\nMulta: {emp.Multa:C}"
            : "Devolvido no prazo. Sem multa.";
        MessageBox.Show(msg, "Devolução");
    }

    private static int IdSelecionado(DataGridView grid)
    {
        if (grid.CurrentRow is null)
            throw new BibliotecaException("Selecione uma linha na tabela.");
        return (int)grid.CurrentRow.Cells["Id"].Value;
    }


    private void Atualizar()
    {
        _gridItens.DataSource = _bib.Itens.Select(i => new
        {
            i.Id, i.Tipo, i.Titulo, i.Ano, i.Detalhes,
            Situação = i.Disponivel ? "Disponível" : "Emprestado",
        }).ToList();

        _gridUsuarios.DataSource = _bib.Usuarios.Select(u => new
        {
            u.Id, u.Perfil, u.Nome, u.Email,
            Empréstimos = $"{_bib.EmprestimosAtivosDe(u)}/{u.LimiteEmprestimos}",
        }).ToList();

        _gridEmprestimos.DataSource = _bib.Emprestimos.Select(e => new
        {
            e.Id,
            Item = e.Item.Titulo,
            Usuário = e.Usuario.Nome,
            Emprestado = e.DataEmprestimo.ToString("dd/MM/yyyy"),
            Previsto = e.DataPrevista.ToString("dd/MM/yyyy"),
            Devolvido = e.DataDevolucao?.ToString("dd/MM/yyyy") ?? "-",
            Multa = e.Multa.ToString("C"),
        }).ToList();

        var itemSel = _cbItem.SelectedItem;
        var userSel = _cbUsuario.SelectedItem;
        _cbItem.DataSource = _bib.Itens.Where(i => i.Disponivel).ToList();
        _cbUsuario.DataSource = _bib.Usuarios.ToList();
        if (itemSel != null && _cbItem.Items.Contains(itemSel)) _cbItem.SelectedItem = itemSel;
        if (userSel != null && _cbUsuario.Items.Contains(userSel)) _cbUsuario.SelectedItem = userSel;
    }
}

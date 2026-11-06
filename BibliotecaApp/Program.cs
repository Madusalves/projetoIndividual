using BibliotecaApp.Data;
using BibliotecaApp.Forms;
using BibliotecaApp.Services;

namespace BibliotecaApp;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var repo = new Repositorio();
        try
        {
            repo.TestarConexao();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Não foi possível conectar ao banco de dados.\n\n" +
                "Suba o container com:  docker compose up -d\n\n" + ex.Message,
                "Banco indisponível", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        Application.Run(new MainForm(new Biblioteca(repo)));
    }
}

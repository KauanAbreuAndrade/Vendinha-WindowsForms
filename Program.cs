namespace Vendinha;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        try
        {
            using var teste = Banco.Abrir();
            Application.Run(new FormClientes());
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível conectar ao PostgreSQL:\n" + ex.Message +
                "\n\nConfira a variável VENDINHA_CONNECTION e o banco techcore.", "Vendinha");
        }
    }
}

using InventoryPos.Pos.Api;

namespace InventoryPos.Pos;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var apiClient = new ApiClient();
        var loginForm = new LoginForm(apiClient);

        if (loginForm.ShowDialog() == DialogResult.OK)
        {
            Application.Run(new MainForm(apiClient));
        }
    }
}

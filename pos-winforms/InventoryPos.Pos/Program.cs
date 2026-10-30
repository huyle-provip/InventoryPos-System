using InventoryPos.Pos.Api;

namespace InventoryPos.Pos;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        ApiSettings settings;
        try
        {
            settings = ApiSettings.Load();
        }
        catch (Exception ex) when (ex is InvalidOperationException or FileNotFoundException or InvalidDataException)
        {
            MessageBox.Show(ex.Message, "Configuration error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var apiClient = new ApiClient(settings);
        var loginForm = new LoginForm(apiClient);

        if (loginForm.ShowDialog() == DialogResult.OK)
        {
            Application.Run(new MainForm(apiClient));
        }
    }
}

using InventoryPos.Pos.Api;

namespace InventoryPos.Pos;

public class LoginForm : Form
{
    private readonly ApiClient _apiClient;
    private readonly TextBox _usernameBox = new() { Left = 120, Top = 40, Width = 220 };
    private readonly TextBox _passwordBox = new() { Left = 120, Top = 80, Width = 220, PasswordChar = '*' };
    private readonly Label _statusLabel = new() { Left = 20, Top = 150, Width = 320, Height = 40, ForeColor = Color.Firebrick, Text = string.Empty };
    private readonly Button _loginButton = new() { Left = 120, Top = 115, Width = 100, Text = "Log In" };

    public LoginForm(ApiClient apiClient)
    {
        _apiClient = apiClient;

        Text = "InventoryPos POS - Log In";
        Width = 400;
        Height = 280;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        MinimizeBox = false;

        var usernameLabel = new Label { Left = 20, Top = 43, Width = 90, Text = "Username:" };
        var passwordLabel = new Label { Left = 20, Top = 83, Width = 90, Text = "Password:" };

        _loginButton.Click += OnLoginClick;
        _passwordBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                OnLoginClick(_loginButton, EventArgs.Empty);
            }
        };

        Controls.Add(usernameLabel);
        Controls.Add(_usernameBox);
        Controls.Add(passwordLabel);
        Controls.Add(_passwordBox);
        Controls.Add(_loginButton);
        Controls.Add(_statusLabel);

        AcceptButton = _loginButton;
    }

    private async void OnLoginClick(object? sender, EventArgs e)
    {
        _statusLabel.Text = string.Empty;
        _loginButton.Enabled = false;

        try
        {
            await _apiClient.LoginAsync(_usernameBox.Text.Trim(), _passwordBox.Text);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (ApiException ex)
        {
            _statusLabel.Text = ex.Message;
        }
        catch (HttpRequestException)
        {
            _statusLabel.Text = "Could not reach the server. Is the backend running?";
        }
        finally
        {
            _loginButton.Enabled = true;
        }
    }
}

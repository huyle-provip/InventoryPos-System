Imports InventoryPos.Warehouse.Api

Public Class LoginForm
    Inherits Form

    Private ReadOnly _apiClient As ApiClient
    Private ReadOnly _usernameBox As New TextBox With {.Left = 120, .Top = 40, .Width = 220}
    Private ReadOnly _passwordBox As New TextBox With {.Left = 120, .Top = 80, .Width = 220, .PasswordChar = "*"c}
    Private ReadOnly _statusLabel As New Label With {.Left = 20, .Top = 150, .Width = 320, .Height = 40, .ForeColor = Color.Firebrick}
    Private ReadOnly _loginButton As New Button With {.Left = 120, .Top = 115, .Width = 100, .Text = "Log In"}

    Public Sub New(apiClient As ApiClient)
        _apiClient = apiClient

        Text = "InventoryPos Warehouse - Log In"
        Width = 400
        Height = 280
        FormBorderStyle = FormBorderStyle.FixedDialog
        StartPosition = FormStartPosition.CenterScreen
        MaximizeBox = False
        MinimizeBox = False

        Dim usernameLabel As New Label With {.Left = 20, .Top = 43, .Width = 90, .Text = "Username:"}
        Dim passwordLabel As New Label With {.Left = 20, .Top = 83, .Width = 90, .Text = "Password:"}

        AddHandler _loginButton.Click, AddressOf OnLoginClick
        AddHandler _passwordBox.KeyDown, Sub(sender, e)
                                              If e.KeyCode = Keys.Enter Then
                                                  OnLoginClick(_loginButton, EventArgs.Empty)
                                              End If
                                          End Sub

        Controls.Add(usernameLabel)
        Controls.Add(_usernameBox)
        Controls.Add(passwordLabel)
        Controls.Add(_passwordBox)
        Controls.Add(_loginButton)
        Controls.Add(_statusLabel)

        AcceptButton = _loginButton
    End Sub

    Private Async Sub OnLoginClick(sender As Object, e As EventArgs)
        _statusLabel.Text = String.Empty
        _loginButton.Enabled = False

        Try
            Await _apiClient.LoginAsync(_usernameBox.Text.Trim(), _passwordBox.Text)
            DialogResult = DialogResult.OK
            Close()
        Catch ex As ApiException
            _statusLabel.Text = ex.Message
        Catch ex As Net.Http.HttpRequestException
            _statusLabel.Text = "Could not reach the server. Is the backend running?"
        Finally
            _loginButton.Enabled = True
        End Try
    End Sub

End Class

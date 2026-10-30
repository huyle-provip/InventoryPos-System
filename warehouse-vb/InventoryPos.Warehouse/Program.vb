Imports InventoryPos.Warehouse.Api

Friend Module Program

    <STAThread()>
    Friend Sub Main(args As String())
        Application.SetHighDpiMode(HighDpiMode.SystemAware)
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)

        Dim settings As ApiSettings
        Try
            settings = ApiSettings.Load()
        Catch ex As Exception When TypeOf ex Is InvalidOperationException OrElse TypeOf ex Is IO.FileNotFoundException OrElse TypeOf ex Is IO.InvalidDataException
            MessageBox.Show(ex.Message, "Configuration error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try

        Dim apiClient As New ApiClient(settings)
        Dim loginForm As New LoginForm(apiClient)

        If loginForm.ShowDialog() = DialogResult.OK Then
            Application.Run(New MainForm(apiClient))
        End If
    End Sub

End Module

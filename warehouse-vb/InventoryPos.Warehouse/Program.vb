Imports InventoryPos.Warehouse.Api

Friend Module Program

    <STAThread()>
    Friend Sub Main(args As String())
        Application.SetHighDpiMode(HighDpiMode.SystemAware)
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)

        Dim apiClient As New ApiClient()
        Dim loginForm As New LoginForm(apiClient)

        If loginForm.ShowDialog() = DialogResult.OK Then
            Application.Run(New MainForm(apiClient))
        End If
    End Sub

End Module

Imports System.IO
Imports System.Text.Json

Namespace Api

    ' Connection settings read from appsettings.json next to the executable; falls back to local development defaults.
    Public Class ApiSettings
        Public Property BaseUrl As String = "https://localhost:44395"

        Public Property ClientId As String = "InventoryPos_App"

        Public Shared Function Load() As ApiSettings
            Dim path = System.IO.Path.Combine(AppContext.BaseDirectory, "appsettings.json")
            If Not File.Exists(path) Then
                Return New ApiSettings()
            End If

            Try
                Dim settings = JsonSerializer.Deserialize(Of ApiSettings)(
                    File.ReadAllText(path),
                    New JsonSerializerOptions With {.PropertyNameCaseInsensitive = True})
                Return If(settings, New ApiSettings())
            Catch ex As JsonException
                Return New ApiSettings()
            End Try
        End Function
    End Class

End Namespace

Imports Microsoft.Extensions.Configuration

Namespace Api

    ' Connection settings bound from the "Api" configuration section. Sources, later ones winning:
    ' appsettings.json (required), appsettings.{DOTNET_ENVIRONMENT}.json (optional), then INVENTORYPOS_ environment variables
    ' (for example INVENTORYPOS_Api__BaseUrl). Environment-specific values have no defaults, so a missing one fails at startup.
    Public Class ApiSettings
        Public Const SectionName As String = "Api"

        Public Property BaseUrl As String = String.Empty

        Public Property ClientId As String = String.Empty

        Public Property TimeoutSeconds As Integer = 30

        Public Shared Function Load() As ApiSettings
            Dim environment = If(System.Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"), "Production")

            Dim configuration = New ConfigurationBuilder() _
                .SetBasePath(AppContext.BaseDirectory) _
                .AddJsonFile("appsettings.json", optional:=False) _
                .AddJsonFile($"appsettings.{environment}.json", optional:=True) _
                .AddEnvironmentVariables(prefix:="INVENTORYPOS_") _
                .Build()

            Dim settings = configuration.GetSection(SectionName).Get(Of ApiSettings)()
            If settings Is Nothing Then
                settings = New ApiSettings()
            End If

            settings.Validate()
            Return settings
        End Function

        Private Sub Validate()
            Dim uri As Uri = Nothing
            If Not Uri.TryCreate(BaseUrl, UriKind.Absolute, uri) OrElse (uri.Scheme <> Uri.UriSchemeHttps AndAlso uri.Scheme <> Uri.UriSchemeHttp) Then
                Throw New InvalidOperationException($"Configuration '{SectionName}:BaseUrl' must be an absolute http(s) URL, but was '{BaseUrl}'.")
            End If

            If String.IsNullOrWhiteSpace(ClientId) Then
                Throw New InvalidOperationException($"Configuration '{SectionName}:ClientId' is required.")
            End If

            If TimeoutSeconds <= 0 Then
                Throw New InvalidOperationException($"Configuration '{SectionName}:TimeoutSeconds' must be greater than zero.")
            End If
        End Sub
    End Class

End Namespace

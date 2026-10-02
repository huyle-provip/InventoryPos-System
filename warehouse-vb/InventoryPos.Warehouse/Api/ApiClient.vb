Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Text
Imports System.Text.Json

Namespace Api

    Public Class ApiException
        Inherits Exception

        Public Sub New(message As String)
            MyBase.New(message)
        End Sub
    End Class

    Public Class ApiClient
        Private Const BaseUrl As String = "https://localhost:44395"
        Private Const ClientId As String = "InventoryPos_App"

        Private Shared ReadOnly JsonOptions As New JsonSerializerOptions With {
            .PropertyNameCaseInsensitive = True
        }

        Private ReadOnly _httpClient As New HttpClient With {
            .BaseAddress = New Uri(BaseUrl)
        }

        Public Property AccessToken As String

        Public ReadOnly Property IsLoggedIn As Boolean
            Get
                Return Not String.IsNullOrEmpty(AccessToken)
            End Get
        End Property

        Public Async Function LoginAsync(username As String, password As String) As Task
            Dim form As New Dictionary(Of String, String) From {
                {"grant_type", "password"},
                {"client_id", ClientId},
                {"username", username},
                {"password", password},
                {"scope", "InventoryPos offline_access"}
            }

            Using response = Await _httpClient.PostAsync("/connect/token", New FormUrlEncodedContent(form))
                Dim body = Await response.Content.ReadAsStringAsync()

                If Not response.IsSuccessStatusCode Then
                    Throw New ApiException(ExtractTokenError(body))
                End If

                Dim token = JsonSerializer.Deserialize(Of TokenResponse)(body, JsonOptions)
                If token Is Nothing Then
                    Throw New ApiException("Unexpected login response from server.")
                End If

                AccessToken = token.AccessToken
                _httpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Bearer", AccessToken)
            End Using
        End Function

        Public Sub Logout()
            AccessToken = Nothing
            _httpClient.DefaultRequestHeaders.Authorization = Nothing
        End Sub

        Public Async Function GetProductsAsync(Optional filter As String = Nothing, Optional lowStockOnly As Boolean = False) As Task(Of PagedResult(Of ProductDto))
            Dim url = "/api/app/product?maxResultCount=1000"
            If Not String.IsNullOrWhiteSpace(filter) Then
                url &= "&filter=" & Uri.EscapeDataString(filter)
            End If
            If lowStockOnly Then
                url &= "&lowStockOnly=true"
            End If

            Return Await GetAsync(Of PagedResult(Of ProductDto))(url)
        End Function

        Public Async Function GetStockTransactionsAsync(Optional productId As String = Nothing) As Task(Of PagedResult(Of StockTransactionDto))
            Dim url = "/api/app/stock-transaction?maxResultCount=50&sorting=creationTime desc"
            If Not String.IsNullOrWhiteSpace(productId) Then
                url &= "&productId=" & Uri.EscapeDataString(productId)
            End If

            Return Await GetAsync(Of PagedResult(Of StockTransactionDto))(url)
        End Function

        Public Async Function CreateStockTransactionAsync(input As CreateStockTransactionDto) As Task(Of StockTransactionDto)
            Return Await PostAsync(Of CreateStockTransactionDto, StockTransactionDto)("/api/app/stock-transaction", input)
        End Function

        Private Async Function GetAsync(Of T)(url As String) As Task(Of T)
            Using response = Await _httpClient.GetAsync(url)
                Dim body = Await response.Content.ReadAsStringAsync()

                If Not response.IsSuccessStatusCode Then
                    Throw New ApiException(ExtractApiError(body, response.ReasonPhrase))
                End If

                Dim result = JsonSerializer.Deserialize(Of T)(body, JsonOptions)
                If result Is Nothing Then
                    Throw New ApiException("Unexpected empty response from server.")
                End If
                Return result
            End Using
        End Function

        Private Async Function PostAsync(Of TRequest, TResponse)(url As String, input As TRequest) As Task(Of TResponse)
            Dim json = JsonSerializer.Serialize(input)
            Using content As New StringContent(json, Encoding.UTF8, "application/json")
                Using response = Await _httpClient.PostAsync(url, content)
                    Dim body = Await response.Content.ReadAsStringAsync()

                    If Not response.IsSuccessStatusCode Then
                        Throw New ApiException(ExtractApiError(body, response.ReasonPhrase))
                    End If

                    Dim result = JsonSerializer.Deserialize(Of TResponse)(body, JsonOptions)
                    If result Is Nothing Then
                        Throw New ApiException("Unexpected empty response from server.")
                    End If
                    Return result
                End Using
            End Using
        End Function

        Private Shared Function ExtractApiError(body As String, fallback As String) As String
            Try
                Dim envelope = JsonSerializer.Deserialize(Of ApiErrorEnvelope)(body, JsonOptions)
                If envelope IsNot Nothing AndAlso envelope.Error IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(envelope.Error.Message) Then
                    Return envelope.Error.Message
                End If
            Catch ex As JsonException
                ' fall through to generic message
            End Try

            Return If(fallback, "The request failed.")
        End Function

        Private Shared Function ExtractTokenError(body As String) As String
            Try
                Using doc = JsonDocument.Parse(body)
                    Dim descriptionElement As JsonElement = Nothing
                    If doc.RootElement.TryGetProperty("error_description", descriptionElement) Then
                        Return descriptionElement.GetString()
                    End If

                    Dim errorElement As JsonElement = Nothing
                    If doc.RootElement.TryGetProperty("error", errorElement) Then
                        Return errorElement.GetString()
                    End If
                End Using
            Catch ex As JsonException
                ' fall through to generic message
            End Try

            Return "Invalid username or password."
        End Function

    End Class

End Namespace

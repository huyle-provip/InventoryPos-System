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
        Private Shared ReadOnly JsonOptions As New JsonSerializerOptions With {
            .PropertyNameCaseInsensitive = True
        }

        Private ReadOnly _settings As ApiSettings = ApiSettings.Load()
        Private ReadOnly _httpClient As HttpClient
        Private _refreshToken As String

        Public Sub New()
            _httpClient = New HttpClient With {
                .BaseAddress = New Uri(_settings.BaseUrl),
                .Timeout = TimeSpan.FromSeconds(30)
            }
        End Sub

        Public Property AccessToken As String

        Public ReadOnly Property IsLoggedIn As Boolean
            Get
                Return Not String.IsNullOrEmpty(AccessToken)
            End Get
        End Property

        Public Async Function LoginAsync(username As String, password As String) As Task
            Await RequestTokenAsync(New Dictionary(Of String, String) From {
                {"grant_type", "password"},
                {"client_id", _settings.ClientId},
                {"username", username},
                {"password", password},
                {"scope", "InventoryPos offline_access"}
            })
        End Function

        Public Sub Logout()
            AccessToken = Nothing
            _refreshToken = Nothing
            _httpClient.DefaultRequestHeaders.Authorization = Nothing
        End Sub

        Private Async Function RequestTokenAsync(form As Dictionary(Of String, String)) As Task
            Try
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
                    _refreshToken = token.RefreshToken
                    _httpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Bearer", AccessToken)
                End Using
            Catch ex As HttpRequestException
                Throw NetworkError()
            Catch ex As TaskCanceledException
                Throw NetworkError()
            End Try
        End Function

        ' Swaps the refresh token for a new access token; returns False when the session cannot be renewed.
        Private Async Function TryRefreshAsync() As Task(Of Boolean)
            If String.IsNullOrEmpty(_refreshToken) Then
                Return False
            End If

            Try
                Await RequestTokenAsync(New Dictionary(Of String, String) From {
                    {"grant_type", "refresh_token"},
                    {"client_id", _settings.ClientId},
                    {"refresh_token", _refreshToken}
                })
                Return True
            Catch ex As ApiException
                Return False
            End Try
        End Function

        Private Function NetworkError() As ApiException
            Return New ApiException($"Cannot reach the server at {_settings.BaseUrl}. Check that the API is running and your network connection, then try again.")
        End Function

        ' Sends a request; on 401 renews the token once and retries, and turns network failures into friendly errors.
        Private Async Function SendAsync(createRequest As Func(Of HttpRequestMessage)) As Task(Of String)
            Try
                Using response = Await _httpClient.SendAsync(createRequest())
                    If response.StatusCode = Net.HttpStatusCode.Unauthorized AndAlso Await TryRefreshAsync() Then
                        Using retry = Await _httpClient.SendAsync(createRequest())
                            Return Await ReadBodyAsync(retry)
                        End Using
                    End If

                    Return Await ReadBodyAsync(response)
                End Using
            Catch ex As HttpRequestException
                Throw NetworkError()
            Catch ex As TaskCanceledException
                Throw NetworkError()
            End Try
        End Function

        Private Shared Async Function ReadBodyAsync(response As HttpResponseMessage) As Task(Of String)
            Dim body = Await response.Content.ReadAsStringAsync()
            If response.IsSuccessStatusCode Then
                Return body
            End If

            Select Case response.StatusCode
                Case Net.HttpStatusCode.Unauthorized
                    Throw New ApiException("Your session has expired. Please log in again.")
                Case Net.HttpStatusCode.Conflict
                    Throw New ApiException("Someone else changed this data at the same time. Please refresh and try again.")
                Case Else
                    Throw New ApiException(ExtractApiError(body, response.ReasonPhrase))
            End Select
        End Function

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

        Public Async Function GetReceivablePurchaseOrdersAsync() As Task(Of PagedResult(Of PurchaseOrderDto))
            Return Await GetAsync(Of PagedResult(Of PurchaseOrderDto))("/api/app/purchase-order?receivableOnly=true&maxResultCount=100")
        End Function

        Public Async Function ReceivePurchaseOrderAsync(orderId As String, input As ReceivePurchaseOrderDto) As Task(Of PurchaseOrderDto)
            Return Await PostAsync(Of ReceivePurchaseOrderDto, PurchaseOrderDto)($"/api/app/purchase-order/{orderId}/receive", input)
        End Function

        ' Returns Nothing when the product has no image (or it cannot be loaded) so the UI can simply show nothing.
        Public Async Function GetProductImageAsync(productId As String) As Task(Of Image)
            Try
                Using response = Await _httpClient.GetAsync($"/api/app/product-image/{productId}")
                    If Not response.IsSuccessStatusCode Then
                        Return Nothing
                    End If

                    Dim bytes = Await response.Content.ReadAsByteArrayAsync()
                    Using stream As New IO.MemoryStream(bytes)
                        Using original = Image.FromStream(stream)
                            Return New Bitmap(original)
                        End Using
                    End Using
                End Using
            Catch ex As HttpRequestException
                Return Nothing
            Catch ex As ArgumentException
                Return Nothing
            Catch ex As OutOfMemoryException
                Return Nothing
            End Try
        End Function

        Private Async Function GetAsync(Of T)(url As String) As Task(Of T)
            Dim body = Await SendAsync(Function() New HttpRequestMessage(HttpMethod.Get, url))

            Dim result = JsonSerializer.Deserialize(Of T)(body, JsonOptions)
            If result Is Nothing Then
                Throw New ApiException("Unexpected empty response from server.")
            End If
            Return result
        End Function

        Private Async Function PostAsync(Of TRequest, TResponse)(url As String, input As TRequest) As Task(Of TResponse)
            Dim json = JsonSerializer.Serialize(input)
            Dim body = Await SendAsync(Function() New HttpRequestMessage(HttpMethod.Post, url) With {
                .Content = New StringContent(json, Encoding.UTF8, "application/json")
            })

            Dim result = JsonSerializer.Deserialize(Of TResponse)(body, JsonOptions)
            If result Is Nothing Then
                Throw New ApiException("Unexpected empty response from server.")
            End If
            Return result
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

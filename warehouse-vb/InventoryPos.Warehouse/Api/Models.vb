Imports System.Text.Json.Serialization

Namespace Api

    Public Class TokenResponse
        <JsonPropertyName("access_token")>
        Public Property AccessToken As String = String.Empty

        <JsonPropertyName("token_type")>
        Public Property TokenType As String = String.Empty

        <JsonPropertyName("expires_in")>
        Public Property ExpiresIn As Integer
    End Class

    Public Class ApiErrorDetail
        Public Property Code As String
        Public Property Message As String
        Public Property Details As String
    End Class

    Public Class ApiErrorEnvelope
        Public Property [Error] As ApiErrorDetail
    End Class

    Public Class PagedResult(Of T)
        Public Property Items As List(Of T) = New List(Of T)()
        Public Property TotalCount As Integer
    End Class

    Public Class ProductDto
        Public Property Id As String = String.Empty
        Public Property Sku As String = String.Empty
        Public Property Name As String = String.Empty
        Public Property CategoryId As String
        Public Property Price As Decimal
        Public Property QuantityOnHand As Integer
        Public Property ReorderThreshold As Integer
        Public Property IsLowStock As Boolean
        Public Property HasImage As Boolean
    End Class

    Public Enum PurchaseOrderStatus
        Open = 1
        PartiallyReceived = 2
        Received = 3
        Cancelled = 4
    End Enum

    Public Class PurchaseOrderItemDto
        Public Property Id As String = String.Empty
        Public Property ProductId As String = String.Empty
        Public Property ProductName As String = String.Empty
        Public Property Sku As String = String.Empty
        Public Property QuantityOrdered As Integer
        Public Property QuantityReceived As Integer
        Public Property QuantityOutstanding As Integer
        Public Property UnitCost As Decimal
    End Class

    Public Class PurchaseOrderDto
        Public Property Id As String = String.Empty
        Public Property OrderNumber As String = String.Empty
        Public Property SupplierName As String = String.Empty
        Public Property Status As PurchaseOrderStatus
        Public Property Note As String
        Public Property TotalCost As Decimal
        Public Property Items As List(Of PurchaseOrderItemDto) = New List(Of PurchaseOrderItemDto)()

        Public ReadOnly Property Display As String
            Get
                Dim received = Items.Sum(Function(i) i.QuantityReceived)
                Dim ordered = Items.Sum(Function(i) i.QuantityOrdered)
                Return $"{OrderNumber}  |  {SupplierName}  |  {Status}  |  received {received}/{ordered}"
            End Get
        End Property
    End Class

    Public Class ReceivePurchaseOrderItemDto
        Public Property PurchaseOrderItemId As String = String.Empty
        Public Property Quantity As Integer
    End Class

    Public Class ReceivePurchaseOrderDto
        Public Property Items As List(Of ReceivePurchaseOrderItemDto) = New List(Of ReceivePurchaseOrderItemDto)()
    End Class

    Public Enum StockTransactionType
        [In] = 1
        [Out] = 2
    End Enum

    Public Class CreateStockTransactionDto
        Public Property ProductId As String = String.Empty
        Public Property Type As StockTransactionType
        Public Property Quantity As Integer
        Public Property Note As String
    End Class

    Public Class StockTransactionDto
        Public Property Id As String = String.Empty
        Public Property ProductId As String = String.Empty
        Public Property Type As StockTransactionType
        Public Property Quantity As Integer
        Public Property Note As String
        Public Property CreationTime As DateTime
    End Class

End Namespace

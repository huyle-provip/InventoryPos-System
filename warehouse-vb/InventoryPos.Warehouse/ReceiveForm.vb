Imports System.ComponentModel
Imports InventoryPos.Warehouse.Api

Public Class ReceiveLine
    Public Property ItemId As String
    Public Property Sku As String
    Public Property ProductName As String
    Public Property Ordered As Integer
    Public Property Received As Integer
    Public Property Outstanding As Integer
    Public Property ReceiveNow As Integer
End Class

Public Class ReceiveForm
    Inherits Form

    Private ReadOnly _apiClient As ApiClient
    Private ReadOnly _lines As New BindingList(Of ReceiveLine)()
    Private _orders As New List(Of PurchaseOrderDto)()

    Private ReadOnly _orderLabel As New Label With {.Left = 12, .Top = 16, .Width = 90, .Text = "Order:"}
    Private ReadOnly _orderCombo As New ComboBox With {.Left = 100, .Top = 12, .Width = 560, .DropDownStyle = ComboBoxStyle.DropDownList}
    Private ReadOnly _refreshButton As New Button With {.Left = 670, .Top = 11, .Width = 80, .Text = "Refresh"}
    Private ReadOnly _noteLabel As New Label With {.Left = 12, .Top = 44, .Width = 740, .Height = 22, .ForeColor = Color.DimGray}
    Private ReadOnly _linesGrid As New DataGridView With {
        .Left = 12, .Top = 72, .Width = 740, .Height = 320,
        .AllowUserToAddRows = False, .AllowUserToDeleteRows = False,
        .AutoGenerateColumns = False, .MultiSelect = False,
        .SelectionMode = DataGridViewSelectionMode.CellSelect
    }
    Private ReadOnly _hintLabel As New Label With {.Left = 12, .Top = 398, .Width = 740, .Height = 22, .ForeColor = Color.DimGray, .Text = "Edit the 'Receive now' column for what actually arrived. Partial deliveries are fine - the order stays open for the rest."}
    Private ReadOnly _receiveButton As New Button With {.Left = 12, .Top = 428, .Width = 200, .Height = 36, .Text = "Receive Into Stock"}
    Private ReadOnly _closeButton As New Button With {.Left = 552, .Top = 428, .Width = 200, .Height = 36, .Text = "Close"}
    Private ReadOnly _statusLabel As New Label With {.Left = 12, .Top = 472, .Width = 740, .Height = 40, .ForeColor = Color.Firebrick}

    Public Sub New(apiClient As ApiClient)
        _apiClient = apiClient

        Text = "Receive Purchase Order"
        Width = 780
        Height = 560
        FormBorderStyle = FormBorderStyle.FixedDialog
        StartPosition = FormStartPosition.CenterParent
        MaximizeBox = False
        MinimizeBox = False

        BuildColumns()
        _linesGrid.DataSource = _lines

        AddHandler _orderCombo.SelectedIndexChanged, AddressOf OnOrderChanged
        AddHandler _refreshButton.Click, Sub() LoadOrdersAsync(Nothing)
        AddHandler _receiveButton.Click, AddressOf OnReceiveClick
        AddHandler _closeButton.Click, Sub() Close()
        AddHandler _linesGrid.DataError, Sub(sender, e)
                                             e.ThrowException = False
                                             _statusLabel.Text = "Enter a whole number for the received quantity."
                                         End Sub
        AddHandler Load, Sub() LoadOrdersAsync(Nothing)

        Controls.Add(_orderLabel)
        Controls.Add(_orderCombo)
        Controls.Add(_refreshButton)
        Controls.Add(_noteLabel)
        Controls.Add(_linesGrid)
        Controls.Add(_hintLabel)
        Controls.Add(_receiveButton)
        Controls.Add(_closeButton)
        Controls.Add(_statusLabel)
    End Sub

    Private Sub BuildColumns()
        _linesGrid.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = NameOf(ReceiveLine.Sku), .HeaderText = "SKU", .Width = 100, .ReadOnly = True})
        _linesGrid.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = NameOf(ReceiveLine.ProductName), .HeaderText = "Product", .Width = 250, .ReadOnly = True})
        _linesGrid.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = NameOf(ReceiveLine.Ordered), .HeaderText = "Ordered", .Width = 70, .ReadOnly = True})
        _linesGrid.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = NameOf(ReceiveLine.Received), .HeaderText = "Received", .Width = 75, .ReadOnly = True})
        _linesGrid.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = NameOf(ReceiveLine.Outstanding), .HeaderText = "Outstanding", .Width = 85, .ReadOnly = True})
        _linesGrid.Columns.Add(New DataGridViewTextBoxColumn With {
            .DataPropertyName = NameOf(ReceiveLine.ReceiveNow), .HeaderText = "Receive now", .Width = 90,
            .DefaultCellStyle = New DataGridViewCellStyle With {.BackColor = Color.LightYellow}
        })
    End Sub

    Private Async Sub LoadOrdersAsync(keepOrderId As String)
        Try
            _statusLabel.Text = String.Empty
            Dim result = Await _apiClient.GetReceivablePurchaseOrdersAsync()
            _orders = result.Items

            _orderCombo.DataSource = Nothing
            _orderCombo.DataSource = _orders
            _orderCombo.DisplayMember = NameOf(PurchaseOrderDto.Display)

            If _orders.Count = 0 Then
                _lines.Clear()
                _noteLabel.Text = "There are no open purchase orders to receive."
                Return
            End If

            Dim index = If(keepOrderId Is Nothing, 0, _orders.FindIndex(Function(o) o.Id = keepOrderId))
            _orderCombo.SelectedIndex = If(index >= 0, index, 0)
        Catch ex As ApiException
            _statusLabel.Text = ex.Message
        Catch ex As Net.Http.HttpRequestException
            _statusLabel.Text = "Could not reach the server."
        End Try
    End Sub

    Private Sub OnOrderChanged(sender As Object, e As EventArgs)
        _lines.Clear()
        Dim order = TryCast(_orderCombo.SelectedItem, PurchaseOrderDto)
        If order Is Nothing Then
            Return
        End If

        _noteLabel.Text = If(String.IsNullOrWhiteSpace(order.Note), String.Empty, "Note: " & order.Note)

        For Each item In order.Items.Where(Function(i) i.QuantityOutstanding > 0)
            _lines.Add(New ReceiveLine With {
                .ItemId = item.Id,
                .Sku = item.Sku,
                .ProductName = item.ProductName,
                .Ordered = item.QuantityOrdered,
                .Received = item.QuantityReceived,
                .Outstanding = item.QuantityOutstanding,
                .ReceiveNow = item.QuantityOutstanding
            })
        Next
    End Sub

    Private Async Sub OnReceiveClick(sender As Object, e As EventArgs)
        Dim order = TryCast(_orderCombo.SelectedItem, PurchaseOrderDto)
        If order Is Nothing Then
            _statusLabel.Text = "Choose a purchase order first."
            Return
        End If

        _linesGrid.EndEdit()

        Dim input As New ReceivePurchaseOrderDto()
        For Each line In _lines
            If line.ReceiveNow < 0 OrElse line.ReceiveNow > line.Outstanding Then
                _statusLabel.Text = $"'{line.ProductName}': receive between 0 and {line.Outstanding}."
                Return
            End If

            If line.ReceiveNow > 0 Then
                input.Items.Add(New ReceivePurchaseOrderItemDto With {.PurchaseOrderItemId = line.ItemId, .Quantity = line.ReceiveNow})
            End If
        Next

        If input.Items.Count = 0 Then
            _statusLabel.Text = "Enter a received quantity for at least one item."
            Return
        End If

        _receiveButton.Enabled = False
        _statusLabel.Text = String.Empty

        Try
            Dim updated = Await _apiClient.ReceivePurchaseOrderAsync(order.Id, input)
            MessageBox.Show(
                $"Stock updated. {updated.OrderNumber} is now {updated.Status}.",
                "Received",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)
            LoadOrdersAsync(order.Id)
        Catch ex As ApiException
            _statusLabel.Text = ex.Message
        Catch ex As Net.Http.HttpRequestException
            _statusLabel.Text = "Could not reach the server."
        Finally
            _receiveButton.Enabled = True
        End Try
    End Sub

End Class

Imports InventoryPos.Warehouse.Api

Public Class MainForm
    Inherits Form

    Private ReadOnly _apiClient As ApiClient
    Private ReadOnly _productsBindingSource As New BindingSource()
    Private ReadOnly _historyBindingSource As New BindingSource()
    Private _products As List(Of ProductDto) = New List(Of ProductDto)()

    Private ReadOnly _searchBox As New TextBox With {.Left = 12, .Top = 12, .Width = 300}
    Private ReadOnly _searchButton As New Button With {.Left = 320, .Top = 11, .Width = 90, .Text = "Search"}
    Private ReadOnly _lowStockCheck As New CheckBox With {.Left = 425, .Top = 13, .Width = 150, .Text = "Low stock only"}
    Private ReadOnly _bannerLabel As New Label With {.Left = 12, .Top = 534, .Width = 560, .Height = 28, .Font = New Font("Segoe UI", 10, FontStyle.Bold)}
    Private ReadOnly _productsGrid As New DataGridView With {
        .Left = 12, .Top = 44, .Width = 560, .Height = 480,
        .ReadOnly = True, .AllowUserToAddRows = False, .AllowUserToDeleteRows = False,
        .AutoGenerateColumns = False, .SelectionMode = DataGridViewSelectionMode.FullRowSelect, .MultiSelect = False
    }

    Private ReadOnly _selectedProductLabel As New Label With {.Left = 590, .Top = 44, .Width = 480, .Height = 24, .Font = New Font("Segoe UI", 10, FontStyle.Bold), .Text = "Select a product on the left"}
    Private ReadOnly _typeCombo As New ComboBox With {.Left = 590, .Top = 80, .Width = 150, .DropDownStyle = ComboBoxStyle.DropDownList}
    Private ReadOnly _quantityPicker As New NumericUpDown With {.Left = 750, .Top = 80, .Width = 100, .Minimum = 1, .Maximum = 100000, .Value = 1}
    Private ReadOnly _noteBox As New TextBox With {.Left = 590, .Top = 120, .Width = 320}
    Private ReadOnly _submitButton As New Button With {.Left = 590, .Top = 150, .Width = 150, .Text = "Submit"}
    Private ReadOnly _statusLabel As New Label With {.Left = 590, .Top = 185, .Width = 480, .Height = 40, .ForeColor = Color.Firebrick}

    Private ReadOnly _historyLabel As New Label With {.Left = 590, .Top = 230, .Width = 300, .Font = New Font("Segoe UI", 10, FontStyle.Bold), .Text = "Recent Stock Transactions"}
    Private ReadOnly _historyGrid As New DataGridView With {
        .Left = 590, .Top = 254, .Width = 480, .Height = 270,
        .ReadOnly = True, .AllowUserToAddRows = False, .AllowUserToDeleteRows = False,
        .AutoGenerateColumns = False, .SelectionMode = DataGridViewSelectionMode.FullRowSelect, .MultiSelect = False
    }

    Private ReadOnly _logoutButton As New Button With {.Left = 890, .Top = 530, .Width = 180, .Height = 36, .Text = "Log Out"}

    Private _selectedProduct As ProductDto

    Public Sub New(apiClient As ApiClient)
        _apiClient = apiClient

        Text = "InventoryPos - Warehouse Stock"
        Width = 1100
        Height = 650
        StartPosition = FormStartPosition.CenterScreen

        BuildProductsGridColumns()
        BuildHistoryGridColumns()

        _productsGrid.DataSource = _productsBindingSource
        _historyGrid.DataSource = _historyBindingSource

        _typeCombo.Items.Add("Stock In")
        _typeCombo.Items.Add("Stock Out")
        _typeCombo.SelectedIndex = 0

        AddHandler _searchButton.Click, Sub() LoadProductsAsync()
        AddHandler _searchBox.KeyDown, Sub(sender, e)
                                           If e.KeyCode = Keys.Enter Then
                                               LoadProductsAsync()
                                           End If
                                       End Sub
        AddHandler _lowStockCheck.CheckedChanged, Sub() LoadProductsAsync()
        AddHandler _productsGrid.RowPrePaint, AddressOf OnProductsRowPrePaint
        AddHandler _productsGrid.SelectionChanged, AddressOf OnProductSelectionChanged
        AddHandler _submitButton.Click, AddressOf OnSubmitClick
        AddHandler _logoutButton.Click, AddressOf OnLogoutClick

        Controls.Add(_searchBox)
        Controls.Add(_searchButton)
        Controls.Add(_lowStockCheck)
        Controls.Add(_bannerLabel)
        Controls.Add(_productsGrid)
        Controls.Add(_selectedProductLabel)
        Controls.Add(_typeCombo)
        Controls.Add(_quantityPicker)
        Controls.Add(_noteBox)
        Controls.Add(_submitButton)
        Controls.Add(_statusLabel)
        Controls.Add(_historyLabel)
        Controls.Add(_historyGrid)
        Controls.Add(_logoutButton)

        AddHandler Load, Sub() LoadProductsAsync()
    End Sub

    Private Sub BuildProductsGridColumns()
        _productsGrid.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = NameOf(ProductDto.Sku), .HeaderText = "SKU", .Width = 90})
        _productsGrid.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = NameOf(ProductDto.Name), .HeaderText = "Name", .Width = 220})
        _productsGrid.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = NameOf(ProductDto.QuantityOnHand), .HeaderText = "On Hand", .Width = 80})
        _productsGrid.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = NameOf(ProductDto.ReorderThreshold), .HeaderText = "Reorder At", .Width = 80})
        _productsGrid.Columns.Add(New DataGridViewCheckBoxColumn With {.DataPropertyName = NameOf(ProductDto.IsLowStock), .HeaderText = "Low?", .Width = 50})
    End Sub

    Private Sub BuildHistoryGridColumns()
        _historyGrid.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = NameOf(StockTransactionDto.CreationTime), .HeaderText = "Date", .Width = 130, .DefaultCellStyle = New DataGridViewCellStyle With {.Format = "g"}})
        _historyGrid.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = NameOf(StockTransactionDto.Type), .HeaderText = "Type", .Width = 70})
        _historyGrid.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = NameOf(StockTransactionDto.Quantity), .HeaderText = "Qty", .Width = 60})
        _historyGrid.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = NameOf(StockTransactionDto.Note), .HeaderText = "Note", .Width = 210})
    End Sub

    Private Async Sub LoadProductsAsync()
        Try
            _statusLabel.Text = String.Empty
            Dim result = Await _apiClient.GetProductsAsync(_searchBox.Text.Trim(), _lowStockCheck.Checked)
            _products = result.Items
            _productsBindingSource.DataSource = Nothing
            _productsBindingSource.DataSource = _products
            UpdateBanner()
        Catch ex As ApiException
            _statusLabel.Text = ex.Message
        Catch ex As Net.Http.HttpRequestException
            _statusLabel.Text = "Could not reach the server."
        End Try
    End Sub

    Private Sub UpdateBanner()
        Dim lowCount = _products.Where(Function(p) p.IsLowStock).Count()
        Dim outCount = _products.Where(Function(p) p.QuantityOnHand <= 0).Count()

        If lowCount = 0 Then
            _bannerLabel.ForeColor = Color.SeaGreen
            _bannerLabel.Text = "All listed products are well stocked."
        Else
            _bannerLabel.ForeColor = Color.Firebrick
            _bannerLabel.Text = $"{lowCount} low-stock item(s), {outCount} out of stock - reorder soon."
        End If
    End Sub

    Private Sub OnProductsRowPrePaint(sender As Object, e As DataGridViewRowPrePaintEventArgs)
        Dim row = _productsGrid.Rows(e.RowIndex)
        Dim product = TryCast(row.DataBoundItem, ProductDto)
        If product Is Nothing Then
            Return
        End If

        If product.QuantityOnHand <= 0 Then
            row.DefaultCellStyle.BackColor = Color.IndianRed
            row.DefaultCellStyle.ForeColor = Color.White
        ElseIf product.IsLowStock Then
            row.DefaultCellStyle.BackColor = Color.MistyRose
            row.DefaultCellStyle.ForeColor = Color.Firebrick
        Else
            row.DefaultCellStyle.BackColor = Color.White
            row.DefaultCellStyle.ForeColor = Color.Black
        End If
    End Sub

    Private Sub OnProductSelectionChanged(sender As Object, e As EventArgs)
        Dim row = _productsGrid.CurrentRow
        If row Is Nothing OrElse TypeOf row.DataBoundItem IsNot ProductDto Then
            _selectedProduct = Nothing
            _selectedProductLabel.Text = "Select a product on the left"
            Return
        End If

        _selectedProduct = CType(row.DataBoundItem, ProductDto)
        _selectedProductLabel.Text = $"{_selectedProduct.Name} - currently {_selectedProduct.QuantityOnHand} on hand"
        LoadHistoryAsync(_selectedProduct.Id)
    End Sub

    Private Async Sub LoadHistoryAsync(productId As String)
        Try
            Dim result = Await _apiClient.GetStockTransactionsAsync(productId)
            _historyBindingSource.DataSource = Nothing
            _historyBindingSource.DataSource = result.Items
        Catch ex As ApiException
            _statusLabel.Text = ex.Message
        End Try
    End Sub

    Private Async Sub OnSubmitClick(sender As Object, e As EventArgs)
        If _selectedProduct Is Nothing Then
            _statusLabel.Text = "Select a product first."
            Return
        End If

        _submitButton.Enabled = False
        _statusLabel.Text = String.Empty

        Try
            Dim input As New CreateStockTransactionDto With {
                .ProductId = _selectedProduct.Id,
                .Type = If(_typeCombo.SelectedIndex = 0, StockTransactionType.In, StockTransactionType.Out),
                .Quantity = CInt(_quantityPicker.Value),
                .Note = _noteBox.Text.Trim()
            }

            Await _apiClient.CreateStockTransactionAsync(input)

            MessageBox.Show("Stock transaction recorded.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)

            _noteBox.Text = String.Empty
            _quantityPicker.Value = 1
            LoadProductsAsync()
            LoadHistoryAsync(_selectedProduct.Id)
        Catch ex As ApiException
            _statusLabel.Text = ex.Message
        Catch ex As Net.Http.HttpRequestException
            _statusLabel.Text = "Could not reach the server."
        Finally
            _submitButton.Enabled = True
        End Try
    End Sub

    Private Sub OnLogoutClick(sender As Object, e As EventArgs)
        _apiClient.Logout()
        Hide()

        Dim loginForm As New LoginForm(_apiClient)
        If loginForm.ShowDialog() = DialogResult.OK Then
            Show()
            LoadProductsAsync()
        Else
            Close()
        End If
    End Sub

End Class

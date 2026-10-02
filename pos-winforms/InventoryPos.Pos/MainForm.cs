using InventoryPos.Pos.Api;

namespace InventoryPos.Pos;

public class MainForm : Form
{
    private readonly ApiClient _apiClient;
    private readonly BindingSource _productsBindingSource = new();
    private readonly BindingSource _cartBindingSource = new();
    private readonly List<CartLine> _cart = new();
    private List<ProductDto> _products = new();

    private readonly TextBox _searchBox = new() { Left = 12, Top = 12, Width = 240, PlaceholderText = "Search name or SKU (F3)" };
    private readonly Button _searchButton = new() { Left = 258, Top = 11, Width = 70, Text = "Search" };
    private readonly TextBox _skuBox = new() { Left = 340, Top = 12, Width = 232, PlaceholderText = "Scan / enter SKU + Enter (F2)" };
    private readonly DataGridView _productsGrid = new()
    {
        Left = 12,
        Top = 44,
        Width = 560,
        Height = 480,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AutoGenerateColumns = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
    };
    private readonly NumericUpDown _quantityPicker = new() { Left = 12, Top = 534, Width = 80, Minimum = 1, Maximum = 10000, Value = 1 };
    private readonly Button _addToCartButton = new() { Left = 100, Top = 532, Width = 150, Text = "Add to Cart ->" };

    private readonly DataGridView _cartGrid = new()
    {
        Left = 590,
        Top = 44,
        Width = 480,
        Height = 420,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AutoGenerateColumns = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
    };
    private readonly Button _removeLineButton = new() { Left = 590, Top = 474, Width = 150, Text = "Remove Line" };
    private readonly Label _totalLabel = new() { Left = 590, Top = 520, Width = 480, Height = 30, Font = new Font("Segoe UI", 14, FontStyle.Bold), Text = "Total: $0.00" };
    private readonly Button _checkoutButton = new() { Left = 590, Top = 560, Width = 230, Height = 40, Text = "Checkout" };
    private readonly Button _logoutButton = new() { Left = 840, Top = 560, Width = 230, Height = 40, Text = "Log Out" };
    private readonly Label _statusLabel = new() { Left = 12, Top = 568, Width = 560, Height = 40, ForeColor = Color.Firebrick };

    public MainForm(ApiClient apiClient)
    {
        _apiClient = apiClient;

        Text = "InventoryPos - Point of Sale   [F2 SKU | F3 Search | F9 Checkout]";
        Width = 1100;
        Height = 680;
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;
        KeyDown += OnFormKeyDown;

        BuildProductsGridColumns();
        BuildCartGridColumns();

        _productsGrid.DataSource = _productsBindingSource;
        _cartGrid.DataSource = _cartBindingSource;

        _searchButton.Click += async (_, _) => await LoadProductsAsync();
        _searchBox.KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                await LoadProductsAsync();
            }
        };
        _skuBox.KeyDown += OnSkuBoxKeyDown;
        _addToCartButton.Click += OnAddToCartClick;
        _removeLineButton.Click += OnRemoveLineClick;
        _checkoutButton.Click += OnCheckoutClick;
        _logoutButton.Click += OnLogoutClick;

        Controls.Add(_searchBox);
        Controls.Add(_searchButton);
        Controls.Add(_skuBox);
        Controls.Add(_productsGrid);
        Controls.Add(_quantityPicker);
        Controls.Add(_addToCartButton);
        Controls.Add(_cartGrid);
        Controls.Add(_removeLineButton);
        Controls.Add(_totalLabel);
        Controls.Add(_checkoutButton);
        Controls.Add(_logoutButton);
        Controls.Add(_statusLabel);

        Load += async (_, _) => await LoadProductsAsync();
    }

    private void BuildProductsGridColumns()
    {
        _productsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ProductDto.Sku), HeaderText = "SKU", Width = 90 });
        _productsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ProductDto.Name), HeaderText = "Name", Width = 220 });
        _productsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ProductDto.Price), HeaderText = "Price", Width = 80, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } });
        _productsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ProductDto.QuantityOnHand), HeaderText = "On Hand", Width = 80 });
    }

    private void BuildCartGridColumns()
    {
        _cartGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CartLine.ProductName), HeaderText = "Product", Width = 200 });
        _cartGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CartLine.Quantity), HeaderText = "Qty", Width = 60 });
        _cartGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CartLine.UnitPrice), HeaderText = "Unit Price", Width = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } });
        _cartGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CartLine.Subtotal), HeaderText = "Subtotal", Width = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } });
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            _statusLabel.Text = string.Empty;
            var result = await _apiClient.GetProductsAsync(_searchBox.Text.Trim());
            _products = result.Items;
            _productsBindingSource.DataSource = _products;
        }
        catch (ApiException ex)
        {
            _statusLabel.Text = ex.Message;
        }
        catch (HttpRequestException)
        {
            _statusLabel.Text = "Could not reach the server.";
        }
    }

    private void OnAddToCartClick(object? sender, EventArgs e)
    {
        if (_productsGrid.CurrentRow?.DataBoundItem is not ProductDto product)
        {
            _statusLabel.Text = "Select a product first.";
            return;
        }

        AddToCart(product, (int)_quantityPicker.Value);
    }

    private void AddToCart(ProductDto product, int quantity)
    {
        var existingLine = _cart.FirstOrDefault(l => l.ProductId == product.Id);
        var alreadyInCart = existingLine?.Quantity ?? 0;

        if (alreadyInCart + quantity > product.QuantityOnHand)
        {
            _statusLabel.Text = $"Only {product.QuantityOnHand} of '{product.Name}' in stock ({alreadyInCart} already in cart).";
            return;
        }

        _statusLabel.Text = string.Empty;

        if (existingLine != null)
        {
            existingLine.Quantity += quantity;
        }
        else
        {
            _cart.Add(new CartLine
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = quantity,
            });
        }

        RefreshCart();
    }

    private void OnSkuBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
        {
            return;
        }

        e.SuppressKeyPress = true;
        var sku = _skuBox.Text.Trim();
        if (sku.Length == 0)
        {
            return;
        }

        var product = _products.FirstOrDefault(p => string.Equals(p.Sku, sku, StringComparison.OrdinalIgnoreCase));
        if (product == null)
        {
            _statusLabel.Text = $"No product with SKU '{sku}'.";
        }
        else
        {
            AddToCart(product, (int)_quantityPicker.Value);
        }

        _skuBox.Clear();
        _skuBox.Focus();
    }

    private void OnFormKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.F2:
                _skuBox.Focus();
                e.Handled = true;
                break;
            case Keys.F3:
                _searchBox.Focus();
                e.Handled = true;
                break;
            case Keys.F9:
                OnCheckoutClick(_checkoutButton, EventArgs.Empty);
                e.Handled = true;
                break;
        }
    }

    private void OnRemoveLineClick(object? sender, EventArgs e)
    {
        if (_cartGrid.CurrentRow?.DataBoundItem is CartLine line)
        {
            _cart.Remove(line);
            RefreshCart();
        }
    }

    private void RefreshCart()
    {
        _cartBindingSource.DataSource = null;
        _cartBindingSource.DataSource = _cart;
        _totalLabel.Text = $"Total: {_cart.Sum(l => l.Subtotal):C2}";
    }

    private async void OnCheckoutClick(object? sender, EventArgs e)
    {
        if (_cart.Count == 0)
        {
            _statusLabel.Text = "Cart is empty.";
            return;
        }

        _checkoutButton.Enabled = false;
        _statusLabel.Text = string.Empty;

        try
        {
            var input = new CreateSaleOrderDto
            {
                Items = _cart.Select(l => new CreateSaleOrderItemDto { ProductId = l.ProductId, Quantity = l.Quantity }).ToList(),
            };

            var order = await _apiClient.CreateSaleOrderAsync(input);

            var receiptLines = _cart.Select(l => new CartLine
            {
                ProductId = l.ProductId,
                ProductName = l.ProductName,
                UnitPrice = l.UnitPrice,
                Quantity = l.Quantity,
            }).ToList();

            ReceiptPrinter.ShowPreview(this, order, receiptLines);

            _cart.Clear();
            RefreshCart();
            await LoadProductsAsync();
        }
        catch (ApiException ex)
        {
            _statusLabel.Text = ex.Message;
        }
        catch (HttpRequestException)
        {
            _statusLabel.Text = "Could not reach the server.";
        }
        finally
        {
            _checkoutButton.Enabled = true;
        }
    }

    private void OnLogoutClick(object? sender, EventArgs e)
    {
        _apiClient.Logout();
        Hide();

        var loginForm = new LoginForm(_apiClient);
        if (loginForm.ShowDialog() == DialogResult.OK)
        {
            Show();
            _ = LoadProductsAsync();
        }
        else
        {
            Close();
        }
    }
}

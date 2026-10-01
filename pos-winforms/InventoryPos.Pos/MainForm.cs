using InventoryPos.Pos.Api;

namespace InventoryPos.Pos;

public class MainForm : Form
{
    private readonly ApiClient _apiClient;
    private readonly BindingSource _productsBindingSource = new();
    private readonly BindingSource _cartBindingSource = new();
    private readonly List<CartLine> _cart = new();

    private readonly TextBox _searchBox = new() { Left = 12, Top = 12, Width = 300 };
    private readonly Button _searchButton = new() { Left = 320, Top = 11, Width = 90, Text = "Search" };
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

        Text = "InventoryPos - Point of Sale";
        Width = 1100;
        Height = 680;
        StartPosition = FormStartPosition.CenterScreen;

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
        _addToCartButton.Click += OnAddToCartClick;
        _removeLineButton.Click += OnRemoveLineClick;
        _checkoutButton.Click += OnCheckoutClick;
        _logoutButton.Click += OnLogoutClick;

        Controls.Add(_searchBox);
        Controls.Add(_searchButton);
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
            _productsBindingSource.DataSource = result.Items;
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

        var quantity = (int)_quantityPicker.Value;

        var existingLine = _cart.FirstOrDefault(l => l.ProductId == product.Id);
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

            MessageBox.Show(
                $"Sale completed.\n\nTotal: {order.TotalAmount:C2}\nItems: {order.Items.Count}",
                "Receipt",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

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

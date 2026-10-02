using InventoryPos.Pos.Api;

namespace InventoryPos.Pos;

public class MainForm : Form
{
    private readonly ApiClient _apiClient;
    private readonly BindingSource _productsBindingSource = new();
    private readonly BindingSource _cartBindingSource = new();
    private readonly List<CartLine> _cart = new();
    private readonly Dictionary<string, Image?> _imageCache = new();
    private List<ProductDto> _products = new();
    private decimal _taxRatePercent;
    private string? _previewProductId;

    private readonly TextBox _searchBox = new() { Left = 12, Top = 12, Width = 240, PlaceholderText = "Search name or SKU (F3)" };
    private readonly Button _searchButton = new() { Left = 258, Top = 11, Width = 70, Text = "Search" };
    private readonly TextBox _skuBox = new() { Left = 340, Top = 12, Width = 232, PlaceholderText = "Scan / enter SKU + Enter (F2)" };
    private readonly DataGridView _productsGrid = new()
    {
        Left = 12,
        Top = 44,
        Width = 560,
        Height = 380,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AutoGenerateColumns = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
    };
    private readonly NumericUpDown _quantityPicker = new() { Left = 12, Top = 434, Width = 80, Minimum = 1, Maximum = 10000, Value = 1 };
    private readonly Button _addToCartButton = new() { Left = 100, Top = 432, Width = 150, Text = "Add to Cart ->" };
    private readonly PictureBox _productPicture = new()
    {
        Left = 12,
        Top = 478,
        Width = 110,
        Height = 110,
        BorderStyle = BorderStyle.FixedSingle,
        SizeMode = PictureBoxSizeMode.Zoom,
    };
    private readonly Label _productInfoLabel = new() { Left = 134, Top = 478, Width = 438, Height = 110, Font = new Font("Segoe UI", 10), Text = "Select a product to see its details." };
    private readonly Label _statusLabel = new() { Left = 12, Top = 600, Width = 560, Height = 44, ForeColor = Color.Firebrick };

    private readonly DataGridView _cartGrid = new()
    {
        Left = 590,
        Top = 44,
        Width = 480,
        Height = 230,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AutoGenerateColumns = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
    };
    private readonly Button _removeLineButton = new() { Left = 590, Top = 280, Width = 150, Text = "Remove Line" };

    private readonly Label _discountLabel = new() { Left = 590, Top = 318, Width = 70, Text = "Discount:" };
    private readonly ComboBox _discountTypeCombo = new() { Left = 662, Top = 314, Width = 110, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown _discountValueBox = new() { Left = 780, Top = 314, Width = 100, DecimalPlaces = 2, Maximum = 10_000_000 };
    private readonly Label _paymentLabel = new() { Left = 590, Top = 352, Width = 70, Text = "Payment:" };
    private readonly ComboBox _paymentCombo = new() { Left = 662, Top = 348, Width = 110, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _tenderedLabel = new() { Left = 780, Top = 352, Width = 100, Text = "Cash received:" };
    private readonly NumericUpDown _tenderedBox = new() { Left = 884, Top = 348, Width = 110, DecimalPlaces = 2, Maximum = 10_000_000 };

    private readonly Label _summaryLabel = new() { Left = 590, Top = 388, Width = 480, Height = 84, Font = new Font("Consolas", 10) };
    private readonly Label _totalLabel = new() { Left = 590, Top = 476, Width = 480, Height = 32, Font = new Font("Segoe UI", 16, FontStyle.Bold), Text = "Total: $0.00" };
    private readonly Label _changeLabel = new() { Left = 590, Top = 512, Width = 480, Height = 26, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.SeaGreen };
    private readonly Button _checkoutButton = new() { Left = 590, Top = 560, Width = 230, Height = 44, Text = "Checkout (F9)" };
    private readonly Button _logoutButton = new() { Left = 840, Top = 560, Width = 230, Height = 44, Text = "Log Out" };

    public MainForm(ApiClient apiClient)
    {
        _apiClient = apiClient;

        Text = "InventoryPos - Point of Sale   [F2 SKU | F3 Search | F9 Checkout]";
        Width = 1100;
        Height = 700;
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;
        KeyDown += OnFormKeyDown;

        BuildProductsGridColumns();
        BuildCartGridColumns();

        _productsGrid.DataSource = _productsBindingSource;
        _cartGrid.DataSource = _cartBindingSource;

        _discountTypeCombo.Items.AddRange(new object[] { "None", "Percent (%)", "Amount ($)" });
        _discountTypeCombo.SelectedIndex = 0;
        _discountValueBox.Enabled = false;
        _paymentCombo.Items.AddRange(new object[] { "Cash", "Card" });
        _paymentCombo.SelectedIndex = 0;

        _searchButton.Click += async (_, _) => await LoadProductsAsync();
        _searchBox.KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                await LoadProductsAsync();
            }
        };
        _skuBox.KeyDown += OnSkuBoxKeyDown;
        _productsGrid.SelectionChanged += async (_, _) => await ShowSelectedProductAsync();
        _addToCartButton.Click += OnAddToCartClick;
        _removeLineButton.Click += OnRemoveLineClick;
        _discountTypeCombo.SelectedIndexChanged += (_, _) =>
        {
            _discountValueBox.Enabled = _discountTypeCombo.SelectedIndex != 0;
            if (_discountTypeCombo.SelectedIndex == 0)
            {
                _discountValueBox.Value = 0;
            }
            else if (_discountTypeCombo.SelectedIndex == 1 && _discountValueBox.Value > 100)
            {
                _discountValueBox.Value = 100;
            }

            UpdateTotals();
        };
        _discountValueBox.ValueChanged += (_, _) => UpdateTotals();
        _paymentCombo.SelectedIndexChanged += (_, _) =>
        {
            var isCash = _paymentCombo.SelectedIndex == 0;
            _tenderedBox.Enabled = isCash;
            _tenderedLabel.Enabled = isCash;
            if (!isCash)
            {
                _tenderedBox.Value = 0;
            }

            UpdateTotals();
        };
        _tenderedBox.ValueChanged += (_, _) => UpdateTotals();
        _checkoutButton.Click += OnCheckoutClick;
        _logoutButton.Click += OnLogoutClick;

        Controls.Add(_searchBox);
        Controls.Add(_searchButton);
        Controls.Add(_skuBox);
        Controls.Add(_productsGrid);
        Controls.Add(_quantityPicker);
        Controls.Add(_addToCartButton);
        Controls.Add(_productPicture);
        Controls.Add(_productInfoLabel);
        Controls.Add(_statusLabel);
        Controls.Add(_cartGrid);
        Controls.Add(_removeLineButton);
        Controls.Add(_discountLabel);
        Controls.Add(_discountTypeCombo);
        Controls.Add(_discountValueBox);
        Controls.Add(_paymentLabel);
        Controls.Add(_paymentCombo);
        Controls.Add(_tenderedLabel);
        Controls.Add(_tenderedBox);
        Controls.Add(_summaryLabel);
        Controls.Add(_totalLabel);
        Controls.Add(_changeLabel);
        Controls.Add(_checkoutButton);
        Controls.Add(_logoutButton);

        UpdateTotals();

        Load += async (_, _) =>
        {
            await LoadPricingAsync();
            await LoadProductsAsync();
        };
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

    private async Task LoadPricingAsync()
    {
        try
        {
            var pricing = await _apiClient.GetPricingInfoAsync();
            _taxRatePercent = pricing.TaxRatePercent;
            UpdateTotals();
        }
        catch (ApiException ex)
        {
            _statusLabel.Text = "Could not load the tax rate: " + ex.Message;
        }
        catch (HttpRequestException)
        {
            _statusLabel.Text = "Could not reach the server.";
        }
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            _statusLabel.Text = string.Empty;
            var result = await _apiClient.GetProductsAsync(_searchBox.Text.Trim());
            _products = result.Items;

            foreach (var cached in _imageCache.Values)
            {
                cached?.Dispose();
            }

            _imageCache.Clear();
            _previewProductId = null;
            _productsBindingSource.DataSource = _products;
            await ShowSelectedProductAsync();
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

    private async Task ShowSelectedProductAsync()
    {
        if (_productsGrid.CurrentRow?.DataBoundItem is not ProductDto product)
        {
            _productPicture.Image = null;
            _productInfoLabel.Text = "Select a product to see its details.";
            _previewProductId = null;
            return;
        }

        if (_previewProductId == product.Id)
        {
            return;
        }

        _previewProductId = product.Id;
        _productInfoLabel.Text =
            $"{product.Name}\nSKU {product.Sku}\n{product.Price:C2}   |   {product.QuantityOnHand} in stock" +
            (product.IsLowStock ? "   (low)" : string.Empty);

        if (!product.HasImage)
        {
            _productPicture.Image = null;
            return;
        }

        if (!_imageCache.TryGetValue(product.Id, out var image))
        {
            image = await _apiClient.GetProductImageAsync(product.Id);
            _imageCache[product.Id] = image;
        }

        // The selection may have moved on while the image was downloading.
        if (_previewProductId == product.Id)
        {
            _productPicture.Image = image;
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
        UpdateTotals();
    }

    private DiscountType SelectedDiscountType => _discountTypeCombo.SelectedIndex switch
    {
        1 => DiscountType.Percent,
        2 => DiscountType.Amount,
        _ => DiscountType.None,
    };

    private PaymentMethod SelectedPayment => _paymentCombo.SelectedIndex == 1 ? PaymentMethod.Card : PaymentMethod.Cash;

    /// <summary>Mirrors the server's pricing so the cashier sees the final amount before charging; the server stays authoritative.</summary>
    private (decimal Subtotal, decimal Discount, decimal Tax, decimal Total) ComputeTotals()
    {
        var subtotal = _cart.Sum(l => l.Subtotal);
        var value = _discountValueBox.Value;

        var discount = SelectedDiscountType switch
        {
            DiscountType.Percent => Math.Round(subtotal * Math.Min(value, 100m) / 100m, 2, MidpointRounding.AwayFromZero),
            DiscountType.Amount => Math.Min(Math.Round(value, 2, MidpointRounding.AwayFromZero), subtotal),
            _ => 0m,
        };

        var taxable = subtotal - discount;
        var tax = Math.Round(taxable * _taxRatePercent / 100m, 2, MidpointRounding.AwayFromZero);
        return (subtotal, discount, tax, taxable + tax);
    }

    private void UpdateTotals()
    {
        var (subtotal, discount, tax, total) = ComputeTotals();

        _summaryLabel.Text =
            $"{"Subtotal",-16}{subtotal,12:C2}\n" +
            $"{"Discount",-16}{-discount,12:C2}\n" +
            $"{$"Tax ({_taxRatePercent:0.##}%)",-16}{tax,12:C2}";
        _totalLabel.Text = $"Total: {total:C2}";

        if (SelectedPayment == PaymentMethod.Cash && _tenderedBox.Value > 0)
        {
            var change = _tenderedBox.Value - total;
            _changeLabel.ForeColor = change < 0 ? Color.Firebrick : Color.SeaGreen;
            _changeLabel.Text = change < 0 ? $"Short by {-change:C2}" : $"Change due: {change:C2}";
        }
        else
        {
            _changeLabel.Text = string.Empty;
        }
    }

    private async void OnCheckoutClick(object? sender, EventArgs e)
    {
        if (_cart.Count == 0)
        {
            _statusLabel.Text = "Cart is empty.";
            return;
        }

        var (_, _, _, total) = ComputeTotals();
        if (SelectedPayment == PaymentMethod.Cash && _tenderedBox.Value > 0 && _tenderedBox.Value < total)
        {
            _statusLabel.Text = "The cash received is less than the total due.";
            return;
        }

        _checkoutButton.Enabled = false;
        _statusLabel.Text = string.Empty;

        try
        {
            var input = new CreateSaleOrderDto
            {
                Items = _cart.Select(l => new CreateSaleOrderItemDto { ProductId = l.ProductId, Quantity = l.Quantity }).ToList(),
                DiscountType = SelectedDiscountType,
                DiscountValue = SelectedDiscountType == DiscountType.None ? 0 : _discountValueBox.Value,
                PaymentMethod = SelectedPayment,
                // Zero means "exact amount" for cash; the server ignores it for card.
                AmountTendered = SelectedPayment == PaymentMethod.Cash && _tenderedBox.Value > 0 ? _tenderedBox.Value : null,
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
            _discountTypeCombo.SelectedIndex = 0;
            _tenderedBox.Value = 0;
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
            _ = LoadPricingAsync();
            _ = LoadProductsAsync();
        }
        else
        {
            Close();
        }
    }
}

using System.Drawing.Printing;
using System.Text;
using InventoryPos.Pos.Api;

namespace InventoryPos.Pos;

public static class ReceiptPrinter
{
    private const int LineWidth = 38;

    public static void ShowPreview(IWin32Window owner, SaleOrderDto order, IReadOnlyList<CartLine> lines)
    {
        var receiptText = BuildReceiptText(order, lines);
        var font = new Font("Consolas", 9f);

        using var document = new PrintDocument { DocumentName = "InventoryPos Receipt" };
        document.PrintPage += (_, e) =>
        {
            if (e.Graphics == null)
            {
                return;
            }

            e.Graphics.DrawString(receiptText, font, Brushes.Black, e.MarginBounds.Left, e.MarginBounds.Top);
        };

        using var preview = new PrintPreviewDialog
        {
            Document = document,
            Width = 520,
            Height = 700,
            StartPosition = FormStartPosition.CenterParent,
            Text = "Receipt - use the printer icon to print",
        };

        preview.ShowDialog(owner);
        font.Dispose();
    }

    public static string BuildReceiptText(SaleOrderDto order, IReadOnlyList<CartLine> lines)
    {
        var sb = new StringBuilder();
        var divider = new string('-', LineWidth);

        sb.AppendLine(Center("INVENTORYPOS STORE"));
        sb.AppendLine(Center("Thank you for shopping!"));
        sb.AppendLine(divider);
        sb.AppendLine($"Date : {order.CreationTime:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"Order: {order.Id[..Math.Min(8, order.Id.Length)]}");
        sb.AppendLine(divider);

        foreach (var line in lines)
        {
            var name = line.ProductName.Length > LineWidth ? line.ProductName[..LineWidth] : line.ProductName;
            sb.AppendLine(name);
            sb.AppendLine(Row($"  {line.Quantity} x {line.UnitPrice:C2}", line.Subtotal.ToString("C2")));
        }

        sb.AppendLine(divider);
        sb.AppendLine(Row("TOTAL", order.TotalAmount.ToString("C2")));
        sb.AppendLine(divider);

        return sb.ToString();
    }

    private static string Row(string left, string right)
    {
        var padding = Math.Max(1, LineWidth - left.Length - right.Length);
        return left + new string(' ', padding) + right;
    }

    private static string Center(string text)
    {
        var padding = Math.Max(0, (LineWidth - text.Length) / 2);
        return new string(' ', padding) + text;
    }
}

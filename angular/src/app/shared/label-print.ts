import { code128Svg } from './code128';

export interface LabelItem {
  sku: string;
  name: string;
  price: number;
}

function escapeHtml(value: string): string {
  return value
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;');
}

/** Opens a print-ready sheet of barcode labels. Product text is escaped because names are user-entered. */
export function printLabels(items: LabelItem[]): boolean {
  if (items.length === 0) {
    return false;
  }

  const labels = items
    .map(
      item => `
      <div class="label">
        <div class="name">${escapeHtml(item.name)}</div>
        ${code128Svg(item.sku, 2, 50)}
        <div class="sku">${escapeHtml(item.sku)}</div>
        <div class="price">$${item.price.toFixed(2)}</div>
      </div>`,
    )
    .join('');

  const html = `<!doctype html>
<html>
<head>
<meta charset="utf-8">
<title>Barcode labels</title>
<style>
  body { font-family: Arial, sans-serif; margin: 12px; }
  .sheet { display: flex; flex-wrap: wrap; gap: 8px; }
  .label { width: 62mm; border: 1px dashed #999; padding: 6px; text-align: center; page-break-inside: avoid; }
  .label svg { max-width: 100%; height: auto; }
  .name { font-size: 12px; font-weight: bold; margin-bottom: 4px; overflow: hidden; white-space: nowrap; text-overflow: ellipsis; }
  .sku { font-family: monospace; font-size: 11px; }
  .price { font-size: 14px; font-weight: bold; }
  @media print { .label { border: none; } }
</style>
</head>
<body><div class="sheet">${labels}</div></body>
</html>`;

  const printWindow = window.open('', '_blank', 'width=800,height=600');
  if (!printWindow) {
    return false;
  }

  printWindow.document.open();
  printWindow.document.write(html);
  printWindow.document.close();
  printWindow.focus();
  // Give the SVGs a tick to lay out before the print dialog opens.
  setTimeout(() => printWindow.print(), 300);
  return true;
}

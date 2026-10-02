export type CsvCell = string | number | boolean | Date | null | undefined;

function escapeCell(value: CsvCell): string {
  if (value === null || value === undefined) {
    return '';
  }

  let text = value instanceof Date ? value.toISOString() : String(value);

  // Stop spreadsheet apps from executing user-entered text as a formula (e.g. a product named "=HYPERLINK(...)").
  if (typeof value === 'string' && /^[=+\-@\t\r]/.test(text)) {
    text = "'" + text;
  }

  if (/[",\r\n]/.test(text)) {
    text = '"' + text.replace(/"/g, '""') + '"';
  }

  return text;
}

export function downloadCsv(fileName: string, headers: string[], rows: CsvCell[][]): void {
  const lines = [headers, ...rows].map(row => row.map(escapeCell).join(','));
  // BOM so Excel opens UTF-8 text (e.g. accented product names) correctly.
  const blob = new Blob(['﻿' + lines.join('\r\n')], { type: 'text/csv;charset=utf-8;' });

  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = fileName;
  link.click();
  URL.revokeObjectURL(url);
}

export function csvTimestamp(): string {
  return new Date().toISOString().slice(0, 10);
}

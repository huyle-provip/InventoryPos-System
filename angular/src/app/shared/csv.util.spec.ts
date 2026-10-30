import { csvTimestamp, downloadCsv } from './csv.util';

describe('csv.util', () => {
  let captured: Blob | undefined;

  beforeEach(() => {
    captured = undefined;
    spyOn(URL, 'createObjectURL').and.callFake((blob: Blob | MediaSource) => {
      captured = blob as Blob;
      return 'blob:test';
    });
    spyOn(URL, 'revokeObjectURL');
    spyOn(HTMLAnchorElement.prototype, 'click');
  });

  async function csvOf(headers: string[], rows: any[][]): Promise<string> {
    downloadCsv('test.csv', headers, rows);
    return (await captured!.text()).replace(/^﻿/, '');
  }

  it('joins rows with CRLF and cells with commas', async () => {
    expect(await csvOf(['Name', 'Qty'], [['Cola', 3]])).toBe('Name,Qty\r\nCola,3');
  });

  it('quotes cells containing commas, quotes or newlines', async () => {
    expect(await csvOf(['A'], [['a,b'], ['say "hi"'], ['x\ny']])).toBe('A\r\n"a,b"\r\n"say ""hi"""\r\n"x\ny"');
  });

  it('writes null and undefined as empty cells', async () => {
    expect(await csvOf(['A', 'B'], [[null, undefined]])).toBe('A,B\r\n,');
  });

  it('neutralises spreadsheet formulas in text', async () => {
    expect(await csvOf(['A'], [['=HYPERLINK("x")'], ['@cmd']])).toBe('A\r\n"\'=HYPERLINK(""x"")"\r\n\'@cmd');
  });

  it('does not alter negative numbers', async () => {
    expect(await csvOf(['A'], [[-5]])).toBe('A\r\n-5');
  });

  it('triggers a download with the given file name and releases the URL', () => {
    downloadCsv('stock.csv', ['A'], []);
    expect(HTMLAnchorElement.prototype.click).toHaveBeenCalled();
    expect(URL.revokeObjectURL).toHaveBeenCalledWith('blob:test');
  });

  it('formats the timestamp as yyyy-mm-dd', () => {
    expect(csvTimestamp()).toMatch(/^\d{4}-\d{2}-\d{2}$/);
  });
});

import { code128Modules, code128Svg, code128Widths } from './code128';

describe('code128', () => {
  it('encodes a single character with start, data, checksum and stop patterns', () => {
    // Start B (104) + 'A' (33) => checksum (104 + 33) % 103 = 34
    expect(code128Widths('A')).toEqual(['211214', '111323', '131123', '2331112']);
  });

  it('produces 11 modules per symbol and 13 for the stop pattern', () => {
    expect(code128Modules('A').length).toBe(3 * 11 + 13);
    expect(code128Modules('COLA-330').length).toBe((1 + 8 + 1) * 11 + 13);
  });

  it('starts with a bar and ends with the stop bar', () => {
    const modules = code128Modules('SKU-1');
    expect(modules[0]).toBe('1');
    expect(modules.endsWith('11')).toBeTrue();
  });

  it('replaces characters outside printable ASCII with "?"', () => {
    expect(code128Widths('é')).toEqual(code128Widths('?'));
  });

  it('renders an SVG with bars', () => {
    const svg = code128Svg('A');
    expect(svg.startsWith('<svg')).toBeTrue();
    expect(svg).toContain('<rect');
  });
});

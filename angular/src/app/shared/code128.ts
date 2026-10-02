// Code 128 (subset B: printable ASCII 32-126), rendered as inline SVG for printable labels.
// Each entry is the bar/space/bar/space/bar/space widths (in modules) for symbol values 0-105; the stop pattern has 7 elements.
const PATTERNS = [
  '212222', '222122', '222221', '121223', '121322', '131222', '122213', '122312', '132212', '221213',
  '221312', '231212', '112232', '122132', '122231', '113222', '123122', '123221', '223211', '221132',
  '221231', '213212', '223112', '312131', '311222', '321122', '321221', '312212', '322112', '322211',
  '212123', '212321', '232121', '111323', '131123', '131321', '112313', '132113', '132311', '211313',
  '231113', '231311', '112133', '112331', '132131', '113123', '113321', '133121', '313121', '211331',
  '231131', '213113', '213311', '213131', '311123', '311321', '331121', '312113', '312311', '332111',
  '314111', '221411', '431111', '111224', '111422', '121124', '121421', '141122', '141221', '112214',
  '112412', '122114', '122411', '142112', '142211', '241211', '221114', '413111', '241112', '134111',
  '111242', '121142', '121241', '114212', '124112', '124211', '411212', '421112', '421211', '212141',
  '214121', '412121', '111143', '111341', '131141', '114113', '114311', '411113', '411311', '113141',
  '114131', '311141', '411131', '211412', '211214', '211232',
];

const STOP_PATTERN = '2331112';
const START_B = 104;

export function code128Widths(text: string): string[] {
  const values: number[] = [];
  for (const char of text) {
    const code = char.charCodeAt(0);
    // Code B only covers printable ASCII; anything else would produce an unscannable symbol.
    values.push(code >= 32 && code <= 126 ? code - 32 : '?'.charCodeAt(0) - 32);
  }

  let checksum = START_B;
  values.forEach((value, index) => (checksum += value * (index + 1)));

  return [
    PATTERNS[START_B],
    ...values.map(value => PATTERNS[value]),
    PATTERNS[checksum % 103],
    STOP_PATTERN,
  ];
}

/** Binary module string (1 = bar, 0 = space) for the whole symbol, without quiet zones. */
export function code128Modules(text: string): string {
  return code128Widths(text)
    .map(pattern =>
      [...pattern].map((width, index) => (index % 2 === 0 ? '1' : '0').repeat(Number(width))).join(''),
    )
    .join('');
}

export function code128Svg(text: string, moduleWidth = 2, height = 60): string {
  const modules = code128Modules(text);
  const quietZone = 10;
  const width = (modules.length + quietZone * 2) * moduleWidth;

  let bars = '';
  let index = 0;
  while (index < modules.length) {
    if (modules[index] === '1') {
      let run = 1;
      while (modules[index + run] === '1') {
        run++;
      }
      bars += `<rect x="${(index + quietZone) * moduleWidth}" y="0" width="${run * moduleWidth}" height="${height}"/>`;
      index += run;
    } else {
      index++;
    }
  }

  return `<svg xmlns="http://www.w3.org/2000/svg" width="${width}" height="${height}" viewBox="0 0 ${width} ${height}" shape-rendering="crispEdges"><rect width="${width}" height="${height}" fill="#fff"/><g fill="#000">${bars}</g></svg>`;
}

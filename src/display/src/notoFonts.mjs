export const notoFontFamilies = Object.freeze([
  Object.freeze({ key: 'sc', family: 'Noto Sans SC', fallback: '"Noto Sans SC", "Microsoft YaHei", sans-serif' }),
  Object.freeze({ key: 'kr', family: 'Noto Sans KR', fallback: '"Noto Sans KR", "Malgun Gothic", sans-serif' }),
  Object.freeze({ key: 'jp', family: 'Noto Sans JP', fallback: '"Noto Sans JP", "Yu Gothic", sans-serif' }),
  Object.freeze({ key: 'arabic', family: 'Noto Sans Arabic', fallback: '"Noto Sans Arabic", Tahoma, sans-serif' })
]);

export async function preloadNotoFonts(fontSet = globalThis.document?.fonts) {
  if (!fontSet) {
    return [];
  }

  return Promise.all(
    notoFontFamilies.flatMap(({ family }) => [
      fontSet.load(`400 1em "${family}"`),
      fontSet.load(`700 1em "${family}"`)
    ])
  );
}

export type NotoFontFamily = Readonly<{
  key: 'sc' | 'kr' | 'jp' | 'arabic';
  family: string;
  fallback: string;
}>;

export const notoFontFamilies: readonly NotoFontFamily[];

export function preloadNotoFonts(
  fontSet?: Pick<FontFaceSet, 'load'>
): Promise<FontFace[][]>;

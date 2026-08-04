export type NotoFontFamily = Readonly<{
  key: 'sc' | 'kr' | 'jp' | 'arabic';
  family: string;
  fallback: string;
}>;

export const notoFontFamilies: readonly NotoFontFamily[];

export function preloadNotoFonts(
  fontSet?: Pick<FontFaceSet, 'load'>
): Promise<FontFace[][]>;

export const themeFontFamilies: readonly Readonly<{
  family: string;
  weights: readonly number[];
}>[];

export function preloadThemeFonts(
  fontSet?: Pick<FontFaceSet, 'load'>
): Promise<FontFace[][]>;

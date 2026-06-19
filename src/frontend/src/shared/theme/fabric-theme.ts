import { z } from 'zod';

export type HexColor = `#${string}`;

export type FabricTheme = {
  primaryColor: HexColor;
  textColor: HexColor;
  textMutedColor: HexColor;
  errorColor: HexColor;
  dangerColor: HexColor;
  successColor: HexColor;
};

const hexColorSchema = z.custom<HexColor>((value) => typeof value === 'string' && /^#(?:[0-9a-fA-F]{3}){1,2}$/.test(value));

export const fabricThemeSchema = z.object({
  primaryColor: hexColorSchema,
  textColor: hexColorSchema,
  textMutedColor: hexColorSchema,
  errorColor: hexColorSchema,
  dangerColor: hexColorSchema,
  successColor: hexColorSchema,
});

export const defaultFabricTheme: FabricTheme = {
  primaryColor: '#238cff',
  textColor: '#212529',
  textMutedColor: '#6c757d',
  errorColor: '#ff6467',
  dangerColor: '#ff6467',
  successColor: '#00c950',
};

export function isHexColor(value: unknown): value is HexColor {
  return hexColorSchema.safeParse(value).success;
}

export function applyFabricTheme(theme: FabricTheme, root: HTMLElement = document.documentElement) {
  const parsedTheme = fabricThemeSchema.parse(theme);

  root.style.setProperty('--fabric-primary', parsedTheme.primaryColor);
  root.style.setProperty('--fabric-text', parsedTheme.textColor);
  root.style.setProperty('--fabric-text-muted', parsedTheme.textMutedColor);
  root.style.setProperty('--fabric-error', parsedTheme.errorColor);
  root.style.setProperty('--fabric-danger', parsedTheme.dangerColor);
  root.style.setProperty('--fabric-success', parsedTheme.successColor);
}

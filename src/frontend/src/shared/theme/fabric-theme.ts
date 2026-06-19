import { z } from 'zod';

export type HexColor = `#${string}`;

export type FabricTheme = {
  backgroundColor: HexColor;
  contentColor: HexColor;
  primaryColor: HexColor;
  textColor: HexColor;
  textMutedColor: HexColor;
  borderColor: HexColor;
  hoverBlueColor: HexColor;
  activeBlueColor: HexColor;
  hoverGrayColor: HexColor;
  errorColor: HexColor;
  errorBackgroundColor: HexColor;
  dangerColor: HexColor;
  successColor: HexColor;
  successBackgroundColor: HexColor;
};

const hexColorSchema = z.custom<HexColor>((value) => typeof value === 'string' && /^#(?:[0-9a-fA-F]{3}){1,2}$/.test(value));

export const fabricThemeSchema = z.object({
  backgroundColor: hexColorSchema,
  contentColor: hexColorSchema,
  primaryColor: hexColorSchema,
  textColor: hexColorSchema,
  textMutedColor: hexColorSchema,
  borderColor: hexColorSchema,
  hoverBlueColor: hexColorSchema,
  activeBlueColor: hexColorSchema,
  hoverGrayColor: hexColorSchema,
  errorColor: hexColorSchema,
  errorBackgroundColor: hexColorSchema,
  dangerColor: hexColorSchema,
  successColor: hexColorSchema,
  successBackgroundColor: hexColorSchema,
});

export const defaultFabricTheme: FabricTheme = {
  backgroundColor: '#f8f8f8',
  contentColor: '#ffffff',
  primaryColor: '#238cff',
  textColor: '#212529',
  textMutedColor: '#6c757d',
  borderColor: '#dddddd',
  hoverBlueColor: '#eef6ff',
  activeBlueColor: '#deeeff',
  hoverGrayColor: '#f3f3f3',
  errorColor: '#ff6467',
  errorBackgroundColor: '#feeaea',
  dangerColor: '#ff6467',
  successColor: '#00c950',
  successBackgroundColor: '#e6faeb',
};

export function isHexColor(value: unknown): value is HexColor {
  return hexColorSchema.safeParse(value).success;
}

export function applyFabricTheme(theme: FabricTheme, root: HTMLElement = document.documentElement) {
  const parsedTheme = fabricThemeSchema.parse(theme);

  root.style.setProperty('--fabric-background', parsedTheme.backgroundColor);
  root.style.setProperty('--fabric-content', parsedTheme.contentColor);
  root.style.setProperty('--fabric-primary', parsedTheme.primaryColor);
  root.style.setProperty('--fabric-text', parsedTheme.textColor);
  root.style.setProperty('--fabric-text-muted', parsedTheme.textMutedColor);
  root.style.setProperty('--fabric-border', parsedTheme.borderColor);
  root.style.setProperty('--fabric-hover-blue', parsedTheme.hoverBlueColor);
  root.style.setProperty('--fabric-active-blue', parsedTheme.activeBlueColor);
  root.style.setProperty('--fabric-hover-gray', parsedTheme.hoverGrayColor);
  root.style.setProperty('--fabric-error', parsedTheme.errorColor);
  root.style.setProperty('--fabric-error-background', parsedTheme.errorBackgroundColor);
  root.style.setProperty('--fabric-danger', parsedTheme.dangerColor);
  root.style.setProperty('--fabric-success', parsedTheme.successColor);
  root.style.setProperty('--fabric-success-background', parsedTheme.successBackgroundColor);
}

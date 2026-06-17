import { describe, expect, it } from 'vitest';

import { applyFabricTheme, defaultFabricTheme, isHexColor } from './fabric-theme';

describe('fabric theme', () => {
  it('accepts short and long hex colors', () => {
    expect(isHexColor('#fff')).toBe(true);
    expect(isHexColor('#ffffff')).toBe(true);
  });

  it('rejects non-hex colors', () => {
    expect(isHexColor('blue')).toBe(false);
    expect(isHexColor('rgb(0, 0, 0)')).toBe(false);
  });

  it('applies theme variables', () => {
    const root = document.createElement('div');

    applyFabricTheme(defaultFabricTheme, root);

    expect(root.style.getPropertyValue('--fabric-primary')).toBe(defaultFabricTheme.primaryColor);
    expect(root.style.getPropertyValue('--fabric-text')).toBe(defaultFabricTheme.textColor);
  });
});

export type KioskActivityDefinition = {
  readonly type: string;
  readonly displayName: string;
  readonly description: string;
  readonly category: 'Kiosk';
  readonly inputs: readonly KioskActivityInput[];
};

export type KioskActivityInput = {
  readonly name: string;
  readonly displayName: string;
  readonly type: 'text' | 'textarea' | 'boolean' | 'select' | 'json';
  readonly required?: boolean;
  readonly description?: string;
};

export const kioskActivityDefinitions = [
  {
    type: 'Kiosk.DisplayMessage',
    displayName: 'Display Message',
    description: 'Shows a localized title/message with optional visual assets.',
    category: 'Kiosk',
    inputs: [
      { name: 'titleKey', displayName: 'Title key', type: 'text', required: true },
      { name: 'messageKey', displayName: 'Message key', type: 'textarea' },
      { name: 'imageAssetName', displayName: 'Image asset', type: 'text' },
      { name: 'backgroundAssetName', displayName: 'Background asset', type: 'text' },
      { name: 'layoutMode', displayName: 'Layout mode', type: 'select', description: 'single-column, split-left-visual, or split-right-visual' },
    ],
  },
  {
    type: 'Kiosk.PromptChoice',
    displayName: 'Prompt User Choice',
    description: 'Shows fixed button choices; each value can be mapped to a workflow outcome.',
    category: 'Kiosk',
    inputs: [
      { name: 'titleKey', displayName: 'Title key', type: 'text', required: true },
      { name: 'messageKey', displayName: 'Message key', type: 'textarea' },
      { name: 'choices', displayName: 'Choices', type: 'json', required: true, description: 'Array of { value, labelKey }.' },
    ],
  },
  {
    type: 'Kiosk.PromptDynamicChoice',
    displayName: 'Prompt Dynamic User Choice',
    description: 'Shows dynamic button choices and stores selected value for later branching.',
    category: 'Kiosk',
    inputs: [
      { name: 'titleKey', displayName: 'Title key', type: 'text', required: true },
      { name: 'messageKey', displayName: 'Message key', type: 'textarea' },
      { name: 'choicesExpression', displayName: 'Choices expression', type: 'json', required: true },
      { name: 'resultVariable', displayName: 'Result variable', type: 'text', required: true },
    ],
  },
  {
    type: 'Kiosk.DisplayForm',
    displayName: 'Display Form',
    description: 'Shows simple text/password fields and returns Dictionary<string, string>.',
    category: 'Kiosk',
    inputs: [
      { name: 'titleKey', displayName: 'Title key', type: 'text', required: true },
      { name: 'messageKey', displayName: 'Message key', type: 'textarea' },
      { name: 'fields', displayName: 'Fields', type: 'json', required: true, description: 'Array of { name, labelKey, placeholderKey, isRequired, isMaskRequired }.' },
      { name: 'resultVariable', displayName: 'Result variable', type: 'text', required: true },
    ],
  },
  {
    type: 'Kiosk.CompleteSession',
    displayName: 'Complete Kiosk Session',
    description: 'Marks the current kiosk session as completed and runs session cleanup.',
    category: 'Kiosk',
    inputs: [],
  },
] as const satisfies readonly KioskActivityDefinition[];

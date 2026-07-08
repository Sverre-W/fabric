import type { components } from '@/shared/api/generated/schema';

export type KioskConfig = components['schemas']['KioskConfigResponse'];
export type KioskSession = components['schemas']['KioskSessionResponse'];
export type KioskInstructionResponse = components['schemas']['KioskInstructionResponse'];
export type KioskProfileLanguage = components['schemas']['KioskProfileLanguageResponse'];

export type KioskInstructionEnvelope = {
  readonly instructionId?: string;
  readonly version?: number;
  readonly type?: string;
  readonly languageCode?: string;
  readonly layout?: {
    readonly mode?: string;
    readonly backgroundUrl?: string | null;
    readonly imageUrl?: string | null;
  };
  readonly content?: {
    readonly title?: string | null;
    readonly titleKey?: string | null;
    readonly message?: string | null;
    readonly messageKey?: string | null;
  };
  readonly theme?: Record<string, string>;
  readonly choices?: readonly {
    readonly value: string;
    readonly label?: string | null;
    readonly labelKey?: string | null;
  }[];
  readonly fields?: readonly {
    readonly name: string;
    readonly label?: string | null;
    readonly labelKey?: string | null;
    readonly placeholder?: string | null;
    readonly placeholderKey?: string | null;
    readonly isRequired?: boolean;
    readonly isMaskRequired?: boolean;
  }[];
};

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
    readonly backgroundAssetName?: string | null;
    readonly imageAssetName?: string | null;
  };
  readonly content?: {
    readonly title?: string | null;
    readonly message?: string | null;
  };
  readonly choices?: readonly {
    readonly value: string;
    readonly label: string;
  }[];
  readonly fields?: readonly {
    readonly name: string;
    readonly label: string;
    readonly placeholder?: string | null;
    readonly isRequired?: boolean;
    readonly isMaskRequired?: boolean;
  }[];
};

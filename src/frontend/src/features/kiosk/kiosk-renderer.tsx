import { useState, type FormEvent } from 'react';

import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Label } from '@/shared/components/ui/label';

import type { KioskInstructionEnvelope } from './kiosk-types';

export function KioskRenderer({ instruction, onSubmit }: { readonly instruction: KioskInstructionEnvelope; readonly onSubmit: (values: Record<string, string>) => void }) {
  const type = instruction.type ?? 'loading';
  const layout = instruction.layout?.mode ?? 'single-column';
  const title = instruction.content?.title || instruction.content?.titleKey || 'Continue';
  const message = instruction.content?.message || instruction.content?.messageKey || null;
  const imageUrl = instruction.layout?.imageUrl ?? null;

  const content = type === 'prompt-choice' || type === 'prompt-dynamic-choice'
    ? <ChoiceContent instruction={instruction} onSubmit={onSubmit} />
    : type === 'display-form'
      ? <FormContent instruction={instruction} onSubmit={onSubmit} />
      : type === 'display-message'
        ? <MessageContent title={title} message={message} imageUrl={imageUrl} />
        : type === 'completed'
          ? <MessageContent title={title || 'Done'} message={message ?? 'Session completed.'} imageUrl={imageUrl} />
          : type === 'error'
            ? <MessageContent title={title || 'Something went wrong'} message={message ?? 'Please contact support.'} imageUrl={imageUrl} />
            : <MessageContent title="Please wait" message="Loading next step..." imageUrl={imageUrl} />;

  if (layout === 'split-left-visual' || layout === 'split-right-visual') {
    const visual = <VisualPanel imageUrl={imageUrl} />;
    const main = <div className="flex min-h-[32rem] items-center justify-center rounded-[2rem] border border-border bg-content p-8 shadow-sm sm:p-12">{content}</div>;
    return <section className="grid w-full gap-6 lg:grid-cols-[3fr_5fr]">{layout === 'split-left-visual' ? <>{visual}{main}</> : <>{main}{visual}</>}</section>;
  }

  return <section className="w-full max-w-4xl rounded-[2rem] border border-border bg-content p-8 text-center shadow-sm sm:p-12">{content}</section>;
}

function MessageContent({ title, message, imageUrl }: { readonly title: string; readonly message: string | null; readonly imageUrl: string | null }) {
  return <div className="mx-auto grid max-w-3xl place-items-center gap-5 text-center">{imageUrl ? <img src={imageUrl} alt="" className="max-h-72 rounded-[1.5rem] object-contain" /> : null}<h2 className="text-[36px] font-semibold tracking-tight sm:text-[56px]">{title}</h2>{message ? <p className="text-[18px] leading-8 text-muted-foreground sm:text-[22px] sm:leading-9">{message}</p> : null}</div>;
}

function ChoiceContent({ instruction, onSubmit }: { readonly instruction: KioskInstructionEnvelope; readonly onSubmit: (values: Record<string, string>) => void }) {
  const title = instruction.content?.title || instruction.content?.titleKey || 'Choose an option';
  const message = instruction.content?.message || instruction.content?.messageKey || null;
  const choices = instruction.choices ?? [];

  return <div className="mx-auto grid max-w-3xl gap-8 text-center"><div><h2 className="text-[36px] font-semibold tracking-tight sm:text-[52px]">{title}</h2>{message ? <p className="mt-4 text-[18px] leading-8 text-muted-foreground sm:text-[22px] sm:leading-9">{message}</p> : null}</div><div className="grid gap-4 sm:grid-cols-2">{choices.map((choice) => <Button key={choice.value} type="button" size="lg" className="h-auto min-h-28 rounded-[1.5rem] p-6 text-[20px] sm:text-[24px]" onClick={() => onSubmit({ value: choice.value })}>{choice.label || choice.labelKey || choice.value}</Button>)}</div></div>;
}

function FormContent({ instruction, onSubmit }: { readonly instruction: KioskInstructionEnvelope; readonly onSubmit: (values: Record<string, string>) => void }) {
  const [values, setValues] = useState<Record<string, string>>({});
  const title = instruction.content?.title || instruction.content?.titleKey || 'Fill in details';
  const message = instruction.content?.message || instruction.content?.messageKey || null;
  const fields = instruction.fields ?? [];

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    onSubmit(values);
  }

  return <form className="mx-auto grid max-w-3xl gap-7 text-left" onSubmit={handleSubmit}><div className="text-center"><h2 className="text-[34px] font-semibold tracking-tight sm:text-[48px]">{title}</h2>{message ? <p className="mt-4 text-[18px] leading-8 text-muted-foreground">{message}</p> : null}</div><div className="grid gap-5">{fields.map((field) => <label key={field.name} className="grid gap-2 text-[16px] font-medium"><span>{field.label || field.labelKey || field.name}</span><Input className="h-14 rounded-xl px-4 text-[18px] md:text-[18px]" type={field.isMaskRequired ? 'password' : 'text'} required={field.isRequired} placeholder={field.placeholder || field.placeholderKey || ''} value={values[field.name] ?? ''} onChange={(event) => setValues((current) => ({ ...current, [field.name]: event.target.value }))} /></label>)}</div><Button type="submit" className="h-14 rounded-xl text-[18px] font-semibold">Continue</Button></form>;
}

function VisualPanel({ imageUrl }: { readonly imageUrl: string | null }) {
  return <div className="hidden min-h-[32rem] overflow-hidden rounded-[2rem] border border-border bg-hover-gray lg:block">{imageUrl ? <img src={imageUrl} alt="" className="size-full object-cover" /> : <div className="size-full bg-gradient-to-br from-primary/20 via-hover-blue to-background" />}</div>;
}

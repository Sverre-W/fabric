import { useMutation, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft } from 'lucide-react';
import { useState, type FormEvent } from 'react';
import { toast } from 'sonner';

import { api } from '@/shared/api/client';
import { Button } from '@/shared/components/ui/button';
import { Card } from '@/shared/components/ui/card';
import { Input } from '@/shared/components/ui/input';

import { systemProvidersQueryKey, type CreateSystemProviderRequest, type SystemVariableProviderKind } from './card-management-types';

type FormValues = { readonly name: string; readonly providerType: SystemVariableProviderKind; readonly fixedValue: string; readonly initialValue: number };

const emptyValues: FormValues = {
  name: '',
  providerType: 'Fixed',
  fixedValue: '',
  initialValue: 1,
};

export default function SystemProviderCreatePage() {
  const queryClient = useQueryClient();
  const [values, setValues] = useState<FormValues>(emptyValues);

  const createProvider = useMutation({
    mutationFn: async (request: CreateSystemProviderRequest) => {
      const { error } = await api.POST('/api/desfire/system-providers', { body: request });
      if (error) {
        throw new Error('Could not add system provider.');
      }
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: systemProvidersQueryKey });
      toast.success('System provider added.');
      window.history.back();
    },
    onError: () => toast.error('Could not add system provider.'),
  });

  function updateValue<TKey extends keyof FormValues>(key: TKey, value: FormValues[TKey]) {
    setValues((current) => ({ ...current, [key]: value }));
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    createProvider.mutate(toRequest(values));
  }

  return (
    <div className="grid gap-6">
      <header className="flex items-start gap-4">
        <Button variant="outline" size="icon" aria-label="Go back" onClick={() => window.history.back()}>
          <ArrowLeft className="size-4" aria-hidden="true" />
        </Button>
        <div>
          <h2 className="text-[20px] font-semibold tracking-tight">Add system provider</h2>
          <p className="mt-2 max-w-2xl text-[14px] text-muted-foreground">Create a named fixed value or sequence counter that transformations can reference.</p>
        </div>
      </header>

      <Card className="p-4 sm:p-6">
        <form className="grid gap-5" onSubmit={handleSubmit}>
          <div className="grid gap-4 md:grid-cols-2">
            <label className="grid gap-2 text-[14px] font-medium">
              Name
              <Input value={values.name} onChange={(event) => updateValue('name', event.target.value)} required />
            </label>
            <label className="grid gap-2 text-[14px] font-medium">
              Type
              <select className="h-9 rounded-interactive border border-border bg-content px-3 text-[14px] outline-none transition focus:border-primary" value={values.providerType} onChange={(event) => updateValue('providerType', event.target.value as SystemVariableProviderKind)}>
                <option value="Fixed">Fixed</option>
                <option value="Sequence">Sequence</option>
              </select>
            </label>
          </div>

          {values.providerType === 'Fixed' ? (
            <label className="grid gap-2 text-[14px] font-medium">
              Fixed value
              <Input value={values.fixedValue} onChange={(event) => updateValue('fixedValue', event.target.value)} required />
            </label>
          ) : (
            <label className="grid gap-2 text-[14px] font-medium">
              Initial value
              <Input value={String(values.initialValue)} type="number" min={0} onChange={(event) => updateValue('initialValue', Number(event.target.value))} required />
            </label>
          )}

          <p className="rounded-interactive border border-border bg-hover-gray px-3 py-2 text-[14px] text-muted-foreground">System providers are immutable after creation. Delete and recreate one if its type or value source must change.</p>

          <div className="flex flex-col-reverse gap-3 sm:flex-row sm:justify-end">
            <Button type="button" variant="outline" onClick={() => window.history.back()}>Cancel</Button>
            <Button type="submit" disabled={createProvider.isPending}>{createProvider.isPending ? 'Saving...' : 'Add system provider'}</Button>
          </div>
        </form>
      </Card>
    </div>
  );
}

function toRequest(values: FormValues): CreateSystemProviderRequest {
  return {
    name: values.name,
    providerType: values.providerType,
    fixedValue: values.providerType === 'Fixed' ? values.fixedValue : null,
    initialValue: values.providerType === 'Sequence' ? values.initialValue : null,
  };
}

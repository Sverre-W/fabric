import { Link, useParams } from '@tanstack/react-router';

import { AutomationPageShell } from './automation-page-shell';
import { WorkflowDefinitionEditor } from './elsa-studio-elements';

export default function WorkflowDefinitionEditorPage() {
  const { definitionId } = useParams({ from: '/main/automation/workflow-definitions/$definitionId/edit' });

  return (
    <AutomationPageShell title="Workflow Definition Editor" description="Edit workflow structure, activities, inputs, outputs, and persistence settings in Elsa Studio.">
      {(props) => (
        <div className="grid gap-3">
          <Link to="/automation/workflow" search={{ tab: 'definitions' } as never} className="w-fit rounded-interactive border border-border px-3 py-2 text-[14px] font-semibold transition hover:bg-hover-blue">
            Back to workflow definitions
          </Link>
          <WorkflowDefinitionEditor {...props} definitionId={definitionId} />
        </div>
      )}
    </AutomationPageShell>
  );
}

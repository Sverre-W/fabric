import { Link, useParams } from '@tanstack/react-router';

import { AutomationPageShell } from './automation-page-shell';
import { WorkflowInstanceViewer } from './elsa-studio-elements';

export default function WorkflowInstanceViewerPage() {
  const { instanceId } = useParams({ from: '/main/automation/workflow-instances/$instanceId' });

  return (
    <AutomationPageShell title="Workflow Instance Viewer" description="View workflow instance state, execution log, variables, and completed activities.">
      {(props) => (
        <div className="grid gap-3">
          <Link to="/automation/workflow" search={{ tab: 'history' } as never} className="w-fit rounded-interactive border border-border px-3 py-2 text-[14px] font-semibold transition hover:bg-hover-blue">
            Back to workflow instances
          </Link>
          <WorkflowInstanceViewer {...props} instanceId={instanceId} />
        </div>
      )}
    </AutomationPageShell>
  );
}

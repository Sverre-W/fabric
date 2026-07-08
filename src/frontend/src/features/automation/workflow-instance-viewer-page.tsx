import { useAuth } from 'react-oidc-context';
import { useNavigate, useParams } from '@tanstack/react-router';

import { elsaApiBaseUrl } from './automation-settings';
import { ElsaStudioViewerScreen } from './elsa-studio-fullscreen';

export default function WorkflowInstanceViewerPage() {
  const auth = useAuth();
  const navigate = useNavigate();
  const { instanceId } = useParams({ from: '/main/automation/workflow-instances/$instanceId' });

  return (
    <ElsaStudioViewerScreen
      instanceId={instanceId}
      runtime={{ remoteEndpoint: elsaApiBaseUrl, accessToken: auth.user?.access_token }}
      onEditWorkflowDefinition={(definitionId) => void navigate({ to: '/automation/workflow-definitions/$definitionId/edit', params: { definitionId } })}
    />
  );
}

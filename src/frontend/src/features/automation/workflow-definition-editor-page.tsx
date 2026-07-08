import { useAuth } from 'react-oidc-context';
import { useNavigate, useParams } from '@tanstack/react-router';

import { elsaApiBaseUrl } from './automation-settings';
import { ElsaStudioEditorScreen } from './elsa-studio-fullscreen';

export default function WorkflowDefinitionEditorPage() {
  const auth = useAuth();
  const navigate = useNavigate();
  const { definitionId } = useParams({ from: '/main/automation/workflow-definitions/$definitionId/edit' });

  return (
    <ElsaStudioEditorScreen
      definitionId={definitionId}
      runtime={{ remoteEndpoint: elsaApiBaseUrl, accessToken: auth.user?.access_token }}
      onWorkflowDefinitionExecuted={(workflowInstanceId) => void navigate({ to: '/automation/workflow-instances/$instanceId', params: { instanceId: workflowInstanceId } })}
    />
  );
}

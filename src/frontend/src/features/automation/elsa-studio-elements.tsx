import { createElement, useEffect, useRef, type Ref } from 'react';

type ElsaElementProps = {
  readonly remoteEndpoint: string;
  readonly accessToken?: string;
  readonly className?: string;
};

type WorkflowDefinitionEditorProps = ElsaElementProps & {
  readonly definitionId: string;
  readonly onWorkflowDefinitionExecuted?: (workflowInstanceId: string) => void;
};

type WorkflowInstanceViewerProps = ElsaElementProps & {
  readonly instanceId: string;
  readonly onEditWorkflowDefinition?: (definitionId: string) => void;
};

type WorkflowDefinitionListProps = ElsaElementProps & {
  readonly onEditWorkflowDefinition: (definitionId: string) => void;
};

type WorkflowInstanceListProps = ElsaElementProps & {
  readonly onViewWorkflowInstance: (instanceId: string) => void;
};

type ElsaCustomElement = HTMLElement & {
  editWorkflowDefinition?: (definitionId: string) => void;
  viewWorkflowInstance?: (instanceId: string) => void;
  workflowDefinitionExecuted?: (workflowInstanceId: string) => void;
};

export function WorkflowDefinitionList({ onEditWorkflowDefinition, ...props }: WorkflowDefinitionListProps) {
  const ref = useRef<ElsaCustomElement>(null);

  useEffect(() => {
    if (ref.current) {
      ref.current.editWorkflowDefinition = onEditWorkflowDefinition;
    }
  }, [onEditWorkflowDefinition]);

  return createElsaElement('elsa-workflow-definition-list', props, ref);
}

export function WorkflowDefinitionEditor({ definitionId, ...props }: WorkflowDefinitionEditorProps) {
  const ref = useRef<ElsaCustomElement>(null);

  useEffect(() => {
    if (ref.current)
      ref.current.workflowDefinitionExecuted = props.onWorkflowDefinitionExecuted;
  }, [props.onWorkflowDefinitionExecuted]);

  return createElsaElement('elsa-workflow-definition-editor', { ...props, definitionId }, ref);
}

export function WorkflowInstanceList({ onViewWorkflowInstance, ...props }: WorkflowInstanceListProps) {
  const ref = useRef<ElsaCustomElement>(null);

  useEffect(() => {
    if (ref.current) {
      ref.current.viewWorkflowInstance = onViewWorkflowInstance;
    }
  }, [onViewWorkflowInstance]);

  return createElsaElement('elsa-workflow-instance-list', props, ref);
}

export function WorkflowInstanceViewer({ instanceId, ...props }: WorkflowInstanceViewerProps) {
  const ref = useRef<ElsaCustomElement>(null);

  useEffect(() => {
    if (ref.current)
      ref.current.editWorkflowDefinition = props.onEditWorkflowDefinition;
  }, [props.onEditWorkflowDefinition]);

  return createElsaElement('elsa-workflow-instance-viewer', { ...props, instanceId }, ref);
}

function createElsaElement(tagName: string, props: ElsaElementProps & { definitionId?: string; instanceId?: string }, ref?: Ref<ElsaCustomElement>) {
  return createElement(tagName, {
    ref,
    'remote-endpoint': props.remoteEndpoint,
    'access-token': props.accessToken,
    'definition-id': props.definitionId,
    'instance-id': props.instanceId,
    class: props.className ?? 'fabric-elsa-studio-root block h-full w-full',
  });
}

import { Component, type ErrorInfo, type ReactNode } from 'react';
import PlayerStateScreen from './PlayerStateScreen';
import { getDisplayStatePresentation } from './displayPresentation.mjs';

type ErrorBoundaryProps = {
  children: ReactNode;
};

type ErrorBoundaryState = {
  hasError: boolean;
};

export default class ErrorBoundary extends Component<
  ErrorBoundaryProps,
  ErrorBoundaryState
> {
  public state: ErrorBoundaryState = { hasError: false };

  public static getDerivedStateFromError(): ErrorBoundaryState {
    return { hasError: true };
  }

  public componentDidCatch(error: Error, errorInfo: ErrorInfo): void {
    console.error('Unexpected display application error.', error, errorInfo);
  }

  public render(): ReactNode {
    if (this.state.hasError) {
      return <PlayerStateScreen {...getDisplayStatePresentation('unexpected')} onAction={() => window.location.reload()} />;
    }

    return this.props.children;
  }
}

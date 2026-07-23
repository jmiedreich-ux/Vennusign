import { Component, type ErrorInfo, type ReactNode } from 'react';

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
      return (
        <main role="alert">
          <h1>Display unavailable</h1>
          <p>An unexpected error occurred. Reload the display to try again.</p>
        </main>
      );
    }

    return this.props.children;
  }
}

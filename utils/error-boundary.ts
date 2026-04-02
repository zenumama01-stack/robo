 * @fileoverview React error boundary creation utilities.
 * Provides platform-agnostic error boundary components for React applications.
 * @module @memberjunction/react-runtime/runtime
import { ErrorBoundaryOptions, ComponentError } from '../types';
 * Creates a React error boundary component class
 * @param React - React library instance
 * @param options - Error boundary options
 * @returns Error boundary component class
export function createErrorBoundary(React: any, options: ErrorBoundaryOptions = {}): any {
    onError,
    fallback,
    logErrors = true,
    recovery = 'none'
   * Error boundary component class
  return class ErrorBoundary extends React.Component {
    state: { hasError: boolean; error: Error | null; errorInfo: any; retryCount: number };
    constructor(props: any) {
      super(props);
        hasError: false,
    static getDerivedStateFromError(error: Error): any {
      // Update state to trigger fallback UI
      return { hasError: true, error };
    componentDidCatch(error: Error, errorInfo: any) {
      // Log error if enabled
      if (logErrors) {
        console.error('React Error Boundary caught error:', error);
        console.error('Error Info:', errorInfo);
      // Call custom error handler if provided
      if (onError) {
          onError(error, errorInfo);
        } catch (handlerError) {
          console.error('Error in custom error handler:', handlerError);
      // Update state with error details
      this.setState({ errorInfo });
    handleRetry = () => {
      this.setState((prevState: any) => ({
        retryCount: prevState.retryCount + 1
    handleReset = () => {
      this.setState({
    render() {
      if (this.state.hasError) {
        // Use custom fallback if provided
        if (fallback) {
          if (typeof fallback === 'function') {
            return fallback({
              error: this.state.error,
              errorInfo: this.state.errorInfo,
              retry: this.handleRetry,
              reset: this.handleReset,
              retryCount: this.state.retryCount
        // Default error UI
        const showRetry = recovery === 'retry' && this.state.retryCount < 3;
        const showReset = recovery === 'reset';
        return React.createElement(
          'div',
              backgroundColor: '#f8f8f8',
              margin: '10px'
          React.createElement('h2', { style: { color: '#d32f2f' } }, 'Component Error'),
          React.createElement(
            'p',
            { style: { color: '#666' } },
            'An error occurred while rendering this component.'
          this.state.error && React.createElement(
            'details',
            { style: { marginTop: '10px' } },
              'summary',
              { style: { cursor: 'pointer', color: '#333' } },
              'Error Details'
              'pre',
                  backgroundColor: '#f0f0f0',
              this.state.error.toString(),
              '\n\n',
              this.state.error.stack
          (showRetry || showReset) && React.createElement(
            showRetry && React.createElement(
              'button',
                onClick: this.handleRetry,
                  backgroundColor: '#1976d2',
              `Retry (${3 - this.state.retryCount} attempts left)`
            showReset && React.createElement(
                onClick: this.handleReset,
                  backgroundColor: '#757575',
              'Reset Component'
      return this.props.children;
 * Creates a functional error boundary wrapper using React hooks
 * @param Component - Component to wrap
 * @returns Wrapped component with error boundary
export function withErrorBoundary(React: any, Component: any, options: ErrorBoundaryOptions = {}): any {
  const ErrorBoundaryComponent = createErrorBoundary(React, options);
  return (props: any) => {
      ErrorBoundaryComponent,
      React.createElement(Component, props)
 * Formats a component error for display or logging
 * @param error - Error to format
 * @param componentName - Name of the component where error occurred
 * @param phase - Phase when error occurred
export function formatComponentError(
  phase: ComponentError['phase']
): ComponentError {
    message: error.message || 'Unknown error',
      name: error.name,
 * Creates a simple error logger for error boundaries
 * @returns Error logging function
export function createErrorLogger(componentName: string): (error: Error, errorInfo: any) => void {
  return (error: Error, errorInfo: any) => {
    console.group(`🚨 React Component Error: ${componentName}`);
    console.error('Error:', error);
    console.error('Component Stack:', errorInfo.componentStack);
    console.error('Props:', errorInfo.props);

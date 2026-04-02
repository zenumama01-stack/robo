 * @fileoverview Spinner management for CLI progress indication
import type { Ora } from 'ora-classic';
 * Spinner manager for CLI operations
export class SpinnerManager {
    private spinner: Ora | null = null;
     * Start a spinner with the given message
    start(message: string): void {
        if (this.spinner) {
        this.spinner = ora(message).start();
     * Update spinner message
    update(message: string): void {
            this.spinner.text = message;
    succeed(message?: string): void {
            this.spinner.succeed(message);
            this.spinner = null;
    fail(message?: string): void {
            this.spinner.fail(message);
    warn(message?: string): void {
            this.spinner.warn(message);
     * Stop spinner with info message
    info(message?: string): void {
            this.spinner.info(message);
     * Stop spinner without any message
    stop(): void {
     * Check if spinner is currently running
    get isSpinning(): boolean {
        return this.spinner !== null;

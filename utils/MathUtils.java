package org.openhab.core.model.core.internal.util;
 * This class provides a few mathematical helper functions that are required by
 * code of this bundle.
public class MathUtils {
     * calculates the greatest common divisor of two numbers
     * @param m
     *            first number
     * @param n
     *            second number
     * @return the gcd of m and n
    public static int gcd(int m, int n) {
        if (m % n == 0) {
        return gcd(n, m % n);
     * calculates the least common multiple of two numbers
     * @return the lcm of m and n
    public static int lcm(int m, int n) {
        return m * n / gcd(n, m);
     * calculates the greatest common divisor of n numbers
     * @param numbers
     *            an array of n numbers
     * @return the gcd of the n numbers
    public static int gcd(Integer[] numbers) {
        int n = numbers[0];
        for (int m : numbers) {
            n = gcd(n, m);
     * determines the least common multiple of n numbers
     * @return the least common multiple of all numbers of the array
    public static int lcm(Integer[] numbers) {
            n = lcm(n, m);

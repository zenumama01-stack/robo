package org.openhab.core.thing.firmware.types;
 * This class represents a semantic version (semver) with major, minor, and patch components.
 * See <a href="https://semver.org/#summary">Semantic Versioning 2.0</a> for more details.
 * @param major major version number
 * @param minor minor version number
 * @param patch patch version number
public record SemverVersion(int major, int minor, int patch) {
    public static final SemverVersion ZERO = new SemverVersion(0, 0, 0);
     * Creates a SemverVersion from a valid semver string, i.e. <code>major.minor.patch</code>.
     * @param version the semver string to parse
     * @return a SemverVersion
    public static SemverVersion fromString(String version) {
        String[] parts = version.split("\\.");
        if (parts.length != 3) {
            throw new IllegalArgumentException("Invalid version format: " + version);
        return new SemverVersion(Integer.parseInt(parts[0]), Integer.parseInt(parts[1]), Integer.parseInt(parts[2]));
     * Checks if this semver version is greater than the other semver version.
     * @param other the other semver version to compare with
     * @return true if this version is greater than the other, false otherwise
    public boolean isGreaterThan(SemverVersion other) {
        if (this.major != other.major) {
            return this.major > other.major;
        if (this.minor != other.minor) {
            return this.minor > other.minor;
        return this.patch > other.patch;
     * Checks if this semver version is greater than or equal to the other semver version.
     * @return true if this version is greater than or equal to the other, false otherwise
    public boolean isGreaterThanOrEqualTo(SemverVersion other) {
        return this.patch >= other.patch;
     * Checks if this semver version is less than the other semver version.
     * @return true if this version is less than the other, false otherwise
    public boolean isLessThan(SemverVersion other) {
            return this.major < other.major;
            return this.minor < other.minor;
        return this.patch < other.patch;
     * Checks if this semver version is less than or equal to the other semver version.
     * @return true if this version is less than or equal to the other, false otherwise
    public boolean isLessThanOrEqualTo(SemverVersion other) {
        return this.patch <= other.patch;
     * Checks if this semver version is equal to the other semver version.
     * @return true if both versions are equal, false otherwise
    public boolean isEqualTo(SemverVersion other) {
        return this.major == other.major && this.minor == other.minor && this.patch == other.patch;
        return major + "." + minor + "." + patch;

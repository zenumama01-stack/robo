package org.openhab.core.io.transport.modbus.test;
import org.hamcrest.Description;
import org.hamcrest.TypeSafeMatcher;
abstract class AbstractRequestComparer<T extends ModbusWriteRequestBlueprint> extends TypeSafeMatcher<T> {
    private int expectedUnitId;
    private int expectedAddress;
    private ModbusWriteFunctionCode expectedFunctionCode;
    private int expectedMaxTries;
    protected AbstractRequestComparer(int expectedUnitId, int expectedAddress,
            ModbusWriteFunctionCode expectedFunctionCode, int expectedMaxTries) {
        this.expectedUnitId = expectedUnitId;
        this.expectedAddress = expectedAddress;
        this.expectedFunctionCode = expectedFunctionCode;
        this.expectedMaxTries = expectedMaxTries;
    public void describeTo(@NonNullByDefault({}) Description description) {
        description.appendText("should return request with");
        description.appendText(" unitID=");
        description.appendValue(expectedUnitId);
        description.appendText(" address=");
        description.appendValue(expectedAddress);
        description.appendText(" functionCode=");
        description.appendValue(expectedFunctionCode);
        description.appendText(" maxTries=");
        description.appendValue(expectedMaxTries);
    protected boolean matchesSafely(T item) {
        if (item.getUnitID() != expectedUnitId) {
        if (item.getReference() != expectedAddress) {
        if (item.getFunctionCode() != expectedFunctionCode) {
        if (item.getMaxTries() != expectedMaxTries) {
        return doMatchData(item);
    protected abstract boolean doMatchData(T item);

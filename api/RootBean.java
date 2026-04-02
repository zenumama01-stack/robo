package org.openhab.core.io.rest.internal.resources.beans;
 * This is a java bean that is used to define the root entry
 * page of the REST interface.
 * @author Yannick Schaus - Add runtime info
public class RootBean {
    public final String version = RESTConstants.API_VERSION;
    public final String locale;
    @Schema(allowableValues = { "SI", "US" })
    public final String measurementSystem;
    public final String timezone;
    public final RuntimeInfo runtimeInfo = new RuntimeInfo();
    public final List<Links> links = new ArrayList<>();
    public RootBean(LocaleProvider localeProvider, UnitProvider unitProvider, TimeZoneProvider timeZoneProvider) {
        this.locale = localeProvider.getLocale().toString();
        this.measurementSystem = unitProvider.getMeasurementSystem().getName();
        this.timezone = timeZoneProvider.getTimeZone().toString();
    public static class RuntimeInfo {
        public final String version = OpenHAB.getVersion();
        public final String buildString = OpenHAB.buildString();
    public static class Links {
        public Links(String type, String url) {

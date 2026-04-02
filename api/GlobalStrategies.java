package org.openhab.core.model.persistence.scoping;
import org.openhab.core.model.persistence.persistence.impl.StrategyImpl;
 * This class defines a few persistence strategies that are globally available to
 * all persistence models.
public class GlobalStrategies {
    public static final Strategy UPDATE = new StrategyImpl() {
            return "everyUpdate";
    public static final Strategy CHANGE = new StrategyImpl() {
            return "everyChange";
    public static final Strategy RESTORE = new StrategyImpl() {
            return "restoreOnStartup";
    public static final Strategy FORECAST = new StrategyImpl() {
            return "forecast";

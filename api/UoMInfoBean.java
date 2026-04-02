 * This is a java bean that is used to define UoM information for the REST interface.
public class UoMInfoBean {
    public final UoMInfo uomInfo;
    public static class UoMInfo {
        public final List<DimensionInfo> dimensions;
        public static class DimensionInfo {
            public final String dimension;
            public final String systemUnit;
            public DimensionInfo(String dimension, String systemUnit) {
                this.dimension = dimension;
                this.systemUnit = systemUnit;
        public UoMInfo(UnitProvider unitProvider) {
            dimensions = unitProvider.getAllDimensions().stream().map(dimension -> {
                Unit<?> unit = unitProvider.getUnit((Class<? extends Quantity>) dimension);
                String dimensionName = Objects.requireNonNull(UnitUtils.getDimensionName(unit));
                return new DimensionInfo(dimensionName, unit.toString());
            }).sorted(Comparator.comparing(a -> a.dimension)).toList();
    public UoMInfoBean(UnitProvider unitProvider) {
        uomInfo = new UoMInfo(unitProvider);

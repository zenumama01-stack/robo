 * This is a data transfer object that is used to serialize widgets.
 * @author Mark herwege - New fields pattern, unit
 * @author Laurent Garnier - New field columns
 * @author Laurent Garnier - Remove field columns
 * @author Laurent Garnier - New fields row, column, command, releaseCommand and stateless for Button element
@Schema(name = "Widget")
public class WidgetDTO {
     * staticIcon is a boolean indicating if the widget state must be ignored when requesting the icon.
     * It is set to true when the widget has either the staticIcon property set or the icon property set
     * with conditional rules.
    public Boolean staticIcon;
    // widget-specific attributes
    public final List<MappingDTO> mappings = new ArrayList<>();
    public Boolean switchSupport;
    public Boolean releaseOnly;
    public Integer refresh;
    public Integer height;
    public BigDecimal minValue;
    public BigDecimal maxValue;
    public BigDecimal step;
    public String inputHint;
    public String encoding;
    public String service;
    public String period;
    public String yAxisDecimalPattern;
    public String interpolation;
    public Boolean legend;
    public Boolean forceAsItem;
    public Boolean stateless;
    public PageDTO linkedPage;
    // only for frames and button grids, other linkable widgets link to a page
    public final List<WidgetDTO> widgets = new ArrayList<>();
    public WidgetDTO() {

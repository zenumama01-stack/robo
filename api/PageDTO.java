 * This is a data transfer object that is used to serialize page content.
@Schema(name = "Page")
public class PageDTO {
    public PageDTO parent;
    public boolean leaf;
    public boolean timeout;
    public List<WidgetDTO> widgets = new ArrayList<>();
    public PageDTO() {

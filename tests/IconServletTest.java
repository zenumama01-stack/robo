import static org.openhab.core.ui.icon.internal.IconServlet.*;
import javax.servlet.ServletOutputStream;
import javax.servlet.WriteListener;
 * Tests for {@link IconServlet}.
public class IconServletTest {
    private static class ByteArrayServletOutputStream extends ServletOutputStream {
        private ByteArrayOutputStream outputStream = new ByteArrayOutputStream();
            outputStream.write(b);
        public void setWriteListener(@Nullable WriteListener arg0) {
        public String getOutput() {
            return outputStream.toString();
            outputStream.reset();
    private @NonNullByDefault({}) IconServlet servlet;
    private ByteArrayServletOutputStream responseOutputStream = new ByteArrayServletOutputStream();
    private @Mock @NonNullByDefault({}) HttpServletResponse responseMock;
    private @Mock @NonNullByDefault({}) IconProvider provider1Mock;
    private @Mock @NonNullByDefault({}) IconProvider provider2Mock;
    public void before() throws IOException {
        servlet = new IconServlet();
        responseOutputStream.reset();
    public void testPriority() throws ServletException, IOException {
        when(requestMock.getRequestURI()).thenReturn("/icon/x");
        when(requestMock.getParameter(PARAM_FORMAT)).thenReturn("svg");
        when(requestMock.getParameter(PARAM_ICONSET)).thenReturn("test");
        when(requestMock.getParameter(PARAM_STATE)).thenReturn("34");
        when(responseMock.getOutputStream()).thenReturn(responseOutputStream);
        when(provider1Mock.hasIcon("x", "test", Format.SVG)).thenReturn(0);
        when(provider1Mock.getIcon("x", "test", "34", Format.SVG))
                .thenReturn(new ByteArrayInputStream("provider 1 icon: x test 34 svg".getBytes()));
        servlet.addIconProvider(provider1Mock);
        servlet.doGet(requestMock, responseMock);
        assertEquals("provider 1 icon: x test 34 svg", responseOutputStream.getOutput());
        verify(responseMock, never()).sendError(anyInt());
        when(provider2Mock.hasIcon("x", "test", Format.SVG)).thenReturn(1);
        when(provider2Mock.getIcon("x", "test", "34", Format.SVG))
                .thenReturn(new ByteArrayInputStream("provider 2 icon: x test 34 svg".getBytes()));
        servlet.addIconProvider(provider2Mock);
        assertEquals("provider 2 icon: x test 34 svg", responseOutputStream.getOutput());
    public void testMissingIcon() throws ServletException, IOException {
        when(requestMock.getRequestURI()).thenReturn("/icon/missing_for_test.png");
        when(provider1Mock.hasIcon(anyString(), anyString(), isA(Format.class))).thenReturn(null);
        assertEquals("", responseOutputStream.getOutput());
        verify(responseMock).sendError(404);
    public void testAnyFormatFalse() throws ServletException, IOException {
        when(requestMock.getRequestURI()).thenReturn("/icon/z");
        when(requestMock.getParameter(PARAM_ANY_FORMAT)).thenReturn("false");
        when(provider1Mock.hasIcon("z", "test", Format.SVG)).thenReturn(0);
        when(provider1Mock.getIcon("z", "test", "34", Format.SVG))
                .thenReturn(new ByteArrayInputStream("provider 1 icon: z test 34 svg".getBytes()));
        assertEquals("provider 1 icon: z test 34 svg", responseOutputStream.getOutput());
        verify(provider1Mock, never()).hasIcon("z", "test", Format.PNG);
    public void testAnyFormatSameProviders() throws ServletException, IOException {
        when(requestMock.getParameter(PARAM_ANY_FORMAT)).thenReturn("true");
        when(provider1Mock.hasIcon("z", "test", Format.PNG)).thenReturn(0);
        verify(provider1Mock, atLeastOnce()).hasIcon("z", "test", Format.PNG);
        verify(provider1Mock, atLeastOnce()).hasIcon("z", "test", Format.SVG);
    public void testAnyFormatHigherPriorityOtherFormat() throws ServletException, IOException {
        when(provider2Mock.hasIcon("z", "test", Format.PNG)).thenReturn(1);
        when(provider2Mock.hasIcon("z", "test", Format.SVG)).thenReturn(null);
        when(provider2Mock.getIcon("z", "test", "34", Format.PNG))
                .thenReturn(new ByteArrayInputStream("provider 2 icon: z test 34 png".getBytes()));
        assertEquals("provider 2 icon: z test 34 png", responseOutputStream.getOutput());
        verify(provider2Mock, atLeastOnce()).hasIcon("z", "test", Format.PNG);
        verify(provider2Mock, atLeastOnce()).hasIcon("z", "test", Format.SVG);
    public void testAnyFormatHigherPriorityRequestedFormat() throws ServletException, IOException {
        when(provider2Mock.hasIcon("z", "test", Format.PNG)).thenReturn(null);
        when(provider2Mock.hasIcon("z", "test", Format.SVG)).thenReturn(1);
        when(provider2Mock.getIcon("z", "test", "34", Format.SVG))
                .thenReturn(new ByteArrayInputStream("provider 2 icon: z test 34 svg".getBytes()));
        assertEquals("provider 2 icon: z test 34 svg", responseOutputStream.getOutput());
    public void testAnyFormatNoOtherFormat() throws ServletException, IOException {
        when(provider1Mock.hasIcon("z", "test", Format.PNG)).thenReturn(null);
    public void testAnyFormatNoRequestedFormat() throws ServletException, IOException {
        when(provider1Mock.hasIcon("z", "test", Format.SVG)).thenReturn(null);
        when(provider1Mock.getIcon("z", "test", "34", Format.PNG))
                .thenReturn(new ByteArrayInputStream("provider 1 icon: z test 34 png".getBytes()));
        assertEquals("provider 1 icon: z test 34 png", responseOutputStream.getOutput());

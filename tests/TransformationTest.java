public class TransformationTest {
    public void testTransform() {
        String result = Transformation.transform("UnknownTransformation", "function", "test");
        assertThat(result, is("test"));
    public void testTransformRaw() {
                () -> Transformation.transformRaw("UnknownTransformation", "function", "test"));
        assertThat(e.getMessage(), is("No transformation service 'UnknownTransformation' could be found."));

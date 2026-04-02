public class StringsCompleterTest {
    public void completeSimple() {
        var sc = new StringsCompleter(List.of("def", "abc", "ghi"), false);
        // positive match
        assertTrue(sc.complete(new String[] { "a" }, 0, 1, candidates));
        assertEquals("abc ", candidates.getFirst());
        candidates.clear();
        // negative match
        assertFalse(sc.complete(new String[] { "z" }, 0, 1, candidates));
        // case insensitive
        assertTrue(sc.complete(new String[] { "A" }, 0, 1, candidates));
        // second argument
        assertTrue(sc.complete(new String[] { "a", "d" }, 1, 1, candidates));
        assertEquals("def ", candidates.getFirst());
        // cursor not at end of word (truncates rest)
        assertTrue(sc.complete(new String[] { "a", "dg" }, 1, 1, candidates));
        // first argument when second is present
        assertTrue(sc.complete(new String[] { "a", "d" }, 0, 1, candidates));
    public void caseSensitive() {
        var sc = new StringsCompleter(List.of("dEf", "ABc", "ghi"), true);
        assertFalse(sc.complete(new String[] { "D" }, 0, 1, candidates));
        assertFalse(sc.complete(new String[] { "ab" }, 0, 1, candidates));
        assertTrue(sc.complete(new String[] { "AB" }, 0, 1, candidates));
        assertEquals("ABc ", candidates.getFirst());
    public void multipleCandidates() {
        var sc = new StringsCompleter(List.of("abcde", "bcde", "abcdef", "abcdd", "abcdee", "abcdf"), false);
        assertTrue(sc.complete(new String[] { "abcd" }, 0, 4, candidates));
        assertEquals(5, candidates.size());
        assertEquals("abcdd ", candidates.getFirst());
        assertEquals("abcde ", candidates.get(1));
        assertEquals("abcdee ", candidates.get(2));
        assertEquals("abcdef ", candidates.get(3));
        assertEquals("abcdf ", candidates.get(4));
        assertTrue(sc.complete(new String[] { "abcde" }, 0, 5, candidates));
        assertEquals(3, candidates.size());
        assertEquals("abcde ", candidates.getFirst());
        assertEquals("abcdee ", candidates.get(1));
        assertEquals("abcdef ", candidates.get(2));
        assertTrue(sc.complete(new String[] { "abcdee" }, 0, 6, candidates));
        assertEquals("abcdee ", candidates.getFirst());

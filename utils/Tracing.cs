    /// Tracer.
    public sealed partial class Tracer : EtwActivity
        /// DebugMessage.
        [EtwEvent(0xc000)]
        public void DebugMessage(Exception exception)
            DebugMessage(GetExceptionString(exception));
        /// Converts exception object into a string.
        public static string GetExceptionString(Exception exception)
            while (WriteExceptionText(sb, exception))
        private static bool WriteExceptionText(StringBuilder sb, Exception e)
            sb.Append(e.GetType().Name);

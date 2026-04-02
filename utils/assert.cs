// The define below is only valid for this file. It allows the methods
// defined here to call Diagnostics.Assert when only ASSERTIONS_TRACE is defined
// Any #if DEBUG is pointless (always true) in this file because of this declaration.
// The presence of the define will cause the System.Diagnostics.Debug.Asser calls
// always to be compiled in for this file. What can be compiled out are the calls to
// System.Management.Automation.Diagnostics.Assert in other files when neither DEBUG
// nor ASSERTIONS_TRACE is defined.
#define DEBUG
    /// Exception with a full stack trace excluding the last two frames.
    internal class AssertException : SystemException
        /// Calls the base class with message and sets the stack frame.
        /// <param name="message">Repassed to the base class.</param>
        internal AssertException(string message) : base(message)
            // 3 will skip the assertion caller, this method and AssertException.StackTrace
            StackTrace = Diagnostics.StackTrace(3);
        /// Returns the stack trace set in the constructor.
        /// <value>the constructor's stackTrace</value>
        public override string StackTrace { get; }
    /// This class contain the few methods necessary for
    /// the basic assertion use.
    /// All methods are public and static.
    /// The class cannot derive from the sealed System.Diagnostics.Debug
    /// The class was also made sealed.
    /// Diagnostics.Assert(x >= 0,"A negative x would have caused early return.");
    internal sealed class Diagnostics
        internal static string StackTrace(int framesToSkip)
            StackTrace trace = new StackTrace(true);
            StackFrame[] frames = trace.GetFrames();
            StringBuilder frameString = new StringBuilder();
            int maxFrames = 10;
            maxFrames += framesToSkip;
            for (int i = framesToSkip; (i < frames.Length) && (i < maxFrames); i++)
                StackFrame frame = frames[i];
                frameString.Append(frame.ToString());
            return frameString.ToString();
        private static readonly object s_throwInsteadOfAssertLock = 1;
        private static bool s_throwInsteadOfAssert = false;
        /// If set to true will prevent the assertion dialog from showing up
        /// by throwing an exception instead of calling Debug.Assert.
        /// <value>false for dialog, true for exception</value>
        internal static bool ThrowInsteadOfAssert
                lock (s_throwInsteadOfAssertLock)
                    return s_throwInsteadOfAssert;
                    s_throwInsteadOfAssert = value;
        /// This class only has statics, so we shouldn't need to instantiate any object.
        private Diagnostics() { }
        /// Basic assertion with logical condition and message.
        /// <param name="condition">
        /// logical condition that should be true for program to proceed
        /// <param name="whyThisShouldNeverHappen">
        /// Message to explain why condition should always be true
        // These two lines are playing havoc with asmmeta. Since only one asmmeta file
        // can be checked in at a time if you compile the asmmeta for a fre build then
        // the checked can't compile against it since these methods will not exist. If
        // you check in the chk asmmeta the fre build will not compile because it is
        // not expecting these methods to exist.
        internal static void Assert(
            [DoesNotReturnIf(false)]
            bool condition,
            string whyThisShouldNeverHappen)
            Diagnostics.Assert(condition, whyThisShouldNeverHappen, string.Empty);
        /// Basic assertion with logical condition, message and detailed message.
        /// <param name="detailMessage">
        /// Additional information about the assertion
            string whyThisShouldNeverHappen,
            string detailMessage)
            // Early out avoids some slower code below (mostly the locking done in ThrowInsteadOfAssert).
            if (condition)
#if ASSERTIONS_TRACE
                if (Diagnostics.ThrowInsteadOfAssert)
                    string assertionMessage = "ASSERT: " + whyThisShouldNeverHappen + "  " + detailMessage + " ";
                    AssertException e = new AssertException(assertionMessage);
                builder.Append("ASSERT: ");
                builder.Append(whyThisShouldNeverHappen);
                builder.Append(".");
                if (detailMessage.Length != 0)
                    builder.Append(detailMessage);
                // 2 to skip this method and Diagnostics.StackTace
                builder.Append(Diagnostics.StackTrace(2));
                tracer.TraceError(builder.ToString());
                throw new AssertException(assertionMessage);
            System.Diagnostics.Debug.Fail(whyThisShouldNeverHappen, detailMessage);

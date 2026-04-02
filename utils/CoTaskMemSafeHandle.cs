/*============================================================
**
** Purpose:
** This internal class is a SafeHandle implementation over a
** native CoTaskMem allocated via StringToCoTaskMemAuto.
============================================================*/
namespace System.Diagnostics.Eventing.Reader
    // Marked as SecurityCritical due to link demands from inherited
    // SafeHandle members.
    internal sealed class CoTaskMemSafeHandle : SafeHandle
        internal CoTaskMemSafeHandle()
            : base(IntPtr.Zero, true)
        internal void SetMemory(IntPtr handle)
            SetHandle(handle);
        internal IntPtr GetMemory()
            return handle;
                return IsClosed || handle == IntPtr.Zero;
            Marshal.FreeCoTaskMem(handle);
            handle = IntPtr.Zero;
        // DONT compare CoTaskMemSafeHandle with CoTaskMemSafeHandle.Zero
        // use IsInvalid instead. Zero is provided where a NULL handle needed
        public static CoTaskMemSafeHandle Zero
                return new CoTaskMemSafeHandle();

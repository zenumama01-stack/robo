using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
    /// Partial class implementation for SeparatedList control.
    public partial class ScalableImageSource : Freezable
        /// Initializes a new instance of the <see cref="Microsoft.Management.UI.Internal.ScalableImageSource" /> class.
        public ScalableImageSource()
        /// Creates a new instance of the Freezable derived class.
        /// <returns>The new instance of the Freezable derived class.</returns>
        protected override Freezable CreateInstanceCore()
            return new ScalableImageSource();
        #endregion Overrides

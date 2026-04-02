    /// Converts a an ImageButtonBase to its corresponding ToolTip.
    [SuppressMessage("Microsoft.MSInternal", "CA903:InternalNamespaceShouldNotContainPublicTypes", Justification = "Needed for XAML")]
    public class ImageButtonToolTipConverter : IValueConverter
        // This class is meant to be used like this in XAML:
        //  <Window xmlns:controls="clr-namespace:Microsoft.PowerShell.Commands.ShowCommandInternal" ...>
        //     ...
        //     <Window.Resources>
        //        <controls:RoutedUICommandToString x:Key="routedUICommandToString"/>
        //     </Window.Resources>
        //     <ContentControl ToolTip="{Binding Path=..., Converter={StaticResource routedUICommandToString}"/>
        /// Converts a an ImageButtonBase to its corresponding ToolTip by checking if it has a tooltip property
        /// or a command with tooltip text
        /// <param name="value">The ImageButtonBase we are trying to Convert.</param>
        /// <param name="targetType"><paramref name="targetType"/> is not used.</param>
        /// <param name="parameter"><paramref name="parameter"/> is not used.</param>
        /// <param name="culture"><paramref name="culture"/> is not used.</param>
        /// <returns>The resulting object obtained from retrieving the property value in <paramref name="parameter"/> (or property values if <paramref name="parameter"/> contains dots) out of <paramref name="value"/>. .</returns>
            ImageButtonBase imageButtonBase = value as ImageButtonBase;
            if (imageButtonBase == null)
            object toolTipObj = imageButtonBase.GetValue(Button.ToolTipProperty);
            if (toolTipObj != null)
                return toolTipObj.ToString();
            if (imageButtonBase.Command != null && !string.IsNullOrEmpty(imageButtonBase.Command.Text))
                return imageButtonBase.Command.Text.Replace("_", string.Empty);
        /// This method is not supported.
        /// <param name="value"><paramref name="value"/> is not used.</param>
        /// <returns>No value is returned.</returns>

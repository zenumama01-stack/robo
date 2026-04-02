    /// Defines a method which will be called when
    /// a condition is met.
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <param name="item">The parameter to pass to the method.</param>
    internal delegate void RetryActionCallback<T>(T item);
    /// Provides common WPF methods for use in the library.
    internal static class WpfHelp
        #region RetryActionAfterLoaded
        private static Dictionary<FrameworkElement, RetryActionAfterLoadedDataQueue> retryActionData =
            new Dictionary<FrameworkElement, RetryActionAfterLoadedDataQueue>();
        /// Calls a method when the Loaded event is fired on a FrameworkElement.
        /// <typeparam name="T">The type of the parameter to pass to the callback method.</typeparam>
        /// <param name="element">The element whose Loaded state we are interested in.</param>
        /// <param name="callback">The method we will call if element.IsLoaded is false.</param>
        /// <param name="parameter">The parameter to pass to the callback method.</param>
        /// Returns true if the element is not loaded and the callback will be called
        /// when the element is loaded, false otherwise.
        public static bool RetryActionAfterLoaded<T>(FrameworkElement element, RetryActionCallback<T> callback, T parameter)
            if (element.IsLoaded)
            RetryActionAfterLoadedDataQueue data;
            if (!retryActionData.TryGetValue(element, out data))
                data = new RetryActionAfterLoadedDataQueue();
                retryActionData.Add(element, data);
            data.Enqueue(callback, parameter);
            element.Loaded += Element_Loaded;
            element.ApplyTemplate();
        private static void Element_Loaded(object sender, RoutedEventArgs e)
            FrameworkElement element = (FrameworkElement)sender;
            element.Loaded -= Element_Loaded;
            if (!retryActionData.TryGetValue(element, out data)
                || data.IsEmpty)
                throw new InvalidOperationException("Event loaded callback data expected.");
            Delegate callback;
            object parameter;
            data.Dequeue(out callback, out parameter);
            if (data.IsEmpty)
                retryActionData.Remove(element);
            callback.DynamicInvoke(parameter);
        private class RetryActionAfterLoadedDataQueue
            private Queue<Delegate> callbacks = new Queue<Delegate>();
            private Queue<object> parameters = new Queue<object>();
            /// Adds a callback with its associated parameter to the collection.
            /// <param name="callback">The callback to invoke.</param>
            /// <param name="parameter">The parameter to pass to the callback.</param>
            public void Enqueue(Delegate callback, object parameter)
                this.callbacks.Enqueue(callback);
                this.parameters.Enqueue(parameter);
            /// Removes a callback with its associated parameter from the head of
            /// the collection.
            public void Dequeue(out Delegate callback, out object parameter)
                callback = null;
                parameter = null;
                if (this.callbacks.Count < 1)
                    throw new InvalidOperationException("Trying to remove when there is no data");
                callback = this.callbacks.Dequeue();
                parameter = this.parameters.Dequeue();
            /// Gets whether there is any callback data available.
            public bool IsEmpty
                    return this.callbacks.Count == 0;
        #endregion RetryActionAfterLoaded
        #region RemoveFromParent/AddChild
        /// Removes the specified element from its parent.
        /// <param name="element">The element to remove.</param>
        /// <exception cref="NotSupportedException">The specified value does not have a parent that supports removal.</exception>
        public static void RemoveFromParent(FrameworkElement element)
            ArgumentNullException.ThrowIfNull(element);
            // If the element has already been detached, do nothing \\
            if (element.Parent == null)
            ContentControl parentContentControl = element.Parent as ContentControl;
            if (parentContentControl != null)
                parentContentControl.Content = null;
            var parentDecorator = element.Parent as Decorator;
            if (parentDecorator != null)
                parentDecorator.Child = null;
            ItemsControl parentItemsControl = element.Parent as ItemsControl;
            if (parentItemsControl != null)
                parentItemsControl.Items.Remove(element);
            Panel parentPanel = element.Parent as Panel;
            if (parentPanel != null)
                parentPanel.Children.Remove(element);
            var parentAdorner = element.Parent as UIElementAdorner;
            if (parentAdorner != null)
                parentAdorner.Child = null;
            throw new NotSupportedException("The specified value does not have a parent that supports removal.");
        /// <param name="parent">The parent element.</param>
        /// <param name="element">The element to add.</param>
        public static void AddChild(FrameworkElement parent, FrameworkElement element)
            ArgumentNullException.ThrowIfNull(parent, nameof(element));
            ContentControl parentContentControl = parent as ContentControl;
                parentContentControl.Content = element;
            var parentDecorator = parent as Decorator;
                parentDecorator.Child = element;
            ItemsControl parentItemsControl = parent as ItemsControl;
                parentItemsControl.Items.Add(element);
            Panel parentPanel = parent as Panel;
                parentPanel.Children.Add(element);
            throw new NotSupportedException("The specified parent doesn't support children.");
        #endregion RemoveFromParent/AddChild
        #region VisualChild
        /// Returns the first visual child that matches the type T.
        /// Performs a breadth-first search.
        /// <typeparam name="T">The type of the child to find.</typeparam>
        /// <param name="obj">The object with a visual tree.</param>
        /// <returns>Returns an object of type T if found, otherwise null.</returns>
        public static T GetVisualChild<T>(DependencyObject obj) where T : DependencyObject
            if (obj == null)
            var elementQueue = new Queue<DependencyObject>();
            elementQueue.Enqueue(obj);
            while (elementQueue.Count > 0)
                var element = elementQueue.Dequeue();
                T item = element as T;
                if (item != null)
                    return item;
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
                    var child = VisualTreeHelper.GetChild(element, i);
                    elementQueue.Enqueue(child);
        /// Finds all children of type within the specified object's visual tree.
        /// <returns>All children of the specified object matching the specified type.</returns>
        public static List<T> FindVisualChildren<T>(DependencyObject obj)
            where T : DependencyObject
            Debug.Assert(obj != null, "obj is null");
            ArgumentNullException.ThrowIfNull(obj);
            List<T> childrenOfType = new List<T>();
            // Recursively loop through children looking for children of type within their trees \\
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
                DependencyObject childObj = VisualTreeHelper.GetChild(obj, i);
                T child = childObj as T;
                if (child != null)
                    childrenOfType.Add(child);
                    // Recurse \\
                    childrenOfType.AddRange(FindVisualChildren<T>(childObj));
            return childrenOfType;
        #endregion VisualChild
        /// Searches ancestors for data of the specified type.
        /// <typeparam name="T">The type of the data to find.</typeparam>
        /// <param name="obj">The visual whose ancestors are searched.</param>
        public static T FindVisualAncestorData<T>(this DependencyObject obj)
            where T : class
            FrameworkElement parent = obj.FindVisualAncestor<FrameworkElement>();
                T data = parent.DataContext as T;
                if (data != null)
                    return data;
                    return parent.FindVisualAncestorData<T>();
        /// Walks up the visual tree looking for an ancestor of a given type.
        /// <typeparam name="T">The type to look for.</typeparam>
        /// <param name="object">The object to start from.</param>
        /// <returns>The parent of the right type, or null.</returns>
        public static T FindVisualAncestor<T>(this DependencyObject @object) where T : class
            ArgumentNullException.ThrowIfNull(@object, nameof(@object));
            DependencyObject parent = VisualTreeHelper.GetParent(@object);
                T parentObj = parent as T;
                if (parentObj != null)
                    return parentObj;
                return parent.FindVisualAncestor<T>();
        /// Executes the <see cref="RoutedCommand"/> on the current command target if it is allowed.
        /// <param name="command">The routed command.</param>
        /// <param name="parameter">A user defined data type.</param>
        /// <param name="target">The command target.</param>
        /// <returns><c>true</c> if the command could execute; otherwise, <c>false</c>.</returns>
        public static bool TryExecute(this RoutedCommand command, object parameter, IInputElement target)
            ArgumentNullException.ThrowIfNull(command);
            if (command.CanExecute(parameter, target))
                command.Execute(parameter, target);
        #region TemplateChild
        /// Gets the named child of an item from a templated control.
        /// <typeparam name="T">The type of the child.</typeparam>
        /// <param name="templateParent">The parent of the control.</param>
        /// <param name="childName">The name of the child.</param>
        /// <returns>The reference to the child, or null if the template part wasn't found.</returns>
        public static T GetOptionalTemplateChild<T>(Control templateParent, string childName) where T : FrameworkElement
            ArgumentNullException.ThrowIfNull(templateParent);
            ArgumentException.ThrowIfNullOrEmpty(childName);
            object templatePart = templateParent.Template.FindName(childName, templateParent);
            T item = templatePart as T;
            if (item == null && templatePart != null)
                HandleWrongTemplatePartType<T>(childName);
        /// <returns>The reference to the child.</returns>
        public static T GetTemplateChild<T>(Control templateParent, string childName) where T : FrameworkElement
            T item = GetOptionalTemplateChild<T>(templateParent, childName);
            if (item == null)
                HandleMissingTemplatePart<T>(childName);
        /// Throws an exception with information about the template part with the wrong type.
        /// <typeparam name="T">The type of the expected template part.</typeparam>
        /// <param name="name">The name of the expected template part.</param>
        private static void HandleWrongTemplatePartType<T>(string name)
            throw new ApplicationException(string.Format(
                "A template part with the name of '{0}' is not of type {1}.",
                name,
                typeof(T).Name));
        /// Throws an exception with information about the missing template part.
        public static void HandleMissingTemplatePart<T>(string name)
                "A template part with the name of '{0}' and type of {1} was not found.",
        #endregion TemplateChild
        #region SetComponentResourceStyle
        /// Sets Style for control given a component resource key.
        /// <typeparam name="T">Type in which Component Resource Style is Defined.</typeparam>
        /// <param name="element">Element whose style need to be set.</param>
        /// <param name="keyName">Component Resource Key for Style.</param>
        public static void SetComponentResourceStyle<T>(FrameworkElement element, string keyName) where T : FrameworkElement
            ComponentResourceKey styleKey = new ComponentResourceKey(typeof(T), keyName);
            element.Style = (Style)element.FindResource(styleKey);
        #endregion SetComponentResourceStyle
        #region CreateRoutedPropertyChangedEventArgs
        /// Helper function to create a RoutedPropertyChangedEventArgs from a DependencyPropertyChangedEventArgs.
        /// <typeparam name="T">The type for the RoutedPropertyChangedEventArgs.</typeparam>
        /// <param name="propertyEventArgs">The DependencyPropertyChangedEventArgs data source.</param>
        /// <returns>The created event args, configured from the parameter.</returns>
        public static RoutedPropertyChangedEventArgs<T> CreateRoutedPropertyChangedEventArgs<T>(DependencyPropertyChangedEventArgs propertyEventArgs)
            RoutedPropertyChangedEventArgs<T> eventArgs = new RoutedPropertyChangedEventArgs<T>(
                                                                    (T)propertyEventArgs.OldValue,
                                                                    (T)propertyEventArgs.NewValue);
            return eventArgs;
        /// <param name="routedEvent">The routed event the property change is associated with.</param>
        public static RoutedPropertyChangedEventArgs<T> CreateRoutedPropertyChangedEventArgs<T>(DependencyPropertyChangedEventArgs propertyEventArgs, RoutedEvent routedEvent)
                                                                    (T)propertyEventArgs.NewValue,
                                                                    routedEvent);
        #endregion CreateRoutedPropertyChangedEventArgs
        #region ChangeIndex
        /// Moves the item in the specified collection to the specified index.
        /// <param name="items">The collection to move the item in.</param>
        /// <param name="item">The item to move.</param>
        /// <param name="newIndex">The new index of the item.</param>
        /// <exception cref="ArgumentException">The specified item is not in the specified collection.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The specified index is not valid for the specified collection.</exception>
        public static void ChangeIndex(ItemCollection items, object item, int newIndex)
            if (!items.Contains(item))
                throw new ArgumentException("The specified item is not in the specified collection.", "item");
            if (newIndex < 0 || newIndex > items.Count)
                throw new ArgumentOutOfRangeException("newIndex", "The specified index is not valid for the specified collection.");
            int oldIndex = items.IndexOf(item);
            // If the tile isn't moving, don't do anything \\
            if (newIndex == oldIndex)
            items.Remove(item);
            // If adding to the end, add instead of inserting \\
            if (newIndex > items.Count)
                items.Add(item);
                items.Insert(newIndex, item);
        #endregion ChangeIndex

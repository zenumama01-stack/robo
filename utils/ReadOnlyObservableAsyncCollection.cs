using System.Collections.ObjectModel;
using System.Collections.Specialized;
    /// Represents a read-only ObservableCollection which also implement IAsyncProgress.
    /// <typeparam name="T">The type held by the collection.</typeparam>
    public class ReadOnlyObservableAsyncCollection<T> :
        ReadOnlyCollection<T>,
        IAsyncProgress,
        INotifyPropertyChanged, INotifyCollectionChanged
        #region Private fields
        private IAsyncProgress asyncProgress;
        #endregion Private fields
        /// The constructor.
        /// <param name="list">The collection with which to create this instance of the ReadOnlyObservableAsyncCollection class.
        /// The object must also implement IAsyncProgress, INotifyCollectionChanged and INotifyPropertyChanged.</param>
        public ReadOnlyObservableAsyncCollection(IList<T> list)
            : base(list)
            this.asyncProgress = list as IAsyncProgress;
            ((INotifyCollectionChanged)this.Items).CollectionChanged += this.HandleCollectionChanged;
            ((INotifyPropertyChanged)this.Items).PropertyChanged += this.HandlePropertyChanged;
        #endregion Constructors
        #region Events
        /// Occurs when the collection changes, either by adding or removing an item.
        /// see <see cref="INotifyCollectionChanged"/>
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        /// Occurs when a property changes.
        /// see <see cref="INotifyPropertyChanged"/>
        #endregion Events
        #region IAsyncProgress
        public bool OperationInProgress
                if (this.asyncProgress == null)
                    return this.asyncProgress.OperationInProgress;
        /// Gets the error for the async operation.  This field is only valid if
        public Exception OperationError
                    return this.asyncProgress.OperationError;
        #endregion IAsyncProgress
        #region Private Methods
        private void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
            NotifyCollectionChangedEventHandler eh = this.CollectionChanged;
                eh(this, args);
        private void OnPropertyChanged(PropertyChangedEventArgs args)
            PropertyChangedEventHandler eh = this.PropertyChanged;
        // forward CollectionChanged events from the base list to our listeners
        private void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            this.OnCollectionChanged(e);
        // forward PropertyChanged events from the base list to our listeners
        private void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
            this.OnPropertyChanged(e);
        #endregion Private Methods

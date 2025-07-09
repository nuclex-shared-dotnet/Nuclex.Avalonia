#region Apache License 2.0
/*
Nuclex Foundation libraries for .NET
Copyright (C) 2002-2025 Markus Ewald / Nuclex Development Labs

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
#endregion // Apache License 2.0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;


#if !NO_SPECIALIZED_COLLECTIONS
using System.Collections.Specialized;
#endif

using Nuclex.Support.Collections;

namespace Nuclex.Avalonia.Collections {

  /// <summary>
  ///   List which fires events when items are added or removed, whilst also
  ///   lazily fetching items as needed (for example from a socket or database)
  /// </summary>
  /// <typeparam name="TItem">Type of items the list manages</typeparam>
  public abstract class VirtualObservableReadOnlyList<TItem> :
    IList<TItem>,
    IList,
    ICollection,
#if !NO_SPECIALIZED_COLLECTIONS
    INotifyCollectionChanged,
#endif
    IObservableCollection<TItem> {

    #region class Enumerator

    /// <summary>Enumerates over the items in a virtual list</summary>
    private class Enumerator : IEnumerator<TItem>, IEnumerator {

      /// <summary>Initializes a new virtual list enumerator</summary>
      /// <param name="virtualList">List whose items will be enumerated</param>
      public Enumerator(VirtualObservableReadOnlyList<TItem> virtualList) {
        this.virtualList = virtualList;
        this.currentItemIndex = -1;
        this.lastMoveNextResult = false;

        Reset();
      }

      /// <summary>Immediately releases all resources owned by the instance</summary>
      public void Dispose() {
        this.virtualList = null!; // Only to make life easier got the GC
      }

      /// <summary>The item at the enumerator's current position</summary>
      public TItem Current {
        get {
#if DEBUG
          checkVersion();
#endif

          if(this.lastMoveNextResult == false) {
            throw new InvalidOperationException("Enumerator is not on a valid position");
          }

          this.virtualList.requireCount();
          this.virtualList.requirePage(this.currentItemIndex / this.virtualList.pageSize);

          return this.virtualList.typedList[this.currentItemIndex];
        }
      }

      /// <summary>Advances the enumerator to the next item</summary>
      /// <returns>True if there was a next item</returns>
      public bool MoveNext() {
#if DEBUG
        checkVersion();
#endif

        this.virtualList.requireCount();

        // Go forward if there potentially are items remaining. The count may still be
        // unreliable at this point (due to the uncertain count mechanism that truncates
        // the list when fetching items finds an earlier end of the list)
        if(this.currentItemIndex < this.virtualList.assumedCount.Value) {

          // If the enumerator's 'Current' property is never used an the virtual list
          // uses the dynamic truncation features (for unknown list sizes), then this
          // enumerator could be moved way past the last element via 'MoveNext()'.
          this.virtualList.requirePage(this.currentItemIndex / this.virtualList.pageSize);
          ++this.currentItemIndex; // Accept potentially advancing past the end here

        }

        // Are we on a valid item? If so, return true to indicate the list continued,
        // otherwise we must have hit the end (or are already past it).
        return this.lastMoveNextResult = (
          (this.currentItemIndex < this.virtualList.assumedCount.Value)
        );
      }

      /// <summary>Resets the enumerator to its initial position</summary>
      public void Reset() {
        this.currentItemIndex = -1;
#if DEBUG
        this.expectedVersion = this.virtualList.version;
#endif
      }

      /// <summary>The item at the enumerator's current position</summary>
      object IEnumerator.Current {
        get { return Current!; } // No idea what the compiler's issue is here
      }

#if DEBUG
      /// <summary>Ensures that the virtual list has not changed</summary>
      private void checkVersion() {
        if(this.expectedVersion != this.virtualList.version)
          throw new InvalidOperationException("Virtual list has been modified");
      }
#endif

      /// <summary>Virtual list the enumerator belongs to</summary>
      private VirtualObservableReadOnlyList<TItem> virtualList;
      /// <summary>Index of the item the enumerator currently is in</summary>
      private int currentItemIndex;
      /// <summary>The most recent result returned from MoveNext()</summary>
      private bool lastMoveNextResult;
#if DEBUG
      /// <summary>Version the virtual list is expected to have</summary>
      private int expectedVersion;
#endif
    }

    #endregion // class Enumerator

    /// <summary>Raised when an item has been added to the collection</summary>
    public event EventHandler<ItemEventArgs<TItem>>? ItemAdded;
    /// <summary>Raised when an item is removed from the collection</summary>
    public event EventHandler<ItemEventArgs<TItem>>? ItemRemoved;
    /// <summary>Raised when an item is replaced in the collection</summary>
    public event EventHandler<ItemReplaceEventArgs<TItem>>? ItemReplaced;
    /// <summary>Raised when the collection is about to be cleared</summary>
    /// <remarks>
    ///   This could be covered by calling ItemRemoved for each item currently
    ///   contained in the collection, but it is often simpler and more efficient
    ///   to process the clearing of the entire collection as a special operation.
    /// </remarks>
    public event EventHandler? Clearing;
    /// <summary>Raised when the collection has been cleared</summary>
    public event EventHandler? Cleared;

#if !NO_SPECIALIZED_COLLECTIONS
    /// <summary>Called when the collection has changed</summary>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
#endif

    /// <summary>
    ///   Initializes a new instance of the ObservableList class that is empty.
    /// </summary>
    /// <param name="pageSize">
    ///   How many items to download in one batch
    /// </param>
    /// <remarks>
    ///   The <paramref name="pageSize" /> can be set to one to request items
    ///   individually or to a larger value in order to improve efficiency when
    ///   the source of the items is a database or similar source that gains
    ///   performance from requesting multiple items at once.
    /// </remarks>
    public VirtualObservableReadOnlyList(int pageSize = 32) {
      this.typedList = new TItem[0];
      this.objectList = (IList)this.typedList;
      this.pageSize = pageSize;
      this.fetchedPages = new bool[0];
    }

    /// <summary>
    ///   Marks all items as non-fetched, causing them to be requested again
    /// </summary>
    /// <param name="purgeItems">
    ///   Whether to also clear the items that may already be in memory but would
    ///   get overwritten on the next fetch. If items consume a lot of memory, this
    ///   will make them available for garbage collection.
    /// </param>
    public void InvalidateAll(bool purgeItems = false) {
      if(this.assumedCount.HasValue) { // If not fetched before, no action needed
        int pageCount = this.fetchedPages.Length;
        for(int index = 0; index < pageCount; ++index) {
          this.fetchedPages[index] = false;
        }

        if(purgeItems) {
          int itemCount = this.assumedCount.Value;
          for(int index = 0; index < itemCount; ++index) {
            this.typedList[index] = default(TItem)!; // not going to be exposed to users
          }
        }
      }
    }

    /// <summary>
    ///   Marks an items as non-fetched, causing it to be requested again on access
    /// </summary>
    /// <param name="itemIndex">
    ///   Index of the item that will be marked as non-fetched
    /// </param>
    /// <param name="purgeItems">
    ///   Whether to also clear the items that may already be in memory but would
    ///   get overwritten on the next fetch. If items consume a lot of memory, this
    ///   will make them available for garbage collection.
    /// </param>
    /// <remarks>
    ///   Since the list works in pages, this will actually mark the whole page as
    ///   non-fetched, causing all items in the same page to be requested again
    ///   when any of them are next accessed.
    /// </remarks>
    public void Invalidate(int itemIndex, bool purgeItems = false) {
      if(this.assumedCount.HasValue) { // If not fetched before, no action needed
        int pageIndex = itemIndex / this.pageSize;
        this.fetchedPages[pageIndex] = false;

        if(purgeItems) {
          int count = Math.Min(
            this.assumedCount.Value - (this.pageSize * pageIndex),
            this.pageSize
          );
          for(int index = itemIndex / this.pageSize; index < count; ++index) {
            this.typedList[index] = default(TItem)!; // not going to be exposed to users
          }
        }
      }
    }

    /// <summary>Determines the index of the specified item in the list</summary>
    /// <param name="item">Item whose index will be determined</param>
    /// <returns>The index of the item in the list or -1 if not found</returns>
    public int IndexOf(TItem item) {
      requireCount();
      requireAllPages();

      // TODO: this won't work, it will compare the placeholder items :-/

      IComparer<TItem> itemComparer = Comparer<TItem>.Default;
      for(int index = 0; index < this.assumedCount.Value; ++index) {
        if(itemComparer.Compare(this.typedList[index], item) == 0) {
          return index;
        }
      }

      return -1;
    }

    /// <summary>Inserts an item into the list at the specified index</summary>
    /// <param name="index">Index the item will be inserted at</param>
    /// <param name="item">Item that will be inserted into the list</param>
    public void Insert(int index, TItem item) {
      throw new NotSupportedException("Cannot insert items into a read-only list");
    }

    /// <summary>Removes the item at the specified index from the list</summary>
    /// <param name="index">Index at which the item will be removed</param>
    public void RemoveAt(int index) {
      throw new NotSupportedException("Cannot remove items from a read-only list");
    }

    /// <summary>Accesses the item at the specified index in the list</summary>
    /// <param name="index">Index of the item that will be accessed</param>
    /// <returns>The item at the specified index</returns>
    public TItem this[int index] {
      get {
        requireCount();
        requirePage(index / this.pageSize);

        return this.typedList[index];
      }
      set {
        // Make sure the page is fetched, otherwise, the item would only suddenly
        // revert to its state in the source when the pages around it is fetchd later.
        requireCount();
        requirePage(index / this.pageSize);
#if DEBUG
        ++this.version;
#endif
        TItem oldItem = this.typedList[index];
        this.typedList[index] = value;
        OnReplaced(oldItem, value, index);
      }
    }

    /// <summary>Adds an item to the end of the list</summary>
    /// <param name="item">Item that will be added to the list</param>
    public void Add(TItem item) {
      throw new NotSupportedException("Cannot add items to a read-only list");
    }

    /// <summary>Removes all items from the list</summary>
    public void Clear() {
      throw new NotSupportedException("Cannot clear a read-only list");
    }

    /// <summary>Checks whether the list contains the specified item</summary>
    /// <param name="item">Item the list will be checked for</param>
    /// <returns>True if the list contains the specified items</returns>
    public bool Contains(TItem item) {
      return (IndexOf(item) != -1);
    }

    /// <summary>Copies the contents of the list into an array</summary>
    /// <param name="array">Array the list will be copied into</param>
    /// <param name="arrayIndex">
    ///   Index in the target array where the first item will be copied to
    /// </param>
    public void CopyTo(TItem[] array, int arrayIndex) {
      requireCount();
      requireAllPages();

      this.typedList.CopyTo(array, arrayIndex);
    }

    /// <summary>Total number of items in the list</summary>
    public int Count {
      get {
        requireCount();
        return this.assumedCount.Value;
      }
    }

    /// <summary>Whether the list is a read-only list</summary>
    public bool IsReadOnly {
      get { return this.typedList.IsReadOnly; }
    }

    /// <summary>Removes the specified item from the list</summary>
    /// <param name="item">Item that will be removed from the list</param>
    /// <returns>
    ///   True if the item was found and removed from the list, false otherwise
    /// </returns>
    public bool Remove(TItem item) {
      throw new NotSupportedException("Cannot remove items from a read-only list");
    }

    /// <summary>Returns an enumerator for the items in the list</summary>
    /// <returns>An enumerator for the list's items</returns>
    public IEnumerator<TItem> GetEnumerator() {
      return new Enumerator(this);
    }

    #region IEnumerable implementation

    /// <summary>Returns an enumerator for the items in the list</summary>
    /// <returns>An enumerator for the list's items</returns>
    IEnumerator IEnumerable.GetEnumerator() {
      return this.objectList.GetEnumerator(); // TODO
    }

    #endregion // IEnumerable implementation

    #region ICollection implementation

    /// <summary>Copies the contents of the list into an array</summary>
    /// <param name="array">Array the list will be copied into</param>
    /// <param name="arrayIndex">
    ///   Index in the target array where the first item will be copied to
    /// </param>
    void ICollection.CopyTo(Array array, int arrayIndex) {
      requireCount();
      requireAllPages();

      this.objectList.CopyTo(array, arrayIndex);
    }

    /// <summary>Whether this list performs thread synchronization</summary>
    bool ICollection.IsSynchronized {
      get { return this.objectList.IsSynchronized; }
    }

    /// <summary>Synchronization root used by the list to synchronize threads</summary>
    object ICollection.SyncRoot {
      get { return this.objectList.SyncRoot; }
    }

    #endregion // ICollection implementation

    #region IList implementation

    /// <summary>Adds an item to the list</summary>
    /// <param name="value">Item that will be added to the list</param>
    /// <returns>
    ///   The position at which the item has been inserted or -1 if the item was not inserted
    /// </returns>
    int IList.Add(object value) {
      throw new NotSupportedException("Cannot add items into a read-only list");
    }

    /// <summary>Checks whether the list contains the specified item</summary>
    /// <param name="item">Item the list will be checked for</param>
    /// <returns>True if the list contains the specified items</returns>
    bool IList.Contains(object item) {
      requireCount();
      requireAllPages();
      return this.objectList.Contains(item);
    }

    /// <summary>Determines the index of the specified item in the list</summary>
    /// <param name="item">Item whose index will be determined</param>
    /// <returns>The index of the item in the list or -1 if not found</returns>
    int IList.IndexOf(object item) {
      requireCount();
      requireAllPages();
      return this.objectList.IndexOf(item);
    }

    /// <summary>Inserts an item into the list at the specified index</summary>
    /// <param name="index">Index the item will be inserted at</param>
    /// <param name="item">Item that will be inserted into the list</param>
    void IList.Insert(int index, object item) {
      throw new NotSupportedException("Cannot insert items into a read-only list");
    }

    /// <summary>Whether the list is of a fixed size</summary>
    bool IList.IsFixedSize {
      get { return this.objectList.IsFixedSize; }
    }

    /// <summary>Removes the specified item from the list</summary>
    /// <param name="item">Item that will be removed from the list</param>
    void IList.Remove(object item) {
      throw new NotSupportedException("Cannot remove items from a read-only list");
    }

    /// <summary>Accesses the item at the specified index in the list</summary>
    /// <param name="index">Index of the item that will be accessed</param>
    /// <returns>The item at the specified index</returns>
    object IList.this[int index] {
      get {
        requireCount();
        requirePage(index / this.pageSize);

        return this.objectList[index];
      }
      set {
        // Make sure the page is fetched, otherwise, the item would only suddenly
        // revert to its state in the source when the pages around it is fetchd later.
        requireCount();
        requirePage(index / this.pageSize);
#if DEBUG
        ++this.version;
#endif
        TItem oldItem = this.typedList[index];
        this.objectList[index] = value;
        TItem newItem = this.typedList[index];
        OnReplaced(oldItem, newItem, index);
      }
    }

    #endregion // IList implementation

    /// <summary>Fires the 'ItemAdded' event</summary>
    /// <param name="item">Item that has been added to the collection</param>
    /// <param name="index">Index of the added item</param>
    protected virtual void OnAdded(TItem item, int index) {
      if(ItemAdded != null) {
        ItemAdded(this, new ItemEventArgs<TItem>(item));
      }
#if !NO_SPECIALIZED_COLLECTIONS
      if(CollectionChanged != null) {
        CollectionChanged(
          this,
          new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index)
        );
      }
#endif
    }

    /// <summary>Fires the 'ItemRemoved' event</summary>
    /// <param name="item">Item that has been removed from the collection</param>
    /// <param name="index">Index the item has been removed from</param>
    protected virtual void OnRemoved(TItem item, int index) {
      if(ItemRemoved != null) {
        ItemRemoved(this, new ItemEventArgs<TItem>(item));
      }
#if !NO_SPECIALIZED_COLLECTIONS
      if(CollectionChanged != null) {
        CollectionChanged(
          this,
          new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index)
        );
      }
#endif
    }

    /// <summary>Fires the 'ItemReplaced' event</summary>
    /// <param name="oldItem">Item that has been replaced</param>
    /// <param name="newItem">New item the original item was replaced with</param>
    /// <param name="index">Index of the replaced item</param>
    protected virtual void OnReplaced(TItem oldItem, TItem newItem, int index) {
      if(ItemReplaced != null) {
        ItemReplaced(this, new ItemReplaceEventArgs<TItem>(oldItem, newItem));
      }
#if !NO_SPECIALIZED_COLLECTIONS
      if(CollectionChanged != null) {
        CollectionChanged(
          this,
          new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Replace, newItem, oldItem, index
          )
        );
      }
#endif
    }

    /// <summary>Fires the 'Clearing' event</summary>
    protected virtual void OnClearing() {
      if(Clearing != null) {
        Clearing(this, EventArgs.Empty);
      }
    }

    /// <summary>Fires the 'Cleared' event</summary>
    protected virtual void OnCleared() {
      if(Cleared != null) {
        Cleared(this, EventArgs.Empty);
      }
#if !NO_SPECIALIZED_COLLECTIONS
      if(CollectionChanged != null) {
        CollectionChanged(this, Constants.NotifyCollectionResetEventArgs);
      }
#endif
    }

    /// <summary>Counts the total number of items in the virtual collection</summary>
    /// <returns>The total number of items</returns>
    protected abstract int CountItems();

    /// <summary>Fetches a page required by the collection</summary>
    /// <param name="target">List into which the items should be fetched</param>
    /// <param name="startIndex">
    ///   Index of the first item to fetch. This is both the start index in
    ///   the actual data and the element index at which to write into the list.
    /// </param>
    /// <param name="count">Number of items that should be fetched</param>
    /// <returns>The number of items that were actually fetched</returns>
    /// <remarks>
    ///   If you fetch fewer than the requested number of items here, you will immediately
    ///   truncate the entire list (it will assume that the end was reached, a means
    ///   to support cases where the total number is not known). Fetching more than
    ///   the requested number of items will just put items in memory that the list will
    ///   continue to think are empty and fetch again if they are actually accessed.
    /// </remarks>
    protected abstract int FetchItems(IList<TItem> target, int startIndex, int count);

    /// <summary>Ensures that the total number of items is known</summary>
    [MemberNotNull(nameof(assumedCount))]
    private void requireCount() {
      if(!this.assumedCount.HasValue) {
        int itemCount = CountItems();
        this.assumedCount = itemCount;

        int pageCount = (itemCount + this.pageSize - 1) / this.pageSize;
        this.fetchedPages = new bool[pageCount];
      }
    }

    /// <summary>Ensures that all items have fetched</summary>
    /// <remarks>
    ///   Avoid if possible.
    /// </remarks>
    private void requireAllPages() {
      Debug.Assert(
        this.assumedCount.HasValue,
        "This method should only be called when item count is already known"
      );
      int pageCount = this.fetchedPages.Length;
      for(int index = 0; index < pageCount; ++index) {
        requirePage(index);
      }
    }

    /// <summary>Ensures that the specified page has been fetched</summary>
    /// <param name="pageIndex">Index of the page that needs to be fetched</param>
    private void requirePage(int pageIndex) {
      Debug.Assert(
        this.assumedCount.HasValue,
        "This method should only be called when item count is already known"
      );
      if(!this.fetchedPages[pageIndex]) {
        int count = Math.Min(
          this.assumedCount!.Value - (this.pageSize * pageIndex),
          this.pageSize
        );

        int fetchedItemCount = FetchItems(this.typedList, pageIndex * this.pageSize, count);
        if(fetchedItemCount < this.pageSize) {
          this.assumedCount = pageIndex * this.pageSize + fetchedItemCount;
        }

        this.fetchedPages[pageIndex] = true;
      }
    }

    /// <summary>Number of items the collection believes it has</summary>
    private int? assumedCount;
    /// <summary>Number of items to fetch in a single request</summary>
    private readonly int pageSize;
    /// <summary>Tracks which pages have been fetched so far</summary>
    private bool[] fetchedPages;
    /// <summary>The wrapped list under its type-safe interface</summary>
    private TItem[] typedList;
    /// <summary>The wrapped list under its object interface</summary>
    private IList objectList;
#if DEBUG
    /// <summary>Used to detect when enumerators go out of sync</summary>
    private int version;
#endif

  }

} // namespace Nuclex.Avalonia.Collections

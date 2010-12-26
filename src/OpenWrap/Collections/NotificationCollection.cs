using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OpenWrap.Collections
{
    public class NotificationCollection<T> : Collection<T>
    {
        public NotificationCollection(Func<T, T> add, Action<T> remove, IEnumerable<T> source)
                : this(add, remove)
        {
            foreach (var item in source)
            {
                Add(item);
            }
        }

        public NotificationCollection(Func<T, T> add, Action<T> remove)
        {
            AddHandler = add;
            RemoveHandler = remove;
        }

        public NotificationCollection()
        {
        }

        protected Func<T, T> AddHandler { get; private set; }
        protected Action<T> RemoveHandler { get; private set; }

        protected void AddItemCore(T item)
        {
            base.InsertItem(base.Count, item);
        }

        protected override void ClearItems()
        {
            foreach (var item in this)
                HandleRemove(item);
            ClearItemsCore();
        }

        protected void ClearItemsCore()
        {
            base.ClearItems();
        }

        protected virtual T HandleAdd(T item)
        {
            if (AddHandler != null)
                return AddHandler(item);
            return default(T);
        }

        protected virtual void HandleRemove(T item)
        {
            if (RemoveHandler != null)
                RemoveHandler(item);
        }

        protected override void InsertItem(int index, T item)
        {
            item = HandleAdd(item);

            InsertItemCore(item, index);
        }

        protected void InsertItemCore(T item, int index)
        {
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            HandleRemove(this.Items[index]);

            RemoveItemCore(index);
        }

        protected void RemoveItemCore(int index)
        {
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            if (this.Items.Count > index)
                RemoveHandler(Items[index]);
            item = HandleAdd(item);

            SetItemCore(index, item);
        }

        protected void SetItemCore(int index, T item)
        {
            base.SetItem(index, item);
        }
    }
}
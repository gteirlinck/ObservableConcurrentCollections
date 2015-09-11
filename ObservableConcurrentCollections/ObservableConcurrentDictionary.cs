using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using static System.Collections.Specialized.NotifyCollectionChangedAction;
using System.ComponentModel;
using System.Collections;

namespace ObservableConcurrentCollections
{
    public class ObservableConcurrentDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public ObservableConcurrentDictionary()
            : base()
        { }

        public ObservableConcurrentDictionary(IEqualityComparer<TKey> comparer)
           : base(comparer)
        { }

        public ObservableConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
            : base(collection)
        { }

        public ObservableConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
            : base(collection, comparer)
        { }

        public ObservableConcurrentDictionary(int concurrencyLevel, int capacity)
            : base(concurrencyLevel, capacity)
        { }

        public ObservableConcurrentDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer)
            : base(concurrencyLevel, capacity, comparer)
        { }

        public ObservableConcurrentDictionary(int concurrencyLevel, IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
            : base(concurrencyLevel, collection, comparer)
        { }

        public new TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            return AddOrUpdate(key, k => { return addValue; }, updateValueFactory);
        }

        public new TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            return base.AddOrUpdate(key, (k) =>
            {
                TValue newValue = addValueFactory(k);

                // 1. Notify
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(Add, newValue));

                // 2. Return
                return newValue;
            },
            (k, oldValue) =>
            {
                // 1. Check if TValue implements INotifyPropertyChanged and wire its PropertyChanged event
                if (oldValue as INotifyPropertyChanged != null)
                    ((INotifyPropertyChanged)oldValue).PropertyChanged += (s, e) => PropertyChanged?.Invoke(s, e);

                // 2. Update the value using the provided factory
                TValue updatedValue = updateValueFactory(k, oldValue);

                // 3. Return
                return updatedValue;
            });
        }

        public new void Clear()
        {
            // 1. Clear
            base.Clear();

            // 2. Notify
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(Reset));
        }

        public new TValue GetOrAdd(TKey key, TValue value)
        {
            return GetOrAdd(key, k => { return value; });
        }

        public new TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            return base.GetOrAdd(key, k =>
            {
                TValue newValue = valueFactory(k);

                // 1. Notify
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(Add, new ArrayList() { newValue }));

                // 2. Return
                return newValue;
            });
        }

        public new bool TryAdd(TKey key, TValue value)
        {
            if (base.TryAdd(key, value))
            {
                // 1. Notify
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(Add, value));

                // 2. Return
                return true;
            }
            else
                return false;
        }

        public new bool TryRemove(TKey key, out TValue value)
        {
            if (base.TryRemove(key, out value))
            {
                // 1. Notify
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(Remove, value));

                // 2. Return
                return true;
            }
            else
                return false;
        }

        public new bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            if (base.TryUpdate(key, newValue, comparisonValue))
            {
                // 1. Notify
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(Replace, newValue, comparisonValue));

                // 2. Return
                return true;
            }
            else
                return false;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

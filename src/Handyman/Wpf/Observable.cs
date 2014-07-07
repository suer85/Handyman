﻿using Handyman.Annotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Handyman.Wpf
{
    public class Observable<T> : IObservable<T>
    {
        private T _value;

        public Observable(T value = default (T))
        {
            _value = value;
        }

        public T Value
        {
            get { return _value; }
            set
            {
                if (EqualityComparer<T>.Default.Equals(value, _value)) return;
                _value = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Observable<TItem, TValue> : IObservable<TValue>
        where TItem : INotifyPropertyChanged
    {
        private readonly ObservableCollection<TItem> _collection;
        private readonly Func<IReadOnlyList<TItem>, TValue> _valueGetter;
        private readonly Action<IList<TItem>, TValue> _valueSetter;
        private TValue _value;
        private bool _isValueChanging;

        internal Observable(IEnumerable<TItem> items, Func<IReadOnlyList<TItem>, TValue> valueGetter, Action<IList<TItem>, TValue> valueSetter)
        {
            _collection = items as ObservableCollection<TItem> ?? items.ToObservableCollection();
            _collection.CollectionChanged += OnCollectionChanged;
            _collection.ForEach(x => x.PropertyChanged += OnItemPropertyChanged);
            _valueGetter = valueGetter;
            _valueSetter = valueSetter;
            _value = _valueGetter(_collection);
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.OldItems != null)
                args.OldItems.Cast<TItem>().ForEach(x => x.PropertyChanged -= OnItemPropertyChanged);
            if (args.NewItems != null)
                args.NewItems.Cast<TItem>().ForEach(x => x.PropertyChanged += OnItemPropertyChanged);
            if (_isValueChanging) return;
            var value = _valueGetter(_collection);
            SetValue(value, Cascade.No);
        }

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_isValueChanging) return;
            var value = _valueGetter(_collection);
            SetValue(value, Cascade.No);
        }

        public TValue Value
        {
            get { return _value; }
            set { SetValue(value, Cascade.Yes); }
        }

        private void SetValue(TValue value, Cascade cascade)
        {
            using (new Temp(() => _isValueChanging = true, () => _isValueChanging = false))
            {
                if (EqualityComparer<TValue>.Default.Equals(value, _value)) return;
                if (cascade == Cascade.Yes) _valueSetter(_collection, value);
                _value = value;
                OnPropertyChanged();
            }
        }

        private enum Cascade
        {
            Yes,
            No
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static class Observable
    {
        public static Observable<TItem, TValue> Create<TItem, TValue>(IEnumerable<TItem> items,
                                                                      Func<IReadOnlyList<TItem>, TValue> valueGetter,
                                                                      Action<IList<TItem>, TValue> valueSetter)
            where TItem : INotifyPropertyChanged
        {
            return new Observable<TItem, TValue>(items, valueGetter, valueSetter);
        }

        public static Observable<TItem, TValue> Create<TItem, TValue>(ObservableCollection<TItem> items,
                                                                      Func<IReadOnlyList<TItem>, TValue> valueGetter,
                                                                      Action<IList<TItem>, TValue> valueSetter)
            where TItem : INotifyPropertyChanged
        {
            return new Observable<TItem, TValue>(items, valueGetter, valueSetter);
        }
    }
}
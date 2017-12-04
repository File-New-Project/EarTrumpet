using EarTrumpet.DataModel.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace EarTrumpet.DataModel
{
    public class FilteredAudioDeviceSessionCollection
    {
        ObservableCollection<IAudioDeviceSession> _collection;
        Func<IAudioDeviceSession, bool> _applicabilityCheckCallback;

        public ObservableCollection<IAudioDeviceSession> Sessions { get; private set; }

        public FilteredAudioDeviceSessionCollection(ObservableCollection<IAudioDeviceSession> collection, Func<IAudioDeviceSession,bool> isApplicableCallback)
        {
            _applicabilityCheckCallback = isApplicableCallback;
            _collection = collection;
            _collection.CollectionChanged += Sessions_CollectionChanged;

            Sessions = new ObservableCollection<IAudioDeviceSession>();
            PopulateSessions();
        }

        void PopulateSessions()
        {
            foreach (var item in _collection)
            {
                AddIfApplicable(item);
            }
        }

        private void Sessions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems.Count == 1);

                    AddIfApplicable((IAudioDeviceSession)e.NewItems[0]);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    Debug.Assert(e.OldItems.Count == 1);
                    Sessions.Remove((IAudioDeviceSession)e.OldItems[0]);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    Sessions.Clear();
                    PopulateSessions();
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();

                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();
            }
        }

        void AddIfApplicable(IAudioDeviceSession session)
        {
            if (Sessions.ContainsById(session))
            {
                if (!_applicabilityCheckCallback(session))
                {
                    Sessions.RemoveById(session);
                    session.PropertyChanged += Session_PropertyChanged;
                }
            }
            else
            {
                session.PropertyChanged += Session_PropertyChanged;

                if (_applicabilityCheckCallback(session))
                {
                    Sessions.Add(session);
                }
            }
        }

        private void Session_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "State")
            {
                AddIfApplicable((IAudioDeviceSession)sender);
            }
        }
    }
}

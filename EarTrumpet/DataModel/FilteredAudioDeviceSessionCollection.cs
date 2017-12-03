using System;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace EarTrumpet.DataModel
{
    public class FilteredAudioDeviceSessionCollection : IAudioDeviceSessionCollection
    {
        IAudioDeviceSessionCollection _collection;
        Func<IAudioDeviceSession, bool> _applicabilityCheckCallback;

        public ObservableCollection<IAudioDeviceSession> Sessions { get; private set; }

        public FilteredAudioDeviceSessionCollection(IAudioDeviceSessionCollection collection, Func<IAudioDeviceSession,bool> isApplicableCallback)
        {
            _applicabilityCheckCallback = isApplicableCallback;
            _collection = collection;
            _collection.Sessions.CollectionChanged += Sessions_CollectionChanged;

            Sessions = new ObservableCollection<IAudioDeviceSession>();
            PopulateSessions();
        }

        void PopulateSessions()
        {
            foreach (var item in _collection.Sessions)
            {
                AddRemoveIfApplicable(item);
            }
        }

        private void Sessions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems.Count == 1);
                    AddRemoveIfApplicable((IAudioDeviceSession)e.NewItems[0]);
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

        void AddRemoveIfApplicable(IAudioDeviceSession session)
        {
            if (Sessions.Contains(session)) return;

            if (_applicabilityCheckCallback(session))
            {
                Sessions.Add(session);
                session.PropertyChanged += Session_PropertyChanged;
            }
            else
            {
                if (Sessions.Contains(session))
                {
                    Sessions.Remove(session);
                    session.PropertyChanged += Session_PropertyChanged;
                }
            }
        }

        private void Session_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            AddRemoveIfApplicable((IAudioDeviceSession)sender);
        }
    }
}

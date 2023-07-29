using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Sandbox.Utils
{
    public interface IEventListener
    { }

    public class EventDispatcher
    {
	    private readonly ConcurrentDictionary<Type, List<(WeakReference<IEventListener> listener, Action<Event> callback)>> listeners = new();
	    
	    private readonly object m_syncLock = new object();
	    
	    
		public void RegisterEvent<T>(IEventListener listener, Action<T> callback) where T : Event
		{ 
			var eventType = typeof(T);

			lock ( m_syncLock )
			{
				listeners.AddOrUpdate( eventType,
					new List<(WeakReference<IEventListener> listener, Action<Event> callback)>
					{
						(new WeakReference<IEventListener>( listener ), e => callback.Invoke( (T) e ))
					},
					( key, existingList ) =>
					{
						existingList.Add( (new WeakReference<IEventListener>( listener ), myEvent => callback.Invoke( (T) myEvent )) );
						return existingList;
					} );
			}
		}

		public void UnregisterEvent<T>(IEventListener listenerToUnregister) where T : Event
		{
			var eventType = typeof(T);
			lock (m_syncLock)
			{
				if (listeners.TryGetValue(eventType, out var existingList))
				{
					existingList.RemoveAll(x => x.listener.TryGetTarget(out var listener) && listener == listenerToUnregister);
					if (existingList.Count == 0)
					{
						listeners.TryRemove(eventType, out _);
					}
				}
			}
		}

		public void UnregisterAllEvents(IEventListener eventListener)
		{
			lock (m_syncLock)
			{
				var keysToRemove = new List<Type>();
				foreach (var key in listeners.Keys)
				{
					if (listeners.TryGetValue(key, out var existingList)) 
					{
						existingList.RemoveAll(x => x.listener.TryGetTarget(out var listener) && listener == eventListener);
						if (existingList.Count == 0)
						{
							keysToRemove.Add(key);
						}
					}
				}
				foreach (var key in keysToRemove)
				{
					listeners.TryRemove(key, out _);
				}
			}
		}
        public void SendEvent(Event eventToSend)
        {
	        var eventType = eventToSend.GetType();
	        if (listeners.TryGetValue(eventType, out var eventListeners))
	        {
		        foreach (var (listenerRef, callback) in eventListeners)
		        {
			        if (listenerRef.TryGetTarget(out var listener))
			        {
				        callback.Invoke(eventToSend);
			        }
		        }
	        }
        }

        public void SendEvent<T>(params object[] args) where T : Event
        {
            try
            {
                T instance = TypeLibrary.Create<T>(typeof(T), args);
                SendEvent(instance);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to create event instance and send event: {ex.Message}");
            }
        }
    }

    public class Event
    {
	    public Event()
	    {
	    }
    }
}

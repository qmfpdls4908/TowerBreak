using System;
using System.Collections.Generic;

namespace TowerBreak.EventBus
{
    public sealed class EventBus<T>
    {
        private readonly List<Action<T>> subscribers = new();

        public int SubscriberCount => subscribers.Count;

        public void Subscribe(Action<T> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            subscribers.Add(handler);
        }

        public void Unsubscribe(Action<T> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            subscribers.Remove(handler);
        }

        public void Publish(T payload)
        {
            Action<T>[] snapshot = subscribers.ToArray();

            for (int index = 0; index < snapshot.Length; index++)
            {
                snapshot[index](payload);
            }
        }
    }
}

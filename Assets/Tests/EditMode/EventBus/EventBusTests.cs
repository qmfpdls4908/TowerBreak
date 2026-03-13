using System;
using NUnit.Framework;

namespace TowerBreak.EventBus.Tests
{
    public sealed class EventBusTests
    {
        [Test]
        public void Publish_InvokesSubscribedHandlerWithPayload()
        {
            global::TowerBreak.EventBus.EventBus<int> eventBus = new();
            int received = 0;

            eventBus.Subscribe(payload => received = payload);

            eventBus.Publish(42);

            Assert.That(received, Is.EqualTo(42));
        }

        [Test]
        public void Unsubscribe_RemovesHandlerFromPublish()
        {
            global::TowerBreak.EventBus.EventBus<string> eventBus = new();
            int callCount = 0;
            Action<string> handler = _ => callCount++;

            eventBus.Subscribe(handler);
            eventBus.Unsubscribe(handler);

            eventBus.Publish("ignored");

            Assert.That(callCount, Is.EqualTo(0));
        }

        [Test]
        public void Subscribe_SameHandlerTwice_InvokesHandlerTwice()
        {
            global::TowerBreak.EventBus.EventBus<int> eventBus = new();
            int total = 0;
            Action<int> handler = payload => total += payload;

            eventBus.Subscribe(handler);
            eventBus.Subscribe(handler);

            eventBus.Publish(5);

            Assert.That(total, Is.EqualTo(10));
        }
    }
}

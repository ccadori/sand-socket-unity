using NUnit.Framework;

namespace Tests
{
    public class EventEmitter
    {
        [Test]
        public void ShouldAddAnEvent()
        {
            var emitter = new Sand.EventEmitter();
            emitter.On("test", (string t) => { });

            Assert.IsTrue(emitter.EventDictionary.ContainsKey("test"));
        }

        [Test]
        public void ShoudCallBothEvent()
        {
            int eventIsCalled = 0;
            var emitter = new Sand.EventEmitter();
            emitter.On("test", (string t) => { eventIsCalled++; });
            emitter.On("test", (string t) => { eventIsCalled++; });
            
            emitter.Emit("test", "");

            Assert.AreEqual(eventIsCalled, 2);
        }

        [Test]
        public void ShouldRemoveListeners()
        {
            bool eventIsCalled = false;
            var emitter = new Sand.EventEmitter();

            System.Action<string> action = (string t) => { eventIsCalled = true; };
            emitter.On("test", action);
            emitter.RemoveListener("test", action);
            emitter.Emit("test", "");

            Assert.IsFalse(eventIsCalled);
        }
    }
}

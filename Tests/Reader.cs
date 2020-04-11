using System.Collections.Generic;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace Tests
{
    public class Reader
    {
        [Test]
        public void ShouldEmitTwoEvents()
        {
            List<string> lines = new List<string>();

            var reader = new Sand.Reader((string line) =>
            {
                lines.Add(line);
            },
            "\n");

            reader.OnReceiveData("message1\nmessage2\n");

            Assert.AreEqual(lines.Count, 2);
            Assert.AreEqual(lines[0], "message1");
            Assert.AreEqual(lines[1], "message2");
        }

        [Test]
        public void ShouldEmitEventsWhenMessageIsFullReceived()
        {
            List<string> lines = new List<string>();

            var reader = new Sand.Reader((string line) =>
            {
                lines.Add(line);
            },
            "\n");

            reader.OnReceiveData("messa");

            Assert.AreEqual(lines.Count, 0);
            
            reader.OnReceiveData("ge\n");

            Assert.AreEqual(lines.Count, 1);
            Assert.AreEqual(lines[0], "message");
        }
    
        [Test]
        public void ShouldKeptAnIncompleteMessageInTheBuffer()
        {
            List<string> lines = new List<string>();

            var reader = new Sand.Reader((string line) =>
            {
                lines.Add(line);
            },
            "\n");

            reader.OnReceiveData("message1\nmessa");

            Assert.AreEqual(lines.Count, 1);
            Assert.AreEqual(lines[0], "message1");
            Assert.AreEqual(reader.Buffer, "messa");

            reader.OnReceiveData("ge2\n");

            Assert.AreEqual(lines.Count, 2);
            Assert.AreEqual(lines[1], "message2");
            Assert.AreEqual(reader.Buffer, "");
        }
    }
}

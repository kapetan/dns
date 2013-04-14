using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DNS.Protocol {
    public interface IMessage {
        IList<Question> Questions { get; }

        int Size { get; }
        byte[] ToArray();
    }

    public interface IMessageEntry {
        Domain Name { get; }
        RecordType Type { get; }
        RecordClass Class { get; }

        int Size { get; }
        byte[] ToArray();
    }

    /*public class Message {
        public static void Main(String[] args) {
            string dir = "C:\\Users\\mirza\\Documents\\Visual Studio 2010\\Projects\\DNS\\messages\\";

            foreach (string path in Directory.GetFiles(dir)) {
                byte[] message = File.ReadAllBytes(path);
                bool isRequest = path.EndsWith("request");
                IMessage msg = isRequest ? (IMessage) Request.FromArray(message) : (IMessage) Response.FromArray(message);

                Console.WriteLine("{0} ({2} <=> {3}): {1}", Path.GetFileName(path), msg, msg.Size, message.Length);
                //Console.WriteLine(string.Join(" ", msg.ToArray().Select(b => b.ToString()).ToArray()));
                Console.WriteLine(Enumerable.SequenceEqual(msg.ToArray(), message));
            }
        }
    }*/
}

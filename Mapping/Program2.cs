using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mapping
{
    public class Program2{
    public const int BufferLength = 100000000;
 
        static void Main(string[] args) {

            var type = Convert.ToInt32(Console.ReadLine());

            switch (type)
            {
                case 1:
                    Transmitter transmitter = new Transmitter();
                    var str = "Hello world";
                    while (str!="")
                    {
                        transmitter.BeginTransmit(str);
                        Console.WriteLine("Press 0 to end");
                        str = Console.ReadLine();
                    }
                    Console.ReadKey();
                    transmitter.Dispose();
                    break;
                case 2:
                    Receiver receiver = new Receiver();
                    receiver.BeginReceive();
                    Console.ReadKey();
                    receiver.Dispose();
                    break;
            }
        }
    }
 
    public class Transmitter : IDisposable {
 
        private NamedPipeServerStream pipe = new NamedPipeServerStream("MyPipe", PipeDirection.Out);
 
        public Transmitter() {
            pipe.WaitForConnection();
        }
 
        public void BeginTransmit(string text) {
            var t = new MyClass
            {
                Enum = MyEnum.Val1,
                TripId = text
            };
            using (var stream = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(stream, t);
                var arr = stream.ToArray();
                var length = BitConverter.GetBytes(arr.Length);

                pipe.BeginWrite(length, 0, 4, asyncCallback =>
                {
                    pipe.EndWrite(asyncCallback);
                    pipe.BeginWrite(arr, 0, arr.Length, asyncCallback1 =>
                    {
                        pipe.EndWrite(asyncCallback1);
                    }, null);
                }, null);
            }

        }
 
        public void Dispose() {
            pipe.Dispose();
        }
    }
 
    public class Receiver : IDisposable {
 
        private NamedPipeClientStream pipe = new NamedPipeClientStream(".", "MyPipe", PipeDirection.In);
 
        public Receiver() {
            pipe.Connect();
        }
 
        public void BeginReceive() {
            DateTime dt = DateTime.Now;

            var length = new byte[4];

            pipe.BeginRead(length, 0, 4, new AsyncCallback(result1 =>
            {
                pipe.EndRead(result1);
                byte[] pipeBuffer = new byte[BitConverter.ToInt32(length, 0)];
                pipe.BeginRead(pipeBuffer, 0, pipeBuffer.Length, new AsyncCallback(result =>
                {
                    pipe.EndRead(result);
                    using (var stream = new MemoryStream(pipeBuffer))
                    {
                        var bf = new BinaryFormatter();
                        var res = (MyClass)bf.Deserialize(stream);
                        Console.WriteLine(res);
                    }
                    Console.WriteLine("Pipe: " + (DateTime.Now - dt).TotalMilliseconds + " мс");
                    Task.Run(() => BeginReceive());
                }), null);
            }), null);
        }
 
        public void Dispose() {
            pipe.Dispose();
        }
    }
}

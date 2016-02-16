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
                        transmitter.BeginTransmit(str).Wait();
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
 
        public async Task BeginTransmit(string text) {
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

                await pipe.WriteAsync(length, 0, 4);
                await pipe.WriteAsync(arr, 0, arr.Length);
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
 
        public async Task BeginReceive() {
            DateTime dt = DateTime.Now;

            var length = new byte[4];

            await pipe.ReadAsync(length, 0, 4);
            byte[] pipeBuffer = new byte[BitConverter.ToInt32(length, 0)];
            await pipe.ReadAsync(pipeBuffer, 0, pipeBuffer.Length);
            using (var stream = new MemoryStream(pipeBuffer))
            {
                var bf = new BinaryFormatter();
                var res = (MyClass)bf.Deserialize(stream);
                Console.WriteLine(res);
            }
            Console.WriteLine("Pipe: " + (DateTime.Now - dt).TotalMilliseconds + " мс");
            Task.Run(() => BeginReceive());
        }
 
        public void Dispose() {
            pipe.Dispose();
        }
    }
}

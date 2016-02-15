using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Mapping
{
    class Program
    {
        static void Main(string[] args)
        {
            var type = Convert.ToInt32(Console.ReadLine());
            switch (type)
            {
                case 1:
                    RunSender();
                    break;
                case 2:
                    RunReciver();
                    break;
            }
        }

        private static void RunSender()
        {
            using (var f = MemoryMappedFile.CreateOrOpen("MyMapper", 100, MemoryMappedFileAccess.ReadWrite))
            {
                using (var accessor = f.CreateViewAccessor())
                {
                    var text = "Hello World";
                    var t = new MyClass
                    {
                        Enum = MyEnum.Val1,
                        TripId = "asdadflgh;kflhjkhhhhhhsdfsdfffffffffffffffffffffffffffffffffsd"
                    };
                    using (var stream = new MemoryStream())
                    {
                        var bf = new BinaryFormatter();
                        bf.Serialize(stream, t);
                        var arr = stream.ToArray();
                        accessor.Write(1, arr.Length);
                        accessor.WriteArray(2, arr, 0, arr.Length);
                        accessor.Flush();
                        Console.WriteLine("Wait");
                        Console.ReadLine();
                    }

                }
            }
        }

        private static void RunReciver()
        {
            using (var f = MemoryMappedFile.CreateOrOpen("MyMapper", 100, MemoryMappedFileAccess.ReadWrite))
            {
                using (var accessor = f.CreateViewAccessor())
                {
                    int length = accessor.ReadInt32(1);
                    var bytes = new byte[length];
                    accessor.ReadArray(2, bytes, 0, length);
                    using (var stream = new MemoryStream(bytes))
                    {
                        var bf = new BinaryFormatter();
                        var res = (MyClass) bf.Deserialize(stream);
                        Console.WriteLine(res);
                        Console.ReadLine();
                    }

                }
            }
        }
    }

    [Serializable]
    class MyClass
    {
        public string TripId { get; set; }

        public MyEnum Enum { get; set; }

        public override string ToString()
        {
            return string.Format("{0}--{1}", TripId, Enum);
        }
    }

    enum MyEnum
    {
        Val1, Val2
    }
}

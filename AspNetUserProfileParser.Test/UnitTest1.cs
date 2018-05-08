using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspNetUserProfileParser.Test
{
    [TestClass]
    public class UnitTest1
    {
        const string d1 = @""; //TODO: insert example
        const string d2 = @""; //TODO: insert example
        const string d3 = @""; //TODO: insert example

        [TestMethod]
        public void TestMethod1()
        {
            var ret = UserDefinedFunctions.ReadUserProfileFromString(
                d1,
                d2,
                new SqlBytes(GetByteArray(d3))
                )
                .Cast<object>().ToList()
                ;
        }

        [TestMethod]
        public void TestMethod2()
        {
            var ba = GetByteArray(d3);
            using (var ms = new MemoryStream(ba, false))
            using (var fs = new FileStream("output.bin", FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                ms.CopyTo(fs);
            }
        }

        [TestMethod]
        public void TestMethod3()
        {
            var ba = GetByteArray(d3);
            var f = new BinaryFormatter();
            using (var ms = new MemoryStream(ba, false))
            {
                var obj = f.Deserialize(ms);
                var serializer = new DataContractJsonSerializer(obj.GetType());
                using (var msj = new MemoryStream())
                {
                    serializer.WriteObject(msj, obj);
                    using (var fs = new FileStream("output.json", FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        msj.Seek(0, SeekOrigin.Begin);
                        msj.CopyTo(fs);
                    }

                }
            }
        }

        [TestMethod]
        public void TestMethod4()
        {
            var ba = GetByteArray(d3);
            var f = new BinaryFormatter();
            using (var ms = new MemoryStream(ba, false))
            {
                var obj = f.Deserialize(ms);
                var dic = obj as IDictionary<string, string>;
                Console.WriteLine(string.Join(",", dic.Keys));
                Console.WriteLine(string.Join(",", dic.Values));
            }
        }

        static byte[] GetByteArray(string hexstr)
        {
            var regex = new Regex("(0x)?(?<Hex>[0-9a-fA-F]{2})");
            var match = regex.Match(hexstr);
            var bytes = new List<byte>();
            while (match.Success)
            {
                var hex = match.Groups["Hex"].Value;
                var b = Convert.ToByte(hex, 16);
                bytes.Add(b);
                match = match.NextMatch();
            }
            return bytes.ToArray();
        }
    }
}

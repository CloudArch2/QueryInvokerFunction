using System;
using System.IO;
using FidoFetch.Models;
using System.Runtime.Serialization.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FidoFetch;

namespace UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        public Request CreateRequest()
        {
            return new Request()
            {
                Stores = new string[] { "1234" },
                QueryRange = new DateRange()
                {
                    Start = "10/23/2017 10:00:00",
                    End = "10/24/2017 10:00:00"
                },

                Sources = new Source[]
                {
                    new Source()
                    {
                        Name = "Sales",
                        Fields = new string []
                        {
                            "item",
                            "quanity"
                        }
                    },
                    new Source()
                    {
                        Name = "Partner",
                        Fields = new string []
                        {
                            "Name",
                            "Shift"
                        }
                    }
                }
            };
        }

        [TestMethod]
        public void RequestModel()
        {
            var r = CreateRequest();

            MemoryStream stream1 = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Request));

            ser.WriteObject(stream1, r);

            stream1.Position = 0;
            StreamReader sr = new StreamReader(stream1);
            
            Console.WriteLine(sr.ReadToEnd());
        }

        [TestMethod]
        public void QueryEventModelCCTest()
        {
            var r = CreateRequest();
            var qe = new QueryEvent(r);

            Guid g = new Guid();
            Assert.IsTrue(Guid.TryParse(qe.QueryID, out g), "QueryID not a Guid");

        }

        [TestMethod]
        public void QueryEventModelJsonTest()
        {
            var r = CreateRequest();
            var qe = new QueryEvent(r);

            string json = RegisterQuery.ToJSON<QueryEvent>  (qe);
        }

        [TestMethod]
        public void TestEventPublish()
        {
            var data = new QueryEvent(CreateRequest());

            RegisterQuery.PublishTopic(data);

            Assert.IsTrue(true);
            
        }

        [TestMethod]
        public void FromJson()
        {
            var r = CreateRequest();

            var json = RegisterQuery.ToJSON<Request>(r);

            
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(json);
            writer.Flush();
            stream.Position = 0;

            var outR = new Request();


            RegisterQuery.FromJSON<Request>(stream, out outR);

        }
        
    }
}

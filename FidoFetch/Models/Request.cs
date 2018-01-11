using System.Runtime.Serialization;

namespace FidoFetch.Models
{
    [DataContract]
    public class Request
    {
        [DataMember]
        public string[] Stores { get; set; }

        [DataMember]
        public DateRange QueryRange { get; set; }

        [DataMember]
        public Source[] Sources { get; set; }

    }

    public class DateRange
    {
        [DataMember]
        public string Start { get; set; }
        
        [DataMember]
        public string End { get; set; }
    }

    public class Source
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string[] Fields { get; set; }
    }
}

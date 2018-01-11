using System;
using System.Runtime.Serialization;

namespace FidoFetch.Models
{
    [DataContract]
    public class QueryEvent
    {
        [DataMember]
        public string QueryID { get; set; }

        [DataMember]
        public string CreateDate { get; set; }

        [DataMember]
        public string[] Stores { get; set; }

        [DataMember]
        public DateRange QueryRange { get; set; }

        [DataMember]
        public Source[] Sources { get; set; }

        public QueryEvent (Request r)
        {
            this.QueryID = Guid.NewGuid().ToString();
            this.CreateDate = System.DateTime.UtcNow.ToUniversalTime()
                         .ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
            this.Stores = r.Stores;
            this.QueryRange = r.QueryRange;
            this.Sources = r.Sources;
        }
    }
}

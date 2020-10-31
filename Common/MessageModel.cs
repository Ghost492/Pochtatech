using System;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class MessageModel
    {
        [DataMember]
        public int MessageNumber { get; set; }
        [DataMember]
        public DateTime? SendTime { get; set; }
        [DataMember]
        public string Text { get; set; }
        [DataMember]
        public int Hash { get; set; }
        public DateTime? TimeOfReceipt { get; set; }
    }
}

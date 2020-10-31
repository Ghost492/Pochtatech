using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class Message
    {
        [DataMember][Key]
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

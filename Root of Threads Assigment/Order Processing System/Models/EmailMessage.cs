using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order_Processing_System.Models
{
    public class EmailMessage
    {
        public Guid Id { get; set; }           
        public int OrderId { get; set; }
        // these properties can never be null.if no values are given,they default to emtpy string
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public int Attempts { get; set; }       
        public string? LastError { get; set; }// this property is optional.It is fine if it is null or missing
    }
}

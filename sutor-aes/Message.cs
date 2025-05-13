using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace sutor_aes
{
    struct Message
    {
        public string From { get;set;}
        public string To { get;set;}
        public string Content { get;set;}
        public DateTime Time {  get;set;}

        public Message(string from, string to, string content)
        {
            From = from;
            To = to;
            Content = content;
            Time = DateTime.Now;
        }
    }
}

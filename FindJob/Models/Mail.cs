using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FindJob.Models
{
    public class Mail
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string Introduction { get; set; }
        public string Attachment { get; set; }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Models
{
    public class MailSettings
    {
        public string? ImapHost { get; set; }
        public int ImapPort { get; set; }
        public bool ImapUseSsl { get; set; } = true;
        public string? ImapUsername { get; set; }
        public string? ImapPassword { get; set; }
    }
}

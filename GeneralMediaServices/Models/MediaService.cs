using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeneralMediaServices.Models
{
    public class MediaService
    {
        public int Id { get; set; }
        [DisplayName("Message Description")]
        public string Msg_desc { get; set; } = string.Empty;
        [DisplayName("System")]
        public string Sys_nme { get; set; } = string.Empty;
        [DisplayName("SMS")]
        public string? Sms_no { get; set; } = null;
        [DisplayName("API")]
        public string? Api_url { get; set; } = null;
        [DisplayName("Sent Flag")]
        public bool? Sent_flg { get; set; }
        [DisplayName("Receive Flag")]
        public bool? Receive_flg { get; set; }
        [DisplayName("Sent DateTime")]
        [Column(TypeName = "datetime")]
        public DateTime? Sent_dt { get; set; }
        [DisplayName("Received Date & Time")]
        [Column(TypeName = "datetime")]
        public DateTime? Msg_received_dt { get; set; }
        public string? ErrorLog { get; set; } = null;
        [NotMapped]
        [DisplayName("No")]
        public int NumberCount { get; set; } = 0;

    }
}

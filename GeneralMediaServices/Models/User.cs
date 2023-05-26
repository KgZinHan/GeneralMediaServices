using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace GeneralMediaServices.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string UserPassword { get; set; } = string.Empty;
        [NotMapped]
        public string RetypePassword { get; set; } = string.Empty;
        public string Server { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        [NotMapped]
        [NotNull]
        public string NewPassword { get; set; } = string.Empty;
        [NotMapped]
        [NotNull]
        public string RetypeNewPassword { get; set; } = string.Empty;
    }
}

using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Security.Permissions;

namespace GeneralMediaServices.Models
{
    public class DatabaseSettings
    {
        public int Id { get; set; }
        [NotNull]
        public string Server { get; set; }
        [NotNull]
        public string DataBase { get; set; }
        public string? UserId {get;set;}
        public string? Password { get;set;}
        [NotMapped]
        public string? ConnectionString { get; set;}
    }
}

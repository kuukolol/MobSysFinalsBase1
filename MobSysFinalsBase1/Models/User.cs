using SQLite;
using System;

namespace MobSysFinalsBase1.Models
{
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string PhoneNumber { get; set; }
        public string GroupLabel { get; set; } 
        public string ProfilePicture { get; set; } 
    }
}

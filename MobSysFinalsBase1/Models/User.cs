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
        public string CountryCode { get; set; } 
        public string Email { get; set; }
        public string Nickname { get; set; }
        public string ProfilePicture { get; set; }
        public bool Favourites { get; set; }
    }
}

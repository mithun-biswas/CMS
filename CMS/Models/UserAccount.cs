using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace CMS.Models
{
    public class UserAccount
    {
        
        public int Id { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        
        public string Password { get; set; }
    }
}
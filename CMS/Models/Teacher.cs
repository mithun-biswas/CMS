using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CMS.Models
{
    public class Teacher
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [DisplayName("Short Name")]
        
        public string ShortName { get; set; }
        public string Email { set; get; }
        [DisplayName("Contact No.")]
        public string ContactNo { get; set; }
        public string Designation { get; set; }
        public string Status { get; set; }

        [DisplayName("Number of Course")]
        public int NumberOfCourse { get; set; }
        [NotMapped]
        public int RemainingCourse { get; set; }
        public int UserAccountId { get; set; }
    }
}
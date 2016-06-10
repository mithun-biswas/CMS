using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CMS.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Section { get; set; }
        [DisplayName("Lab Required")]
        public string LabRequired { get; set; }

        [DisplayName("Lab Course")]
        public string IsItaLabCourse { get; set; }
        public string Semester { get; set; }
        [DisplayName("Faculty")]
        public int TeacherId { set; get; }
        [DisplayName("Time")]
        public int TimeId { get; set; }
        public int UserAccountId { set; get; }
    }
}
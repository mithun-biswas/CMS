using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace CMS.Models
{
    public class CourseTeacher
    {
        public int Teacher { set; get; }
        [DisplayName("Number of Course")]
        public int NumberOfCourse { get; set; }
        [DisplayName("Remaining Course")]
        public int RemainingCourse { get; set; }

        [DisplayName("Course Code")]
        public string CourseCode { get; set; }
        [DisplayName("Course Name")]
        public string CourseName { get; set; }
        
        public string Semester { get; set; }

        public int Section { get; set; }
        [DisplayName("Time")]
        public int TimeId { set; get; }
    }
}
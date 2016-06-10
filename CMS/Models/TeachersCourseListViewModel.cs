using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMS.Models
{
    public class TeachersCourseListViewModel
    {
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public int Section { get; set; }
        public string Time { get; set; }

    }
}
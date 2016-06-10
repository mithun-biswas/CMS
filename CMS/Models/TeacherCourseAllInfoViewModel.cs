using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMS.Models
{
    public class TeacherCourseAllInfoViewModel
    {
        public Teacher Teacher { get; set; }
        public List<TeachersCourseListViewModel> CourseList { get; set; }
    }
}
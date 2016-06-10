using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using CMS.Models;
using Microsoft.Ajax.Utilities;


namespace CMS.Controllers
{
    public class CourseController : Controller
    {
        private CourseManagementDbContext db = new CourseManagementDbContext();

        // GET: /Course/

        //public ActionResult RegistrarReport()
        //{
            
        //}
        public ActionResult RegistrarReport()
        {
            if (Session["user_email"] == null)
            {
                return RedirectToAction("Login", "UserAccount");
            }
            int userAccountId = (int) Session["user_id"];     //new append
            string html = null;
            //get all semester
            var semesterList = db.Semesters.OrderBy(x => x.Id).ToList();
            foreach (var semester in semesterList)
            {
                //
                var courselistFristSemester = (from c in db.Courses
                    join t in db.Teachers
                        on c.TeacherId equals t.Id into tGroup
                    from t in tGroup.DefaultIfEmpty()
                    join time in db.Times
                        on c.TimeId equals time.Id into timeGroup
                    from time in timeGroup.DefaultIfEmpty()
                    where c.Semester == semester.Name &&
                    c.UserAccountId == userAccountId           // new append
                    orderby c.Code, c.Section
                    select new
                    {
                        c.Code,
                        c.Name,
                        c.Section,
                        ShortName = t == null ? "" : t.ShortName,
                        Description = time == null ? "" : time.Description
                    }).ToList();
                int numberOfCourseInASemester = courselistFristSemester.Count();
                //string html = "<tr> <td rowspan="+nOfCourse+">First Semester</td></tr>";

                var distinctCourse = courselistFristSemester.Select(x => x.Code).Distinct().ToList(); // x.title

                int numberOfDistinctCourseInASemester = distinctCourse.Count();
                string[] distinctCourseCodeList = new string[numberOfDistinctCourseInASemester + 2];
                int[] numberOfSectionInACourse = new int[numberOfDistinctCourseInASemester + 2];
                int[] rowSpanIndicator = new int[numberOfDistinctCourseInASemester + 2];
                int count = 0;
                foreach (var dCourse in distinctCourse)
                {

                    //string dCourse = distinctCourses;
                    var aCourse = courselistFristSemester.Where(x => x.Code == dCourse).ToList();
                    int numberOfSection = aCourse.Count();
                    distinctCourseCodeList[count] = dCourse;
                    numberOfSectionInACourse[count] = numberOfSection;
                    if (count == 0)
                    {
                        rowSpanIndicator[count] = 0;
                    }
                    else
                    {
                        rowSpanIndicator[count] = rowSpanIndicator[count - 1] + numberOfSectionInACourse[count - 1];
                    }

                    count++;
                }
                int index = 0;
                int rowIndex = 0;
                //string html = null;  // shift top of the loop

                foreach (var course in courselistFristSemester)
                {
                    html += "<tr>";
                    if (index == 0)
                    {
                        html += "<td rowspan=\"" + numberOfCourseInASemester + "\">"+semester.Name;  //Term 1
                    }
                    if (rowSpanIndicator[rowIndex] == index)
                    {
                        html += "<td rowspan=\"" + numberOfSectionInACourse[rowIndex] + "\">";
                        html += distinctCourseCodeList[rowIndex];
                        html += "</td>";

                        html += "<td rowspan=\"" + numberOfSectionInACourse[rowIndex] + "\">";
                        //find the courseTitle by courseCode
                        string courseCodeForCourseTitle = distinctCourseCodeList[rowIndex];
                        var courseTitleList =
                            db.Courses.Where(x => x.Code == courseCodeForCourseTitle).ToList();
                        string courseTitle = null;
                        foreach (var course1 in courseTitleList)
                        {
                            courseTitle = course1.Name;
                        }
                        //End of Find the courseTitle by courseCode

                        html += courseTitle; //distinctCourseCodeList[rowIndex]
                        html += "</td>";
                        rowIndex++;
                    }
                    html += "<td>" + course.Section + "</td>";
                    html += "<td>" + course.ShortName + "</td>";
                    html += "<td>" + course.Description + "</td>";

                    html += "</tr>";
                    index++;

                }
            }

            ViewBag.html = html;
            return View();
        }






        public ActionResult CourseAssignToTeacher()
        {
            if (Session["user_email"] == null)
            {
                return RedirectToAction("Login", "UserAccount");
            }
            int userAccountId = (int) Session["user_id"];     //new append
            ViewBag.TeacherList = db.Teachers.Where(x=>x.UserAccountId == userAccountId).ToList();   //new where append
            var courses = db.Courses.Where(x=>x.UserAccountId==userAccountId).ToList();              //new where append
            var l = new List<string>();
            foreach (var course in courses)
            {
                string code = course.Code;
                l.Add(code);
            }

            ViewBag.CourseList = l.Distinct();
            ViewBag.TimeList = db.Times.ToList();

            return View();
        }

        [HttpPost]
        public ActionResult CourseAssignToTeacher(CourseTeacher courseTeacher)
        {
            if (Session["user_email"] == null)
            {
                return RedirectToAction("Login", "UserAccount");
            }
            int userAccountId = (int)Session["user_id"];     //new append
            ViewBag.TeacherList = db.Teachers.Where(x=>x.UserAccountId==userAccountId).ToList();    //new Where append
            var courses = db.Courses.Where(x=>x.UserAccountId==userAccountId).ToList();            //new where append
            var l = new List<string>();
            foreach (var course in courses)
            {
                string code = course.Code;
                l.Add(code);
            }

            ViewBag.CourseList = l.Distinct();
            ViewBag.TimeList = db.Times.ToList();

            var query = (from c in db.Courses
                where c.Code == courseTeacher.CourseCode &&
                      c.Section == courseTeacher.Section &&
                      c.UserAccountId == userAccountId              //new append
                select c).ToList();
            Course aCourse = new Course();

            foreach (var course in query)
            {
                

                course.TeacherId = courseTeacher.Teacher;
                course.TimeId = courseTeacher.TimeId;
            }

            db.SaveChanges();


            return View();
        }



        public ActionResult Index()
        {
            if (Session["user_email"] == null)
            {
                return RedirectToAction("Login", "UserAccount");
            }
            int userAccountId = (int)Session["user_id"];     //new append
            List<CourseAllInformation> courses = (from c in db.Courses
                              join t in db.Teachers
                                  on c.TeacherId equals t.Id into tGroup
                              from teacherName in tGroup.DefaultIfEmpty()
                              join time in db.Times
                                  on c.TimeId equals time.Id into timeGroup
                              from TimeInfo in timeGroup.DefaultIfEmpty()
                              where c.UserAccountId == userAccountId         //new append
                              orderby c.Code, c.Section
                              select new CourseAllInformation()
                              {
                                  Id = c.Id,
                                  Code = c.Code,
                                  Name = c.Name,
                                  Section =  c.Section,
                                  Semester = c.Semester,
                                  LabRequired = c.LabRequired,
                                  LabCourse = c.IsItaLabCourse,
                                  Faculty = teacherName == null ? String.Empty : teacherName.ShortName,
                                  Time = TimeInfo == null ? String.Empty : TimeInfo.Description
                              }).ToList();

            List<CourseAllInformation> coursesOrderBySemester = new List<CourseAllInformation>();
            var semesters = (from s in db.Semesters
                orderby s.Id
                select s).ToList();
            foreach (var semester in semesters)
            {
                foreach (var aCourse in courses)
                {
                    if (semester.Name == aCourse.Semester)
                    {
                        coursesOrderBySemester.Add(aCourse);
                    }
                }
            }
            return View(coursesOrderBySemester);
        }

        // GET: /Course/Details/5
        
        // GET: /Course/Create
        public ActionResult Create()
        {
            if (Session["user_email"] == null)
            {
                return RedirectToAction("Login", "UserAccount");
            }

            ViewBag.SemesterList = db.Semesters.ToList();
            ViewBag.TimeList = db.Times.ToList();
            //ViewBag.TeacherList = db.Teachers.ToList();     //comment out
            ViewBag.YesNoList = new[]
            {
                new {Name = "No"},
                new {Name = "Yes"}
            };
            return View();
        }

        // POST: /Course/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "Id,Code,Name,Section,LabRequired,IsItaLabCourse,Semester,TeacherId,TimeId")] Course course)
        {
            if (Session["user_email"] == null)
            {
                return RedirectToAction("Login", "UserAccount");
            }
            course.UserAccountId = (int) Session["user_id"];

            ViewBag.SemesterList = db.Semesters.ToList();
            ViewBag.TimeList = db.Times.ToList();
            //ViewBag.TeacherList = db.Teachers.ToList();       //comment out
            ViewBag.YesNoList = new[]
            {
                new {Name = "No"},
                new {Name = "Yes"}
            };
            if (ModelState.IsValid)
            {
                db.Courses.Add(course);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(course);
        }

        // GET: /Course/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["user_email"] == null)
            {
                return RedirectToAction("Login", "UserAccount");
            }

            ViewBag.SemesterList = db.Semesters.ToList();
            ViewBag.TimeList = db.Times.ToList();
            //ViewBag.TeacherList = db.Teachers.ToList();         //comment out
            ViewBag.YesNoList = new[]
            {
                new {Name = "No"},
                new {Name = "Yes"}
            };

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Course course = db.Courses.Find(id);
            if (course == null)
            {
                return HttpNotFound();
            }
            return View(course);
        }

        // POST: /Course/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "Id,Code,Name,Section,LabRequired,IsItaLabCourse,Semester,TeacherId,TimeId")] Course course)
        {
            if (Session["user_email"] == null)
            {
                return RedirectToAction("Login", "UserAccount");
            }

            var courseTeacherTime = (from c in db.Courses
                where c.Id == course.Id
                select c).ToList();
            foreach (var aCourseTeacherTime in courseTeacherTime)
            {
                course.TeacherId = aCourseTeacherTime.TeacherId;
                course.TimeId = aCourseTeacherTime.TimeId;
            }
            if (ModelState.IsValid)
            {
                //db.Entry(course).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(course);
        }

        // GET: /Course/Delete/5
        public ActionResult Delete(int? id)
        {
            if (Session["user_email"] == null)
            {
                return RedirectToAction("Login", "UserAccount");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Course course = db.Courses.Find(id);
            if (course == null)
            {
                return HttpNotFound();
            }
            return View(course);
        }

        // POST: /Course/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (Session["user_email"] == null)
            {
                return RedirectToAction("Login", "UserAccount");
            }

            Course course = db.Courses.Find(id);
            db.Courses.Remove(course);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
           {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public JsonResult IsCourseIsAlreadySavedThenAppendTheCourseName(string code)
        {
            int userAccountId = (int) Session["user_id"];     //new append
            var course = (from c in db.Courses
                where c.Code == code &&
                c.UserAccountId == userAccountId              //new append
                select c).ToList();
            
            return Json(course, JsonRequestBehavior.AllowGet);
        }

        public JsonResult IsSectionValid(int section, string courseCode)
        {
            int userAccountId = (int)Session["user_id"];     //new append
            var query = (from c in db.Courses
                where c.Code == courseCode &&
                      c.Section == section &&
                      c.UserAccountId == userAccountId       //new append
                select c).ToList();
            int count = query.Count();

            if (count > 0)
            {
                string errorMessage = "Section "+section + " is already given to " + courseCode
                    +". Please try different section number";
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }
            return Json(true, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetNumberOfCourseAndReamingCourseByTeacherId(int teacherId)
        {
            int userAccountId = (int) Session["user_id"];     //new append
            var teacher = db.Teachers.Where(a => a.Id == teacherId && a.UserAccountId == userAccountId).ToList();                           //userAccountId added to where
            var course = db.Courses.Where(a => a.TeacherId == teacherId && a.IsItaLabCourse=="No" && a.UserAccountId == userAccountId).ToList(); //userAccount added to where   

            int numberofCourse = 0;
            int reaminingCourse = 0;

            foreach (var person in teacher)
            {
                numberofCourse = person.NumberOfCourse;
                reaminingCourse = person.NumberOfCourse - course.Count();
            }

            var teacherCourseInfo = new[]
            {
                new {NumberOfCourse = numberofCourse, ReaminingCourse = reaminingCourse}
            };

            return Json(teacherCourseInfo);
        }

        public JsonResult GetCourseNameSemesterSectionbyCourseCode(string courseCode)
        {
            int userAccountId = (int)Session["user_id"];     //new append
            var courseList = (from c in db.Courses
                where c.Code == courseCode &&
                c.UserAccountId == userAccountId            //new append
                select new
                {
                    CourseName = c.Name,
                    c.Semester,
                    c.Section
                }).ToList();
            return Json(courseList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CheckIfCourseIsAlreadyAssigned(string courseCode, int section)
        {
            int userAccountId = (int) Session["user_id"];     //new append
            string teacherName = "";
            int teacherId = 0;
            var course = (from c in db.Courses
                where c.Code == courseCode &&
                      c.Section == section &&
                      c.TeacherId != 0  &&
                      c.UserAccountId == userAccountId        //new append  
                 select c);

            int count = course.Count();
            foreach (var courseInfo in course)
            {
                teacherId = courseInfo.TeacherId;
            }

            var teacher = (from t in db.Teachers
                where t.Id == teacherId &&
                t.UserAccountId == userAccountId             //new append
                select t).ToList();
            foreach (var teacherInfo in teacher)
            {
                teacherName = teacherInfo.Name;
            }


            return Json(teacherName, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CheckIfAnotherSectionIsAssignedInSameTime(string courseCode, int section, int time)
        {
            int userAccountId = (int)Session["user_id"];     //new append

            var courseList = (from c in db.Courses
                where c.Code == courseCode &&
                      c.Section != section &&
                      c.TimeId == time &&
                      c.UserAccountId == userAccountId
                select c).ToList();
            return Json(courseList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CheckIfConsecutiveCoursesForSameTeacher(int teacherId, int timeId, string code, int section)
        {
            int userAccountId = (int) Session["user_id"];     //new append
            string report = "";
            string IsItALabCourse = "";
            var courseInfo = (from c in db.Courses
                where c.Code == code &&
                      c.Section == section &&
                      c.UserAccountId == userAccountId      //new append

                select c).ToList();

            foreach (var course in courseInfo)
            {
                IsItALabCourse = course.IsItaLabCourse;
            }

            if (IsItALabCourse == "Yes")
            {
                report = "";
            }
            else
            {
                List<Course> courseList = new List<Course>();
                if (timeId == 6)
                {
                    courseList = (from c in db.Courses
                                  where c.TeacherId == teacherId &&
                                        c.TimeId == (timeId - 1) &&
                                        c.IsItaLabCourse == "No" &&
                                        c.UserAccountId == userAccountId      //new append
                                  select c).ToList();
                }
                else if (timeId == 7)
                {
                    courseList = (from c in db.Courses
                                  where c.TeacherId == teacherId &&
                                        c.TimeId == (timeId + 1) &&
                                        c.IsItaLabCourse == "No" &&
                                        c.UserAccountId == userAccountId      //new append
                                  select c).ToList();
                }

                else
                {
                    courseList = (from c in db.Courses
                                  where c.TeacherId == teacherId &&
                                        ((c.TimeId == (timeId - 1)) || c.TimeId == (timeId + 1)) &&
                                        c.IsItaLabCourse == "No" &&
                                        c.UserAccountId == userAccountId       //new append
                                  select c).ToList();
                }

                int count = courseList.Count();
                if (count > 0)
                {
                    report = "Error";
                }

    
            }
            
            return Json(report, JsonRequestBehavior.AllowGet);
        }


        public JsonResult CheckIfTwoCourseInSameTimeForSelectedTeacher(int teacherId, int timeId)
        {
            string teacherName = "";
            string courseName = "";
            int courseSection = 0;
            string message = "";

            int userAccountId = (int)Session["user_id"];     //new append

            var courseInSameTime = (from c in db.Courses
                where c.TeacherId == teacherId &&
                      c.TimeId == timeId &&
                      c.UserAccountId == userAccountId      //new append
                select c).ToList();

            int count = courseInSameTime.Count();
            if (count > 0)
            {
                var teacherInfo = (from t in db.Teachers
                    where t.Id == teacherId &&
                    t.UserAccountId == userAccountId        //new append
                    select t).ToList();
                foreach (var teacher in teacherInfo)
                {
                    teacherName = teacher.Name;
                }
                foreach (var course in courseInSameTime)
                {
                    courseName = course.Name;
                    courseSection = course.Section;
                }

                message = teacherName + " have a course named " + courseName + "(" + courseSection +
                          ") in the same time. ";
            }
            return Json(message, JsonRequestBehavior.AllowGet);
        }

    }

}

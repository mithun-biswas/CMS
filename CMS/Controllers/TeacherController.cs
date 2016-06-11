using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using CMS.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Ajax.Utilities;
using OfficeOpenXml;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Mail;

namespace CMS.Controllers
{
    public class TeacherController : Controller
    {
        private CourseManagementDbContext db = new CourseManagementDbContext();

        // GET: /Teacher/

        public ActionResult FullTimeFacultyCourseLoad()
        {
            if (Session["user_email"] == null)
            {
                return RedirectToAction("Login", "UserAccount");
            }

            int userAccountId = (int) Session["user_id"];     //new append
            var teacherStatistics = (from t in db.Teachers
                join c in db.Courses
                    on t.Id equals c.TeacherId into cGroup
                    where t.Status == "Full Time" &&
                    t.UserAccountId == userAccountId        //new append
                orderby t.Designation descending  
                select new
                {
                    TeacherInfo = t,
                    CourseInfo = from cg in cGroup
                                 where cg.UserAccountId == userAccountId      //new append
                                 orderby cg.Code ascending 
                                 select cg
                }).ToList();
            
            List<TeacherStatistics> teacherStatisticses = new List<TeacherStatistics>();
            int count=0;
            foreach (var teacherStatistic in teacherStatistics)
            {
                TeacherStatistics aTeacherStatistics = new TeacherStatistics();
                aTeacherStatistics.Name = teacherStatistic.TeacherInfo.Name;
                aTeacherStatistics.Designation = teacherStatistic.TeacherInfo.Designation;
                aTeacherStatistics.NumberOfCourse = teacherStatistic.TeacherInfo.NumberOfCourse;
                count = 0;
                foreach (var courseInfo in teacherStatistic.CourseInfo)
                {
                    if (count != 0)
                    {
                        aTeacherStatistics.Courses += ", ";
                    }

                    aTeacherStatistics.Courses += courseInfo.Code;
                    aTeacherStatistics.Courses += "(";
                    aTeacherStatistics.Courses += courseInfo.Section;
                    aTeacherStatistics.Courses += ")";
                    count++;
                } 

                teacherStatisticses.Add(aTeacherStatistics);
            }


            List<TeacherStatistics> teacherStatisticsesWithDesignationOrder = new List<TeacherStatistics>();
            List<Designation> designations = db.Designations.OrderBy(x => x.Id).ToList();
            foreach (Designation designation in designations)
            {
                foreach (var aTeacher in teacherStatisticses)
                {
                    if (designation.Name == aTeacher.Designation)
                    {
                        teacherStatisticsesWithDesignationOrder.Add(aTeacher);
                    }
                }
            }

            return View(teacherStatisticsesWithDesignationOrder);
        }

        public ActionResult FullTimeFacultyCourseLoadReport()
        {
            var document = new Document(PageSize.A2, 100, 100, 50, 50);
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(document, output);
            document.Open();


            var titleFont = FontFactory.GetFont("Arial", 18, Font.BOLD);
            var subTitleFont = FontFactory.GetFont("Arial", 14, Font.BOLD);
            var boldTableFont = FontFactory.GetFont("Arial", 12, Font.BOLD);
            var endingMessageFont = FontFactory.GetFont("Arial", 10, Font.ITALIC);
            var bodyFont = FontFactory.GetFont("Arial", 12, Font.NORMAL);

            document.Add(new Paragraph("Full Time Faculty Course Load ", titleFont));
            document.Add(new Paragraph("Fall 2016 ", subTitleFont));
            document.Add(new Paragraph("Department of Computer Science and Engineering ", subTitleFont));

            document.Add(Chunk.NEWLINE);


            var teacherTable = new PdfPTable(4);
            teacherTable.WidthPercentage = 100F; 
            //teacherTable = new PdfPTable(PageSize.A4, 100, 100, 50, 50);
            //teacherTable.HorizontalAlignment = 0;
            teacherTable.SpacingBefore = 0;
            teacherTable.SpacingAfter = 0;
            //teacherTable.DefaultCell.Border = 1;
            //teacherTable.TotalWidth = 9f;
            int[] widths = new int[] { 20, 22, 10, 50 };
            teacherTable.SetWidths(widths);

            teacherTable.AddCell(new Phrase("Name", boldTableFont));
            teacherTable.AddCell(new Phrase("Designation", boldTableFont));
            teacherTable.AddCell(new Phrase("No of Co", boldTableFont));
            teacherTable.AddCell(new Phrase("Courses", boldTableFont));

            var teacherStatistics = (from t in db.Teachers
                                     join c in db.Courses
                                         on t.Id equals c.TeacherId into cGroup
                                     where t.Status == "Full Time"
                                     orderby t.Designation descending
                                     select new
                                     {
                                         TeacherInfo = t,
                                         CourseInfo = from cg in cGroup
                                                      orderby cg.Code ascending
                                                      select cg
                                     }).ToList();

            List<TeacherStatistics> teacherStatisticses = new List<TeacherStatistics>();
            int count = 0;
            foreach (var teacherStatistic in teacherStatistics)
            {
                TeacherStatistics aTeacherStatistics = new TeacherStatistics();
                aTeacherStatistics.Name = teacherStatistic.TeacherInfo.Name;
                aTeacherStatistics.Designation = teacherStatistic.TeacherInfo.Designation;
                aTeacherStatistics.NumberOfCourse = teacherStatistic.TeacherInfo.NumberOfCourse;
                count = 0;
                foreach (var courseInfo in teacherStatistic.CourseInfo)
                {
                    if (count != 0)
                    {
                        aTeacherStatistics.Courses += ", ";
                    }

                    aTeacherStatistics.Courses += courseInfo.Code;
                    aTeacherStatistics.Courses += "(";
                    aTeacherStatistics.Courses += courseInfo.Section;
                    aTeacherStatistics.Courses += ")";
                    count++;
                }
                teacherTable.AddCell(aTeacherStatistics.Name);
                teacherTable.AddCell(aTeacherStatistics.Designation);
                teacherTable.AddCell(aTeacherStatistics.NumberOfCourse.ToString());
                teacherTable.AddCell(aTeacherStatistics.Courses);


                teacherStatisticses.Add(aTeacherStatistics);
            }
            document.Add(teacherTable);
            document.Close();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;  filename=FullTimeFaculty.pdf");

            Response.BinaryWrite(output.ToArray());

            ////****///////////////////////////////////


            MemoryStream s = new MemoryStream(output.ToArray());
            s.Seek(0, SeekOrigin.Begin);
            Attachment a = new Attachment(s, "FullTimeFaculty.pdf");
            MailMessage message = new MailMessage("nuha.khan4@gmail.com", "sumaiya.tasmim.cse@ulab.edu.bd",
   "A report for you!", "Here is a report for you");
            message.Attachments.Add(a);
            SmtpClient client = new SmtpClient();
            client.Send(message);
            ////****///////////////////////////////////
            return View(teacherStatisticses);
        }
        public ActionResult SendEmailPDF()
        {
            var document = new Document(PageSize.A3, 100, 100, 50, 50);
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(document, output);
            document.Open();


            var titleFont = FontFactory.GetFont("Arial", 18, Font.BOLD);
            var subTitleFont = FontFactory.GetFont("Arial", 14, Font.BOLD);
            var boldTableFont = FontFactory.GetFont("Arial", 12, Font.BOLD);
            var endingMessageFont = FontFactory.GetFont("Arial", 10, Font.ITALIC);
            var bodyFont = FontFactory.GetFont("Arial", 8, Font.NORMAL);

            document.Add(new Paragraph("Full Time Faculty Course Load ", titleFont));
            document.Add(new Paragraph("Fall 2016 ", subTitleFont));
            document.Add(new Paragraph("Department of Computer Science and Engineering ", subTitleFont));

            document.Add(Chunk.NEWLINE);


            var teacherTable = new PdfPTable(4);
            teacherTable.WidthPercentage = 100F; 
            //teacherTable.HorizontalAlignment = 0;
            //teacherTable.SpacingBefore = 10;
            //teacherTable.SpacingAfter = 10;
            //teacherTable.DefaultCell.Border = 1;
            //teacherTable.TotalWidth = 9f;
            int[] widths = new int[] { 20, 22, 10, 50 };
            teacherTable.SetWidths(widths);

            teacherTable.AddCell(new Phrase("Name", boldTableFont));
            teacherTable.AddCell(new Phrase("Designation", boldTableFont));
            teacherTable.AddCell(new Phrase("No of Co", boldTableFont));
            teacherTable.AddCell(new Phrase("Courses", boldTableFont));

            var teacherStatistics = (from t in db.Teachers
                                     join c in db.Courses
                                         on t.Id equals c.TeacherId into cGroup
                                     where t.Status == "Full Time"
                                     orderby t.Designation descending
                                     select new
                                     {
                                         TeacherInfo = t,
                                         CourseInfo = from cg in cGroup
                                                      orderby cg.Code ascending
                                                      select cg
                                     }).ToList();

            List<TeacherStatistics> teacherStatisticses = new List<TeacherStatistics>();
            int count = 0;
            foreach (var teacherStatistic in teacherStatistics)
            {
                TeacherStatistics aTeacherStatistics = new TeacherStatistics();
                aTeacherStatistics.Name = teacherStatistic.TeacherInfo.Name;
                aTeacherStatistics.Designation = teacherStatistic.TeacherInfo.Designation;
                aTeacherStatistics.NumberOfCourse = teacherStatistic.TeacherInfo.NumberOfCourse;
                count = 0;
                foreach (var courseInfo in teacherStatistic.CourseInfo)
                {
                    if (count != 0)
                    {
                        aTeacherStatistics.Courses += ", ";
                    }

                    aTeacherStatistics.Courses += courseInfo.Code;
                    aTeacherStatistics.Courses += "(";
                    aTeacherStatistics.Courses += courseInfo.Section;
                    aTeacherStatistics.Courses += ")";
                    count++;
                }
                teacherTable.AddCell(aTeacherStatistics.Name);
                teacherTable.AddCell(aTeacherStatistics.Designation);
                teacherTable.AddCell(aTeacherStatistics.NumberOfCourse.ToString());
                teacherTable.AddCell(aTeacherStatistics.Courses);


                teacherStatisticses.Add(aTeacherStatistics);
            }
            document.Add(teacherTable);
            document.Close();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;  filename=FullTimeFaculty.pdf");

            Response.BinaryWrite(output.ToArray());

            ////****///////////////////////////////////


            MemoryStream s = new MemoryStream(output.ToArray());
            s.Seek(0, SeekOrigin.Begin);
            Attachment a = new Attachment(s, "FullTimeFaculty.pdf");
            MailMessage message = new MailMessage("nuha.khan4@gmail.com", "sumaiya.tasmim.cse@ulab.edu.bd",
   "A report for you!", "Here is a report for you");
            message.Attachments.Add(a);
            SmtpClient client = new SmtpClient();
            client.Send(message);
            ////****///////////////////////////////////
            return View(teacherStatisticses);
        }
        

        public ActionResult FullTimeFacultyCourseLoadReportExcel()
        {

            var teacherStatistics = (from t in db.Teachers
                                     join c in db.Courses
                                         on t.Id equals c.TeacherId into cGroup
                                     where t.Status == "Full Time"
                                     orderby t.Designation descending
                                     select new
                                     {
                                         TeacherInfo = t,
                                         CourseInfo = from cg in cGroup
                                                      orderby cg.Code ascending
                                                      select cg
                                     }).ToList();


            List<TeacherStatistics> teacherStatisticses = new List<TeacherStatistics>();
            int count = 0;
            foreach (var teacherStatistic in teacherStatistics)
            {
                TeacherStatistics aTeacherStatistics = new TeacherStatistics();
                aTeacherStatistics.Name = teacherStatistic.TeacherInfo.Name;
                aTeacherStatistics.Designation = teacherStatistic.TeacherInfo.Designation;
                aTeacherStatistics.NumberOfCourse = teacherStatistic.TeacherInfo.NumberOfCourse;
                count = 0;
                foreach (var courseInfo in teacherStatistic.CourseInfo)
                {
                    if (count != 0)
                    {
                        aTeacherStatistics.Courses += ", ";
                    }

                    aTeacherStatistics.Courses += courseInfo.Code;
                    aTeacherStatistics.Courses += "(";
                    aTeacherStatistics.Courses += courseInfo.Section;
                    aTeacherStatistics.Courses += ")";
                    count++;
                }
                teacherStatisticses.Add(aTeacherStatistics);
            }

            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            workSheet.Cells[1, 1].LoadFromCollection(teacherStatisticses, true);
            using (var memoryStream = new MemoryStream())
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;  filename=teacher.xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }

            return View(teacherStatisticses);
        }


        public ActionResult PartTimeFacultyCourseLoad()
        {
            if (Session["user_email"] == null)
            {
                return RedirectToAction("Login", "UserAccount");
            }

            int userAccountId = (int)Session["user_id"];     //new append
            var teacherStatistics = (from t in db.Teachers
                                     join c in db.Courses
                                         on t.Id equals c.TeacherId into cGroup
                                     where t.Status == "Part Time" &&
                                     t.UserAccountId == userAccountId      //new append
                                     orderby t.Designation descending
                                     select new
                                     {
                                         TeacherInfo = t,
                                         CourseInfo = from cg in cGroup
                                                      where cg.UserAccountId == userAccountId    //new append
                                                      orderby cg.Code ascending
                                                      select cg
                                     }).ToList();
            List<TeacherStatistics> teacherStatisticses = new List<TeacherStatistics>();
            int count = 0;
            foreach (var teacherStatistic in teacherStatistics)
            {
                TeacherStatistics aTeacherStatistics = new TeacherStatistics();
                aTeacherStatistics.Name = teacherStatistic.TeacherInfo.Name;
                aTeacherStatistics.Designation = teacherStatistic.TeacherInfo.Designation;
                aTeacherStatistics.NumberOfCourse = teacherStatistic.TeacherInfo.NumberOfCourse;
                count = 0;
                foreach (var courseInfo in teacherStatistic.CourseInfo)
                {
                    if (count != 0)
                    {
                        aTeacherStatistics.Courses += ", ";
                    }

                    aTeacherStatistics.Courses += courseInfo.Code;
                    aTeacherStatistics.Courses += "(";
                    aTeacherStatistics.Courses += courseInfo.Section;
                    aTeacherStatistics.Courses += ")";
                    count++;
                }

                teacherStatisticses.Add(aTeacherStatistics);
            }

            List<TeacherStatistics> teacherStatisticsesWithDesignationOrder = new List<TeacherStatistics>();
            List<Designation> designations = db.Designations.OrderBy(x => x.Id).ToList();
            foreach (Designation designation in designations)
            {
                foreach (var aTeacher in teacherStatisticses)
                {
                    if (designation.Name == aTeacher.Designation)
                    {
                        teacherStatisticsesWithDesignationOrder.Add(aTeacher);
                    }
                }
            }

            return View(teacherStatisticsesWithDesignationOrder);
        }


        public ActionResult PartTimeFacultyCourseLoadReport()
        {
            var document = new Document(PageSize.A4, 50, 50, 25, 25);
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(document, output);
            document.Open();


            var titleFont = FontFactory.GetFont("Arial", 18, Font.BOLD);
            var subTitleFont = FontFactory.GetFont("Arial", 14, Font.BOLD);
            var boldTableFont = FontFactory.GetFont("Arial", 12, Font.BOLD);
            var endingMessageFont = FontFactory.GetFont("Arial", 10, Font.ITALIC);
            var bodyFont = FontFactory.GetFont("Arial", 12, Font.NORMAL);
          

            document.Add(new Paragraph("Part Time Faculty Course Load ", titleFont));
            document.Add(new Paragraph("Fall 2016", bodyFont));
            document.Add(new Paragraph("Department of Computer Science and Engineering ", bodyFont));

            document.Add(Chunk.NEWLINE);


            var teacherTable = new PdfPTable(4);
            //teacherTable.HorizontalAlignment = 0;
            teacherTable.SpacingBefore = 10;
            teacherTable.SpacingAfter = 10;
            //teacherTable.DefaultCell.Border = 1;
            teacherTable.TotalWidth = 9f;

            int[] widths = new int[] { 20, 22, 10, 35 };
            teacherTable.SetWidths(widths);


            teacherTable.AddCell(new Phrase("Name", boldTableFont));
            teacherTable.AddCell(new Phrase("Designation", boldTableFont));
            teacherTable.AddCell(new Phrase("No. of Co", boldTableFont));
            teacherTable.AddCell(new Phrase("Courses", boldTableFont));


            var teacherStatistics = (from t in db.Teachers
                                     join c in db.Courses
                                         on t.Id equals c.TeacherId into cGroup
                                     where t.Status == "Part Time"
                                     orderby t.Designation descending
                                     select new
                                     {
                                         TeacherInfo = t,
                                         CourseInfo = from cg in cGroup
                                                      orderby cg.Code ascending
                                                      select cg
                                     }).ToList();

            List<TeacherStatistics> teacherStatisticses = new List<TeacherStatistics>();

            int count = 0;
            foreach (var teacherStatistic in teacherStatistics)
            {
                TeacherStatistics aTeacherStatistics = new TeacherStatistics();
                aTeacherStatistics.Name = teacherStatistic.TeacherInfo.Name;
                aTeacherStatistics.Designation = teacherStatistic.TeacherInfo.Designation;
                aTeacherStatistics.NumberOfCourse = teacherStatistic.TeacherInfo.NumberOfCourse;

                count = 0;
                foreach (var courseInfo in teacherStatistic.CourseInfo)
                {
                    if (count != 0)
                    {
                        aTeacherStatistics.Courses += ", ";
                    }

                    aTeacherStatistics.Courses += courseInfo.Code;
                    aTeacherStatistics.Courses += "(";
                    aTeacherStatistics.Courses += courseInfo.Section;
                    aTeacherStatistics.Courses += ")";
                    count++;
                }

                teacherTable.AddCell(aTeacherStatistics.Name);
                teacherTable.AddCell(aTeacherStatistics.Designation);
                teacherTable.AddCell(aTeacherStatistics.NumberOfCourse.ToString());
                teacherTable.AddCell(aTeacherStatistics.Courses);

                teacherStatisticses.Add(aTeacherStatistics);
            }

            document.Add(teacherTable);
            document.Close();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;  filename=PartTimeFaculty.pdf");
            Response.BinaryWrite(output.ToArray());

            return View(teacherStatisticses);

        }
        public ActionResult PartTimeFacultyCourseLoadReportExcel()
        {
            var teacherStatistics = (from t in db.Teachers
                                     join c in db.Courses
                                         on t.Id equals c.TeacherId into cGroup
                                     where t.Status == "Part Time"
                                     orderby t.Designation descending
                                     select new
                                     {
                                         TeacherInfo = t,
                                         CourseInfo = from cg in cGroup
                                                      orderby cg.Code ascending
                                                      select cg
                                     }).ToList();

            List<TeacherStatistics> teacherStatisticses = new List<TeacherStatistics>();
            int count = 0;
            foreach (var teacherStatistic in teacherStatistics)
            {
                TeacherStatistics aTeacherStatistics = new TeacherStatistics();
                aTeacherStatistics.Name = teacherStatistic.TeacherInfo.Name;
                aTeacherStatistics.Designation = teacherStatistic.TeacherInfo.Designation;
                aTeacherStatistics.NumberOfCourse = teacherStatistic.TeacherInfo.NumberOfCourse;
                count = 0;
                foreach (var courseInfo in teacherStatistic.CourseInfo)
                {
                    if (count != 0)
                    {
                        aTeacherStatistics.Courses += ", ";
                    }

                    aTeacherStatistics.Courses += courseInfo.Code;
                    aTeacherStatistics.Courses += "(";
                    aTeacherStatistics.Courses += courseInfo.Section;
                    aTeacherStatistics.Courses += ")";
                    count++;
                }

                teacherStatisticses.Add(aTeacherStatistics);
            }

            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            workSheet.Cells[1, 1].LoadFromCollection(teacherStatisticses, true);
            using (var memoryStream = new MemoryStream())
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;  filename=teacher.xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }

            return View(teacherStatisticses);
        }

       
        public ActionResult Index()
        {
            if (Session["user_email"] == null)
            {
                return RedirectToAction("Login", "UserAccount");
            }

            List<Teacher> teachers = new List<Teacher>();
            int userAccountId = (int) Session["user_id"];     //new append
            var teacherList = db.Teachers.Where(x=>x.UserAccountId == userAccountId).OrderBy(x=>x.Name).ToList();  //where clause added
            List<Designation> designations = db.Designations.OrderBy(x=>x.Id).ToList();
            foreach (Designation designation in designations)
            {
                foreach (var aTeacher in teacherList)
                {
                    if (designation.Name == aTeacher.Designation)
                    {
                        teachers.Add(aTeacher);
                    }
                }
            }
            return View(teachers);
        }

        // GET: /Teacher/Details/5
        public ActionResult Details(int? id)
        {
            if (Session["user_email"] == null)
            {
                return RedirectToAction("Login", "UserAccount");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Teacher teacher = db.Teachers.Find(id);
            if (teacher == null)
            {
                return HttpNotFound();
            }
            int userAccountId = (int)Session["user_id"];     //new append

            List<TeachersCourseListViewModel> coursesOfATeacher = (from c in db.Courses
                                                                   join t in db.Times
                                                                   on c.TimeId equals t.Id
                                                                   where c.TeacherId == id &&
            c.UserAccountId == userAccountId    //new append
            orderby c.Code
            select new TeachersCourseListViewModel()
            {
                CourseName = c.Name,
                CourseCode = c.Code,
                Section = c.Section,
                Time = t.Description

            }).ToList();
            TeacherCourseAllInfoViewModel teacherCourse = new TeacherCourseAllInfoViewModel();
            teacherCourse.CourseList = coursesOfATeacher;
            teacherCourse.Teacher = teacher;
            return View(teacherCourse);
        }


        public ActionResult TeacherDetailsPDF(int? id)
        {
            if (Session["user_email"] == null)
            {
                return RedirectToAction("Login", "UserAccount");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Teacher teacher = db.Teachers.Find(id);
            if (teacher == null)
            {
                return HttpNotFound();
            }
            int userAccountId = (int)Session["user_id"];     //new append

            var document = new Document(PageSize.A3, 100, 100, 50, 50);
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(document, output);
            document.Open();


            var titleFont = FontFactory.GetFont("Arial", 18, Font.BOLD);
            var subTitleFont = FontFactory.GetFont("Arial", 14, Font.BOLD);
            var boldTableFont = FontFactory.GetFont("Arial", 12, Font.BOLD);
            var endingMessageFont = FontFactory.GetFont("Arial", 10, Font.ITALIC);
            var bodyFont = FontFactory.GetFont("Arial", 8, Font.NORMAL);

            document.Add(new Paragraph("Your Course Details", titleFont));
            document.Add(new Paragraph("Fall 2016 ", subTitleFont)); // akhane string pass korte hobe
            document.Add(new Paragraph("Department of Computer Science and Engineering ", subTitleFont));

            document.Add(Chunk.NEWLINE);

            var teacherTable = new PdfPTable(4);
            teacherTable.WidthPercentage = 100F;
            //teacherTable.HorizontalAlignment = 0;
            //teacherTable.SpacingBefore = 10;
            //teacherTable.SpacingAfter = 10;
            //teacherTable.DefaultCell.Border = 1;
            //teacherTable.TotalWidth = 9f;
            int[] widths = new int[] { 20, 50, 20, 30};
            teacherTable.SetWidths(widths);

            teacherTable.AddCell(new Phrase("Course Code", boldTableFont));
            teacherTable.AddCell(new Phrase("Course Name", boldTableFont));
            teacherTable.AddCell(new Phrase("Section", boldTableFont));
            teacherTable.AddCell(new Phrase("Time", boldTableFont));


            List<TeachersCourseListViewModel> coursesOfATeacher = (from c in db.Courses
                                                                   join t in db.Times
                                                                   on c.TimeId equals t.Id
                                                                   where c.TeacherId == id &&
                                                                   c.UserAccountId == userAccountId    //new append
                                                                   orderby c.Code
                                                                   select new TeachersCourseListViewModel()
                                                                   {
                                                                       CourseName = c.Name,
                                                                       CourseCode = c.Code,
                                                                       Section = c.Section,
                                                                       Time = t.Description
                                                                   }).ToList();

            var emailTo = (from t in db.Teachers where id == t.Id
            select new {
               t.Email
            }).ToList();

            string EmailToTeacher = " " ;
            foreach(var aEmail in emailTo)
            {
                EmailToTeacher = aEmail.Email;
            }
            foreach (var aTeacher in coursesOfATeacher)
            {
                teacherTable.AddCell(aTeacher.CourseCode);
                teacherTable.AddCell(aTeacher.CourseName);
                teacherTable.AddCell(aTeacher.Section.ToString());
                teacherTable.AddCell(aTeacher.Time);
            }
            document.Add(teacherTable);
            document.Close();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;  filename=CourseDetailsOfTeacher.pdf");
            Response.BinaryWrite(output.ToArray());
            MemoryStream s = new MemoryStream(output.ToArray());
            s.Seek(0, SeekOrigin.Begin);
            Attachment a = new Attachment(s, "FullTimeFaculty.pdf");

            MailMessage message = new MailMessage("nuha.khan4@gmail.com", EmailToTeacher,
   "Report - Your Course Details!", "Here is a report of your course Details");
            message.Attachments.Add(a);
            SmtpClient client = new SmtpClient();
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential
            ("nuha.khan4@gmail.com", "Fui Tui 9*");// Enter seders User name and password  
            client.EnableSsl = true;            
            client.Send(message);

            TeacherCourseAllInfoViewModel teacherCourse = new TeacherCourseAllInfoViewModel();
            teacherCourse.CourseList = coursesOfATeacher;
            teacherCourse.Teacher = teacher;
            return View(teacherCourse);
        }

        // GET: /Teacher/Create
        public ActionResult Create()
        {
            if (Session["user_email"] == null)
            {
                return RedirectToAction("Login", "UserAccount");
            }

            
            ViewBag.DesignationList = db.Designations.ToList();
            ViewBag.StatusList = new[]
            {
                new{Name="Full Time"},
                new{Name="Part Time"}
            };
            return View();
        }

        // POST: /Teacher/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="Id,Name,ShortName,Email,ContactNo,Designation,Status,NumberOfCourse")] Teacher teacher)
        {
            if (Session["user_email"] == null)
            {
                return RedirectToAction("Login", "UserAccount");
            }
            teacher.UserAccountId = (int) Session["user_id"];

            ViewBag.DesignationList = db.Designations.ToList();
            ViewBag.StatusList = new[]
            {
                new{Name="Full Time"},
                new{Name="Part Time"}
            };
            if (ModelState.IsValid)
            { 
                db.Teachers.Add(teacher);
                db.SaveChanges();
                ViewBag.message = "Saved Successfully";
                return View();
            }

            return View(teacher);
        }

        // GET: /Teacher/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["user_email"] == null)
            {
                return RedirectToAction("Login", "UserAccount");
            }

            ViewBag.DesignationList = db.Designations.ToList();
            ViewBag.StatusList = new[]
            {
                new{Name="Full Time"},
                new{Name="Part Time"}
            };

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Teacher teacher = db.Teachers.Find(id);
            if (teacher == null)
            {
                return HttpNotFound();
            }
            return View(teacher);
        }

        // POST: /Teacher/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="Id,Name,ShortName,Email,ContactNo,Designation,NumberOfCourse")] Teacher teacher)
        {
            if (Session["user_email"] == null)
            {
                return RedirectToAction("Login", "UserAccount");
            }

            ViewBag.DesignationList = db.Designations.ToList();
            ViewBag.StatusList = new[]
            {
                new{Name="Full Time"},
                new{Name="Part Time"}
            };

            if (ModelState.IsValid)
            {
                db.Entry(teacher).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(teacher);
        }

        // GET: /Teacher/Delete/5
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
            Teacher teacher = db.Teachers.Find(id);
            if (teacher == null)
            {
                return HttpNotFound();
            }
            return View(teacher);
        }

        // POST: /Teacher/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (Session["user_email"] == null)
            {
                return RedirectToAction("Login", "UserAccount");
            }
            Teacher teacher = db.Teachers.Find(id);
            db.Teachers.Remove(teacher);
            var courseListById = db.Courses.Where(x => x.TeacherId == id);
            foreach (var aCourse in courseListById)
            {
                aCourse.TeacherId = 0;
                aCourse.TimeId = 0;
            }
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

        public JsonResult IsShortNameExist(string ShortName)
        {
            int userAccountId = (int)Session["user_id"];     //new append
            var teacher = db.Teachers.Where(t => t.ShortName.Contains(ShortName) && t.UserAccountId==userAccountId).ToList();  //append userAccountId check in Where
            //var teacherAll = db.Teachers.ToList();

            foreach (var ateacher in teacher)                           
            {
                if (ateacher.ShortName.Equals(ShortName))
                {
                    string errorMessage = ShortName+ " is already given to "+ateacher.Name;
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(true, JsonRequestBehavior.AllowGet);
            
        }

        public JsonResult IsEmailExist(string Email)
        {
            int userAccountId = (int) Session["user_id"];     //new append
            var teacher = db.Teachers.Where(x => x.Email == Email && x.UserAccountId==userAccountId).ToList();     //append userAccountId check in Where
            string teacherName = "";
            if (teacher.Count() >0)
            {
                foreach (var ateacher in teacher)
                {
                    teacherName = ateacher.Name;
                }
                string errorMessage = Email + " is already given to " + teacherName;
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}

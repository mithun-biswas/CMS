using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.AccessControl;
using System.Web;

namespace CMS.Models
{
    public class CourseManagementDbContext: DbContext
    {
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Designation> Designations { get; set; }
        public DbSet<Course> Courses { set; get; }
        public DbSet<Time> Times { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<UserAccount> UserAccounts { get; set; } 
    }
}
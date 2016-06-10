namespace CMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addCourseTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Courses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(),
                        Name = c.String(),
                        Section = c.Int(nullable: false),
                        LabRequired = c.String(),
                        IsItaLabCourse = c.Int(nullable: false),
                        Semester = c.String(),
                        TeacherId = c.String(),
                        TimeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Courses");
        }
    }
}

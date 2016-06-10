namespace CMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeTeacherIdPropertyInCourse : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Courses", "TeacherId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Courses", "TeacherId", c => c.String());
        }
    }
}

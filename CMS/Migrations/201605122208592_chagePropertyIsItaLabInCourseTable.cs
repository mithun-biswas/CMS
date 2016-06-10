namespace CMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class chagePropertyIsItaLabInCourseTable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Courses", "IsItaLabCourse", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Courses", "IsItaLabCourse", c => c.Int(nullable: false));
        }
    }
}

namespace CMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addUserAccountPropertyToCourseModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Courses", "UserAccountId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Courses", "UserAccountId");
        }
    }
}

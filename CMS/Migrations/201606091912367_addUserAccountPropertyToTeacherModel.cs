namespace CMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addUserAccountPropertyToTeacherModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Teachers", "UserAccountId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Teachers", "UserAccountId");
        }
    }
}

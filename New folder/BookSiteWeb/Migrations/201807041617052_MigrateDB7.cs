namespace BookSiteWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MigrateDB7 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Comments", "ImageURL", c => c.String(maxLength: 2083));
            AddColumn("dbo.Comments", "UserName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Comments", "UserName");
            DropColumn("dbo.Comments", "ImageURL");
        }
    }
}

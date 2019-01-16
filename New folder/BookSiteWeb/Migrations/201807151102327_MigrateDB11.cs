namespace BookSiteWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MigrateDB11 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.News", "UserName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.News", "UserName");
        }
    }
}

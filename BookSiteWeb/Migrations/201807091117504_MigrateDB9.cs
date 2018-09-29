namespace BookSiteWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MigrateDB9 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.News", "TagName", c => c.String(maxLength: 100));
        }
        
        public override void Down()
        {
            DropColumn("dbo.News", "TagName");
        }
    }
}

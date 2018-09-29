namespace BookSiteWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MigrateDB5 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.News", "TextNews", c => c.String());
            AlterColumn("dbo.News", "Description", c => c.String(maxLength: 1000));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.News", "Description", c => c.String());
            DropColumn("dbo.News", "TextNews");
        }
    }
}

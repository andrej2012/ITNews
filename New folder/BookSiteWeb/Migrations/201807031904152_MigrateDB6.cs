namespace BookSiteWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MigrateDB6 : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Comments", new[] { "News_NewsId" });
            DropColumn("dbo.Comments", "NewsId");
            RenameColumn(table: "dbo.Comments", name: "News_NewsId", newName: "NewsId");
            AlterColumn("dbo.Comments", "UserId", c => c.String());
            AlterColumn("dbo.Comments", "NewsId", c => c.Int());
            CreateIndex("dbo.Comments", "NewsId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Comments", new[] { "NewsId" });
            AlterColumn("dbo.Comments", "NewsId", c => c.String());
            AlterColumn("dbo.Comments", "UserId", c => c.Int());
            RenameColumn(table: "dbo.Comments", name: "NewsId", newName: "News_NewsId");
            AddColumn("dbo.Comments", "NewsId", c => c.String());
            CreateIndex("dbo.Comments", "News_NewsId");
        }
    }
}

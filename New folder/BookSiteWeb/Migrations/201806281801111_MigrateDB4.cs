namespace BookSiteWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MigrateDB4 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Comments", "NewsId", "dbo.News");
            DropIndex("dbo.Comments", new[] { "NewsId" });
            AddColumn("dbo.Comments", "News_NewsId", c => c.Int());
            AlterColumn("dbo.Comments", "NewsId", c => c.String());
            AlterColumn("dbo.Likes", "UserId", c => c.String());
            AlterColumn("dbo.News", "UserId", c => c.String());
            AlterColumn("dbo.Ratings", "UserId", c => c.String());
            CreateIndex("dbo.Comments", "News_NewsId");
            AddForeignKey("dbo.Comments", "News_NewsId", "dbo.News", "NewsId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Comments", "News_NewsId", "dbo.News");
            DropIndex("dbo.Comments", new[] { "News_NewsId" });
            AlterColumn("dbo.Ratings", "UserId", c => c.Int());
            AlterColumn("dbo.News", "UserId", c => c.Int());
            AlterColumn("dbo.Likes", "UserId", c => c.Int());
            AlterColumn("dbo.Comments", "NewsId", c => c.Int());
            DropColumn("dbo.Comments", "News_NewsId");
            CreateIndex("dbo.Comments", "NewsId");
            AddForeignKey("dbo.Comments", "NewsId", "dbo.News", "NewsId");
        }
    }
}

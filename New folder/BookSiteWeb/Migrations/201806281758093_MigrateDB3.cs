namespace BookSiteWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MigrateDB3 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TagApplicationUsers", "Tag_TagId", "dbo.Tags");
            DropForeignKey("dbo.TagApplicationUsers", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.TagApplicationUsers", new[] { "Tag_TagId" });
            DropIndex("dbo.TagApplicationUsers", new[] { "ApplicationUser_Id" });
            CreateTable(
                "dbo.TagNews",
                c => new
                    {
                        Tag_TagId = c.Int(nullable: false),
                        News_NewsId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Tag_TagId, t.News_NewsId })
                .ForeignKey("dbo.Tags", t => t.Tag_TagId, cascadeDelete: true)
                .ForeignKey("dbo.News", t => t.News_NewsId, cascadeDelete: true)
                .Index(t => t.Tag_TagId)
                .Index(t => t.News_NewsId);
            
            AddColumn("dbo.Comments", "UserId", c => c.Int());
            AddColumn("dbo.Likes", "UserId", c => c.Int());
            AddColumn("dbo.News", "UserId", c => c.Int());
            AddColumn("dbo.Ratings", "UserId", c => c.Int());
            DropTable("dbo.TagApplicationUsers");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.TagApplicationUsers",
                c => new
                    {
                        Tag_TagId = c.Int(nullable: false),
                        ApplicationUser_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Tag_TagId, t.ApplicationUser_Id });
            
            DropForeignKey("dbo.TagNews", "News_NewsId", "dbo.News");
            DropForeignKey("dbo.TagNews", "Tag_TagId", "dbo.Tags");
            DropIndex("dbo.TagNews", new[] { "News_NewsId" });
            DropIndex("dbo.TagNews", new[] { "Tag_TagId" });
            DropColumn("dbo.Ratings", "UserId");
            DropColumn("dbo.News", "UserId");
            DropColumn("dbo.Likes", "UserId");
            DropColumn("dbo.Comments", "UserId");
            DropTable("dbo.TagNews");
            CreateIndex("dbo.TagApplicationUsers", "ApplicationUser_Id");
            CreateIndex("dbo.TagApplicationUsers", "Tag_TagId");
            AddForeignKey("dbo.TagApplicationUsers", "ApplicationUser_Id", "dbo.AspNetUsers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TagApplicationUsers", "Tag_TagId", "dbo.Tags", "TagId", cascadeDelete: true);
        }
    }
}

namespace BookSiteWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MigrateDB : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Likes", "CommentId", "dbo.Comments");
            DropForeignKey("dbo.Comments", "NewsId", "dbo.News");
            DropForeignKey("dbo.Ratings", "NewsId", "dbo.News");
            DropIndex("dbo.Comments", new[] { "NewsId" });
            DropIndex("dbo.Likes", new[] { "CommentId" });
            DropIndex("dbo.Ratings", new[] { "NewsId" });
            DropPrimaryKey("dbo.Comments");
            DropPrimaryKey("dbo.Likes");
            DropPrimaryKey("dbo.News");
            DropPrimaryKey("dbo.Ratings");
            AddColumn("dbo.Comments", "Id", c => c.String(nullable: false, maxLength: 128));
            AddColumn("dbo.Comments", "News_Id", c => c.String(maxLength: 128));
            AddColumn("dbo.Likes", "Id", c => c.String(nullable: false, maxLength: 128));
            AddColumn("dbo.Likes", "Comment_Id", c => c.String(maxLength: 128));
            AddColumn("dbo.News", "Id", c => c.String(nullable: false, maxLength: 128));
            AddColumn("dbo.Ratings", "Id", c => c.String(nullable: false, maxLength: 128));
            AddColumn("dbo.Ratings", "News_Id", c => c.String(maxLength: 128));
            AlterColumn("dbo.Comments", "CommentId", c => c.Int(nullable: false));
            AlterColumn("dbo.Likes", "LikeId", c => c.Int(nullable: false));
            AlterColumn("dbo.News", "NewsId", c => c.Int(nullable: false));
            AlterColumn("dbo.Ratings", "RatingId", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.Comments", "Id");
            AddPrimaryKey("dbo.Likes", "Id");
            AddPrimaryKey("dbo.News", "Id");
            AddPrimaryKey("dbo.Ratings", "Id");
            CreateIndex("dbo.Comments", "News_Id");
            CreateIndex("dbo.Likes", "Comment_Id");
            CreateIndex("dbo.Ratings", "News_Id");
            AddForeignKey("dbo.Likes", "Comment_Id", "dbo.Comments", "Id");
            AddForeignKey("dbo.Comments", "News_Id", "dbo.News", "Id");
            AddForeignKey("dbo.Ratings", "News_Id", "dbo.News", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Ratings", "News_Id", "dbo.News");
            DropForeignKey("dbo.Comments", "News_Id", "dbo.News");
            DropForeignKey("dbo.Likes", "Comment_Id", "dbo.Comments");
            DropIndex("dbo.Ratings", new[] { "News_Id" });
            DropIndex("dbo.Likes", new[] { "Comment_Id" });
            DropIndex("dbo.Comments", new[] { "News_Id" });
            DropPrimaryKey("dbo.Ratings");
            DropPrimaryKey("dbo.News");
            DropPrimaryKey("dbo.Likes");
            DropPrimaryKey("dbo.Comments");
            AlterColumn("dbo.Ratings", "RatingId", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.News", "NewsId", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.Likes", "LikeId", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.Comments", "CommentId", c => c.Int(nullable: false, identity: true));
            DropColumn("dbo.Ratings", "News_Id");
            DropColumn("dbo.Ratings", "Id");
            DropColumn("dbo.News", "Id");
            DropColumn("dbo.Likes", "Comment_Id");
            DropColumn("dbo.Likes", "Id");
            DropColumn("dbo.Comments", "News_Id");
            DropColumn("dbo.Comments", "Id");
            AddPrimaryKey("dbo.Ratings", "RatingId");
            AddPrimaryKey("dbo.News", "NewsId");
            AddPrimaryKey("dbo.Likes", "LikeId");
            AddPrimaryKey("dbo.Comments", "CommentId");
            CreateIndex("dbo.Ratings", "NewsId");
            CreateIndex("dbo.Likes", "CommentId");
            CreateIndex("dbo.Comments", "NewsId");
            AddForeignKey("dbo.Ratings", "NewsId", "dbo.News", "NewsId");
            AddForeignKey("dbo.Comments", "NewsId", "dbo.News", "NewsId");
            AddForeignKey("dbo.Likes", "CommentId", "dbo.Comments", "CommentId");
        }
    }
}

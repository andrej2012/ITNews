namespace BookSiteWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MigrateDB1 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Likes", "Comment_Id", "dbo.Comments");
            DropForeignKey("dbo.Comments", "News_Id", "dbo.News");
            DropForeignKey("dbo.Ratings", "News_Id", "dbo.News");
            DropIndex("dbo.Comments", new[] { "News_Id" });
            DropIndex("dbo.Likes", new[] { "Comment_Id" });
            DropIndex("dbo.Ratings", new[] { "News_Id" });
            DropColumn("dbo.Comments", "NewsId");
            DropColumn("dbo.Likes", "CommentId");
            DropColumn("dbo.Ratings", "NewsId");
            RenameColumn(table: "dbo.Likes", name: "Comment_Id", newName: "CommentId");
            RenameColumn(table: "dbo.Comments", name: "News_Id", newName: "NewsId");
            RenameColumn(table: "dbo.Ratings", name: "News_Id", newName: "NewsId");
            DropPrimaryKey("dbo.Comments");
            DropPrimaryKey("dbo.Likes");
            DropPrimaryKey("dbo.News");
            DropPrimaryKey("dbo.Ratings");
            AlterColumn("dbo.Comments", "CommentId", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.Comments", "NewsId", c => c.Int());
            AlterColumn("dbo.Likes", "LikeId", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.Likes", "CommentId", c => c.Int());
            AlterColumn("dbo.News", "NewsId", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.Ratings", "RatingId", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.Ratings", "NewsId", c => c.Int());
            AddPrimaryKey("dbo.Comments", "CommentId");
            AddPrimaryKey("dbo.Likes", "LikeId");
            AddPrimaryKey("dbo.News", "NewsId");
            AddPrimaryKey("dbo.Ratings", "RatingId");
            CreateIndex("dbo.Comments", "NewsId");
            CreateIndex("dbo.Likes", "CommentId");
            CreateIndex("dbo.Ratings", "NewsId");
            AddForeignKey("dbo.Likes", "CommentId", "dbo.Comments", "CommentId");
            AddForeignKey("dbo.Comments", "NewsId", "dbo.News", "NewsId");
            AddForeignKey("dbo.Ratings", "NewsId", "dbo.News", "NewsId");
            DropColumn("dbo.Comments", "Id");
            DropColumn("dbo.Likes", "Id");
            DropColumn("dbo.News", "Id");
            DropColumn("dbo.Ratings", "Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Ratings", "Id", c => c.String(nullable: false, maxLength: 128));
            AddColumn("dbo.News", "Id", c => c.String(nullable: false, maxLength: 128));
            AddColumn("dbo.Likes", "Id", c => c.String(nullable: false, maxLength: 128));
            AddColumn("dbo.Comments", "Id", c => c.String(nullable: false, maxLength: 128));
            DropForeignKey("dbo.Ratings", "NewsId", "dbo.News");
            DropForeignKey("dbo.Comments", "NewsId", "dbo.News");
            DropForeignKey("dbo.Likes", "CommentId", "dbo.Comments");
            DropIndex("dbo.Ratings", new[] { "NewsId" });
            DropIndex("dbo.Likes", new[] { "CommentId" });
            DropIndex("dbo.Comments", new[] { "NewsId" });
            DropPrimaryKey("dbo.Ratings");
            DropPrimaryKey("dbo.News");
            DropPrimaryKey("dbo.Likes");
            DropPrimaryKey("dbo.Comments");
            AlterColumn("dbo.Ratings", "NewsId", c => c.String(maxLength: 128));
            AlterColumn("dbo.Ratings", "RatingId", c => c.Int(nullable: false));
            AlterColumn("dbo.News", "NewsId", c => c.Int(nullable: false));
            AlterColumn("dbo.Likes", "CommentId", c => c.String(maxLength: 128));
            AlterColumn("dbo.Likes", "LikeId", c => c.Int(nullable: false));
            AlterColumn("dbo.Comments", "NewsId", c => c.String(maxLength: 128));
            AlterColumn("dbo.Comments", "CommentId", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.Ratings", "Id");
            AddPrimaryKey("dbo.News", "Id");
            AddPrimaryKey("dbo.Likes", "Id");
            AddPrimaryKey("dbo.Comments", "Id");
            RenameColumn(table: "dbo.Ratings", name: "NewsId", newName: "News_Id");
            RenameColumn(table: "dbo.Comments", name: "NewsId", newName: "News_Id");
            RenameColumn(table: "dbo.Likes", name: "CommentId", newName: "Comment_Id");
            AddColumn("dbo.Ratings", "NewsId", c => c.Int());
            AddColumn("dbo.Likes", "CommentId", c => c.Int());
            AddColumn("dbo.Comments", "NewsId", c => c.Int());
            CreateIndex("dbo.Ratings", "News_Id");
            CreateIndex("dbo.Likes", "Comment_Id");
            CreateIndex("dbo.Comments", "News_Id");
            AddForeignKey("dbo.Ratings", "News_Id", "dbo.News", "Id");
            AddForeignKey("dbo.Comments", "News_Id", "dbo.News", "Id");
            AddForeignKey("dbo.Likes", "Comment_Id", "dbo.Comments", "Id");
        }
    }
}

namespace BookSiteWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MigrateDB13 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.News", "Rate", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.News", "Rate", c => c.Int(nullable: false));
        }
    }
}

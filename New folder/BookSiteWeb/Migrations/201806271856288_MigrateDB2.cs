namespace BookSiteWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MigrateDB2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.News", "Description", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.News", "Description", c => c.String(maxLength: 1000));
        }
    }
}

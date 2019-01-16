namespace BookSiteWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MigrateDB10 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tags", "TagName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tags", "TagName");
        }
    }
}

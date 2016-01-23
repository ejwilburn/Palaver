namespace Palaver2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EFUpgrade : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Subscriptions",
                c => new
                    {
                        SubscriptionId = c.Int(nullable: false, identity: true),
                        Subject_CommentId = c.Int(),
                        User_UserId = c.Guid(),
                    })
                .PrimaryKey(t => t.SubscriptionId)
                .ForeignKey("dbo.Comments", t => t.Subject_CommentId)
                .ForeignKey("dbo.Users", t => t.User_UserId)
                .Index(t => t.Subject_CommentId)
                .Index(t => t.User_UserId);

            DropIndex("dbo.Comments", new[] { "User_UserId" });
            DropIndex("dbo.Comments", new[] { "Comment_CommentId" });
            DropIndex("dbo.UnreadItems", new[] { "User_UserId" });
            DropIndex("dbo.UnreadItems", new[] { "Comment_CommentId" });
            DropIndex("dbo.RoleUsers", new[] { "User_UserId" });
            DropIndex("dbo.RoleUsers", new[] { "Role_RoleId" });
            CreateIndex("dbo.Comments", "Comment_CommentId");
            CreateIndex("dbo.Comments", "User_UserId");
            CreateIndex("dbo.UnreadItems", "Comment_CommentId");
            CreateIndex("dbo.UnreadItems", "User_UserId");
            CreateIndex("dbo.RoleUsers", "Role_RoleId");
            CreateIndex("dbo.RoleUsers", "User_UserId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Subscriptions", "User_UserId", "dbo.Users");
            DropForeignKey("dbo.Subscriptions", "Subject_CommentId", "dbo.Comments");
            DropIndex("dbo.RoleUsers", new[] { "User_UserId" });
            DropIndex("dbo.RoleUsers", new[] { "Role_RoleId" });
            DropIndex("dbo.UnreadItems", new[] { "User_UserId" });
            DropIndex("dbo.UnreadItems", new[] { "Comment_CommentId" });
            DropIndex("dbo.Subscriptions", new[] { "User_UserId" });
            DropIndex("dbo.Subscriptions", new[] { "Subject_CommentId" });
            DropIndex("dbo.Comments", new[] { "User_UserId" });
            DropIndex("dbo.Comments", new[] { "Comment_CommentId" });
            DropTable("dbo.Subscriptions");
        }
    }
}

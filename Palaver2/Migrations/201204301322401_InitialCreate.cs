/*
Copyright 2012, Marcus McKinnon, E.J. Wilburn, Kevin Williams
This program is distributed under the terms of the GNU General Public License.

This file is part of Palaver.

Palaver is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 2 of the License, or
(at your option) any later version.

Palaver is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Palaver.  If not, see <http://www.gnu.org/licenses/>.
*/

namespace Palaver2.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Comments",
                c => new
                    {
                        CommentId = c.Int(nullable: false, identity: true),
                        CreatedTime = c.DateTime(nullable: false),
                        LastUpdatedTime = c.DateTime(nullable: false),
                        Text = c.String(nullable: false),
                        SubjectId = c.Int(),
                        User_UserId = c.Guid(nullable: false),
                        ParentComment_CommentId = c.Int(),
                    })
                .PrimaryKey(t => t.CommentId)
                .ForeignKey("Users", t => t.User_UserId, cascadeDelete: true)
                .ForeignKey("Comments", t => t.ParentComment_CommentId)
                .Index(t => t.User_UserId)
                .Index(t => t.ParentComment_CommentId);
            
            CreateTable(
                "Users",
                c => new
                    {
                        UserId = c.Guid(nullable: false),
                        Username = c.String(nullable: false, maxLength: 20),
                        Email = c.String(nullable: false, maxLength: 250),
                        Password = c.String(nullable: false, maxLength: 100),
                        IsConfirmed = c.Boolean(nullable: false),
                        PasswordFailuresSinceLastSuccess = c.Int(nullable: false),
                        LastPasswordFailureDate = c.DateTime(),
                        ConfirmationToken = c.String(),
                        CreateDate = c.DateTime(),
                        PasswordChangedDate = c.DateTime(),
                        PasswordVerificationToken = c.String(),
                        PasswordVerificationTokenExpirationDate = c.DateTime(),
                        FirstName = c.String(),
                        LastName = c.String(),
                        TimeZone = c.String(),
                        Culture = c.String(),
                    })
                .PrimaryKey(t => t.UserId);
            
            CreateTable(
                "Roles",
                c => new
                    {
                        RoleId = c.Guid(nullable: false),
                        RoleName = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 250),
                    })
                .PrimaryKey(t => t.RoleId);
            
            CreateTable(
                "UnreadItems",
                c => new
                    {
                        UnreadItemId = c.Int(nullable: false, identity: true),
                        Comment_CommentId = c.Int(),
                        User_UserId = c.Guid(),
                    })
                .PrimaryKey(t => t.UnreadItemId)
                .ForeignKey("Comments", t => t.Comment_CommentId)
                .ForeignKey("Users", t => t.User_UserId)
                .Index(t => t.Comment_CommentId)
                .Index(t => t.User_UserId);
            
            CreateTable(
                "RoleUsers",
                c => new
                    {
                        Role_RoleId = c.Guid(nullable: false),
                        User_UserId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Role_RoleId, t.User_UserId })
                .ForeignKey("Roles", t => t.Role_RoleId, cascadeDelete: true)
                .ForeignKey("Users", t => t.User_UserId, cascadeDelete: true)
                .Index(t => t.Role_RoleId)
                .Index(t => t.User_UserId);
        }
        
        public override void Down()
        {
            DropIndex("RoleUsers", new[] { "User_UserId" });
            DropIndex("RoleUsers", new[] { "Role_RoleId" });
            DropIndex("UnreadItems", new[] { "User_UserId" });
            DropIndex("UnreadItems", new[] { "Comment_CommentId" });
            DropIndex("Comments", new[] { "ParentComment_CommentId" });
            DropIndex("Comments", new[] { "User_UserId" });
            DropForeignKey("RoleUsers", "User_UserId", "Users");
            DropForeignKey("RoleUsers", "Role_RoleId", "Roles");
            DropForeignKey("UnreadItems", "User_UserId", "Users");
            DropForeignKey("UnreadItems", "Comment_CommentId", "Comments");
            DropForeignKey("Comments", "ParentComment_CommentId", "Comments");
            DropForeignKey("Comments", "User_UserId", "Users");
            DropTable("RoleUsers");
            DropTable("UnreadItems");
            DropTable("Roles");
            DropTable("Users");
            DropTable("Comments");
        }
    }
}

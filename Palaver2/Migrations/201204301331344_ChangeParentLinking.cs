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
    
    public partial class ChangeParentLinking : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("Comments", "ParentComment_CommentId", "Comments");
            DropIndex("Comments", new[] { "ParentComment_CommentId" });
            AddColumn("Comments", "ParentCommentId", c => c.Int());
            AddColumn("Comments", "Comment_CommentId", c => c.Int());
            AddForeignKey("Comments", "Comment_CommentId", "Comments", "CommentId");
            CreateIndex("Comments", "Comment_CommentId");
            CreateIndex("Comments", "ParentCommentId");
            DropColumn("Comments", "ParentComment_CommentId");
        }
        
        public override void Down()
        {
            AddColumn("Comments", "ParentComment_CommentId", c => c.Int());
            DropIndex("Comments", new[] { "ParentCommentId" });
            DropIndex("Comments", new[] { "Comment_CommentId" });
            DropForeignKey("Comments", "Comment_CommentId", "Comments");
            DropColumn("Comments", "Comment_CommentId");
            DropColumn("Comments", "ParentCommentId");
            CreateIndex("Comments", "ParentComment_CommentId");
            AddForeignKey("Comments", "ParentComment_CommentId", "Comments", "CommentId");
        }
    }
}

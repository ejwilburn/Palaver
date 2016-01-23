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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using CodeFirstMembership.Models;
using System.ComponentModel.DataAnnotations;
using CodeFirstMembership;
using Palaver2.Helpers;

namespace Palaver2.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        [Required()]
        public virtual User User { get; set; }
        [Required()]
        public DateTime CreatedTime { get; set; }
        [Required()]
        public DateTime LastUpdatedTime { get; set; }
        [Required()]
        [AllowHtml]
        public string Text { get; set; }
        public int? ParentCommentId { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public int? SubjectId { get; set; }

        public Comment()
        {
            CreatedTime = DateTime.UtcNow;
            LastUpdatedTime = DateTime.UtcNow;
        }

        public Comment(string commentText, User creator)
        {
            CreatedTime = DateTime.UtcNow;
            LastUpdatedTime = DateTime.UtcNow;
            User = creator;
            Text = CustomHtmlHelpers.Linkify(commentText);
        }
    }

    public class UnreadItem
    {
        public int UnreadItemId { get; set; }
        public virtual Comment Comment { get; set; }
        public virtual User User { get; set; }
    }

    public class Subscription
    {
        public int SubscriptionId { get; set; }
        public virtual Comment Subject { get; set; }
        public virtual User User { get; set; }
    }

    public class PalaverDb : DbContext
    {
        public DbSet<Comment> Comments { get; set; }
        public DbSet<UnreadItem> UnreadItems { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        public PalaverDb() : base("Palaver") { this.Configuration.LazyLoadingEnabled = false; }
        
        public int GetUnreadCommentCount(Comment subject, Guid userid)
        {
            var q = from c in this.Comments
                    join r in this.UnreadItems on c.CommentId equals r.Comment.CommentId
                    where c.SubjectId == subject.CommentId && r.User.UserId == userid
                    select new
                    {
                        c.CommentId
                    };

            return q.Count();
        }

        public Dictionary<int, int> GetUnreadCommentTotals(Guid userid)
        {
            Dictionary<int, int> counts = new Dictionary<int, int>();

            var countTotals = from c in this.Comments
                    join r in this.UnreadItems on c.CommentId equals r.Comment.CommentId
                    where r.User.UserId == userid && c.SubjectId != null
                    group r by r.Comment.SubjectId into g
                    select new
                    {
                        ThreadId = g.Key, Count = g.Count()
                    };

            foreach (var count in countTotals)
            {
                counts.Add((int)count.ThreadId, (int)count.Count);
            }

            return counts;
        }
    }

    public class CodeFirstContextInit : DropCreateDatabaseAlways<PalaverDb>
    {

        protected override void Seed(PalaverDb context)
        {
            CodeFirstSecurity.CreateAccount("Demo", "Demo", "demo@demo.com");

            User u = context.Users.FirstOrDefault();
            Comment c = new Comment { User = u, Text = "testing", CreatedTime = DateTime.UtcNow, LastUpdatedTime = DateTime.UtcNow };
            context.Comments.Add(c);
            context.SaveChanges();
            c.SubjectId = c.CommentId;

            context.Comments.Add(new Comment { ParentCommentId = c.CommentId, User = u, Text = "testing 1", CreatedTime = DateTime.UtcNow, LastUpdatedTime = DateTime.UtcNow, SubjectId = c.SubjectId });
            context.Comments.Add(new Comment { ParentCommentId = c.CommentId, User = u, Text = "testing 2", CreatedTime = DateTime.UtcNow, LastUpdatedTime = DateTime.UtcNow, SubjectId = c.SubjectId });
            context.Comments.Add(new Comment { ParentCommentId = c.CommentId, User = u, Text = "testing 3", CreatedTime = DateTime.UtcNow, LastUpdatedTime = DateTime.UtcNow, SubjectId = c.SubjectId });

            context.SaveChanges();

            foreach (Comment cc in context.Comments)
            {
                foreach (User uu in context.Users)
                {
                    if (uu.UserId != u.UserId)
                        context.UnreadItems.Add(new UnreadItem { Comment = cc, User = uu });
                }
            }

            context.SaveChanges();
        }
    }
}
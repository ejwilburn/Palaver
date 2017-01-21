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
using System.Data.SqlClient;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using CodeFirstMembership.Models;
using System.ComponentModel.DataAnnotations;
using CodeFirstMembership;
using Palaver2.Helpers;
using Newtonsoft.Json;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.ComponentModel.DataAnnotations.Schema;

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
		[NotMapped]
		public Boolean isUnread { get; set; }
		[NotMapped]
		public int unreadCount { get; set; }

        public Comment()
        {
			isUnread = false;
			unreadCount = 0;
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
		[Required()]
        public int UnreadItemId { get; set; }
		[Required()]
        public virtual Comment Comment { get; set; }
		[Required()]
        public virtual User User { get; set; }
    }

	/*
    public class Subscription
    {
        public int SubscriptionId { get; set; }
        public virtual Comment Subject { get; set; }
        public virtual User User { get; set; }
    }
    */

	/*
	internal sealed class EntityFrameworkDbConfiguration : DbConfiguration
	{
		/// <summary>
		/// The provider manifest token to use for SQL Server.
		/// </summary>
		private const string SqlServerManifestToken = @"2005";

		/// <summary>
		/// Initializes a new instance of the <see cref="EntityFrameworkDbConfiguration"/> class.
		/// </summary>
		public EntityFrameworkDbConfiguration()
		{
			this.AddDependencyResolver(new SingletonDependencyResolver<IManifestTokenResolver>(new ManifestTokenService()));
		}

		/// <inheritdoc />
		private sealed class ManifestTokenService : IManifestTokenResolver
		{
			/// <summary>
			/// The default token resolver.
			/// </summary>
			private static readonly IManifestTokenResolver DefaultManifestTokenResolver = new DefaultManifestTokenResolver();

			/// <inheritdoc />
			public string ResolveManifestToken(DbConnection connection)
			{
				if (connection is SqlConnection)
				{
					return SqlServerManifestToken;
				}

				return DefaultManifestTokenResolver.ResolveManifestToken(connection);
			}
		}
	}

	public class LowerCaseNamesConvention : Convention
	{
		public LowerCaseNamesConvention()
		{
			this.Types().Configure(t =>
		   	{
				   var name = t.ClrType.Name;
				   t.ToTable(name.ToLower() + "s");
		   	});

			this.Properties().Configure(c =>
			{
				var name = c.ClrPropertyInfo.Name;
				var newName = name.ToLower();
				c.HasColumnName(newName);
			});
		}
	}
*/

	public class PalaverDb : DbContext
    {
        public DbSet<Comment> Comments { get; set; }
        public DbSet<UnreadItem> UnreadItems { get; set; }
        //public DbSet<Subscription> Subscriptions { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

		public PalaverDb() : base("Palaver")
		{
			Database.CommandTimeout = 180;
			Configuration.LazyLoadingEnabled = false;
		}


		public List<Comment> GetThreads()
		{
			List<Comment> threads = Comments.Include("User").Where(x => !x.ParentCommentId.HasValue).OrderByDescending(x => x.LastUpdatedTime).ToList();

			// Get unread counts for each thread for the current user.
			Guid userId = CodeFirstMembership.CodeFirstSecurity.CurrentUserId;
			var countTotals = from c in Comments
							  join r in UnreadItems on c.CommentId equals r.Comment.CommentId
							  where r.User.UserId == userId && r.Comment.SubjectId != null
							  group r by r.Comment.SubjectId into g
							  select new
							  {
								  ThreadId = g.Key,
								  Count = g.Count()
							  };


			if (countTotals != null && countTotals.Count() > 0)
			{
				foreach (var count in countTotals)
				{
					threads.Find(t => t.CommentId == (int)count.ThreadId).unreadCount = (int)count.Count;
				}
			}

			return threads;
		}

		public List<Comment> GetComments(int subjectId)
		{
			List<Comment> threadComments = Comments.Include("User").Include("Comments").Where(x => x.SubjectId == subjectId).OrderBy(x=> x.CreatedTime).ToList();

			// Get unread flag for these comments and the current user.
			Guid userId = CodeFirstMembership.CodeFirstSecurity.CurrentUserId;
			var unread = UnreadItems.Where(ui => ui.User.UserId == userId && ui.Comment.SubjectId == subjectId).ToList();
			/*
			var unreadComments = (from c in threadComments
								 join ui in UnreadItems on c.CommentId equals ui.Comment.CommentId
			                     where ui.User.UserId == userId && c.SubjectId == subjectId
			                     select ui.Comment.CommentId).ToList();
			*/

			if (unread != null & unread.Count > 0)
			{
				foreach (var ui in unread)
				{
					threadComments.Find(tc => tc.CommentId == ui.Comment.CommentId).isUnread = true;
				}
			}

			return threadComments;
		}

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			Database.SetInitializer<PalaverDb>(null);
			// Database.SetInitializer<PalaverDb>(new CodeFirstContextInit());
			// modelBuilder.Conventions.Add(new LowerCaseNamesConvention());
			/*
			modelBuilder.Properties().Configure(c =>
			{
				var name = c.ClrPropertyInfo.Name;
				var newName = name.ToLower();
				c.HasColumnName(newName);
			});

			modelBuilder.Entity<User>().HasMany(c => c.Roles)
				.WithMany(p => p.Users).Map(
				m =>
				{
					m.MapLeftKey("role_roleid");
					m.MapRightKey("user_userid");
					m.ToTable("roleusers");
				});
			*/
		}
    }

    public class CodeFirstContextInit : DropCreateDatabaseAlways<PalaverDb>
    {

        protected override void Seed(PalaverDb context)
        {
			base.Seed(context);

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
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
using System.Web.Mvc.Html;
using Palaver2.Models;
using Palaver2.Helpers;
using CodeFirstMembership.Models;
using CodeFirstMembership;

namespace Palaver2.Controllers
{
    public class HomeController : Controller
    {
        private Palaver2.Models.PalaverDb db = new Models.PalaverDb();
        
        [HttpGet]
        [Authorize]
        public ActionResult Index(string threadId)
        {
			ViewBag.threads = db.GetThreads();
			if (ParseThreadId(threadId))
			{
				ViewBag.comments = db.GetComments(ViewBag.threadId);
			}
			return View();
			/*
            int intThreadId;

			ViewBag.Threads = db.Comments.Where(x => !x.ParentCommentId.HasValue).ToList();

            // Guid userId = CodeFirstSecurity.GetUserId(HttpContext.User.Identity.Name);

			var threads = from c in db.Comments
                           // join s in db.Subscriptions on c.CommentId equals s.Subject.CommentId
                           where !c.ParentCommentId.HasValue // && s.User.UserId == userId
                           orderby c.LastUpdatedTime descending
                           select c;

			if (threadId != null && int.TryParse(threadId, out intThreadId))
			{
				ViewBag.Comments = db.Comments.Where(x => x.SubjectId.Equals(intThreadId)).ToList();
				ViewBag.ThreadId = intThreadId;
			}

            return View();
            */
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetThread(string threadId)
        {
			ViewBag.threads = db.GetThreads();
			if (ParseThreadId(threadId))
			{
				ViewBag.comments = db.GetComments(ViewBag.threadId);
			}
			return View("Index");
			/*
            int intThreadId;

            // Guid userId = CodeFirstSecurity.GetUserId(HttpContext.User.Identity.Name);
            var comments = from c in db.Comments
                           // join s in db.Subscriptions on c.CommentId equals s.Subject.CommentId
                           where !c.ParentCommentId.HasValue // && s.User.UserId == userId
                           orderby c.LastUpdatedTime descending
                           select c;
            ViewBag.Comments = comments;
            if (threadId != null && int.TryParse(threadId, out intThreadId))
                ViewBag.ThreadId = intThreadId;

            return View("Index");
            */
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetComment(string threadId, string commentId)
        {
			ViewBag.threads = db.GetThreads();
			if (ParseThreadId(threadId))
			{
				ViewBag.comments = db.GetComments(ViewBag.threadId);
				ParseCommentId(commentId);
			}

            return View("Index");
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetComments(int id)
        {
			//db.Configuration.LazyLoadingEnabled = false;
			List<Comment> threadComments = db.GetComments(id);

            return PartialView("_Comments", threadComments);
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetThreads()
        {
			ViewBag.threads = db.GetThreads();
			return View();

			/*
            // Guid userId = CodeFirstSecurity.GetUserId(HttpContext.User.Identity.Name);
            var comments = from c in db.Comments
                           // join s in db.Subscriptions on c.CommentId equals s.Subject.CommentId
                           where !c.ParentCommentId.HasValue // && s.User.UserId == userId
                           orderby c.LastUpdatedTime descending
                           select c;
            return Content(Helpers.CustomHtmlHelpers.BuildThreads(comments));
			*/
        }

        [HttpGet]
        [Authorize]
        public ActionResult PostReply(int id, string text)
        {

            var comment = db.Comments.First(x => x.CommentId == id);

            // Convert URLs in the text to links if they're not already a link.
            text = CustomHtmlHelpers.Linkify(text);

            Comment newComment = new Comment() {
                Text = text,
                CreatedTime = DateTime.Now};

            comment.Comments.Add(newComment);
            db.SaveChanges();

            return PartialView("_comment", newComment );
        }

        [HttpGet]
        [Authorize]
        public ActionResult CreateThread(string text)
        {
            Comment c = new Comment() { 
                Text = text,
                CreatedTime = DateTime.Now
            };

            db.Comments.Add(c);
            db.SaveChanges();

			//ViewBag.Comments = db.Comments.Where(x => !x.ParentCommentId.HasValue).ToList();

			ViewBag.threads = db.GetThreads();

			return PartialView("_topcomments");
        }

        [HttpGet]
        [Authorize]
        public ActionResult MarkRead(int id)
        {
            UnreadItem read = db.UnreadItems.Where(r => r.Comment.CommentId == id && r.User.UserId == CodeFirstMembership.CodeFirstSecurity.CurrentUserId).FirstOrDefault();

            if (read != null)
            {
                db.UnreadItems.Remove(read);
                db.SaveChanges();
            }

            return new EmptyResult();
        }

		private Boolean ParseThreadId(String stringId)
		{
			int parsedId;
			if (stringId != null && int.TryParse(stringId, out parsedId))
			{
				ViewBag.threadId = parsedId;
				return true;
			}

			return false;
		}

		private Boolean ParseCommentId(String stringId)
		{
			int parsedId;
			if (stringId != null && int.TryParse(stringId, out parsedId))
			{
				ViewBag.commentId = parsedId;
				return true;
			}

			return false;
		}
    }
}

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
using Palaver2.Models;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;
using CodeFirstMembership.Models;

namespace Palaver2.Helpers
{
    public class CustomHtmlHelpers
    {
        public static string Linkify(string input)
        {
            String output = input;
            // Convert URLs in the text to links if they're not already a link.
            // First linkify URLs with the protocol already there.
            Regex regex = new Regex(@"(?<!(?:href=[""']?|src=['""]?|<a[^>]*>)[^.'""]*[\s]*)\b(?:(?:https?|ftps?|ftpes|file)://)[-A-Z0-9+&;@#/%=~_|$?!:,.]*[A-Z0-9+&;@#/%=~_|$]", RegexOptions.IgnoreCase);
            int beginningSize = output.Length;
            MatchCollection matches = regex.Matches(output);
            foreach (Match match in matches)
            {
                int sizeDif = output.Length - beginningSize;
                String cleanedMatchValue = HttpUtility.HtmlDecode(match.Groups[0].Value).Trim();
                output = output.Substring(0, match.Groups[0].Index + sizeDif) +
                    "<a href=\"" + cleanedMatchValue + "\" target=\"_blank\">" + cleanedMatchValue + "</a>" +
                    output.Substring(match.Groups[0].Index + match.Groups[0].Length + sizeDif);
            }

            // Second, linkify URLs without the protocol specified, assume http.
            regex = new Regex(@"(?<!(?:http://|ftp://|href=[""']?|src=[""']?|<a[^>]*>)[^.'""]*[\s]*)\b(?:www\.|ftp\.)[-A-Z0-9+&;@#/%=~_|$?!:,.]*[A-Z0-9+&;@#/%=~_|$]", RegexOptions.IgnoreCase);
            beginningSize = output.Length;
            matches = regex.Matches(output);
            foreach (Match match in matches)
            {
                int sizeDif = output.Length - beginningSize;
                String cleanedMatchValue = HttpUtility.HtmlDecode(match.Groups[0].Value).Trim();
                output = output.Substring(0, match.Groups[0].Index + sizeDif) +
                    "<a href=\"http://" + cleanedMatchValue + "\" target=\"_blank\">" + cleanedMatchValue + "</a>" +
                    output.Substring(match.Groups[0].Index + match.Groups[0].Length + sizeDif);
            }
            return output;
        }

        public static string ResolvePath(String relativePath)
        {
            string root = ConfigurationManager.AppSettings["SiteRoot"];
            if (relativePath != null && relativePath.Trim().Length > 0)
                return root + "/" + relativePath;
            else
                return root;
        }

        public static HtmlString RenderComments(List<Comment> thread)
        {
            StringBuilder html = new StringBuilder();

            // Get a list of unread items and convert it to a dictionary for easy searching when building comments.
            PalaverDb db = new PalaverDb();
            int? subjectId = thread[0].SubjectId;
            List<UnreadItem> unreadList = db.UnreadItems.Include("Comment").Where(r => r.User.UserId == CodeFirstMembership.CodeFirstSecurity.CurrentUserId && r.Comment.SubjectId == subjectId).ToList<UnreadItem>();
            List<int> unreadItems = new List<int>();
            foreach (UnreadItem unread in unreadList)
                unreadItems.Add(unread.Comment.CommentId);

            html.AppendLine("<ul class=\"commentlist\">");

            html.AppendLine(BuildComments(thread[0], unreadItems));
            
            html.AppendLine("</ul>");

            return new HtmlString(html.ToString());
        }

		public static string BuildComments(Comment comment)
		{
			return BuildComments(comment, null);
		}

        public static string BuildComments(Comment comment, List<int> unreadItems)
        {
            StringBuilder html = new StringBuilder();
            bool newComment = false;
            
            html.AppendFormat("<li class=\"comment\" data-id=\"{0}\">\n", comment.CommentId);
            html.AppendFormat("<div data-id=\"{0}\" data-parent-id=\"{1}\" data-subject-id=\"{2}\" tabindex=\"99\" class=\"", comment.CommentId,
                (!comment.ParentCommentId.HasValue ? comment.CommentId : comment.ParentCommentId), comment.SubjectId);

            // unread comment?
            if (comment.User.UserId != CodeFirstMembership.CodeFirstSecurity.CurrentUserId)
            {
                if (unreadItems != null && unreadItems.Contains(comment.CommentId))
                {
                    html.Append("newcomment ");
                    newComment = true;
                }
            }

            html.Append("comment\"" + ( newComment ? " onclick=\"markRead(this)\"" : "" ) + ">");
            html.AppendFormat("<span class=\"user\">{0}</span><span class=\"commentTime\">[{1}]</span><p>", comment.User.Username, comment.LastUpdatedTime.ToLocalTime().ToShortDateString() +
                " " + comment.LastUpdatedTime.ToLocalTime().ToShortTimeString());
            html.Append(comment.Text + "\n</p>\n</div>\n");

            html.AppendLine("<ul class=\"commentlist\">");
            if (comment.Comments != null && comment.Comments.Count > 0)
            {

                foreach (Comment c in comment.Comments)
                {
                    html.Append(BuildComments(c, unreadItems));   
                }

            }
            html.AppendLine("</ul>");
            html.AppendFormat("<div title=\"Reply\" class=\"reply\" onclick=\"WriteReply({0})\"><HR class=\"reply\" /></div>",  comment.CommentId);
            html.AppendLine("</li>");

			return html.ToString();
        }

        public static string BuildThreads(IEnumerable<Comment> threads)
        {
            StringBuilder html = new StringBuilder();

            // Get our unread counts by thread id.
            PalaverDb db = new PalaverDb();
            Dictionary<int, int> unreadCounts = db.GetUnreadCommentTotals(CodeFirstMembership.CodeFirstSecurity.CurrentUserId);

            html.AppendLine("<ul>");

            foreach (Comment c in threads)
            {
                html.AppendLine(BuildThread(c, unreadCounts));
            }

            html.AppendLine("</ul>");

            return html.ToString();
        }

        public static string BuildThread(Comment comment, Dictionary<int, int> unreadCounts)
        {
            int unreadCount = 0;
            if (unreadCounts != null && unreadCounts.ContainsKey((int)comment.SubjectId))
                unreadCount = unreadCounts[(int)comment.SubjectId];

            String rootUrl = HttpContext.Current.Request.ApplicationPath;
            if (!rootUrl.EndsWith("/"))
                rootUrl += "/";
            if (unreadCount > 0)
                return string.Format("<li data-thread-id=\"{0}\" class=\"newcomments\" onclick=\"GetComments({0})\">" +
                    "<span class=\"threadTime\">[{3}]</span> - <a href=\"javascript:;\">{1}</a>" +
                    "&nbsp;<span id=\"newCommentsCount{0}\" class=\"threadNewComments\">({2})</span>" +
                    // Unsubscribe disabled for now.
                    /*
                    "<img src=\"" + rootUrl + "Content/images/Delete-icon.png\" height=\"14px\" " +
                    "alt=\"Unsubscribe from Thread\" onclick=\"unsubscribe(event, {0})\" class=\"unsubscribe\" />" +
                    */
                    "</li>",
                    comment.CommentId, comment.Text, unreadCount, getDisplayTime(comment.LastUpdatedTime.ToLocalTime()), rootUrl);
            else
                return string.Format("<li data-thread-id=\"{0}\" onclick=\"GetComments({0})\"><span class=\"threadTime\">[{3}]</span> - <a  href=\"javascript:;\">{1}</a>" +
                    "&nbsp;<span id=\"newCommentsCount{0}\" class=\"threadNewComments\"></span>" +
                    // Unsubscribe disabled for now.
                    /*
                    "<img src=\"" + rootUrl + "Content/images/Delete-icon.png\" height=\"14px\" " +
                    "alt=\"Unsubscribe from Thread\" onclick=\"unsubscribe(event, {0})\" class=\"unsubscribe\" />" +
                    */
                    "</li>",
                    comment.CommentId, comment.Text, unreadCount, getDisplayTime(comment.LastUpdatedTime.ToLocalTime()), rootUrl);
        }

        public static string BuildThreadsMobile(List<Comment> threads)
        {
            StringBuilder html = new StringBuilder();

            // Get our unread counts by thread id.
            PalaverDb db = new PalaverDb();
            Dictionary<int, int> unreadCounts = db.GetUnreadCommentTotals(CodeFirstMembership.CodeFirstSecurity.CurrentUserId);

            foreach (Comment c in threads)
                html.AppendLine(BuildThreadMobile(c, unreadCounts));

            return html.ToString();
        }

        public static string BuildThreadMobile(Comment comment, Dictionary<int, int> unreadCounts)
        {
            StringBuilder html = new StringBuilder();

            int unreadCount = 0;
            if (unreadCounts != null && unreadCounts.ContainsKey((int)comment.SubjectId))
                unreadCount = unreadCounts[(int)comment.SubjectId];


            String rootUrl = HttpContext.Current.Request.ApplicationPath;
            if (!rootUrl.EndsWith("/"))
                rootUrl += "/";

            html.AppendLine("<li data-theme=\"c\">");
            if (unreadCount > 0)
                html.AppendFormat("<a data-thread-id=\"{0}\" class=\"newcomments\" href=\"" + rootUrl + "Thread/{0}\" data-transition=\"slide\">[{3}] - {1} ({2})</a>",
                    comment.CommentId, comment.Text, unreadCount, getDisplayTime(comment.LastUpdatedTime.ToLocalTime()), rootUrl);
            else
                html.AppendFormat("<a data-thread-id=\"{0}\" href=\"" + rootUrl + "Thread/{0}\" data-transition=\"slide\">[{3}] - {1}</a>",
                    comment.CommentId, comment.Text, unreadCount, getDisplayTime(comment.LastUpdatedTime.ToLocalTime()), rootUrl);
            html.AppendLine("</li>");

            return html.ToString();
        }

        public static HtmlString RenderCommentsMobile(int? ThreadId)
        {
            StringBuilder html = new StringBuilder();

            // Get our comments.
            PalaverDb db = new PalaverDb();
            List<Comment> thread = db.Comments.Include("Comments").Include("User").Where(x => x.SubjectId == ThreadId).OrderBy(x => x.CreatedTime).ToList();

            // Get a list of unread items and convert it to a dictionary for easy searching when building comments.
            int? subjectId = thread[0].SubjectId;
            List<UnreadItem> unreadList = db.UnreadItems.Include("Comment").Where(r => r.User.UserId == CodeFirstMembership.CodeFirstSecurity.CurrentUserId && r.Comment.SubjectId == subjectId).ToList<UnreadItem>();
            List<int> unreadItems = new List<int>();
            foreach (UnreadItem unread in unreadList)
                unreadItems.Add(unread.Comment.CommentId);

            html.AppendLine(BuildCommentsMobile(thread[0], unreadItems));

            return new HtmlString(html.ToString());
        }

        public static string BuildCommentsMobile(Comment comment, List<int> unreadItems)
        {
            StringBuilder html = new StringBuilder();
            bool newComment = false;

            //html.AppendFormat("<li data-theme=\"c\" class=\"comment\" data-id=\"{0}\">\n", comment.CommentId);
            html.AppendFormat("<div data-id=\"{0}\" data-parent-id=\"{1}\" data-subject-id=\"{2}\" tabindex=\"99\" class=\"", comment.CommentId,
                (!comment.ParentCommentId.HasValue ? comment.CommentId : comment.ParentCommentId), comment.SubjectId);

            // unread comment?
            if (comment.User.UserId != CodeFirstMembership.CodeFirstSecurity.CurrentUserId)
            {
                if (unreadItems != null && unreadItems.Contains(comment.CommentId))
                {
                    html.Append("newcomment ");
                    newComment = true;
                }
            }

            html.Append("comment\"" + (newComment ? " onclick=\"markRead(this)\"" : "") + ">");
            html.AppendFormat("<span class=\"user\">{0}</span><span class=\"commentTime\">[{1}]</span><p>", comment.User.Username, comment.LastUpdatedTime.ToLocalTime().ToShortDateString() +
                " " + comment.LastUpdatedTime.ToLocalTime().ToShortTimeString());
            html.Append(comment.Text + "\n</p>\n</div>\n");


            //html.AppendLine("<ul data-role=\"listview\" data-divider-theme=\"b\" data-inset=\"true\" class=\"commentlist\">");
            html.AppendLine("<div class=\"childComment\">");
            if (comment.Comments != null && comment.Comments.Count > 0)
            {

                foreach (Comment c in comment.Comments)
                {
                    html.Append(BuildCommentsMobile(c, unreadItems));
                }

            }
            html.AppendLine("</div>");
            //html.AppendLine("</ul>");
            //html.AppendLine("</li>");

            return html.ToString();
        }

        private static string getDisplayTime(DateTime time)
        {
            if (DateTime.Today == time.Date)
                return time.ToShortTimeString();
            else
                return time.ToShortDateString();
        }
    }
}
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

using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using Palaver2.Models;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using CodeFirstMembership.Models;
using CodeFirstMembership;

[HubName("MessageHub")]
public class MessageHub : Hub
{
    PalaverDb db = new PalaverDb();
    static Dictionary<string, Guid> userIdsByConnection = new Dictionary<string, Guid>();

    public override Task OnConnected()
    {
		userIdsByConnection.Add(Context.ConnectionId, db.Users.Find( (new User { UserId = 
			CodeFirstSecurity.GetUserId(Context.User.Identity.Name) }).UserId ).UserId);
        System.Diagnostics.Debug.WriteLine("Connected ConnectionId: " + Context.ConnectionId);
        return base.OnConnected();
    }

    public override Task OnDisconnected()
    {
        userIdsByConnection.Remove(Context.ConnectionId);
        System.Diagnostics.Debug.WriteLine("Disconnected ConnectionId: " + Context.ConnectionId);
        return base.OnDisconnected();
    }

    public override Task OnReconnected()
    {
		userIdsByConnection.Add(Context.ConnectionId, db.Users.Find( (new User { UserId = 
			CodeFirstSecurity.GetUserId(Context.User.Identity.Name) }).UserId ).UserId);
        System.Diagnostics.Debug.WriteLine("Reconnected ConnectionId: " + Context.ConnectionId);
        return base.OnReconnected();
    }

    /// <summary>
    /// Create a new thread.
    /// Creating a thread will subscribe all users to it and notify any connected clients.
    /// </summary>
    /// <param name="threadSubject">Text of the thread subject.</param>
    public void NewThread(string threadSubject)
    {
		User currentUser = db.Users.Find( (new User { UserId = CodeFirstSecurity.GetUserId(Context.User.Identity.Name) }).UserId );
        Comment comment = new Comment(threadSubject, currentUser);

        db.Comments.Add(comment);
        db.SaveChanges();

        // For new threads, subscribe everyone and add the first entry to everyone's
        // unread list.
		foreach (User uu in db.Users)
        {
            // db.Subscriptions.Add(new Subscription { User = uu, Subject = comment });
            if (uu.UserId != currentUser.UserId)
                db.UnreadItems.Add(new UnreadItem { User = uu, Comment = comment });
        }

        comment.SubjectId = comment.CommentId;
        db.SaveChanges();

        string html = Palaver2.Helpers.CustomHtmlHelpers.BuildThread(comment, null);

        Message sendMessage = new Message()
        {
            text = html,
            commentId = comment.CommentId,
            userid = currentUser.UserId.ToString(),
            threadId = comment.SubjectId,
            authorName = comment.User.Username
        };

        Clients.All.addThread(sendMessage);
    }

    /// <summary>
    /// Create a new reply in an existing thread.
    /// Adding a reply to an existing thread will notify all subscribed and connected clients.
    /// </summary>
    /// <param name="parentId">Id of the parent comment.</param>
    /// <param name="replyText">Text of the reply.</param>
    public void NewReply(int parentId, string replyText)
    {
		//User currentUser = db.Users.Find( (new User { UserId = CodeFirstSecurity.GetUserId(Context.User.Identity.Name) }).UserId );
		User currentUser = db.Users.Find( CodeFirstSecurity.GetUserId(Context.User.Identity.Name) );
        Comment comment = new Comment(replyText, currentUser);
        Comment parentComment = db.Comments.Include("Comments").First(pc => pc.CommentId == parentId);

        if (parentComment.SubjectId != null)
            comment.SubjectId = parentComment.SubjectId;
        else
            comment.SubjectId = parentComment.CommentId;

        comment.ParentCommentId = parentComment.CommentId;

        Comment thread = db.Comments.Find(comment.SubjectId);
        thread.LastUpdatedTime = DateTime.UtcNow;

        parentComment.Comments.Add(comment);

		foreach (User uu in db.Users)
		{
			if (uu.UserId != currentUser.UserId)
				db.UnreadItems.Add(new UnreadItem { User = uu, Comment = comment });
		}
		/*
        // For all subscribed users other than the current user, mark this comment as unread.
        List<Guid> subscribers = new List<Guid>();
        foreach (Subscription subscriber in db.Subscriptions.Include("User").Where(x => x.Subject.CommentId == comment.SubjectId))
        {
            subscribers.Add(subscriber.User.UserId);
            if (subscriber.User.UserId != currentUser.UserId)
                db.UnreadItems.Add(new UnreadItem { User = subscriber.User, Comment = comment });
        }
		*/

        db.SaveChanges();

        Message sendMessage = new Message()
        {
            text = Palaver2.Helpers.CustomHtmlHelpers.BuildComments(comment, null),
            parentId = parentId,
            commentId = comment.CommentId,
            userid = currentUser.UserId.ToString(),
            threadId = comment.SubjectId,
            authorName = comment.User.Username
        };

        // Loop through the connections and if they're subscribed to this
        // thread then update them.
//        foreach (String connectionId in userIdsByConnection.Keys)
//        {
            //if (subscribers.Contains(userIdsByConnection[connectionId]))
                //Clients.Client(connectionId).addReply(sendMessage);
			Clients.All.addReply(sendMessage);
//        }
    }

    /// <summary>
    /// Unsubscribe the current user from the specified thread.
    /// </summary>
    /// <param name="threadId"></param>
	/*
    public void Unsubscribe(int threadId)
    {
        User currentUser = db.Users.Find(CodeFirstSecurity.GetUserId(Context.User.Identity.Name));
        Subscription sub = db.Subscriptions.Where(x => x.Subject.CommentId == threadId && x.User.UserId == currentUser.UserId).Single();
        db.Subscriptions.Remove(sub);
        db.SaveChanges();
    }
    */

    class Message
    {
        public int? threadId { get; set; }
        public int parentId {get; set; }
        public int commentId { get; set; }
        public string text { get; set; }
        public string userid { get; set; }
        public string authorName { get; set; }
    }
}
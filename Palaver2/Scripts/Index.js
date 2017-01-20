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

/*  Functionality just for the Index.cshtml page */
var _editorDefaultHeight = 100;
var _baseUrl = "";
var _srConnection;
var _allowBack = false;
var _isSafariMobile = isSafariMobile();
var NOTIFICATION_SNIPPET_SIZE = 100;
var NOTIFICATION_DURATION = 5000;
var IPAD_SIGNALR_DELAY = 200;
var wowhead_tooltips = { "colorlinks": true, "iconizelinks": true, "renamelinks": true };

// Establish state variables
var History = window.History, // Note: We are using a capital H instead of a lower h
	State = History.getState();

function initPage(baseUrl, threadId, commentId) {
    var currentThreadId;

    _baseUrl = baseUrl;
    _initialCommentId = commentId;

    // Setup layouts.
    addLayouts();

    // Bind to State Change
    if (History.enabled) {
        setTimeout(function () {
            History.Adapter.bind(window, 'popstate', function () { // Note: We are using statechange instead of popstate
                var State = History.getState(); // Note: We are using History.getState() instead of event.state
                if (_allowBack) {
                    if (State.data.threadId != null) {
                        if (State.data.commentId != null)
                            _initialCommentId = State.data.commentId;
                        GetComments(State.data.threadId, true);
                    }
                    else
                        $('#comments').html('');
                }
            });
        }, 1000);
    }

    // Register our primary key event handler for the page.
    $(document).keydown(pageKeyDown);

    // Setup signalr connection.
    startSignalr();

    // Load the selected thread if there is one.
    if (threadId != null)
        GetComments(threadId);

    // Update the page title.
    updateTitle();

    // Default links to target _blank
    CKEDITOR.on('dialogDefinition', function (ev) {
        // Take the dialog name and its definition from the event data.
        var dialogName = ev.data.name;
        var dialogDefinition = ev.data.definition;

        // Check if the definition is from the dialog we're
        // interested on (the Link dialog).
        if (dialogName == 'link') {
            // FCKConfig.LinkDlgHideAdvanced = true
            dialogDefinition.removeContents('advanced');
            // FCKConfig.LinkDlgHideTarget = true
            //dialogDefinition.removeContents('target');

            //Enable this part only if you don't remove the 'target' tab in the previous block.
 
            // FCKConfig.DefaultLinkTarget = '_blank'
            // Get a reference to the "Target" tab.
            var targetTab = dialogDefinition.getContents( 'target' );
            // Set the default value for the URL field.
            var targetField = targetTab.get( 'linkTargetType' );
            targetField[ 'default' ] = '_blank';
        }
    });

    // Set up hover for the unsubscribe buttons for threads.
    // Disabled for now.
    /*
    $('div#threads .unsubscribe').hide();
    $('div#threads ul li').hover(function () {
        $(this).children('.unsubscribe').show();
    },
    function () {
        $(this).children('.unsubscribe').hide();
    });
    */

    // Load wowhead script after the page is loaded to stop it from blocking.
    $.getScript('//wow.zamimg.com/widgets/power.js');
}

function showDisconnected()
{
	$.blockUI({
	   	message: 'Disconnected.  Attempting to reconnect...',
    	css: {
	        border: 'none',
	        padding: '15px',
	        backgroundColor: '#000',
	        '-webkit-border-radius': '10px',
	        '-moz-border-radius': '10px',
	        opacity: .5,
	        color: '#fff'
	    }
	});
}

function hideDisconnected()
{
	$.unblockUI();
}

function startSignalr()
{
    _srConnection = $.connection.MessageHub;
    _srConnection.client.addThread = addThread;
    _srConnection.client.addReply = addReply;

    $.connection.hub.error(function (error) {
    	if (console)
    	{
    		if (error)
    			console.log(error);
    		else
    			console.log('Unknown SignalR hub error.');
    	}
    });

	$.connection.hub.connectionSlow(function () {
		if (console)
		    console.log('SignalR is currently experiencing difficulties with the connection.')
	});

	$.connection.hub.reconnecting(function () {
		showDisconnected();
		if (console)
			console.log('SignalR connection lost, reconnecting.');
	});

    // Try to reconnect every 5 seconds if disconnected.
    $.connection.hub.disconnected(function () {
        showDisconnected();
    	if (console)
        	console.log('SignlR lost its connection, reconnecting in 5 seconds.');

        setTimeout(function () {
        	if (console)
            	console.log('SignlR delayed reconnection in progress.');
            startHub();
        }, 5000); // Restart connection after 5 seconds.
    });

    $.connection.hub.reconnected(function() {
    	if (console)
    		console.log('SignalR reconnected.');

    	triedReconnecting = false;
    	hideDisconnected();
    });

    $.connection.hub.logging = true;
    if (!_isSafariMobile)
    	startHub();
    else {
        // Delay the start to avoid breaking safari mobile.
        window.setTimeout(function () { startHub() }, IPAD_SIGNALR_DELAY);
    }
}

function startHub()
{
	$.connection.hub.start({ transport: [ 'foreverFrame', 'longPolling' ] }).done(function () {
		triedReconnecting = false;
		hideDisconnected();
		if (console)
		    console.log("Connected, transport = " + $.connection.hub.transport.name);
	});
}

function addLayouts() {
    myLayout = $('.page').layout({
			west__size:			400,
            north__size:        70,
            south__size:         30,
            south__resizable:    false,
            north__resizable: false,
            north__onclose_start: function () {
                $('#themeContainer').hide();
            },
            north__onopen_end: function () {
                $('#themeContainer').show();
            }
        });

	// THEME SWITCHER
    addThemeSwitcher('.ui-layout-north', { top: '25px', right: '5px' });
	// if a new theme is applied, it could change the height of some content,
	// so call resizeAll to 'correct' any header/footer heights affected
	// NOTE: this is only necessary because we are changing CSS *AFTER LOADING* using themeSwitcher
	setTimeout( myLayout.resizeAll, 100 ); /* allow time for browser to re-render with new theme */
}

/**
*	addThemeSwitcher
*
*	Remove the cookie set by the UI Themeswitcher to reset a page to default styles
*
*	Dependancies: /Scripts/themeswitchertool.js
*/
function addThemeSwitcher(container, position) {
    var pos = { top: '10px', right: '10px', "z-Index": 100, position: 'absolute' };
    $('<div id="themeContainer"><div id="themeSwitcher" style="position: absolute; z-Index: 100"></div></div>')
		.css($.extend(pos, position))
		.appendTo('body');
    $('#themeSwitcher').themeswitcher({
            imgpath: _baseUrl + "Content/images/themeswitcher/",
            loadTheme: "ui-lightness",
            jqueryUiVersion: "1.10.0"
		});
};

function addThread(thread)
{
    $('#threads ul').prepend(thread.text);
    // Unsubscribe disabled for now.
    /*
    $('#threads ul .unsubscribe').first().hide();
    $('div#threads ul li').first().hover(function () {
         $(this).children('.unsubscribe').show();
        $(this).children('.unsubscribe').show();
    },
    function () {
        $(this).children('.unsubscribe').hide();
    });
    */

    var user = $('#userid').val();
    if (user != thread.userid) {
        var newthread = $('li[data-thread-id="' + thread.commentId + '"]');
        $(newthread).addClass('newcomments');
        $('#newCommentsCount' + thread.commentId).html('(1)');
        notify('Palaver thread posted by ' + thread.authorName + '.', thread.text, thread.threadId, thread.commentId);
    }
    else {
        window.location.href = _baseUrl + "Thread/" + thread.commentId;
    }
    updateTitle();
}

function addReply(reply)
{
    // get the comment's parent
    var commentItem = $('li[data-id="' + reply.parentId + '"]')[0];
    // Add the comment to the parent.
    $(commentItem).children('ul').append(reply.text);

    var user = $('#userid').val();
    var newcomment = $('div[data-id="' + reply.commentId + '"]')[0];
    if (user != reply.userid) {
        $(newcomment).addClass('newcomment');
        $(newcomment).click(function () {
            markRead(this);
        });
        // Update the new comment count for the thread.
        incrementThreadUnread(reply.threadId);
        notify('Palaver comment posted by ' + reply.authorName + '.', reply.text, reply.threadId, reply.commentId);
    }
    else {
        $(newcomment).focus();
    }
    // Move the new comment's thread to the top of the thread list.
    var thread = $('li[data-thread-id="' + reply.threadId + '"]');
    $(thread).prependTo('div#threads ul');
    // Update the thread's time.
    var now = new Date();
    var hour = now.getHours(), minutes, ampm = 'AM';
    if (hour == 0)
        hour = 12;
    else if (hour == 12)
        ampm = 'PM';
    else if (hour > 12) {
        hour -= 12;
        ampm = 'PM';
    }
    if (now.getMinutes() < 10)
        minutes = '0' + now.getMinutes();
    else
        minutes = now.getMinutes().toString();
    $(thread).children('.threadTime').html('[' + hour + ':' + minutes + ' ' + ampm + ']');
    updateTitle();
}

// Notify the user of new comments/threads
function notify(title, message, threadId, commentId) {
	if (!Notification || Notification.permission !== "granted")
		return;

    // Strip the message of any HTML using a temporary div.
    var tempDiv = document.createElement('DIV');
    tempDiv.innerHTML = (message == null ? '' : '<ul>' + message + '</ul>');
    var innerDiv = $(tempDiv).find('div')[0];
    $(innerDiv).children('span.user').remove();
    $(innerDiv).children('span.commentTime').remove();
    var filteredMessage = innerDiv.textContent || innerDiv.innerText;
    filteredMessage = $.trim(filteredMessage.substring(0, NOTIFICATION_SNIPPET_SIZE));
    var notification = new Notification( title, {
    	icon: _baseUrl + 'Content/images/new_message-icon.gif',
    	body: filteredMessage
    });
    notification.onclick = function () {
        window.focus();
        _initialCommentId = commentId;
        GetComments(threadId);
        this.cancel();
    };
    setTimeout(function () { if (notification) notification.close(); }, NOTIFICATION_DURATION);
}

function updateTitle() {
    var threadCounts = $('.threadNewComments');
    var totalUnread = 0;

    // Loop through the thread counters and total them.
    if (threadCounts.length > 0) {
        for (var x = 0; x < threadCounts.length; x++) {
            var countString = $(threadCounts[x]).html();
            if (countString != null && countString != '') {
                totalUnread += parseInt(countString.replace('(', '').replace(')', ''));
            }
        }
    }

    if (totalUnread > 0)
        document.title = '*' + totalUnread + '* Palaver';
    else
        document.title = 'Palaver';
}

function GetThreads() {
    $.get(_baseUrl + 'home/GetThreads',
        null,
        function (payload) {
            $('#threads').html(payload);
            updateTitle();
        }
    );
}

function markRead(comment){
    if ($(comment).hasClass('newcomment')) {
        $(comment).removeClass('newcomment');

        var id = $(comment).attr("data-id");
        var data = { 'id': id };

        updateThreadUnread($(comment).attr("data-subject-id"));

        jQuery.get(_baseUrl + 'home/MarkRead', data);
    }
    updateTitle();
}

function updateThreadUnread(threadId) {
    // First, get our current unread count.
    var unreadCount = $('.newcomment').length;

    // If the count is greater than 0, simply update the unread counter next to the thread.
    // Otherwise, clear the unread counter for the thread.
    if (unreadCount > 0)
        $('#newCommentsCount' + threadId).html('(' + unreadCount + ')');
    else {
        $('#newCommentsCount' + threadId).html('');
        $('li[data-thread-id="' + threadId + '"]').removeClass('newcomments');
    }

    // Update the page title with the unread count.
    updateTitle();
}

function incrementThreadUnread(threadId) {
    // First, get the thread's unread count span.
    var threadCounter = $('#newCommentsCount' + threadId);
    // If the counter is empty, set it to (1)
    if (threadCounter.html() == null || threadCounter.html() == '')
        threadCounter.html('(1)');
    else {
        // Convert the counter to an int and increment it.
        var count = parseInt(threadCounter.html().replace('(', '').replace(')', '')) + 1;
        // Update the page display.
        threadCounter.html('(' + count + ')');
    }

    // Make sure that thread has the newcomments class.
    $('li[data-thread-id="' + threadId + '"]').addClass('newcomments');

    // Update the page title with the unread count.
    updateTitle();
}

function GetComments(id) {
    GetComments(id, false);
}

function GetComments(id, isBack) {

    var thread = {
        'id': id
    };

    currentThreadId = id;

    // If we have pushState() available, use that + Ajax to open our comments.
    // Otherwise, use a redirect.

    if (!History.enabled) {
        window.location.href = _baseUrl + 'Thread/' + id;
        return;
    } else {
        // Change our URL.
        if (!isBack) {
            _allowBack = false;
            if (_initialCommentId != null)
                History.pushState({ threadId: id, commentId: _initialCommentId }, document.title, _baseUrl + 'Thread/' + id + '/' + _initialCommentId);
            else
                History.pushState({ threadId: id }, document.title, _baseUrl + 'Thread/' + id);
            _allowBack = true;
        }
        // Blank out current comments change the class to commentsLoading.
        $(document).addClass('busy');

        $('#comments').html('');
        $.get(
            _baseUrl + 'home/GetComments',
            thread,
            function (data) {
                $('#comments').html(data);
                $('.newcomment').click(function () {
                    markRead(this);
                });
                $(document).removeClass('busy');
                if (_initialCommentId != null) {
                    focusAndMarkReadCommentId(_initialCommentId);
                    _initialCommentId = null;
                }
                selectThread(id);
                updateTitle();
            }
        );
    }
}

function selectThread(threadId) {
    $('.selectedThread').removeClass('selectedThread');
    $('li[data-thread-id="' + threadId + '"]').addClass('selectedThread');
}

function WriteReply(id) {
    // Close other reply divs.
    try {
        $('#replyBox').ckeditorGet().destroy();
    }
    catch (ex) {
        ; // Do nothing.
    }
    $('#replyDiv').remove();

    // Create a new reply div.
    $('li[data-id="' + id + '"]').append('<div id="replyDiv" style="margin-left: 20px;" onkeypress="replyKeyPressed(event,' + id + ')"><textarea id="replyBox" style="height: 15px; width: 100%; margin:0;" /><a id="sendReplyLink" tabindex="1" href="javascript:;" onclick="SendReply(' + id + ')" class="submit">submit</a><a tabindex="2" href="javascript:;" onclick="CancelReply()">cancel</a></div>');
    $('#replyBox').ckeditor();
    $('#replyBox').ckeditorGet().on('key', function (e) { replyKeyPressed(e, id); });
    var currScrollTop = $('#commentsArea').scrollTop();
    var scrollNeeded = currScrollTop + $('#sendReplyLink').position().top + $('#sendReplyLink').height() - ($('#commentsArea').offset().top + $('#commentsArea').height());
    if ( scrollNeeded > currScrollTop )
        $('#commentsArea').scrollTop(scrollNeeded);
}

function isSafariMobile() {
    if (navigator && navigator.userAgent && (navigator.userAgent.toLowerCase().indexOf('ipad') >= 0 || navigator.userAgent.toLowerCase().indexOf('iphone') >= 0
        || navigator.userAgent.toLowerCase().indexOf('ipod') >= 0))
        return true;
    else
        return false;
}

function replyKeyPressed(e, commentId) {
    // Save if shift+enter is pressed.
    if (e.data.keyCode == 2228237) {
        SendReply(commentId);
        return false;
    }
    else if (e.data.keyCode == 27)
    {
        CancelReply();
        return false;
    }
    return true;
}

function SendReply(commentId) {
    var text = $('#replyBox').val();
    $('#replyBox').ckeditorGet().destroy();
    $('#replyDiv').remove();

    // Make sure all URLs in the reply have a target.  If not, set it to _blank.
    // We're doing this by using a fake DIV with jquery to find the links.
    var tempDiv = document.createElement('DIV');
    tempDiv.innerHTML = (text == null ? '' : text);
    var links = $(tempDiv).children('a');
    for (var x = 0; x < links.length; x++)
        if (!$(links[x]).attr('target'))
            $(links[x]).attr('target', '_blank');
    text = tempDiv.innerHTML;

    if (!_isSafariMobile)
        _srConnection.server.newReply(commentId, text);
    else {
        // Send the message in 50ms so Safari mobile won't get stuck in loading mode.
        window.setTimeout(function () { _srConnection.server.newReply(commentId, text); }, IPAD_SIGNALR_DELAY);
    }
}

function CancelReply() {
    $('#replyBox').ckeditorGet().destroy();
    $('#replyDiv').remove();
}

function AddComment() {
    var text = $('#newcomment').val();

    $('#newcomment').val('')

    if (!_isSafariMobile)
        _srConnection.server.newThread(text);
    else {
        // Send the message in 50ms so Safari mobile won't get stuck in loading mode.
        window.setTimeout(function () { _srConnection.server.newThread(text); }, IPAD_SIGNALR_DELAY);
    }
}

// Unsubscribe disabled for now.
/*
function unsubscribe(e, threadId) {
    if (!e)
        e = window.event;

    // Prevent the parent div event from firing.
    // Good browsers
    if (e.stopPropagation) {
        e.stopPropagation();
    }
    // Shit browsers
    else {
        e.cancelBubble = true;
    }

    $(this).hide();

    if (!_isSafariMobile)
        _srConnection.server.unsubscribe(threadId).done(function () { window.location.href = _baseUrl; });
    else {
        // Send the message in 50ms so Safari mobile won't get stuck in loading mode.
        window.setTimeout(function () { _srConnection.server.unsubscribe(threadId).done(function () { window.location.href = _baseUrl; }); }, IPAD_SIGNALR_DELAY);
    }
}
*/

function pageKeyDown(e) {
    // If we're entering a new comment, skip checking for unread.
    var focusedInput = $('input:focus');
    if ( focusedInput == null || focusedInput.length == 0 ) {
        if ( e.keyCode == 32) { // space
            goToNextUnread();
            return false;
        }
        else if ( e.keyCode == 82 ) { // 'r'
            // Find the selected comment if there is one.
            // If not, find the first comment.
            var focusedComment = $('.comment:focus');
            var comment;
            if ( focusedComment != null && focusedComment.length > 0 )
                comment = focusedComment[0];
            else
            {
                comments = $('.comment');
                if ( comments != null && comments.length > 0 )
                    comment = comments[0];
                else
                    return false;  // No comments, do nothing.
            }

            // Reply to this comment if shift isn't pressed.
            // Indented reply if shift is pressed.
            if ( e.shiftKey == false ) {
                var parentId = $(comment).attr("data-parent-id");
                WriteReply(parentId);
            } else {
                var id = $(comment).attr("data-id");
                WriteReply(id);
            }
            return false;
        }
        else if ( e.keyCode == 39 || e.keyCode == 40 || e.keyCode == 78 ) { // right, down, 'n'
            focusNext();
            return false;
        } else if ( e.keyCode == 37 || e.keyCode == 38 || e.keyCode == 80 ) { // left, up, 'p'
            focusPrev();
            return false;
        }
    }
    return true;
}

function goToNextUnread() {
    var unreadItems = $('div.newcomment');
    // Exit if there are no unread items.
    if ( unreadItems == null || unreadItems.length == 0 )
        return;
    // If there is only one, focus on that one and mark it read.
    if ( unreadItems.length == 1 ) {
        focusAndMarkRead($(unreadItems[0]));
        return;
    }
    // Get the currently selected item.
    var focusedComments = $('div.comment:focus');
    var focusedId;
    // If we don't have a focused comment, focus and mark the first unread comment as read.
    if ( focusedComments == null || focusedComments.length == 0 ) {
        focusAndMarkRead($(unreadItems[0]));
        return;
    } else
        focusedId = $(focusedComments[0]).attr('data-id');

    // Find the next unread item.
    var nextUnread = findNextUnreadComment(focusedId);
    // If there isn't a next one, just focus & mark the first.
    if ( nextUnread == null )
        focusAndMarkRead($(unreadItems[0]));
    else
        focusAndMarkRead(nextUnread);
}

function focusAndMarkRead(unreadComment)
{
    focusComment(unreadComment);
    markRead(unreadComment);
}

function focusCommentId(commentId) {
    focusComment($('div[data-id="' + commentId + '"]'));
}
function focusAndMarkReadCommentId(commentId) {
    var comment = $('div[data-id="' + commentId + '"]'); 
    focusComment(comment);
    markRead(comment);
}

function focusComment(comment) {
    $('#commentsArea').scrollTop($('#commentsArea').scrollTop() + ( $(comment).position().top - $('#commentsArea').offset().top ));
    $(comment).focus();
}

function focusNext() {
    // Find all comments, exit if there are none.
    var comments = $('div.comment');
    if ( comments == null || comments.length == 0 )
        return;

    // If we only have one comment, focus on that one.
    if ( comments.length == 1 ) {
        $(comments[0]).focus();
        return;
    }

    // Find the first focused comment.
    var focusedComments = $('div.comment:focus');
    var focusedId;
    // If we don't have a focused comment, find the first one and focus that then exit.
    if ( focusedComments == null || focusedComments.length == 0 ) {
        $(comments[0]).focus();
        return;
    } else
        focusedId = $(focusedComments[0]).attr('data-id');

    // Find the next comment after this one and focus it.
    var nextComment = findNextComment(focusedId);
    if ( nextComment != null )
        nextComment.focus();
}

function findNextComment(commentId) {
    // Find all comments, exit if there are none.
    var comments = $('div.comment');
    if ( comments == null || comments.length == 0 )
        return;

    // Loop through our comment divs until we get to the currentId, then return the one after that.
    // If we don't find the Id or there isn't another comment after this one, return null.
    for ( var x = 0; x < comments.length; x++ ) {
        if ( $(comments[x]).attr('data-id') == commentId ) {
            if ( x + 1 < comments.length )
                return $(comments[x+1]);

            // We found the id but there are no comments after, return null.
            return null;
        }
    }

    // No comment matched the pass Id, return null.
    return null;
}

function findNextUnreadComment(commentId) {
    // Find all comments, exit if there are none.
    var comments = $('div.comment');
    if ( comments == null || comments.length == 0 )
        return;

    // Loop through our comment divs until we get to the currentId, then return the one after that.
    // If we don't find the Id or there isn't another comment after this one, return null.
    for ( var x = 0; x < comments.length; x++ ) {
        if ( $(comments[x]).attr('data-id') == commentId ) {
            // We found our selected comment, loop through the rest
            // of the comments until we find one that's unread.
            // If none are, return null.
            for ( var y = x + 1; y < comments.length; y++ ) {
                if ( $(comments[y]).hasClass('newcomment') )
                    return $(comments[y]);
            }

            // We found the id but there are no unread comments after, return null.
            return null;
        }
    }

    // No comment matched the pass Id, return null.
    return null;
}

function focusPrev() {
    // Find all comments, exit if there are none.
    var comments = $('div.comment');
    if ( comments == null || comments.length == 0 )
        return;

    // If we only have one comment, focus on that one.
    if ( comments.length == 1 )
    {
        $(comments[0]).focus();
        return;
    }

    // Find the first focused comment.
    var focusedComments = $('div.comment:focus');
    var focusedId;
    // If we don't have a focused comment, find the last one and focus that then exit.
    if ( focusedComments == null || focusedComments.length == 0 ) {
        comments.last().focus();
        return;
    } else
        focusedId = $(focusedComments[0]).attr('data-id');

    // Loop through our comment divs until we get to our currently selected one, then focus the one before that, if there is one.
    for ( var x = comments.length -1; x > -1; x-- ) {
        if ( $(comments[x]).attr('data-id') == focusedId ) {
            if ( x - 1 > -1 )
                $(comments[x-1]).focus();

            return;
        }
    }
}

/**
 * @license Copyright (c) 2003-2016, CKSource - Frederico Knabben. All rights reserved.
 * For licensing, see LICENSE.md or http://ckeditor.com/license
 */

CKEDITOR.editorConfig = function( config ) {

	// %REMOVE_START%
	// The configuration options below are needed when running CKEditor from source files.
	config.plugins = 'dialogui,dialog,about,a11yhelp,dialogadvtab,basicstyles,bidi,blockquote,clipboard,button,panelbutton,panel,' +
		'floatpanel,colorbutton,colordialog,templates,menu,div,resize,toolbar,elementspath,enterkey,entities,popup,' +
		'fakeobjects,listblock,richcombo,font,forms,format,horizontalrule,htmlwriter,' +
		'wysiwygarea,image,indent,indentblock,indentlist,justify,menubutton,link,list,magicline,' +
		'pastetext,preview,removeformat,save,selectall,showborders,' +
		'stylescombo,tab,table,undo,lineutils,widget,autogrow,youtube,autocorrect,autolink';
	// Removed codeTag,filebrowser,flash,find,floatingspace,iframe,smiley,language,maximize,newpage,pagebreak,pastefromword,print
	// showblocks,showborders,sourcearea,specialchar,scayt,wsc,contextmenu,tabletools,liststyle
	config.skin = 'moono';
	config.toolbar = [
		{ name: 'basicstyles', items: [ 'Bold', 'Italic', 'Underline', 'Strike', '-', 'RemoveFormat' ] },
		{ name: 'paragraph', items: [ 'NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight' ] },
		{ name: 'insert', items: [ 'Link', 'Unlink', 'Paste', 'PasteText', 'Image', 'Youtube' ] },
		{ name: 'styles', items: [ 'FontSize', 'TextColor', 'BGColor' ] },
		{ name: 'other', items: [ 'AutoCorrect' ] }
	];
	// %REMOVE_END%

	config.disableNativeSpellChecker = false;
	config.startupFocus = true;
	config.toolbarCanCollapse = true;
    config.extraPlugins = (_isSafariMobile ? '' : 'autogrow,') + 'find,image';
    config.removePlugins = (_isSafariMobile ? 'autogrow,' : '') + 'elementspath,tab';
    config.autoGrow_onStartup = false;
    config.autoGrow_minHeight = 100;
    config.resize_enabled = false;
    config.enableTabKeyTools = false;
    config.defaultLanguage = 'en';
    config.height = 100;

	config.autocorrect_enabled = true;
	config.autocorrect_replaceHyphens = false;
	config.autocorrect_replaceSingleQuotes = false;
	config.autocorrect_replaceDoubleQuotes = false;

	// Define changes to default configuration here. For example:
	// config.language = 'fr';
	// config.uiColor = '#AADC6E';
};

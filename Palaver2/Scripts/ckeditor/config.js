/**
 * @license Copyright (c) 2003-2016, CKSource - Frederico Knabben. All rights reserved.
 * For licensing, see LICENSE.md or http://ckeditor.com/license
 */

CKEDITOR.editorConfig = function (config) {

	// %REMOVE_START%
	// The configuration options below are needed when running CKEditor from source files.
	config.plugins = 'dialogui,dialog,about,a11yhelp,dialogadvtab,basicstyles,bidi,blockquote,clipboard,button,panelbutton,panel,' +
		'floatpanel,colorbutton,colordialog,templates,menu,div,resize,toolbar,enterkey,entities,popup,' +
		'fakeobjects,listblock,richcombo,font,forms,format,horizontalrule,htmlwriter,' +
		'wysiwygarea,image,indent,indentblock,indentlist,justify,menubutton,link,list,magicline,' +
		'pastetext,preview,removeformat,save,selectall,showborders,' +
		'stylescombo,table,undo'; //,lineutils,widget,autogrow,youtube,autolink';
	// Removed codeTag,filebrowser,flash,find,floatingspace,iframe,smiley,language,maximize,newpage,pagebreak,pastefromword,print
	// showblocks,showborders,sourcearea,specialchar,scayt,wsc,contextmenu,tabletools,liststyle
	config.skin = 'bootstrapck';
	config.toolbar = [
		{ name: 'basicstyles', items: ['Bold', 'Italic', 'Underline', 'Strike', '-', 'RemoveFormat'] },
		{ name: 'paragraph', items: ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight'] },
		{ name: 'insert', items: ['Link', 'Unlink', 'Paste', 'PasteText', 'Image', 'Youtube'] },
		{ name: 'styles', items: ['FontSize', 'TextColor', 'BGColor'] }
	];
	// %REMOVE_END%

	config.disableNativeSpellChecker = false;
	config.startupFocus = true;
	config.toolbarCanCollapse = true;
	config.extraPlugins = (_isSafariMobile ? '' : 'autogrow,') + 'find,image';
	config.removePlugins = (_isSafariMobile ? 'autogrow,' : '') + 'tab,elementspath';
	config.autoGrow_onStartup = false;
	config.autoGrow_minHeight = 100;
	config.resize_enabled = false;
	config.enableTabKeyTools = false;
	config.defaultLanguage = 'en';
	config.height = 100;
	config.enterMode = CKEDITOR.ENTER_BR;
	config.keystrokes = [
		[ CKEDITOR.SHIFT + 13, null ],
	];
};

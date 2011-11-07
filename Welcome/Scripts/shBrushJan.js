/**
 *  Author: Will Schleter
 *  based on: http://www.jamesrohal.com
 */
SyntaxHighlighter.brushes.JarnacKey = function()
{
  var keywords = 'par p defn ext vol at time => -> end var';
  var functions = 'sin cos exp int mod conv pow sqrt gt';
  this.regexList = [
	{ regex: SyntaxHighlighter.regexLib.singleLineCComments,	css: 'comments' },		// one line comments
	{ regex: /\/\*([^\*][\s\S]*)?\*\//gm,						css: 'comments' },	 	// multiline comments
    { regex: SyntaxHighlighter.regexLib.singleQuotedString, css: 'string' },
    { regex: SyntaxHighlighter.regexLib.doubleQuotedString, css: 'string'},
    { regex: new RegExp(this.getKeywords(keywords), 'gm'), css: 'keyword' }
  ];
};
SyntaxHighlighter.brushes.JarnacKey.prototype	= new SyntaxHighlighter.Highlighter();
SyntaxHighlighter.brushes.JarnacKey.aliases	= ['jan'];

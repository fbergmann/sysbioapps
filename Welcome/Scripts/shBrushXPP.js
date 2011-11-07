/**
 *  Author: Will Schleter
 *  based on: http://www.jamesrohal.com
 */
SyntaxHighlighter.brushes.XPPKey = function()
{
  var keywords = 'par p global bdry  init done solve aux markov then if else wiener % table special param';
  var functions = 'sin cos exp int mod conv pow sqrt';
  this.regexList = [
    { regex: /#.*$/gm,	css: 'comments' }, // one line comments
    { regex: /\#\{[\s\S]*?\#\}/gm, css: 'comments'}, // multiline comments
    { regex: SyntaxHighlighter.regexLib.singleQuotedString, css: 'string' },
    { regex: SyntaxHighlighter.regexLib.doubleQuotedString, css: 'string'},
    { regex: new RegExp(this.getKeywords(keywords), 'gm'), css: 'keyword' }
  ];
};
SyntaxHighlighter.brushes.XPPKey.prototype	= new SyntaxHighlighter.Highlighter();
SyntaxHighlighter.brushes.XPPKey.aliases	= ['xpp'];

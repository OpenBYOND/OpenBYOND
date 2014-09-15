using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using Irony.Interpreter;
using Irony.Ast;
using Irony.Interpreter.Ast;

/**************************************************************************
 * WARNING: HERE BE DRAGONS.
 * 
 * THIS FILE DEFINES THE GRAMMAR FOR THE DREAM PROGRAMMING LANGUAGE.
 * 
 * IF YOU HAVEN'T READ ANYTHING ABOUT COMPILERS OR PROGRAMMING LANGUAGES,
 * FLEE NOW AND ONLY RETURN AFTER YOU KNOW WHAT THE FUCK YOU'RE DOING.
 * 
 * THE SMALLEST OF CHANGES CAN PRODUCE BUGS AND REGRESSIONS.
 * 
 * CONSULT N3X15 BEFORE MODIFYING. nexisentertainment@gmail.com
 ***************************************************************************/

namespace OpenBYOND.VM
{
    /// <summary>
    /// We're going to go ahead and call our butchered implementation of the BYOND programming language "Dream".  
    /// I don't know if BYOND already calls it that, but I've only heard it referenced several different ways 
    /// and it's getting annoying calling it shit like "BYOND Programming Language" or something vague like "DM".
    /// 
    /// "Nightmare" would be more honest, but doesn't do too well in the marketting department. If BYOND pulls a 
    /// trademark card, we'll just switch to that.
    /// </summary>
    [Language("Dream", "0.1", "WIP re-implementation of the BYOND game development language.")]
    public class DreamGrammar : InterpretedLanguageGrammar
    {
        // ROADMAP:
        // 1. Basic object-tree shit (atoms)
        // 2. Proc support
        // 3. Preprocessor
        public DreamGrammar()
            : base(caseSensitive: true)
        {
            // Thankfully, we get built-in pythonic numbers, which is exactly what we need.
            var NUMBER = TerminalFactory.CreatePythonNumber("number");
            var IDENTIFIER = TerminalFactory.CreatePythonIdentifier("identifier");
            var STRING = TerminalFactory.CreatePythonString("string"); // TODO:  May need to be custom.

            // Constant terminals
            var PROC = ToTerm("proc");
            var VAR = ToTerm("var");
            var RETURN = ToTerm("return");

            var COMMA = ToTerm(",");
            var SLASH = ToTerm("/");
            var PIPE = ToTerm("|");

            #region Comment stuff

            // Dream uses both line comments and block comments.
            var lineComment = new CommentTerminal("lineComment", "//", "\n", "\r");

            // May or may not work, since BYOND permits nesting and most other languages don't.
            var blockComment = new CommentTerminal("blockComment", "/*", "*/");

            // From MiniPython example:
            // //comment must to be added to NonGrammarTerminals list; it is not used directly in grammar rules,
            // // so we add it to this list to let Scanner know that it is also a valid terminal. 
            // However, with Dream, we may need to make them grammar terminals so we can handle nesting properly. - N3X
            base.NonGrammarTerminals.Add(lineComment);
            base.NonGrammarTerminals.Add(blockComment);

            // Needed for EOL escaping.
            base.NonGrammarTerminals.Add(ToTerm(@"\"));
            #endregion

            #region Non-Terminals
            // Blocks
            ////////////////////////////

            // procs
            var procblock = new NonTerminal("procblock", "proc block");
            var procslash = new NonTerminal("procslash");
            var procdef = new NonTerminal("procdef", "proc definition");
            var procdefs = new NonTerminal("procdefs");
            var procdef_no_path = new NonTerminal("procdef_no_path", "proc definition (sans path)");
            var procdecl = new NonTerminal("procdecl");
            var procchildren = new NonTerminal("procchildren", "proc children");

            var atomblock = new NonTerminal("atomblock", "atom declaration");
            var atomchildren = new NonTerminal("atomchildren", "atom declaration");
            var atomchild = new NonTerminal("atomchild", "atom child");

            // Path stuff
            var path = new NonTerminal("path");
            var abspath = new NonTerminal("abspath", "absolute path");
            var relpath = new NonTerminal("relpath", "relative path");
            var pathslash = new NonTerminal("pathslash");

            // Variable declaration
            var vardefs = new NonTerminal("vardefs");
            var vardef = new NonTerminal("vardef");
            var inline_vardef_no_default = new NonTerminal("inline_vardef_no_default");
            var inline_vardef = new NonTerminal("inline_vardef");
            var varblock = new NonTerminal("varblock");

            // Parameters
            var param = new NonTerminal("param", "parameter");
            var parameters = new NonTerminal("parameters");
            var paramlist = new NonTerminal("paramlist", "parameter list", typeof(ParamListNode));

            // Primitives (Identifiers won't work because of the "anything in <list>" rule.)
            var primitive = new NonTerminal("primitive");
            var primitivelist = new NonTerminal("primitivelist","primitive list");

            // Expressions
            var const_expression = new NonTerminal("const_expression");
            var expression = new NonTerminal("expression");
            var expressions = new NonTerminal("expressions");
            var assignable_expression = new NonTerminal("assignable_expression");
            var assignment = new NonTerminal("assignment");
            var operations = new NonTerminal("operations");

            // Statements
            var return_stmt = new NonTerminal("return_stmt");

            var script = new NonTerminal("script", "script root", typeof(StatementListNode));
            var declblocks = new NonTerminal("declblocks", "declarative blocks");
            #endregion

            #region BNF Rules

            #region Type Paths
            // <path> ::= <abspath> | <relpath>
            path.Rule = abspath | relpath;

            // <relpath> ::= <identifier>
            //             | <relpath> '/' <identifier>
            relpath.Rule = IDENTIFIER 
                | relpath + SLASH + IDENTIFIER;

            // <abspath> ::= '/' <relpath>
            //             | <abspath> '/' <identifier>
            abspath.Rule = SLASH + relpath
                | abspath + SLASH + IDENTIFIER;

            // <pathslash> ::= <path> '/'
            pathslash.Rule = path + SLASH;
            #endregion

            #region Blocks (atoms, procs, if, etc)
            // <atomblock> ::= <path> INDENT <atomchildren> DEDENT
            atomblock.Rule = path + Indent + atomchildren + Dedent;

            // <atomchildren> ::= <atomchild>*
            atomchildren.Rule = MakeStarRule(atomchildren, atomchild);

            // <atomchild> ::= <vardecl>
            //               | <atomblock>
            //               | <procblock>
            //               | <varblock>
            atomchild.Rule = vardef 
                           | atomblock 
                           | procblock 
                           | varblock
                           ;

            // <procblock> ::= PROC INDENT <procdefs> DEDENT
            procblock.Rule = PROC + Indent + procdefs + Dedent;

            // <procdefs> ::= <procdef_no_path>+
            procdefs.Rule = MakePlusRule(procdefs,procdef_no_path);

            // <declblocks> ::= <procblock>
            //                | <atomblock>
            declblocks.Rule = atomblock | procblock;
            #endregion

            #region Variable stuff
            // <vardef> ::= <path> <varblock>
            //            | <varblock>
            //            | <inline_vardef>
            vardef.Rule = path + varblock
	                    | varblock
	                    | inline_vardef
	                    ;

            // <vardefs> ::= <vardef>+
            vardefs.Rule = MakePlusRule(vardefs,vardef);
            
	        // var/honk is basically 
	        // VAR path(/) identifier(honk)
            // <inline_vardef_no_default> ::= VAR <abspath> '/' IDENTIFIER
            //                              | VAR '/' IDENTIFIER
            inline_vardef_no_default.Rule = VAR + abspath + SLASH + IDENTIFIER
                                          | VAR + SLASH + IDENTIFIER
                                          ;
            // <inline_vardef> ::= inline_vardef_no_default
            //                   | inline_vardef_no_default '='
            inline_vardef.Rule = inline_vardef_no_default                     
	                           | inline_vardef_no_default + "=" + const_expression
                               ;
	
            // <varblock> ::= VAR INDENT <vardefs> DEDENT
            varblock.Rule = VAR + Indent + vardefs + Dedent;
            #endregion

            #region Proc stuff
            // <parameters> ::= '(' <paramlist> ')'
            parameters.Rule = ToTerm("(") + paramlist + ")";

            // <paramlist> ::= <param>*
            paramlist.Rule = MakeStarRule(paramlist, COMMA, param);

            // This is probably one of the worst parts about Dream. This shit is an absolute mess. - N3X
            // <param> ::= <inline_vardef>                          // var/type/blah [=blah]
            //           | <identifier>                       // blah
            //           | <inline_vardef_no_default> 'as' <primitivelist> // var/blah as obj|mob
            //           | <identifier> 'as' <primitivelist>  // blah as obj|mob
            param.Rule = inline_vardef
                 | IDENTIFIER
                 | inline_vardef_no_default + "as" + primitivelist
                 | IDENTIFIER + "as" + primitivelist
                 ;

            // <procdef> ::= <path> <parameters> INDENT <expressions> DEDENT
            procdef.Rule = path + parameters + Indent + expressions + Dedent;

            // <procslash> ::= PROC SLASH
            procslash.Rule = PROC + SLASH;

            // <procdecl> ::= <pathslash> <procslash> <procdef_no_path>
            //              | <pathslash> <procblock>
            //              | <procblock> 
            procdecl.Rule = pathslash + procslash + procdef_no_path
                          | pathslash + procblock
                          | procblock
                          ;
            #endregion

            #region Expressions
            // Note:  this gets particularly hairy.

            // <const_expression> ::= NUMBER
            //                      | STRING
            //                      | '(' <const_expression> ')'
            //                      | <operations>
            const_expression.Rule = NUMBER
	                              | STRING
	                              | "(" + const_expression +  ")"
                                  | operations
                                  ;

            // <operations> ::= <const_expression> "*" <const_expression>
	        //                | <const_expression> "/" <const_expression>
	        //                | <const_expression> "%" <const_expression>
	        //                | <const_expression> "+" <const_expression>
	        //                | <const_expression> "-" <const_expression>
            operations.Rule = const_expression + "*" + const_expression
	                        | const_expression + "/" + const_expression
	                        | const_expression + "%" + const_expression
	                        | const_expression + "+" + const_expression
	                        | const_expression + "-" + const_expression
	                        ;

            // <expression> ::= <assignment>
            //                | <inline_vardef>
            //                | <return_stmt>
            expression.Rule = assignment
	                        | inline_vardef
	                        | return_stmt
	                        ;
	
            // <expressions> ::= <expression>*
            expressions.Rule = MakeStarRule(expressions,expression);
	
            // <assignable_expression> ::= <const_expression>
            //                           | IDENTIFIER
            //                           | STRING
            assignable_expression.Rule = const_expression
	                                   | IDENTIFIER 
	                                   | STRING
	                                   ;
	
            // <assignment> ::= IDENTIFIER '=' <assignable_expression>
            assignment.Rule=IDENTIFIER + "=" + assignable_expression;

            // <return_stmt> ::= RETURN const_expression
            return_stmt.Rule = RETURN + const_expression;
            #endregion

            // Okay, this is apparently the right way to do it. - N3X
            primitive.Rule = ToTerm("obj")|"mob"|"turf"/*|"anything" + "in" + list*/;

            // <primitivelist> ::= <primitive>+ (| seperator)
            primitivelist.Rule = MakePlusRule(primitivelist,PIPE,primitive);

            // <paramlist> ::= <parameter>*
            paramlist.Rule = MakeStarRule(paramlist, COMMA, param);

            // <script> ::= <declblocks>*
            script.Rule = MakeStarRule(script, declblocks);
            this.Root = script;
            #endregion
        }

        public override void CreateTokenFilters(LanguageData language, TokenFilterList filters)
        {
            var outlineFilter = new CodeOutlineFilter(language.GrammarData,
              OutlineOptions.ProduceIndents | OutlineOptions.CheckBraces, ToTerm(@"\")); // "\" is continuation symbol
            filters.Add(outlineFilter);
        }
    }
}

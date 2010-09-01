﻿using System;
using System.Collections.Generic;
using System.Linq;
using Irony.Parsing;

namespace Demo
{
    [Language("Dafny", "1.0", "Dafny Programming Language")]
    public class Grammar : Irony.Parsing.Grammar
    {
      public Grammar() {
        #region 1. Terminals
        NumberLiteral n = TerminalFactory.CreateCSharpNumber("number");

        IdentifierTerminal ident = new IdentifierTerminal("Identifier");
        StringLiteral stringLiteral = TerminalFactory.CreateCSharpString("String");

        this.MarkReservedWords(  // NOTE: these keywords must also appear once more below
          "class", "ghost", "static", "var", "method", "datatype",
          "assert", "assume", "new", "this", "object", "refines", "replaces", "by",
          "unlimited", "module", "imports",
          "call", "if", "then", "else", "while", "invariant",
          "break", "label", "return", "foreach", "havoc", "print", "use",
          "returns", "requires", "ensures", "modifies", "reads", "decreases",
          "int", "bool", "false", "true", "null",
          "function", "free",
          "in", "forall", "exists",
          "seq", "set", "array",
          "match", "case",
          "fresh", "old"
          );

        StringLiteral s = new StringLiteral("String", "'", StringFlags.AllowsDoubledQuote);

        Terminal dot = ToTerm(".", "dot");
        Terminal less = ToTerm("<");
        Terminal greater = ToTerm(">");
        Terminal arrow = ToTerm("=>");
        Terminal LBracket = ToTerm("[");
        Terminal RBracket = ToTerm("]");
        Terminal LParen = ToTerm("(");
        Terminal RParen = ToTerm(")");
        Terminal RCurly = ToTerm("}");
        Terminal LCurly = ToTerm("{");
        Terminal LMb = ToTerm("<[");
        Terminal RMb = ToTerm("]>");
        Terminal comma = ToTerm(",");
        Terminal semicolon = ToTerm(";");
        Terminal colon = ToTerm(":");

        #endregion

        #region 2. Non-terminals
        #region 2.1 Expressions
        NonTerminal expression = new NonTerminal("Expr");
        NonTerminal BinOp = new NonTerminal("BinOp");
        NonTerminal LUnOp = new NonTerminal("LUnOp");
        NonTerminal RUnOp = new NonTerminal("RUnOp");

        NonTerminal ArrayConstructor = new NonTerminal("ArrayConstructor");
        NonTerminal MObjectConstructor = new NonTerminal("MObjectConstructor");
        NonTerminal MObjectList = new NonTerminal("MObjectList");
        #endregion

        #region 2.2 QualifiedName
        //Expression List:  expr1, expr2, expr3, ..
        NonTerminal expressionList = new NonTerminal("ExprList");
        NonTerminal identList = new NonTerminal("identList");
        //A name in form: a.b.c().d[1,2].e ....
        NonTerminal NewStmt = new NonTerminal("NewStmt");
        NonTerminal NewArrStmt = new NonTerminal("NewArrStmt");
        NonTerminal QualifiedName = new NonTerminal("QualifiedName");
        NonTerminal GenericsPostfix = new NonTerminal("GenericsPostfix");
        NonTerminal ArrayExpression = new NonTerminal("ArrayExpression");
        NonTerminal FunctionExpression = new NonTerminal("FunctionExpression");
        NonTerminal selectExpr = new NonTerminal("selectExpr");
        #endregion

        #region 2.3 Statement
        NonTerminal Condition = new NonTerminal("Condition");

        NonTerminal Statement = new NonTerminal("Statement");
        NonTerminal Statements = new NonTerminal("Statements");

        //Block
        NonTerminal blockStatement = new NonTerminal("CompoundStatement");
        #endregion

        #region 2.4 Program and Functions
        NonTerminal Prog = new NonTerminal("Prog");
        NonTerminal anything = new NonTerminal("anything");  // temporary hack
        NonTerminal declaration = new NonTerminal("declaration");
        NonTerminal classDecl = new NonTerminal("class decl");
        NonTerminal memberDecl = new NonTerminal("member decl");
        NonTerminal fieldDecl = new NonTerminal("field declaration");
        NonTerminal idType = new NonTerminal("identifier type");
        NonTerminal typeDecl = new NonTerminal("type reference");
        NonTerminal methodDecl = new NonTerminal("method declaration");
        NonTerminal formalParameters = new NonTerminal("formals");
        NonTerminal methodSpec = new NonTerminal("method spec");
        NonTerminal formalsList = new NonTerminal("ParamaterListOpt");
        NonTerminal functionDecl = new NonTerminal("function declaration");
        NonTerminal predicateDecl = new NonTerminal("predicate declaration");
        NonTerminal invariantDecl = new NonTerminal("invariant declaration");
        NonTerminal Semi = new NonTerminal("semi");
        NonTerminal Rhs = new NonTerminal("right-hand side");
        NonTerminal FieldInit = new NonTerminal("field init");
        NonTerminal FieldInits = new NonTerminal("field inits");
        NonTerminal installBounds = new NonTerminal("installBounds");
        NonTerminal localVarStmt = new NonTerminal("localVarStmt");
        NonTerminal evalstate = new NonTerminal("evalstate");
        NonTerminal channelDecl = new NonTerminal("channel declaration");
        NonTerminal loopSpec = new NonTerminal("loop specification");
        NonTerminal rdPermArg = new NonTerminal("rdPermArg");
        #endregion

        #endregion

        #region 3. BNF rules

        Semi.Rule = semicolon;

        #region 3.1 Expressions
        selectExpr.Rule = (ToTerm("this") + ".").Q() + QualifiedName;
        evalstate.Rule =
          ident + ToTerm(".") +
          (ToTerm("acquire")
          | "release"
          | "fork" + FunctionExpression
          )
          ;
        rdPermArg.Rule = ToTerm("*") | expression;

        expression.Rule = ToTerm("true")
                    | "false"
                    | "null"
                    | "maxlock"
                    | "lockbottom"
                    | "this"
                    | "result"
                    | s
                    | n
                    | QualifiedName
          // The following is needed: to parse "A<B ..." either as comparison or as beginning of GenericsPostfix
                    | QualifiedName + less + expression
                    //| QualifiedName + less + QualifiedName + greater
                    //| NewStmt
                    | NewArrStmt
                    | ArrayExpression
                    | FunctionExpression
                    | ArrayConstructor
                    | MObjectConstructor
                    | expression + BinOp + expression
                    | LUnOp + expression
                    | expression + RUnOp
                    | LMb + declaration.Star() + RMb
                    | LParen + expression + RParen
                    | ToTerm("unfolding") + expression + "in" + expression
                    | ToTerm("acc") + "(" + selectExpr  + (("," + expression) | Empty) + ")"
                    | ToTerm("old") + "(" + expression + ")"
                    | ToTerm("eval") + "(" + evalstate + "," + expression + ")"
                    | ToTerm("credit") + "(" + expression + "," + expression + ")"
                    | ToTerm("credit") + "(" + expression + ")"
                    | expression + PreferShiftHere() + "?" + expression + ":" + expression
                    | ToTerm("rd") +
                      (ToTerm("holds") + "(" + expression + ")"
                      | "(" + selectExpr + rdPermArg.Q() + ")"
                      )

                    ;
        expressionList.Rule = MakePlusRule(expressionList, comma, expression);
        identList.Rule = MakePlusRule(identList, comma, ident);
        NewStmt.Rule = "new" + QualifiedName + GenericsPostfix.Q() + LParen + expressionList.Q() + RParen;
        NewArrStmt.Rule = "new" + QualifiedName + GenericsPostfix.Q() + LBracket + expressionList.Q() + RBracket;
        BinOp.Rule = ToTerm("+") | "-" | "*" | "/" | "%" | "^" | "&" | "|"
                    | "&&" | "||" | "==" | "!=" | greater | less
                    | ">=" | "<=" | "is"
                    | "=" | "+=" | "-="
                    | "."
                    | "==>" | "<==>" | "<<"
                    ;

        LUnOp.Rule = ToTerm("-") | "~" | "!";
        RUnOp.Rule = ToTerm("++") | "--";

        ArrayConstructor.Rule = LBracket + expressionList + RBracket;
        MObjectConstructor.Rule = LBracket + ident + arrow + expression + MObjectList.Star() + RBracket;
        MObjectList.Rule = comma + ident + arrow + expression;
        #endregion

        #region 3.2 QualifiedName
        ArrayExpression.Rule = QualifiedName + LBracket + expressionList + RBracket;
        FunctionExpression.Rule = QualifiedName + LParen + expressionList.Q() + RParen;

        QualifiedName.Rule = ident | QualifiedName + dot + ident;


        GenericsPostfix.Rule = less + QualifiedName + greater;

        //ExprList.Rule = Expr.Plus(comma);
        #endregion

        #region 3.3 Statement
        Condition.Rule = LParen + expression + RParen;
        installBounds.Rule
          = "installBounds"
          //= ToTerm("between") + expressionList + "and" + expressionList
          //| "below" + expressionList
          //| "below" + expressionList + "above" + expressionList
          //| "above" + expressionList
          //| "above" + expressionList + "below" + expressionList
          ;
        FieldInit.Rule
          = ident + ":=" + expression
          ;
        FieldInits.Rule = MakeStarRule(FieldInits, ToTerm(","), FieldInit);
        Rhs.Rule
          = ToTerm("new") + ident
          | ToTerm("new") + ident + "{" + FieldInits + "}"
          | ToTerm("new") + ident + installBounds
          | ToTerm("new") + ident + "{" + FieldInits + "}" + installBounds
          | expression
          ;
        localVarStmt.Rule
          = idType + ":=" + Rhs + Semi
          | idType + Semi
          ;
        loopSpec.Rule
          = ToTerm("invariant") + expression + Semi
          | "lockchange" + expressionList + Semi
          ;



        Statement.Rule = Semi
                        | "if" + Condition + Statement
                        | "if" + Condition + Statement + PreferShiftHere() + "else" + Statement
                        | "while" + Condition + loopSpec.Star() + Statement
                        | "for" + LParen + expression.Q() + Semi + expression.Q() + Semi + expression.Q() + RParen + Statement
                        | "foreach" + LParen + ident + "in" + expression + RParen + Statement
                        | blockStatement
                        | expression + Semi
                        | "break" + Semi
                        | "continue" + Semi
                        | "return" + expression + Semi
                        | QualifiedName + ":=" + Rhs

                        | "var" + localVarStmt

                        | "call" + identList + ":=" + FunctionExpression + Semi
                        | "call" + FunctionExpression + Semi
                        | "assert" + expression + Semi
                        | "assume" + expression + Semi
                        | "unshare" + expression + Semi
                        | "acquire" + expression + Semi
                        | "release" + expression + Semi
                        | "downgrade" + expression + Semi
                        | "free" + expression + Semi
                        | "fold" + expression + Semi
                        | "unfold" + expression + Semi
                        | "reorder" + expression + installBounds + Semi
                        | "reorder" + expression + Semi
                        | "share" + expression + installBounds + Semi
                        | "share" + expression + Semi
                        | "fork" + identList + ":=" + FunctionExpression + Semi
                        | "fork" + FunctionExpression + Semi
                        | "join" + identList + ":=" + expression + Semi
                        | "join" + expression + Semi
                        | "send" + expression + Semi
                        | "receive" + identList + ":=" + expression + Semi
                        | "receive" + expression + Semi

                        ;
        Statements.Rule = MakeStarRule(Statements, null, Statement);
        blockStatement.Rule = LCurly + Statements + RCurly;


        #endregion

        #region 3.4 Prog
        Prog.Rule = anything.Star() + Eof;

        anything.Rule
          = ToTerm("class")
          | "ghost"
          | "static"
          | "var"
          | "method"
          | "datatype"
          | "assert"
          | "assume"
          | "new"
          | "this"
          | "object"
          | "refines"
          | "replaces"
          | "by"
          | "unlimited"
          | "module"
          | "imports"
          | "call"
          | "if"
          | "then"
          | "else"
          | "while"
          | "invariant"
          | "break"
          | "label"
          | "return"
          | "foreach"
          | "havoc"
          | "print"
          | "use"
          | "returns"
          | "requires"
          | "ensures"
          | "modifies"
          | "reads"
          | "decreases"
          | "int"
          | "bool"
          | "false"
          | "true"
          | "null"
          | "function"
          | "free"
          | "in"
          | "forall"
          | "exists"
          | "seq"
          | "set"
          | "array"
          | "match"
          | "case"
          | "fresh"
          | "old"
          | ident
          | "}"
          | "{"
          | "("
          | ")"
          | "["
          | "]"
          | ","
          | ":"
          | ";"
          | "."
          | "`"
          | "=="
          | "!="
          | "<"
          | "<="
          | ">="
          | ">"
          | "=>"
          | ":="
          | "+"
          | "-"
          | "*"
          | "/"
          | "%"
          | "!!"
          | "|"
          | "!"
          | "&&"
          | "||"
          | "==>"
          | "<==>"
          | "#"
          | n
          | stringLiteral
          ;

        idType.Rule
          = ident + ":" + typeDecl
          | ident
          ;

        typeDecl.Rule
          = (ToTerm("int") | "bool" | ident | "seq" | "set" | "array") + (("<" + MakePlusRule(typeDecl, ToTerm(","), typeDecl) + ">") | Empty)
          | ToTerm("token") + "<" + (typeDecl + ".") + ident + ">"
          ;

        fieldDecl.Rule
          = ToTerm("var") + idType + Semi
          | ToTerm("ghost") + "var" + idType + Semi
          ;

        methodSpec.Rule = (ToTerm("requires") | "ensures" | "lockchange") + expression + Semi;

        formalsList.Rule = MakeStarRule(formalsList, comma, idType);
        formalParameters.Rule = LParen + formalsList + RParen;
        methodDecl.Rule = "method" + ident + formalParameters
          + (("returns" + formalParameters) | Empty)
          + methodSpec.Star()
          + blockStatement;
        functionDecl.Rule
          = ToTerm("function") + ident + formalParameters + ":" + typeDecl + methodSpec.Star() + "{" + expression + "}";
        predicateDecl.Rule
          = ToTerm("predicate") + ident + "{" + expression + "}";
        invariantDecl.Rule
          = ToTerm("invariant") + expression + Semi;

        memberDecl.Rule
          = fieldDecl
          | invariantDecl
          | methodDecl
          //| conditionDecl
          | predicateDecl
          | functionDecl
          ;
        classDecl.Rule
          = (ToTerm("external") | Empty) + "class" + ident + ("module" + ident | Empty) + "{" + memberDecl.Star() + "}";
        channelDecl.Rule
          = ToTerm("channel") + ident + formalParameters + "where" + expression + Semi
          | ToTerm("channel") + ident + formalParameters + Semi;
        declaration.Rule = classDecl | channelDecl
          ;


        Terminal Comment = new CommentTerminal("Comment", "/*", "*/");
        NonGrammarTerminals.Add(Comment);
        Terminal LineComment = new CommentTerminal("LineComment", "//", "\n");
        NonGrammarTerminals.Add(LineComment);
        #endregion
        #endregion

        #region 4. Set starting symbol
        this.Root = Prog; // Set grammar root
        #endregion

        #region 5. Operators precedence
        RegisterOperators(1, "<==>");
        RegisterOperators(2, "+", "-");
        RegisterOperators(3, "*", "/", "%", "!!");
        RegisterOperators(4, Associativity.Right, "^");
        RegisterOperators(5, "||");
        RegisterOperators(6, "&&");
        RegisterOperators(7, "==", "!=", ">", "<", ">=", "<=");
        RegisterOperators(8, "in");
        RegisterOperators(9, "-", "!", "++", "--");
        RegisterOperators(10, "==>");
        RegisterOperators(11, ".");

        //RegisterOperators(10, Associativity.Right, ".",",", ")", "(", "]", "[", "{", "}");
        //RegisterOperators(11, Associativity.Right, "else");
        #endregion

        #region 6. Punctuation symbols
        RegisterPunctuation("(", ")", "[", "]", "{", "}", ",", ";");
        #endregion
      }
    }
}
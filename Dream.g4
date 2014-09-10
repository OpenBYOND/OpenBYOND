/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */

grammar Dream;

script: atomblock
      | procblock
      | vardecl;

atomblock: path INDENT atomchildren DEDENT;
procblock: path '(' arglist ')' INDENT procchildren DEDENT;
vardecl: VAR abspath EOL
       | VAR abspath EQ stmt EOL;

path: abspath
    | relpath;
abspath: SLASH relpath;
relpath: IDENTIFIER
       | IDENTIFIER SLASH relpath;

atomchildren: procblock | atomblock | vardecl | atomchildren | "";

procchildren: "" | expr EOL procchildren;

expr: vardecl | assignment | return;
assignment: IDENTIFIER EQ stmt;
stmt: binop | 
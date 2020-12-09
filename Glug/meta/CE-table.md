# Compilation error table

## Levels of compilation errors

| Type    | Description                                                  |
| ------- | ------------------------------------------------------------ |
| Error   | An error in source code has been found, it's impossible to continue compiling any longer. |
| Warning | An error in source code has been found, it's possible to continue but it's very likely to produce unexpected result. |
| Info    | A defect in source code has been found. It's safe to ignore it but it's better to fix it. |

| Description     | E    | W    | I    |
| --------------- | ---- | ---- | ---- |
| Good code       | No   | No   | No   |
| Acceptable code | No   | No   | Yes  |
| Bad code        | No   | Yes  | Any  |
| Wrong code      | Yes  | Any  | Any  |

## List of compilation errors

| Stage          | Description                        |
| -------------- | ---------------------------------- |
| Tokenizing     | From input stream to token stream. |
| Parsing        | From token stream to AST.          |
| Postprocessing | AST check and transformation.      |
| CodeGen        | From processed AST to code.        |

| ID   | Level | Stage          | Content                                                      | Note                                                         |
| ---- | ----- | -------------- | ------------------------------------------------------------ | ------------------------------------------------------------ |
| 1000 | W     | Postprocessing | `Callee and arguments in different lines without '$'`        | To prevent unexpected results caused by missing semicolon(s). |
| 1001 | E     | Parsing        | `Unexpected end of input`                                    |                                                              |
| 1002 | E     | Postprocessing | `Dangling break`                                             |                                                              |
| 1003 | E     | CodeGen        | `Evaluation of pseudo expression '${PseudoExpressionType}'`  |                                                              |
| 1004 | E     | Postprocessing | `Assignment to unassignable expression`                      |                                                              |
| 1005 | E     | Tokenizing     | `Unclosed string literal`                                    |                                                              |
| 1006 | W     | Parsing        | `Loop body begins with left parenthesis but do not ends with corresponding right parenthesis` | To prevent unexpected results in expressions like:<br />`([1] .. while (true) (break [2]) .. [3]) # it's [1, 2], not [1, 2, 3]`<br />Loop body here is `(break [2]) .. [3]` because the parser is greedy when reading expressions. |


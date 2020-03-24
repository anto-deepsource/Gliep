# TODO List

REFACTOR!!!!

**Check in-file TODOs first**!

## Cross-project

- add debug info in GlosUnit.

## Glos

- add tostring instruction.
- add hash instruction.
  - find a good way to evaluate and update glosvalue's hash.
- add truthy and falsey instruction.

## Glug

- add locally ren and uen operators.
- allow empty expr `;`. e.g. `while (sth) ;`. a possible method is making `;` termination marks of expr, instead of seperators in blocks.
- add short-circuit operators.
- add null check operator `?` and `??`.
- add compound assignment operator.
- add lua-like integer index table optimization.
- ~~make `-a.b` `-(a.b)` instead of `(-a).b`. meanwhile should we keep `-a@b` `(-a)@b`?~~
  - think it through.

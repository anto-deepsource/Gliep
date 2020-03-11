# TODO List

REFACTOR!!!!

**Check in-file TODOs first**!

FIRST OF ALL: Rework on AST, move all extra informations in AST to other structure.

## Glos

- add `duplist` instruction.
- add debug and normal stringify function for glosvalue.
- add tostring instruction.
- add hash instruction.
  - find a good way to evaluate and update glosvalue's hash.
- add truthy and falsey instruction.

## Glug

- make index in osl ref. i.e. allow `[a.a, a.b] = [1, 2]`.
- add locally ren and uen operators.
- Node.IsVarRef and Node.IsOnStackList may cause implicit deep recursive evaluation, consider calculate these properties by a visitor.
- ~~code generator could take is_value_used as a parameter so~~
  - ~~it can generate better code.~~
  - `[...] = [...]` will no longer be nil. (instruction `duplist` required).
- allow empty expr `;`. e.g. `while (sth) ;`. a possible method is making `;` termination marks of expr, instead of seperators in blocks.
- add short-circuit operators.
- add null check operator `?` and `??`.
- add compound assignment operator.
- add lua-like integer index table optimization.
- make `-a.b` `-(a.b)` instead of `(-a).b`. meanwhile should we keep `-a@b` `(-a)@b`?

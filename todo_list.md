# TODO List

REFACTOR!!!!

**Check in-file TODOs first**!

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
- ~~code generator could take is_value_used as a parameter so~~
  - ~~it can generate better code.~~
  - `[...] = [...]` will no longer be nil. (instruction `duplist` required).
- allow empty expr `;`. e.g. `while (sth) ;`. a possible method is making `;` termination marks of expr, instead of seperators in blocks.
- add short-circuit operators.
- add null check operator `?` and `??`.
- add compound assignment operator.
- add lua-like integer index table optimization.
- make `-a.b` `-(a.b)` instead of `(-a).b`. meanwhile should we keep `-a@b` `(-a)@b`?

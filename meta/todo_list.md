# TODO List

- [x] *(2020/08/27)* Make for-loop breakable

    ```
    print for (i: iter {| 1, 1, 2, 3, 5, 8, 13 |}) if (i > 5) break i;
    # 8 expected
    ```

- [x] *(2020/08/27)* Add optional labels for breakable blocks

    ```
    print for 'outer (i: iter {| 1, 2, 3, 4, 5 |}) (
        for 'inner (j: iter {| 6, 7, 8, 9, 0 |}) (
            if (i * 10 + j == 37) (
                break 'outer [i, j];
            )
        )
    );
    ```

- [ ] *(2020/08/27)* A better standard library
    
    ... like this.

    ```
    # stl: a.gl
    { .foo: __built_in_foo, .bar: __built_in_bar, .taz: __built_in_taz }
    
    # main.gl
    a = require "a";
    a.foo[a.bar, a.taz]
    ```

- [ ] *(2020/08/28)* Replace current unit manager with a `GliepEnv` class

    which:
    - provides builtin functions
    - manage vm and units
    - etc

    *Update (2020/09/25)*

    - [x] Function prototypes should not record its parent unit itself. This reference broke too many things. It may be recorded in functions.

    *Update (2020/12/11)*

    New plan: a improved `GlosViMa` which:

    - provides builtin functions
    - manage units
    - ~~manage execution contexts, which contain all stacks~~ call it coroutine
    - manage coroutines

    *Update (2020/12/21)*

    To be answered:

    - How should unit manager (especially function `require`) react with coroutine system?

- [x] *(2020/08/29)* Make it clear where trailing separators are allowed/prohibited and make parser more strict

    - [ ] **Prohibited** in function parameter list:

        ```
        // CE
        fn x[p0, p1,] p0 + p1;
        ```

    - [ ] **Allowed** in on-stack-list, and lambda parameter list (parsed as on-stack-list):

        ```
        // OK
        [a, b,] = [0, 1,];
        // OK
        y = [p2, p3,] -> p2 - p3;
        ```

    - [ ] **Allowed** in block:

        ```
        // OK
        ( 0; 1; 2; );
        ```
    
    - [ ] **Allowed** in table and vector literal:

        ```
        // OK
        x = { .a: 0, .b: 1, .c: 2, };
        // OK
        y = {| 3, 4, 5, |};
        ```
    
    - [ ] **Prohibited** in for-loop iteration variable list:

        ```
        // CE
        for (x, y,: pairs {| 0, 1, 2 |}) x + y;
        ```

- [x] *(2020/09/09)* Add unpacking vector operator

    `...` may be a good choice:

    ```
    // [x, y, z] = 0, 1, 2
    [x, y, z] = ...{| 0, 1, 2, 3 |};
    ```

    However, there are still several problems. For example, when I try to concat unpacked vector after an osl, it becomes:

    ```
    // HORRIBLE
    print([0, 1, 2, 3] .. ...{| 4, 5, 6 |});
    ```

    A solution is make `...` special inside osl literals, which means:

    ```
    // it's [0, 1, 2, 3, 4, 5, 6]
    [0, 1, 2, 3, ...{| 4, 5, 6 |}];
    ```

    And `...` can be applied to osls too:

    ```
    // the same as above
    [0, 1, 2, 3, ...[4, 5, 6]];
    ```

    And if we allow this in vector literals:

    ```
    // BRAVO!
    {| 0, 1, 2, 3, ...[4, 5, 6] |};
    ``` 

    However, if there is something like:

    ```
    // God bless The Go Language!
    // a function which returns an osl with a vector-typed head
    fn returnAVector[] [{| 0, 1, 2 |}, "noError"];
    ```

    And we want to unpack the vector returned by this function, it's just ridiculous:

    ```
    // it's [-2, -1, {| 0, 1, 2 |}, "noError"]
    [-2, -1, ... returnAVector[]];
    // it's [-2, -1, 0, 1, 2]
    [-2, -1, ... ... returnAVector[]];
    // the first `...` here is the same as the `...` above, the second `...` is the unary unpacking operator
    ```

    If it's intolerable, adopting different symbols for osls and vectors is a solution:

    ```
    // if we use .... for osls
    // it's [-2, -1, {| 0, 1, 2 |}, "noError"]
    [-2, -1, .... returnAVector[]];
    // it's [-2, -1, 0, 1, 2]
    [-2, -1, ... returnAVector[]];
    ```

    Another solution is do not apply `...` to osls, with one critical exception:

    ```
    // RE
    [-2, -1, ...[0, 1, 2]]
    // the corrent way
    [-2, -1] .. [0, 1, 2]
    // it's [-2, -1, 0, 1, 2] now
    [-2, -1, ... returnAVector[]];
    // EXCEPTION HERE is the only way to implement packing vector
    // it's {| -2, -1, 0, 1, 2 |}
    {| ...([-2, -1] .. [0, 1, 2]) |}
    ```

    Or maybe a new operator which explicitly convert osls to values:

    ```
    // it's [-2, -1, {| 0, 1, 2 |}, "noError"]
    [-2, -1, ... returnAVector[]];
    // it's [-2, -1, 0, 1, 2]
    [-2, -1, ... `to_value returnAVector[]];
    ```

    It seems even worse here, but this operator maybe useful elsewhere ... 

    Which one is the best way?

    *Update (2020/09/13)*

    Use `..` for osls.

    ```
    // it's [-2, -1, {| 0, 1, 2 |}, "noError"]
    [-2, -1, .. returnAVector[]];
    // it's [-2, -1, 0, 1, 2]
    [-2, -1, ... returnAVector[]];
    ```

    Add a warning here:

    ```
    // warn: `..` is used to unpacking osls only
    // it will be [-2, -1, 0, {| 1, 2 |}]
    [-2, -1, 0, ..{| 1, 2 |}]
    ```

- [x] *(2020/09/09)* Introducing *Discard* (`_`)

    
    ```
    // [x, y] == 1, 3
    [x, _, y] = [1, 2, 3];
    ```

    **WARNING**: do not use Trie here!!!

- [ ] *(2020/09/10)* Choose another license

    Candidates:
    
    - [ ] Apache 2.0
    - [ ] MPL
    
    Candidates for text materials:
    
    - [ ] CC BY-NC-SA 4.0
    - [ ] Same as above

- [ ] *(2020/09/10)* Make projects separate repos

    When:
    
    - When projects are stable enough for first preview release
    - No longer tons of new features to implement
    
    Potential issues:
    
    - Project reference
    
        May be resolved by a private NuGet server and CD system.

    *(2020/10/12) Breakup plan*:

    - `Glos.Code`: Model, builder and de/serialization of codes executable on Glos.
    - `Glos`: Glos execution engine.
    - `Glug`: Glug compiler.
    - `Gliep`: Runtime.

- [ ] *(2020/09/10)* Better CE system

    - [ ] From exception to message
    - [ ] Error recovery

- [ ] *(2020/09/10)* GlosUnit serialization

    - [ ] Text-based vs. Binary-based
    - [ ] Debug info in serialized unit
        - [ ] first of all, ip to location in file

- [ ] *(2020/09/10)* Better RE system

    Use debug info above to convert ip to location in file, etc.

    *Update (2020/12/11)*

    RE Handling structure

    - [ ] Add `try`, `endtry` and `throw` op.
    - [ ] Add `try` block and in glug.

        Examples:

        ```
        # parentheses optional
        return 0 + try (1 + nil) catch() (return -1);
        ```

        will be compiled to:

        ```
        000000: ld.0
        000001: try 00000f      ; enter try block
        000006: ld.1
        000007: ldnil
        000008: add
        000009: endtry          ; leave try block
        00000a: b 000011
        00000f: ld.neg1         ; enter catch block
        000010: ret             ; leave catch block
        000011: add
        000012: ret
        ```

        It's forbidden to pop any element from any stack (execution, call, delimiter) if it already exists when entering current try block.

        When entering catch block, all stacks will be restored as if the `try` op were a `b` op, then something (to be determined) containing the exception message and a copy of execution context will be pushed to the stack.

    *Update (2021/02/08)*

    Maybe it's good to throw-then-catch an osl?

    ```
    1 + ([x, y] -> x + y) (try throw [2, 3, 4] catch[a, b] [a, b])
    ```

- [ ] *(2020/09/10)* *(Planning)* Use reflection to generate builtin functions, so they don't need to check parameters' types every time they're called

- [ ] *(2020/12/11)* Coroutine system
    
    - [ ] Add new `GlosValueType`: `Coroutine`

    - [ ] Add `mkc`, `resume`, `yield` op. `resume` works like `call`, `yield` works like `ret`

    - [ ] Add operators for coroutine creating, resuming and yielding (and possible destroying, but it's convenient enough to exit and destroy a coroutine with `return`). Here is a proposal:

        ```
        c = -> fn [a, b] (
            [c, d] = <- [b, a];
            return [d, c];
        );

        [x, y] = c <- [1, 2];
        [z, w] = c <- [4, 5];

        print[x, y, z, w] # [2, 1, 5, 4] expected
        ```

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

- [ ] *(2020/09/09)* Add unpacking vector operator

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

- [ ] *(2020/09/09)* Introducing *Discard* (_)

    
    ```
    [x, _, y] = [1, 2, 3]
    ```

    **WARNING**: do not use Trie here!!!


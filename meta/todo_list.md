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
        [a, b,] = [0, 1,]
        // OK
        y = [p2, p3,] -> p2 - p3;
        ```

    - [ ] **Allowed** in block:

        ```
        // OK
        ( 0; 1; 2; )
        ```
    
    - [ ] **Allowed** in table and vector literal:

        ```
        // OK
        x = { .a: 0, .b: 1, .c: 2, }
        // OK
        y = {| 3, 4, 5, |}
        ```
    
    - [ ] **Prohibited** in for-loop iteration variable list:

        ```
        // CE
        for (x, y,: pairs {| 0, 1, 2 |}) x + y;
        ```
    
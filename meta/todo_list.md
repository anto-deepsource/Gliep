# TODO List

- [ ] *(2020/08/27)* Make for-loop breakable

    ```
    print for (i: iter {| 1, 1, 2, 3, 5, 8, 13 |}) if (i > 5) break i;
    # 8 expected
    ```

- [ ] *(2020/08/27)* Add optional labels for breakable blocks

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

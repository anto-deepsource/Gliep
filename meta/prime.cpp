#include <bitset>
#include <vector>
#include <cstdint>
#include <cstdio>
using namespace std;

constexpr int64_t len = 0X7FEFFFFF;

bitset<len> flag;
vector<int64_t> primes; 
vector<int64_t> selected;

int main() {
    for (int64_t i = 2; i < len; ++i) {
        if (!flag[i]) {
            // printf("found %lld\n", i);
            primes.push_back(i);
        }

        for (auto p: primes) {
            if (i * p >= len) {
                break;
            }

            flag.set(i * p);

            if (i % p == 0) {
                break;
            }
        }
    }

    printf("there are %llu primes in [2, %lld)\n", primes.size(), len);

    auto begin = primes.cbegin() + 1, end = primes.cend();
    while (true) {
        auto current = *begin;
        selected.push_back(current);

        current = int64_t(current * 1.2) + 1;
        begin = lower_bound(begin, end, current);

        if (begin == end) {
            break;
        }
    }

    if (*primes.crbegin() != *selected.crbegin()) {
        selected.push_back(*primes.crbegin());
    }

    printf("there are %llu primes selected:\n", selected.size());

    printf("private static readonly int[] Primes = {\n   ");
    for (auto p: selected) {
        printf(" %lld,", p);
    }
    printf("\n};");

    return 0;
}

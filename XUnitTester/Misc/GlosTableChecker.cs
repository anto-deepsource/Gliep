using System;
using System.Collections.Generic;
using GeminiLab.Glos;
using Xunit;

namespace GeminiLab.XUnitTester.Gliep.Misc {
    public class GlosTableChecker {
        public GlosTable Table { get; }

        public GlosTableChecker(GlosTable table) {
            Table = table;
        }

        public static GlosTableChecker Create(GlosTable table) => new GlosTableChecker(table);

        public GlosTableChecker HasNot(GlosValue key) {
            Assert.False(Table.TryReadEntry(key, out _));
            return this;
        }

        public GlosTableChecker Has(GlosValue key) {
            _keysVisited.Add(key);
            Assert.True(Table.TryReadEntry(key, out _));
            return this;
        }

        public GlosTableChecker Has(GlosValue key, Action<GlosValue> valueChecker) {
            _keysVisited.Add(key);
            Assert.True(Table.TryReadEntry(key, out var value));
            valueChecker(value);
            return this;
        }

        public GlosTableChecker Has(GlosValue key, Predicate<GlosValue> valuePredicate) {
            _keysVisited.Add(key);
            Assert.True(Table.TryReadEntry(key, out var value));
            Assert.True(valuePredicate(value));
            return this;
        }

        private readonly HashSet<GlosValue> _keysVisited = new HashSet<GlosValue>();
        public GlosTableChecker AssertAllKeyChecked() { 
            Assert.Equal(Table.Count, _keysVisited.Count);
            foreach (var key in Table.Keys) {
                Assert.Contains(key, _keysVisited);
            }

            return this;
        }
    }
}

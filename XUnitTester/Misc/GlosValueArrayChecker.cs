using System;
using System.Collections.Generic;

using GeminiLab.Glos.ViMa;

using Xunit;

namespace XUnitTester.Misc {
    public class GlosValueArrayItemChecker {
        public GlosValueArrayChecker Checker { get; }
        public int Position { get; private set; }
        private ref GlosValue Current => ref Checker.Target[Position];

        public GlosValueArrayItemChecker(GlosValueArrayChecker checker, int position) {
            Checker = checker;
            Position = position;
        }

        public GlosValueArrayItemChecker MoveNext() {
            ++Position;
            return this;
        }

        public GlosValueArrayItemChecker AssertPositionInRange() {
            Assert.True(0 <= Position && Position < Checker.Length);
            return this;
        }

        public GlosValueArrayItemChecker AssertEnd() {
            Assert.True(Position == Checker.Length);
            return this;
        }

        public GlosValueArrayItemChecker AssertNil() {
            Current.AssertNil();
            return this;
        }

        public GlosValueArrayItemChecker AssertInteger() {
            AssertPositionInRange();
            Current.AssertInteger();
            return this;
        }

        public GlosValueArrayItemChecker AssertInteger(long value) {
            AssertPositionInRange();
            Assert.Equal(value, Current.AssertInteger());
            return this;
        }

        public GlosValueArrayItemChecker AssertInteger(Action<long> checker) {
            AssertPositionInRange();
            checker(Current.AssertInteger());
            return this;
        }

        public GlosValueArrayItemChecker AssertInteger(Predicate<long> predicate) {
            AssertPositionInRange();
            Assert.True(predicate(Current.AssertInteger()));
            return this;
        }

        public GlosValueArrayItemChecker AssertFloat() {
            AssertPositionInRange();
            Current.AssertFloat();
            return this;
        }

        public GlosValueArrayItemChecker AssertFloat(double value) {
            AssertPositionInRange();
            Assert.Equal(value, Current.AssertFloat());
            return this;
        }

        public GlosValueArrayItemChecker AssertFloat(Action<double> checker) {
            AssertPositionInRange();
            checker(Current.AssertFloat());
            return this;
        }

        public GlosValueArrayItemChecker AssertFloat(Predicate<double> predicate) {
            AssertPositionInRange();
            Assert.True(predicate(Current.AssertFloat()));
            return this;
        }

        public GlosValueArrayItemChecker AssertFloat(ulong binaryRepresentation) {
            AssertPositionInRange();
            Current.AssertFloat();
            Assert.Equal(unchecked((long)binaryRepresentation), Current.ValueNumber.Integer);
            return this;
        }

        public GlosValueArrayItemChecker AssertFloat(Action<ulong> binaryRepresentationChecker) {
            AssertPositionInRange();
            Current.AssertFloat();
            binaryRepresentationChecker(unchecked((ulong)Current.ValueNumber.Integer));
            return this;
        }

        public GlosValueArrayItemChecker AssertFloat(Predicate<ulong> binaryRepresentationPredicate) {
            AssertPositionInRange();
            Current.AssertFloat();
            Assert.True(binaryRepresentationPredicate(unchecked((ulong)Current.ValueNumber.Integer)));
            return this;
        }

        public GlosValueArrayItemChecker AssertBoolean() {
            AssertPositionInRange();
            Current.AssertBoolean();
            return this;
        }

        public GlosValueArrayItemChecker AssertBoolean(bool value) {
            AssertPositionInRange();
            Assert.False(value ^ Current.AssertBoolean());
            return this;
        }

        public GlosValueArrayItemChecker AssertBoolean(Action<bool> checker) {
            AssertPositionInRange();
            checker(Current.AssertBoolean());
            return this;
        }

        public GlosValueArrayItemChecker AssertBoolean(Predicate<bool> predicate) {
            AssertPositionInRange();
            Assert.True(predicate(Current.AssertBoolean()));
            return this;
        }

        public GlosValueArrayItemChecker AssertTrue() {
            AssertPositionInRange();
            Assert.True(Current.AssertBoolean());
            return this;
        }

        public GlosValueArrayItemChecker AssertFalse() {
            AssertPositionInRange();
            Assert.False(Current.AssertBoolean());
            return this;
        }

        public GlosValueArrayItemChecker AssertString() {
            AssertPositionInRange();
            Current.AssertString();
            return this;
        }

        public GlosValueArrayItemChecker AssertString(string value) {
            AssertPositionInRange();
            Assert.Equal(value, Current.AssertString());
            return this;
        }

        public GlosValueArrayItemChecker AssertString(Action<string> checker) {
            AssertPositionInRange();
            checker(Current.AssertString());
            return this;
        }

        public GlosValueArrayItemChecker AssertString(Predicate<string> predicate) {
            AssertPositionInRange();
            Assert.True(predicate(Current.AssertString()));
            return this;
        }

        public GlosValueArrayItemChecker AssertTable() {
            AssertPositionInRange();
            Current.AssertTable();
            return this;
        }

        public GlosValueArrayItemChecker AssertTable(Action<GlosTable> checker) {
            AssertPositionInRange();
            checker(Current.AssertTable());
            return this;
        }

        public GlosValueArrayItemChecker AssertTable(Predicate<GlosTable> predicate) {
            AssertPositionInRange();
            Assert.True(predicate(Current.AssertTable()));
            return this;
        }
    }

    public class GlosValueArrayChecker {
        public GlosValue[] Target { get; }
        public int Length { get; }

        public GlosValueArrayChecker(GlosValue[] target) {
            Target = target;
            Length = target.Length;
        }

        public static GlosValueArrayChecker Create(GlosValue[] target) => new GlosValueArrayChecker(target);

        public GlosValueArrayItemChecker FirstOne() => new GlosValueArrayItemChecker(this, 0);
    }

    public class GlosTableChecker {
        public GlosTable Table { get; }

        public GlosTableChecker(GlosTable table) {
            Table = table;
        }

        public static GlosTableChecker Create(GlosTable table) => new GlosTableChecker(table);

        public GlosTableChecker HasNot(GlosValue key) {
            Assert.False(Table.TryReadEntryLocally(key, out _));
            return this;
        }

        public GlosTableChecker Has(GlosValue key) {
            _keysVisited.Add(key);
            Assert.True(Table.TryReadEntryLocally(key, out _));
            return this;
        }

        public GlosTableChecker Has(GlosValue key, Action<GlosValue> valueChecker) {
            _keysVisited.Add(key);
            Assert.True(Table.TryReadEntryLocally(key, out var value));
            valueChecker(value);
            return this;
        }

        public GlosTableChecker Has(GlosValue key, Predicate<GlosValue> valuePredicate) {
            _keysVisited.Add(key);
            Assert.True(Table.TryReadEntryLocally(key, out var value));
            Assert.True(valuePredicate(value));
            return this;
        }

        private HashSet<GlosValue> _keysVisited = new HashSet<GlosValue>();
        public GlosTableChecker AssertAllKeyChecked() { 
            Assert.Equal(Table.Count, _keysVisited.Count);
            foreach (var key in Table.Keys) {
                Assert.Contains(key, _keysVisited);
            }

            return this;
        }
    }
}

using EternalWar.Core.Helpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EternalWar.Core.Tests
{
    [TestFixture]
    internal sealed class ValueCollectionTests
    {
        [Test]
        public void ValueArray_DefaultEmptyAndSequenceEqualityAreConsistent()
        {
            ValueArray<string> defaultArray = default;
            ValueArray<string> empty = ValueArray<string>.Empty;
            ValueArray<string> values = new("a", "b");

            Assert.That(defaultArray, Is.EqualTo(empty));
            Assert.That(defaultArray.Count, Is.Zero);
            string[] expected1 = new[] { "a", "b" };
            Assert.That(values, Is.EqualTo(new ValueArray<string>(expected1)));
            string[] expected2 = new[] { "b", "a" };
            Assert.That(values, Is.Not.EqualTo(new ValueArray<string>(expected2)));
            Assert.That(values.ToString(), Is.EqualTo("[\"a\", \"b\"]"));
            Assert.That(new ValueArray<object?>(null, 1).ToString(), Is.EqualTo("[null, 1]"));
            Assert.Throws<IndexOutOfRangeException>(() => _ = values[-1]);
        }

        [Test]
        public void ValueArray_BuilderCopiesAndExtensionsRemainImmutable()
        {
            ValueArray<int> original = new(1, 2);
            using ValueArray<int>.Builder builder = new(original);
            builder.Add(3);
            builder.Insert(0, 0);
            Assert.That(builder.Contains(2), Is.True);
            Assert.That(builder.IndexOf(3), Is.EqualTo(3));
            Assert.That(builder.Remove(2), Is.True);
            builder.RemoveAt(0);
            int[] copied = new int[2];
            builder.CopyTo(copied, 0);
            ValueArray<int> snapshot = builder.ToValueArray();
            builder.Clear();

            Assert.That(snapshot, Is.EqualTo(new ValueArray<int>(1, 3)));
            Assert.That(original, Is.EqualTo(new ValueArray<int>(1, 2)));
            Assert.That(original.Add(3), Is.EqualTo(new ValueArray<int>(1, 2, 3)));
            int[] addition1 = new[] { 3, 4 };
            int[] addition2 = new[] { 5 };
            Assert.That(original.Add(addition1, addition2), Is.EqualTo(new ValueArray<int>(1, 2, 3, 4, 5)));
            Assert.That(original.Remove(1), Is.EqualTo(new ValueArray<int>(2)));
            int[] subtraction = new[] { 1, 2 };
            Assert.That(original.Remove(subtraction), Is.EqualTo(ValueArray<int>.Empty));
            Assert.That(original.RemoveAt(0), Is.EqualTo(new ValueArray<int>(2)));
            Assert.That(original.ReplaceAt(1, 9), Is.EqualTo(new ValueArray<int>(1, 9)));
        }

        [Test]
        public void ValueArray_BuilderThrowsAfterDisposeAndDisposeIsIdempotent()
        {
            ValueArray<int>.Builder builder = new();
            builder.Add(1);
            builder.Dispose();
            builder.Dispose();
            Assert.Throws<ObjectDisposedException>(() => builder.Add(2));
            Assert.Throws<ObjectDisposedException>(() => _ = builder.Count);
        }

        [Test]
        public void ValueDictionaryEqualityAndHashDoNotDependOnInsertionOrder()
        {
            ValueDictionary<string, int> a = new(new Dictionary<string, int> { ["x"] = 1, ["y"] = 2 });
            ValueDictionary<string, int> same = new(new Dictionary<string, int> { ["y"] = 2, ["x"] = 1 });
            ValueDictionary<string, int> different = new(new Dictionary<string, int> { ["x"] = 1, ["z"] = 2 });

            Assert.That(a, Is.EqualTo(same));
            Assert.That(a.GetHashCode(), Is.EqualTo(same.GetHashCode()));
            Assert.That(a, Is.Not.EqualTo(different));
            Assert.That(a.ToString(), Does.Contain("Count = 2"));
            Assert.That(a.ContainsKey("x"), Is.True);
            Assert.That(a.TryGetValue("missing", out _), Is.False);
            Assert.That(() => _ = a["missing"], Throws.TypeOf<KeyNotFoundException>());
            Assert.That(ValueDictionary<string, int>.Empty.Count, Is.Zero);
        }

        [Test]
        public void ValueDictionaryBuilderSupportsMapOperationsAndImmutableSnapshots()
        {
            using ValueDictionary<string, int>.Builder builder = ValueDictionary<string, int>.CreateBuilder(4);
            builder.Add("a", 1);
            builder.Add(new KeyValuePair<string, int>("b", 2));
            builder.AddRange(new[] { new KeyValuePair<string, int>("c", 3) });
            Assert.That(builder.ContainsKey("a"), Is.True);
            Assert.That(builder.Contains(new KeyValuePair<string, int>("b", 2)), Is.True);
            Assert.That(builder.TryGetValue("b", out int value), Is.True);
            Assert.That(value, Is.EqualTo(2));
            KeyValuePair<string, int>[] copy = new KeyValuePair<string, int>[3];
            builder.CopyTo(copy, 0);
            Assert.That(copy, Has.Length.EqualTo(3));
            Assert.That(builder.Remove(new KeyValuePair<string, int>("b", 2)), Is.True);
            Assert.That(builder.Remove("c"), Is.True);
            ValueDictionary<string, int> snapshot = builder.ToValueDictionary();
            builder["a"] = 9;
            Assert.That(snapshot["a"], Is.EqualTo(1));
            Assert.That(builder["a"], Is.EqualTo(9));
            builder.Clear();
            Assert.That(builder.Count, Is.Zero);
        }

        [Test]
        public void ValueDictionaryBuilderDetectsDuplicateKeysAndDisposedUse()
        {
            ValueDictionary<string, int>.Builder builder = ValueDictionary<string, int>.CreateBuilder();
            builder.Add("a", 1);
            Assert.Throws<ArgumentException>(() => builder.Add("a", 2));
            builder.Dispose();
            builder.Dispose();
            Assert.Throws<ObjectDisposedException>(() => builder.Add("b", 2));
            Assert.Throws<ObjectDisposedException>(() => _ = builder.Count);
        }

        [Test]
        public void SortedValueArraySortsByKeyAndBuilderCanReplaceAndRemove()
        {
            SortedValueArray<int, string> sorted = new((3, "c"), (1, "a"), (2, "b"));
            int[] expected = new[] { 1, 2, 3 };
            Assert.That(sorted.Keys.ToArray(), Is.EqualTo(expected));
            Assert.That(sorted[0], Is.EqualTo("a"));
            Assert.That(sorted[2], Is.EqualTo("c"));
            Assert.That(sorted[2], Is.EqualTo("c"));
            Assert.That(sorted.ContainsKey(2), Is.True);
            Assert.That(sorted.TryGetValue(7, out _), Is.False);
            Assert.That(sorted.ToString(), Is.EqualTo("[(1)=\"a\", (2)=\"b\", (3)=\"c\"]"));
            Assert.That(sorted.Equals(null), Is.False);
            Assert.That(SortedValueArrayExtensions.ToArray(sorted!), Has.Length.EqualTo(3));
            Assert.Throws<ArgumentException>(() => new SortedValueArray<int, string>((1, "a"), (1, "duplicate")));

            using SortedValueArray<int, string>.Builder builder = new(sorted!);
            builder.Add(4, "d");
            Assert.That(builder.ContainsValue("d"), Is.True);
            Assert.That(builder.IndexOfKey(4), Is.EqualTo(3));
            Assert.That(builder.IndexOfValue("missing"), Is.EqualTo(-1));
            Assert.That(builder[0].Key, Is.EqualTo(1));
            Assert.That(builder.Remove(3, out string? removed), Is.True);
            Assert.That(removed, Is.EqualTo("c"));
            Assert.That(builder.Remove(99), Is.False);
            builder.RemoveAt(0);
            Assert.That(builder.Count, Is.EqualTo(2));
            builder.Clear();
            Assert.That(builder.Count, Is.Zero);
        }

        [Test]
        public void SortedValueArrayEqualityUsesOrderedEntriesAndBuilderDisposalIsChecked()
        {
            SortedValueArray<int, string> a = new((1, "a"), (2, "b"));
            SortedValueArray<int, string> b = new((2, "b"), (1, "a"));
            Assert.That(a, Is.EqualTo(b));
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
            Assert.That(a, Is.Not.EqualTo(new SortedValueArray<int, string>((1, "x"))));
            Assert.That(SortedValueArray<int, string>.Empty.Count, Is.Zero);

            SortedValueArray<int, string>.Builder builder = new();
            builder.Add(1, "a");
            builder.Dispose();
            builder.Dispose();
            Assert.Throws<ObjectDisposedException>(() => builder.Add(2, "b"));
            Assert.Throws<ObjectDisposedException>(() => _ = builder.Count);
        }
    }
}

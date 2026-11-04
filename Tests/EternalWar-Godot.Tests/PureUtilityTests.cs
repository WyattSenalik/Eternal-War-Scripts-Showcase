// Original Authors - Wyatt Senalik
using EVLibrary.Extensions;
using EVLibrary.FileIO;
using EVLibrary.FileIO.Extensions;
using EVLibrary.Xml;
using EVLibrary.Xml.XPath;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace EternalWar.Engine.Godot.Tests
{
    [TestFixture]
    internal sealed class PureUtilityTests
    {
        [Test]
        public void DictionaryHelpersFindNullableValuesAndMergeByReplacingExistingKeys()
        {
            Dictionary<string, string?> target = new() { ["replace"] = "old", ["keep"] = null };
            Dictionary<string, string?> source = new() { ["replace"] = "new", ["add"] = "value" };

            Assert.That(target.GetKeyFromValue<string, string?>(null), Is.EqualTo("keep"));
            Assert.That(target.GetKeyFromValue<string, string?>("missing"), Is.Null);
            Assert.That(target.Merge(source), Is.SameAs(target));
            Assert.That(target["replace"], Is.EqualTo("new"));
            Assert.That(target["add"], Is.EqualTo("value"));
            Assert.That(target["keep"], Is.Null);

            Dictionary<string, int> selfMerge = new() { ["key"] = 1 };
            selfMerge.Merge(selfMerge);
            Assert.That(selfMerge["key"], Is.EqualTo(1));
        }

        [Test]
        public void ListMergeAppendsItemsAndHandlesSelfMerge()
        {
            List<int> target = new() { 1 };
            target.Merge(new List<int> { 2, 3 });

            int[] expected = new[] { 1, 2, 3 };
            Assert.That(target, Is.EqualTo(expected));

            target.Merge(target);

            expected = new[] { 1, 2, 3, 1, 2, 3 };
            Assert.That(target, Is.EqualTo(expected));
        }

        [Test]
        public void StringIndentHelpersHandleZeroAndPositiveDepth()
        {
            Assert.That("node".Indent(0), Is.EqualTo("node"));
            Assert.That("node".Indent(2), Is.EqualTo("    node"));
            Assert.That("node".VisibleIndent(0), Is.EqualTo("node"));
            Assert.That("node".VisibleIndent(2), Is.EqualTo("| | node"));
            Assert.Throws<ArgumentOutOfRangeException>(() => "node".Indent(-1));
        }

        [Test]
        public void PathHelpersJoinSegmentsAndStandardizeSlashDirection()
        {
            Assert.That("a/b/c".StandardizeSlashes(), Is.EqualTo("a\\b\\c"));
            Assert.That(StringPathExtensions.CombinePaths("a", "b/c", "d"), Is.EqualTo("a\\b\\c\\d"));
            Assert.That(StringPathExtensions.CombinePaths("a"), Is.EqualTo("a"));
        }

        [Test]
        public void FileSearchHandlesMissingFoldersPatternsAndRecursion()
        {
            string root = Path.Combine(Path.GetTempPath(), $"ew-file-search-{Guid.NewGuid():N}");
            string nested = Path.Combine(root, "nested");
            Directory.CreateDirectory(nested);
            System.IO.File.WriteAllText(Path.Combine(root, "first.blend"), "a");
            System.IO.File.WriteAllText(Path.Combine(root, "ignore.txt"), "b");
            System.IO.File.WriteAllText(Path.Combine(nested, "second.blend"), "c");
            try
            {
                Assert.That(FileSearchHelper.SearchPathForFilesWithExtension(Path.Combine(root, "missing")), Is.Empty);
                IReadOnlyCollection<string> recursive = FileSearchHelper.SearchPathForFilesWithExtension(root, "*.blend");
                Assert.That(recursive.Count, Is.EqualTo(2));
                Assert.That(recursive.All(path => path.EndsWith(".blend", StringComparison.InvariantCulture)), Is.True);
                Assert.That(FileSearchHelper.SearchPathForFilesWithExtension(root, "*.blend", SearchOption.TopDirectoryOnly), Has.Count.EqualTo(1));
                Assert.That(FileSearchHelper.SearchPathForFilesWithExtension(root, "*.txt"), Has.Count.EqualTo(1));
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }

        [Test]
        public void XmlScalarWrappersUseInvariantFormattingAndDefaultInvalidValues()
        {
            CultureInfo oldCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
                IntXml defaultInt = new();
                Assert.That(defaultInt.Element.Name.LocalName, Is.EqualTo(nameof(IntXml)));
                Assert.That(defaultInt.Value, Is.Zero);
                Assert.That(new IntXml("count", 1234).Value, Is.EqualTo(1234));
                IntXml invalidInt = new("bad");
                invalidInt.Element.Value = "not-an-int";
                Assert.That(invalidInt.Value, Is.Zero);
                Assert.That(new IntXml(XName.Get("custom"), 7).Element.Name.LocalName, Is.EqualTo("custom"));

                FloatXml floatXml = new("amount", 1.5f);
                Assert.That(floatXml.Element.Value, Is.EqualTo("1.5"));
                Assert.That(floatXml.Value, Is.EqualTo(1.5f));
                FloatXml invalidFloat = new("bad");
                invalidFloat.Element.Value = "x";
                Assert.That(invalidFloat.Value, Is.Zero);
                Assert.That(new FloatXml(XName.Get("f"), 2.25f).Value, Is.EqualTo(2.25f));

                Assert.That(new BoolXml("enabled", true).Bool, Is.True);
                Assert.That(new BoolXml("disabled", false).Bool, Is.False);
                BoolXml invalidBool = new("bad");
                invalidBool.Element.Value = "invalid";
                Assert.That(invalidBool.Bool, Is.False);
                Assert.That(new DataTypeXml("thing", "custom", 1.25m).Element.Value, Is.EqualTo("1.25"));
            }
            finally
            {
                CultureInfo.CurrentCulture = oldCulture;
            }
        }

        [Test]
        public void BooleanXmlConversionRequiresBooleanTypeMetadata()
        {
            XElement validTrue = new("value", new XAttribute(DataTypeXml.TYPE_ATTRIBUTE_NAME, BoolXml.BoolType), "True");
            XElement validFalse = new("value", new XAttribute(DataTypeXml.TYPE_ATTRIBUTE_NAME, BoolXml.BoolType), "False");
            Assert.That(validTrue.ConvertToBoolXML().Bool, Is.True);
            Assert.That(validFalse.ConvertToBoolXML().Bool, Is.False);
            Assert.Throws<Exception>(() => new XElement("value").ConvertToBoolXML());
            Assert.Throws<Exception>(() => new XElement("value", new XAttribute(DataTypeXml.TYPE_ATTRIBUTE_NAME, "Integer")).ConvertToBoolXML());
        }

        [Test]
        public void XPathSearchFindsElementsContainingClassTypeAndText()
        {
            XDocument document = XDocument.Parse("<root><item class='panel selected' type='Control'>match text</item><other class='other' type='Node'>nothing</other></root>");
            string[] expected = new[] { "item" };
            Assert.That(document.ContainsClass("panel").Select(element => element.Name.LocalName), Is.EqualTo(expected));
            Assert.That(document.ContainsType("Control").Select(element => element.Name.LocalName), Is.EqualTo(expected));
            Assert.That(document.TextMatch("match").Select(element => element.Name.LocalName), Is.EqualTo(expected));
            Assert.That(document.ContainsClass("missing"), Is.Empty);
            Assert.Throws<System.Xml.XPath.XPathException>(() => document.ContainsClass("'").ToList());
        }
    }
}

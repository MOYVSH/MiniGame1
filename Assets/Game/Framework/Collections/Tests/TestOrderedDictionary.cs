using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public class TestClass
{
    public int id;

    public TestClass(int id)
    {
        this.id = id;
    }
}

public class TestOrderedDictionary
{
    private OrderedDictionary<string, TestClass> dic;

    [SetUp]
    public void SetUp()
    {
        dic = new OrderedDictionary<string, TestClass>(true);
    }

    [Test]
    public void Test_Add()
    {
        dic.Add("One", new TestClass(1));
        dic.Add("Two", new TestClass(2));
        dic.Add("Three", new TestClass(3));

        Assert.AreEqual(3, dic.Count);
    }

    [Test]
    public void Test_Indexer()
    {
        Assert.Throws<ArgumentNullException>(() => dic[null] = new TestClass(1));

        dic.Add("One", new TestClass(1));
        dic.Add("Two", new TestClass(2));
        dic.Add("Three", new TestClass(3));

        Assert.AreEqual(1, dic["One"].id);
        Assert.AreEqual(2, dic["Two"].id);
        Assert.AreEqual(3, dic["Three"].id);
    }

    [Test]
    public void Test_Keys()
    {
        dic.Add("One", new TestClass(1));
        dic.Add("Two", new TestClass(2));
        dic.Add("Three", new TestClass(3));

        Assert.AreEqual(3, dic.Keys.Count);
        Assert.IsTrue(dic.Keys.Contains("One"));
        Assert.IsTrue(dic.Keys.Contains("Two"));
        Assert.IsTrue(dic.Keys.Contains("Three"));
    }

    [Test]
    public void Test_Values()
    {
        dic.Add("One", new TestClass(1));
        dic.Add("Two", new TestClass(2));
        dic.Add("Three", new TestClass(3));

        Assert.AreEqual(3, dic.Values.Count);
        var array = dic.Values.ToArray();
        Assert.AreEqual(1, array[0].id);
        Assert.AreEqual(2, array[1].id);
        Assert.AreEqual(3, array[2].id);
    }

    [Test]
    public void Test_Eldest()
    {
        dic.Add("One", new TestClass(1));
        dic.Add("Two", new TestClass(2));
        dic.Add("Three", new TestClass(3));

        Assert.AreEqual("One", dic.Eldest.Key);
        Assert.AreEqual(1, dic.Eldest.Value.id);
    }

    [Test]
    public void Test_AccessOrder()
    {
        dic.Add("One", new TestClass(1));
        dic.Add("Two", new TestClass(2));
        dic.Add("Three", new TestClass(3));

        var a = dic["One"];
        Assert.AreEqual("Two", dic.Eldest.Key);
        Assert.AreEqual(1, a.id);
        Assert.AreEqual(2, dic.Eldest.Value.id);

        a = dic["Two"];
        Assert.AreEqual(2, a.id);
        Assert.AreEqual("Three", dic.Eldest.Key);
        Assert.AreEqual(3, dic.Eldest.Value.id);

        a = dic["One"];
        Assert.AreEqual(1, a.id);
        Assert.AreEqual("Three", dic.Eldest.Key);
        Assert.AreEqual(3, dic.Eldest.Value.id);
    }

    [Test]
    public void Test_Clear()
    {
        dic.Add("One", new TestClass(1));
        dic.Add("Two", new TestClass(2));
        dic.Add("Three", new TestClass(3));

        Assert.AreEqual(3, dic.Count);

        dic.Clear();

        Assert.AreEqual(0, dic.Count);
        Assert.AreEqual((KeyValuePair<string, TestClass>) default, dic.Eldest);
    }

    [Test]
    public void Test_ContainsKey()
    {
        dic.Add("One", new TestClass(1));
        dic.Add("Two", new TestClass(2));
        dic.Add("Three", new TestClass(3));

        Assert.IsTrue(dic.ContainsKey("One"));
        Assert.IsTrue(dic.ContainsKey("Two"));
        Assert.IsTrue(dic.ContainsKey("Three"));
        Assert.IsFalse(dic.ContainsKey("Four"));
    }

    [Test]
    public void Test_Enumerator()
    {
        dic.Add("One", new TestClass(1));
        dic.Add("Two", new TestClass(2));
        dic.Add("Three", new TestClass(3));

        using var ie = dic.GetEnumerator();
        ie.MoveNext();

        Assert.AreEqual("One", ie.Current.Key);
        Assert.AreEqual(1, ie.Current.Value.id);

        ie.MoveNext();
        Assert.AreEqual("Two", ie.Current.Key);
        Assert.AreEqual(2, ie.Current.Value.id);

        ie.MoveNext();
        Assert.AreEqual("Three", ie.Current.Key);
        Assert.AreEqual(3, ie.Current.Value.id);

        Assert.IsFalse(ie.MoveNext());
    }

    [Test]
    public void Test_Remove()
    {
        dic.Add("One", new TestClass(1));
        dic.Add("Two", new TestClass(2));
        dic.Add("Three", new TestClass(3));

        dic.Remove("One");
        using (var ie = dic.GetEnumerator())
        {
            ie.MoveNext();
            Assert.AreEqual("Two", ie.Current.Key);
            Assert.AreEqual(2, ie.Current.Value.id);

            ie.MoveNext();
            Assert.AreEqual("Three", ie.Current.Key);
            Assert.AreEqual(3, ie.Current.Value.id);

            Assert.IsFalse(ie.MoveNext());
        }

        dic.Remove("Three");
        using (var ie = dic.GetEnumerator())
        {
            ie.MoveNext();
            Assert.AreEqual("Two", ie.Current.Key);
            Assert.AreEqual(2, ie.Current.Value.id);

            Assert.IsFalse(ie.MoveNext());
        }

        Assert.AreEqual(1, dic.Count);
    }
    
    [Test]
    public void Test_TryGetValue()
    {
        dic.Add("One", new TestClass(1));
        dic.Add("Two", new TestClass(2));
        dic.Add("Three", new TestClass(3));

        Assert.IsTrue(dic.TryGetValue("One",out var _));
        Assert.IsTrue(dic.TryGetValue("Two",out var _));
        Assert.IsTrue(dic.TryGetValue("Three",out var _));
        Assert.IsFalse(dic.TryGetValue("Four",out var _));
    }
}
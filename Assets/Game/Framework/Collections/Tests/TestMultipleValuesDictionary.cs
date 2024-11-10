using System;
using System.Collections.Generic;
using NUnit.Framework;

public class TestMultipleValuesDictionary
{
    [Test]
    public void Test_Add()
    {
        var dic = new MultipleValuesDictionary<string, int>();
        dic.Add("a", 1);
        dic.Add("b", 2);
        dic.Add("c", 3);

        Assert.Throws<InvalidOperationException>(() => dic.Add("c", 3));

        dic.Add("c", 4);

        Assert.AreEqual(3, dic.Count);
    }

    [Test]
    public void Test_Indexer()
    {
        var dic = new MultipleValuesDictionary<string, int>();
        dic.Add("a", 1);
        dic.Add("b", 2);
        dic.Add("c", 3);
        dic.Add("c", 4);

        Assert.AreEqual((List<int>) default, dic["d"]);

        Assert.AreEqual(2, dic["c"].Count);

        dic["a"] = new List<int>() {5, 6};

        Assert.AreEqual(5, dic["a"][0]);
        Assert.AreEqual(6, dic["a"][1]);

        dic["c"] = new List<int>() {7};

        Assert.AreEqual(1, dic["c"].Count);
        Assert.AreEqual(7, dic["c"][0]);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            var t = dic["c"][1];
        });
    }

    [Test]
    public void Test_Indexer2()
    {
        var dic = new MultipleValuesDictionary<string, int>();
        dic.Add("a", 1);
        dic.Add("b", 2);
        dic.Add("c", 3);
        dic.Add("c", 4);
        dic.Add("d", 4);

        Assert.AreEqual(2, dic[4].Count);
        Assert.AreEqual("c", dic[4][0]);
        Assert.AreEqual("d", dic[4][1]);
    }

    [Test]
    public void Test_TryGetValue()
    {
        var dic = new MultipleValuesDictionary<string, int>();
        dic.Add("a", 1);
        dic.Add("b", 2);
        dic.Add("c", 3);
        dic.Add("c", 4);
        dic.Add("d", 4);

        Assert.IsFalse(dic.TryGetValue(5, out var _));
        Assert.IsFalse(dic.TryGetValue("e", out var _));

        dic.TryGetValue("c", out var value);
        Assert.AreEqual(2, value.Count);

        dic.TryGetValue(4, out var value1);
        Assert.AreEqual(2, value1.Count);

        dic.TryGetValue("a", out var value2);
        Assert.AreEqual(1, value2.Count);

        dic.TryGetValue(3, out var value3);
        Assert.AreEqual(1, value3.Count);
    }

    [Test]
    public void Test_Clear()
    {
        var dic = new MultipleValuesDictionary<string, int>();
        dic.Add("a", 1);
        dic.Add("b", 2);
        dic.Add("c", 3);
        dic.Add("c", 4);
        dic.Add("d", 4);

        dic.Clear();

        Assert.AreEqual(0, dic.Count);

        Assert.IsFalse(dic.TryGetValue("a", out var _));
        Assert.IsFalse(dic.TryGetValue("b", out var _));
        Assert.IsFalse(dic.TryGetValue("c", out var _));
        Assert.IsFalse(dic.TryGetValue("d", out var _));

        Assert.IsFalse(dic.TryGetValue(1, out var _));
        Assert.IsFalse(dic.TryGetValue(2, out var _));
        Assert.IsFalse(dic.TryGetValue(3, out var _));
        Assert.IsFalse(dic.TryGetValue(4, out var _));
    }

    [Test]
    public void Test_Remove()
    {
        var dic = new MultipleValuesDictionary<string, int>();
        dic.Add("a", 1);
        dic.Add("b", 2);
        dic.Add("c", 3);
        dic.Add("c", 4);
        dic.Add("d", 4);

        dic.Remove("a");
        Assert.AreEqual((string) default, dic[1]);
        
        dic.Remove(4);
        Assert.AreEqual(1, dic["c"].Count);
        
        dic.Remove("c");
        Assert.AreEqual((List<int>)default, dic[4]);
    }

    [Test]
    public void Test_Keys()
    {
        var dic = new MultipleValuesDictionary<string, int>();
        dic.Add("a", 1);
        dic.Add("b", 2);
        dic.Add("c", 3);
        dic.Add("c", 4);
        dic.Add("d", 4);

        Assert.AreEqual(4, dic.Keys.Count);

        dic.Remove("d");
        Assert.AreEqual(3, dic.Keys.Count);

        dic.Remove("c");
        Assert.AreEqual(2, dic.Keys.Count);

        dic.Clear();
        Assert.AreEqual(0, dic.Keys.Count);
    }

    [Test]
    public void Test_SameTypes()
    {
        Assert.Throws<InvalidOperationException>(() => new MultipleValuesDictionary<int, int>());
    }
}
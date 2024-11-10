using System;
using NUnit.Framework;
using UnityEngine;

public class TestMultipleKeysDictionary
{
    [Test]
    public void Test_Add()
    {
        var dic = new MultipleKeysDictionary<string, int>();

        dic.Add("a", 1);
        dic.Add("b", 2);
        dic.Add("c", 3);
        dic.Add("d", 1);

        Assert.AreEqual(1, dic["a"]);
        Assert.AreEqual(2, dic["b"]);
        Assert.AreEqual(3, dic["c"]);
        Assert.AreEqual(1, dic["d"]);
        
        Assert.That(dic.Count,Is.EqualTo(3));

        Assert.Throws<InvalidOperationException>(() => dic.Add("d", 4));
    }
    
    [Test]
    public void Test_Indexer()
    {
        var dic = new MultipleKeysDictionary<string, int>();

        dic.Add("a", 1);
        dic.Add("b", 1);
        dic.Add("c", 2);
        
        Assert.AreEqual((int)default,dic["d"]);

        dic["a"] = 100;
        dic["d"] = 2;
        dic["e"] = 2;
        Assert.AreEqual(100, dic["a"]);
        Assert.AreEqual(1, dic["b"]);
        Assert.AreEqual(2, dic["c"]);
        
        Assert.That(dic.Count,Is.EqualTo(3));

        
        dic["b"] = 100;
        Assert.AreEqual(100, dic["a"]);
        Assert.AreEqual(100, dic["b"]);

        dic["e"] = 5;
        Assert.AreEqual(5, dic["e"]);
    }

    [Test]
    public void Test_Remove()
    {
        var dic = new MultipleKeysDictionary<string, int>();

        dic.Add("a", 1);
        dic.Add("b", 2);
        dic.Add("c", 3);
        dic.Add("d", 3);
        
        dic.Remove("a");
        Assert.AreEqual(2, dic.Count);
        Assert.IsFalse(dic.TryGetValue("a",out var _));
        
        dic.Remove("c");
        Assert.AreEqual(1, dic.Count);
        Assert.IsFalse(dic.TryGetValue("c",out var _));
        Assert.IsFalse(dic.TryGetValue("d",out var _));
    }

    [Test]
    public void Test_Clear()
    {
        var dic = new MultipleKeysDictionary<string, int>();

        dic.Add("a", 1);
        dic.Add("b", 2);
        dic.Add("c", 3);
        dic.Add("d", 3);
        
        dic.Clear();
        
        Assert.AreEqual(0, dic.Count);
        Assert.IsFalse(dic.TryGetValue("a",out var _));
        Assert.IsFalse(dic.TryGetValue("b",out var _));
        Assert.IsFalse(dic.TryGetValue("c",out var _));
        Assert.IsFalse(dic.TryGetValue("d",out var _));
    }
    
    [Test]
    public void Test_SameTypes()
    {
        Assert.Throws<InvalidOperationException>(() => new MultipleKeysDictionary<int, int>());
    }
}
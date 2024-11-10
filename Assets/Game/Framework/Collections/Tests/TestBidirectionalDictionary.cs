using System;
using NUnit.Framework;

public class TestBidirectionalDictionary
{
    [Test]
    public void Test_Indexer()
    {
        var dic = new BidirectionalDictionary<int, string>();

        dic[1] = "One";
        dic[2] = "Two";
        dic[3] = "Three";

        Assert.AreEqual(dic[1], "One");
        Assert.AreEqual(dic[2], "Two");
        Assert.AreEqual(dic[3], "Three");

        Assert.AreEqual(dic["One"], 1);
        Assert.AreEqual(dic["Two"], 2);
        Assert.AreEqual(dic["Three"], 3);
    }

    [Test]
    public void Test_Add()
    {
        var dic = new BidirectionalDictionary<int, string>();

        dic.Add(1, "One");
        dic.Add(2, "Two");
        dic.Add(3, "Three");

        Assert.AreEqual(dic[1], "One");
        Assert.AreEqual(dic[2], "Two");
        Assert.AreEqual(dic[3], "Three");
    }

    [Test]
    public void Test_Clear()
    {
        var dic = new BidirectionalDictionary<int, string>();

        dic.Add(1, "One");
        dic.Add(2, "Two");
        dic.Add(3, "Three");

        Assert.AreEqual(dic.Count, 3);

        dic.Clear();

        Assert.AreEqual(dic.Count, 0);
    }

    [Test]
    public void Test_Remove()
    {
        var dic = new BidirectionalDictionary<int, string>();

        dic.Add(1, "One");
        dic.Add(2, "Two");
        dic.Add(3, "Three");

        dic.Remove(1);
        dic.Remove("Two");

        Assert.IsFalse(dic.TryGetValue(1, out _));
        Assert.IsFalse(dic.TryGetValue("Two", out _));
        Assert.IsTrue(dic.TryGetValue(3, out _));
        Assert.IsTrue(dic.TryGetValue("Three", out _));
    }
    
    [Test]
    public void Test_SameTypes()
    {
        Assert.Throws<InvalidOperationException>(() => new BidirectionalDictionary<int, int>());
    }
}
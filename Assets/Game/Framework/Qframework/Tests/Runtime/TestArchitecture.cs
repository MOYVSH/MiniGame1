using System;
using UnityEngine;

namespace QFramework.Tests
{
    public class TestArchitecture : Architecture<TestArchitecture>
    {
        protected override void Init()
        {
            var pathProvider = new TestPathProvider();
            RegisterUtility<IPrefs>(new FilePrefs(pathProvider));
            RegisterUtility<IPersistentFile>(new PersistentFile(pathProvider));
        }
    }

    public class TestPathProvider : IPathProvider
    {
        public string GetSavePath()
        {
            return Application.persistentDataPath + $"/Tests/";
        }

        public string GetLoadPath()
        {
            return Application.persistentDataPath + $"/Tests/";
        }
    }
}
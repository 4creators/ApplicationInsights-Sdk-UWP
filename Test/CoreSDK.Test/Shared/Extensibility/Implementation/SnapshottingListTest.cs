﻿// <copyright file="SnapshottingListTest.cs" company="Microsoft">
// Copyright © Microsoft. All Rights Reserved.
// </copyright>

namespace Microsoft.ApplicationInsights.Extensibility.Implementation
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;

#if NET40 || NET45 || NET46
    using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif
    using Assert = Xunit.Assert;

    [TestClass]
    public class SnapshottingListTest
    {
        [TestClass]
        public class Class : SnapshottingListTest
        {
            [TestMethod]
            public void IsInternalAndNotMeantForPublicConsumption()
            {
                Assert.False(typeof(SnapshottingList<>).GetTypeInfo().IsPublic);
            }

            [TestMethod]
            public void ClassImplementsIListForCompatibilityWithPublicApis()
            {
                Assert.True(typeof(IList<object>).IsAssignableFrom(typeof(SnapshottingList<object>)));
            }
        }

        [TestClass]
        public class Constructor : SnapshottingListTest
        {
            [TestMethod]
            public void CreatesNewList()
            {
                var target = new TestableSnapshottingList<object>();
                Assert.NotNull(target.Collection);
            }
        }

        [TestClass]
        public class CreateSnapshot : SnapshottingListTest
        {
            [TestMethod]
            public void CreatesCloneOfGivenList()
            {
                var target = new TestableSnapshottingList<object>();

                var item = new object();
                var input = new List<object> { item };
                IList<object> output = target.CreateSnapshot(input);

                Assert.Same(item, output[0]);
            }
        }

        [TestClass]
        public class IndexOf : SnapshottingListTest
        {
            [TestMethod]
            public void ReturnsIndexOfGivenItemInSnapshot()
            {
                var target = new TestableSnapshottingList<object>();
                var item = new object();
                target.Snapshot = new List<object> { item };

                Assert.Equal(0, target.IndexOf(item));
            }
        }

        [TestClass]
        public class Insert : SnapshottingCollectionTest
        {
            [TestMethod]
            public void InsertsItemInListAtTheSpecifiedIndex()
            {
                var target = new TestableSnapshottingList<object>();
                var item = new object();

                target.Insert(0, item);

                Assert.Same(item, target.Collection[0]);
            }

            [TestMethod]
            public void ResetsSnapshotSoThatItIsRecreatedAtNextRead()
            {
                var target = new TestableSnapshottingList<object>();
                target.GetSnapshot();

                target.Insert(0, null);

                Assert.Null(target.Snapshot);
            }

            [TestMethod]
            public void LocksCollectionForThreadSafety()
            {
                Task anotherThread;
                var target = new TestableSnapshottingList<object>();
                lock (target.Collection)
                {
#if !NETFX_CORE
					anotherThread = TaskEx.Run(() => target.Insert(0, new object()));
#else
					anotherThread = Task.Run(() => target.Insert(0, new object()));
#endif
					Assert.False(anotherThread.Wait(20));
                }

                Assert.True(anotherThread.Wait(20));
            }
        }

        [TestClass]
        public class RemoveAt : SnapshottingListTest
        {
            [TestMethod]
            public void RemovesItemAtTheSpecifiedIndexInList()
            {
                var target = new TestableSnapshottingList<object> { null };

                target.RemoveAt(0);

                Assert.Equal(0, target.Collection.Count);
            }

            [TestMethod]
            public void ResetsSnapshotSoThatItIsRecreatedAtNextRead()
            {
                var target = new TestableSnapshottingList<object> { null };
                target.GetSnapshot();

                target.RemoveAt(0);

                Assert.Null(target.Snapshot);
            }

            [TestMethod]
            public void LocksCollectionForThreadSafety()
            {
                Task anotherThread;
                var target = new TestableSnapshottingList<object> { null };
                lock (target.Collection)
                {
#if !NETFX_CORE
					anotherThread = TaskEx.Run(() => target.RemoveAt(0));
#else
					anotherThread = Task.Run(() => target.RemoveAt(0));
#endif
					Assert.False(anotherThread.Wait(20));
                }

                Assert.True(anotherThread.Wait(20));
            }
        }

        [TestClass]
        public class Item : SnapshottingListTest
        {
            [TestMethod]
            public void GetterReturnsSnapshotItemAtSpecifiedIndex()
            {
                var target = new TestableSnapshottingList<object>();
                var item = new object();
                target.Snapshot = new List<object> { item };

                Assert.Same(item, target[0]);
            }

            [TestMethod]
            public void SetterReplacesItemAtTheSpecifiedIndexInList()
            {
                var target = new TestableSnapshottingList<object> { null };
                var item = new object();

                target[0] = item;

                Assert.Same(item, target.Collection[0]);
            }

            [TestMethod]
            public void SetterResetsSnapshotSoThatItIsRecreatedAtNextRead()
            {
                var target = new TestableSnapshottingList<object> { null };
                target.GetSnapshot();

                target[0] = new object();

                Assert.Null(target.Snapshot);
            }

            [TestMethod]
            public void SetterLocksCollectionForThreadSafety()
            {
                Task anotherThread;
                var target = new TestableSnapshottingList<object> { null };
                lock (target.Collection)
                {
#if !NETFX_CORE
					anotherThread = TaskEx.Run(() => target[0] = new object());
#else
					anotherThread = Task.Run(() => target[0] = new object());
#endif
					Assert.False(anotherThread.Wait(20));
                }

                Assert.True(anotherThread.Wait(20));
            }
        }

        private class TestableSnapshottingList<T> : SnapshottingList<T>
        {
            public new IList<T> Collection
            {
                get { return base.Collection; }
            }

            public IList<T> Snapshot
            {
                get { return this.snapshot; }
                set { this.snapshot = value; }
            }

            public new IList<T> CreateSnapshot(IList<T> collection)
            {
                return base.CreateSnapshot(collection);
            }

            public new IList<T> GetSnapshot()
            {
                return base.GetSnapshot();
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class HLAreaTestSuite
    {
        // A Test behaves as an ordinary method
        [Test]
        public void TestHLAreaInstantiation()
        {

            HLArea area = CreateDummyArea();
            Assert.True(area.Connections.Count == 0);
            Assert.True(area.BackConnection.Count == 0);
            Assert.True(area.UnusedConnections.Count == 1);
            Assert.True(area.UnusedConnections[0].Position == new Vector2(0, 5));
            Assert.True(area.UnusedConnections[0].Rotation == new Vector2(0, 1));
            Assert.True(area.ConnectionPoints.Count == 1);
            Assert.True(area.ConnectionPoints[0].Position == new Vector2(0, 5));
            Assert.True(area.ConnectionPoints[0].Rotation == new Vector2(0, 1));
            Assert.AreEqual(area.rect, new Rect(-10, -5, 20, 10), area.rect.ToString());
        }

        [Test]
        public void TestHLAreaRotationBy90() {
            HLArea area = CreateDummyArea();
            area.RotateAroundRectPos(90);
            Assert.True(area.UnusedConnections[0].Position == new Vector2(-5, 0));
            Assert.True(area.UnusedConnections[0].Rotation == new Vector2(-1, 0));
            Assert.True(area.ConnectionPoints[0].Position == new Vector2(-5, 0));
            Assert.True(area.ConnectionPoints[0].Rotation == new Vector2(-1, 0));
            Assert.AreEqual(area.rect, new Rect(-10,-5,-10,20), area.rect.ToString());
        }
        [Test]
        public void TestHLAreaRotationBy180()
        {
            HLArea area = CreateDummyArea();
            area.RotateAroundRectPos(180);
            Assert.True(area.UnusedConnections[0].Position == new Vector2(0, -5));
            Assert.True(area.UnusedConnections[0].Rotation == new Vector2(0, -1));
            Assert.True(area.ConnectionPoints[0].Position == new Vector2(0, -5));
            Assert.True(area.ConnectionPoints[0].Rotation == new Vector2(0, -1));
            Assert.AreEqual(area.rect, new Rect(-10, -5, -20, -10), area.rect.ToString());
        }
        [Test]
        public void TestHLAreaRotationBy270()
        {
            HLArea area = CreateDummyArea();
            area.RotateAroundRectPos(270);
            Assert.True(area.UnusedConnections[0].Position == new Vector2(5, 0));
            Assert.True(area.UnusedConnections[0].Rotation == new Vector2(1, 0));
            Assert.True(area.ConnectionPoints[0].Position == new Vector2(5, 0));
            Assert.True(area.ConnectionPoints[0].Rotation == new Vector2(1, 0));
            Assert.AreEqual(area.rect, new Rect(-10, -5, 10, -20), area.rect.ToString());
        }
        [Test]
        public void TestHLAreaRotationBy360()
        {
            HLArea area = CreateDummyArea();
            area.RotateAroundRectPos(360);
            Assert.True(area.UnusedConnections[0].Position == new Vector2(0, 5));
            Assert.True(area.UnusedConnections[0].Rotation == new Vector2(0, 1));
            Assert.True(area.ConnectionPoints[0].Position == new Vector2(0, 5));
            Assert.True(area.ConnectionPoints[0].Rotation == new Vector2(0, 1));
            Assert.AreEqual(area.rect, new Rect(-10, -5, 20, 10), area.rect.ToString());
        }
        [Test]
        public void TestHLAreaAddForwardConnection()
        {
            HLArea area = CreateDummyArea();
            area.AddConnection(new Connection(0,0,1,0),false);
            Assert.True(area.Connections.Count == 1);
            Assert.True(area.BackConnection.Count == 0);
            Assert.True(area.UnusedConnections.Count == 0);
            Assert.True(area.ConnectionPoints.Count == 1);
            Assert.True(area.ConnectionPoints[0].Position == new Vector2(0, 5));
        }
        [Test]
        public void TestHLAreaAddBackwardConnection()
        {
            HLArea area = CreateDummyArea();
            area.AddConnection(new Connection(0, 0, 1, 0), true);
            Assert.True(area.Connections.Count == 0);
            Assert.True(area.BackConnection.Count == 1);
            Assert.True(area.UnusedConnections.Count == 0);
            Assert.True(area.ConnectionPoints.Count == 1);
            Assert.True(area.ConnectionPoints[0].Position == new Vector2(0, 5));
        }
        [Test]
        public void TestHLAreaMaxConnectionReached()
        {
            HLArea area = CreateDummyArea();
            area.AddConnection(new Connection(0, 0, 1, 0), true);
            Assert.True(area.IsMaxConnectionsReached());
        }
        [Test]
        public void TestHLAreaMaxConnectionNotReached()
        {
            HLArea area = CreateDummyArea();
            Assert.False(area.IsMaxConnectionsReached());
        }

        private HLArea CreateDummyArea() {
            List<ConnectionPoint> points = new List<ConnectionPoint>();
            points.Add(new ConnectionPoint(new Vector2(0, 5), new Vector2(0, 1)));
            HLAreaData data = new HLAreaData(new Vector2(20, 10), new GameObject(), points);
            return new HLArea(data, Vector2.zero);
        }
    }
}
//// A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
//// `yield return null;` to skip a frame.
//[UnityTest]
//public IEnumerator TestSuiteWithEnumeratorPasses()
//{
//    // Use the Assert class to test conditions.
//    // Use yield to skip a frame.
//    yield return null;
//}
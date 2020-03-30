using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class HLMapTestSuite
    {

        [Test]
        public void TestDoesntOverlap()
        {
            HLArea area1 = HLAreaTestSuite.CreateDummyArea();
            HLArea area2 = HLAreaTestSuite.CreateDummyArea();
            HLArea area3 = HLAreaTestSuite.CreateNoneConnectingDummyArea();
            HLMap map = new HLMap(new List<GameObject>());
            map.areas.Add(area1);
            Assert.IsFalse(map.DoesntOverlap(area2.rect));
            area2.rect.position = new Vector2(-10,5);
            Assert.IsTrue(map.DoesntOverlap(area2.rect));
            area2.rect.position = new Vector2(-10, 10);
            Assert.IsTrue(map.DoesntOverlap(area2.rect));
            area3.rect = new Rect(10, 10, -20, -10);
            Assert.IsFalse(map.DoesntOverlap(area3.rect));
        }

    }
}
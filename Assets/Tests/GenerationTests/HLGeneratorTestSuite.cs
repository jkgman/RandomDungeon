using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class HLGeneratorTestSuite
    {
        [Test]
        public void TestCanConnectValidAreas()
        {
            HLMap map = new HLMap();
            HLArea area = HLAreaTestSuite.CreateDummyArea();
            map.areas.Add(area);
            HLArea area2 = HLAreaTestSuite.CreateDummyArea();

            Assert.IsTrue(HLGenerator.CanConnect(map, area2,0,0,0));
            Assert.AreEqual(new Vector2(10, 15), area2.rect.position);
            Assert.AreEqual(new Vector2(-20, -10), area2.rect.size);
            Assert.AreEqual(new Vector2(0, -5), area2.ConnectionPoints[0].Position);
            Assert.AreEqual(new Vector2(0, -1), area2.ConnectionPoints[0].Rotation);
        }

        [Test]
        public void TestCanConnectHandlesInvalidAreas()
        {
            HLMap map = new HLMap();
            HLArea area = HLAreaTestSuite.CreateDummyArea();
            map.areas.Add(area);
            HLArea area2 = HLAreaTestSuite.CreateNoneConnectingDummyArea();
            bool statement = HLGenerator.CanConnect(map, area2, 0, 0, 0);
            area.PrintAreaSummary(true);
            area2.PrintAreaSummary(true);
            Assert.IsFalse(statement);
        }

        [Test]
        public void TestCanFindValidConnection()
        {

        }

    }
}

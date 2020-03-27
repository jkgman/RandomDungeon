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
        public void TestReverseConnection()
        {
            Connection conn = new Connection(0, 2, 1, 3);
            Connection desired = new Connection(1, 3, 0, 2);
            Connection reversed = Connection.ReverseConnection(conn);

            Assert.AreEqual(desired, reversed);
        }

    }
}
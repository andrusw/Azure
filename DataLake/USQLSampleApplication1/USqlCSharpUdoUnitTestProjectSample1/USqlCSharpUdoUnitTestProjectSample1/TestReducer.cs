using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using Microsoft.Analytics.UnitTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using USqlCSharpUdoSample;

namespace USqlCSharpUdoUnitTestProjectSample1
{
    /// <summary>
    /// Summary description for TestReducer
    /// </summary>
    [TestClass]
    public class TestReducer
    {
        public TestReducer()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestMyReducer()
        {
            USqlColumn<string> col1 = new USqlColumn<string>("Query");
            USqlColumn<string> col2 = new USqlColumn<string>("Market");
            USqlColumn<int> col3 = new USqlColumn<int>("Latency");
            List<IColumn> columns = new List<IColumn> { col1, col2, col3 };
            USqlSchema schema = new USqlSchema(columns);
            IUpdatableRow output = new USqlRow(schema, null).AsUpdatable();

            //Generate Rowset with specified values
            List<object[]> values = new List<object[]>{
                new object[3] { "Query1","Market1", 1 },
                new object[3] { "Query2","Market2", 2 },
                new object[3] { "Query3","Market3", 3 },
                new object[3] { "Query4","Market4", 4 },
            };

            IEnumerable<IRow> rows = UnitTestHelper.CreateRowsFromValues(schema, values);
            IRowset rowset = UnitTestHelper.GetRowsetFromCollection(rows, output.AsReadOnly());

            //Create UDO instance. The reducer should get the largest latency
            MyReducer reducer = new MyReducer();
            foreach (IRow r in reducer.Reduce(rowset, output))
            {
                Assert.IsTrue(rowset.Schema.Count == 3);

            }
            //Make sure the reducer returns the row of the largest latency
            Assert.IsTrue(output.Get<int>("Latency") == 4);
        }
    }
}

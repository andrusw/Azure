using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using Microsoft.Analytics.UnitTest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using USqlCSharpUdoSample;

namespace USqlCSharpUdoUnitTestProjectSample1
{
    [TestClass]
    public class TestExtractor
    {
        public TestExtractor()
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

        public IRow RowGenerator()
        {
            //generate the schema
            USqlColumn<string> col1 = new USqlColumn<string>("Market");
            USqlColumn<string> col2 = new USqlColumn<string>("Query");
            USqlColumn<int> col3 = new USqlColumn<int>("Latency");
            List<IColumn> columns = new List<IColumn> { col1, col2, col3 };
            USqlSchema schema = new USqlSchema(columns);
            return new USqlRow(schema, null);
        }

        [TestMethod]
        [DeploymentItem(@"Input\searchlog.txt")]
        public void TestMyExtractor()
        {
            IUpdatableRow output = RowGenerator().AsUpdatable();

            using (FileStream stream = new FileStream(@"searchlog.txt", FileMode.Open))
            {
                //Read input file 
                USqlStreamReader reader = new USqlStreamReader(stream);
                //Run the UDO
                MyExtractor extractor = new MyExtractor();
                List<IRow> result = extractor.Extract(reader, output).ToList();
                //Verify the schema
                Assert.IsTrue(result[0].Schema.Count == 3);
                //Verify the result
                Assert.IsTrue(result[0].Get<string>("Market") == "en-gb");
            }
        }
    }
}

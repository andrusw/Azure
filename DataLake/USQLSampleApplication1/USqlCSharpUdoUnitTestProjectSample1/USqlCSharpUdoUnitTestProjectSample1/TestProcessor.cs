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
    public class TestProcessor
    {
        [TestMethod]
        [DeploymentItem(@"Input\processor.txt")]
        public void TestMyProcessor()
        {
            //Schema: "a:int, b:int"
            USqlColumn<int> col1 = new USqlColumn<int>("a");
            USqlColumn<int> col2 = new USqlColumn<int>("b");
            List<IColumn> columns = new List<IColumn> { col1, col2 };
            USqlSchema schema = new USqlSchema(columns);

            //Generate one row with specified column values
            object[] values = new object[2] { 2, 3 };
            IRow input = new USqlRow(schema, values);
            IUpdatableRow output = input.AsUpdatable();

            //Create UDO instance
            MyProcessor processor = new MyProcessor(floor: 4);
            IRow newOutput = processor.Process(input, output);

            //Verify results
            Assert.IsTrue(newOutput.Schema.Count == 2);
            Assert.IsTrue(newOutput.Get<int>(0) == 2);
            Assert.IsTrue(newOutput.Get<int>(1) == 4);
        }

        [ExpectedException(typeof(Exception), "Passthrough wasn't enabled: 30")]
        [DeploymentItem(@"Input\processor.txt")]
        [TestMethod]
        public void TestMyProcessorWithFile()
        {
            //Schema: "a:int, b:int"
            USqlColumn<int> col1 = new USqlColumn<int>("a");
            USqlColumn<int> col2 = new USqlColumn<int>("b");
            List<IColumn> columns = new List<IColumn> { col1, col2 };
            USqlSchema schema = new USqlSchema(columns);

            //Generate one row with default values
            IUpdatableRow output = new USqlRow(schema, null).AsUpdatable();

            //Get upstreams from file
            IRowset rowset = UnitTestHelper.GetRowsetFromFile(@"processor.txt", schema, output.AsReadOnly(), discardAdditionalColumns: true, rowDelimiter: null, columnSeparator: '\t');

            //Create UDO instance
            MyProcessor processor = new MyProcessor(floor: 50, enforce: true);
            foreach (IRow r in rowset.Rows)
            {
                processor.Process(r, output);
            }
        }

        [TestMethod]
        public void TestMyProcessorWithCollection()
        {
            //Schema: "a:int, b:int"
            USqlSchema schema = new USqlSchema(
                   new USqlColumn<int>("a"),
                   new USqlColumn<int>("b")
                   );
            IUpdatableRow output = new USqlRow(schema, null).AsUpdatable();

            //Generate Rowset with specified values
            List<object[]> values = new List<object[]>{
                new object[2] { 2, 3 },
                new object[2] { 10, 20 }
            };
            IEnumerable<IRow> rows = UnitTestHelper.CreateRowsFromValues(schema, values);
            IRowset rowset = UnitTestHelper.GetRowsetFromCollection(rows, output.AsReadOnly());

            //Create UDO instance
            MyProcessor processor = new MyProcessor(floor: 1, enforce: true);
            foreach (IRow r in rowset.Rows)
            {
                IRow after = processor.Process(r, output);
                //Verify result
                Assert.IsTrue(after.Get<int>(0) == 2);
                Assert.IsTrue(after.Get<int>(1) == 4);
                break;
            }
        }

        [TestMethod]
        public void TestMyProcessorWithCollection_LessOutput()
        {
            //Schema: "a:int, b:int"
            USqlSchema schema = new USqlSchema(
                   new USqlColumn<int>("a"),
                   new USqlColumn<int>("b"),
                   new USqlColumn<string>("c")
                   );
            USqlSchema outputSchema = new USqlSchema(
                   new USqlColumn<int>("a"),
                   new USqlColumn<int>("b")
                   );
            IUpdatableRow output = new USqlRow(outputSchema, null).AsUpdatable();

            //Generate Rowset with specified values
            List<object[]> values = new List<object[]>{
                new object[3] { 2, 3, 4 },
                new object[3] { 10, 20, 50}
            };
            IEnumerable<IRow> rows = UnitTestHelper.CreateRowsFromValues(schema, values);
            IRowset rowset = UnitTestHelper.GetRowsetFromCollection(rows, output.AsReadOnly());

            //Create UDO instance
            MyProcessor processor = new MyProcessor(floor: 1, enforce: true);
            foreach (IRow r in rowset.Rows)
            {
                IRow after = processor.Process(r, output);
                //Verify result
                Assert.IsTrue(after.Get<int>(0) == 2);
                Assert.IsTrue(after.Get<int>(1) == 4);
                break;
            }
        }
    }
}

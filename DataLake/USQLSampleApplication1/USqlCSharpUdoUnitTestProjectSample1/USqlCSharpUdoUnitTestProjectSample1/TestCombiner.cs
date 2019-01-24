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
    /// Summary description for TestCombiner
    /// </summary>
    [TestClass]
    //This sample shows how to test a user defined combiner by constructing baseline values directly
    public class TestCombiner
    {
        public TestCombiner()
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
        public void TestMyCombiner()
        {
            //Construct left columns
            USqlColumn<int> col1 = new USqlColumn<int>("employee_id");
            USqlColumn<string> col2 = new USqlColumn<string>("employee_name");
            USqlColumn<string> col3 = new USqlColumn<string>("department_name");
            //Construct left schema
            List<IColumn> columns_left = new List<IColumn> { col1, col3 };
            USqlSchema schema_left = new USqlSchema(columns_left);
            //Construct right schema
            List<IColumn> columns_right = new List<IColumn> { col1, col2 };
            USqlSchema schema_right = new USqlSchema(columns_right);
            //Construct result schema. expected schema: employee_id, employee_name, department_name
            List<IColumn> columns_result = new List<IColumn> { col1, col2, col3 };
            USqlSchema schema_result = new USqlSchema(columns_result);

            //Construct the output schema which will be needed later
            IUpdatableRow output_left = new USqlRow(schema_left, null).AsUpdatable();
            IUpdatableRow output_right = new USqlRow(schema_right, null).AsUpdatable();
            IUpdatableRow output_result = new USqlRow(schema_result, null).AsUpdatable();

            //Generate Rowset with specified values for left input
            List<object[]> values_left = new List<object[]>{
                new object[2] { 1,"HR"},
                new object[2] { 2,"R&D"},
                new object[2] { 4,"Operation"},
            };
            //Generate Rowset with specified values for right input
            List<object[]> values_right = new List<object[]>{
                new object[2] { 1,"John"},
                new object[2] { 2,"Tom"},
                new object[2] { 3,"Melinda"},
            };

            IEnumerable<IRow> rows_left = UnitTestHelper.CreateRowsFromValues(schema_left, values_left);
            IEnumerable<IRow> rows_right = UnitTestHelper.CreateRowsFromValues(schema_right, values_right);
            IRowset rowset_left = UnitTestHelper.GetRowsetFromCollection(rows_left, output_left.AsReadOnly());
            IRowset rowset_right = UnitTestHelper.GetRowsetFromCollection(rows_right, output_right.AsReadOnly());

            //Create UDO instance. The combiner will do something like inner join 
            MyCombiner combiner = new MyCombiner();

            IEnumerable<IRow> after = combiner.Combine(rowset_left, rowset_right, output_result);
            //Verify result
            List<IRow> resultlist = after.ToList();
            Assert.IsTrue(resultlist[0].Schema.Count == 3);
            //Verify the values. expected to see:
            //the first row: 1, "John", "HR"
            //the second row: 2, "Tom", "R&D"
            //here we only verify the first row
            Assert.IsTrue(resultlist[0].Get<int>("employee_id") == 1);
            Assert.IsTrue(resultlist[0].Get<string>(1) == "John");
            Assert.IsTrue(resultlist[0].Get<string>(2) == "HR");
        }
    }
}

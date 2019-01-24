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
    public class TestOutputter
    {
        [TestMethod]
        [DeploymentItem(@"Baseline\output_baseline.txt")]
        public void TestMyOutputter()
        {
            using (FileStream stream = new FileStream(@"output.txt", FileMode.OpenOrCreate))
            {
                USqlStreamWriter writer = new USqlStreamWriter(stream);
                MyOutputter outputter = new MyOutputter();
                outputter.Output(RowGenerator(), writer);
            }
            //Make sure the file is there
            Assert.IsTrue(File.Exists(@"output.txt"));
            //Make sure the output content is as expected.
            Assert.IsTrue(FileEquals(@"output.txt", @"output_baseline.txt"));

        }

        public bool FileEquals(string path1, string path2)
        {
            byte[] file1 = File.ReadAllBytes(path1);
            byte[] file2 = File.ReadAllBytes(path2);
            if (file1.Length == file2.Length)
            {
                for (int i = 0; i < file1.Length; i++)
                {
                    if (file1[i] != file2[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public IRow RowGenerator()
        {
            USqlColumn<string> col1 = new USqlColumn<string>("col1");
            USqlColumn<int> col2 = new USqlColumn<int>("id");
            USqlColumn<int> col3 = new USqlColumn<int>("rank");
            List<IColumn> columns = new List<IColumn> { col1, col2, col3 };
            USqlSchema schema = new USqlSchema(columns);
            object[] values = new object[3] { "name", 2, 100 };
            return new USqlRow(schema, values);
        }
    }
}

using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Interfaces.Streaming;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

//This file contains the UDOs to be tested.
//For more information on how to write UDOs, please refer to the doc http://go.microsoft.com/fwlink/?LinkID=623598&clcid=0x409

namespace USqlCSharpUdoSample
{
    //This Processor does a very simple job: if a+b is greater than a certain value, then increase b;
    [SqlUserDefinedProcessor]
    public class MyProcessor : IProcessor
    {
        private int floor;
        private bool enforce;
        private string[] inputReadOnly;
        private string[] inputSchema;
        private string[] outputSchema;

        public MyProcessor(int floor, bool enforce = false, string[] inputReadOnly = null, string[] inputSchema = null, string[] outputSchema = null)
        {
            this.floor = floor;
            this.enforce = enforce;
            this.inputReadOnly = inputReadOnly;
            this.inputSchema = inputSchema;
            this.outputSchema = outputSchema;
        }

        public override IRow Process(IRow input, IUpdatableRow output)
        {
            var a = input.Get<int>("a");
            var b = input.Get<int>("b");

            if (a + b > floor)
            {
                output.Set("a", a);
                output.Set("b", b + 1);
            }
            else if (enforce)
            {
                throw new Exception("Passthrough wasn't enabled: " + (a + b));
            }

            return output.AsReadOnly();
        }
    }


    //This extractor does a very simple job: read a certain TSV file which has 3 columns
    [SqlUserDefinedExtractor]
    public class MyExtractor : IExtractor
    {
        public override IEnumerable<IRow> Extract(IUnstructuredReader input, IUpdatableRow output)
        {

            char column_delimiter = '\t';
            string line;
            var reader = new StreamReader(input.BaseStream);
            while ((line = reader.ReadLine()) != null)
            {
                var tokens = line.Split(column_delimiter);
                output.Set("Market", tokens[0]);
                output.Set("Query", tokens[1]);
                output.Set("Latency", Convert.ToInt64(tokens[2]));
                yield return output.AsReadOnly();
            }
        }
    }


    //This outputter does a very simple job: whatever data is read, just output the column named "col1"
    [SqlUserDefinedOutputter]
    public class MyOutputter : IOutputter
    {
        public override void Output(IRow row, IUnstructuredWriter output)
        {
            var buffer = Encoding.UTF8.GetBytes(row.Get<string>("col1"));
            output.BaseStream.Write(buffer, 0, buffer.Length);
        }
    }

    //The following example Reducer shows a custom Reducer that takes a grouped rowset as an input 
    //and process each row in turn and returns top one record per group (grouping column: market). 
    //Returns top record per group based on latency field in the input rowset. This reducer has same input and output schema.
    [SqlUserDefinedReducer()]
    public class MyReducer : IReducer
    {
        public override IEnumerable<IRow> Reduce(IRowset input, IUpdatableRow output)
        {
            int? maxlatency = null;
            string tquery = "", tmarket = "";
            foreach (var row in input.Rows)
            {
                int? l = row.Get<int?>("Latency");
                if (maxlatency == null || l > maxlatency)
                {
                    maxlatency = l;
                    tquery = row.Get<string>("Query");
                    tmarket = row.Get<string>("Market");
                }
            }

            output.Set("Query", tquery);
            output.Set("Market", tmarket);
            output.Set("Latency", maxlatency);
            yield return output.AsReadOnly();
        }
    }


    //This combiner performs an operation similar to inner join on certain keys
    [SqlUserDefinedCombiner]
    public class MyCombiner : ICombiner
    {
        /// <summary/>
        public override IEnumerable<IRow> Combine(IRowset left, IRowset right, IUpdatableRow output)
        {
            var buffer = new List<Tuple<int, string>>();

            foreach (var row2 in right.Rows)
            {
                buffer.Add(Tuple.Create<int, string>(
                    row2.Get<int>("employee_id"),
                    row2.Get<string>("employee_name")
                    ));
            }

            foreach (var row in left.Rows)
            {
                foreach (var tuple in buffer)
                {

                    if (row.Get<int>("employee_id") == tuple.Item1)
                    {
                        output.Set("employee_id", tuple.Item1);
                        output.Set("employee_name", tuple.Item2);
                        output.Set("department_name", row.Get<string>("department_name"));
                        yield return output.AsReadOnly();
                    }
                }
            }
        }
    }
}

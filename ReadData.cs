using ModelStochastic1.Models;
using ModelStochastic1.Models.Structs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Text;

namespace ModelStochastic1
{
    class ReadData
    {
        public InputData readData(string inputFolder) {          

            string sheetName = "Sheet1";
            string strFile = "";
            DataTable dtXLS = new DataTable();
            string dataTableToJsonString = "";

            //READING T.xlsx
            strFile = "T.xlsx";
            Console.WriteLine("READING DATA: " + strFile);
            dtXLS = ReadExcelFileToDataTable(inputFolder, strFile, sheetName);
            List<T_param> TList = new List<T_param>();
            dataTableToJsonString = DataTableToJsonString(dtXLS);
            Console.WriteLine(JsonConvert.SerializeObject(dataTableToJsonString));
            TList = JsonConvert.DeserializeObject<List<T_param>>(dataTableToJsonString);

                //READING PROD.xlsx
            strFile = "PROD.xlsx";
            Console.WriteLine("READING DATA: " + strFile);
            dtXLS = ReadExcelFileToDataTable(inputFolder, strFile, sheetName);
            List<Prod_set> ProdList = new List<Prod_set>();
            dataTableToJsonString = DataTableToJsonString(dtXLS);
            Console.WriteLine(JsonConvert.SerializeObject(dataTableToJsonString));
            ProdList = JsonConvert.DeserializeObject<List<Prod_set>>(dataTableToJsonString);

            //READING SCEN.xlsx
            strFile = "SCEN.xlsx";
            Console.WriteLine("READING DATA: " + strFile);
            dtXLS = ReadExcelFileToDataTable(inputFolder, strFile, sheetName);
            List<Scen_set> ScenList = new List<Scen_set>();
            dataTableToJsonString = DataTableToJsonString(dtXLS);
            Console.WriteLine(JsonConvert.SerializeObject(dataTableToJsonString));
            ScenList = JsonConvert.DeserializeObject<List<Scen_set>>(dataTableToJsonString);

            //READING AVAIL.xlsx
            strFile = "AVAIL.xlsx";
            Console.WriteLine("READING DATA: " + strFile);
            dtXLS = ReadExcelFileToDataTable(inputFolder, strFile, sheetName);
            List<Avail_param> AvailList = new List<Avail_param>();
            dataTableToJsonString = DataTableToJsonString(dtXLS);
            Console.WriteLine(JsonConvert.SerializeObject(dataTableToJsonString));
            AvailList = JsonConvert.DeserializeObject<List<Avail_param>>(dataTableToJsonString);

            //READING RATE.xlsx
            strFile = "RATE.xlsx";
            Console.WriteLine("READING DATA: " + strFile);
            dtXLS = ReadExcelFileToDataTable(inputFolder, strFile, sheetName);
            List<Rate_param> RateList = new List<Rate_param>();
            dataTableToJsonString = DataTableToJsonString(dtXLS);
            Console.WriteLine(JsonConvert.SerializeObject(dataTableToJsonString));
            RateList = JsonConvert.DeserializeObject<List<Rate_param>>(dataTableToJsonString);

            //READING INV0.xlsx
            strFile = "INV0.xlsx";
            Console.WriteLine("READING DATA: " + strFile);
            dtXLS = ReadExcelFileToDataTable(inputFolder, strFile, sheetName);
            List<Inv0_param> Inv0List = new List<Inv0_param>();
            dataTableToJsonString = DataTableToJsonString(dtXLS);
            Console.WriteLine(JsonConvert.SerializeObject(dataTableToJsonString));
            Inv0List = JsonConvert.DeserializeObject<List<Inv0_param>>(dataTableToJsonString);

            //READING PRODCOST.xlsx
            strFile = "PRODCOST.xlsx";
            Console.WriteLine("READING DATA: " + strFile);
            dtXLS = ReadExcelFileToDataTable(inputFolder, strFile, sheetName);
            List<ProdCost_param> ProdCostList = new List<ProdCost_param>();
            dataTableToJsonString = DataTableToJsonString(dtXLS);
            Console.WriteLine(JsonConvert.SerializeObject(dataTableToJsonString));
            ProdCostList = JsonConvert.DeserializeObject<List<ProdCost_param>>(dataTableToJsonString);

            //READING INVCOST.xlsx
            strFile = "INVCOST.xlsx";
            Console.WriteLine("READING DATA: " + strFile);
            dtXLS = ReadExcelFileToDataTable(inputFolder, strFile, sheetName);
            List<InvCost_param> InvCostList = new List<InvCost_param>();
            dataTableToJsonString = DataTableToJsonString(dtXLS);
            Console.WriteLine(JsonConvert.SerializeObject(dataTableToJsonString));
            InvCostList = JsonConvert.DeserializeObject<List<InvCost_param>>(dataTableToJsonString);

            //READING REVENUE.xlsx
            strFile = "REVENUE.xlsx";
            Console.WriteLine("READING DATA: " + strFile);
            dtXLS = ReadExcelFileToDataTable(inputFolder, strFile, sheetName);
            List<Revenue_param> RevenueList = new List<Revenue_param>();
            dataTableToJsonString = DataTableToJsonString(dtXLS);
            Console.WriteLine(JsonConvert.SerializeObject(dataTableToJsonString));
            RevenueList = JsonConvert.DeserializeObject<List<Revenue_param>>(dataTableToJsonString);

            //READING MARKET.xlsx
            strFile = "MARKET.xlsx";
            Console.WriteLine("READING DATA: " + strFile);
            dtXLS = ReadExcelFileToDataTable(inputFolder, strFile, sheetName);
            List<Market_param> MarketList = new List<Market_param>();
            dataTableToJsonString = DataTableToJsonString(dtXLS);
            Console.WriteLine(JsonConvert.SerializeObject(dataTableToJsonString));
            MarketList = JsonConvert.DeserializeObject<List<Market_param>>(dataTableToJsonString);

            //READING PROB.xlsx
            strFile = "PROB.xlsx";
            Console.WriteLine("READING DATA: " + strFile);
            dtXLS = ReadExcelFileToDataTable(inputFolder, strFile, sheetName);
            List<Prob_param> ProbList = new List<Prob_param>();
            dataTableToJsonString = DataTableToJsonString(dtXLS);
            Console.WriteLine(JsonConvert.SerializeObject(dataTableToJsonString));
            ProbList = JsonConvert.DeserializeObject<List<Prob_param>>(dataTableToJsonString);

            return new InputData()
            {
                TList = TList,
                AvailList = AvailList,
                Inv0List = Inv0List,
                InvCostList = InvCostList,
                MarketList = MarketList,
                ProbList = ProbList,
                ProdCostList = ProdCostList,
                ProdList = ProdList,
                RateList = RateList,
                RevenueList = RevenueList,
                ScenList = ScenList
            };
        }

        static public DataTable ReadExcelFileToDataTable(string inputFolder, string strFile, string sheetName)
        {
            DataTable dtXLS = new DataTable(sheetName);
            try
            {
                string strConnectionString = "";
                if (strFile.Trim().EndsWith(".xlsx"))
                {
                    strConnectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", inputFolder + strFile);
                }
                else if (strFile.Trim().EndsWith(".xls"))
                {
                    strConnectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=1\";", inputFolder + strFile);
                }

                OleDbConnection SQLConn = new OleDbConnection(strConnectionString);
                SQLConn.Open();
                OleDbDataAdapter SQLAdapter = new OleDbDataAdapter();
                string sql = "SELECT * FROM [" + sheetName + "$]";
                OleDbCommand selectCMD = new OleDbCommand(sql, SQLConn);
                SQLAdapter.SelectCommand = selectCMD;
                SQLAdapter.Fill(dtXLS);
                SQLConn.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return dtXLS;
        }

        static public string DataTableToJsonString(DataTable dataTable)
        {
            string dataTableToJsonString = "";
            try
            {
                List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
                Dictionary<string, object> row;
                foreach (DataRow dr in dataTable.Rows)
                {
                    row = new Dictionary<string, object>();
                    foreach (DataColumn col in dataTable.Columns)
                    {
                        row.Add(col.ColumnName, dr[col]);
                    }
                    rows.Add(row);
                }
                dataTableToJsonString = JsonConvert.SerializeObject(rows);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return dataTableToJsonString;

        }

    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MK7_Binary_Data_Editor
{
    public class DataTableCreator
    {
        public DataTable DataTable { get; set; } = new DataTable();
        public int GetColumnCount => DataTable.Columns.Count;
        public int GetRowCount => DataTable.Rows.Count;

        public void SortTo(string ColumnName, string SearchDataArray)
        {
            DataTable.DefaultView.RowFilter = $"['{ColumnName}']='{SearchDataArray}'";
        }

        public void SortTo(string ColumnName, string[] SearchDataArray)
        {
            DataTable.DefaultView.RowFilter = $"['{ColumnName}']='{SearchDataArray}'";
        }

        public void DisableSort()
        {
            DataTable.DefaultView.RowFilter = "";
        }

        public void AddRow(object[] DataArray)
        {
            DataTable.Rows.Add(DataArray);
        }

        public void ClearDataTable()
        {
            DataTable.Rows.Clear();
            DataTable.Columns.Clear();
        }

        public void ClearDataTableRows()
        {
            DataTable.Rows.Clear();
        }

        public void ClearDataTableColumns()
        {
            DataTable.Columns.Clear();
        }

        /// <summary>
        /// 指定した行の値を全て取得します
        /// </summary>
        /// <param name="index">行番号</param>
        /// <returns></returns>
        public object[] GetRows(int index)
        {
            return DataTable.Rows[index].ItemArray;
        }

        /// <summary>
        /// 指定した行の値を全て取得します
        /// </summary>
        /// <param name="index">行番号</param>
        /// <returns></returns>
        public DataRow GetDataRow(int index)
        {
            return DataTable.Rows[index];
        }

        public string GetColumn(int index)
        {
            return DataTable.Columns[index].ColumnName;
        }

        public string[] GetColumnArray()
        {
            string[] ColumnArray = new string[GetColumnCount];
            for (int i = 0; i < GetColumnCount; i++)
            {
                ColumnArray[i] = DataTable.Columns[i].ColumnName;
            }

            return ColumnArray;
        }

        public object GetValue(int RowIndex, int CellIndex)
        {
            return DataTable.Rows[RowIndex][CellIndex];
        } 

        /// <summary>
        /// Initialize DataTableCreator (from : string[])
        /// </summary>
        /// <param name="ColumnStringArray"></param>
        public DataTableCreator(string[] ColumnStringArray)
        {
            DataColumn[] DataColumnArray = ColumnStringArray.Select(x => new DataColumn(x)).ToArray();
            DataTable.Columns.AddRange(DataColumnArray);
        }

        /// <summary>
        /// Initialize DataTableCreator (from : DataColumn[])
        /// </summary>
        /// <param name="DataColumnArray">Add DataColumn[]</param>
        public DataTableCreator(DataColumn[] DataColumnArray)
        {
            DataTable.Columns.AddRange(DataColumnArray);
        }

        /// <summary>
        /// 既にあるDataTableをDataTableCreatorに使用する
        /// </summary>
        /// <param name="dataTable"></param>
        public DataTableCreator(DataTable dataTable)
        {
            DataTable = dataTable;
        }
    }

    public class DataGridView_DataTableConvertor
    {
        public static DataTable GetDataTable(DataGridView dataGridView, bool Copy = false)
        {
            DataTable dataTable;
            dataTable = (DataTable)dataGridView.DataSource;
            if (Copy == true) dataTable = dataTable.Copy();

            return dataTable;
        }
    }

    public class DataTableSheetCreator
    {
        public Dictionary<int, List<DataTableCreator>> DataTableSheetDictionary { get; set; }
        public int Count => DataTableSheetDictionary.Count;

        public DataTableCreator GroupDataTable => ConvertToGroupDataTable();

        public DataTableCreator ConvertToGroupDataTable()
        {
            string[] IndexColumnArray = new string[] { "Sheet Index", "Table Index" };
            string[] BaseColumnArray = DataTableSheetDictionary[0][0].GetColumnArray();
            string[] ColumnArray = IndexColumnArray.Concat(BaseColumnArray).ToArray();

            //string[] UnknownSectionValueStrArray = new string[] { "Value 1", "Value 2", "Value 3", "Value 4", "Value 5" };
            DataTableCreator DataTableCreator = new DataTableCreator(ColumnArray);


            for (int SheetCount = 0; SheetCount < DataTableSheetDictionary.Count; SheetCount++)
            {
                //Add DGV
                for (int TableCount = 0; TableCount < DataTableSheetDictionary[SheetCount].Count; TableCount++)
                {
                    for (int RowCount = 0; RowCount < DataTableSheetDictionary[SheetCount][TableCount].GetRowCount; RowCount++)
                    {
                        object[] IndexAry = new object[] { SheetCount, TableCount };

                        object[] RowObj = new object[DataTableSheetDictionary[SheetCount][TableCount].GetColumnCount];
                        for (int ColumnCount = 0; ColumnCount < DataTableSheetDictionary[SheetCount][TableCount].GetColumnCount; ColumnCount++)
                        {
                            RowObj[ColumnCount] = DataTableSheetDictionary[SheetCount][TableCount].GetValue(RowCount, ColumnCount);
                        }

                        object[] IndexedRowArray = IndexAry.Concat(RowObj).ToArray();

                        DataTableCreator.AddRow(IndexedRowArray);
                    }
                }
            }

            return DataTableCreator;
        }

        #region Sheet
        public void AddSheet(List<DataTableCreator> dataTableCreatorList)
        {
            DataTableSheetDictionary.Add(DataTableSheetDictionary.Count, dataTableCreatorList);
        }

        //public enum InsertFor
        //{
        //    Next = 1,
        //    Preview = -1,
        //}

        //public void InsertSheet(DataTableCreator dataTableCreator, InsertFor insertFor, int Index)
        //{
        //    DataTableSheetDictionary.Add(DataTableSheetDictionary.Count, dataTableCreator);
        //}

        public void DeleteSheet(int Index)
        {
            DataTableSheetDictionary.Remove(Index);
        }

        public void Clear()
        {
            DataTableSheetDictionary.Clear();
        }
        #endregion

        #region Table
        public void AddTable(int SheetIndex, DataTableCreator dataTableCreator)
        {
            DataTableSheetDictionary[SheetIndex].Add(dataTableCreator);
        }

        public void AddTable(int SheetIndex, DataTable dataTable)
        {
            DataTableCreator dataTableCreator = new DataTableCreator(dataTable);
            DataTableSheetDictionary[SheetIndex].Add(dataTableCreator);
        }

        public void DeleteTable(int SheetIndex, int TableIndex)
        {
            DataTableSheetDictionary[SheetIndex].RemoveAt(TableIndex);
        }

        public void InsertTable(int SheetIndex, int TableIndex, DataTableCreator dataTableCreator)
        {
            DataTableSheetDictionary[SheetIndex].Insert(TableIndex, dataTableCreator);
        }

        /// <summary>
        /// 指定したIndexのテーブルから要素を全て削除します
        /// </summary>
        /// <param name="SheetIndex"></param>
        public void ClearTable(int SheetIndex)
        {
            DataTableSheetDictionary[SheetIndex].Clear();
        }

        public void ClearAllTable()
        {
            foreach (var item in DataTableSheetDictionary)
            {
                item.Value.Clear();
            }
        }
        #endregion

        /// <summary>
        /// Initialize DataTableSheetCreator (from : string[])
        /// </summary>
        /// <param name="ColumnStringArray">Add Column String Array</param>
        /// <param name="SheetNum">作成するシートの数</param>
        /// <param name="TableNum">作成するテーブルの数</param>
        public DataTableSheetCreator(string[] ColumnStringArray, int SheetNum, int TableNum)
        {
            for (int i = 0; i < SheetNum; i++)
            {
                List<DataTableCreator> dataTableCreators = new List<DataTableCreator>();
                for (int j = 0; j < TableNum; j++)
                {
                    DataTableCreator dataTableCreator = new DataTableCreator(ColumnStringArray);
                    dataTableCreators.Add(dataTableCreator);
                }

                
                DataTableSheetDictionary.Add(i, dataTableCreators);
            }
        }

        /// <summary>
        /// Initialize DataTableSheetCreator (from : DataColumn[])
        /// </summary>
        /// <param name="DataColumnArray">Add DataColumn[]</param>
        /// <param name="SheetNum">作成するシートの数</param>
        /// /// <param name="TableNum">作成するテーブルの数</param>
        public DataTableSheetCreator(DataColumn[] DataColumnArray, int SheetNum, int TableNum)
        {
            for (int i = 0; i < SheetNum; i++)
            {
                List<DataTableCreator> dataTableCreators = new List<DataTableCreator>();
                for (int j = 0; j < TableNum; j++)
                {
                    DataTableCreator dataTableCreator = new DataTableCreator(DataColumnArray);
                    dataTableCreators.Add(dataTableCreator);
                }


                DataTableSheetDictionary.Add(i, dataTableCreators);
            }
        }

        /// <summary>
        /// 既にあるDataTableSheetDictionaryをDataTableSheetCreatorに使用する
        /// </summary>
        /// <param name="dataTable"></param>
        public DataTableSheetCreator(Dictionary<int, List<DataTableCreator>> dataTableSheetDict)
        {
            DataTableSheetDictionary = dataTableSheetDict;
        }

        public DataTableSheetCreator()
        {
            DataTableSheetDictionary = new Dictionary<int, List<DataTableCreator>>();

            ////Dictionary<int, List<DataTableCreator>> PageTableDictionary = new Dictionary<int, List<DataTableCreator>>();
            //for (int PageCount = 0; PageCount < effectParams.Data2_List.Count; PageCount++)
            //{
            //    //string[] UnknownSectionValueStrArray = new string[] { "Value 1", "Value 2", "Value 3", "Value 4", "Value 5" };
            //    //DataTableCreator UnknownSectionValueMainDataTableCreator = new DataTableCreator(UnknownSectionValueStrArray);

            //    List<DataTableCreator> DataTableCreatorList = new List<DataTableCreator>();

            //    //Add DGV
            //    for (int ParamValueCount = 0; ParamValueCount < effectParams.Data2_List[PageCount].ParamValue_List.Count; ParamValueCount++)
            //    {
            //        string[] UnknownSectionValueStrArray = new string[] { "Value 1", "Value 2", "Value 3", "Value 4", "Value 5" };
            //        DataTableCreator UnknownSectionValueMainDataTableCreator = new DataTableCreator(UnknownSectionValueStrArray);

            //        for (int n = 0; n < effectParams.Data2_List[PageCount].ParamValue_List[ParamValueCount].Count; n++)
            //        {
            //            object[] RowObj = new object[]
            //            {
            //                        effectParams.Data2_List[PageCount].ParamValue_List[ParamValueCount][n][0],
            //                        effectParams.Data2_List[PageCount].ParamValue_List[ParamValueCount][n][1],
            //                        effectParams.Data2_List[PageCount].ParamValue_List[ParamValueCount][n][2],
            //                        effectParams.Data2_List[PageCount].ParamValue_List[ParamValueCount][n][3],
            //                        effectParams.Data2_List[PageCount].ParamValue_List[ParamValueCount][n][4]
            //            };

            //            UnknownSectionValueMainDataTableCreator.AddRow(RowObj);
            //        }

            //        DataTableCreatorList.Add(UnknownSectionValueMainDataTableCreator);
            //    }

            //    DataTableSheetDictionary.Add(PageCount, DataTableCreatorList);
            //}
        }
    }

    //public class PageTableConverter
    //{
    //    public static DataTable FromPageTable(List<DataTable> SourcePageTable)
    //    {
    //        var d = SourcePageTable.Select(x => x.Columns).Distinct().ToArray();
    //        if (d.Length != 1) throw new Exception("Columnの要素数が一致しません");

    //        DataTable dataTable = new DataTable();
    //        dataTable.Columns.AddRange(d[0].Cast<DataColumn>().ToArray());

    //        foreach (var page in SourcePageTable)
    //        {
    //            foreach (var tableRow in page.Rows)
    //            {
    //                dataTable.Rows.Add(tableRow);
    //            }
    //        }

    //        return dataTable;
    //    }

    //    public static List<DataTable> FromDataTable(DataTable Source, int Split)
    //    {
    //        DataColumn[] dataColumns = new DataColumn[Source.Columns.Count];
    //        for (int ColumnCount = 0; ColumnCount < Source.Columns.Count; ColumnCount++)
    //        {
    //            dataColumns[ColumnCount] = new DataColumn(Source.Columns[ColumnCount].ColumnName);
    //        }

    //        List<DataTable> table = new List<DataTable>();

    //        if (Split <= Source.Rows.Count)
    //        {
    //            if ((Source.Rows.Count % Split) == 0)
    //            {
    //                int PageCount = Source.Rows.Count / Split;

    //                int RowCount = 0;
    //                for (int i = 0; i < PageCount; i++)
    //                {
    //                    DataTable d0 = new DataTable();
    //                    d0.Columns.AddRange(dataColumns);

    //                    for (int SplitCount = 0; SplitCount < Split; SplitCount++)
    //                    {
    //                        d0.Rows.Add(Source.Rows[RowCount]);
    //                        RowCount++;
    //                    }

    //                    table.Add(d0);
    //                }
    //            }
    //            else
    //            {

    //            }
    //        }
    //        else if (Split > Source.Rows.Count)
    //        {

    //        }

    //        return table;
    //    }
    //}

    //public class Test
    //{
    //    public void TestData()
    //    {
    //        string[] strings = new string[] { "A", "B", "C" };
    //        DataTableCreator dataTableCreator = new DataTableCreator(strings);
    //        dataTableCreator.AddRow(new object[] { 0, 1, 2 });
    //        dataTableCreator.AddRow(new object[] { 3, 4, 5 });
    //        dataTableCreator.AddRow(new object[] { 6, 7, 8 });
    //    }

    //    public void TestData2()
    //    {
    //        string[] strings = new string[] { "A", "B", "C" };

    //        DataTableCreator dataTableCreator = new DataTableCreator(strings);
    //        dataTableCreator.AddRow(new object[] { 0, 1, 2 });
    //        dataTableCreator.AddRow(new object[] { 3, 4, 5 });
    //        dataTableCreator.AddRow(new object[] { 6, 7, 8 });
    //    }
    //}
}

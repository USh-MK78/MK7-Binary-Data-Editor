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

    public class Test
    {
        public void TestData()
        {
            string[] strings = new string[] { "A", "B", "C" };
            DataTableCreator dataTableCreator = new DataTableCreator(strings);
            dataTableCreator.AddRow(new object[] { 0, 1, 2 });
            dataTableCreator.AddRow(new object[] { 3, 4, 5 });
            dataTableCreator.AddRow(new object[] { 6, 7, 8 });
        }
    }
}

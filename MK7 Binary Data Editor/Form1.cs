﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Security.Claims;

namespace MK7_Binary_Data_Editor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            ParamMain_DGV.AllowUserToAddRows = false;
            MinMaxValue_DGV.AllowUserToAddRows = false;
            GeoHitItemTableDGV.AllowUserToAddRows = false;
            UnknownGeoHitTableSection_DGV.AllowUserToAddRows = false;
            EffectParamsColorSectionDGV.AllowUserToAddRows = false;
            EffectParams_UnknownSection_DGV.AllowUserToAddRows = false;

            tabControl1.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top);
            tabControl2.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top);
            tabControl3.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top);

            label1.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);
            label2.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);

            EffectParams_UnknownSection_DGV.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top);
            EffectParam_NameSelectComboBox.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);
            comboBox2.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);

            label3.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);
            label4.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            FilePah_LBL.Text = FilePah_LBL.Tag.ToString();
        }

        //public EffectParams effectParams { get; set; }


        public DataTableCreator EffectParamMain_DataTableCreator { get; set; }
        //public Dictionary<int, List<DataTableCreator>> UnknownParam_DataTableCreatorDictionary { get; set; }
        public DataTableSheetCreator UnknownParamDataTableSheetCreator { get; set; }

        private void Open_BinData(object sender, EventArgs e)
        {
            ParamBinSelectorForm openParamSelectorForm = new ParamBinSelectorForm();
            openParamSelectorForm.ShowDialog();
            //Select
            ParamBinSelectorForm.ParamBinType openParamBinType = openParamSelectorForm.ParamTypes[openParamSelectorForm.Index];
            if (openParamBinType == ParamBinSelectorForm.ParamBinType.SlotTable)
            {
                OpenFileDialog Open_SlotTable = new OpenFileDialog()
                {
                    Title = "Open MK7 BinaryData (SlotTable)",
                    InitialDirectory = @"C:\Users\User\Desktop",
                    Filter = "bin file|*.bin"
                };

                if (Open_SlotTable.ShowDialog() == DialogResult.OK)
                {
                    FileStream fs = new FileStream(Open_SlotTable.FileName, FileMode.Open, FileAccess.Read);
                    BinaryReader br = new BinaryReader(fs);

                    SlotTable SlotTable_Format = new SlotTable();
                    SlotTable_Format.Read(br);

                    label2.Text = SlotTable_Format.DataType.ToString();

                    #region MinMaxValue
                    string[] CLAry = SlotTable_Format.CellLabel_List.ToArray();

                    DataTableCreator MinMaxValuedataTableCreator = new DataTableCreator(CLAry);

                    ArrayList Data_MinValue_ArrayList = new ArrayList();
                    Data_MinValue_ArrayList.Add("MinValue");
                    Data_MinValue_ArrayList.InsertRange(1, SlotTable_Format.MinMaxValueList.Select(x => x.Data_MinValue).ToArray());

                    ArrayList Data_MaxValue_ArrayList = new ArrayList();
                    Data_MaxValue_ArrayList.Add("MaxValue");
                    Data_MaxValue_ArrayList.InsertRange(1, SlotTable_Format.MinMaxValueList.Select(x => x.Data_MaxValue).ToArray());

                    MinMaxValuedataTableCreator.AddRow(Data_MinValue_ArrayList.ToArray());
                    MinMaxValuedataTableCreator.AddRow(Data_MaxValue_ArrayList.ToArray());
                    MinMaxValue_DGV.DataSource = MinMaxValuedataTableCreator.DataTable;
                    #endregion

                    #region ParamMain
                    DataTableCreator ParamMainDataTableCreator = new DataTableCreator(CLAry);

                    //Add Row
                    for (int RowCount = 0; RowCount < SlotTable_Format.RowCount; RowCount++)
                    {
                        ParamMainDataTableCreator.AddRow(SlotTable_Format.DataValueArrayList[RowCount]);
                    }

                    ParamMain_DGV.DataSource = ParamMainDataTableCreator.DataTable;
                    #endregion

                    br.Close();
                    fs.Close();

                    FilePah_LBL.Text = Open_SlotTable.FileName;
                }
                //else return;
            }
            else if (openParamBinType == ParamBinSelectorForm.ParamBinType.GeoHitTable)
            {
                OpenFileDialog OpenGeoHitTable = new OpenFileDialog()
                {
                    Title = "Open MK7 BinaryData (GeoHitTable)",
                    InitialDirectory = @"C:\Users\User\Desktop",
                    Filter = "bin file|*.bin"
                };

                if (OpenGeoHitTable.ShowDialog() == DialogResult.OK)
                {
                    FileStream fs = new FileStream(OpenGeoHitTable.FileName, FileMode.Open, FileAccess.Read);
                    BinaryReader br = new BinaryReader(fs);

                    GeoHitTable GeoHitTable = new GeoHitTable();
                    GeoHitTable.Read(br);

                    #region DataValues

                    #region DataValueColumnAry
                    List<string> CLAry = new List<string>();
                    CLAry.Add("ID");

                    string[] ValueStrAry = new string[GeoHitTable.CellCount];
                    for (int i = 0; i < GeoHitTable.CellCount; i++)
                    {
                        ValueStrAry[i] = "Value " + i;
                    }
                    CLAry.AddRange(ValueStrAry);
                    #endregion

                    DataTableCreator GeoHitTableDataTableCreator = new DataTableCreator(CLAry.ToArray());

                    for (int GHIT_DataValueCount = 0; GHIT_DataValueCount < GeoHitTable.RowCount; GHIT_DataValueCount++)
                    {
                        List<object> objList = new List<object>();
                        objList.Add(GeoHitTable.DataValues_List[GHIT_DataValueCount].ID);

                        object[] objects = new object[GeoHitTable.CellCount];
                        for (int ValueArrayCount = 0; ValueArrayCount < GeoHitTable.CellCount; ValueArrayCount++)
                        {
                            objects[ValueArrayCount] = GeoHitTable.DataValues_List[GHIT_DataValueCount].Value_Array[ValueArrayCount];
                        }

                        objList.AddRange(objects);

                        GeoHitTableDataTableCreator.AddRow(objList.ToArray());
                    }

                    GeoHitItemTableDGV.DataSource = GeoHitTableDataTableCreator.DataTable;
                    #endregion

                    textBox1.Text = ((int)GeoHitTable.UnknownColor.c0).ToString();
                    textBox2.Text = ((int)GeoHitTable.UnknownColor.c1).ToString();
                    textBox3.Text = ((int)GeoHitTable.UnknownColor.c2).ToString();
                    textBox4.Text = ((int)GeoHitTable.UnknownColor.c3).ToString();

                    #region UnknownSectionGeoHitTable
                    string[] UnknownSectionGeoHitTable_CLAry = new string[] { "Data 1", "Data 2" };
                    DataTableCreator UnknownSectionDataTableCreator = new DataTableCreator(UnknownSectionGeoHitTable_CLAry);
                    for (int RowCount = 0; RowCount < GeoHitTable.RowCount; RowCount++)
                    {
                        object[] objects = new object[2];
                        objects[0] = GeoHitTable.UnknownShortDataList[RowCount].Data1;
                        objects[1] = GeoHitTable.UnknownShortDataList[RowCount].Data2;

                        UnknownSectionDataTableCreator.AddRow(objects);
                    }

                    UnknownGeoHitTableSection_DGV.DataSource = UnknownSectionDataTableCreator.DataTable;
                    #endregion

                    br.Close();
                    fs.Close();

                    FilePah_LBL.Text = OpenGeoHitTable.FileName;
                }
                //else return;
            }
            else if (openParamBinType == ParamBinSelectorForm.ParamBinType.EffectParam)
            {
                OpenFileDialog OpenEffectParams = new OpenFileDialog()
                {
                    Title = "Open MK7 BinaryData (EffectParams)",
                    InitialDirectory = @"C:\Users\User\Desktop",
                    Filter = "bin file|*.bin"
                };

                if (OpenEffectParams.ShowDialog() == DialogResult.OK)
                {
                    FileStream fs = new FileStream(OpenEffectParams.FileName, FileMode.Open, FileAccess.Read);
                    BinaryReader br = new BinaryReader(fs);

                    EffectParams effectParams = new EffectParams();
                    effectParams.ReadEffectParams(br);

                    #region EffectParamMian
                    string[] strings = new string[] { "Name", "Color R", "Color G", "Color B", "Color A", "Flag 1", "Flag 2", "Flag 3", "Flag 4" };
                    DataTableCreator EffectParamMainDataTableCreator = new DataTableCreator(strings);

                    for (int Count = 0; Count < effectParams.Data1_RowCount; Count++)
                    {
                        object[] RowObj = new object[]
                        {
                            new string(effectParams.Data1_List[Count].Label_String),
                            effectParams.Data1_List[Count].Color_RGBA_Data.R,
                            effectParams.Data1_List[Count].Color_RGBA_Data.G,
                            effectParams.Data1_List[Count].Color_RGBA_Data.B,
                            effectParams.Data1_List[Count].Color_RGBA_Data.A,
                            effectParams.Data1_List[Count].Flag.f1,
                            effectParams.Data1_List[Count].Flag.f2,
                            effectParams.Data1_List[Count].Flag.f3,
                            effectParams.Data1_List[Count].Flag.f4
                        };

                        EffectParamMainDataTableCreator.AddRow(RowObj);
                    }

                    EffectParamsColorSectionDGV.DataSource = EffectParamMainDataTableCreator.DataTable;

                    EffectParamMain_DataTableCreator = EffectParamMainDataTableCreator;
                    #endregion

                    #region UnknownSectionValue

                    Dictionary<int, List<DataTableCreator>> PageTableDictionary = new Dictionary<int, List<DataTableCreator>>();
                    for (int SheetCount = 0; SheetCount < effectParams.Data2_List.Count; SheetCount++)
                    {
                        List<DataTableCreator> DataTableCreatorList = new List<DataTableCreator>();

                        //Add DGV
                        for (int TableCount = 0; TableCount < effectParams.Data2_List[SheetCount].ParamValue_List.Count; TableCount++)
                        {
                            string[] UnknownSectionValueStrArray = new string[] { "Value 1", "Value 2", "Value 3", "Value 4", "Value 5" };
                            DataTableCreator UnknownSectionValueMainDataTableCreator = new DataTableCreator(UnknownSectionValueStrArray);

                            for (int n = 0; n < effectParams.Data2_List[SheetCount].ParamValue_List[TableCount].Count; n++)
                            {
                                object[] RowObj = new object[]
                                {
                                    effectParams.Data2_List[SheetCount].ParamValue_List[TableCount][n][0],
                                    effectParams.Data2_List[SheetCount].ParamValue_List[TableCount][n][1],
                                    effectParams.Data2_List[SheetCount].ParamValue_List[TableCount][n][2],
                                    effectParams.Data2_List[SheetCount].ParamValue_List[TableCount][n][3],
                                    effectParams.Data2_List[SheetCount].ParamValue_List[TableCount][n][4]
                                };

                                UnknownSectionValueMainDataTableCreator.AddRow(RowObj);
                            }

                            DataTableCreatorList.Add(UnknownSectionValueMainDataTableCreator);
                        }

                        PageTableDictionary.Add(SheetCount, DataTableCreatorList);
                    }

                    UnknownParamDataTableSheetCreator = new DataTableSheetCreator(PageTableDictionary);

                    EffectParams_UnknownSection_DGV.DataSource = UnknownParamDataTableSheetCreator.DataTableSheetDictionary[0][0].DataTable;
                    


                    //Dictionary<int, List<DataTableCreator>> PageTableDictionary = new Dictionary<int, List<DataTableCreator>>();
                    //for (int SheetCount = 0; SheetCount < effectParams.Data2_List.Count; SheetCount++)
                    //{
                    //    List<DataTableCreator> DataTableCreatorList = new List<DataTableCreator>();

                    //    //Add DGV
                    //    for (int TableCount = 0; TableCount < effectParams.Data2_List[SheetCount].ParamValue_List.Count; TableCount++)
                    //    {
                    //        string[] UnknownSectionValueStrArray = new string[] { "Value 1", "Value 2", "Value 3", "Value 4", "Value 5" };
                    //        DataTableCreator UnknownSectionValueMainDataTableCreator = new DataTableCreator(UnknownSectionValueStrArray);

                    //        for (int n = 0; n < effectParams.Data2_List[SheetCount].ParamValue_List[TableCount].Count; n++)
                    //        {
                    //            object[] RowObj = new object[]
                    //            {
                    //                effectParams.Data2_List[SheetCount].ParamValue_List[TableCount][n][0],
                    //                effectParams.Data2_List[SheetCount].ParamValue_List[TableCount][n][1],
                    //                effectParams.Data2_List[SheetCount].ParamValue_List[TableCount][n][2],
                    //                effectParams.Data2_List[SheetCount].ParamValue_List[TableCount][n][3],
                    //                effectParams.Data2_List[SheetCount].ParamValue_List[TableCount][n][4]
                    //            };

                    //            UnknownSectionValueMainDataTableCreator.AddRow(RowObj);
                    //        }

                    //        DataTableCreatorList.Add(UnknownSectionValueMainDataTableCreator);
                    //    }

                    //    PageTableDictionary.Add(SheetCount, DataTableCreatorList);
                    //}

                    //dataGridView5.DataSource = PageTableDictionary[0][0].DataTable;
                    //UnknownParam_DataTableCreatorDictionary = PageTableDictionary;

                    #region DELETE
                    //string[] UnknownSectionValueStrArray = new string[] { "Value 1", "Value 2", "Value 3", "Value 4", "Value 5" };
                    //DataTableCreator UnknownSectionValueMainDataTableCreator = new DataTableCreator(UnknownSectionValueStrArray);

                    ////Add DGV
                    //for (int n = 0; n < effectParams.Data2_List[0].ParamValue_List[0].Count; n++)
                    //{
                    //    object[] RowObj = new object[]
                    //    {
                    //        effectParams.Data2_List[0].ParamValue_List[0][n][0],
                    //        effectParams.Data2_List[0].ParamValue_List[0][n][1],
                    //        effectParams.Data2_List[0].ParamValue_List[0][n][2],
                    //        effectParams.Data2_List[0].ParamValue_List[0][n][3],
                    //        effectParams.Data2_List[0].ParamValue_List[0][n][4]
                    //    };

                    //    UnknownSectionValueMainDataTableCreator.AddRow(RowObj);
                    //}

                    //dataGridView5.DataSource = UnknownSectionValueMainDataTableCreator.DataTable;
                    #endregion

                    #endregion

                    comboBox2.Items.AddRange(Enumerable.Range(0, Convert.ToInt32(effectParams.Data2_RowCount)).ToList().ConvertAll<object>(x => x).ToArray());
                    comboBox2.SelectedIndex = 0;

                    EffectParam_NameSelectComboBox.Items.AddRange(effectParams.Data2_List.Select(x => new string(x.Value.Label_String)).ToArray());
                    EffectParam_NameSelectComboBox.SelectedIndex = 0;

                    br.Close();
                    fs.Close();

                    FilePah_LBL.Text = OpenEffectParams.FileName;
                }
                //else return;

            }
            else if (openParamBinType == ParamBinSelectorForm.ParamBinType.KartConstructInfo)
            {
                OpenFileDialog Open_KartConstructInfo = new OpenFileDialog()
                {
                    Title = "Open MK7 BinaryData (KartConstructInfo)",
                    InitialDirectory = @"C:\Users\User\Desktop",
                    Filter = "bin file|*.bin"
                };

                if (Open_KartConstructInfo.ShowDialog() == DialogResult.OK)
                {
                    FileStream fs = new FileStream(Open_KartConstructInfo.FileName, FileMode.Open, FileAccess.Read);
                    BinaryReader br = new BinaryReader(fs);

                    KartConstructInfo kartConstructInfo = new KartConstructInfo();
                    kartConstructInfo.ReadKartConstructInfo(br);


                    br.Close();
                    fs.Close();

                    FilePah_LBL.Text = Open_KartConstructInfo.FileName;
                }
                //else return;

            }
            else if (openParamBinType == ParamBinSelectorForm.ParamBinType.Abort)
            {
                MessageBox.Show("ファイルの読み込みを中止しました。");
            }
        }

        private void Save_BinData(object sender, EventArgs e)
        {
            ParamBinSelectorForm openParamSelectorForm = new ParamBinSelectorForm();
            openParamSelectorForm.ShowDialog();
            ParamBinSelectorForm.ParamBinType openParamBinType = openParamSelectorForm.ParamTypes[openParamSelectorForm.Index];
            if (openParamBinType == ParamBinSelectorForm.ParamBinType.SlotTable)
            {
                SaveFileDialog Save_SlotTable = new SaveFileDialog()
                {
                    Title = "Save Binary Data (SlotTable)",
                    InitialDirectory = @"C:\Users\User\Desktop",
                    Filter = "bin file|*.bin"
                };

                if (Save_SlotTable.ShowDialog() == DialogResult.OK)
                {
                    FileStream fs = new FileStream(Save_SlotTable.FileName, FileMode.Create, FileAccess.Write);
                    BinaryWriter bw = new BinaryWriter(fs);

                    #region ParamMain
                    DataTable ParamMainDataTable = DataGridView_DataTableConvertor.GetDataTable(ParamMain_DGV, false);
                    DataTableCreator ParamMain_DataTableCreator = new DataTableCreator(ParamMainDataTable);

                    List<string> CellLabelList = ParamMain_DataTableCreator.GetColumnArray().ToList();
                    CellLabelList.RemoveAt(0); //Delete (Name)

                    List<string> RowLabelList = new List<string>();
                    for (int String_RowCount = 0; String_RowCount < ParamMain_DataTableCreator.GetRowCount; String_RowCount++)
                    {
                        RowLabelList.Add(ParamMain_DataTableCreator.GetValue(String_RowCount, 0).ToString());
                    }

                    List<object[]> DataValueArrayList = new List<object[]>();
                    for (int DataRow = 0; DataRow < ParamMain_DataTableCreator.GetRowCount; DataRow++)
                    {
                        List<object> ValueList = new List<object>();
                        for (int CellCount = 1; CellCount < ParamMain_DataTableCreator.GetColumnCount; CellCount++)
                        {
                            if (int.Parse(label2.Text) == (int)Type.Type_UInt32)
                            {
                                ValueList.Add(uint.Parse(ParamMain_DataTableCreator.GetValue(DataRow, CellCount).ToString()));
                            }
                            else if (int.Parse(label2.Text) == (int)Type.Type_Float_P1)
                            {
                                ValueList.Add(float.Parse(ParamMain_DataTableCreator.GetValue(DataRow, CellCount).ToString()));
                            }
                            else if (int.Parse(label2.Text) == (int)Type.Type_Float_P2)
                            {
                                ValueList.Add(float.Parse(ParamMain_DataTableCreator.GetValue(DataRow, CellCount).ToString()));
                            }
                        }

                        DataValueArrayList.Add(ValueList.ToArray());
                    }
                    #endregion

                    #region MinMaxValue
                    DataTable MinMaxValueDataTable = DataGridView_DataTableConvertor.GetDataTable(MinMaxValue_DGV, false);
                    DataTableCreator MinMaxValue_DataTableCreator = new DataTableCreator(MinMaxValueDataTable);

                    List<SlotTable.MinMaxValue> MinMaxValue_List = new List<SlotTable.MinMaxValue>();
                    for (int MInMaxValueColumnCount = 1; MInMaxValueColumnCount < MinMaxValue_DataTableCreator.GetColumnCount; MInMaxValueColumnCount++)
                    {
                        float MinVal = float.Parse(MinMaxValue_DataTableCreator.GetValue(0, MInMaxValueColumnCount).ToString());
                        float MaxVal = float.Parse(MinMaxValue_DataTableCreator.GetValue(1, MInMaxValueColumnCount).ToString());

                        SlotTable.MinMaxValue minMaxValue = new SlotTable.MinMaxValue(MinVal, MaxVal);
                        MinMaxValue_List.Add(minMaxValue);
                    }
                    #endregion

                    SlotTable Write_SlotTable = new SlotTable
                    {
                        DataType = int.Parse(label2.Text),
                        RowCount = ParamMain_DGV.RowCount, //0 or -1(?)
                        CellCount = ParamMain_DGV.ColumnCount - 1,
                        RowLabel_List = RowLabelList,
                        CellLabel_List = CellLabelList,
                        MinMaxValueList = MinMaxValue_List,
                        DataValueArrayList = DataValueArrayList
                    };
                    Write_SlotTable.Write(bw);

                    bw.Close();
                    fs.Close();
                }
                else return;

            }
            else if (openParamBinType == ParamBinSelectorForm.ParamBinType.GeoHitTable)
            {
                SaveFileDialog Save_GeoHitTable = new SaveFileDialog()
                {
                    Title = "Save MK7 BinaryData (GeoHitTable)",
                    InitialDirectory = @"C:\Users\User\Desktop",
                    Filter = "bin file|*.bin"
                };

                if (Save_GeoHitTable.ShowDialog() == DialogResult.OK)
                {
                    FileStream fs = new FileStream(Save_GeoHitTable.FileName, FileMode.Create, FileAccess.Write);
                    BinaryWriter bw = new BinaryWriter(fs);

                    #region DataValueList
                    DataTable GeoHitTableDataTable = DataGridView_DataTableConvertor.GetDataTable(GeoHitItemTableDGV, false);
                    DataTableCreator GeoHitTable_DataTableCreator = new DataTableCreator(GeoHitTableDataTable);

                    List<GeoHitTable.DataValues> DataValuesList = new List<GeoHitTable.DataValues>();
                    for (int RowCount = 0; RowCount < GeoHitTable_DataTableCreator.GetRowCount; RowCount++)
                    {
                        ushort ID = ushort.Parse(GeoHitTable_DataTableCreator.GetValue(RowCount, 0).ToString());

                        List<ushort> ushortList = new List<ushort>();
                        for (int ArrayCount = 1; ArrayCount < GeoHitTable_DataTableCreator.GetColumnCount; ArrayCount++)
                        {
                            ushortList.Add(ushort.Parse(GeoHitTable_DataTableCreator.GetValue(RowCount, ArrayCount).ToString()));
                        }

                        GeoHitTable.DataValues dataValues = new GeoHitTable.DataValues(ID, ushortList.ToArray());
                        DataValuesList.Add(dataValues);
                    }
                    #endregion

                    #region UnknownColorData
                    byte c0 = (byte)int.Parse(textBox1.Text.ToString());
                    byte c1 = (byte)int.Parse(textBox2.Text.ToString());
                    byte c2 = (byte)int.Parse(textBox3.Text.ToString());
                    byte c3 = (byte)int.Parse(textBox4.Text.ToString());

                    GeoHitTable.Color UnknownColor = new GeoHitTable.Color(new byte[] { c0, c1, c2, c3 });
                    #endregion

                    #region UnknownGeoHitTableData
                    DataTable UnknownGeoHitTableDataTable = DataGridView_DataTableConvertor.GetDataTable(UnknownGeoHitTableSection_DGV, false);
                    DataTableCreator UnknownGeoHitTable_DataTableCreator = new DataTableCreator(UnknownGeoHitTableDataTable);

                    List<GeoHitTable.UnknownShortData> UnknownShortDataList = new List<GeoHitTable.UnknownShortData>();
                    for (int RowCount = 0; RowCount < GeoHitTable_DataTableCreator.GetRowCount; RowCount++)
                    {
                        short Data1 = short.Parse(UnknownGeoHitTable_DataTableCreator.GetValue(RowCount, 0).ToString());
                        short Data2 = short.Parse(UnknownGeoHitTable_DataTableCreator.GetValue(RowCount, 1).ToString());

                        GeoHitTable.UnknownShortData UnknownShortData = new GeoHitTable.UnknownShortData(Data1, Data2);
                        UnknownShortDataList.Add(UnknownShortData);
                    }
                    #endregion

                    GeoHitTable geoHitTable = new GeoHitTable()
                    {
                        CellCount = (ushort)GeoHitTable_DataTableCreator.GetColumnCount,
                        RowCount = (ushort)GeoHitTable_DataTableCreator.GetRowCount,
                        DataValues_List = DataValuesList,
                        UnknownColor = UnknownColor,
                        UnknownShortDataList = UnknownShortDataList
                    };

                    geoHitTable.Write(bw);

                    bw.Close();
                    fs.Close();
                }
                else return;

            }
            else if (openParamBinType == ParamBinSelectorForm.ParamBinType.EffectParam)
            {
                SaveFileDialog Save_EffectParams = new SaveFileDialog()
                {
                    Title = "Save MK7 BinaryData (EffectParams)",
                    InitialDirectory = @"C:\Users\User\Desktop",
                    Filter = "bin file|*.bin"
                };

                if (Save_EffectParams.ShowDialog() == DialogResult.OK)
                {
                    FileStream fs = new FileStream(Save_EffectParams.FileName, FileMode.Create, FileAccess.Write);
                    BinaryWriter bw = new BinaryWriter(fs);


                    #region Color
                    DataTable EffectParamsColorSectionDataTable = DataGridView_DataTableConvertor.GetDataTable(EffectParamsColorSectionDGV, false);
                    DataTableCreator EffectParamsColorSection_DataTableCreator = new DataTableCreator(EffectParamsColorSectionDataTable);

                    List<EffectParams.Data1> Data1_List = new List<EffectParams.Data1>();
                    for (int RowCount = 0; RowCount < EffectParamsColorSection_DataTableCreator.GetRowCount; RowCount++)
                    {
                        EffectParams.Data1 data1 = new EffectParams.Data1
                        {
                            Label_String = EffectParamsColorSection_DataTableCreator.GetValue(RowCount, 0).ToString().ToCharArray(),
                            Color_RGBA_Data = new EffectParams.Data1.ColorRGBA_Data
                            {
                                R = float.Parse(EffectParamsColorSection_DataTableCreator.GetValue(RowCount, 1).ToString()),
                                G = float.Parse(EffectParamsColorSection_DataTableCreator.GetValue(RowCount, 2).ToString()),
                                B = float.Parse(EffectParamsColorSection_DataTableCreator.GetValue(RowCount, 3).ToString()),
                                A = float.Parse(EffectParamsColorSection_DataTableCreator.GetValue(RowCount, 4).ToString())
                            },
                            Flag = new EffectParams.Data1.Flags
                            {
                                f1 = uint.Parse(EffectParamsColorSection_DataTableCreator.GetValue(RowCount, 5).ToString()),
                                f2 = uint.Parse(EffectParamsColorSection_DataTableCreator.GetValue(RowCount, 6).ToString()),
                                f3 = uint.Parse(EffectParamsColorSection_DataTableCreator.GetValue(RowCount, 7).ToString()),
                                f4 = uint.Parse(EffectParamsColorSection_DataTableCreator.GetValue(RowCount, 8).ToString())
                            }
                        };

                        Data1_List.Add(data1);


                        //EffectParams.Data1 data1 = new EffectParams.Data1();
                    }
                    #endregion


                    //List<EffectParams.Data2> data2s = new List<EffectParams.Data2>();
                    Dictionary<int, EffectParams.Data2> data2s = new Dictionary<int, EffectParams.Data2>();
                    for (int SheetCount = 0; SheetCount < UnknownParamDataTableSheetCreator.Count; SheetCount++)
                    {
                        List<List<float[]>> SheetList = new List<List<float[]>>();

                        for (int TableCount = 0; TableCount < UnknownParamDataTableSheetCreator.DataTableSheetDictionary[SheetCount].Count; TableCount++)
                        {
                            List<float[]> TableList = new List<float[]>();
                            for (int RowCount = 0; RowCount < UnknownParamDataTableSheetCreator.DataTableSheetDictionary[SheetCount][TableCount].GetRowCount; RowCount++)
                            {
                                float[] data = new float[UnknownParamDataTableSheetCreator.DataTableSheetDictionary[SheetCount][TableCount].GetColumnCount];

                                //ColumnCount
                                for (int ColumnCount = 0; ColumnCount < UnknownParamDataTableSheetCreator.DataTableSheetDictionary[SheetCount][TableCount].GetColumnCount; ColumnCount++)
                                {
                                    data[ColumnCount] = float.Parse(UnknownParamDataTableSheetCreator.DataTableSheetDictionary[SheetCount][TableCount].GetValue(RowCount, ColumnCount).ToString());
                                }

                                TableList.Add(data);
                            }

                            SheetList.Add(TableList);
                        }


                        EffectParams.Data2 data2 = new EffectParams.Data2
                        {
                            Label_String = EffectParam_NameSelectComboBox.Items[SheetCount].ToString().ToArray(),
                            ParamValue_List = SheetList,
                        };

                        data2s.Add(SheetCount, data2);
                    }




                    EffectParams effectParams = new EffectParams
                    {
                        Type = 4,
                        Data1_RowCount = (uint)EffectParamsColorSection_DataTableCreator.GetRowCount,
                        Data1_CellCount = (uint)EffectParamsColorSection_DataTableCreator.GetColumnCount,
                        Data2_CellCount = (uint)UnknownParamDataTableSheetCreator.DataTableSheetDictionary[0][0].GetColumnCount,
                        Data2_RowCount = (uint)UnknownParamDataTableSheetCreator.DataTableSheetDictionary[0][0].GetRowCount,
                        //Data2_TableCount = (uint)UnknownParam_DataTableCreatorDictionary.Count,
                        Data2_TableCount = (uint)UnknownParamDataTableSheetCreator.DataTableSheetDictionary.Count,
                        Data2_Offset = 0,
                        Data1_List = Data1_List,
                        Data2_List = data2s
                        //Data2_List = new Dictionary<int, EffectParams.Data2>(),
                    };


                    effectParams.WriteEffectParams(bw);


                    bw.Close();
                    fs.Close();
                }
                else return;

            }
            else if (openParamBinType == ParamBinSelectorForm.ParamBinType.KartConstructInfo)
            {

            }
            else if (openParamBinType == ParamBinSelectorForm.ParamBinType.Abort)
            {
                MessageBox.Show("ファイルの書き込みを中止しました。");
            }
        }

        private void Close_BinData(object sender, EventArgs e)
        {
            //Delete Data
            ParamMain_DGV.DataSource = null;
            MinMaxValue_DGV.DataSource = null;

            GeoHitItemTableDGV.DataSource = null;
            UnknownGeoHitTableSection_DGV.DataSource = null;

            EffectParamsColorSectionDGV.DataSource = null;
            EffectParams_UnknownSection_DGV.DataSource = null;
            EffectParam_NameSelectComboBox.Items.Clear();
            EffectParam_NameSelectComboBox.Text = "";
            comboBox2.Items.Clear();
            comboBox2.Text = "";

            FilePah_LBL.Text = FilePah_LBL.Tag.ToString();

        }

        private void EffectParam_NameSelectComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            EffectParams_UnknownSection_DGV.DataSource = null;
            EffectParams_UnknownSection_DGV.DataSource = UnknownParamDataTableSheetCreator.DataTableSheetDictionary[EffectParam_NameSelectComboBox.SelectedIndex][comboBox2.SelectedIndex].DataTable;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            EffectParams_UnknownSection_DGV.DataSource = null;
            if (EffectParam_NameSelectComboBox.SelectedIndex > -1)
            {
                EffectParams_UnknownSection_DGV.DataSource = UnknownParamDataTableSheetCreator.DataTableSheetDictionary[EffectParam_NameSelectComboBox.SelectedIndex][comboBox2.SelectedIndex].DataTable;
            }
        }

        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Created by U5h_MK78");
        }
    }
}

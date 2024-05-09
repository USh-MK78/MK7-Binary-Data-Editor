using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MK7_Binary_Data_Editor
{
    public enum Type
    {
        Type_UInt32 = 0, //uint
        Type_Float_P1 = 1, //Single(?)
        Type_Float_P2 = 2, //Float
        Type_Unknown_P3 = 3, //?
        Type_Unknown_P4 = 4, //?
        Type_Unknown_P6 = 6 //?
    }

    public class SlotTable
    {
        public int DataType { get; set; } //0x4
        public Type DataTypeFlag => (Type)Enum.ToObject(typeof(Type), DataType);

        public int RowCount { get; set; } //0x4
        public int CellCount { get; set; } //0x4

        //0x20(32byte), 0x20 * CellCount, 0x20 * RowCount
        public List<string> RowLabel_List { get; set; }
        public List<string> CellLabel_List { get; set; }

        public List<MinMaxValue> MinMaxValueList { get; set; }
        public class MinMaxValue
        {
            public float Data_MinValue { get; set; } //0x4((Data_MinValue * RowCount)
            public float Data_MaxValue { get; set; } //0x4((Data_MaxValue * RowCount)

            public void ReadMinMaxValue(BinaryReader br)
            {
                Data_MinValue = BitConverter.ToSingle(br.ReadBytes(4), 0);
                Data_MaxValue = BitConverter.ToSingle(br.ReadBytes(4), 0);
            }

            public void WriteMinMaxValue(BinaryWriter bw)
            {
                bw.Write(BitConverter.GetBytes(Data_MinValue));
                bw.Write(BitConverter.GetBytes(Data_MaxValue));
            }

            public MinMaxValue(float MinValue, float MaxValue)
            {
                Data_MinValue = MinValue;
                Data_MaxValue = MaxValue;
            }

            public MinMaxValue()
            {
                Data_MinValue = 0;
                Data_MaxValue = 0;
            }
        }

        public List<object[]> DataValueArrayList { get; set; } //0x4(RowCount * CellCount)

        ////data[i] : 0 = false, 1 = true 
        //public List<T[]> GetValues<T>()
        //{
        //    List<T[]> values = new List<T[]>();

        //    foreach (var data in DataValueArrayList)
        //    {
        //        T[] values1 = new T[data.Length];
        //        for (int i = 0; i < values1.Length; i++)
        //        {
        //            values1[i] = (T)data[i];
        //        }

        //        values.Add(values1);
        //    }

        //    return values;
        //}


        public void AddData(object[] Data, int ArrayLength)
        {
            object[] objects = new object[ArrayLength];

            for (int i = 0; i < ArrayLength; i++)
            {
                objects[i] = Data[i];
            }

            DataValueArrayList.Add(objects);
        }


        public void Read(BinaryReader br)
        {
            DataType = BitConverter.ToInt32(br.ReadBytes(4), 0);
            RowCount = BitConverter.ToInt32(br.ReadBytes(4), 0);
            CellCount = BitConverter.ToInt32(br.ReadBytes(4), 0);

            for (int RowLabelStrCount = 0; RowLabelStrCount < RowCount; RowLabelStrCount++)
            {
                RowLabel_List.Add(new string(br.ReadChars(32)));
            }

            CellLabel_List.Add("Name");
            for (int CellLabelStrCount = 0; CellLabelStrCount < CellCount; CellLabelStrCount++)
            {
                CellLabel_List.Add(new string(br.ReadChars(32)));
            }

            for (int MinMaxValueCount = 0; MinMaxValueCount < CellCount; MinMaxValueCount++)
            {
                MinMaxValue minMaxValue = new MinMaxValue(BitConverter.ToSingle(br.ReadBytes(4), 0), BitConverter.ToSingle(br.ReadBytes(4), 0));
                MinMaxValueList.Add(minMaxValue);
            }

            //DataValue
            for (int AddRowCount = 0; AddRowCount < RowCount; AddRowCount++)
            {
                List<object> DataValue_ArrayList = new List<object>();
                DataValue_ArrayList.Add(RowLabel_List[AddRowCount]);

                for (int AddCellCount = 0; AddCellCount < CellCount; AddCellCount++)
                {
                    //MK7Binary_Format.Data_Value = br1.ReadBytes(4);
                    if (DataTypeFlag == Type.Type_UInt32)
                    {
                        int Value = BitConverter.ToInt32(br.ReadBytes(4), 0);
                        DataValue_ArrayList.Add(Value);
                    }
                    else if (DataTypeFlag == Type.Type_Float_P1)
                    {
                        Single Value = BitConverter.ToSingle(br.ReadBytes(4), 0);
                        DataValue_ArrayList.Add(Value);
                    }
                    else if (DataTypeFlag == Type.Type_Float_P2)
                    {
                        float Value = BitConverter.ToSingle(br.ReadBytes(4), 0);
                        DataValue_ArrayList.Add(Value);
                    }
                }

                DataValueArrayList.Add(DataValue_ArrayList.ToArray());
            }
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(DataType);
            bw.Write(RowCount);
            bw.Write(CellCount);

            foreach (var r in RowLabel_List)
            {
                bw.Write(Encoding.UTF8.GetBytes(r));
            }

            foreach (var c in CellLabel_List)
            {
                bw.Write(Encoding.UTF8.GetBytes(c));
            }

            foreach (var minmaxValue in MinMaxValueList)
            {
                byte[] MinVal_Byte = BitConverter.GetBytes(minmaxValue.Data_MinValue);
                bw.Write(MinVal_Byte);

                byte[] MaxVal_Byte = BitConverter.GetBytes(minmaxValue.Data_MaxValue);
                bw.Write(MaxVal_Byte);
            }

            for (int i = 0; i < DataValueArrayList.Count; i++)
            {
                for (int j = 0; j < DataValueArrayList[i].Length; j++)
                {
                    if (DataTypeFlag == Type.Type_UInt32)
                    {
                        bw.Write(BitConverter.GetBytes((uint)DataValueArrayList[i][j]));
                    }
                    else if (DataTypeFlag == Type.Type_Float_P1)
                    {
                        bw.Write(BitConverter.GetBytes((Single)DataValueArrayList[i][j]));
                    }
                    else if (DataTypeFlag == Type.Type_Float_P2)
                    {
                        bw.Write(BitConverter.GetBytes((float)DataValueArrayList[i][j]));
                    }
                }
            }
        }

        public SlotTable()
        {
            DataType = -1;
            RowCount = 0;
            CellCount = 0;

            RowLabel_List = new List<string>();
            CellLabel_List = new List<string>();
            MinMaxValueList = new List<MinMaxValue>();
            DataValueArrayList = new List<object[]>();
        }
    }

    public class GeoHitTable
    {
        public ushort RowCount { get; set; } //0x4
        public ushort CellCount { get; set; } //0x4, (ID data does not count)
        public List<DataValues> DataValues_List { get; set; }
        public class DataValues
        {
            public ushort ID { get; set; }
            public ushort[] Value_Array { get; set; }

            public void ReadDataValue(BinaryReader br, ushort CellCount)
            {
                ID = br.ReadUInt16();

                Value_Array = new ushort[CellCount];
                for (int DataValueCells = 0; DataValueCells < CellCount; DataValueCells++)
                {
                    Value_Array[DataValueCells] = br.ReadUInt16();
                }
            }

            public void WriteDataValue(BinaryWriter bw)
            {
                bw.Write(ID);
                foreach (var value in Value_Array) bw.Write(value);
            }

            public DataValues(ushort ID, ushort[] ValueAry)
            {
                this.ID = ID;
                this.Value_Array = ValueAry;
            }

            public DataValues()
            {
                this.ID = 0;
                this.Value_Array = new List<ushort>().ToArray();
            }
        }

        public Color UnknownColor { get; set; }
        public class Color
        {
            public byte c0 { get; set; }
            public byte c1 { get; set; }
            public byte c2 { get; set; }
            public byte c3 { get; set; }

            public System.Drawing.Color GetColor()
            {
                return System.Drawing.Color.FromArgb(c0, c1, c2, c3);
            }

            public void ReadColor(BinaryReader br)
            {
                c0 = br.ReadByte();
                c1 = br.ReadByte();
                c2 = br.ReadByte();
                c3 = br.ReadByte();
            }

            public void WriteColor(BinaryWriter bw)
            {
                bw.Write(c0);
                bw.Write(c1);
                bw.Write(c2);
                bw.Write(c3);
            }

            public Color(byte[] color)
            {
                c0 = color[0];
                c1 = color[1];
                c2 = color[2];
                c3 = color[3];
            }
        }

        public List<UnknownShortData> UnknownShortDataList { get; set; }
        public class UnknownShortData
        {
            public short Data1 { get; set; }
            public short Data2 { get; set; }

            public void Read(BinaryReader br)
            {
                Data1 = br.ReadInt16();
                Data2 = br.ReadInt16();
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(Data1);
                bw.Write(Data2);
            }

            public UnknownShortData(short Data1, short Data2)
            {
                this.Data1 = Data1;
                this.Data2 = Data2;
            }

            public UnknownShortData()
            {
                Data1 = BitConverter.ToInt16(new byte[] { 0xFF, 0xFF }, 0);
                Data2 = BitConverter.ToInt16(new byte[] { 0xFF, 0xFF }, 0);
            }
        }

        public void Read(BinaryReader br)
        {
            RowCount = br.ReadUInt16();
            CellCount = br.ReadUInt16();

            for (int GHIT_DataValueCount = 0; GHIT_DataValueCount < RowCount; GHIT_DataValueCount++)
            {
                DataValues DataValue = new DataValues();
                DataValue.ReadDataValue(br, CellCount);
                DataValues_List.Add(DataValue);
            }

            UnknownColor.ReadColor(br);

            for (int Count = 0; Count < RowCount; Count++)
            {
                UnknownShortData unknownShortData = new UnknownShortData();
                unknownShortData.Read(br);
                UnknownShortDataList.Add(unknownShortData);
            }
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(RowCount);
            bw.Write(CellCount);

            foreach (var h in DataValues_List)
            {
                h.WriteDataValue(bw);
            }

            UnknownColor.WriteColor(bw);

            foreach (var item in UnknownShortDataList)
            {
                item.Write(bw);
            }
        }

        public GeoHitTable()
        {
            RowCount = 0;
            CellCount = 0;
            DataValues_List = new List<DataValues>();
            UnknownColor = new Color(new byte[4]);
            UnknownShortDataList = new List<UnknownShortData>();
        }
    }

    public class EffectParams
    {
        public uint Type { get; set; } //0x4
        public Type DataTypeFlag => (Type)Enum.ToObject(typeof(Type), Type);


        public uint Data1_RowCount { get; set; } //0x4
        public uint Data1_CellCount { get; set; } //0x4
        public uint Data2_CellCount { get; set; }
        public uint Data2_RowCount { get; set; } //0x4
        public uint Data2_TableCount { get; set; } //0x4
        public uint Data2_Offset { get; set; } //0x4

        public List<Data1> Data1_List { get; set; } 
        public class Data1
        {
            //String
            public string Name
            {
                get
                {
                    return new string(Label_String);
                }
            }

            public char[] Label_String { get; set; } //0x20(32byte)

            public ColorRGBA_Data Color_RGBA_Data { get; set; }
            public class ColorRGBA_Data
            {
                public float R { get; set; }
                public float G { get; set; }
                public float B { get; set; }
                public float A { get; set; }

                public void Read(BinaryReader br)
                {
                    R = br.ReadSingle();
                    G = br.ReadSingle();
                    B = br.ReadSingle();
                    A = br.ReadSingle();
                }

                public void Write(BinaryWriter bw)
                {
                    bw.Write(R);
                    bw.Write(G);
                    bw.Write(B);
                    bw.Write(A);
                }

                public ColorRGBA_Data(float R, float G, float B, float A)
                {
                    this.R = R;
                    this.G = G;
                    this.B = B;
                    this.A = A;
                }

                public ColorRGBA_Data()
                {
                    this.R = 0;
                    this.G = 0;
                    this.B = 0;
                    this.A = 0;
                }
            }

            public Flags Flag { get; set; }
            public class Flags
            {
                public uint f1 { get; set; }
                public uint f2 { get; set; }
                public uint f3 { get; set; }
                public uint f4 { get; set; }

                public void ReadFlags(BinaryReader br)
                {
                    f1 = br.ReadUInt32();
                    f2 = br.ReadUInt32();
                    f3 = br.ReadUInt32();
                    f4 = br.ReadUInt32();
                }

                public void WriteFlags(BinaryWriter bw)
                {
                    bw.Write(f1);
                    bw.Write(f2);
                    bw.Write(f3);
                    bw.Write(f4);
                }

                public Flags(uint f1, uint f2, uint f3, uint f4)
                {
                    this.f1 = f1;
                    this.f2 = f2;
                    this.f3 = f3;
                    this.f4 = f4;
                }

                public Flags()
                {
                    this.f1 = 0;
                    this.f2 = 0;
                    this.f3 = 0;
                    this.f4 = 0;
                }
            }

            public void Read(BinaryReader br)
            {
                Label_String = br.ReadChars(32);
                Color_RGBA_Data.Read(br);
                Flag.ReadFlags(br);
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(Label_String);
                Color_RGBA_Data.Write(bw);
                Flag.WriteFlags(bw);
            }

            public Data1(char[] LabelString, ColorRGBA_Data RGBAData, Flags Flags)
            {
                Label_String = LabelString;
                Color_RGBA_Data = RGBAData;
                Flag = Flags;
            }

            public Data1()
            {
                Label_String = new char[32];
                Color_RGBA_Data = new ColorRGBA_Data();
                Flag = new Flags();
            }
        }

        public Dictionary<int, Data2> Data2_List { get; set; } //Page
        public class Data2
        {
            public char[] Label_String { get; set; } //0x20(32byte)
            public List<List<float[]>> ParamValue_List { get; set; } //Tables

            public void ReadData2(BinaryReader br, uint Data2_RowCount, uint Data2_CellCount)
            {
                Label_String = br.ReadChars(32);

                for (int TRowsCount = 0; TRowsCount < Data2_RowCount; TRowsCount++) //6
                {
                    List<float[]> Cell_FloatAry_List = new List<float[]>();
                    for (int TCellCount = 0; TCellCount < Data2_CellCount; TCellCount++) //42
                    {
                        float[] r1 = new float[] { br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle() };
                        Cell_FloatAry_List.Add(r1);
                    }

                    ParamValue_List.Add(Cell_FloatAry_List);
                }
            }

            public void WriteData2(BinaryWriter bw, uint Data2_RowCount, uint Data2_CellCount)
            {
                bw.Write(Label_String);

                for (int TRowsCount = 0; TRowsCount < Data2_RowCount; TRowsCount++) //6
                {
                    for (int TCellCount = 0; TCellCount < Data2_CellCount; TCellCount++) //42
                    {
                        bw.Write(ParamValue_List[TRowsCount][TCellCount][0]);
                        bw.Write(ParamValue_List[TRowsCount][TCellCount][1]);
                        bw.Write(ParamValue_List[TRowsCount][TCellCount][2]);
                        bw.Write(ParamValue_List[TRowsCount][TCellCount][3]);
                        bw.Write(ParamValue_List[TRowsCount][TCellCount][4]);
                    }
                }
            }

            public Data2(string Name, List<List<float[]>> ParamValueList)
            {
                Label_String = Name.ToCharArray();
                ParamValue_List = ParamValueList;
            }

            public Data2()
            {
                Label_String = new char[32];
                ParamValue_List = new List<List<float[]>>();
            }
        }

        public void ReadEffectParams(BinaryReader br)
        {
            Type = br.ReadUInt32();
            Data1_RowCount = br.ReadUInt32();
            Data1_CellCount = br.ReadUInt32();
            Data2_CellCount = br.ReadUInt32();
            Data2_RowCount = br.ReadUInt32();
            Data2_TableCount = br.ReadUInt32();
            Data2_Offset = br.ReadUInt32();

            long BasePos = br.BaseStream.Position;

            if (DataTypeFlag == MK7_Binary_Data_Editor.Type.Type_Unknown_P4)
            {
                for (int r = 0; r < Data1_RowCount; r++)
                {
                    Data1 data1 = new Data1();
                    data1.Read(br);

                    Data1_List.Add(data1);
                }
            }

            br.BaseStream.Position = BasePos;
            br.BaseStream.Seek(Data2_Offset, SeekOrigin.Current);

            for (int Data2_Count = 0; Data2_Count < Data2_TableCount; Data2_Count++) //42
            {
                Data2 data2_Value = new Data2();
                data2_Value.ReadData2(br, Data2_RowCount, Data2_CellCount);
                Data2_List.Add(Data2_Count, data2_Value);
            }
        }

        public void WriteEffectParams(BinaryWriter bw)
        {
            bw.Write(Type);
            bw.Write(Data1_RowCount);
            bw.Write(Data1_CellCount);
            bw.Write(Data2_CellCount);
            bw.Write(Data2_RowCount);
            bw.Write(Data2_TableCount);
            bw.Write(Data2_Offset);

            long Data2_LengthBeginPos = bw.BaseStream.Position;

            if (DataTypeFlag == MK7_Binary_Data_Editor.Type.Type_Unknown_P4)
            {
                for (int r = 0; r < Data1_RowCount; r++)
                {

                    Data1_List[r].Write(bw);
                }
            }

            long Data2_LengthEndPos = bw.BaseStream.Position;

            //WriteLength
            bw.BaseStream.Position = Data2_LengthBeginPos - 4;
            bw.Write(Data2_LengthEndPos - Data2_LengthBeginPos);
            bw.BaseStream.Position = Data2_LengthEndPos;

            for (int Data2_Count = 0; Data2_Count < Data2_TableCount; Data2_Count++) //42
            {
                Data2_List[Data2_Count].WriteData2(bw, Data2_CellCount, Data2_RowCount);

                //Data2 data2_Value = new Data2();
                //data2_Value.ReadData2(br, Data2_RowCount, Data2_CellCount);
                //Data2_List.Add(Data2_Count, data2_Value);
            }
        }

        public EffectParams()
        {
            Type = 0;
            Data1_RowCount = 0;
            Data1_CellCount = 0;
            Data2_CellCount = 0;
            Data2_RowCount = 0;
            Data2_TableCount = 0;
            Data2_Offset = 0;

            Data1_List = new List<Data1>();
            Data2_List = new Dictionary<int, Data2>();
        }
    }

    public class KartConstructInfo
    {
        //KartBody : 0x10

        //Data : 0xFDCC, float (0x4)


        public enum Type
        {
            All = 54,
            screw = 1,
        }

        public byte Unknown1 { get; set; }
        public byte Unknown2 { get; set; }
        public ushort Unknown3 { get; set; }
        public uint UnknownDataCount { get; set; }

        public float[] UnknownByteArray1 { get; set; } //0x8

        //public List<float> UnknownFloatArray { get; set; } //UnknownDataCount

        public List<UnknownDataArea> UnknownDataAreaList { get; set; }
        public class UnknownDataArea
        {
            public float Data_1 { get; set; }
            public float Data_2 { get; set; }

            public void ReadUnknownDataArea(BinaryReader br)
            {
                Data_1 = br.ReadSingle();
                Data_2 = br.ReadSingle();
            }

            public void WriteUnknownDataArea(BinaryWriter bw)
            {
                bw.Write(Data_1);
                bw.Write(Data_2);
            }

            public UnknownDataArea(float Data_1, float Data_2)
            {
                this.Data_1 = Data_1;
                this.Data_2 = Data_2;
            }

            public UnknownDataArea()
            {
                Data_1 = 0;
                Data_2 = 0;
            }
        }


        public List<UnknownCharacterParam> UnknownCharacterParams { get; set; }
        public class UnknownCharacterParam
        {
            public int Param1 { get; set; }
            public int Param2 { get; set; }
            public int Param3 { get; set; }
            public int Param4 { get; set; }
            public int Param5 { get; set; }
            public int Param6 { get; set; }
            public int Param7 { get; set; }

            public void ReadUnknownCharacterParam(BinaryReader br)
            {
                Param1 = br.ReadInt32();
                Param2 = br.ReadInt32();
                Param3 = br.ReadInt32();
                Param4 = br.ReadInt32();
                Param5 = br.ReadInt32();
                Param6 = br.ReadInt32();
                Param7 = br.ReadInt32();
            }

            public UnknownCharacterParam()
            {
                Param1 = 0;
                Param2 = 0;
                Param3 = 0;
                Param4 = 0;
                Param5 = 0;
                Param6 = 0;
                Param7 = 0;
            }
        }


        //RowCOunt = 21(?)

        //public byte[] UnknownByteArray2 { get; set; } //504 byte

        //14

        //public List<UnknownCharacterParam> UnknownCharacterParams { get; set; }
        //public class UnknownCharacterParam
        //{
        //    public byte Param1 { get; set; }
        //    public byte Param2 { get; set; }
        //    public byte Param3 { get; set; }
        //    public byte Param4 { get; set; }
        //    public byte Param5 { get; set; }
        //    public byte Param6 { get; set; }
        //    public byte Param7 { get; set; }
        //    public byte Param8 { get; set; }
        //    public byte Param9 { get; set; }
        //    public byte Param10 { get; set; }
        //    public byte Param11 { get; set; }
        //    public byte Param12 { get; set; }
        //    public byte Param13 { get; set; }
        //    public byte Param14 { get; set; }

        //    public void ReadUnknownCharacterParam(BinaryReader br)
        //    {
        //        Param1 = br.ReadByte();
        //        Param2 = br.ReadByte();
        //        Param3 = br.ReadByte();
        //        Param4 = br.ReadByte();
        //        Param5 = br.ReadByte();
        //        Param6 = br.ReadByte();
        //        Param7 = br.ReadByte();
        //        Param8 = br.ReadByte();
        //        Param9 = br.ReadByte();
        //        Param10 = br.ReadByte();
        //        Param11 = br.ReadByte();
        //        Param12 = br.ReadByte();
        //        Param13 = br.ReadByte();
        //        Param14 = br.ReadByte();
        //    }

        //    public UnknownCharacterParam()
        //    {
        //        Param1 = 0x00;
        //        Param2 = 0x00;
        //        Param3 = 0x00;
        //        Param4 = 0x00;
        //        Param5 = 0x00;
        //        Param6 = 0x00;
        //        Param7 = 0x00;
        //        Param8 = 0x00;
        //        Param9 = 0x00;
        //        Param10 = 0x00;
        //        Param11 = 0x00;
        //        Param12 = 0x00;
        //        Param13 = 0x00;
        //        Param14 = 0x00;
        //    }
        //}

        //Count : 1
        public List<Screw> Screws { get; set; }
        public class Screw
        {
            public char[] ScrewNameCharArray { get; set; } //0x16

            public void ReadScrew(BinaryReader br)
            {
                ScrewNameCharArray = br.ReadChars(16);
            }

            public Screw()
            {
                ScrewNameCharArray = new char[16];
            }
        }

        //Count : 7
        public List<Wing> Wings { get; set; }
        public class Wing
        {
            public char[] WingNameCharArray { get; set; } //0x16

            public void ReadWing(BinaryReader br)
            {
                WingNameCharArray = br.ReadChars(16);
            }

            public Wing()
            {
                WingNameCharArray = new char[16];
            }
        }

        //Count : 10
        public List<Tire> Tires { get; set; }
        public class Tire
        {
            public char[] TireAbbreviationNameCharArray { get; set; } //0x8
            public char[] TireNameCharArray { get; set; } //0x16

            public void ReadTire(BinaryReader br)
            {
                TireAbbreviationNameCharArray = br.ReadChars(8);
                TireNameCharArray = br.ReadChars(16);
            }

            public Tire()
            {
                TireAbbreviationNameCharArray = new char[8];
                TireNameCharArray = new char[16];
            }
        }

        //Count : 17
        public List<Body> Bodies { get; set; }
        public class Body
        {
            public char[] BodyNameCharArray { get; set; } //0x16

            public void ReadBody(BinaryReader br)
            {
                BodyNameCharArray = br.ReadChars(16);
            }

            public Body()
            {
                BodyNameCharArray = new char[16];
            }
        }

        //Count : 18
        public List<Character> Characters { get; set; }
        public class Character
        {
            public enum Weight
            {
                L,
                M,
                S,
            }

            public float UnknownValue1 { get; set; }
            public char[] CharacterAbbreviationName { get; set; } //0x4
            public char WeightType { get; set; } //0x1
            public byte UnknownValue2 { get; set; }
            public byte UnknownValue3 { get; set; }
            public byte UnknownValue4 { get; set; }
            public char[] CharacterName { get; set; } //0x12

            public void ReadCharacter(BinaryReader br)
            {
                UnknownValue1 = br.ReadSingle();
                CharacterAbbreviationName = br.ReadChars(4);
                WeightType = br.ReadChar();
                UnknownValue2 = br.ReadByte();
                UnknownValue3 = br.ReadByte();
                UnknownValue4 = br.ReadByte();
                CharacterName = br.ReadChars(12);
            }

            public Character()
            {
                UnknownValue1 = 0;
                CharacterAbbreviationName = new char[4];
                WeightType = new char();
                UnknownValue2 = 0x00;
                UnknownValue3 = 0x00;
                UnknownValue4 = 0x00;
                CharacterName = new char[12];
            }
        }

        //(5200 / 4) / 52 = 25 [*]
        //(5200 / 4) / 50 = 26
        //(5200 / 4) / 26 = 50
        public List<UnknownDataArea2> UnknownDataArea2_List { get; set; } //25
        public class UnknownDataArea2
        {
            public float[] FloatData { get; set; } //52

            public void ReadUnknownDataArea2(BinaryReader br)
            {
                for (int i = 0; i < 52; i++)
                {
                    FloatData[i] = br.ReadSingle();
                }
            }

            public void WriteUnknownDataArea2(BinaryWriter bw)
            {
                for (int i = 0; i < 52; i++)
                {
                    bw.Write(FloatData[i]);
                }
            }

            public UnknownDataArea2()
            {
                FloatData = new float[52];
            }
        }

        //(59772 / 4) / 17 
        public List<UnknownDataArea3> UnknownDataArea3_List { get; set; }
        public class UnknownDataArea3
        {
            public float[] Data0_Array { get; set; } //17

            public List<Data1> Data1_List { get; set; }
            public class Data1
            {
                //Daat 1
                //304 / 4 / 19 = 4
                //304 / 4 / 4 = 19 [*]

                public float[] Data1_Array { get; set; }

                public void Read_Data1Array(BinaryReader br)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Data1_Array[i] = br.ReadSingle();
                    }
                }

                public void Write_Data1Array(BinaryWriter bw)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        bw.Write(Data1_Array[i]);
                    }
                }

                public Data1()
                {
                    Data1_Array = new float[4];
                }
            }

            public List<Data2> Data2_List { get; set; }
            public class Data2
            {
                //Data 2
                //504 / 4 / 18 = 7
                //504 / 4 / 7 = 18 [*]

                public float[] Data2_Array { get; set; }

                public void Read_Data2Array(BinaryReader br)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        Data2_Array[i] = br.ReadSingle();
                    }
                }

                public void Write_Data2Array(BinaryWriter bw)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        bw.Write(Data2_Array[i]);
                    }
                }

                public Data2()
                {
                    Data2_Array = new float[7];
                }
            }

            public List<Data3> Data3_List { get; set; }
            public class Data3
            {
                //Data 3
                //2640 / 4 / 20 = 33 [*]
                //2640 / 4 / 33 = 20
                //2640 / 4 / 44 = 15
                //2640 / 4 / 15 = 44

                public float[] Data3_Array { get; set; }

                public void Read_Data3Array(BinaryReader br)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        Data3_Array[i] = br.ReadSingle();
                    }
                }

                public void Write_Data3Array(BinaryWriter bw)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        bw.Write(Data3_Array[i]);
                    }
                }

                public Data3()
                {
                    Data3_Array = new float[20];
                }
            }

            public void ReadUnknownDataArea3(BinaryReader br)
            {
                for (int i = 0; i < 17; i++)
                {
                    Data0_Array[i] = br.ReadSingle();
                }

                for (int i = 0; i < 19; i++)
                {
                    Data1 data1 = new Data1();
                    data1.Read_Data1Array(br);
                    Data1_List.Add(data1);
                }

                for (int i = 0; i < 18; i++)
                {
                    Data2 data2 = new Data2();
                    data2.Read_Data2Array(br);
                    Data2_List.Add(data2);
                }

                for (int i = 0; i < 20; i++)
                {
                    Data3 data3 = new Data3();
                    data3.Read_Data3Array(br);
                    Data3_List.Add(data3);
                }
            }

            public void WriteUnknownDataArea3(BinaryWriter bw)
            {
                for (int i = 0; i < 17; i++)
                {
                    bw.Write(Data0_Array[i]);
                }

                for (int i = 0; i < 19; i++)
                {
                    Data1_List[i].Write_Data1Array(bw);
                }

                for (int i = 0; i < 18; i++)
                {
                    Data2_List[i].Write_Data2Array(bw);
                }

                for (int i = 0; i < 20; i++)
                {
                    Data3_List[i].Write_Data3Array(bw);
                }
            }

            public UnknownDataArea3()
            {
                Data0_Array = new float[17];
                Data1_List = new List<Data1>();
                Data2_List = new List<Data2>();
                Data3_List = new List<Data3>();

            }
        }




        //26


        //public List<UnknownDataArea2> UnknownDataArea2_List { get; set; }
        //public class UnknownDataArea2
        //{
        //    public float Data_1 { get; set; }
        //    public float Data_2 { get; set; }

        //    public void ReadUnknownDataArea2(BinaryReader br)
        //    {
        //        Data_1 = br.ReadSingle();
        //        Data_2 = br.ReadSingle();
        //    }

        //    public void WriteUnknownDataArea2(BinaryWriter bw)
        //    {
        //        bw.Write(Data_1);
        //        bw.Write(Data_2);
        //    }

        //    public UnknownDataArea2(float Data_1, float Data_2)
        //    {
        //        this.Data_1 = Data_1;
        //        this.Data_2 = Data_2;
        //    }

        //    public UnknownDataArea2()
        //    {
        //        Data_1 = 0;
        //        Data_2 = 0;
        //    }
        //}

        //// (720 / 4) * 10 = 18, (720 / 4) * 18 = 10
        //public List<UnknownDataArea3> UnknownDataArea3_List { get; set; }
        //public class UnknownDataArea3
        //{
        //    public float Data_1 { get; set; }
        //    public float Data_2 { get; set; }
        //    public float Data_3 { get; set; }
        //    public float Data_4 { get; set; }
        //    public float Data_5 { get; set; }
        //    public float Data_6 { get; set; }
        //    public float Data_7 { get; set; }
        //    public float Data_8 { get; set; }
        //    public float Data_9 { get; set; }
        //    public float Data_10 { get; set; }

        //    //public float Data_11 { get; set; }
        //    //public float Data_12 { get; set; }
        //    //public float Data_13 { get; set; }
        //    //public float Data_14 { get; set; }
        //    //public float Data_15 { get; set; }
        //    //public float Data_16 { get; set; }
        //    //public float Data_17 { get; set; }
        //    //public float Data_18 { get; set; }

        //    public void ReadUnknownDataArea3(BinaryReader br)
        //    {
        //        Data_1 = br.ReadSingle();
        //        Data_2 = br.ReadSingle();
        //        Data_3 = br.ReadSingle();
        //        Data_4 = br.ReadSingle();
        //        Data_5 = br.ReadSingle();
        //        Data_6 = br.ReadSingle();
        //        Data_7 = br.ReadSingle();
        //        Data_8 = br.ReadSingle();
        //        Data_9 = br.ReadSingle();
        //        Data_10 = br.ReadSingle();


        //        //Data_11 = br.ReadSingle();
        //        //Data_12 = br.ReadSingle();
        //        //Data_13 = br.ReadSingle();
        //        //Data_14 = br.ReadSingle();
        //        //Data_15 = br.ReadSingle();
        //        //Data_16 = br.ReadSingle();
        //        //Data_17 = br.ReadSingle();
        //        //Data_18 = br.ReadSingle();
        //    }

        //    public void WriteUnknownDataArea3(BinaryWriter bw)
        //    {
        //        bw.Write(Data_1);
        //        bw.Write(Data_2);
        //        bw.Write(Data_3);
        //        bw.Write(Data_4);
        //        bw.Write(Data_5);
        //        bw.Write(Data_6);
        //        bw.Write(Data_7);
        //        bw.Write(Data_8);
        //        bw.Write(Data_9);
        //        bw.Write(Data_10);


        //        //bw.Write(Data_11);
        //        //bw.Write(Data_12);
        //        //bw.Write(Data_13);
        //        //bw.Write(Data_14);
        //        //bw.Write(Data_15);
        //        //bw.Write(Data_16);
        //        //bw.Write(Data_17);
        //        //bw.Write(Data_18);

        //    }

        //    //public UnknownDataArea3(float Data_1, float Data_2)
        //    //{
        //    //    this.Data_1 = Data_1;
        //    //    this.Data_2 = Data_2;
        //    //}

        //    public UnknownDataArea3()
        //    {
        //        Data_1 = 0;
        //        Data_2 = 0;
        //        Data_3 = 0;
        //        Data_4 = 0;
        //        Data_5 = 0;
        //        Data_6 = 0;
        //        Data_7 = 0;
        //        Data_8 = 0;
        //        Data_9 = 0;
        //        Data_10 = 0;


        //        //Data_11 = 0;
        //        //Data_12 = 0;
        //        //Data_13 = 0;
        //        //Data_14 = 0;
        //        //Data_15 = 0;
        //        //Data_16 = 0;
        //        //Data_17 = 0;
        //        //Data_18 = 0;
        //    }
        //}


        ////public float[] Data0 { get; set; } //880 bytes


        //public List<UnknownData1> UnknownData1_List { get; set; } //Count : 21
        //public class UnknownData1
        //{
        //    public float D1 { get; set; }
        //    public float D2 { get; set; }
        //    public float D3 { get; set; }
        //    public float D4 { get; set; }
        //    public float D5 { get; set; }
        //    public float D6 { get; set; }
        //    public float D7 { get; set; }
        //    public float D8 { get; set; }
        //    public float D9 { get; set; }
        //    public float D10 { get; set; }
        //    public float D11 { get; set; }
        //    public float D12 { get; set; }
        //    public float D13 { get; set; }
        //    public float D14 { get; set; }
        //    public float D15 { get; set; }
        //    public float D16 { get; set; }
        //    public float D17 { get; set; }
        //    public float D18 { get; set; }
        //    public float D19 { get; set; }
        //    public float D20 { get; set; }
        //    public float D21 { get; set; }
        //    public float D22 { get; set; }
        //    public float D23 { get; set; }
        //    public float D24 { get; set; }
        //    public float D25 { get; set; }
        //    public float D26 { get; set; }
        //    public float D27 { get; set; }
        //    public float D28 { get; set; }
        //    public float D29 { get; set; }
        //    public float D30 { get; set; }
        //    public float D31 { get; set; }
        //    public float D32 { get; set; }
        //    public float D33 { get; set; }
        //}


        //public List<UnknownData1> UnknownData1_List { get; set; } //Count : 21
        //public class UnknownData1
        //{
        //    public float D1 { get; set; }
        //    public float D2 { get; set; }
        //    public float D3 { get; set; }
        //    public float D4 { get; set; }
        //    public float D5 { get; set; }
        //    public float D6 { get; set; }
        //    public float D7 { get; set; }
        //    public float D8 { get; set; }
        //    public float D9 { get; set; }
        //    public float D10 { get; set; }
        //    public float D11 { get; set; }
        //    public float D12 { get; set; }
        //    public float D13 { get; set; }
        //    public float D14 { get; set; }
        //    public float D15 { get; set; }
        //    public float D16 { get; set; }
        //    public float D17 { get; set; }
        //    public float D18 { get; set; }
        //    public float D19 { get; set; }
        //    public float D20 { get; set; }
        //    public float D21 { get; set; }
        //    public float D22 { get; set; }
        //    public float D23 { get; set; }
        //    public float D24 { get; set; }
        //    public float D25 { get; set; }
        //    public float D26 { get; set; }
        //    public float D27 { get; set; }
        //    public float D28 { get; set; }
        //    public float D29 { get; set; }
        //    public float D30 { get; set; }
        //    public float D31 { get; set; }
        //    public float D32 { get; set; }
        //    public float D33 { get; set; }
        //}

        public void ReadKartConstructInfo(BinaryReader br)
        {
            Unknown1 = br.ReadByte();
            Unknown2 = br.ReadByte();
            Unknown3 = br.ReadUInt16();
            UnknownDataCount = br.ReadUInt32();
            UnknownByteArray1 = new float[] { br.ReadSingle(), br.ReadSingle() };

            for (int i = 0; i < 18; i++)
            {
                UnknownDataArea unknownDataArea = new UnknownDataArea();
                unknownDataArea.ReadUnknownDataArea(br);
                UnknownDataAreaList.Add(unknownDataArea);
            }

            for (int i = 0; i < 18; i++)
            {
                UnknownCharacterParam unknownCharacterParam = new UnknownCharacterParam();
                unknownCharacterParam.ReadUnknownCharacterParam(br);
                UnknownCharacterParams.Add(unknownCharacterParam);
            }

            for (int i = 0; i < 1; i++)
            {
                Screw screw = new Screw();
                screw.ReadScrew(br);
                Screws.Add(screw);
            }

            for (int i = 0; i < 7; i++)
            {
                Wing wing = new Wing();
                wing.ReadWing(br);
                Wings.Add(wing);
            }

            for (int i = 0; i < 10; i++)
            {
                Tire tire = new Tire();
                tire.ReadTire(br);
                Tires.Add(tire);
            }

            for (int i = 0; i < 17; i++)
            {
                Body body = new Body();
                body.ReadBody(br);
                Bodies.Add(body);
            }

            for (int i = 0; i < 18; i++)
            {
                Character character = new Character();
                character.ReadCharacter(br);
                Characters.Add(character);
            }

            for (int i = 0; i < 25; i++)
            {
                UnknownDataArea2 unknownDataArea2 = new UnknownDataArea2();
                unknownDataArea2.ReadUnknownDataArea2(br);
                UnknownDataArea2_List.Add(unknownDataArea2);
            }

            for (int i = 0; i < 17; i++)
            {
                UnknownDataArea3 unknownDataArea3 = new UnknownDataArea3();
                unknownDataArea3.ReadUnknownDataArea3(br);
                UnknownDataArea3_List.Add(unknownDataArea3);
            }
        }

        public KartConstructInfo()
        {
            Unknown1 = 0x00;
            Unknown2 = 0x00;
            Unknown3 = 0;
            UnknownDataCount = 0;
            UnknownByteArray1 = new float[2];

            UnknownDataAreaList = new List<UnknownDataArea>();
            UnknownCharacterParams = new List<UnknownCharacterParam>();

            Screws = new List<Screw>();
            Wings = new List<Wing>();
            Tires = new List<Tire>();
            Bodies = new List<Body>();
            Characters = new List<Character>();

            UnknownDataArea2_List = new List<UnknownDataArea2>();
            UnknownDataArea3_List = new List<UnknownDataArea3>();
        }

        public class Para_D1
        {
            public List<D1> d1s { get; set; }
            public class D1
            {
                public uint Value { get; set; }
            }

            public List<D2> d2s { get; set; }
            public class D2
            {
                public uint Value { get; set; }
            }

            public List<D3> d3s { get; set; }
            public class D3
            {
                public uint Value { get; set; }
            }

            public List<D4> d4s { get; set; }
            public class D4
            {
                public uint Value { get; set; }
            }

            public List<D5> d5s { get; set; }
            public class D5
            {
                public uint Value { get; set; }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MK7_Binary_Data_Editor
{
    public partial class ParamBinSelectorForm : Form
    {
        public ParamBinType[] ParamTypes { get; set; }
        public int Index { get; set; }

        public enum ParamBinType
        {
            Abort = 0,
            SlotTable = 1,
            GeoHitTable = 2,
            EffectParam = 3,
            KartConstructInfo = 4
        }

        public ParamBinSelectorForm()
        {
            InitializeComponent();
        }

        private void ParamSelectorForm_Load(object sender, EventArgs e)
        {
            //Disable Close Button
            this.ControlBox = false;

            ParamTypes = Enum.GetValues(typeof(ParamBinType)).Cast<ParamBinType>().ToArray();
            string[] ary = ParamTypes.Select(x => x.ToString()).ToArray();

            ParamBinTypeSelect_ComboBox.Items.AddRange(ary);

            ParamBinTypeSelect_ComboBox.SelectedIndex = 1;
        }

        private void ParamBinTypeSelect_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Index = ParamBinTypeSelect_ComboBox.SelectedIndex;
            ParamBinType paramBinType = ParamTypes[Index];
            if (paramBinType != ParamBinType.Abort)
            {
                Select_ParamDataType_Btn.Text = "Select";
            }
            else
            {
                Select_ParamDataType_Btn.Text = "Close";
            }
        }

        private void Select_ParamDataType_Btn_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

namespace MK7_Binary_Data_Editor
{
    partial class ParamBinSelectorForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Select_ParamDataType_Btn = new System.Windows.Forms.Button();
            this.ParamBinTypeSelect_ComboBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // Select_ParamDataType_Btn
            // 
            this.Select_ParamDataType_Btn.Location = new System.Drawing.Point(12, 38);
            this.Select_ParamDataType_Btn.Name = "Select_ParamDataType_Btn";
            this.Select_ParamDataType_Btn.Size = new System.Drawing.Size(172, 23);
            this.Select_ParamDataType_Btn.TabIndex = 3;
            this.Select_ParamDataType_Btn.Text = "Select";
            this.Select_ParamDataType_Btn.UseVisualStyleBackColor = true;
            this.Select_ParamDataType_Btn.Click += new System.EventHandler(this.Select_ParamDataType_Btn_Click);
            // 
            // ParamBinTypeSelect_ComboBox
            // 
            this.ParamBinTypeSelect_ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ParamBinTypeSelect_ComboBox.FormattingEnabled = true;
            this.ParamBinTypeSelect_ComboBox.Location = new System.Drawing.Point(12, 12);
            this.ParamBinTypeSelect_ComboBox.Name = "ParamBinTypeSelect_ComboBox";
            this.ParamBinTypeSelect_ComboBox.Size = new System.Drawing.Size(172, 20);
            this.ParamBinTypeSelect_ComboBox.TabIndex = 2;
            this.ParamBinTypeSelect_ComboBox.SelectedIndexChanged += new System.EventHandler(this.ParamBinTypeSelect_ComboBox_SelectedIndexChanged);
            // 
            // ParamBinSelectorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(197, 71);
            this.Controls.Add(this.Select_ParamDataType_Btn);
            this.Controls.Add(this.ParamBinTypeSelect_ComboBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "ParamBinSelectorForm";
            this.Text = "Select";
            this.Load += new System.EventHandler(this.ParamSelectorForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Select_ParamDataType_Btn;
        private System.Windows.Forms.ComboBox ParamBinTypeSelect_ComboBox;
    }
}
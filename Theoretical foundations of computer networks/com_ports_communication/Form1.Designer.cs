
namespace com_ports_communication
{
    partial class ComForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Input = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.Output = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.ClearButton = new System.Windows.Forms.Button();
            this.SendButton = new System.Windows.Forms.Button();
            this.BaudRateComboBox = new System.Windows.Forms.ComboBox();
            this.ControlAndDebug = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // Input
            // 
            this.Input.Location = new System.Drawing.Point(26, 45);
            this.Input.Multiline = true;
            this.Input.Name = "Input";
            this.Input.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.Input.Size = new System.Drawing.Size(634, 143);
            this.Input.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(29, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "Input:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(29, 199);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "Output:";
            // 
            // Output
            // 
            this.Output.BackColor = System.Drawing.SystemColors.HighlightText;
            this.Output.Location = new System.Drawing.Point(26, 229);
            this.Output.Multiline = true;
            this.Output.Name = "Output";
            this.Output.ReadOnly = true;
            this.Output.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.Output.Size = new System.Drawing.Size(634, 143);
            this.Output.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(29, 384);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(139, 20);
            this.label3.TabIndex = 5;
            this.label3.Text = "Control and Debug:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(414, 457);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(73, 20);
            this.label4.TabIndex = 6;
            this.label4.Text = "Baud rate";
            // 
            // ClearButton
            // 
            this.ClearButton.Location = new System.Drawing.Point(418, 415);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Size = new System.Drawing.Size(94, 29);
            this.ClearButton.TabIndex = 10;
            this.ClearButton.Text = "Clear";
            this.ClearButton.UseVisualStyleBackColor = true;
            this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // SendButton
            // 
            this.SendButton.Location = new System.Drawing.Point(566, 415);
            this.SendButton.Name = "SendButton";
            this.SendButton.Size = new System.Drawing.Size(94, 29);
            this.SendButton.TabIndex = 11;
            this.SendButton.Text = "Send";
            this.SendButton.UseVisualStyleBackColor = true;
            this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
            // 
            // BaudRateComboBox
            // 
            this.BaudRateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.BaudRateComboBox.FormattingEnabled = true;
            this.BaudRateComboBox.Items.AddRange(new object[] {
            "300",
            "600",
            "1200",
            "2400",
            "4800",
            "9600",
            "19200",
            "38400",
            "57600",
            "115200"});
            this.BaudRateComboBox.Location = new System.Drawing.Point(418, 485);
            this.BaudRateComboBox.Name = "BaudRateComboBox";
            this.BaudRateComboBox.Size = new System.Drawing.Size(151, 28);
            this.BaudRateComboBox.TabIndex = 12;
            this.BaudRateComboBox.SelectedIndexChanged += new System.EventHandler(this.BaudRateComboBox_SelectedIndexChanged);
            // 
            // ControlAndDebug
            // 
            this.ControlAndDebug.BackColor = System.Drawing.SystemColors.HighlightText;
            this.ControlAndDebug.ReadOnly = true;
            this.ControlAndDebug.Location = new System.Drawing.Point(26, 415);
            this.ControlAndDebug.Name = "ControlAndDebug";
            this.ControlAndDebug.Size = new System.Drawing.Size(379, 275);
            this.ControlAndDebug.TabIndex = 13;
            this.ControlAndDebug.Text = "";
            // 
            // ComForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(683, 716);
            this.Controls.Add(this.ControlAndDebug);
            this.Controls.Add(this.BaudRateComboBox);
            this.Controls.Add(this.SendButton);
            this.Controls.Add(this.ClearButton);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Output);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Input);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(701, 763);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(701, 763);
            this.Name = "ComForm";
            this.Text = "Serial Ports Communication";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox Input;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox Output;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button ClearButton;
        private System.Windows.Forms.Button SendButton;
        private System.Windows.Forms.ComboBox BaudRateComboBox;
        private System.Windows.Forms.RichTextBox ControlAndDebug;
    }
}


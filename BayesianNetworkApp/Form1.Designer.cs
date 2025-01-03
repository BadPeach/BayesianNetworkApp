namespace BayesianNetworkApp
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private Panel graphPanel;
        private ComboBox queryNodeComboBox;

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
            loadButton = new Button();
            setEvidenceButton = new Button();
            queryButton = new Button();
            graphPanel = new Panel();
            queryNodeComboBox = new ComboBox();
            SuspendLayout();
            // 
            // loadButton
            // 
            loadButton.Location = new Point(36, 389);
            loadButton.Name = "loadButton";
            loadButton.Size = new Size(148, 29);
            loadButton.TabIndex = 0;
            loadButton.Text = "Load network";
            loadButton.UseVisualStyleBackColor = true;
            loadButton.Click += LoadButton_Click;
            // 
            // setEvidenceButton
            // 
            setEvidenceButton.Location = new Point(288, 389);
            setEvidenceButton.Name = "setEvidenceButton";
            setEvidenceButton.Size = new Size(138, 29);
            setEvidenceButton.TabIndex = 1;
            setEvidenceButton.Text = "Set evidence";
            setEvidenceButton.UseVisualStyleBackColor = true;
            setEvidenceButton.Click += SetEvidenceButton_Click;
            // 
            // queryButton
            // 
            queryButton.Location = new Point(498, 389);
            queryButton.Name = "queryButton";
            queryButton.Size = new Size(184, 29);
            queryButton.TabIndex = 2;
            queryButton.Text = "Query network";
            queryButton.UseVisualStyleBackColor = true;
            queryButton.Click += QueryButton_Click;
            // 
            // graphPanel
            // 
            graphPanel.BackColor = Color.White;
            graphPanel.Location = new Point(134, 50);
            graphPanel.Name = "graphPanel";
            graphPanel.Size = new Size(500, 300);
            graphPanel.TabIndex = 3;
            // 
            // queryNodeComboBox
            // 
            queryNodeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            queryNodeComboBox.Location = new Point(50, 20);
            queryNodeComboBox.Name = "queryNodeComboBox";
            queryNodeComboBox.Size = new Size(200, 28);
            queryNodeComboBox.TabIndex = 4;
            queryNodeComboBox.SelectedIndexChanged += queryNodeComboBox_SelectedIndexChanged;
            // 
            // Form1
            // 
            ClientSize = new Size(725, 495);
            Controls.Add(queryButton);
            Controls.Add(setEvidenceButton);
            Controls.Add(loadButton);
            Controls.Add(graphPanel);
            Controls.Add(queryNodeComboBox);
            Name = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private Button loadButton;
        private Button setEvidenceButton;
        private Button queryButton;
    }
}

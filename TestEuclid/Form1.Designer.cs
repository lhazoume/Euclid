namespace TestEuclid
{
    partial class Form1
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.testBtn = new System.Windows.Forms.Button();
            this.testLvw = new System.Windows.Forms.ListView();
            this.testChr = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.passedChr = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // testBtn
            // 
            this.testBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.testBtn.Location = new System.Drawing.Point(197, 226);
            this.testBtn.Name = "testBtn";
            this.testBtn.Size = new System.Drawing.Size(75, 23);
            this.testBtn.TabIndex = 0;
            this.testBtn.Text = "Test";
            this.testBtn.UseVisualStyleBackColor = true;
            this.testBtn.Click += new System.EventHandler(this.testBtn_Click);
            // 
            // testLvw
            // 
            this.testLvw.AllowColumnReorder = true;
            this.testLvw.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.testLvw.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.testChr,
            this.passedChr});
            this.testLvw.GridLines = true;
            this.testLvw.Location = new System.Drawing.Point(12, 12);
            this.testLvw.Name = "testLvw";
            this.testLvw.Size = new System.Drawing.Size(260, 208);
            this.testLvw.TabIndex = 1;
            this.testLvw.UseCompatibleStateImageBehavior = false;
            this.testLvw.View = System.Windows.Forms.View.Details;
            // 
            // testChr
            // 
            this.testChr.Text = "Test";
            // 
            // passedChr
            // 
            this.passedChr.Text = "Passed ?";
            this.passedChr.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.testLvw);
            this.Controls.Add(this.testBtn);
            this.Name = "Form1";
            this.Text = "Euclid test";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button testBtn;
        private System.Windows.Forms.ListView testLvw;
        private System.Windows.Forms.ColumnHeader testChr;
        private System.Windows.Forms.ColumnHeader passedChr;
    }
}


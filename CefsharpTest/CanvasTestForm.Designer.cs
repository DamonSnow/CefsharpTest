namespace CefsharpTest
{
    partial class CanvasTestForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CanvasTestForm));
            this.panelBody = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblNow = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblCopyrightYear = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.timerClearMemory = new System.Windows.Forms.Timer(this.components);
            this.panelTop = new System.Windows.Forms.Panel();
            this.timerInterval = new System.Windows.Forms.Timer(this.components);
            this.panelBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // panelBody
            // 
            this.panelBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBody.Location = new System.Drawing.Point(0, 32);
            this.panelBody.Name = "panelBody";
            this.panelBody.Size = new System.Drawing.Size(1264, 618);
            this.panelBody.TabIndex = 7;
            // 
            // panelBottom
            // 
            this.panelBottom.BackColor = System.Drawing.Color.DodgerBlue;
            this.panelBottom.Controls.Add(this.pictureBox1);
            this.panelBottom.Controls.Add(this.lblNow);
            this.panelBottom.Controls.Add(this.label3);
            this.panelBottom.Controls.Add(this.lblCopyrightYear);
            this.panelBottom.Controls.Add(this.label1);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 650);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(1264, 31);
            this.panelBottom.TabIndex = 5;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Right;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(1118, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(20, 31);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // lblNow
            // 
            this.lblNow.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblNow.Font = new System.Drawing.Font("微软雅黑", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblNow.ForeColor = System.Drawing.Color.White;
            this.lblNow.Location = new System.Drawing.Point(1138, 0);
            this.lblNow.Name = "lblNow";
            this.lblNow.Padding = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.lblNow.Size = new System.Drawing.Size(126, 31);
            this.lblNow.TabIndex = 3;
            this.lblNow.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Left;
            this.label3.Font = new System.Drawing.Font("微软雅黑", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(124, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(271, 31);
            this.label3.TabIndex = 2;
            this.label3.Text = "by Unicompound.com 版权所有";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCopyrightYear
            // 
            this.lblCopyrightYear.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblCopyrightYear.Font = new System.Drawing.Font("微软雅黑", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblCopyrightYear.ForeColor = System.Drawing.Color.White;
            this.lblCopyrightYear.Location = new System.Drawing.Point(72, 0);
            this.lblCopyrightYear.Name = "lblCopyrightYear";
            this.lblCopyrightYear.Size = new System.Drawing.Size(52, 31);
            this.lblCopyrightYear.TabIndex = 1;
            this.lblCopyrightYear.Text = "2022";
            this.lblCopyrightYear.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 31);
            this.label1.TabIndex = 0;
            this.label1.Text = "Copyright";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // timerClearMemory
            // 
            this.timerClearMemory.Enabled = true;
            this.timerClearMemory.Interval = 20000;
            this.timerClearMemory.Tick += new System.EventHandler(this.timerClearMemory_Tick);
            // 
            // panelTop
            // 
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1264, 32);
            this.panelTop.TabIndex = 6;
            this.panelTop.Visible = false;
            // 
            // timerInterval
            // 
            this.timerInterval.Enabled = true;
            this.timerInterval.Interval = 1000;
            // 
            // CanvasTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1264, 681);
            this.Controls.Add(this.panelBody);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelTop);
            this.Name = "CanvasTestForm";
            this.Text = "CanvasTestForm";
            this.Load += new System.EventHandler(this.CanvasTestForm_Load);
            this.panelBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelBody;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblNow;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblCopyrightYear;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer timerClearMemory;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Timer timerInterval;
    }
}
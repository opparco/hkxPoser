﻿namespace hkxPoser
{
    partial class Form2
    {
        /// <summary>
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナで生成されたコード

        /// <summary>
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.lbRot = new System.Windows.Forms.Label();
            this.lbTra = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.btnLocZ = new hkxPoser.NotSelectableButton();
            this.btnLocY = new hkxPoser.NotSelectableButton();
            this.btnLocX = new hkxPoser.NotSelectableButton();
            this.btnRotZ = new hkxPoser.NotSelectableButton();
            this.btnRotY = new hkxPoser.NotSelectableButton();
            this.btnRotX = new hkxPoser.NotSelectableButton();
            this.SuspendLayout();
            // 
            // lbRot
            // 
            this.lbRot.Location = new System.Drawing.Point(68, 9);
            this.lbRot.Name = "lbRot";
            this.lbRot.Size = new System.Drawing.Size(47, 12);
            this.lbRot.TabIndex = 0;
            this.lbRot.Text = "Rot";
            this.lbRot.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbTra
            // 
            this.lbTra.Location = new System.Drawing.Point(14, 9);
            this.lbTra.Name = "lbTra";
            this.lbTra.Size = new System.Drawing.Size(47, 12);
            this.lbTra.TabIndex = 4;
            this.lbTra.Text = "Loc";
            this.lbTra.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(135, 24);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(117, 21);
            this.textBox1.TabIndex = 8;
            this.textBox1.TabStop = false;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(135, 55);
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(117, 21);
            this.textBox2.TabIndex = 9;
            this.textBox2.TabStop = false;
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(135, 86);
            this.textBox3.Name = "textBox3";
            this.textBox3.ReadOnly = true;
            this.textBox3.Size = new System.Drawing.Size(117, 21);
            this.textBox3.TabIndex = 10;
            this.textBox3.TabStop = false;
            // 
            // btnLocZ
            // 
            this.btnLocZ.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.btnLocZ.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLocZ.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnLocZ.ForeColor = System.Drawing.Color.Blue;
            this.btnLocZ.Location = new System.Drawing.Point(14, 86);
            this.btnLocZ.Name = "btnLocZ";
            this.btnLocZ.Size = new System.Drawing.Size(47, 25);
            this.btnLocZ.TabIndex = 7;
            this.btnLocZ.Text = "Z";
            this.btnLocZ.UseVisualStyleBackColor = true;
            this.btnLocZ.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Control_MouseDown);
            this.btnLocZ.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Control_MouseMove);
            this.btnLocZ.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Control_MouseUp);
            // 
            // btnLocY
            // 
            this.btnLocY.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.btnLocY.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLocY.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnLocY.ForeColor = System.Drawing.Color.Green;
            this.btnLocY.Location = new System.Drawing.Point(14, 55);
            this.btnLocY.Name = "btnLocY";
            this.btnLocY.Size = new System.Drawing.Size(47, 25);
            this.btnLocY.TabIndex = 6;
            this.btnLocY.Text = "Y";
            this.btnLocY.UseVisualStyleBackColor = true;
            this.btnLocY.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Control_MouseDown);
            this.btnLocY.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Control_MouseMove);
            this.btnLocY.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Control_MouseUp);
            // 
            // btnLocX
            // 
            this.btnLocX.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.btnLocX.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLocX.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnLocX.ForeColor = System.Drawing.Color.Red;
            this.btnLocX.Location = new System.Drawing.Point(14, 24);
            this.btnLocX.Name = "btnLocX";
            this.btnLocX.Size = new System.Drawing.Size(47, 25);
            this.btnLocX.TabIndex = 5;
            this.btnLocX.Text = "X";
            this.btnLocX.UseVisualStyleBackColor = true;
            this.btnLocX.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Control_MouseDown);
            this.btnLocX.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Control_MouseMove);
            this.btnLocX.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Control_MouseUp);
            // 
            // btnRotZ
            // 
            this.btnRotZ.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.btnRotZ.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRotZ.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnRotZ.ForeColor = System.Drawing.Color.Blue;
            this.btnRotZ.Location = new System.Drawing.Point(68, 86);
            this.btnRotZ.Name = "btnRotZ";
            this.btnRotZ.Size = new System.Drawing.Size(47, 25);
            this.btnRotZ.TabIndex = 3;
            this.btnRotZ.Text = "Z";
            this.btnRotZ.UseVisualStyleBackColor = true;
            this.btnRotZ.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Control_MouseDown);
            this.btnRotZ.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Control_MouseMove);
            this.btnRotZ.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Control_MouseUp);
            // 
            // btnRotY
            // 
            this.btnRotY.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.btnRotY.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRotY.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnRotY.ForeColor = System.Drawing.Color.Green;
            this.btnRotY.Location = new System.Drawing.Point(68, 55);
            this.btnRotY.Name = "btnRotY";
            this.btnRotY.Size = new System.Drawing.Size(47, 25);
            this.btnRotY.TabIndex = 2;
            this.btnRotY.Text = "Y";
            this.btnRotY.UseVisualStyleBackColor = true;
            this.btnRotY.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Control_MouseDown);
            this.btnRotY.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Control_MouseMove);
            this.btnRotY.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Control_MouseUp);
            // 
            // btnRotX
            // 
            this.btnRotX.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.btnRotX.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRotX.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnRotX.ForeColor = System.Drawing.Color.Red;
            this.btnRotX.Location = new System.Drawing.Point(68, 24);
            this.btnRotX.Name = "btnRotX";
            this.btnRotX.Size = new System.Drawing.Size(47, 25);
            this.btnRotX.TabIndex = 1;
            this.btnRotX.Text = "X";
            this.btnRotX.UseVisualStyleBackColor = true;
            this.btnRotX.Click += new System.EventHandler(this.btnRotX_Click);
            this.btnRotX.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Control_MouseDown);
            this.btnRotX.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Control_MouseMove);
            this.btnRotX.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Control_MouseUp);
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(277, 134);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btnLocZ);
            this.Controls.Add(this.btnLocY);
            this.Controls.Add(this.btnLocX);
            this.Controls.Add(this.lbTra);
            this.Controls.Add(this.btnRotZ);
            this.Controls.Add(this.btnRotY);
            this.Controls.Add(this.btnRotX);
            this.Controls.Add(this.lbRot);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Form2";
            this.Text = "Transform";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form2_FormClosing);
            this.Load += new System.EventHandler(this.Form2_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form2_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Form2_KeyUp);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Control_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Control_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Control_MouseUp);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbRot;
        private NotSelectableButton btnRotX;
        private NotSelectableButton btnRotY;
        private NotSelectableButton btnRotZ;
        private NotSelectableButton btnLocZ;
        private NotSelectableButton btnLocY;
        private NotSelectableButton btnLocX;
        private System.Windows.Forms.Label lbTra;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
    }
}

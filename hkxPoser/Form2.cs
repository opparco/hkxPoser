using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Forms;

namespace hkxPoser
{
    public partial class Form2 : Form
    {
        internal Viewer viewer = null;
        
        public Form2()
        {
            InitializeComponent();
        }

        // マウスポイントしているスクリーン座標
        internal Point lastScreenPoint = Point.Empty;

        // This method handles the mouse down event for all the controls on the form.  
        // When a control has captured the mouse
        // the control's name will be output on label1.
        private void Control_MouseDown(System.Object sender,
            System.Windows.Forms.MouseEventArgs e)
        {
            this.ActiveControl = null;
            lastScreenPoint.X = e.X;
            lastScreenPoint.Y = e.Y;
            viewer.BeginBoneCommand();
            valueInvalidate();

        }

        /// <summary>
        /// 変形操作時に呼び出されるハンドラ
        /// </summary>
        public event EventHandler TransformationEvent;

        private void valueInvalidate()
        {
            hkaBone bone = viewer.getSelectedBone();
            if (bone != null)
            {
                Transform t = bone.local * bone.patch;
                this.textBox1.Text = t.translation.X + "|" + t.rotation.X;
                this.textBox2.Text = t.translation.Y + "|" + t.rotation.Y;
                this.textBox3.Text = t.translation.Z + "|" + t.rotation.Z;
            }
        }

        private void Control_MouseMove(object sender, MouseEventArgs e)
        {
            int dx = e.X - lastScreenPoint.X;
            int dy = e.Y - lastScreenPoint.Y;
            this.Focus();

            Control control = (Control)sender;
            if (viewer.HasBoneCommand())
            {
                if (control == btnLocX)
                {
                    if (e.Button == MouseButtons.Left)
                        viewer.TranslateAxis(dx, dy, new SharpDX.Vector3(1, 0, 0));
                    else if (e.Button == MouseButtons.Right)
                        viewer.TranslateRootAxis(dx, dy, new SharpDX.Vector3(1, 0, 0));
                }   
                else
                if (control == btnLocY)
                {
                    if (e.Button == MouseButtons.Left)
                        viewer.TranslateAxis(dx, dy, new SharpDX.Vector3(0, 1, 0));
                    else if (e.Button == MouseButtons.Right)
                        viewer.TranslateRootAxis(dx, dy, new SharpDX.Vector3(0, 1, 0));
                }
                else
                if (control == btnLocZ)
                {
                    if (e.Button == MouseButtons.Left)
                        viewer.TranslateAxis(dx, dy, new SharpDX.Vector3(0, 0, 1));
                    else if (e.Button == MouseButtons.Right)
                        viewer.TranslateRootAxis(dx, dy, new SharpDX.Vector3(0, 0, 1));
                }
                else
                if (control == btnRotX)
                {
                    if (e.Button == MouseButtons.Left)
                        viewer.RotateAxis(dx, dy, new SharpDX.Vector3(1, 0, 0));
                    else if (e.Button == MouseButtons.Right)
                        viewer.RotateCenterAxis(dx, dy, new SharpDX.Vector3(1, 0, 0));
                }
                else
                if (control == btnRotY)
                {
                    if (e.Button == MouseButtons.Left)
                        viewer.RotateAxis(dx, dy, new SharpDX.Vector3(0, 1, 0));
                    else if (e.Button == MouseButtons.Right)
                        viewer.RotateCenterAxis(dx, dy, new SharpDX.Vector3(0, 1, 0));
                }
                else
                if (control == btnRotZ && e.Button == MouseButtons.Left)
                    viewer.RotateAxis(dx, dy, new SharpDX.Vector3(0, 0, 1));
                else
                if (control == btnRotZ && e.Button == MouseButtons.Right)
                    viewer.RotateCenterAxis(dx, dy, new SharpDX.Vector3(0, 0, 1));
                if (TransformationEvent != null)
                    TransformationEvent(this, EventArgs.Empty);

                lastScreenPoint.X = e.X;
                lastScreenPoint.Y = e.Y;
                valueInvalidate();
            }
        }

        private void Control_MouseUp(object sender, MouseEventArgs e)
        {
            viewer.EndBoneCommand();
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        public void ReceiveKeyDown(object sender, KeyEventArgs e)
        {
            Form2_KeyDown(sender, e);
        }

        private void Form2_KeyDown(object sender, KeyEventArgs e)
        {
            viewer.BeginBoneCommand();
            //Console.WriteLine("keyin");
            viewer.processKey(e.KeyCode);
            if (TransformationEvent != null)
                TransformationEvent(this, EventArgs.Empty);
            valueInvalidate();


        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }


        private void btnRotX_Click(object sender, EventArgs e)
        {

        }

        private void Form2_KeyUp(object sender, KeyEventArgs e)
        {
            viewer.EndBoneCommand();
        }
    }
}

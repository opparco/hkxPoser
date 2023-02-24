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

        private void Form2_KeyDown(object sender, KeyEventArgs e)
        {
            //Console.WriteLine("keyin");
            switch (e.KeyCode)
            {
                case Keys.Space :
                    viewer.SelectBone("NPC COM [COM ]");
                    break;
                case Keys.D1 :
                    viewer.SelectBone("NPC Spine [Spn0]");
                    break;
                case Keys.D2 :
                    viewer.SelectBone("NPC Spine1 [Spn1]");
                    break;
                case Keys.D3 :
                    viewer.SelectBone("NPC Spine2 [Spn2]");
                    break;
                case Keys.Q:
                    viewer.RotateAxis(-10, 0, new SharpDX.Vector3(1, 0, 0));
                    break;
                case Keys.W:
                    viewer.RotateAxis(10, 0, new SharpDX.Vector3(1, 0, 0));
                    break;
                case Keys.A:
                    viewer.RotateAxis(-10, 0, new SharpDX.Vector3(0, 1, 0));
                    break;
                case Keys.S:
                    viewer.RotateAxis(10, 0, new SharpDX.Vector3(0, 1, 0));
                    break;
                case Keys.Z:
                    viewer.RotateAxis(-10, 0, new SharpDX.Vector3(0, 0, 1));
                    break;
                case Keys.X:
                    viewer.RotateAxis(10, 0, new SharpDX.Vector3(0, 0, 1));
                    break;
            }

                        
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }


        private void btnRotX_Click(object sender, EventArgs e)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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
            lastScreenPoint.X = e.X;
            lastScreenPoint.Y = e.Y;
            viewer.BeginBoneCommand();
        }

        /// <summary>
        /// 変形操作時に呼び出されるハンドラ
        /// </summary>
        public event EventHandler TransformationEvent;

        private void Control_MouseMove(object sender, MouseEventArgs e)
        {
            int dx = e.X - lastScreenPoint.X;
            int dy = e.Y - lastScreenPoint.Y;

            Control control = (Control)sender;
            if (viewer.HasBoneCommand())
            {
                if (control == btnLocX)
                    viewer.TranslateAxis(dx, dy, new SharpDX.Vector3(1, 0, 0));
                else
                if (control == btnLocY)
                    viewer.TranslateAxis(dx, dy, new SharpDX.Vector3(0, 1, 0));
                else
                if (control == btnLocZ)
                    viewer.TranslateAxis(dx, dy, new SharpDX.Vector3(0, 0, 1));
                else
                if (control == btnRotX)
                    viewer.RotateAxis(dx, dy, new SharpDX.Vector3(1, 0, 0));
                else
                if (control == btnRotY)
                    viewer.RotateAxis(dx, dy, new SharpDX.Vector3(0, 1, 0));
                else
                if (control == btnRotZ)
                    viewer.RotateAxis(dx, dy, new SharpDX.Vector3(0, 0, 1));

                if (TransformationEvent != null)
                    TransformationEvent(this, EventArgs.Empty);

                lastScreenPoint.X = e.X;
                lastScreenPoint.Y = e.Y;
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
    }
}

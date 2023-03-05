using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hkxPoser
{
    public partial class Form1 : Form
    {
        internal Viewer viewer = null;

        public Form1(Settings settings)
        {
            InitializeComponent();
            this.ClientSize = settings.ClientSize;
            viewer = new Viewer(settings);
            viewer.LoadAnimationEvent += delegate(object sender, EventArgs args)
            {
                trackBar1.Maximum = viewer.GetNumFrames()-1;
                trackBar1.Value = 0;
            };
            if (viewer.InitializeGraphics(this))
            {
                timer1.Enabled = true;
            }
            

            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            viewer.Update();
            viewer.Render();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewer.command_man.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewer.command_man.Redo();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "hkx files|*.hkx";
            dialog.FilterIndex = 0;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string source_file = dialog.FileName;
                viewer.LoadAnimation(source_file);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = "out.hkx";
            dialog.Filter = "hkx files|*.hkx";
            dialog.FilterIndex = 0;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string dest_file = dialog.FileName;
                viewer.SaveAnimation(dest_file);
            }
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            viewer.SetCurrentPose(trackBar1.Value);
            this.toolStripStatusLabel1.Text = "Frame:" + trackBar1.Value;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                foreach (string source_file in (string[])e.Data.GetData(DataFormats.FileDrop))
                    viewer.LoadAnimation(source_file);
            }
        }

        private void Form1_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Move;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {

        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewer.exportPose(trackBar1.Value);
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewer.importPose();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            viewer.BeginBoneCommand();
            //Console.WriteLine("keyin");
            viewer.processKey(e.KeyCode);
            this.ActiveControl = null;
        }

        private void Form1_MouseHover(object sender, EventArgs e)
        {
            
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            this.Focus();
        }

        private void applyPatcchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewer.ApplyPatchToAnimation();
            viewer.AssignAnimationPose(0);
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void setStartFrameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.StartFrameLabel.Text = "StartFrame:" + trackBar1.Value;
            viewer.SetStartFrame(trackBar1.Value);
            
        }

        private void toToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys key = keyData & ~(Keys.Shift | Keys.Control);

            switch (key)
            {
                case Keys.E: // Export
                    if ((keyData & Keys.Control) != 0)
                    {
                        viewer.exportPose(trackBar1.Value);
                        return true;
                    }
                    break;
                case Keys.F: // 대화하기
                    if ((keyData & Keys.Control) != 0)
                    {
                        viewer.SetStartFrame(trackBar1.Value);
                        this.StartFrameLabel.Text = "StartFrame:" + trackBar1.Value;
                        return true;
                    }
                    break;
                case Keys.G: // 대화하기
                    if ((keyData & Keys.Control) != 0)
                    {
                        viewer.makeToAnimationBezier(trackBar1.Value);
                        viewer.SetStartFrame(trackBar1.Value);
                        this.StartFrameLabel.Text = "StartFrame:" + trackBar1.Value;
                        return true;
                    }
                    break;
                case Keys.H: // 대화하기
                    if ((keyData & Keys.Control) != 0)
                    {
                        viewer.makeToAnimation(trackBar1.Value);
                        viewer.SetStartFrame(trackBar1.Value);
                        this.StartFrameLabel.Text = "StartFrame:" + trackBar1.Value;
                        return true;
                    }
                    break;
                case Keys.V: // 대화하기
                    if ((keyData & Keys.Control) != 0)
                    {
                        viewer.importPoseFrame(trackBar1.Value);
                        viewer.makeToAnimationBezier(trackBar1.Value);
                        viewer.SetStartFrame(trackBar1.Value);
                        this.StartFrameLabel.Text = "StartFrame:" + trackBar1.Value;
                        return true;
                    }
                    break;
                case Keys.F5:
                    //MessageBox.Show("f5");
                    return true;
                    //break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void importFrameCtrlVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewer.importPoseFrame(trackBar1.Value);
            viewer.makeToAnimation(trackBar1.Value);
            viewer.SetStartFrame(trackBar1.Value);
            this.StartFrameLabel.Text = "StartFrame:" + trackBar1.Value;
        }

        private void deleteAnimationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewer.deleteAnimationFrame(trackBar1.Value);
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            viewer.EndBoneCommand();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void linearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //viewer.makeToAnimation(trackBar1.Value);
            viewer.makeToAnimationBezier(trackBar1.Value);
            viewer.SetStartFrame(trackBar1.Value);
            this.StartFrameLabel.Text = "StartFrame:" + trackBar1.Value;
        }

        private void linearToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //viewer.makeToAnimation(trackBar1.Value);
            viewer.makeToAnimation(trackBar1.Value);
            viewer.SetStartFrame(trackBar1.Value);
            this.StartFrameLabel.Text = "StartFrame:" + trackBar1.Value;
        }

        private void exportAnimationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewer.exportAnimation(trackBar1.Value);
        }

        private void importAnimationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewer.importAnimation(trackBar1.Value);
 
        }
    }
}

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
    }
}

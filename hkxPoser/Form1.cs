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

        public Form1()
        {
            InitializeComponent();

            this.ClientSize = new Size(640, 640);
            viewer = new Viewer();
            if (viewer.InitializeGraphics(this))
                timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            viewer.Update();
            viewer.Render();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewer.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewer.Redo();
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
    }
}

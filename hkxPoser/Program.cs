using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hkxPoser
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (Form1 form1 = new Form1())
            using (Form2 form2 = new Form2())
            {
                form2.TopLevel = false;
                form2.Location = new System.Drawing.Point(0, 26);
                form1.Controls.Add(form2);
                form2.BringToFront();
                form2.viewer = form1.viewer;

                form1.Show();
                form2.Show();

                Application.Run(form1);
            }
        }
    }
}

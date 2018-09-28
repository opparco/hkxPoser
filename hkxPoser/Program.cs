using System;
using System.IO;
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

            Settings settings = Settings.Load(Path.Combine(Application.StartupPath, @"config.xml"));
            //settings.Dump();

            Form1 form1 = new Form1(settings);
            Form2 form2 = new Form2();

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

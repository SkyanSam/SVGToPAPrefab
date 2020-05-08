using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SVGToPrefab;
using System.Threading;
namespace UIWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string[] colors;
        Thread console;
        public MainWindow()
        {
            InitializeComponent();

            colors = new string[]
            {
                Color1.Text, Color2.Text, Color3.Text,
                Color4.Text, Color5.Text, Color6.Text,
                Color7.Text, Color8.Text, Color9.Text,
                Color10.Text
            };
            for (int i = 0; i < colors.Length; i++)
                colors[i] = Input.Colors.ids[i];

            PAPATHDATA.Text = Input.prefabPath.ToString();
            SVGPATHDATA.Text = Input.svgPath.ToString();
            PrefabNameDATA.Text = Input.prefabName.ToString();
            SizeMultiplierDATA.Text = Input.sizeMultiplier.ToString();
            SecondsToLastDATA.Text = Input.secondsToLast.ToString();
            PrefabTypeDATA.Text = "Misc1";

            
        }
        public void ConsoleLog()
        {
            //Thread.Sleep(TimeSpan.FromSeconds(0.5));
            System.Diagnostics.Debug.WriteLine("CONSOLE LOG()");
            Dispatcher.BeginInvoke(new Action(() =>
            {
                System.Diagnostics.Debug.WriteLine("DISPATCHER()");
                if (LineWriter.notRead())
                {
                    for (int i = 0; i < LineWriter.console.Count; i++)
                    {
                        string line = LineWriter.console[i];
                        LineBreak linebreak = new LineBreak();
                        CONSOLE.Text += "\n" + line; // Linebreak

                        
                        LineWriter.console.Remove(line); // Remove stuff
                    }
                }
            }));
        }
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            
            for (int i = 0; i < colors.Length; i++)
                Input.Colors.ids[i] = colors[i];

            Input.prefabPath = PAPATHDATA.Text;
            Input.svgPath = SVGPATHDATA.Text;
            Input.prefabName = PrefabNameDATA.Text;
            Input.prefabType = Input.stringToPrefabType[PrefabTypeDATA.Text];
            Input.sizeMultiplier = float.Parse(SizeMultiplierDATA.Text);
            Input.secondsToLast = float.Parse(SecondsToLastDATA.Text);

            Program.Process();
            console = new Thread(ConsoleLog);
            console.Start();
        }
    }
}

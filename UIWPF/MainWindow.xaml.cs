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
using System.IO;
namespace UIWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Thread console;
        public MainWindow()
        {
            InitializeComponent();

            for (int i = 0; i < 10; i++)
                SetColor(i, Input.Colors.ids[i]);

            PAPATHDATA.Text = Input.prefabPath.ToString();
            SVGPATHDATA.Text = Input.svgPath.ToString();
            PrefabNameDATA.Text = Input.prefabName.ToString();
            SizeMultiplierDATA.Text = Input.sizeMultiplier.ToString();
            SecondsToLastDATA.Text = Input.secondsToLast.ToString();
            PrefabTypeDATA.Text = "Misc1";

            

            
        }
        void SetColor(int i, string v)
        {
            switch (i)
            {
                case 0: Color1.Text = v; break;
                case 1: Color2.Text = v; break;
                case 2: Color3.Text = v; break;
                case 3: Color4.Text = v; break;
                case 4: Color5.Text = v; break;
                case 5: Color6.Text = v; break;
                case 6: Color7.Text = v; break;
                case 7: Color8.Text = v; break;
                case 8: Color9.Text = v; break;
                case 9: Color10.Text = v; break;
            }
        }
        string GetColor(int i)
        {
            switch (i)
            {
                case 0: return Color1.Text;
                case 1: return Color2.Text;
                case 2: return Color3.Text;
                case 3: return Color4.Text;
                case 4: return Color5.Text;
                case 5: return Color6.Text;
                case 6: return Color7.Text;
                case 7: return Color8.Text;
                case 8: return Color9.Text;
                case 9: return Color10.Text;
                default: return "";
            }
        }
        public void ConsoleLog()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
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
            
            for (int i = 0; i < 10; i++)
                Input.Colors.ids[i] = GetColor(i);

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

        private void SVGPATHDATA_TextChanged(object sender, TextChangedEventArgs e)
        {
            /*if (File.Exists(SVGPATHDATA.Text))
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(SVGPATHDATA.Text);
                image.EndInit();
                CanvasDisplayImage.Source = image;
            }*/
        }

        private void CONSOLE_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}

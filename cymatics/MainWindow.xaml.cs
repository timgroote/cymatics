using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using SharpGL;
using SharpGL.SceneGraph;

namespace cymatics
{

    public enum HighlightingType
    {
        Tim,
        ShaderBuilder
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private OpenGL _gl;
        CymaticsScene _scene = new CymaticsScene();

        private System.Windows.Threading.DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            LoadSyntaxHighlighting(HighlightingType.Tim);
            Editor.TextChanged += EditorOnTextChanged;
            Editor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
            _scene.Compilation += _scene_Compilation;
        }

        private void _scene_Compilation(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(_scene.CompilationFailureText))
            {
                ErrorTextblock.Text = "";
                ErrorTextblock.MaxHeight = 0;
            }
            else
            {
                ErrorTextblock.Text = _scene.CompilationFailureText;
                ErrorTextblock.MaxHeight = 120;
            }
        }

        private void Caret_PositionChanged(object sender, EventArgs e)
        {
            RowIndicator.Text = Editor.TextArea.Caret.Line.ToString();
            ColIndicator.Text = Editor.TextArea.Caret.Column.ToString();
        }

        private void EditorOnTextChanged(object sender, EventArgs eventArgs)
        {
            //todo : ew.
            _scene.FragmentShaderSource = Editor.Text;

            if (timer == null)
            {
                timer = new DispatcherTimer();
                Closing += (q, args) =>
                {
                    timer?.Stop();
                };
                timer.Tick += (s, args) =>
                {
                    Dispatcher.Invoke(() => { _scene.Initialise(_gl); });
                    timer.Stop();
                };
            }
            else
            {
                timer.Stop();
            }
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Start();
        }

        public void LoadSyntaxHighlighting(HighlightingType highlighting)
        {
            Stream xshdStream;
            switch (highlighting)
            {
                case HighlightingType.ShaderBuilder:
                    xshdStream = File.OpenRead("SBSyntaxHighlighting.xshd");
                    break;
                case HighlightingType.Tim:
                    xshdStream = File.OpenRead("SyntaxHighlighting.xshd");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(highlighting));
            }

            
            XmlTextReader xshdReader = new XmlTextReader(xshdStream);
            Editor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(xshdReader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
            xshdReader.Close();
            xshdStream.Close();
        }

        private void new_Item_Click(object sender, RoutedEventArgs e)
        {
            Editor.Text = "";
        }

        private void open_Item_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog(){DefaultExt = ".frag", CheckFileExists = true, CheckPathExists = true};
            if (openFileDialog.ShowDialog() == true)
                Editor.Text = File.ReadAllText(openFileDialog.FileName);
        }

        private void save_Item_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog(){DefaultExt = ".frag", CheckFileExists=false,AddExtension = true};
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, Editor.Text);
            }
        }

        private void exit_Item_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OpenglControl_OnOpenGLDraw(object sender, OpenGLEventArgs args)
        {
            _scene.Draw(_gl, (float)OpenglControl.ActualWidth, (float)OpenglControl.ActualHeight);
            FPSIndicator.Text = OpenglControl.FrameRate.ToString();
        }

        private void OpenglControl_OnOpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            _gl = args.OpenGL;
            try
            {
                _scene.Initialise(_gl);
            }
            catch(Exception ex)
            {
                Debug.WriteLine("initialise fail : " + ex.Message);
            }
            
            // init channel 0(image)
            //Ch0Image = new BitmapImage(new Uri(@"C:/sta.jpg"));
            //CHO_ImageBox.Source = Ch0Image;
            //scn.UpdateTextureBitmap(gl, 1, ImageUtilities.BitmapImage2Bitmap(Ch0Image));
            //scn.UpdateTextureBitmap(gl, 1, ab.lastBitmap);

            //texture1.Create(gl, ImageUtilities.BitmapImage2Bitmap(Ch0Image));
            //texture1.TextureName = "iChannel1";

            //init channel 1 (audio)
            //texture2.Create(gl);
        }

//        public void OnFftCalculated(object sender, FftEventArgs e)
//        {
//            NAudio.Dsp.Complex[] result = e.Result;
//            this.Dispatcher.Invoke(new Action(() => {
//                SpecAnalyser.Update(result);
//                ab.Update(OpenglControl.OpenGL, result);
//            }));
//        }
        private void Tim_item_OnClick(object sender, RoutedEventArgs e)
        {
            LoadSyntaxHighlighting(HighlightingType.Tim);
        }

        private void Shaderbuilder_item_OnClick(object sender, RoutedEventArgs e)
        {
            LoadSyntaxHighlighting(HighlightingType.ShaderBuilder);
        }
    }
}

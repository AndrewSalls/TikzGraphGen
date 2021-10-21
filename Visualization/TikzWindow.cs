using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace TikzGraphGen.Visualization
{
    public class TikzWindow : Form
    {
        public static readonly string PROGRAM_NAME = "Tikz Graph Tool";
        public static readonly Rectangle DEFAULT_BOUNDS = new(0, 0, 800, 600);
        public static readonly Size MIN_SIZE = new(400, 400);
        public static readonly Color BG_COLOR = Color.FromArgb(232, 232, 232);

        public static readonly string NO_SAVE_QUIT_DIALOG = "Quit";
        public static readonly string SAVE_QUIT_DIALOG = "Save and Quit";
        public static readonly string CANCEL_QUIT_DIALOG = "Cancel";

        private readonly TikzDrawingWindow _editor;
        private readonly TikzCompiler _compiler;
        private readonly GraphMenuStrip _menubar;
        private readonly GraphToolStrip _toolbar;

        private readonly RoutedShortcutCommand _rsc;

        [STAThread]
        public static void Main(string[] _)
        {
            TikzWindow program = new();
            Application.EnableVisualStyles();
            Application.Run(program);
        }

        public TikzWindow() : base()
        {
            _rsc = new();
            LinkCommands();
            _menubar = new(_rsc);
            _toolbar = new(_rsc)
            {
                Top = _menubar.Bottom,
                Height = ClientRectangle.Height - _menubar.Height
            };

            _compiler = new();
            _editor = new TikzDrawingWindow(this, _rsc)
            {
                Bounds = new(ClientRectangle.X + _toolbar.Width, ClientRectangle.Y + _menubar.Height, ClientRectangle.Width - _toolbar.Width, ClientRectangle.Height - _menubar.Height)
            };

            InitializeWindowSettings();

            Controls.Add(_editor);
            Controls.Add(_menubar);
            Controls.Add(_toolbar);

            _rsc.CurrentTool = TikzDrawingWindow.SelectedTool.Vertex; //Should update all related menus

            BringToFront();
            Show();
            _editor.Show();
            Focus();
        }

        private void InitializeWindowSettings()
        {
            BackColor = BG_COLOR;
            ForeColor = Color.FromArgb(23, 23, 23);
            Font = new(FontFamily.GenericSansSerif, 14, FontStyle.Regular, GraphicsUnit.Point);
            Bounds = DEFAULT_BOUNDS;
            WindowState = FormWindowState.Maximized;
            Icon = Properties.Resources.TikzGenIcon;
            MinimumSize = MIN_SIZE;
            DoubleBuffered = true;
            IsMdiContainer = true;
            Text = PROGRAM_NAME;
            ShowInTaskbar = true;

            FormClosing += TikzWindow_FormClosing;
            Resize += TikzWindow_Resize;
        }

        private void TikzWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_editor.HasUnsavedChanges() || true)
                e.Cancel = SaveOrContinue();
        }

        private void TikzWindow_Resize(object sender, EventArgs e) //TODO: Account for future sidebars
        {

            _editor.Bounds = new(ClientRectangle.X + _toolbar.Width, ClientRectangle.Y + _menubar.Height, ClientRectangle.Width - _toolbar.Width, ClientRectangle.Height - _menubar.Height);
            _toolbar.Top = _menubar.Bottom;
            _toolbar.Height = ClientRectangle.Height - _menubar.Height;
        }

        private bool SaveOrContinue()
        {
            string result = RoutedShortcutCommand.SavePrompt();
            if (result.Equals(CANCEL_QUIT_DIALOG))
                return true;
            else if (result.Equals(SAVE_QUIT_DIALOG))
                _compiler.Save(_editor.GetData(), false);

            return false;
        }

        private void LinkCommands()
        {
            _rsc.NewProject = () =>
            {
                if (!_editor.HasUnsavedChanges() || SaveOrContinue())
                {
                    SaveFileDialog fd = new()
                    {
                        Filter = TikzCompiler.FILE_SAVE_EXTENSION,
                        FileName = "Untitled",
                        DefaultExt = TikzCompiler.FILE_SAVE_EXTENSION,
                        Title = "Create File:",
                        InitialDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                        AddExtension = true,
                        CheckPathExists = true,
                        OverwritePrompt = true,
                        RestoreDirectory = true,
                        ValidateNames = true
                    };
                    if (fd.ShowDialog().Equals(DialogResult.OK))
                        _editor.NewGraph();
                }
            };
            _rsc.Open = () => {
                if (!_editor.HasUnsavedChanges() || SaveOrContinue())
                {
                    OpenFileDialog fd = new()
                    {
                        Filter = TikzCompiler.FILE_SAVE_EXTENSION,
                        Title = "Open File:",
                        InitialDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                        AddExtension = true,
                        CheckPathExists = true,
                        RestoreDirectory = true,
                        ValidateNames = true
                    };
                    if(fd.ShowDialog().Equals(DialogResult.OK))
                        _editor.NewGraph(_compiler.ConvertFromFile(fd.FileName));
                }
            };
            _rsc.Save = () => _compiler.Save(_editor.GetData(), false);
            _rsc.SaveAs = () => _compiler.Save(_editor.GetData(), true);
            _rsc.Export = () => { _compiler.Save(_editor.GetData(), false); _compiler.ConvertToPDF(_editor.GetData()); };
            _rsc.DisplayInfo = OpenInfoWindow;
            _rsc.Quit = Close;

            _rsc.DisplayHistory = OpenHistoryWindow;
            _rsc.ResizeAll = OpenResizeDialog;
            _rsc.SetUnits = OpenUnitDialog;
            _rsc.FontOptions = OpenFontWindow;
            _rsc.Preferences = OpenPreferencesWindow;
            _rsc.ToggleFullscreen = () => { if (WindowState.Equals(FormWindowState.Normal)) WindowState = FormWindowState.Maximized; else WindowState = FormWindowState.Normal; };
            _rsc.ToggleMenu = () => _menubar.Visible = !_menubar.Visible;
            _rsc.ToggleTools = () => _toolbar.Visible = !_toolbar.Visible;

            _rsc.Guide = OpenHelpWindow;
            _rsc.About = OpenAboutWindow;
        }

        private void OpenInfoWindow()
        {
            throw new NotImplementedException();
        }
        private void OpenHistoryWindow()
        {
            throw new NotImplementedException();
        }
        private void OpenResizeDialog()
        {
            throw new NotImplementedException();
        }
        private void OpenUnitDialog()
        {
            throw new NotImplementedException();
        }
        private void OpenFontWindow()
        {
            throw new NotImplementedException();
        }
        private void OpenPreferencesWindow()
        {
            throw new NotImplementedException();
        }

        private void OpenHelpWindow()
        {
            throw new NotImplementedException();
        }
        private void OpenAboutWindow()
        {
            throw new NotImplementedException();
        }
    }
}
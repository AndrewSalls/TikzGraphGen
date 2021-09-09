using System;
using System.Drawing;
using System.Windows.Forms;

namespace TikzGraphGen.Visualization
{
    public class PDFDisplayWindow : Form
    {
        public static readonly string NO_PDF_TEXT = "No PDF file found. \nSave and compile a file to a \n.dvi, .ps, or .pdf file to display here.";

        private readonly WebBrowser _content;
        private readonly TikzCompiler _compiler;

        public PDFDisplayWindow(Form parent, TikzCompiler compiler) : base()
        {
            _content = new WebBrowser();
            _compiler = compiler;

            Owner = parent;
            TopLevel = false;
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.None;

            BackColor = ControlPaint.Dark(Color.SlateGray);

            TryLoadPDF();
        }

        public bool TryLoadPDF()
        {
            if (_compiler.HasFileName())
            {
                Controls.Add(_content);
                _content.Url = new(_compiler.GetFilePath());
                _content.Dock = DockStyle.Fill;
                return true;
            }

            Label noPDF = new()
            {
                Text = NO_PDF_TEXT,
                ForeColor = Color.AntiqueWhite,
                Font = new(FontFamily.GenericSansSerif, 14, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            Controls.Add(noPDF);
            return false;
        }
    }
}
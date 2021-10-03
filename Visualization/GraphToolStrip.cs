using System;
using System.Drawing;
using System.Windows.Forms;
using TikzGraphGen.Properties;
using static TikzGraphGen.Visualization.TikzDrawingWindow;

namespace TikzGraphGen.Visualization
{
    public class GraphToolStrip : FlowLayoutPanel
    {
        public static readonly int NUMBER_TOOLBAR_ITEMS = 11;

        private readonly RoutedShortcutCommand _rsc;

        public GraphToolStrip(RoutedShortcutCommand rsc) : base()
        {
            _rsc = rsc;
            FlowDirection = FlowDirection.TopDown;
            Width = 48;

            Initialize();
        }

        public void Initialize()
        {
            Button[] items = new Button[NUMBER_TOOLBAR_ITEMS];

            items[0] = GenerateButton("Vertex", Resources.VertexIcon, (o, e) => _rsc.CurrentTool = SelectedTool.Vertex);
            items[1] = GenerateButton("Edge", Resources.EdgeIcon, (o, e) => _rsc.CurrentTool = SelectedTool.Edge);
            items[2] = GenerateButton("Edge Cap", Resources.EdgeCapIcon, (o, e) => _rsc.CurrentTool = SelectedTool.EdgeCap);
            items[3] = GenerateButton("Label", Resources.LabelIcon, (o, e) => _rsc.CurrentTool = SelectedTool.Label);
            items[4] = GenerateButton("Eraser", Resources.EraserIcon, (o, e) => _rsc.CurrentTool = SelectedTool.Eraser);
            items[5] = GenerateButton("Transform", Resources.TransformIcon, (o, e) => _rsc.CurrentTool = SelectedTool.Transform);
            items[6] = GenerateButton("Select", Resources.SelectIcon, (o, e) => _rsc.CurrentTool = SelectedTool.Select);
            items[7] = GenerateButton("Weight", Resources.WeightIcon, (o, e) => _rsc.CurrentTool = SelectedTool.Weight);
            items[8] = GenerateButton("Tracker", Resources.TrackerIcon, (o, e) => _rsc.CurrentTool = SelectedTool.Tracker);
            items[9] = GenerateButton("Merge", Resources.MergeIcon, (o, e) => _rsc.CurrentTool = SelectedTool.Merge);
            items[10] = GenerateButton("Split", Resources.SplitIcon, (o, e) => _rsc.CurrentTool = SelectedTool.Split);

            foreach(Button item in items)
            {
                item.Size = new Size(48, 48);
                item.FlatStyle = FlatStyle.Flat;
                item.FlatAppearance.BorderSize = 0;
                item.Margin = Padding.Empty;
                item.Padding = Padding.Empty;

                item.EnabledChanged += (o, e) =>
                {
                    if (item.Enabled)
                        item.BackColor = SystemColors.ButtonFace;
                    else
                        item.BackColor = Color.FromArgb(210, 210, 210);
                };
            }

            Controls.AddRange(items);
            
            _rsc.CurrentToolChanged += (SelectedTool cur) =>
            {
                foreach(Control sub in Controls)
                {
                    sub.Enabled = true;
                    sub.BackColor = SystemColors.ButtonFace;
                }

                switch(cur)
                {
                    case SelectedTool.Vertex:
                        items[0].Enabled = false;
                        break;
                    case SelectedTool.Edge:
                        items[1].Enabled = false;
                        break;
                    case SelectedTool.EdgeCap:
                        items[2].Enabled = false;
                        break;
                    case SelectedTool.Label:
                        items[3].Enabled = false;
                        break;
                    case SelectedTool.Eraser:
                        items[4].Enabled = false;
                        break;
                    case SelectedTool.Transform:
                        items[5].Enabled = false;
                        break;
                    case SelectedTool.Select:
                    case SelectedTool.AreaSelect:
                    case SelectedTool.Lasso:
                        items[6].Enabled = false;
                        break;
                    case SelectedTool.Weight:
                        items[7].Enabled = false;
                        break;
                    case SelectedTool.Tracker:
                        items[8].Enabled = false;
                        break;
                    case SelectedTool.Merge:
                        items[9].Enabled = false;
                        break;
                    case SelectedTool.Split:
                        items[10].Enabled = false;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            };
        }

        private static Button GenerateButton(string name, Bitmap icon, EventHandler click)
        {
            Button output = new()
            {
                Name = name,
                BackgroundImage = icon,
            };
            output.Click += click;

            return output;
        }
    }
}

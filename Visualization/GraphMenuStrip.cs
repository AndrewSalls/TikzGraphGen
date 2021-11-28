using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static TikzGraphGen.Visualization.TikzDrawingWindow;

namespace TikzGraphGen.Visualization
{
    public class GraphMenuStrip : MenuStrip
    {
        public static readonly int SUBMENU_WIDTH_PX = 100;

        private readonly RoutedShortcutCommand _rsc;

        public GraphMenuStrip(RoutedShortcutCommand rsc) : base()
        {
            _rsc = rsc;

            CanOverflow = true;
            Stretch = true;
            Anchor = AnchorStyles.Top;
            Dock = DockStyle.Top;

            InitializeMenuStrip();
            foreach(ToolStripMenuItem i in Items)
            {
                SetSubmenuWidth(i, SUBMENU_WIDTH_PX);
            }
        }

        private void InitializeMenuStrip()
        {
            Items.Add(InitializeFileOptions());
            Items.Add(InitializeEditOptions());
            Items.Add(InitializeViewOptions());
            Items.Add(InitializeToolOptions());
            Items.Add(InitializeAnalyzeOptions());
            Items.Add(InitializeHelpOptions());
        }

        private ToolStripMenuItem InitializeFileOptions()
        {
            ToolStripMenuItem file = new("&File");
            file.DropDownItems.Add(GenerateMenuItem("New Project", Keys.Control | Keys.Shift | Keys.N, "Ctrl Shift N", (o, e) => _rsc.NewProject()));
            file.DropDownItems.Add(GenerateMenuItem("Open", Keys.Control | Keys.O, "Ctrl O", (o, e) => _rsc.Open()));
            file.DropDownItems.Add(new ToolStripMenuItem("Open Recent"));
            file.DropDownItems.Add(GenerateMenuItem("Save", Keys.Control | Keys.S, "Ctrl S", (o, e) => _rsc.Save()));
            file.DropDownItems.Add(GenerateMenuItem("Save As", Keys.Control | Keys.Shift | Keys.S, "Ctrl Shift S", (o, e) => _rsc.SaveAs()));
            file.DropDownItems.Add(GenerateMenuItem("Export", Keys.Control | Keys.Shift | Keys.E, "Ctrl Shift E", (o, e) => _rsc.Export()));
            file.DropDownItems.Add(new ToolStripSeparator());
            //Get number of vertices, edges, directed edges, faces, etc.
            file.DropDownItems.Add(GenerateMenuItem("Graph Info", Keys.Control | Keys.I, "Ctrl I", (o, e) => _rsc.DisplayInfo()));
            file.DropDownItems.Add(new ToolStripSeparator());
            file.DropDownItems.Add(GenerateMenuItem("Quit", Keys.Control | Keys.Alt | Keys.Q, "Ctrl Alt Q", (o, e) => _rsc.Quit()));

            return file;
        }

        private ToolStripMenuItem InitializeEditOptions()
        {
            ToolStripMenuItem edit = new("&Edit");

            edit.DropDownItems.Add(GenerateMenuItem("Delete", Keys.Delete, "Delete", (o, e) => _rsc.DeleteSelected()));

            ToolStripMenuItem init = GenerateMenuItem("Undo", Keys.Control | Keys.Z, "Ctrl Z", (o, e) => _rsc.Undo());
            _rsc.CanUndoStatusChanged += (b) => init.Enabled = b;
            init.Enabled = false;
            edit.DropDownItems.Add(init);

            init = GenerateMenuItem("Redo", Keys.Control | Keys.Y, "Ctrl Y", (o, e) => _rsc.Redo());
            _rsc.CanRedoStatusChanged += (b) => init.Enabled = b;
            init.Enabled = false;
            edit.DropDownItems.Add(init);

            init = new("View History");
            init.Click += (o, e) => _rsc.DisplayHistory();
            edit.DropDownItems.Add(init);

            edit.DropDownItems.Add(new ToolStripSeparator());
            edit.DropDownItems.Add(GenerateMenuItem("Cut", Keys.Control | Keys.X, "Ctrl X", (o, e) => _rsc.Cut()));
            edit.DropDownItems.Add(GenerateMenuItem("Copy", Keys.Control | Keys.C, "Ctrl C", (o, e) => _rsc.Copy()));
            edit.DropDownItems.Add(GenerateMenuItem("Paste", Keys.Control | Keys.V, "Ctrl V", (o, e) => _rsc.Paste()));
            edit.DropDownItems.Add(new ToolStripSeparator());

            init = new("Resize");
            init.Click += (o, e) => _rsc.ResizeAll();
            edit.DropDownItems.Add(init);

            edit.DropDownItems.Add(GenerateMenuItem("Set Units", Keys.Control | Keys.Alt | Keys.U, "Ctrl Alt U", (o, e) => _rsc.SetUnits()));
            //(include apply to all option)
            edit.DropDownItems.Add(GenerateMenuItem("Font Options", Keys.Control | Keys.Shift | Keys.F, "Ctrl Shift F", (o, e) => _rsc.FontOptions()));
            edit.DropDownItems.Add(GenerateMenuItem("Preferences", Keys.Control | Keys.Alt | Keys.P, "Ctrl Alt P", (o, e) => _rsc.Preferences()));

            return edit;
        }

        private ToolStripMenuItem InitializeViewOptions()
        {
            ToolStripMenuItem view = new("&View");

            view.DropDownItems.Add(GenerateMenuItem("Zoom In", Keys.Control | Keys.Oemplus, "Ctrl +", (o, e) => _rsc.ZoomInc()));
            view.DropDownItems.Add(GenerateMenuItem("Zoom Out", Keys.Control | Keys.OemMinus, "Ctrl -", (o, e) => _rsc.ZoomDec()));

            ToolStripMenuItem zoom = new("Zoom Ratio");
            zoom.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("10%"),
                new ToolStripMenuItem("25%"),
                new ToolStripMenuItem("50%"),
                new ToolStripMenuItem("75%"),
                new ToolStripMenuItem("100%"),
                new ToolStripMenuItem("125%"),
                new ToolStripMenuItem("150%"),
                new ToolStripMenuItem("200%")
            });
            ((ToolStripMenuItem)zoom.DropDownItems[4]).Checked = true;
            for (int i = 0; i < zoom.DropDownItems.Count; i++)
            {
                int temp = i;
                zoom.DropDownItems[temp].Click += (o, e) =>
                {
                    for (int j = 0; j < zoom.DropDownItems.Count; j++)
                        ((ToolStripMenuItem)zoom.DropDownItems[j]).Checked = false;

                    ((ToolStripMenuItem)zoom.DropDownItems[temp]).Checked = true;
                    _rsc.SetZoomPercentage(new[] { 0.1f, 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 2f}[temp]);
                };
            }
            view.DropDownItems.Add(zoom);

            //Make entire graph fit on screen (if possible)
            view.DropDownItems.Add(GenerateMenuItem("Zoom To Fit", Keys.Control | Keys.Shift | Keys.O, "Ctrl Shift O", (o, e) => _rsc.ZoomFit()));
            view.DropDownItems.Add(new ToolStripSeparator());
            view.DropDownItems.Add(GenerateMenuItem("Fullscreen", Keys.F11, "F11", (o, e) => _rsc.ToggleFullscreen()));
            //(snap to every 15%, toggle)
            view.DropDownItems.Add(GenerateMenuItem("Angle Snap", Keys.Control | Keys.Alt | Keys.A, "Ctrl Alt A", (o, e) => _rsc.ToggleAngleSnap(), true, true));
            //(on by default, attempts to make new lines/vertices along increments (i.e. every new vertex is 10mm apart))
            view.DropDownItems.Add(GenerateMenuItem("Snap To Unit", Keys.Control | Keys.Shift | Keys.I, "Ctrl Shift I", (o, e) => _rsc.ToggleUnitSnap(), false, true));
            view.DropDownItems.Add(GenerateMenuItem("Snap To Unit Grid", Keys.Control | Keys.U, "Ctrl U", (o, e) => _rsc.ToggleGridUnitSnap(), false, true));
            view.DropDownItems.Add(new ToolStripSeparator());
            //(create line showing page border + margin dimensions for PDF page)
            view.DropDownItems.Add(GenerateMenuItem("Show Page Border", Keys.Control | Keys.Alt | Keys.B, "Ctrl Alt B", (o, e) => _rsc.ToggleBorder(), false, true));
            view.DropDownItems.Add(GenerateMenuItem("Show Unit Grid", Keys.Control | Keys.Shift | Keys.U, "Ctrl Shift U", (o, e) => _rsc.ToggleUnitGrid(), false, true));
            view.DropDownItems.Add(GenerateMenuItem("Show Menubar", Keys.Control | Keys.Shift | Keys.M, "Ctrl Shift M", (o, e) => _rsc.ToggleMenu(), true, true));
            view.DropDownItems.Add(GenerateMenuItem("Show Toolbar", Keys.Control | Keys.Shift | Keys.T, "Ctrl Shift T", (o, e) => _rsc.ToggleTools(), true, true));
            return view;
        }

        private ToolStripMenuItem InitializeToolOptions()
        {
            ToolStripMenuItem tool = new("&Tools");
            ToolStripMenuItem vertex = GenerateMenuItem("Vertex", Keys.Control | Keys.J, "Ctrl J", (o, e) => _rsc.CurrentTool = SelectedTool.Vertex);
            _rsc.CurrentToolChanged += (s) => vertex.Checked = s.Equals(SelectedTool.Vertex);
            tool.DropDownItems.Add(vertex);

            ToolStripMenuItem edge = GenerateMenuItem("Edge", Keys.Control | Keys.G, "Ctrl G", (o, e) => _rsc.CurrentTool = SelectedTool.Edge);
            _rsc.CurrentToolChanged += (s) => edge.Checked = s.Equals(SelectedTool.Edge);
            tool.DropDownItems.Add(edge);

            ToolStripMenuItem edgeCap = GenerateMenuItem("Edge Cap", Keys.Control | Keys.H, "Ctrl H", (o, e) => _rsc.CurrentTool = SelectedTool.EdgeCap);
            _rsc.CurrentToolChanged += (s) => edgeCap.Checked = s.Equals(SelectedTool.EdgeCap);
            tool.DropDownItems.Add(edgeCap);

            tool.DropDownItems.Add(new ToolStripSeparator());

            ToolStripMenuItem label = GenerateMenuItem("Label", Keys.Control | Keys.L, "Ctrl L", (o, e) => _rsc.CurrentTool = SelectedTool.Label);
            _rsc.CurrentToolChanged += (s) => label.Checked = s.Equals(SelectedTool.Label);
            tool.DropDownItems.Add(label);

            tool.DropDownItems.Add(GenerateMenuItem("Label Snap", Keys.Control | Keys.Shift | Keys.L, "Ctrl Shift L", (o, e) => _rsc.ToggleLabelEdgeSnap(), true, true));
            tool.DropDownItems.Add(new ToolStripSeparator());

            ToolStripMenuItem eraser = GenerateMenuItem("Eraser", Keys.Control | Keys.B, "Ctrl B", (o, e) => _rsc.CurrentTool = SelectedTool.Eraser);
            _rsc.CurrentToolChanged += (s) => eraser.Checked = s.Equals(SelectedTool.Eraser);
            tool.DropDownItems.Add(eraser);

            //TODO: Expand to be several menuitems with each transform subtool (each with a shortcut too)
            ToolStripMenuItem transform = GenerateMenuItem("Transform", Keys.Control | Keys.N, "Ctrl N", (o, e) => _rsc.CurrentTool = SelectedTool.Transform);
            _rsc.CurrentToolChanged += (s) => transform.Checked = s.Equals(SelectedTool.Transform);
            tool.DropDownItems.Add(transform);

            ToolStripMenuItem shape = GenerateMenuItem("Shape", Keys.Control | Keys.P, "Ctrl P", (o, e) => _rsc.CurrentTool = SelectedTool.Shape);
            _rsc.CurrentToolChanged += (s) => shape.Checked = s.Equals(SelectedTool.Shape);
            tool.DropDownItems.Add(shape);

            tool.DropDownItems.Add(new ToolStripMenuItem("Shape Options"));
            tool.DropDownItems.Add(new ToolStripSeparator());

            ToolStripMenuItem select = GenerateMenuItem("Select", Keys.Control | Keys.E, "Ctrl E", (o, e) => _rsc.CurrentTool = SelectedTool.Select);
            _rsc.CurrentToolChanged += (s) => select.Checked = s.Equals(SelectedTool.Select);
            tool.DropDownItems.Add(select);

            ToolStripMenuItem areaSelect = GenerateMenuItem("Area Select", Keys.Control | Keys.A, "Ctrl A", (o, e) => _rsc.CurrentTool = SelectedTool.AreaSelect);
            _rsc.CurrentToolChanged += (s) => areaSelect.Checked = s.Equals(SelectedTool.AreaSelect);
            tool.DropDownItems.Add(areaSelect);

            tool.DropDownItems.Add(GenerateMenuItem("Select All", Keys.Control | Keys.Shift | Keys.A, "Ctrl Shift A", (o, e) => _rsc.SelectAll()));

            ToolStripMenuItem lasso = GenerateMenuItem("Lasso", Keys.Control | Keys.Q, "Ctrl Q", (o, e) => _rsc.CurrentTool = SelectedTool.Lasso);
            _rsc.CurrentToolChanged += (s) => lasso.Checked = s.Equals(SelectedTool.Lasso);
            tool.DropDownItems.Add(lasso);

            tool.DropDownItems.Add(new ToolStripSeparator());

            ToolStripMenuItem weight = GenerateMenuItem("Weight", Keys.Control | Keys.W, "Ctrl W", (o, e) => _rsc.CurrentTool = SelectedTool.Weight);
            _rsc.CurrentToolChanged += (s) => weight.Checked = s.Equals(SelectedTool.Merge);
            tool.DropDownItems.Add(weight);

            tool.DropDownItems.Add(new ToolStripMenuItem("Clear All Weights"));
            tool.DropDownItems.Add(new ToolStripSeparator());

            ToolStripMenuItem tracker = GenerateMenuItem("Tracker", Keys.Control | Keys.T, "Ctrl T", (o, e) => _rsc.CurrentTool = SelectedTool.Tracker);
            _rsc.CurrentToolChanged += (s) => tracker.Checked = s.Equals(SelectedTool.Merge);
            tool.DropDownItems.Add(tracker);

            ToolStripMenuItem init = new("Tracking Categories");
            init.DropDownItems.AddRange(new ToolStripItem[] { //TODO: Add shortcuts for tracker subtools
                new ToolStripMenuItem("Vertex"),
                new ToolStripMenuItem("Edge"),
                new ToolStripMenuItem("Arc Direction")
            });
            for(int i = 0; i < init.DropDownItems.Count; i++)
            {
                ((ToolStripMenuItem)init.DropDownItems[i]).Checked = true;
                ((ToolStripMenuItem)init.DropDownItems[i]).CheckOnClick = true;
            }
            init.DropDownItems.Insert(0, new ToolStripMenuItem("Filter by Categories"));
            init.DropDownItems.Insert(1, new ToolStripMenuItem("Enable All in Category"));
            init.DropDownItems.Insert(2, new ToolStripMenuItem("Disable All in Category"));
            init.DropDownItems.Insert(3, new ToolStripSeparator());
            tool.DropDownItems.Add(init);

            tool.DropDownItems.Add(new ToolStripSeparator());
            //(G \ e)
            ToolStripMenuItem merge = GenerateMenuItem("Merge", Keys.Control | Keys.M, "Ctrl M", (o, e) => _rsc.CurrentTool = SelectedTool.Merge);
            _rsc.CurrentToolChanged += (s) => merge.Checked = s.Equals(SelectedTool.Merge);
            tool.DropDownItems.Add(merge);

            ToolStripMenuItem split = GenerateMenuItem("Split", Keys.Control | Keys.K, "Ctrl K", (o, e) => _rsc.CurrentTool = SelectedTool.Split);
            _rsc.CurrentToolChanged += (s) => split.Checked = s.Equals(SelectedTool.Split);
            tool.DropDownItems.Add(split);

            return tool;
        }

        private ToolStripMenuItem InitializeAnalyzeOptions()
        {
            ToolStripMenuItem analysis = new("&Analysis");

            ToolStripMenuItem search = new("Searches");
            ToolStripMenuItem spg = new("Platonic Subgraph");
            spg.DropDownItems.AddRange(new ToolStripItem[]{
                new ToolStripButton("Tetrahedron"),
                new ToolStripButton("Cube"),
                new ToolStripButton("Octahedron"),
                new ToolStripButton("Isocahedron"),
                new ToolStripButton("Dodecahedron")
            });
            search.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripButton("Breadth First Search"), //(select vertex, find other vertex)
                new ToolStripButton("Depth First Search"), //(select vertex, find other vertex)
                new ToolStripButton("Cut-vertex"),
                new ToolStripButton("Cut-set"),
                new ToolStripButton("Subgraph K_n"), //Enter n
                new ToolStripButton("Subgraph K_n,m"), //Enter n,m
                spg
            });
            ToolStripMenuItem identify = new("Tests");
            identify.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripButton("Eulerian Path"),
                new ToolStripButton("Hamiltonian Path"),
                new ToolStripButton("Eulerian Cycle"),
                new ToolStripButton("Hamiltonian Cycle"),
                new ToolStripButton("Regular"),
                new ToolStripButton("Bipartite"),
                new ToolStripButton("K-Connectivity"),
                new ToolStripButton("K-Edge-Connectivity"),
                new ToolStripButton("Spanning Tree"),
                new ToolStripButton("Girth"),
                new ToolStripButton("Graph Genus") //(???)
            });
            ToolStripMenuItem optimize = new("Optimizations");
            optimize.DropDownItems.AddRange(new ToolStripItem[]{
                new ToolStripButton("Minimal Vertex Coloring"),
                new ToolStripButton("Minimal Edge Coloring"),
                new ToolStripButton("Minimal Face Coloring"), //(?)
                new ToolStripButton("Minimal Spanning Tree"), //(tree with minimum weight)
                new ToolStripButton("Critical Path"),
                new ToolStripButton("Maximum Flow") //(define source/sink values)
            });
            ToolStripMenuItem transform = new("Mutations");
            transform.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripButton("Planarize"), //(?)
                new ToolStripButton("Outerplanar"),
                new ToolStripButton("Compliment"),
                new ToolStripButton("Converse"),
                new ToolStripButton("Dualize"), //(?)
                new ToolStripButton("Linearize"), //(Line graph)
            });

            analysis.DropDownItems.AddRange(new ToolStripItem[] {
                search,
                identify,
                optimize,
                transform
            });

            return analysis;
        }

        private ToolStripMenuItem InitializeHelpOptions()
        {
            ToolStripMenuItem help = new("&Help");
            help.DropDownItems.Add(GenerateMenuItem("Guide", Keys.F1, "F1", (o, e) => _rsc.Guide()));
            help.DropDownItems.Add(GenerateMenuItem("Context Help", Keys.Shift | Keys.F1, "Shift F1", (o, e) => _rsc.ContextHelp()));

            ToolStripMenuItem init = new("About");
            init.Click += (o, e) => _rsc.About();
            help.DropDownItems.Add("About");

            return help;
        }

        private void SetSubmenuWidth(ToolStripItem submenu, int width)
        {
            submenu.Width = width;
            if((submenu is ToolStripMenuItem m) && m.DropDownItems.Count != 0)
            {
                foreach(ToolStripItem i in m.DropDownItems)
                    SetSubmenuWidth(i, width);
            }
        }

        private static ToolStripMenuItem GenerateMenuItem(string text, Keys shortcut, string shortcutText, EventHandler clickEvent, bool check = false, bool checkOnClick = false)
        {
            ToolStripMenuItem output = new(text)
            {
                ShortcutKeys = shortcut,
                ShortcutKeyDisplayString = shortcutText,
                ShowShortcutKeys = true,
                Checked = check,
                CheckOnClick = checkOnClick
            };
            output.Click += clickEvent;

            return output;
        }
    }
}
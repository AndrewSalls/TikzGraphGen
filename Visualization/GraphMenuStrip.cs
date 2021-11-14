using System;
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

            ToolStripMenuItem init = new("New Project")
            {
                ShortcutKeys = Keys.Control | Keys.Shift | Keys.N,
                ShortcutKeyDisplayString = "Ctrl Shift N",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.NewProject();
            file.DropDownItems.Add(init);
                
            init = new("Open")
            {
                ShortcutKeys = Keys.Control | Keys.O,
                ShortcutKeyDisplayString = "Ctrl O",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.Open();
            file.DropDownItems.Add(init);

            init = new("Open Recent");
            file.DropDownItems.Add(init);

            init = new("Save")
            {
                ShortcutKeys = Keys.Control | Keys.S,
                ShortcutKeyDisplayString = "Ctrl S",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.Save();
            file.DropDownItems.Add(init);

            init = new("Save As")
            {
                ShortcutKeys = Keys.Control | Keys.Shift | Keys.S,
                ShortcutKeyDisplayString = "Ctrl Shift S",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.SaveAs();
            file.DropDownItems.Add(init);

            init = new("Export")
            {
                ShortcutKeys = Keys.Control | Keys.Shift | Keys.E,
                ShortcutKeyDisplayString = "Ctrl Shift E",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.Export();
            file.DropDownItems.Add(init);

            file.DropDownItems.Add(new ToolStripSeparator());

            init = new("Graph Info") //Get number of vertices, edges, directed edges, faces, etc.
            {
                ShortcutKeys = Keys.Control | Keys.I,
                ShortcutKeyDisplayString = "Ctrl I",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.DisplayInfo();
            file.DropDownItems.Add(init);

            file.DropDownItems.Add(new ToolStripSeparator());

            init = new("Quit")
            {
                ShortcutKeys = Keys.Control | Keys.Alt | Keys.Q,
                ShortcutKeyDisplayString = "Ctrl Alt Q",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.Quit();
            file.DropDownItems.Add(init);

            return file;
        }

        private ToolStripMenuItem InitializeEditOptions()
        {
            ToolStripMenuItem edit = new("&Edit");

            ToolStripMenuItem init = new("Delete")
            {
                ShortcutKeys = Keys.Delete,
                ShortcutKeyDisplayString = "Delete",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.DeleteSelected();
            edit.DropDownItems.Add(init);
                
            init = new("Undo")
            {
                ShortcutKeys = Keys.Control | Keys.Z,
                ShortcutKeyDisplayString = "Ctrl Z",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.Undo();
            _rsc.CanUndoStatusChanged += (b) => init.Enabled = b;
            init.Enabled = false;
            edit.DropDownItems.Add(init);

            init = new("Redo")
            {
                ShortcutKeys = Keys.Control | Keys.Y,
                ShortcutKeyDisplayString = "Ctrl Y",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.Redo();
            _rsc.CanRedoStatusChanged += (b) => init.Enabled = b;
            init.Enabled = false;
            edit.DropDownItems.Add(init);

            init = new("View History");
            init.Click += (o, e) => _rsc.DisplayHistory();
            edit.DropDownItems.Add(init);

            edit.DropDownItems.Add(new ToolStripSeparator());

            init = new("Cut")
            {
                ShortcutKeys = Keys.Control | Keys.X,
                ShortcutKeyDisplayString = "Ctrl X",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.Cut();
            edit.DropDownItems.Add(init);

            init = new("Copy")
            {
                ShortcutKeys = Keys.Control | Keys.C,
                ShortcutKeyDisplayString = "Ctrl C",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.Copy();
            edit.DropDownItems.Add(init);

            init = new("Paste")
            {
                ShortcutKeys = Keys.Control | Keys.V,
                ShortcutKeyDisplayString = "Ctrl V",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.Paste();
            edit.DropDownItems.Add(init);

            edit.DropDownItems.Add(new ToolStripSeparator());

            init = new("Resize");
            init.Click += (o, e) => _rsc.ResizeAll();
            edit.DropDownItems.Add(init);

            init = new("Set Units")
            {
                ShortcutKeys = Keys.Control | Keys.Alt | Keys.U,
                ShortcutKeyDisplayString = "Ctrl Alt U",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.SetUnits();
            edit.DropDownItems.Add(init);

            init = new("Font Options") //(include apply to all option)
            {
                ShortcutKeys = Keys.Control | Keys.Shift | Keys.F,
                ShortcutKeyDisplayString = "Ctrl Shift F",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.FontOptions();
            edit.DropDownItems.Add(init);

            init = new("Preferences")
            {
                ShortcutKeys = Keys.Control | Keys.Alt | Keys.P,
                ShortcutKeyDisplayString = "Ctrl Alt P",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.Preferences();
            edit.DropDownItems.Add(init);

            return edit;
        }

        private ToolStripMenuItem InitializeViewOptions()
        {
            ToolStripMenuItem view = new("&View");

            ToolStripMenuItem init = new("Zoom In")
            {
                ShortcutKeys = Keys.Control | Keys.Oemplus,
                ShortcutKeyDisplayString = "Ctrl +",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.ZoomInc();
            view.DropDownItems.Add(init);

            init = new("Zoom Out")
            {
                ShortcutKeys = Keys.Control | Keys.OemMinus,
                ShortcutKeyDisplayString = "Ctrl -",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.ZoomDec();
            view.DropDownItems.Add(init);

            init = new("Zoom Ratio");
            init.DropDownItems.AddRange(new ToolStripItem[] {
                new  ToolStripMenuItem("10%"),
                new ToolStripMenuItem("25%"),
                new ToolStripMenuItem("50%"),
                new ToolStripMenuItem("75%"),
                new ToolStripMenuItem("100%"),
                new ToolStripMenuItem("125%"),
                new ToolStripMenuItem("150%"),
                new ToolStripMenuItem("200%")
            });
            ((ToolStripMenuItem)init.DropDownItems[4]).Checked = true;
            view.DropDownItems.Add(init);

            init = new("Zoom To Fit") //Make entire graph fit on screen (if possible)
            {
                ShortcutKeys = Keys.Control | Keys.Shift | Keys.O,
                ShortcutKeyDisplayString = "Ctrl Shift O",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.ZoomFit();
            view.DropDownItems.Add(init);

            view.DropDownItems.Add(new ToolStripSeparator());

            init = new("Fullscreen")
            {
                ShortcutKeys = Keys.F11,
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.ToggleFullscreen();
            view.DropDownItems.Add(init);

            init = new("Angle Snap") //(snap to every 15%, toggle)
            {
                ShortcutKeys = Keys.Control | Keys.Alt | Keys.A,
                ShortcutKeyDisplayString = "Ctrl Alt A",
                ShowShortcutKeys = true,
                Checked = true,
                CheckOnClick = true
            };
            init.Click += (o, e) => _rsc.ToggleAngleSnap();
            view.DropDownItems.Add(init);

            init = new("Snap To Unit") //(on by default, attempts to make new lines/vertices along increments (i.e. every new vertex is 10mm apart))
            {
                ShortcutKeys = Keys.Control | Keys.Shift | Keys.I,
                ShortcutKeyDisplayString = "Ctrl Shift I",
                ShowShortcutKeys = true,
                Checked = false,
                CheckOnClick = true
            };
            init.Click += (o, e) => _rsc.ToggleUnitSnap();
            view.DropDownItems.Add(init);

            init = new("Snap To Unit Grid")
            {
                ShortcutKeys = Keys.Control | Keys.U,
                ShortcutKeyDisplayString = "Ctrl U",
                ShowShortcutKeys = true,
                Checked = false,
                CheckOnClick = true
            };
            init.Click += (o, e) => _rsc.ToggleGridUnitSnap();
            view.DropDownItems.Add(init);

            view.DropDownItems.Add(new ToolStripSeparator());

            init = new("Show Page Border") //(create line showing page border + margin dimensions for PDF page)
            {
                ShortcutKeys = Keys.Control | Keys.Alt | Keys.B,
                ShortcutKeyDisplayString = "Ctrl Alt B",
                ShowShortcutKeys = true,
                Checked = false,
                CheckOnClick = true
            };
            init.Click += (o, e) => _rsc.ToggleBorder();
            view.DropDownItems.Add(init);

            init = new("Show Unit Grid")
            {
                ShortcutKeys = Keys.Control | Keys.Shift | Keys.U,
                ShortcutKeyDisplayString = "Ctrl Shift U",
                ShowShortcutKeys = true,
                Checked = false,
                CheckOnClick = true
            };
            init.Click += (o, e) => _rsc.ToggleUnitGrid();
            view.DropDownItems.Add(init);

            init = new("Show Menubar")
            {
                ShortcutKeys = Keys.Control | Keys.Shift | Keys.M,
                ShortcutKeyDisplayString = "Ctrl Shift M",
                ShowShortcutKeys = true,
                Checked = true,
                CheckOnClick = true
            };
            init.Click += (o, e) => _rsc.ToggleMenu();
            view.DropDownItems.Add(init);

            init = new("Show Toolbar")
            {
                ShortcutKeys = Keys.Control | Keys.Shift | Keys.T,
                ShortcutKeyDisplayString = "Ctrl Shift T",
                ShowShortcutKeys = true,
                Checked = true,
                CheckOnClick = true
            };
            init.Click += (o, e) => _rsc.ToggleTools();
            view.DropDownItems.Add(init);

            return view;
        }

        private ToolStripMenuItem InitializeToolOptions()
        {
            ToolStripMenuItem tool = new("&Tools");
            ToolStripMenuItem init = new("Vertex")
            {
                ShortcutKeys = Keys.Control | Keys.J,
                ShortcutKeyDisplayString = "Ctrl J",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.CurrentTool = SelectedTool.Vertex;
            tool.DropDownItems.Add(init);

            init = new("Edge")
            {
                ShortcutKeys = Keys.Control | Keys.G,
                ShortcutKeyDisplayString = "Ctrl G",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.CurrentTool = SelectedTool.Edge;
            tool.DropDownItems.Add(init);

            init = new("Edge Cap")
            {
                ShortcutKeys = Keys.Control | Keys.H,
                ShortcutKeyDisplayString = "Ctrl H",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.CurrentTool = SelectedTool.EdgeCap;
            tool.DropDownItems.Add(init);

            tool.DropDownItems.Add(new ToolStripSeparator());

            init = new("Label")
            {
                ShortcutKeys = Keys.Control | Keys.L,
                ShortcutKeyDisplayString = "Ctrl L",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.CurrentTool = SelectedTool.Label;
            tool.DropDownItems.Add(init);

            init = new("Label Snap")
            {
                Checked = true,
                CheckOnClick = true,
                ShortcutKeys = Keys.Control | Keys.Shift | Keys.L,
                ShortcutKeyDisplayString = "Ctrl Shift L",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.ToggleLabelEdgeSnap();
            tool.DropDownItems.Add(init);

            tool.DropDownItems.Add(new ToolStripSeparator());

            init = new("Eraser")
            {
                ShortcutKeys = Keys.Control | Keys.B,
                ShortcutKeyDisplayString = "Ctrl B",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.CurrentTool = SelectedTool.Eraser;
            tool.DropDownItems.Add(init);

            init = new("Transform") //TODO: Expand to be several menuitems with each transform subtool (each with a shortcut too)
            {
                ShortcutKeys = Keys.Control | Keys.N,
                ShortcutKeyDisplayString = "Ctrl N",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.CurrentTool = SelectedTool.Transform;
            tool.DropDownItems.Add(init);

            init = new("Select")
            {
                ShortcutKeys = Keys.Control | Keys.E,
                ShortcutKeyDisplayString = "Ctrl E",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.CurrentTool = SelectedTool.Select;
            tool.DropDownItems.Add(init);

            init = new("Area Select")
            {
                ShortcutKeys = Keys.Control | Keys.A,
                ShortcutKeyDisplayString = "Ctrl A",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.CurrentTool = SelectedTool.AreaSelect;
            tool.DropDownItems.Add(init);

            init = new("Select All")
            {
                ShortcutKeys = Keys.Control | Keys.Shift | Keys.A,
                ShortcutKeyDisplayString = "Ctrl Shift A",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.SelectAll();
            tool.DropDownItems.Add(init);

            init = new("Lasso")
            {
                ShortcutKeys = Keys.Control | Keys.Q,
                ShortcutKeyDisplayString = "Ctrl Q",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.CurrentTool = SelectedTool.Lasso;
            tool.DropDownItems.Add(init);

            tool.DropDownItems.Add(new ToolStripSeparator());

            init = new("Weight")
            {
                ShortcutKeys = Keys.Control | Keys.W,
                ShortcutKeyDisplayString = "Ctrl W",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.CurrentTool = SelectedTool.Weight;
            tool.DropDownItems.Add(init);

            tool.DropDownItems.Add(init); init = new("Clear All Weights");
            tool.DropDownItems.Add(init);

            tool.DropDownItems.Add(new ToolStripSeparator());

            init = new("Tracker")
            {
                ShortcutKeys = Keys.Control | Keys.T,
                ShortcutKeyDisplayString = "Ctrl T",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.CurrentTool = SelectedTool.Tracker;
            tool.DropDownItems.Add(init);

            init = new("Tracking Categories");
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

            init = new("Merge") //(G \ e)
            {
                ShortcutKeys = Keys.Control | Keys.M,
                ShortcutKeyDisplayString = "Ctrl M",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.CurrentTool = SelectedTool.Merge;
            tool.DropDownItems.Add(init);

            init = new("Split")
            {
                ShortcutKeys = Keys.Control | Keys.K,
                ShortcutKeyDisplayString = "Ctrl K",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.CurrentTool = SelectedTool.Split;
            tool.DropDownItems.Add(init);

            _rsc.CurrentToolChanged += (SelectedTool cur) =>
            {
                foreach (ToolStripItem i in tool.DropDownItems)
                {
                    if(i is ToolStripMenuItem item)
                        item.Checked = false;
                }

                switch (cur)
                {
                    case SelectedTool.Vertex:
                        ((ToolStripMenuItem)tool.DropDownItems[0]).Checked = true;
                        break;
                    case SelectedTool.Edge:
                        ((ToolStripMenuItem)tool.DropDownItems[1]).Checked = true;
                        break;
                    case SelectedTool.EdgeCap:
                        ((ToolStripMenuItem)tool.DropDownItems[2]).Checked = true;
                        break;
                    case SelectedTool.Label:
                        ((ToolStripMenuItem)tool.DropDownItems[4]).Checked = true;
                        break;
                    case SelectedTool.Eraser:
                        ((ToolStripMenuItem)tool.DropDownItems[7]).Checked = true;
                        break;
                    case SelectedTool.Transform:
                        ((ToolStripMenuItem)tool.DropDownItems[8]).Checked = true;
                        break;
                    case SelectedTool.Select:
                        ((ToolStripMenuItem)tool.DropDownItems[9]).Checked = true;
                        break;
                    case SelectedTool.AreaSelect:
                        ((ToolStripMenuItem)tool.DropDownItems[10]).Checked = true;
                        break;
                    case SelectedTool.Lasso:
                        ((ToolStripMenuItem)tool.DropDownItems[12]).Checked = true;
                        break;
                    case SelectedTool.Weight:
                        ((ToolStripMenuItem)tool.DropDownItems[14]).Checked = true;
                        break;
                    case SelectedTool.Tracker:
                        ((ToolStripMenuItem)tool.DropDownItems[17]).Checked = true;
                        break;
                    case SelectedTool.Merge:
                        ((ToolStripMenuItem)tool.DropDownItems[18]).Checked = true;
                        break;
                    case SelectedTool.Split:
                        ((ToolStripMenuItem)tool.DropDownItems[21]).Checked = true;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            };

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
            ToolStripMenuItem init = new("Guide")
            {
                ShortcutKeys = Keys.F1,
                ShortcutKeyDisplayString = "F1",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.Guide();
            help.DropDownItems.Add(init);
            init = new("Context Help")
            {
                ShortcutKeys = Keys.Shift | Keys.F1,
                ShortcutKeyDisplayString = "Shift F1",
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.ContextHelp();
            help.DropDownItems.Add(init);

            init = new("About");
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
    }
}

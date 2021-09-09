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
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.NewProject();
            file.DropDownItems.Add(init);
                
            init = new("Open")
            {
                ShortcutKeys = Keys.Control | Keys.O,
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.Open();
            file.DropDownItems.Add(init);

            init = new("Open Recent");
            file.DropDownItems.Add(init);

            init = new("Save")
            {
                ShortcutKeys = Keys.Control | Keys.S,
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.Save();
            file.DropDownItems.Add(init);

            init = new("Save As")
            {
                ShortcutKeys = Keys.Control | Keys.Shift | Keys.S,
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.SaveAs();
            file.DropDownItems.Add(init);

            init = new("Export")
            {
                ShortcutKeys = Keys.Control | Keys.Shift | Keys.E,
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.Export();
            file.DropDownItems.Add(init);

            file.DropDownItems.Add(new ToolStripSeparator());

            init = new("Graph Info") //Get number of vertices, edges, directed edges, faces, etc.
            {
                ShortcutKeys = Keys.Control | Keys.I,
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.DisplayInfo();
            file.DropDownItems.Add(init);

            file.DropDownItems.Add(new ToolStripSeparator());

            init = new("Quit")
            {
                ShortcutKeys = Keys.Control | Keys.Alt | Keys.Q,
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.Quit();
            file.DropDownItems.Add(init);

            return file;
        }

        private ToolStripMenuItem InitializeEditOptions()
        {
            ToolStripMenuItem edit = new("&Edit");

            ToolStripMenuItem init = new("Undo")
            {
                ShortcutKeys = Keys.Control | Keys.Z,
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.Undo();
            _rsc.CanUndoStatusChanged += (b) => init.Enabled = b;
            init.Enabled = false;
            edit.DropDownItems.Add(init);

            init = new("Redo")
            {
                ShortcutKeys = Keys.Control | Keys.Y,
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
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.Cut();
            edit.DropDownItems.Add(init);

            init = new("Copy")
            {
                ShortcutKeys = Keys.Control | Keys.C,
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.Copy();
            edit.DropDownItems.Add(init);

            init = new("Paste")
            {
                ShortcutKeys = Keys.Control | Keys.V,
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
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.SetUnits();
            edit.DropDownItems.Add(init);

            init = new("Font Options") //(include apply to all option)
            {
                ShortcutKeys = Keys.Control | Keys.Shift | Keys.F,
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.FontOptions();
            edit.DropDownItems.Add(init);

            init = new("Preferences")
            {
                ShortcutKeys = Keys.Control | Keys.Alt | Keys.P,
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.Preferences();
            edit.DropDownItems.Add(init);

            return edit;
        }

        private ToolStripMenuItem InitializeViewOptions()
        {
            ToolStripMenuItem view = new("&View");

            ToolStripMenuItem init = new("Zoom In"); //TODO: Add ctrl + mouse^/v to MouseWheel event
            init.Click += (o, e) => _rsc.ZoomInc();
            view.DropDownItems.Add(init);

            init = new("Zoom Out");
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
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.ZoomFit();
            view.DropDownItems.Add(init);

            view.DropDownItems.Add(new ToolStripSeparator());

            init = new("Page Border") //(create line showing page border + margin dimensions for PDF page)
            {
                ShortcutKeys = Keys.Control | Keys.B,
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.ToggleBorder();
            view.DropDownItems.Add(init);

            init = new("Fullscreen")
            {
                ShortcutKeys = Keys.F11,
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.ToggleFullscreen();
            view.DropDownItems.Add(init);

            init = new("Snap To Unit") //(on by default, attempts to make new lines/vertices along increments (i.e. every new vertex is 10mm apart))
            {
                ShortcutKeys = Keys.Control | Keys.U,
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.ToggleUnitSnap();
            view.DropDownItems.Add(init);

            init = new("Show Unit Grid")
            {
                ShortcutKeys = Keys.Control | Keys.Shift | Keys.U,
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.ToggleUnitGrid();
            view.DropDownItems.Add(init);

            view.DropDownItems.Add(new ToolStripSeparator());

            init = new("Show Menubar")
            {
                ShortcutKeys = Keys.Control | Keys.Shift | Keys.M,
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.ToggleMenu();
            view.DropDownItems.Add(init);

            init = new("Show Toolbar")
            {
                ShortcutKeys = Keys.Control | Keys.Shift | Keys.T,
                ShowShortcutKeys = true
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
                ShortcutKeys = Keys.Control | Keys.K,
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.CurrentTool = SelectedTool.Vertex;
            init.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Circle"),
                new ToolStripMenuItem("Square"),
                new ToolStripMenuItem("Diamond"),
                new ToolStripMenuItem("Rectangle"),
                new ToolStripMenuItem("Ellipse"),
                new ToolStripMenuItem("No Outline"),
                new ToolStripSeparator(),
                new ToolStripButton("Vertex Options") //Fit vertex to text option
            });
            ((ToolStripMenuItem)init.DropDownItems[0]).Checked = true;
            tool.DropDownItems.Add(init);

            init = new("Edge")
            {
                ShortcutKeys = Keys.Control | Keys.G,
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.CurrentTool = SelectedTool.Edge;
            init.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Regular"),
                new ToolStripMenuItem("Bent"),
                new ToolStripMenuItem("Curved"),
                new ToolStripMenuItem("Beizer"),
                new ToolStripMenuItem("No Line"),
                new ToolStripSeparator(),
                new ToolStripButton("Edge Options")
            });
            ((ToolStripMenuItem)init.DropDownItems[0]).Checked = true;
            tool.DropDownItems.Add(init);

            init = new("Edge Cap")
            {
                ShortcutKeys = Keys.Control | Keys.H,
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.CurrentTool = SelectedTool.EdgeCap;
            init.DropDownItems.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem("Arrow"),
                new ToolStripMenuItem("Square"),
                new ToolStripMenuItem("Round"),
                new ToolStripSeparator(),
                new ToolStripButton("Edge Cap Options")
            });
            ((ToolStripMenuItem)init.DropDownItems[2]).Checked = true;
            tool.DropDownItems.Add(init);

            init = new("Label")
            {
                ShortcutKeys = Keys.Control | Keys.L,
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.CurrentTool = SelectedTool.Label;
            init.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Label Snap"),
                new ToolStripSeparator(),
                new ToolStripButton("Font Options"),
                new ToolStripButton("Label Options")
            });
            tool.DropDownItems.Add(init);
            init = (ToolStripMenuItem)init.DropDownItems[0];
            init.Checked = true;
            init.ShortcutKeys = Keys.Control | Keys.Shift | Keys.L;
            init.ShowShortcutKeys = true;
            init.Click += (o, e) => _rsc.ToggleAngleSnap();

            init = new("Transform") //TODO: Add shortcuts for transform subtools
            {
                ShortcutKeys = Keys.Control | Keys.N,
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.CurrentTool = SelectedTool.Transform;
            init.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Rotate"),
                new ToolStripMenuItem("Move"),
                new ToolStripMenuItem("Resize"),
                new ToolStripSeparator(),
                new ToolStripMenuItem("Angle Snap") //(snap to every 15%, toggle)
            });
            ((ToolStripMenuItem)init.DropDownItems[1]).Checked = true;
            ((ToolStripMenuItem)init.DropDownItems[4]).Checked = true;
            tool.DropDownItems.Add(init);

            init = new("Select")
            {
                ShortcutKeys = Keys.Control | Keys.E,
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.CurrentTool = SelectedTool.Select;
            tool.DropDownItems.Add(init);

            init = new("Area Select")
            {
                ShortcutKeys = Keys.Control | Keys.A,
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.CurrentTool = SelectedTool.AreaSelect;
            tool.DropDownItems.Add(init);

            init = new("Select All")
            {
                ShortcutKeys = Keys.Control | Keys.Shift | Keys.A,
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.SelectAll();
            tool.DropDownItems.Add(init);

            init = new("Lasso")
            {
                ShortcutKeys = Keys.Control | Keys.Q,
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.CurrentTool = SelectedTool.Lasso;
            tool.DropDownItems.Add(init);

            init = new("Weight")
            {
                ShortcutKeys = Keys.Control | Keys.W,
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.CurrentTool = SelectedTool.Weight;
            init.DropDownItems.AddRange(new ToolStripItem[] { //TODO: Add shortcuts for weight subtools
                new ToolStripMenuItem("Increment"),
                new ToolStripMenuItem("Decrement"),
                new ToolStripMenuItem("Clear"),
                new ToolStripSeparator(),
                new ToolStripMenuItem("Clear All")
            });
            ((ToolStripMenuItem)init.DropDownItems[0]).Checked = true;
            tool.DropDownItems.Add(init);

            init = new("Tracker")
            {
                ShortcutKeys = Keys.Control | Keys.T,
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.CurrentTool = SelectedTool.Tracker;
            init.DropDownItems.AddRange(new ToolStripItem[] { //TODO: Add shortcuts for tracker subtools
                new ToolStripMenuItem("Disable"),
                new ToolStripMenuItem("Enable"),
                new ToolStripSeparator(),
                new ToolStripMenuItem("Filter"), //Only apply to items selected below
                new ToolStripMenuItem("Vertex"),
                new ToolStripMenuItem("Edge"),
                new ToolStripMenuItem("Arc"),
                new ToolStripMenuItem("Arrow Head"),
                new ToolStripMenuItem("Label"),
                new ToolStripMenuItem("Weight"),
                new ToolStripButton("Apply to All")
            });
            ((ToolStripMenuItem)init.DropDownItems[0]).Checked = true;
            tool.DropDownItems.Add(init);

            init = new("Merge") //(G \ e)
            {
                ShortcutKeys = Keys.Control | Keys.M,
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.CurrentTool = SelectedTool.Merge;
            tool.DropDownItems.Add(init);

            init = new("Split")
            {
                ShortcutKeys = Keys.Control | Keys.J,
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.CurrentTool = SelectedTool.Split;
            tool.DropDownItems.Add(init);

            _rsc.CurrentToolChanged += (SelectedTool cur) =>
            {
                foreach (ToolStripItem i in tool.DropDownItems)
                {
                    if(i is ToolStripMenuItem item)
                    {
                        item.Checked = false;
                    }
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
                        ((ToolStripMenuItem)tool.DropDownItems[3]).Checked = true;
                        break;
                    case SelectedTool.Transform:
                        ((ToolStripMenuItem)tool.DropDownItems[4]).Checked = true;
                        break;
                    case SelectedTool.Select:
                        ((ToolStripMenuItem)tool.DropDownItems[5]).Checked = true;
                        break;
                    case SelectedTool.AreaSelect:
                        ((ToolStripMenuItem)tool.DropDownItems[6]).Checked = true;
                        break;
                    case SelectedTool.Lasso:
                        ((ToolStripMenuItem)tool.DropDownItems[8]).Checked = true;
                        break;
                    case SelectedTool.Weight:
                        ((ToolStripMenuItem)tool.DropDownItems[9]).Checked = true;
                        break;
                    case SelectedTool.Tracker:
                        ((ToolStripMenuItem)tool.DropDownItems[10]).Checked = true;
                        break;
                    case SelectedTool.Merge:
                        ((ToolStripMenuItem)tool.DropDownItems[11]).Checked = true;
                        break;
                    case SelectedTool.Split:
                        ((ToolStripMenuItem)tool.DropDownItems[12]).Checked = true;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            };

            return tool;
        }

#pragma warning disable CA1822 // Mark members as static
        private ToolStripMenuItem InitializeAnalyzeOptions()
#pragma warning restore CA1822 // Mark members as static
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
                ShowShortcutKeys = true
            };
            init.Click += (o, e) => _rsc.Guide();
            help.DropDownItems.Add(init);
            init = new("Context Help")
            {
                ShortcutKeys = Keys.Shift | Keys.F1,
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
                {
                    SetSubmenuWidth(i, width);
                }
            }
        }
    }
}

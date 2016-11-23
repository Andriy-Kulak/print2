// ===========================================================================
//	©2013 WebSupergoo. All rights reserved.
// ===========================================================================
// NB This does not currently support repeating table headers and footers.

#if DEBUG
//#define DEBUG_TABLES
#endif

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Drawing;
using System.Drawing.Imaging;
using System.Web;
using System.Diagnostics;

using WebSupergoo.ABCpdf10;
using WebSupergoo.ABCpdf10.Objects;
using WebSupergoo.ABCpdf10.Atoms;


namespace HtmlTables {
	/// <summary>
	/// Table layout class for taking simple HTML and paging it into a PDF.
	/// </summary>
	public class HtmlDoc {
		private Doc doc = null;
		private Dictionary<string, ImageHolder> images = new Dictionary<string, ImageHolder>();
		private List<int> pages = new List<int>();
		private class Range { public int start = 0; public int end = 0; };
		private Dictionary<int, Range> layers = new Dictionary<int, Range>();
		private bool imagesAdded = false;
		
		/// <summary>
		/// The table layout class.
		/// </summary>
		/// <param name="doc">The document into which the content is to be added.</param>
		public HtmlDoc(Doc doc) {
			this.doc = doc;
			#if DEBUG_TABLES
			Block.sN = 0;
			#endif
		}

		~HtmlDoc() { 
			#if DEBUG_TABLES
			if (!imagesAdded)
				throw new Exception("AddImage needs to be called to add images.");
			#endif
		}

        private Size maxImageSize;
        /// <summary>
        /// The maximum size of image that should be allowed. This figure is in document Units.
        /// Any images larger than this width will be scaled down to fit. If the maximum dimensions
        /// are zero then this automatic scaling is disabled.
        /// </summary>
        public Size MaxImageSize { get { return maxImageSize; } set { maxImageSize = value; } }

		/// <summary>
		/// Set the HTML file that is to be added to the PDF document.
		/// </summary>
		/// <param name="file">The path to the HTML file.</param>
		public void SetFile(string file) {
			string theText;
			using (StreamReader theReader = File.OpenText(file)) {
				theText = theReader.ReadToEnd();
			}
			SetHtml(theText, "file://" + Path.GetDirectoryName(file) + Path.DirectorySeparatorChar);
		}

		TableBlock mRoot = null;
		/// <summary>
		/// Set the HTML that is to be added to the PDF document.
		/// </summary>
		/// <param name="text">The HTML to be added.</param>
		/// <param name="baseAddress">The base address to be used for resolving relative URLs.</param>
		public void SetHtml(string text, string baseAddress) {
            Size max = new Size();
            max.Width = (int)Math.Round(ConvertToPoints(maxImageSize.Width));
            max.Height = (int)Math.Round(ConvertToPoints(maxImageSize.Height));
            mRoot = new HtmlReader(text, baseAddress, max).CreateTable(images);
		}

		/// <summary>
		/// Whether all the content has been drawn.
		/// </summary>
		public bool Drawn { get { return (mRoot == null) || (mRoot.StartKid >= mRoot.Kids.Count); } }

		/// <summary>
		/// Draw the current content into the currect rectangle of the current page of the document.
		/// </summary>
		public XRect Draw() {
			XRect xr = null;
			if (!Drawn) {
				// save state
				string saveRect = doc.Rect.String;
				UnitType saveUnits = doc.Units;
				bool saveTopDown = doc.TopDown;
				string saveColor = doc.Color.String;
				double saveHPos = doc.TextStyle.HPos;
				// do work
				doc.Units = UnitType.Points;
				doc.TopDown = false;
				doc.Color.String = "0 0 0";
				string saveRectPoints = doc.Rect.String;
				Range range = new Range();
				range.start = doc.LayerCount;
				mRoot.Reset(false);
				mRoot.Width = (int)Math.Round(doc.Rect.Width);
				mRoot.Draw(doc, false);
				mRoot.Draw(doc, true);
				pages.Add(doc.Page);
				if (Drawn)
					AddImages();
				range.end = doc.LayerCount;
				layers.Add(doc.Page, range);
				// put table area into rect
				doc.Rect.String = saveRectPoints;
				doc.Rect.Bottom = doc.Rect.Top - mRoot.Height;
				// reset state
				doc.TextStyle.HPos = saveHPos;
				doc.Color.String = saveColor;
				doc.TopDown = saveTopDown;
				doc.Units = saveUnits;
				// put table area into return value
				xr = new XRect();
				xr.String = doc.Rect.String;
				// reset drawing area
				doc.Rect.String = saveRect;
			}
			return xr;
		}

		/// <summary>
		/// Get the areas into which content has been drawn. These can be divided into two types. The cells contain content
		/// which occupies a certain area. The tables also contain chrome such as dividing lines and borders and these occupy
		/// a certain area too. This function allows you to distinguish between the two.
		/// </summary>
		/// <param name="textOnly">Whether one should include table chrome as well as cell contents in the output list.</param>
		/// <returns></returns>
		public List<XRect> GetContentArea(bool noChrome) {
			List<XRect> rects = new List<XRect>();
			Page page = doc.ObjectSoup[doc.Page] as Page;
			Debug.Assert(page != null);
			if (page != null) {
				Range range = layers[page.ID];
				StreamObject[] streams = page.GetLayers();
				for (int i = range.start; i < range.end; i++) {
					Layer gl = streams[i] as Layer;
					if ((noChrome) && ((gl as TextLayer) == null))
						continue;
                    if (gl != null) {
                        string rc = doc.GetInfo(gl.ID, "/TextRect:rect");
                        rects.Add(rc.Length > 0 ? new XRect(rc) : gl.Rect);
                    }
				}
			}
			// if we're dealing with an abstracted coordinate space
			// then we need to convert these rectangles from native units
			if ((doc.TopDown == true) || (doc.Units != UnitType.Points)) {
				string saveRect = doc.Rect.String;
				bool topDown = doc.TopDown;
				UnitType units = doc.Units;
				foreach (XRect xr in rects) {
					doc.Units = UnitType.Points;
					doc.TopDown = false;
					doc.Rect.String = xr.String;
					doc.Units = units;
					doc.TopDown = topDown;
					xr.String = doc.Rect.String;
				}
				doc.Rect.String = saveRect;
			}
			return rects;
		}

		/// <summary>
		/// Delete the content added to the page by the most recent call to Draw.
		/// </summary>
		public void Delete() {
			foreach (int id in pages) {
				doc.Page = id;
				Page page = doc.ObjectSoup[doc.Page] as Page;
				Debug.Assert(page != null);
				if (page != null) {
					Range range = layers[page.ID];
					StreamObject[] streams = page.GetLayers();
					Debug.Assert(streams.Length >= range.end);
					for (int i = range.start; i < range.end; i++)
						doc.Delete(streams[i].ID);
				}
			}
			mRoot.Reset(true);
			pages = new List<int>();
			layers = new Dictionary<int, Range>();
			imagesAdded = false;
		}

		private void AddImages() {
			if (!imagesAdded) {
				foreach (int id in pages) {
					doc.Page = id;
					Page page = doc.ObjectSoup[id] as Page;
					Debug.Assert(page != null);
					if (page != null) {
						Annotation[] annots = page.GetAnnotations();
						foreach (Annotation annot in annots) {
							string uri = Atom.GetText(Atom.GetItem(Atom.GetItem(annot.Atom, "A"), "URI"));
							ImageHolder img;
							if (images.TryGetValue(uri, out img)) {
								doc.Rect.String = annot.Rect.String;
								if (img.image.Type == "MemoryBMP") // EMF
									doc.AddImageData(img.data);
								else
									doc.AddImageObject(img.image, true);
								// NB remove all annotations as this is much faster than deleting
								DictAtom.RemoveItem(page.Atom, "Annots");
							}
						}
					}
				}
			}
			imagesAdded = true;
		}

        double scaleFactor = 0.0;
        private double ConvertToPoints(double value) {
            if (scaleFactor == 0.0) {
                double saveWidth = doc.Width;
				UnitType saveUnits = doc.Units;
                doc.Width = 1;
                doc.Units = UnitType.Points;
                scaleFactor = doc.Width;
                doc.Units = saveUnits;
                doc.Width = saveWidth;
            }
            return scaleFactor * value;
        }
	}

	internal class ImageHolder {
		public ImageHolder(XImage image, byte[] data) {
			this.image = image;
			this.data = data;
		}
		public XImage image = null;
		public byte[] data = null;
	}

	internal abstract class Block {
		protected Container parent = null;
		public Dictionary<string, string> attributes = null;
		public enum TruncationType { None, Partial, Total };
		protected TruncationType truncation = TruncationType.None;

		public Block(Container mother) { parent = mother; }

		public Container Parent { get { return parent; } }

		protected int width = Int32.MinValue, height = Int32.MinValue, minWidth = Int32.MinValue;
		public virtual int Width { get { return (width != Int32.MinValue) ? width : 0; } set { width = value; } }
		public virtual int Height { get { return (height != Int32.MinValue) ? height : 0; } set { height = value; } }
		public virtual int MinWidth { get { return (minWidth != Int32.MinValue) ? minWidth : 0; } set { minWidth = value; } }

		public abstract void Draw(Doc doc, bool chrome);
		public virtual TruncationType Truncation { get { return truncation; } }

		public void AddAttribute(string name, string value) {
			if (attributes == null)
				attributes = new Dictionary<string, string>();
			if (!attributes.ContainsKey(name))
				attributes.Add(name, value);
		}

		public string GetAttribute(string name) {
			if (attributes == null)
				return null;
			string value;
			return (attributes.TryGetValue(name, out value)) ? value : null;
		}

		public int GetAttribute(string name, int deflt) {
			int value = 0;
			string str = GetAttribute(name);
			if (str == null)
				return deflt;
			if (!Int32.TryParse(str, out value))
				return deflt;
			return value;
		}

		public string GetAttributeColor(string name, string deflt) {
			string str = GetAttribute(name);
			if (str == null)
				return deflt;
			try {
				Color c = ColorTranslator.FromHtml(str);
				str = String.Format("{0} {1} {2}", c.R, c.G, c.B);
				return str;
			}
			catch {
			}
			return deflt;
		}

		protected string id = null;
		protected string bgcolor = null;
		public string Id { get { return id; } }
		public string BgColor { get { return bgcolor; } }

		public virtual void RealizeAttributes() {
			id = GetAttribute("id");
			bgcolor = GetAttributeColor("bgcolor", null);
		}

		public virtual void RealizeWidth() { }

		public virtual void Reset(bool hard) {
			truncation = TruncationType.None;
			height = Int32.MinValue;
			if (hard)
				width = Int32.MinValue;
		}

		#if DEBUG_TABLES
		public static int sN = 0, sBreakN = -1;
		private static Random rnd = new Random(5);
		public static void Annotate(Doc doc, int size, string message) {
			if (sN == sBreakN)
				Debugger.Break();
			string color = rnd.Next(0xFFFFFF).ToString("X").PadLeft(6, '0');
			message = String.Format("<font fontsize=\'{0}\' color=\'#{1}\'>#{2} - {3}</font>", size, color, sN++, message);
			string s = doc.Rect.String;
			doc.AddHtml(message);
			doc.Rect.String = s;
		}
		#endif
	}

	internal abstract class Container : Block {
		public enum Direction { LeftRight, TopDown }
		protected List<Block> kids = new List<Block>();
		protected Direction direction = Direction.LeftRight;
		protected int startKid = 0, endKid = Int32.MaxValue; // the index of the first and last kids to be drawn

		protected int header = 0;
		protected int footer = 0;
		public int Header { get { return header; } set { header = value; } }
		public int Footer { get { return footer; } set { footer = value; } }

		public Container(Container mother, Direction dir)
			: base(mother) {
			direction = dir;
		}

		public List<Block> Kids { get { return kids; } }
		public int StartKid { get { return startKid; } }

		public override void RealizeAttributes() {
			base.RealizeAttributes();
			foreach (Block child in kids)
				child.RealizeAttributes();
		}

		public override void RealizeWidth() {
			base.RealizeWidth();
			foreach (Block child in kids)
				child.RealizeWidth();
		}

		public override void Reset(bool hard) {
			base.Reset(hard);
			layercount = -1;
			if (hard)
				startKid = 0;
			foreach (Block kid in kids)
				kid.Reset(hard);
		}

		public override int Width {
			get {
				if (width != Int32.MinValue)
					return width;
				int v = 0;
				if (direction == Direction.LeftRight) {
					foreach (Block child in kids)
						v += child.Width;
				}
				else {
					v = kids.Count > 0 ? Int32.MinValue : 0;
					foreach (Block child in kids)
						v = Math.Max(child.Width, v);
				}
				return v;
			}
			set {
				// Here we override the specified width if it is too large
				// for the container. However it would probably be better
				// to scale all the contents instead.
				if ((width == Int32.MinValue) || (width > value)) {
					width = value;
					if ((direction == Direction.TopDown) || (kids.Count == 1)) {
						foreach (Block child in kids)
							child.Width = value;
					}
				}
			}
		}

		public override int Height {
			get {
				if (height != Int32.MinValue)
					return height;
				int v = 0;
				if (direction == Direction.LeftRight) {
					v = kids.Count > 0 ? Int32.MinValue : 0;
					foreach (Block child in kids)
						v = Math.Max(v, child.Height);
				}
				else {
					foreach (Block child in kids)
						v += child.Height;
				}
				return v;
			}
			set {
				if (height == Int32.MinValue) {
					height = value;
					if ((direction == Direction.LeftRight) || (kids.Count == 1)) {
						foreach (Block child in kids)
							child.Height = value;
					}
				}
			}
		}

		private int layercount = -1;
		public void PrepareBackground(Doc doc) {
			if (layercount < 0)
				layercount = doc.LayerCount;
		}

		public void DrawBackground(Doc doc, string bgcolor, int border, string bordercolor) {
			#if DEBUG_TABLES
			Debug.Assert(doc.Rect.Height < 100000);
			Debug.Assert(doc.Rect.Width < 100000);
			Debug.Assert(doc.Rect.Height > 0);
			Debug.Assert(doc.Rect.Width > 0);
			#endif
			doc.Layer = doc.LayerCount - layercount + 1;
			string color = doc.Color.String;
			if (bgcolor != null) {
				doc.Color.String = bgcolor;
				doc.FillRect();
			}
			if ((border > 0) && (bordercolor != null)) {
				doc.Color.String = bordercolor;
				doc.Width = border;
				doc.FrameRect(true);
			}
			doc.Color.String = color;
			doc.Layer = 1;
			layercount = 0;
		}

		public override void Draw(Doc doc, bool chrome) {
			#if DEBUG_TABLES
			//if ((id == "address") /*&& (doc.PageNumber == 2)*/)
			//	Debugger.Break();
			#endif
			PrepareBackground(doc);
			if (!chrome) {
				endKid = Int32.MaxValue;
				truncation = TruncationType.None;
			}
			doc.Rect.Right = doc.Rect.Left + Width;
			string rect = doc.Rect.String;
			int theStart = startKid, theEnd = Math.Min(endKid, kids.Count - 1), i = 0;
			if (direction == Direction.TopDown) {
				for (i = theStart; i <= theEnd; i++) {
					Block child = kids[i];
					string save = doc.Rect.String;
					child.Draw(doc, chrome);
					doc.Rect.String = save;
					if ((!chrome) && (child.Truncation != TruncationType.None)) {
						truncation = ((child.Truncation == TruncationType.Total) && (i == theStart)) ? TruncationType.Total : TruncationType.Partial;
						if (child.Truncation == TruncationType.Total)
							i--;
						break;
					}
					doc.Rect.Top = doc.Rect.Top - child.Height;
				}
			}
			else { // Direction.LeftRight
				bool partial = false, total = false;
				for (i = theStart; i <= theEnd; i++) {
					Block child = kids[i];
					int w = child.Width;
					doc.Rect.Right = doc.Rect.Left + w;
					string save = doc.Rect.String;
					child.Draw(doc, chrome);
					doc.Rect.String = save;
					if (!chrome) {
						if (child.Truncation == TruncationType.Partial)
							partial = true;
						else if (child.Truncation == TruncationType.Total)
							total = true;
					}
					doc.Rect.Move(w, 0);
				}
				if (!chrome) {
					if (partial)
						truncation = TruncationType.Partial;
					else if (total) {
						truncation = TruncationType.Total;
						i = -1;
					}
				}
			}
			if (chrome) {
				// we've done the chrome now so set the startKid to the point we left off
				if (direction == Direction.TopDown) {
					startKid = endKid;
					if ((startKid >= 0) && (startKid < kids.Count)) {
						Block child = kids[startKid];
						if (child.Truncation != TruncationType.Partial)
							startKid++;
					}
				}
				else // Direction.LeftRight
					startKid = (truncation == TruncationType.None) ? kids.Count - 1 : 0;
				if (startKid < 0)
					startKid = 0;
			}
			else { 
				// we drew content for items from startKid up to point at which we ran out of space
				// so set endKid so that we don't draw chrome for items that wouldn't fit
				endKid = i;
			}
			doc.Rect.String = rect;
		}
	}

	internal class TableBlock : Container {
		public TableBlock(Container mother) : base(mother, Direction.TopDown) { }

		protected bool readAttributes = true;
		protected int spacing = 0;
		protected int padding = 0;
		protected int border = 0;
		protected string bordercolor = "128 128 128";
		protected int rows = 0;
		protected int columns = 0;

		public bool ReadAttributes { get { return readAttributes; } set { readAttributes = value; } }
		public int Spacing { get { return spacing; } set { spacing = value; } }
		public int Padding { get { return padding; } set { padding = value; } }
		public int Border { get { return border; } set { border = value; } }
		public int Rows { get { return rows; } set { rows = value; } }
		public int Columns { get { return columns; } set { columns = value; } }

		public override void RealizeAttributes() {
			base.RealizeAttributes();
			if (readAttributes) {
				spacing = GetAttribute("cellspacing", 2);
				padding = GetAttribute("cellpadding", 1);
				border = GetAttribute("border", 1);
				bordercolor = GetAttributeColor("bordercolor", bordercolor);
			}
		}

		public override void RealizeWidth() {
			base.RealizeWidth();
			int v = GetAttribute("width", -1);
			if (v > 0)
				Width = v;
		}

		public override int Width {
			get {
				if (width != Int32.MinValue)
					return width;
				int v = kids.Count > 0 ? Int32.MinValue : 0;
				foreach (Block child in kids)
					v = Math.Max(child.Width, v); // top-down layout
				return v;
			}
			set {
				TdBlock td = Parent as TdBlock;
				if (td != null) {
					td.ApplySpacing(null);
					value -= (int)td.HorizontalSpace;
				}
				// Here we override the specified width if it is too large
				// for the container. However it would probably be better
				// to scale all the contents instead.
				if ((width == Int32.MinValue) || (width > value)) {
					width = value;
					SetColumnWidths(width);
				}
			}
		}

		public override void Draw(Doc doc, bool chrome) {
			TdBlock td = Parent as TdBlock;
			if (td != null)
				td.ApplySpacing(doc.Rect);
			base.Draw(doc, chrome);
			double spaceTop = (td == null) ? 0 : td.spaceTop;
			double spaceBottom = (td == null) ? 0 : td.spaceBottom;
			double verticalSpace = (td == null) ? 0 : td.VerticalSpace;
			if (chrome) {
				if ((border > 0) || (bgcolor != null)) {
					double contentHeight = Height - verticalSpace;
					if (contentHeight > 0) {
						doc.Rect.Bottom = doc.Rect.Top - contentHeight;
						DrawBackground(doc, bgcolor, border, bordercolor);
					}
				}
			}
			else {
				if (Truncation == TruncationType.Total) {
					Height = 0;
				}
				else {
					int h = Height; // work out table height
					Height = h + (int)Math.Round(verticalSpace);
				}
				#if DEBUG_TABLES
				Debug.Assert(Height < 100000);
				Annotate(doc, 3, String.Format("Table Height = {0}", Height));
				#endif
			}
		}

		public TrBlock AddRow() {
			TrBlock tr = new TrBlock(this, kids.Count);
			kids.Add(tr);
			return tr;
		}

		private void SetColumnWidths(int width) {
			List<int> widths = new List<int>();
			List<int> minWidths = new List<int>();
			// work out number of columns
			foreach (TrBlock tr in kids) {
				for (int i = 0; i < tr.Kids.Count; i++) {
					while (tr.Kids.Count > widths.Count) {
						widths.Add(-1);
						minWidths.Add(-1);
					}
				}
			}
			// work out widths for columns that have specified them
			// we have to take out the spacing element to allow sensible
			// percentage widths to be calculated and then we need to add
			// it back in again as a constant value depending on the cell
			// position
			int totalSpacing = spacing * (widths.Count + 1) + (border * 2);
			int widthNoSpacing = width - totalSpacing;
			foreach (TrBlock tr in kids) {
				for (int i = 0; i < tr.Kids.Count; i++) {
					TdBlock td = tr.Kids[i] as TdBlock;
					if (td != null) {
						int cw = td.GetWidthInPoints(widthNoSpacing);
						if (cw >= 0) { // will be -1 if no width specified
							if ((i == 0) || (i == (widths.Count - 1)))
								cw += (int)Math.Round(spacing * 1.5);
							else
								cw += spacing;
							widths[i] = Math.Max(widths[i], cw);
						}
						if (td.MinWidth > 0) {
							cw = td.MinWidth;
							if ((i == 0) || (i == (widths.Count - 1)))
								cw += (int)Math.Round(spacing * 1.5);
							else
								cw += spacing;
							minWidths[i] = Math.Max(minWidths[i], cw);
						}
					}
				}
			}
			// assign minimum widths and work out total space used up
			int total = 0, count = 0;
			for (int i = 0; i < widths.Count; i++) {
				if (widths[i] > 0) {
					if (widths[i] < minWidths[i])
						widths[i] = minWidths[i];
					total += widths[i];
					count++;
				}
			}
			total += totalSpacing;
			// divide up leftover space between remaining columns with no specified width
			if (count < widths.Count) {
				int colWidth = (width - total) / (widths.Count - count);
				for (int i = 0; i < widths.Count; i++) {
					if (widths[i] <= 0) {
						if ((i == 0) && (widths.Count == 1))
							widths[i] = colWidth + (spacing * 2) + (border * 2);
						if ((i == 0) || (i == (widths.Count - 1)))
							widths[i] = colWidth + (int)Math.Round(spacing * 1.5) + border;
						else
							widths[i] = colWidth + spacing;
					}
				}
			}
			// iterate through columns adding and removing bits until they fit
			while (true) {
				int modifiableColumns = 0;
				int diff = width;
				for (int i = 0; i < widths.Count; i++) {
					if (widths[i] > minWidths[i])
						modifiableColumns++;
					diff -= widths[i];
				}
				if ((diff == 0) || (modifiableColumns == 0))
					break;
				int delta = diff / modifiableColumns;
				if (delta == 0)
					delta = (diff > 0) ? 1 : -1;
				for (int i = 0; i < widths.Count; i++) {
					int w = widths[i] + delta;
					if (w < minWidths[i])
						w = minWidths[i];
					diff -= (w - widths[i]);
					widths[i] = w;
					if (diff == 0)
						break;
				}
			}
			// assign widths to cells
			foreach (TrBlock tr in kids) {
				tr.Width = width;
				for (int i = 0; i < tr.Kids.Count; i++) {
					tr.Kids[i].Width = widths[i];
				}
			}
			// assign extent to table
			rows = kids.Count;
			columns = widths.Count;
		}
	}

	internal class TrBlock : Container {
		public TrBlock(Container mother, int y)
			: base(mother, Direction.LeftRight) {
			this.y = y;
		}

		protected int y;
		//public int y { get { return yp; } set { yp = value; } }

		public override void RealizeAttributes() {
			base.RealizeAttributes();
			border = GetAttribute("border", border);
		}

		protected int border = Int32.MinValue;
		public int Border { get { return (border == Int32.MinValue) ? ((TableBlock)parent).Border : border; } set { border = value; } }

		public TdBlock AddColumn() {
			TdBlock td = new TdBlock(this, kids.Count, y);
			kids.Add(td);
			return td;
		}

		public override void Draw(Doc doc, bool chrome) {
			base.Draw(doc, chrome);
			if (!chrome) {
				if (Truncation == TruncationType.Total) {
					TableBlock t = parent as TableBlock;
					Height = (t == null) ? 0 : t.Border;
				}
				else {
					int h = Height; // work out row height
					Height = h; // fix height of cells
				}
			}
			#if DEBUG_TABLES
			Debug.Assert(Height < 100000);
			Annotate(doc, 2, String.Format("Row {0}, Height = {1}", y, Height));
			#endif
		}
	}

	internal class TdBlock : Container {
		public TdBlock(Container mother, int x, int y)
			: base(mother, Direction.TopDown) {
			this.xp = x;
			this.yp = y;
			spaceLeft = spaceRight = spaceTop = spaceBottom = padding = 0;
		}

		private double hPos = 0.0;
		public double HPos { get { return this.hPos; } }

		private bool isHeader = false;
		public bool IsHeader { get { return isHeader; } set { isHeader = value; }  }

		protected int border = Int32.MinValue;
		public int Border { get { return (border == Int32.MinValue) ? ((TrBlock)parent).Border : border; } set { border = value; } }

		public override void RealizeAttributes() {
			base.RealizeAttributes();
			string align = GetAttribute("align");
			switch (align) {
				case "left": hPos = 0.0; break;
				case "right": hPos = 1.0; break;
				case "center": hPos = 0.5; break;
				case "centre": hPos = 0.5; break;
			}
			border = GetAttribute("border", border);
		}

		public override int MinWidth { 
			get {
				if (base.MinWidth == 0) return 0;
				ApplySpacing(null);
				return base.MinWidth + (int)Math.Round(padding * 2);
			}
		}

		private int xp, yp;
		public double x { get { return this.xp; } }
		public double y { get { return this.yp; } }

		public double spacing, spaceLeft, spaceRight, spaceTop, spaceBottom, padding;
		public double HorizontalSpace { get { return spaceLeft + spaceRight; } }
		public double VerticalSpace { get { return spaceTop + spaceBottom; } }
		public double Spacing { get { return spacing; } } // NB equal to half table cellspacing

		public int GetWidthInPoints(int containerWidth) {
			int v = -1;
			string widthAttribute = GetAttribute("width");
			if ((widthAttribute != null) && (widthAttribute.Length > 0)) {
				if (widthAttribute.EndsWith("%")) {
					Int32.TryParse(widthAttribute.Substring(0, widthAttribute.Length - 1), out v);
					v = (v * containerWidth) / 100;
				}
				else {
					Int32.TryParse(widthAttribute, out v);
				}
			}
			return v;
		}

		public ContentBlock AddContent() {
			ContentBlock cb = new ContentBlock(this);
			kids.Add(cb);
			return cb;
		}

		public TableBlock AddTable() {
			TableBlock table = new TableBlock(this);
			kids.Add(table);
			return table;
		}

		public void ApplySpacing(XRect rect) {
			TableBlock table = (TableBlock)(Parent.Parent);
			spacing = (double)table.Spacing / 2; // half for each adjacent cell
			double border = (double)Border;
			double edgeSpace = border + (double)table.Spacing;
			spaceLeft = (x == 0) ? edgeSpace : spacing;
			spaceRight = (x == (table.Columns - 1)) ? edgeSpace : spacing;
			spaceTop = (y == table.StartKid) ? edgeSpace : spacing;
			spaceBottom = (y == (table.Rows - 1)) ? edgeSpace : spacing;
			padding = table.Padding;
			if (rect != null) {
				if (spaceLeft + spaceRight >= rect.Width)
					spaceRight = rect.Width - spaceLeft;
				if (spaceTop + spaceBottom > rect.Height)
					spaceBottom = rect.Height - spaceTop;
				rect.Left += spaceLeft;
				rect.Right -= spaceRight;
				rect.Top -= spaceTop;
				rect.Bottom += spaceBottom;
			}
		}
	}

	internal class ContentBlock : Block {
		public ContentBlock(Container mother) : base(mother) { }

		private string html = "";
		public string InnerHtml { set { html = value; } get { return html; } }

		private int pid = 0, sanity = 5;
		public bool chainable = false;

		public override void Reset(bool hard) {
			base.Reset(hard);
			height = Int32.MinValue;
			if (hard)
				pid = 0;
		}

		public override void Draw(Doc doc, bool chrome) {
			#if DEBUG_TABLES
			//TdBlock tdb = (TdBlock)Parent;
			//if (Id == "yy")
			//	Debugger.Break();
			//if (html.Contains("500.00"))
			//	Debugger.Break();
			#endif
			TdBlock td = (TdBlock)Parent;
			TrBlock tr = (TrBlock)(td.Parent);
			TableBlock table = (TableBlock)(tr.Parent);
			td.ApplySpacing(doc.Rect);
			int border = td.Border;
			if (chrome) {
				if (truncation != TruncationType.Total) {
					if (td.BgColor != null) bgcolor = td.BgColor;
					if (tr.BgColor != null) bgcolor = tr.BgColor;
					if (border > 0) {
						double contentHeight = Height - td.VerticalSpace;
						doc.Rect.Bottom = doc.Rect.Top - contentHeight;
						td.DrawBackground(doc, bgcolor, 1, "128 128 128");
					}
				}
			}
			else {
				// do padding
				double pad = table.Padding;
				if (border > 0)
					pad += 2;
				doc.Rect.Inset(pad, pad);
				// do drawing
				double y = doc.Pos.Y;
				if ((pid != 0) && (!doc.Chainable(pid))) {
					// we have already finished and we are just filling in blanks
					// so that we match up with other elements of the table row
					truncation = TruncationType.None;
					height = 0;
				}
				else {
					double savedHPos = doc.TextStyle.HPos;
					bool savedBold = doc.TextStyle.Bold;
					doc.TextStyle.HPos = td.HPos;
					doc.TextStyle.Bold = td.IsHeader;
					pid = (pid == 0) ? doc.AddHtml(html) : doc.AddHtml("", pid);
					if (pid == 0)
                        truncation = ((html.Length > 0) && (sanity-- > 0)) ? TruncationType.Total : TruncationType.None;
					else
						truncation = doc.Chainable(pid) ? TruncationType.Partial : TruncationType.None;
					if (truncation != TruncationType.Total) {
                        XRect xr = new XRect(doc.GetInfo(pid, "Rect")); // content without spaces
                        xr.Top = Math.Max(xr.Top, doc.Rect.Top); // include space at top
                        if ((doc.Pos.Y > doc.Rect.Bottom) && (doc.Pos.X > doc.Rect.Left))
                            doc.Pos.Y -= doc.FontSize;
                        xr.Bottom = Math.Min(xr.Bottom, doc.Pos.Y); // and space at bottom
						height = (int)Math.Round(xr.Height + (pad * 2) + td.VerticalSpace);
                        doc.SetInfo(pid, "/TextRect:rect", xr.String); // store for later use
					}
					else {
						height = (int)Math.Round(td.Border + td.Spacing);
					}
					doc.TextStyle.HPos = savedHPos;
					doc.TextStyle.Bold = savedBold;
				}
			}
		}
	}

	internal class HtmlReader : HtmlParser {
        private Size maxImageSize;

		public HtmlReader(string txt, string baseAddress, Size maxImageSize) : base(txt, baseAddress) {
            this.maxImageSize = maxImageSize;
		}

		public TableBlock CreateTable(Dictionary<string, ImageHolder> images) {
			HtmlNode body = ReadHtml();
			body = CheckAndFix(body);
			TableBlock root = CreateTable(images, body, null);
			root.RealizeAttributes();
			root.RealizeWidth();
			return root;
		}

		protected HtmlNode CheckAndFix(HtmlNode node) {
			HtmlNode root = null;
			switch (node.tag) {
				case "html":
				case "body":
					foreach (HtmlNode kid in node.kids)
						root = CheckAndFix(kid);
					if (root == null)
						root = node;
					break;
				case "table":
					foreach (HtmlNode kid in node.kids) {
						if ((kid.tag != "tr") && (kid.tag != "thead") && (kid.tag != "tfoot"))
							throw new Exception(String.Format("Table contains <{0}> tag. Error at character {0}.", kid.tag, pos));
						CheckAndFix(kid);
					}
					break;
				case "thead":
				case "tfoot":
					foreach (HtmlNode kid in node.kids) {
						if (kid.tag != "tr")
							throw new Exception(String.Format("Table contains <{0}> tag. Error at character {0}.", kid.tag, pos));
						CheckAndFix(kid);
					}
					break;
				case "tr":
					foreach (HtmlNode kid in node.kids) {
						if ((kid.tag != "td") && (kid.tag != "th"))
							throw new Exception(String.Format("Table contains <{0}> tag. Error at character {0}.", kid.tag, pos));
					}
					break;
				case "td":
				case "th":
					break;
			}
			return root;
		}

        protected TableBlock CreateTable(Dictionary<string, ImageHolder> images, HtmlNode node, Container block) {
			TableBlock root = null;
			switch (node.tag) {
				case "body" :
					root = new TableBlock(null);
					root.ReadAttributes = false;
					block = root.AddRow().AddColumn();
					if (node.kids.Count > 0) {
						foreach (HtmlNode kid in node.kids)
							CreateTable(images, kid, block);
					}
					else {
						AddContent(node.InnerHtml, (TdBlock)block, images);
					}
					break;
				case "table":
					block = ((TdBlock)block).AddTable();
					block.attributes = node.attributes;
					foreach (HtmlNode kid in node.kids)
						CreateTable(images, kid, block);
					break;
				case "thead":
					if (node.kids.Count > 0) {
						TableBlock tb = (TableBlock)block;
						if (tb.Header == 0) // don't add more than one header
							tb.Header = node.kids.Count;
						foreach (HtmlNode kid in node.kids)
							CreateTable(images, kid, tb);
					}
					break;
				case "tfoot":
					if (node.kids.Count > 0) {
						TableBlock tb = (TableBlock)block;
						if (tb.Footer == 0) // don't add more than one footer
							tb.Footer = node.kids.Count;
						foreach (HtmlNode kid in node.kids)
							CreateTable(images, kid, tb);
					}
					break;
				case "tr":
					if (node.kids.Count > 0) {
						block = ((TableBlock)block).AddRow();
						block.attributes = node.attributes;
						foreach (HtmlNode kid in node.kids)
							CreateTable(images, kid, block);
					}
					break;
				case "td":
				case "th":
					block = ((TrBlock)block).AddColumn();
					block.attributes = node.attributes;
					if (node.kids.Count > 0) {
						foreach (HtmlNode kid in node.kids)
							CreateTable(images, kid, block);
					}
					else {
						AddContent(node.InnerHtml, (TdBlock)block, images);
						((TdBlock)block).IsHeader = (node.tag == "th");
					}
					break;
			}
			return root;
		}

		private void AddContent(string txt, TdBlock td, Dictionary<string, ImageHolder> images) {
			ContentBlock cb = td.AddContent();
            ImgReader reader = new ImgReader(txt, baseAddress, maxImageSize);
			int contentMinWidth = 0;
			cb.InnerHtml = reader.Encode(images, out contentMinWidth);
			if (contentMinWidth > 0)
				td.MinWidth = contentMinWidth;
		}
	}

	internal class ImgReader : HtmlParser {
        private Size maxImageSize;

		public ImgReader(string txt, string baseAddress, Size maxImageSize) : base(txt, baseAddress) {
            this.maxImageSize = maxImageSize;
        }

		public string Encode(Dictionary<string, ImageHolder> images, out int maxWidth) {
			maxWidth = 0;
			StringBuilder sb = new StringBuilder(len);
			int start = 0;
			while (true) {
				string tag = FindTag();
				if (tag == null)
					break;
				switch (tag) {
					case "img":
						// get the information out of the html
						string src = "";
						int width = -1, height = -1;
						string name, value;
						while (FindAttribute(out name, out value)) {
							switch (name.ToLower()) {
								case "src":
									src = value;
									break;
								case "width":
									width = StringToInt(value, width);
									break;
								case "height":
									height = StringToInt(value, height);
									break;
							}
						}
						// get the images
						ImageHolder xi;
						if (images.ContainsKey(src))
							xi = images[src];
						else {
							xi = GetImageFromUri(src);
							images[src] = xi;
						}
						if (width < 0) {
							double res = (xi.image.HRes > 0) ? xi.image.HRes : 96.0;
							width = (int)Math.Round((xi.image.Width * 72.0) / res);
						}
						if (height < 0) {
							double res = (xi.image.VRes > 0) ? xi.image.VRes : 96.0;
							height = (int)Math.Round((xi.image.Height * 72.0) / res);
						}
                        // limit physical dimensions of images
                        if ((maxImageSize.Width > 0) && (width > maxImageSize.Width)) {
                            height = (int)(((double)maxImageSize.Width / (double)width) * (double)height);
                            width = maxImageSize.Width;
                        }
                        if ((maxImageSize.Height > 0) && (height > maxImageSize.Height)) {
                            width = (int)(((double)maxImageSize.Height / (double)height) * (double)width);
                            height = maxImageSize.Height;
                        }
						maxWidth = Math.Max(maxWidth, width);
						// move to next html
						string run = String.Format("<stylerun fixedwidth={0} fontsize={1} annots=\'link:{2}\'>&nbsp;</stylerun>", width, height, HttpUtility.HtmlEncode(src));
						SwapTag(sb, run, ref start);
						break;
					case "center":
						SwapTag(sb, "<p align=center spacebefore=0 spaceafter=0>", ref start);
						break;
					case "/center":
						SwapTag(sb, "</p>", ref start);
						break;
					case "p":
						SkipBackTo('<');
						sb.Append(html.Substring(start, pos - start));
						SkipUntilAfter('>');
						start = pos;
						break;
					case "/p":
						SwapTag(sb, "<br>", ref start);
						break;
					case "strong":
						SwapTag(sb, "<b>", ref start);
						break;
					case "/strong":
						SwapTag(sb, "</b>", ref start);
						break;
				}
			}
			sb.Append(html.Substring(start, len - start));
			return sb.ToString();
		}

		void SwapTag(StringBuilder sb, string tag, ref int start) {
			SkipBackTo('<');
			sb.Append(html.Substring(start, pos - start));
			sb.Append(tag);
			SkipUntilAfter('>');
			start = pos;
		}

		ImageHolder GetImageFromUri(string uri) {
			ImageHolder ih = null;
			try {
				using (WebClient wc = new WebClient()) {
					wc.BaseAddress = baseAddress;
					byte[] data = wc.DownloadData(uri);
					XReadOptions xr = new XReadOptions();
					xr.PreserveTransparency = true;
					XImage image = XImage.FromData(data, xr);
					ih = new ImageHolder(image, data);
				}
			}
			catch { // create placeholder
				MemoryStream ms = new MemoryStream();
				using (Bitmap bm = new Bitmap(100, 20)) {
					using (Graphics gr = Graphics.FromImage(bm)) {
						gr.FillRectangle(new SolidBrush(Color.Cyan), 0, 0, bm.Width, bm.Height);
						gr.DrawString("Missing", new Font("Verdana", bm.Height, GraphicsUnit.Pixel), new SolidBrush(Color.Black), (float)0, (float)(bm.Height * -0.2));
					}
					bm.Save(ms, ImageFormat.Png);
				}
				XImage image = XImage.FromStream(ms, null);
				ih = new ImageHolder(image, ms.GetBuffer());
			}
			return ih;
		}
	}

	internal abstract class HtmlParser {
		protected class HtmlNode {
			public string tag = null, html = null;
			public List<HtmlNode> kids = null;
			public HtmlNode parent = null;
			public int start = 0, end = 0;
			public Dictionary<string, string> attributes = null;

			public HtmlNode(string tag, string html, HtmlNode parent) {
				this.tag = tag.ToLower();
				this.html = html;
				this.parent = parent;
				if (this.parent != null)
					this.parent.kids.Add(this);
				kids = new List<HtmlNode>();
			}

			public void AddAttribute(string name, string value) {
				if (attributes == null)
					attributes = new Dictionary<string, string>();
				if (!attributes.ContainsKey(name))
					attributes.Add(name, value);
			}

			public string InnerHtml { get { return html.Substring(start, end - start); } }
		}

		protected string html = null;
		protected string baseAddress = null;
		protected int pos = 0;
		protected int len = 0;

		public HtmlParser(string txt, string uri) {
			html = txt;
			len = txt.Length;
			pos = 0;
			baseAddress = uri;
		}

		protected HtmlNode ReadHtml() {
			HtmlNode root = new HtmlNode("body", html, null);
			root.end = html.Length;
			HtmlNode current = root;
			while (true) {
				string tag = FindTag();
				if (tag == null)
					break;
				switch (tag) {
					case "html":
					case "body":
					case "table":
					case "thead":
					case "tfoot":
					case "tr":
					case "td":
					case "th":
						current = new HtmlNode(tag, html, current);
						AddAttributes(current);
						current.start = pos;
						break;
					case "/html":
					case "/body":
					case "/table":
					case "/thead":
					case "/tfoot":
					case "/tr":
					case "/td":
					case "/th":
						if (current.parent == null)
							throw new Exception(String.Format("Tag <{0}> not matched by corresponding open tag. Error at character {1}.", tag, pos));
						current.end = html.LastIndexOf('<', pos);
						current = current.parent;
						break;
				}
			}
			if (current != root)
				throw new Exception(String.Format("Tag <{0}> unterminated at end of stream", current.tag));
			return root;
		}

		private void AddAttributes(HtmlNode node) {
			string name, value;
			while (FindAttribute(out name, out value))
				node.AddAttribute(name.ToLower(), value);
			SkipUntilAfter('>');
		}

		protected bool FindAttribute(out string name, out string value) {
			bool ok = false;
			SkipWhite();
			name = ReadName();
			value = null;
			if (name.Length > 0) {
				SkipWhite();
				if (html[pos] == '=') {
					pos++;
					value = ReadName();
					ok = true;
				}
			}
			return ok;
		}

		protected int StringToInt(string str, int def) {
			int v;
			return Int32.TryParse(str, out v) ? v : def;
		}

		protected string FindTag() {
			int s = html.IndexOf('<', pos);
			if (s < 0)
				return null;
			pos = s + 1;
			SkipWhite();
			return ReadName().ToLower();
		}

		protected void SkipWhite() {
			while ((pos < len) && (Char.IsWhiteSpace(html[pos])))
				pos++;
		}

		protected string ReadName() {
			bool quoted = (html[pos] == '\"') || (html[pos] == '\'');
			if (quoted) pos++;
			int start = pos;
			if (quoted) {
				while (pos < len) {
					if ((html[pos] == '\"') || (html[pos] == '\''))
						break;
					pos++;
				}
			}
			else {
				while (pos < len) {
					char c = html[pos];
					if ((Char.IsWhiteSpace(c)) || (c == '=') || (c == '>'))
						break;
					pos++;
				}
			}
			string name = html.Substring(start, pos - start);
			if (quoted)
				pos++;
			return name;
		}

		protected bool SkipUntilTag(string search) {
			while (true) {
				string tag = FindTag();
				if (tag == null)
					return false;
				if (tag == search) {
					pos = html.LastIndexOf('<', pos);
					return true;
				}
			}
		}

		protected void SkipUntilAfter(char c) {
			char t;
			while (pos < len) {
				t = html[pos];
				pos++;
				if (t == c)
					break;
			}
		}

		protected void SkipUntil(char c) {
			int p = html.IndexOf(c, pos);
			pos = (p >= 0) ? p : html.Length;
		}

		protected bool SkipBackTo(char c) {
			int p = html.LastIndexOf(c, pos);
			if (p < 0) return false;
			pos = p;
			return true;
		}
	}
}

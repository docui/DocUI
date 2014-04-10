using Org.Filedrops.FileSystem;
using Org.Filedrops.FileSystem.UI;
using Org.Filedrops.FileSystem.UI.Listview;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Org.DocUI.Wpf
{
    /// <summary>
    /// Interaction logic for NewFilePanel.xaml
    /// </summary>
    public partial class NewFilePanel : Window
    {		

		public static Dictionary<string, string[]> Typedict;
		private static Dictionary<string, BitmapImage> _icons;
		public string NewPath;
		public FiledropsFileSystemEntry NewType;
		public bool Succes { get; set; }
		private string _folderpath;
		private readonly FiledropsFileList _lv;
		private string _ext;
        public GetIcon IconFunction { get; set; }

		static NewFilePanel()
		{
			Typedict = new Dictionary<string, string[]>();			
		}


		public NewFilePanel(FiledropsDirectory folder, FiledropsDirectory schemadir, string ext, GetIcon iconFunction)
		{
			InitializeComponent();
            this.IconFunction = iconFunction;
            this._ext = ext;
            this._folderpath = folder.FullName;

            _lv = new FiledropsFileList(schemadir, false, false, ".xsd");
            _lv.ShowFiles = true;

			Button b = new Button();
			b.Content = "Create file";
			b.Click += CreateFileClick;
			b.Margin = new System.Windows.Thickness(10);

            _lv.MouseDoubleClick += CreateFileClick;


			Grid.SetRow(_lv, 0);
			Grid.SetRow(b, 1);

            grid.Children.Add(_lv);
            grid.Children.Add(b);
		}


		public void CreateFileClick(object sender, EventArgs args)
		{
            FiledropsFileSystemEntry entry = this._lv.SelectedItem;
			if (entry != null)
			{                
				this.NewType = entry;
                this.DialogResult = true;
                this.Close();
			}
			else
			{
				if (entry != null)
				{
					_lv.BorderBrush = Brushes.Black;
				}
				else
				{                    
					_lv.BorderBrush = Brushes.Crimson;
				}				
			}
		}

		class ItemPanel : StackPanel
		{
			private string _type;

			public ItemPanel(string name)
			{
				this._type = name;
				this.Orientation = System.Windows.Controls.Orientation.Horizontal;
				this.VerticalAlignment = System.Windows.VerticalAlignment.Center;
				BitmapImage bm;
				if (_icons.TryGetValue(name, out bm))
				{
					Image icon = new Image();
					icon.Source = bm;
					icon.Width = 40;
					icon.Margin = new System.Windows.Thickness(15);
					this.Children.Add(icon);
				}

				TextBlock tb = new TextBlock() { FontSize = 15, Margin = new Thickness(0,5,0,0) };
				tb.Text = name;
				this.Children.Add(tb);
			}
		}
    }
}

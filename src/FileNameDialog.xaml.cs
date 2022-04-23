using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CleanArchitecture.CodeGenerator
{
	public class CheckListItem
	{
		public string TheText { get; set; }
		public bool IsSelected { get; set; }
	}


	public partial class FileNameDialog : Window, INotifyPropertyChanged
	{

		List<CheckListItem> checkList;
		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, e);
			}
		}

		private const string DEFAULT_TEXT = "Select a entity name";
		private static readonly List<string> _tips = new List<string> {
	  	"Tip: CQRS stands for Command/Query Responsibility Segregation, and it's a wonderful thing",
			"Tip: All business logic is in a use case",
			"Tip: Good monolith with clear use cases that you can split in microservices later on, once you’ve learned more about them ",
			"Tip: CI/CD processes and solutions help to generate more value for the end-users of software",
			"Tip: the architecture is decoupled from the underlying data store",
		  "Tip: An effective testing strategy that follows the testing pyramid",
		};

		//Property to bind to
		public List<CheckListItem> CheckList {
			get { return checkList; }
			set { checkList = value; this.OnPropertyChanged(new PropertyChangedEventArgs("CheckList")); }
		}

		//Test data helper
		List<CheckListItem> GenerateTestData()
		{
			List<CheckListItem> checkListItems = new List<CheckListItem>();
			checkListItems.Add(new CheckListItem { TheText = "Create", IsSelected = false });
			checkListItems.Add(new CheckListItem { TheText = "Update", IsSelected = false });
			checkListItems.Add(new CheckListItem { TheText = "Delete", IsSelected = false });
			checkListItems.Add(new CheckListItem { TheText = "GetAll", IsSelected = false });
			checkListItems.Add(new CheckListItem { TheText = "GetById", IsSelected = false });
			checkListItems.Add(new CheckListItem { TheText = "GetAllWithPagination", IsSelected = false });
			return checkListItems;
		}

		public FileNameDialog(string folder,string[] entities)
		{
			InitializeComponent();
			CheckList = GenerateTestData();

			lblFolder.Content = string.Format("{0}/", folder);
			foreach(var item in entities)
			{
				selectName.Items.Add(item);
			}
			selectName.Text = DEFAULT_TEXT;
			selectName.SelectionChanged += (s,e) => {
				btnCreate.IsEnabled = true;
			};
				Loaded += (s, e) =>
			{
				Icon = BitmapFrame.Create(new Uri("pack://application:,,,/CleanArchitectureCodeGenerator;component/Resources/icon.png", UriKind.RelativeOrAbsolute));
				Title = Vsix.Name;


				//txtName.Focus();
				//txtName.CaretIndex = 0;
				//txtName.Text = DEFAULT_TEXT;
				//txtName.Select(0, txtName.Text.Length);

				//txtName.PreviewKeyDown += (a, b) =>
				//{
				//	if (b.Key == Key.Escape)
				//	{
				//		if (string.IsNullOrWhiteSpace(txtName.Text) || txtName.Text == DEFAULT_TEXT)
				//		{
				//			Close();
				//		}
				//		else
				//		{
				//			txtName.Text = string.Empty;
				//		}
				//	}
				//	else if (txtName.Text == DEFAULT_TEXT)
				//	{
				//		txtName.Text = string.Empty;
				//		btnCreate.IsEnabled = true;
				//	}
				//};

			};
		}

		public string Input => selectName.SelectedItem?.ToString();

		

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Close();
		}

		private void listExtraSkills_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{

		}

		private void selectName_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{

		}
	}
}

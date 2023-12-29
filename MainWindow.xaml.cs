using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;
using Mandelbrot_Set.MVVM.ViewModel;

namespace Mandelbrot_Set
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml.
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			DataContext = new MainViewModel(MandelbrotImage);
		}

		/// <summary>
		/// Передвигает окно.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Header_MouseDown(object sender, MouseButtonEventArgs e)
		{
			DragMove();
		}

		/// <summary>
		/// Закрывает окно.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CloseWindowButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		/// <summary>
		/// Развернуть окно.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MaxWindowButton_Click(object sender, RoutedEventArgs e)
		{
			WindowState = (WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized);
		}

		/// <summary>
		/// Свернуть окно.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MinWindowButton_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Minimized;
		}
	}
}
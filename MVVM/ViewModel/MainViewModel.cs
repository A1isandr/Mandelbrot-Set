using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using Mandelbrot_Set.MVVM.Model;

namespace Mandelbrot_Set.MVVM.ViewModel
{
	internal class MainViewModel : INotifyPropertyChanged
	{
		private RelayCommand? resetCommand;

		private Visibility labelVisibility;

		public Visibility LabelVisibility
		{
			get => labelVisibility; 
			set
			{
				labelVisibility = value;
				OnPropertyChanged(nameof(LabelVisibility));
			}
		}

		private MandelbrotSet mandelbrotSet;
		/// <summary>
		/// Возвращает модель множества Мандельброта.
		/// </summary>
		public MandelbrotSet MandelbrotSet 
		{ 
			get => mandelbrotSet;
			set
			{
				mandelbrotSet = value;
				OnPropertyChanged(nameof(MandelbrotSet));
			}
		}

		public MainViewModel(Image mandelbrotImage)
		{
			MandelbrotSet = new();

			LabelVisibility = Visibility.Collapsed;

			mandelbrotImage.MouseLeftButtonDown += async (s, e) =>
			{
				if (!MandelbrotSet.IsZooming)
				{
					LabelVisibility = Visibility.Visible;

					var mouseX = e.GetPosition(mandelbrotImage).X;
					var mouseY = e.GetPosition(mandelbrotImage).Y;

					MandelbrotSet.IsZooming = true;
					await MandelbrotSet.DrawOnCoordsAsync(mouseX, mouseY);
				}
				else
				{
					LabelVisibility = Visibility.Collapsed;

					MandelbrotSet.IsZooming = false;
				}
			};
		}

		/// <summary>
		/// Сбрасывает множество Мандельброта.
		/// </summary>
		public RelayCommand? ResetCommand
		{
			get
			{
				return resetCommand ??= new RelayCommand((e) =>
				{
					LabelVisibility = Visibility.Collapsed;

					MandelbrotSet.IsZooming = false;
					MandelbrotSet = new();
				});
			}
		}


		public event PropertyChangedEventHandler? PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string name = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}


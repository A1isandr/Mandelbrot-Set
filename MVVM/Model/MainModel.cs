using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Mandelbrot_Set.MVVM.Model
{
	internal class MandelbrotSet : INotifyPropertyChanged
	{
		private WriteableBitmap mandelbrotBitmap;
		/// <summary>
		/// Карта изображения Мандельброта.
		/// </summary>
		public WriteableBitmap MandelbrotBitmap 
		{
			get => mandelbrotBitmap;
			private set
			{
				mandelbrotBitmap = value;
				
			}
		}

		/// <summary>
		/// Максимальное число итераций.
		/// </summary>
		private const int MAX_ITERATIONS = 1000;

		/// <summary>
		/// Коэффициент увеличения масштаба.
		/// </summary>
		private const double ZOOM_FACTOR = 1.1;

		/// <summary>
		/// Границы множества Мандельброта по оси X.
		/// </summary>
		private double minX = -2.5, maxX = 2.5;
		/// <summary>
		/// Границы множества Мандельброта по оси Y.
		/// </summary>
		private double minY = -2.0, maxY = 2.0;

		/// <summary>
		/// Ширина изображения.
		/// </summary>
		private int imageWidth = 1000;
		/// <summary>
		/// Высота изображения.
		/// </summary>
		private int imageHeight = 700;

		/// <summary>
		/// Ширина одного блока.
		/// </summary>
		private int BlockWidth
		{
			get => imageWidth / 4;
		}

		/// <summary>
		/// Высота одного блока.
		/// </summary>
		private int BlockHeight
		{
			get => imageHeight / 4;
		}

		/// <summary>
		/// Возвращает статус масштабирования.
		/// </summary>
		public bool IsZooming {get; set;}

		private double scale = 1.0;
		/// <summary>
		/// Возвращает текущий масштаб.
		/// </summary>
		public double Scale
		{
			get => Math.Round(scale, 1);
			set
			{
				scale = value;
				OnPropertyChanged(nameof(Scale));
			}
		}

		/// <summary>
		/// Представляет собой количество байтов в строке изображения. 
		/// </summary>
		public int Stride 
		{
			// В данном случае, каждый пиксель представлен четырьмя байтами (по одному для каждого канала: Blue, Green, Red, и Alpha),
			// поэтому мы умножаем ширину изображения на 4.
			get => imageWidth * 4;
		}
	
		/// <summary>
		/// Представляет собой общее количество байтов в массиве pixels.
		/// </summary>
		public int DataSize
		{
			// Равно количеству байтов в каждой строке, умноженному на количество строк.
			get => Stride * imageHeight;
		}
		
		public MandelbrotSet()
		{
			MandelbrotBitmap = new(imageWidth, imageHeight, 96, 96, PixelFormats.Bgr32, null);

			var result = Draw(Stride, DataSize);
			MandelbrotBitmap.WritePixels(new Int32Rect(0, 0, imageWidth, imageHeight), result.Item1, result.Item2, 0);
		}

		/// <summary>
		/// Используется для масштабирования значения из одного диапазона в другой.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="fromlow"></param>
		/// <param name="fromhigh"></param>
		/// <param name="tolow"></param>
		/// <param name="tohigh"></param>
		/// <returns></returns>
		private static double Map(double value, double fromlow, double fromhigh, double tolow, double tohigh)
		{
			return tolow + (value - fromlow) * (tohigh - tolow) / (fromhigh - fromlow);
		}

		/// <summary>
		/// Присваивает цвет пикселя в зависимости от количества итераций.
		/// </summary>
		/// <param name="iterations"></param>
		/// <returns></returns>
		private (byte, byte, byte, byte) Paint(int iterations)
		{
			if (iterations <= 512)
			{
				if (iterations <= 256)
				{
					return (0, 0, (byte)(iterations % 256), 255);
				}
				else
				{
					return (0, (byte)(iterations % 256), 255, 255);
				}
			}
            else
            {
				if (iterations <= 768)
				{
					return ((byte)(iterations % 256), 100, 255, 255);
				}
				else if (iterations == 1000)
				{
					return (0, 0, 0, 255);
				}
				else
				{
					return (255, 100, (byte)(iterations % 256), 255);
				}
            }
        }

		/// <summary>
		/// Рисует множество Мандельброта.
		/// </summary>
		/// <param name="stride"></param>
		/// <param name="dataSize"></param>
		/// <returns></returns>
		public (byte[], int) Draw(int stride, int dataSize)
		{
			byte[] pixels = new byte[dataSize];

			// Определение количества параллельных задач
			int numTasks = Environment.ProcessorCount;

			Parallel.For(0, imageWidth, x =>
			{
				for (int y = 0; y < imageHeight; y++)
				{
					double a = Map(x, 0, imageWidth, minX, maxX);
					double b = Map(y, 0, imageHeight, minY, maxY);

					double realPart = a;
					double imaginaryPart = b;

					int iteration = 0;
					while (iteration < MAX_ITERATIONS)
					{
						double tempReal = realPart * realPart - imaginaryPart * imaginaryPart + a;
						double tempImaginary = 2 * realPart * imaginaryPart + b;

						realPart = tempReal;
						imaginaryPart = tempImaginary;

						if (realPart * realPart + imaginaryPart * imaginaryPart > 4)
							break;

						iteration++;
					}

					int index = y * stride + 4 * x;

					(pixels[index], pixels[index + 1], pixels[index + 2], pixels[index + 3]) = Paint(iteration);
				}
			});

			return (pixels, stride);
		}

		/// <summary>
		/// Ассинхронно циклично рисует множество Мандельброта по заданным координатам.
		/// </summary>
		/// <param name="rawMouseX"></param>
		/// <param name="rawMouseY"></param>
		/// <returns></returns>
		async public Task DrawOnCoordsAsync(double rawMouseX, double rawMouseY)
		{
			double mouseX = Map((double)rawMouseX, 0, imageWidth, minX, maxX);
			double mouseY = Map((double)rawMouseY, 0, imageHeight, minY, maxY);

			while (IsZooming)
			{
				double newWidth = (maxX - minX) / (double)ZOOM_FACTOR;
				double newHeight = (maxY - minY) / (double)ZOOM_FACTOR;

				minX = mouseX - newWidth / 2;
				maxX = mouseX + newWidth / 2;
				minY = mouseY - newHeight / 2;
				maxY = mouseY + newHeight / 2;

				Scale *= (double)ZOOM_FACTOR;
				var result = await Task.Run(() => Draw(Stride, DataSize));

				MandelbrotBitmap.WritePixels(new Int32Rect(0, 0, imageWidth, imageHeight), result.Item1, result.Item2, 0);
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string name = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}

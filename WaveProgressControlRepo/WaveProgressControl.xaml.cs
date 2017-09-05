using System;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;

namespace WaveProgressControlRepo
{
	public sealed partial class WaveProgressControl : UserControl
	{
		private readonly Compositor _compositor;
		private readonly CompositionPropertySet _percentPropertySet;

		public WaveProgressControl()
		{
			InitializeComponent();

			_compositor = Window.Current.Compositor;

			_percentPropertySet = _compositor.CreatePropertySet();
			_percentPropertySet.InsertScalar("Value", 0.0f);

			Loaded += OnLoaded;
		}

		public double Percent
		{
			get => (double)GetValue(PercentProperty);
			set => SetValue(PercentProperty, value);
		}
		public static readonly DependencyProperty PercentProperty =
			DependencyProperty.Register("Percent", typeof(double), typeof(WaveProgressControl),
				new PropertyMetadata(0.0d, (s, e) =>
				{
					var self = (WaveProgressControl)s;
					var propertySet = self._percentPropertySet;
					propertySet.InsertScalar("Value", Convert.ToSingle(e.NewValue) / 100);
				}));

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			CompositionSurfaceBrush imageSurfaceBrush;

			SetupClippedWaveImage();
			SetupEndlessWaveAnimationOnXAxis();
			SetupExpressionAnimationOnYAxisBasedOnPercentValue();

			void SetupClippedWaveImage()
			{
				// Note LoadedImageSurface is only available in 15063 onward.
				var imageSurface = LoadedImageSurface.StartLoadFromUri(new Uri(BaseUri, "/Assets/wave.png"));
				imageSurfaceBrush = _compositor.CreateSurfaceBrush(imageSurface);
				imageSurfaceBrush.Stretch = CompositionStretch.None;
				imageSurfaceBrush.Offset = new Vector2(120, 248);

				var maskBrush = _compositor.CreateMaskBrush();
				var maskSurfaceBrush = ClippedImageContainer.GetAlphaMask(); // CompositionSurfaceBrush
				maskBrush.Mask = maskSurfaceBrush;
				maskBrush.Source = imageSurfaceBrush;

				var imageVisual = _compositor.CreateSpriteVisual();
				imageVisual.RelativeSizeAdjustment = Vector2.One;
				ElementCompositionPreview.SetElementChildVisual(ClippedImageContainer, imageVisual);

				imageVisual.Brush = maskBrush;
			}

			void SetupEndlessWaveAnimationOnXAxis()
			{
				var waveOffsetXAnimation = _compositor.CreateScalarKeyFrameAnimation();
				waveOffsetXAnimation.InsertKeyFrame(1.0f, -80.0f, _compositor.CreateLinearEasingFunction());
				waveOffsetXAnimation.Duration = TimeSpan.FromSeconds(1);
				waveOffsetXAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
				imageSurfaceBrush.StartAnimation("Offset.X", waveOffsetXAnimation);
			}

			void SetupExpressionAnimationOnYAxisBasedOnPercentValue()
			{
				var waveOffsetYExpressionAnimation = _compositor.CreateExpressionAnimation("Lerp(248.0f, 120.0f, Percent.Value)");
				waveOffsetYExpressionAnimation.SetReferenceParameter("Percent", _percentPropertySet);
				imageSurfaceBrush.StartAnimation("Offset.Y", waveOffsetYExpressionAnimation);
			}
		}
	}
}

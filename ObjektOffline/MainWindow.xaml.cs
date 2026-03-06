using Microsoft.ML.Data;
using Microsoft.Win32;
using MLTrain2;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ObjektOffline
{
    public partial class MainWindow : Window
    {
        private string selectedImagePath;
        private MLModel1.ModelOutput _lastPrediction;

        public MainWindow()
        {
            InitializeComponent();

            MainImage.SizeChanged += MainImage_SizeChanged;
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Images (*.jpg;*.png)|*.jpg;*.png";

            if (dlg.ShowDialog() == true)
            {
                selectedImagePath = dlg.FileName;
                MainImage.Source = new BitmapImage(new Uri(selectedImagePath));
                OverlayCanvas.Children.Clear();
                PredictionsList.Items.Clear();
            }
        }

        private void Predict_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedImagePath))
                return;

            var input = new MLModel1.ModelInput()
            {
                Image = MLImage.CreateFromFile(selectedImagePath)
            };

            _lastPrediction = MLModel1.Predict(input);

            DrawBoxes(_lastPrediction);
        }

        private void ConfidenceSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_lastPrediction != null)
            {
                DrawBoxes(_lastPrediction);
            }
        }

        private void DrawBoxes(MLModel1.ModelOutput prediction)
        {
            if (MainImage.Source is not BitmapSource bitmapSource)
                return;

            OverlayCanvas.Children.Clear();
            PredictionsList.Items.Clear();

            float threshold = (float)ConfidenceSlider.Value;

            double imageWidth = bitmapSource.PixelWidth;
            double imageHeight = bitmapSource.PixelHeight;
           
            double controlWidth = OverlayCanvas.ActualWidth;
            double controlHeight = OverlayCanvas.ActualHeight;

            if (controlWidth <= 0 || controlHeight <= 0)
                return;
            
            double ratioX = controlWidth / imageWidth;
            double ratioY = controlHeight / imageHeight;
            double ratio = Math.Min(ratioX, ratioY);

            double renderedWidth = imageWidth * ratio;
            double renderedHeight = imageHeight * ratio;

            double offsetX = (controlWidth - renderedWidth) / 2.0;
            double offsetY = (controlHeight - renderedHeight) / 2.0;

            int count = prediction.Score?.Length ?? 0;

            for (int i = 0; i < count; i++)
            {
                float score = prediction.Score[i];
                if (score < threshold)
                    continue;

                int boxOffset = i * 4;
                if (prediction.PredictedBoundingBoxes == null || boxOffset + 3 >= prediction.PredictedBoundingBoxes.Length)
                    continue;

                float xMin = prediction.PredictedBoundingBoxes[boxOffset + 0];
                float yMin = prediction.PredictedBoundingBoxes[boxOffset + 1];
                float xMax = prediction.PredictedBoundingBoxes[boxOffset + 2];
                float yMax = prediction.PredictedBoundingBoxes[boxOffset + 3];
               
                double boxX = xMin;
                double boxY = yMin;
                double boxW = xMax - xMin;
                double boxH = yMax - yMin;
                
                double left = boxX * ratio + offsetX;
                double top = boxY * ratio + offsetY;
                double width = boxW * ratio;
                double height = boxH * ratio;
               
                var rect = new System.Windows.Shapes.Rectangle
                {
                    Width = width,
                    Height = height,
                    Stroke = System.Windows.Media.Brushes.Red,
                    StrokeThickness = 3
                };

                System.Windows.Controls.Canvas.SetLeft(rect, left);
                System.Windows.Controls.Canvas.SetTop(rect, top);
                OverlayCanvas.Children.Add(rect);
                
                string label = (prediction.PredictedLabel != null && i < prediction.PredictedLabel.Length)
                    ? prediction.PredictedLabel[i]
                    : "Unknown";

                string text = $"{label} {score * 100:F1}%";

                var textBlock = new System.Windows.Controls.TextBlock
                {
                    Text = text,
                    Foreground = System.Windows.Media.Brushes.White,
                    FontWeight = FontWeights.Bold,
                    FontSize = 14,
                    Padding = new Thickness(6, 2, 6, 2),
                    Background = new SolidColorBrush(Color.FromArgb(190, 220, 0, 0)) 
                };
               
                double labelLeft = left;
                double labelTop = top - 26;
                if (labelTop < 0) labelTop = top + 2;

                Canvas.SetLeft(textBlock, labelLeft);
                Canvas.SetTop(textBlock, labelTop);
                OverlayCanvas.Children.Add(textBlock);

                PredictionsList.Items.Add(text);
            }
        }

        private void MainImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_lastPrediction != null)
            {
                DrawBoxes(_lastPrediction);
            }
        }
    }
}
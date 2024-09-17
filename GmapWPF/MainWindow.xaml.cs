using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using MusteriTakipWithElasticSearch.Elastic;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GmapWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    public partial class MainWindow : Window
    {
        private List<PointLatLng> _polygonPoints = new List<PointLatLng>();
        private GMapPolygon _currentPolygon;
        private GMapMarker _selectedMarker;
        private bool _isDragging;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
         
            gmapControl.MapProvider = GMapProviders.OpenStreetMap;
            gmapControl.Position = new PointLatLng(39.91987, 32.85427);
            gmapControl.MinZoom = 2;
            gmapControl.MaxZoom = 18;
            gmapControl.Zoom = 15;
            gmapControl.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
            gmapControl.CanDragMap = true;
            gmapControl.DragButton = MouseButton.Right;
        }

        private void gmapControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(gmapControl);
            var latLng = gmapControl.FromLocalToLatLng((int)point.X, (int)point.Y);

            // Check if a marker was clicked
            foreach (var marker in gmapControl.Markers.OfType<GMapMarker>())
            {
                var localPosition = gmapControl.FromLatLngToLocal(marker.Position);
                var rect = new Rect(localPosition.X - 5, localPosition.Y - 5, 10, 10);
                if (rect.Contains(point))
                {
                    _selectedMarker = marker;
                    _isDragging = true;
                    return;
                }
            }

            // Add a new marker if no existing marker was clicked
            _polygonPoints.Add(latLng);
            AddMarker(latLng);

            UpdatePolygon();
        }

        private void gmapControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && _selectedMarker != null)
            {
                var point = e.GetPosition(gmapControl);
                var latLng = gmapControl.FromLocalToLatLng((int)point.X, (int)point.Y);
                _selectedMarker.Position = latLng;

                // Update polygon points
                var index = _polygonPoints.IndexOf(_selectedMarker.Position);
                if (index >= 0)
                {
                    _polygonPoints[index] = latLng;
                    UpdatePolygon();
                }
            }
        }

        private void gmapControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            _selectedMarker = null;
        }

        private void AddMarker(PointLatLng point)
        {
            var marker = new GMapMarker(point)
            {
                Shape = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Stroke = Brushes.Red,
                    StrokeThickness = 2,
                    Fill = Brushes.Red
                }
            };
            gmapControl.Markers.Add(marker);
        }

        private void UpdatePolygon()
        {
            if (_currentPolygon != null)
            {
                gmapControl.Markers.Remove(_currentPolygon);
            }

            if (_polygonPoints.Count >= 2)
            {
                _currentPolygon = new GMapPolygon(_polygonPoints);
                //{
                //    Stroke = new Pen(Brushes.Blue, 2),
                //    Fill = new SolidColorBrush(Color.FromArgb(50, 0, 0, 255)),
                //};

                gmapControl.Markers.Add(_currentPolygon);
            
            }
        }

        private void RemoveLastPoint_Click(object sender, RoutedEventArgs e)
        {
            if (_polygonPoints.Count > 0) // point varsa 
            {
                _polygonPoints.RemoveAt(_polygonPoints.Count - 1); // son indextekini at
                gmapControl.Markers.RemoveAt(_polygonPoints.Count); // son noktayı da kaldır
                UpdatePolygon();
            }
        }

        private void ListPoints_Click(object sender, RoutedEventArgs e)
        {
            if (_polygonPoints.Count > 2) // belirli bir alan içinde olmalı.
            {
                var pointsList = string.Join(Environment.NewLine, _polygonPoints.Select(p => $"Lat: {p.Lat}, Lng: {p.Lng}"));
                ElasticConnection _connection = new ElasticConnection();
                var result = _connection.CreateConnection();
                var points = _connection.ElasticSearchQuery(result,_polygonPoints);
               
                // noktaları goster
                //MessageBox.Show(pointsList, "Nokta Listesi");
            }
            else
            {
                 // hiç nokta yoksa 
                MessageBox.Show("Herhangi bir nokta bulunamadı.", "Nokta Listesi");
            }
        }
    }
}
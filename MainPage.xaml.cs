using System;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GeoApp
{
    public partial class MainPage : ContentPage
    {
        private double distance;
        private string direction;
        private double targetBearing;

        private double currentBearing;
        private double accelerometerAngle;
        private double gyroscopeAngle;
        private double previousTimestamp;

        public MainPage()
        {
            InitializeComponent();
            Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
            Gyroscope.ReadingChanged += Gyroscope_ReadingChanged;
        }

        private void OnStartNavigationClicked(object sender, EventArgs e)
        {
            direction = DirectionEntry.Text.ToUpper();
            if (!double.TryParse(DistanceEntry.Text, out distance))
            {
                DisplayAlert("Error", "Invalid distance", "OK");
                return;
            }

            targetBearing = GetBearingFromDirection(direction);
            Accelerometer.Start(SensorSpeed.UI);
            Gyroscope.Start(SensorSpeed.UI);
        }

        private void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            var data = e.Reading;
            accelerometerAngle = Math.Atan2(data.Acceleration.Y, data.Acceleration.X) * 180 / Math.PI;
        }

        private void Gyroscope_ReadingChanged(object sender, GyroscopeChangedEventArgs e)
        {
            var data = e.Reading;
            double deltaTime = (e.Timestamp - previousTimestamp) / 1000000000.0; // Convert nanoseconds to seconds
            previousTimestamp = e.Timestamp;

            // Integrate gyroscope data to get angular displacement
            gyroscopeAngle += data.AngularVelocity.Z * deltaTime;

            // Apply a complementary filter to combine accelerometer and gyroscope data
            double alpha = 0.98;
            currentBearing = alpha * (currentBearing + data.AngularVelocity.Z * deltaTime) + (1 - alpha) * accelerometerAngle;

            UpdateDirectionIndicator(currentBearing);
        }

        private double GetBearingFromDirection(string direction)
        {
            switch (direction)
            {
                case "N": return 0;
                case "NE": return 45;
                case "E": return 90;
                case "SE": return 135;
                case "S": return 180;
                case "SW": return 225;
                case "W": return 270;
                case "NW": return 315;
                default: return -1;
            }
        }

        private void UpdateDirectionIndicator(double currentBearing)
        {
            double bearingDifference = targetBearing - currentBearing;
            // Adjust the rotation of the DirectionIndicator BoxView based on the bearing difference
            Device.BeginInvokeOnMainThread(() =>
            {
                DirectionIndicator.Rotation = bearingDifference;
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Accelerometer.Stop();
            Gyroscope.Stop();
        }
    }
}

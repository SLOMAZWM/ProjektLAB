﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ProjektLAB.TrainService.Class;
using ProjektLAB.TrainService.Class.ServiceClass;
using ProjektLAB.TrainService.Pages.DialogWindow.DetailStation;
using static System.Collections.Specialized.BitVector32;

namespace ProjektLAB.TrainService.Pages.DialogWindow
{
    /// <summary>
    /// Interaction logic for RouteAddPage.xaml
    /// </summary>
    public partial class RouteAddPage : Page
    {
        public Route newRoute { get; set; }
        private string startStation;
        private string endStation;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public Train? selectedTrain { get; set; }
        private bool isEdit = false;

        public RouteAddPage(Route newR, string start, string end, bool Edited, Train selectT)
        {
            InitializeComponent();
            newRoute = newR;
            startStation = start;
            endStation = end;
            isEdit = Edited;
            selectedTrain = selectT;
            InitializeData();
        }

        private void InitializeData()
        {
            StartStationLbl.Content = startStation;
            EndStationLbl.Content = endStation;
            RouteListBox.ItemsSource = newRoute.Stations;
            AmountOfStationsLbl.Content = RouteListBox.Items.Count;

            if(isEdit == true)
            {
                TitleLbl.Content = "Edytowanie trasy";
                InitializeEditRoute();
            }
            else
            {
                TitleLbl.Content = "Dodawanie trasy";
            }
        }

        private void InitializeEditRoute()
        {
            StartTimeTextBox.Text = newRoute.StartTimeString;
            EndTimeTextBox.Text = newRoute.EndTimeString;
            DateOfStartDP.Text = newRoute.StartDate.ToString();
            TrainTypeLbl.Content = selectedTrain?.TrainType;
            CarrierLbl.Content = selectedTrain?.Carrier;
            TrainNumberLbl.Content = selectedTrain?.TrainNumber;
            NumberOfSeatsLbl.Content = selectedTrain?.NumberOfSeats;
            MaxSpeedLbl.Content = selectedTrain?.MaxSpeed;
            CompartmentCarLbl.Content = selectedTrain?.CompartmentCar;
            OpenCarLbl.Content = selectedTrain?.OpenCar;
            SleepingCarLbl.Content = selectedTrain?.SleepingCar;
            SaveBtnLbl.Content = "Edytuj trasę";

            foreach(Station stations in newRoute.Stations) 
            {
                stations.SelectedTrack = stations.TrackNumber;
                stations.SelectedPlatform = stations.PlatformNumber;
            }

        }

        private void LoadTrainBtn_Click(object sender, RoutedEventArgs e)
        {
            SelectTrainDialog selectTrain = new SelectTrainDialog(this);
            selectTrain.ShowDialog();
        }

        private void RouteListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Station selectedStation = (Station)RouteListBox.SelectedItem;

            ArrivalTimeStationLbl.Content = selectedStation.ArrivalTime;
            DepartureTimeStationLbl.Content = selectedStation.DepartureTime;
            SelectedPlatformLbl.Content = selectedStation.SelectedPlatform.ToString();
            SelectedTrackLbl.Content = selectedStation.SelectedTrack.ToString();
        }

        private void SeeDetailsBtn_Click(object sender, RoutedEventArgs e)
        {
            Station selectedStation = (Station)RouteListBox.SelectedItem;
            if (selectedStation != null)
            {
                if (TimeSpan.TryParseExact(StartTimeTextBox.Text, @"hh\:mm", null, out TimeSpan startTime) &&
            TimeSpan.TryParseExact(EndTimeTextBox.Text, @"hh\:mm", null, out TimeSpan endTime))
                {
                    DetailStationWindow details = new DetailStationWindow(RouteListBox, startTime, endTime, selectedStation);
                    details.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Wypełnij datę rozpoczęcia i zakończenia kursu, w odpowiednim formacie (hh:mm)!", "Błąd wypełnienia", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Zaznacz stację by sprawdzić jej szczegóły!", "Błąd zaznaczenia", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveRoute_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTrain == null)
            {
                MessageBox.Show("Proszę wybrać pociąg przed zapisaniem trasy.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!TimeSpan.TryParseExact(StartTimeTextBox.Text, @"hh\:mm", null, out TimeSpan startTime))
            {
                MessageBox.Show("Nieprawidłowy format czasu rozpoczęcia. Użyj formatu HH:mm.", "Błąd formatu czasu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!TimeSpan.TryParseExact(EndTimeTextBox.Text, @"hh\:mm", null, out TimeSpan endTime))
            {
                MessageBox.Show("Nieprawidłowy format czasu zakończenia. Użyj formatu HH:mm.", "Błąd formatu czasu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            newRoute.StartTime = startTime;
            newRoute.EndTime = endTime;

            foreach (Station station in newRoute.Stations)
            {
                if (string.IsNullOrEmpty(station.ArrivalTime) ||
                    string.IsNullOrEmpty(station.DepartureTime) ||
                    station.SelectedPlatform == null ||
                    station.SelectedTrack == null)
                {
                    MessageBox.Show($"Proszę uzupełnić wszystkie informacje dla stacji: {station.Name}.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            foreach (Station station in newRoute.Stations)
            {
                station.AssignData();
            }

            if (DateOfStartDP.SelectedDate.HasValue)
            {
                DateTime selectedDate = DateOfStartDP.SelectedDate.Value.Date;
                if (selectedDate >= DateTime.Today)
                {
                    newRoute.StartDate = selectedDate;
                }
                else
                {
                    MessageBox.Show("Data rozpoczęcia nie może być datą z przeszłości.", "Błąd daty", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else
            {
                MessageBox.Show("Proszę wybrać datę rozpoczęcia.", "Brak daty", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if(isEdit == false)
            {
                RouteServiceDataBase.SaveRouteAndStations(newRoute, selectedTrain, newRoute.StartDate);

            }
            else
            {
                foreach(Station station in newRoute.Stations)
                {
                    if(TimeSpan.Parse(station.ArrivalTime!) < TimeSpan.Parse(StartTimeTextBox.Text))
                    {
                        MessageBox.Show($"Stacja: {station.Name} - czas przyjazdu nie może być wcześniejszy niż czas rozpoczęcia kursu.", "Błąd czasu przyjazdu", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    else if(TimeSpan.Parse(station.DepartureTime!) > TimeSpan.Parse(EndTimeTextBox.Text))
                    {
                        MessageBox.Show($"Stacja: {station.Name} - czas odjazdu musi być późniejszy niż czas zakończenia kursu.", "Błąd czasu odjazdu", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                RouteServiceDataBase.UpdateTrainSchedule(newRoute, selectedTrain);
            }
            MessageBox.Show("Trasa została pomyślnie zapisana.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }

}

﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using ArenaSApp.Data;
using ArenaSApp.Helpers;
using ArenaSApp.Services.Dependency;
using ArenaSApp.Services.Navigation;
using ArenaSApp.Services.Settings;
using ArenaSApp.ViewModels.Base;
using ArenaSApp.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ArenaSApp
{
    public partial class App : Application
    {
        ISettingsService _settingsService;
        
        public App()
        {
            InitializeComponent();

            InitApp();
            if (Device.RuntimePlatform == Device.UWP)
            {
                InitNavigation();
            }
        }
        private void InitApp()
        {
            _settingsService = ViewModelLocator.Resolve<ISettingsService>();
            if (!_settingsService.UseMocks)
                ViewModelLocator.UpdateDependencies(_settingsService.UseMocks);
        }

        private Task InitNavigation()
        {
            var navigationService = ViewModelLocator.Resolve<INavigationService>();
            return navigationService.InitializeAsync();
        }
         protected override async void OnStart()
        {
            base.OnStart();

            if (Device.RuntimePlatform != Device.UWP)
            {
                await InitNavigation();
            }
            // if (_settingsService.AllowGpsLocation && !_settingsService.UseFakeLocation)
            // {
            //     await GetGpsLocation();
            // }
            if (!_settingsService.UseMocks && !string.IsNullOrEmpty(_settingsService.AuthAccessToken))
            {
                await SendCurrentLocation();
            }

            base.OnResume();
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        private async Task GetGpsLocation()
        {
            var dependencyService = ViewModelLocator.Resolve<IDependencyService>();
            // var locator = dependencyService.Get<ILocationServiceImplementation>();

            // if (locator.IsGeolocationEnabled && locator.IsGeolocationAvailable)
            // {
            //     locator.DesiredAccuracy = 50;
            //
            //     try
            //     {
            //         var position = await locator.GetPositionAsync();
            //         _settingsService.Latitude = position.Latitude.ToString();
            //         _settingsService.Longitude = position.Longitude.ToString();
            //     }
            //     catch (Exception ex)
            //     {
            //         Debug.WriteLine(ex);
            //     }
            // }
            // else
            // {
            //     _settingsService.AllowGpsLocation = false;
            // }
        }

        private async Task SendCurrentLocation()
        {
            // var location = new Location
            // {
            //     Latitude = double.Parse(_settingsService.Latitude, CultureInfo.InvariantCulture),
            //     Longitude = double.Parse(_settingsService.Longitude, CultureInfo.InvariantCulture)
            // };
            //
            // var locationService = ViewModelLocator.Resolve<ILocationService>();
            // await locationService.UpdateUserLocation(location, _settingsService.AuthAccessToken);
        }
    }
}

﻿using System.Threading.Tasks;
using PFRCenterGlobal.UnitTests.Mocks;
using Xunit;

namespace PFRCenterGlobal.UnitTests.ViewModels
{
    public class MainViewModelTests
    {
        public MainViewModelTests()
        {
            ViewModelLocator.UpdateDependencies(true);
            ViewModelLocator.RegisterSingleton<ISettingsService, MockSettingsService>();
        }

        [Fact]
        public void SettingsCommandIsNotNullWhenViewModelInstantiatedTest()
        {
            var mainViewModel = new MainViewModel();
            Assert.NotNull(mainViewModel.SettingsCommand);
        }

        [Fact]
        public async Task ViewModelInitializationSendsChangeTabMessageTest()
        {
            bool messageReceived = false;
            var mainViewModel = new MainViewModel();
            var tabParam = new TabParameter { TabIndex = 2 };

            Xamarin.Forms.MessagingCenter.Subscribe<MainViewModel, int>(this, MessageKeys.ChangeTab, (sender, arg) =>
            {
                messageReceived = true;
            });
            await mainViewModel.InitializeAsync(tabParam);

            Assert.True(messageReceived);
        }

        [Fact]
        public void IsBusyPropertyIsFalseWhenViewModelInstantiatedTest()
        {
            var mainViewModel = new MainViewModel();
            Assert.False(mainViewModel.IsBusy);
        }

        [Fact]
        public async Task IsBusyPropertyIsTrueAfterViewModelInitializationTest()
        {
            var mainViewModel = new MainViewModel();
            await mainViewModel.InitializeAsync(null);
            Assert.True(mainViewModel.IsBusy);
        }
    }
}
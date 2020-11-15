﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using GitTrends.Shared;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GitTrends
{
    public class App : Xamarin.Forms.Application
    {
        readonly static WeakEventManager _resumedEventManager = new();

        readonly LanguageService _languageService;
        readonly IAnalyticsService _analyticsService;
        readonly NotificationService _notificationService;

        public App(ThemeService themeService,
                    LanguageService languageService,
                    SplashScreenPage splashScreenPage,
                    IAnalyticsService analyticsService,
                    NotificationService notificationService,
                    IDeviceNotificationsService deviceNotificationsService)
        {
            _languageService = languageService;
            _analyticsService = analyticsService;
            _notificationService = notificationService;

            analyticsService.Track("App Initialized", new Dictionary<string, string>
            {
                { nameof(LanguageService.PreferredLanguage), _languageService.PreferredLanguage ?? "default" },
                { nameof(CultureInfo.CurrentUICulture), CultureInfo.CurrentUICulture.TwoLetterISOLanguageName }
            });

            InitializeEssentialServices(themeService, deviceNotificationsService, languageService);

            MainPage = splashScreenPage;

            On<iOS>().SetHandleControlUpdatesOnMainThread(true);
        }

        public static event EventHandler Resumed
        {
            add => _resumedEventManager.AddEventHandler(value);
            remove => _resumedEventManager.RemoveEventHandler(value);
        }

        protected override async void OnStart()
        {
            base.OnStart();

            _analyticsService.Track("App Started");

            await ClearBageNotifications();
        }

        protected override async void OnResume()
        {
            base.OnResume();

            OnResumed();

            _analyticsService.Track("App Resumed");

            await ClearBageNotifications();
        }

        protected override void OnSleep()
        {
            base.OnSleep();

            _analyticsService.Track("App Backgrounded");
        }

        ValueTask ClearBageNotifications() => _notificationService.SetAppBadgeCount(0);

        async void InitializeEssentialServices(ThemeService themeService, IDeviceNotificationsService notificationService, LanguageService languageService)
        {
            languageService.Initialize();
            notificationService.Initialize();
            await themeService.Initialize().ConfigureAwait(false);
        }

        void OnResumed() => _resumedEventManager.RaiseEvent(this, EventArgs.Empty, nameof(Resumed));
    }
}

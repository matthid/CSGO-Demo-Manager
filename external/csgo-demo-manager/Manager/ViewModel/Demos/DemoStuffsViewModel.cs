﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Core;
using Core.Models;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;
using Manager.Models;
using Manager.Services;
using Manager.ViewModel.Shared;
using Manager.Views.Demos;
using Services.Concrete;
using Services.Interfaces;
using Services.Models;
using Clipboard = System.Windows.Clipboard;

namespace Manager.ViewModel.Demos
{
	public class DemoStuffsViewModel : BaseViewModel
	{
		#region Properties

		private readonly IDialogService _dialogService;

		private Demo _demo;

		private DrawService _drawService;

		private readonly IStuffService _stuffService;

		private readonly ICacheService _cacheService;

		private readonly IMapService _mapService;

		private Stuff _selectedStuff;

		private Player _selectedPlayer;

		private WriteableBitmap _overviewLayer;

		private WriteableBitmap _stuffLayer;

		private List<Stuff> _stuffs; 

		private List<ComboboxSelector> _stuffSelectors = new List<ComboboxSelector>();

		private ComboboxSelector _currentStuffSelector;

		private RelayCommand<Demo> _backToDemoDetailsCommand;

		private RelayCommand _stuffTypeChangedCommand;

		private RelayCommand _windowLoadedCommand;

		private RelayCommand _watchStuff;

		private RelayCommand _watchPlayerStuffCommand;

		private RelayCommand _copySetPosCommand;

		private RelayCommand<DataGridRowClipboardEventArgs> _copyCellContent;

		private RelayCommand<SelectedCellsChangedEventArgs> _selectedCellChanged;

		#endregion Properties

		#region Accessors

		public Demo Demo
		{
			get => _demo;
			set { Set(() => Demo, ref _demo, value); }
		}

		public Stuff SelectedStuff
		{
			get { return _selectedStuff; }
			set
			{
				{
					Set(() => SelectedStuff, ref _selectedStuff, value);
					if (value != null && !IsInDesignMode) DrawStuff(value);
				}
			}
		}

		public Player SelectedPlayer
		{
			get { return _selectedPlayer; }
			set { Set(() => SelectedPlayer, ref _selectedPlayer, value); }
		}

		public List<ComboboxSelector> StuffSelectors
		{
			get { return _stuffSelectors; }
			set { Set(() => StuffSelectors, ref _stuffSelectors, value); }
		}

		public ComboboxSelector CurrentStuffSelector
		{
			get { return _currentStuffSelector; }
			set { Set(() => CurrentStuffSelector, ref _currentStuffSelector, value); }
		}

		public List<Stuff> Stuffs
		{
			get { return _stuffs; }
			set { Set(() => Stuffs, ref _stuffs, value); }
		}

		public WriteableBitmap StuffLayer
		{
			get { return _stuffLayer; }
			set { Set(() => StuffLayer, ref _stuffLayer, value); }
		}

		public WriteableBitmap OverviewLayer
		{
			get { return _overviewLayer; }
			set { Set(() => OverviewLayer, ref _overviewLayer, value); }
		}

		#endregion

		#region Commands

		public RelayCommand<SelectedCellsChangedEventArgs> SelectedCellChanged
		{
			get
			{
				return _selectedCellChanged
					   ?? (_selectedCellChanged = new RelayCommand<SelectedCellsChangedEventArgs>(
						   e =>
						   {
							   if (e.AddedCells.Count > 0)
								   SelectedStuff = (Stuff)e.AddedCells[0].Item;
							   else
								   SelectedStuff = null;
						   }));
			}
		}
		public RelayCommand<DataGridRowClipboardEventArgs> CopyCellContent
		{
			get
			{
				return _copyCellContent
					   ?? (_copyCellContent = new RelayCommand<DataGridRowClipboardEventArgs>(
						   e =>
						   {
							   if (e.ClipboardRowContent.Count > 0)
								   Clipboard.SetText(e.ClipboardRowContent[0].Content.ToString());
						   }));
			}
		}

		public RelayCommand WindowLoaded
		{
			get
			{
				return _windowLoadedCommand
					?? (_windowLoadedCommand = new RelayCommand(
					async () =>
					{
						await LoadData();
					}));
			}
		}

		/// <summary>
		/// Command to back to details view
		/// </summary>
		public RelayCommand<Demo> BackToDemoDetailsCommand
		{
			get
			{
				return _backToDemoDetailsCommand
					?? (_backToDemoDetailsCommand = new RelayCommand<Demo>(
						demo =>
						{
							var detailsViewModel = new ViewModelLocator().DemoDetails;
							detailsViewModel.Demo = demo;
							var mainViewModel = new ViewModelLocator().Main;
							DemoDetailsView detailsView = new DemoDetailsView();
							mainViewModel.CurrentPage.ShowPage(detailsView);
							Cleanup();
						},
						demo => Demo != null));
			}
		}

		public RelayCommand StuffTypeChangedCommand
		{
			get
			{
				return _stuffTypeChangedCommand
					?? (_stuffTypeChangedCommand = new RelayCommand(
						() =>
						{
							DispatcherHelper.CheckBeginInvokeOnUI(
							async () =>
							{
								await LoadStuffs();
							});
						},
						() => Demo != null));
			}
		}

		public RelayCommand WatchStuff
		{
			get
			{
				return _watchStuff
					?? (_watchStuff = new RelayCommand(
						async () =>
						{
							if (AppSettings.SteamExePath() == null)
							{
								await _dialogService.ShowSteamNotFoundAsync();
								return;
							}
							GameLauncherConfiguration config = Config.BuildGameLauncherConfiguration(Demo);
							config.FocusPlayerSteamId = SelectedStuff.ThrowerSteamId;
							GameLauncher launcher = new GameLauncher(config);
							launcher.WatchDemoAt(SelectedStuff.Tick, true);
						}, () => SelectedStuff != null));
			}
		}

		public RelayCommand WatchPlayerStuffCommand
		{
			get
			{
				return _watchPlayerStuffCommand
					?? (_watchPlayerStuffCommand = new RelayCommand(
						async () =>
						{
							if (AppSettings.SteamExePath() == null)
							{
								await _dialogService.ShowSteamNotFoundAsync();
								return;
							}
							GameLauncherConfiguration config = Config.BuildGameLauncherConfiguration(Demo);
							config.FocusPlayerSteamId = SelectedPlayer.SteamId;
							GameLauncher launcher = new GameLauncher(config);
							launcher.WatchPlayerStuff(SelectedPlayer, CurrentStuffSelector.Id);
						}, () => Demo != null && SelectedPlayer != null));
			}
		}

		public RelayCommand CopySetPosCommand
		{
			get
			{
				return _copySetPosCommand
					?? (_copySetPosCommand = new RelayCommand(
						async () =>
						{
							try
							{
								string command = "setpos " + SelectedStuff.ShooterPosX
														   + " " + SelectedStuff.ShooterPosY + " " +
														   SelectedStuff.ShooterPosZ
														   + " ;setang " + SelectedStuff.ShooterAnglePitch + " " +
														   SelectedStuff.ShooterAngleYaw;
								Clipboard.SetDataObject(command);
								Notification = Properties.Resources.NotificationSetposCommandCopied;
							}
							catch (Exception ex)
							{
								Logger.Instance.Log(ex);
								Notification = "Impossible to copy position into clipboard";
							}
							finally
							{
								HasNotification = true;
								await Task.Delay(5000);
								HasNotification = false;
							}
						},
						() => SelectedStuff != null));
			}
		}

		#endregion

		private void DrawStuff(Stuff stuff)
		{
			_drawService.DrawStuff(stuff);
		}

		private async Task LoadStuffs()
		{
			IsBusy = true;
			Stuffs = await _stuffService.GetStuffPointListAsync(Demo, CurrentStuffSelector.ToStuffType());
			IsBusy = false;
		}

		public DemoStuffsViewModel(
			IDialogService dialogService, ICacheService cacheService,
			IStuffService stuffService, IMapService mapService)
		{
			_dialogService = dialogService;
			_cacheService = cacheService;
			_stuffService = stuffService;
			_mapService = mapService;

			StuffSelectors.Add(new ComboboxSelector("smokes", Properties.Resources.Smokes));
			StuffSelectors.Add(new ComboboxSelector("flashbangs", Properties.Resources.Flashbangs));
			StuffSelectors.Add(new ComboboxSelector("he", Properties.Resources.HeGrenades));
			StuffSelectors.Add(new ComboboxSelector("molotovs", Properties.Resources.Molotovs));
			StuffSelectors.Add(new ComboboxSelector("incendiaries", Properties.Resources.Incendiaries));
			StuffSelectors.Add(new ComboboxSelector("decoys", Properties.Resources.Decoys));
			CurrentStuffSelector = StuffSelectors[0];

			if (IsInDesignMode)
			{
				CurrentStuffSelector = StuffSelectors[1];
				DispatcherHelper.CheckBeginInvokeOnUI(async () =>
				{
					await LoadData();
					SelectedStuff = Stuffs[10];
				});
			}
		}

		private async Task LoadData()
		{
			Notification = Properties.Resources.NotificationLoading;
			IsBusy = true;
			HasNotification = true;
			if (IsInDesignMode)
			{
				Demo = await _cacheService.GetDemoDataFromCache(string.Empty);
			}
			Demo.WeaponFired = await _cacheService.GetDemoWeaponFiredAsync(Demo);
			_mapService.InitMap(Demo);
			OverviewLayer = _mapService.GetWriteableImage(Properties.Settings.Default.UseSimpleRadar);
			_drawService = new DrawService(_mapService)
			{
				UseSimpleRadar = Properties.Settings.Default.UseSimpleRadar,
			};
			StuffLayer = _drawService.SmokeLayer;
			await LoadStuffs();
			IsBusy = false;
			HasNotification = false;
		}

		public override void Cleanup()
		{
			base.Cleanup();
			OverviewLayer = null;
			StuffLayer = null;
		}
	}
}

using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Newtonsoft.Json.Converters;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using DynamicData.Binding;
using Windows.UI.Core;
using Windows.System;



#if __ANDROID__ || __WASM__
using ReactiveUI;
#endif

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Confooxars
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel = new MainViewModel();
        public MainPage()
        {
            DataContext = ViewModel;
            this.InitializeComponent();
        }
    }

    public class Photo
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string Url { get; set; }

        public string ThumbnailUrl { get; set; }
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        public ReadOnlyObservableCollection<Photo> Photos => _photosBinding;

        public ReadOnlyObservableCollection<Photo> EvenPhotos => _evenPhotodBinding;

        public ReadOnlyObservableCollection<Photo> TH10PhotoBinding => _10thPhotoBinding;

        private DataLoad _data;

        public ICommand LoadDataCommand { get; }

        public ICommand AddItemCommand { get; }

        public ICommand UpdateItemCommand { get; }

        public ICommand DeleteItemCommand { get; }

        public MainViewModel()
        {
            _data = new DataLoad();

            LoadDataCommand = new DelegateCommand(LoadDataTask);
            AddItemCommand = new DelegateCommand(AddItem);
            UpdateItemCommand = new DelegateCommand(UpdateItem);
            DeleteItemCommand = new DelegateCommand(DeleteItem);

            var sourceObservable = _sourceCache.Connect();

            sourceObservable
#if __ANDROID__ || __WASM__
                .ObserveOn(RxApp.MainThreadScheduler)
#else
                .ObserveOnDispatcher(Windows.UI.Core.CoreDispatcherPriority.Normal)
#endif
                .Bind(out _photosBinding)
                .DisposeMany()
                .Subscribe();

            sourceObservable
                .Filter(a => a.Id % 2 == 0)
                .Sort(SortExpressionComparer<Photo>.Descending(b => b.Id))
#if __ANDROID__ || __WASM__
                .ObserveOn(RxApp.MainThreadScheduler)
#else
                .ObserveOnDispatcher(Windows.UI.Core.CoreDispatcherPriority.Normal)
#endif
                .Bind(out _evenPhotodBinding)
                .DisposeMany()
                .Subscribe();

            sourceObservable
                .Filter(a => a.Id % 10 == 0)
#if __ANDROID__ || __WASM__
                .ObserveOn(RxApp.MainThreadScheduler)
#else
                .ObserveOnDispatcher(Windows.UI.Core.CoreDispatcherPriority.Normal)
#endif
                .Bind(out _10thPhotoBinding)
                .DisposeMany()
                .Subscribe();
        }

        private void LoadDataTask()
        {
            var data = _data.GetData();

            _sourceCache.Clear();
            _sourceCache.AddOrUpdate(data);
        }

        private void DeleteItem()
        {
            // Remove items in two list
            _sourceCache.RemoveKey(10);
        }

        private void UpdateItem()
        {
            var photo = _sourceCache.Items.FirstOrDefault(a => a.Id == 4);
            photo.Title = "I was updated...";

            _sourceCache.AddOrUpdate(photo);
        }

        private void AddItem()
        {
            var photo = new Photo
            {
                Id = DateTime.Now.Ticks,
                Title = DateTime.Now.ToString(),
                ThumbnailUrl = "https://via.placeholder.com/600/92c952",
                Url = "https://via.placeholder.com/150/92c952"
            };
            _sourceCache.AddOrUpdate(photo);
        }

        private readonly ReadOnlyObservableCollection<Photo> _photosBinding;
        private readonly ReadOnlyObservableCollection<Photo> _evenPhotodBinding;
        private readonly ReadOnlyObservableCollection<Photo> _10thPhotoBinding;
        private readonly SourceCache<Photo, long> _sourceCache = new SourceCache<Photo, long>(a => a.Id);

        #region "INotifyPropertyChanged"
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = default)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }

    public class DelegateCommand : ICommand
    {
        private Action _action;
        private bool _canExecuteEnabled = true;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action action)
        {
            _action = action;
        }

        public bool CanExecute(object parameter) => CanExecuteEnabled;

        public void Execute(object parameter) => _action?.Invoke();

        private void OnCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public bool CanExecuteEnabled
        {
            get => _canExecuteEnabled;
            set
            {
                _canExecuteEnabled = value;
                OnCanExecuteChanged();
            }
        }
    }

    public class DataLoad
    {
        private IList<Photo> _photo = new List<Photo>();
        public IEnumerable<Photo> GetData()
        {
            return _photo;
        }

        public DataLoad()
        {
            var data = """
                [
                {
                  "albumId": 1,
                  "id": 1,
                  "title": "accusamus beatae ad facilis cum similique qui sunt",
                  "url": "https://via.placeholder.com/600/92c952",
                  "thumbnailUrl": "https://via.placeholder.com/150/92c952"
                },
                {
                  "albumId": 1,
                  "id": 2,
                  "title": "reprehenderit est deserunt velit ipsam",
                  "url": "https://via.placeholder.com/600/771796",
                  "thumbnailUrl": "https://via.placeholder.com/150/771796"
                },
                {
                  "albumId": 1,
                  "id": 3,
                  "title": "officia porro iure quia iusto qui ipsa ut modi",
                  "url": "https://via.placeholder.com/600/24f355",
                  "thumbnailUrl": "https://via.placeholder.com/150/24f355"
                },
                {
                  "albumId": 1,
                  "id": 4,
                  "title": "culpa odio esse rerum omnis laboriosam voluptate repudiandae",
                  "url": "https://via.placeholder.com/600/d32776",
                  "thumbnailUrl": "https://via.placeholder.com/150/d32776"
                },
                {
                  "albumId": 1,
                  "id": 5,
                  "title": "natus nisi omnis corporis facere molestiae rerum in",
                  "url": "https://via.placeholder.com/600/f66b97",
                  "thumbnailUrl": "https://via.placeholder.com/150/f66b97"
                },
                {
                  "albumId": 1,
                  "id": 6,
                  "title": "accusamus ea aliquid et amet sequi nemo",
                  "url": "https://via.placeholder.com/600/56a8c2",
                  "thumbnailUrl": "https://via.placeholder.com/150/56a8c2"
                },
                {
                  "albumId": 1,
                  "id": 7,
                  "title": "officia delectus consequatur vero aut veniam explicabo molestias",
                  "url": "https://via.placeholder.com/600/b0f7cc",
                  "thumbnailUrl": "https://via.placeholder.com/150/b0f7cc"
                },
                {
                  "albumId": 1,
                  "id": 8,
                  "title": "aut porro officiis laborum odit ea laudantium corporis",
                  "url": "https://via.placeholder.com/600/54176f",
                  "thumbnailUrl": "https://via.placeholder.com/150/54176f"
                },
                {
                  "albumId": 1,
                  "id": 9,
                  "title": "qui eius qui autem sed",
                  "url": "https://via.placeholder.com/600/51aa97",
                  "thumbnailUrl": "https://via.placeholder.com/150/51aa97"
                },
                {
                  "albumId": 1,
                  "id": 10,
                  "title": "beatae et provident et ut vel",
                  "url": "https://via.placeholder.com/600/810b14",
                  "thumbnailUrl": "https://via.placeholder.com/150/810b14"
                },
                {
                  "albumId": 1,
                  "id": 11,
                  "title": "nihil at amet non hic quia qui",
                  "url": "https://via.placeholder.com/600/1ee8a4",
                  "thumbnailUrl": "https://via.placeholder.com/150/1ee8a4"
                },
                ]
                """;

            var parsed = Newtonsoft.Json.JsonConvert.DeserializeObject<Photo[]>(data);

            _photo = new List<Photo>(parsed);
        }
    }
}

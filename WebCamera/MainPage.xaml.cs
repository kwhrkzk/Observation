using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types
using Microsoft.WindowsAzure.Storage.Auth;
using System.Reactive.Linq;
using System.Reactive.Windows.Foundation;
using System.Reactive;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 を参照してください

namespace WebCamera
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private CloudBlockBlob createBlob()
        {
            var credentials = new StorageCredentials("strage", "accesskey");
            var account = new CloudStorageAccount(credentials, true);

            CloudBlobClient blobClient = account.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference("container");

            // Retrieve reference to a blob named "myblob".
            return container.GetBlockBlobReference("blob");
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            CloudBlockBlob blockBlob = createBlob();

            MediaCapture mediaCaptureManager = new MediaCapture();
            mediaCaptureManager.InitializeAsync().ToObservable()
                .ObserveOnDispatcher()
                .SelectMany(_ =>
                {
                    previewElement.Source = mediaCaptureManager;
                    return mediaCaptureManager.StartPreviewAsync().ToObservable();
                })
                .Subscribe(_ =>
                {
                    Windows.Storage.KnownFolders.PicturesLibrary.CreateFileAsync("tmp.jpg", Windows.Storage.CreationCollisionOption.ReplaceExisting).ToObservable()
                        .Repeat()
                        .Delay(TimeSpan.FromSeconds(5))
                        .Subscribe(photoStorageFile =>
                        {
                            Task.Run(async () => {
                                await mediaCaptureManager.CapturePhotoToStorageFileAsync(ImageEncodingProperties.CreateJpeg(), photoStorageFile);
                                await blockBlob.UploadFromFileAsync(photoStorageFile);
                            }).Wait();
                        }, ex => Debug.WriteLine(ex.Message));
                });
        }
    }
}

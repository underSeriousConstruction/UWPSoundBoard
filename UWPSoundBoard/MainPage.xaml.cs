using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UWPSoundBoard.Model;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPSoundBoard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<Sound> Sounds;
        private List<String> Suggestions;
        private List<MenuItem> MenuItems;
        public MainPage()
        {
            this.InitializeComponent();
            Sounds = new ObservableCollection<Sound>();
            MenuItems = new List<MenuItem>();
            SoundManger.GetAllSounds(Sounds);
            MenuItems.Add(new MenuItem { IconFile = "Assets/Icons/animals.png", Category = SoundCategory.Animals });
            MenuItems.Add(new MenuItem { IconFile = "Assets/Icons/cartoon.png", Category = SoundCategory.Cartoons });
            MenuItems.Add(new MenuItem { IconFile = "Assets/Icons/taunt.png", Category = SoundCategory.Taunts });
            MenuItems.Add(new MenuItem { IconFile = "Assets/Icons/warning.png", Category = SoundCategory.Warnings });
            backButton.Visibility = Visibility.Collapsed;
        }
        private void goBack()
        {
            SoundManger.GetAllSounds(Sounds);
            CategoryTextBlock.Text = "All Sounds";
            MenuItemsListView.SelectedItem = null;
            backButton.Visibility = Visibility.Collapsed;
        }

        private void hammburgerButton_Click(object sender, RoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            goBack();
        }

        private void searchAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (string.IsNullOrEmpty(sender.Text)) goBack();
            SoundManger.GetAllSounds(Sounds);
            Suggestions = Sounds.Where(p => p.Name.StartsWith(sender.Text)).Select(p=>p.Name).ToList();
            searchAutoSuggestBox.ItemsSource = Suggestions; 
        }

        private void searchAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            SoundManger.GetSoundsByName(Sounds, sender.Text);
            MenuItemsListView.SelectedItem = null;
            CategoryTextBlock.Text = sender.Text;
            backButton.Visibility = Visibility.Visible;
        }

        private void MenuItemsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var menuItem = (MenuItem)e.ClickedItem;
            CategoryTextBlock.Text = menuItem.Category.ToString();
            SoundManger.GetSoundsByCategory(Sounds, menuItem.Category);
            backButton.Visibility = Visibility.Visible;
        }

        private void SoundGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var sound = (Sound)e.ClickedItem;
            MyMediaElement.Source = new Uri(this.BaseUri, sound.AudioFile);
        }

        private async void SoundGridView_Drop(object sender, DragEventArgs e)
        {
            if(e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if(items.Any())
                {
                    var storageFile = items[0] as StorageFile;
                    var contentType = storageFile.ContentType;
                    StorageFolder folder = ApplicationData.Current.LocalFolder;
                    
                    if(contentType == "image/png" || contentType == "image/jpeg")
                    {
                        StorageFile newFile = await storageFile.CopyAsync(folder, storageFile.Name,NameCollisionOption.GenerateUniqueName);
                        var bitmapImage = new BitmapImage();
                        bitmapImage.SetSource(await storageFile.OpenAsync(FileAccessMode.Read));
                        //
                    }
                    else if(contentType == "audio/wav" || contentType =="audio/mpeg")
                    {
                        StorageFile newFile = await storageFile.CopyAsync(folder, storageFile.Name, NameCollisionOption.GenerateUniqueName);
                        MyMediaElement.SetSource(await storageFile.OpenAsync(FileAccessMode.Read),contentType);
                        MyMediaElement.Play();
                    }
                }
            }
        }

        private void SoundGridView_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "drop to crate a custom sound..";
            e.DragUIOverride.IsCaptionVisible = true;
            e.DragUIOverride.IsContentVisible = true;
            e.DragUIOverride.IsGlyphVisible = true;
        }
    }
}

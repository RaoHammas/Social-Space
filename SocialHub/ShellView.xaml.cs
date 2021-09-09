using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Color = System.Drawing.Color;

namespace SocialSpace
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : Window
    {
        public ObservableCollection<ControlSocialPage> SocialPages { get; set; }

        public ObservableCollection<SocialApp> SocialMenuItems { get; set; }
        public WorkSpace ActiveWorkSpace { get; set; }
        public List<WorkSpace> WorkSpaces { get; set; }

        public ShellView()
        {
            InitializeComponent();
            DataContext = this;

            SocialPages = new ObservableCollection<ControlSocialPage>();
            SocialMenuItems = new ObservableCollection<SocialApp>();
            ActiveWorkSpace = new WorkSpace { Id = 0, WorkSpaceName = String.Empty };
            WorkSpaces = new List<WorkSpace>();
            LoadMenu();
            LoadSpaces();
            ActivateSpace();
        }

        private void ClosePage(ControlSocialPage page)
        {
            try
            {
                var columnIndex = Grid.GetColumn(page);
                GridContainer.Children.Remove(page);
                var splitter = GridContainer.Children.OfType<GridSplitter>()
                    .FirstOrDefault(x => (x.Tag as ControlSocialPage) == page);
                if (splitter != null)
                {
                    GridContainer.Children.Remove(splitter);
                }

                SocialPages.Remove(page);

                foreach (var child in GridContainer.Children)
                {
                    if (child.GetType() == page.GetType())
                    {
                        var childIndex = Grid.GetColumn((ControlSocialPage)child);
                        if (childIndex > columnIndex)
                        {
                            Grid.SetColumn((ControlSocialPage)child, childIndex - 1);
                        }
                    }

                    if (child.GetType() == typeof(GridSplitter))
                    {
                        var childIndex = Grid.GetColumn((GridSplitter)child);
                        if (childIndex > columnIndex)
                        {
                            Grid.SetColumn((GridSplitter)child, childIndex - 1);
                        }
                    }
                }

                GridContainer.ColumnDefinitions.RemoveAt(GridContainer.ColumnDefinitions.Count - 1);

                SetMinWindowWidth();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void ActivateSpace()
        {
            if (WorkSpaces.Any())
            {
                ActiveWorkSpace = WorkSpaces.FirstOrDefault(x => x.IsActive == "True");
                if (ActiveWorkSpace == null)
                {
                    ActiveWorkSpace = new WorkSpace { Id = 0, WorkSpaceName = String.Empty };
                }
                else
                {
                    GridContainer.Children.Clear();
                    GridContainer.ColumnDefinitions.Clear();
                    SocialPages.Clear();

                    foreach (var item in ActiveWorkSpace.WorkSpaceItems)
                    {
                        if (item != null)
                        {
                            CreateNewPage(new SocialApp
                            {
                                DefaultAddress = item.Address,
                                Id = item.AppId
                            });
                        }
                    }
                }
            }
        }

        private void LoadSpaces()
        {
            WorkSpaces = Helper.GetWorkSpaces();
            WorkSpacesItemsControl.ItemsSource = null;
            WorkSpacesItemsControl.ItemsSource = WorkSpaces;
        }

        private void LoadMenu()
        {
            var menuItems = Helper.GetSocialApps();
            MenuItemsControl.ItemsSource = null;
            MenuItemsControl.ItemsSource = menuItems;
        }


        private void CreateNewPage(SocialApp socialApp)
        {
            SocialPage page = new SocialPage(socialApp.DefaultAddress);
            page.WebView2.DefaultBackgroundColor = Color.White;

            ControlSocialPage socialPageControl = new ControlSocialPage
            {
                Page = page,
                App = socialApp
            };

            socialPageControl.ButtonClose.DataContext = socialPageControl;
            socialPageControl.ButtonDeleteApp.DataContext = socialPageControl;
            socialPageControl.ButtonClose.Click += ButtonCloseOnClick;
            socialPageControl.ButtonDeleteApp.Click += ButtonDeleteAppOnClick;

            socialPageControl.GridBrowserContainer.Children.Add(page.WebView2);
            var count = GridContainer.ColumnDefinitions.Count;

            if (count > 0)
            {
                var splitter = new GridSplitter
                {
                    Width = 5,
                    Margin = new Thickness(0, 0, 0, 00),
                    ShowsPreview = false,
                    ResizeBehavior = GridResizeBehavior.BasedOnAlignment,
                    ResizeDirection = GridResizeDirection.Columns,
                    Background = new SolidColorBrush(Colors.DodgerBlue),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Tag = socialPageControl
                };
                Grid.SetColumn(splitter, count - 1);
                GridContainer.Children.Add(splitter);

                GridContainer.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = 300 });
                Grid.SetColumn(socialPageControl, count);
                GridContainer.Children.Add(socialPageControl);
            }
            else
            {
                GridContainer.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = 300 });
                Grid.SetColumn(socialPageControl, 0);
                GridContainer.Children.Add(socialPageControl);
            }

            SocialPages.Add(socialPageControl);
            SetMinWindowWidth();
        }

        private void ButtonDeleteAppOnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var confirm = MessageBox.Show("Do you want to delete this app ?",
                    "Confirmation Required !", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirm == MessageBoxResult.No)
                {
                    return;
                }

                var senderBtn = (Button)sender;
                var senderPage = (ControlSocialPage)senderBtn.DataContext;
                if (senderPage != null)
                {
                    var resp = Helper.DeleteSocialApp(senderPage.App);
                    if (resp)
                    {
                        ClosePage(senderPage);
                        LoadMenu();
                        LoadSpaces();
                        MessageBox.Show("Deleted App Successfully.");
                    }
                    else
                    {
                        MessageBox.Show("Unable to delete this App.");
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void ButtonCloseOnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var senderPage = ((Button)sender).DataContext as ControlSocialPage;
                if (senderPage != null)
                {
                    ClosePage(senderPage);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void Clicked_MenuItem(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                if (button != null && button.Tag is SocialApp senderItem)
                {
                    CreateNewPage(senderItem);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void Click_ViewWorkSpaces(object sender, RoutedEventArgs e)
        {
            if (GridTopMenu.Visibility == Visibility.Collapsed)
            {
                TextSpaceHeader.Text = "Your WorkSpaces";

                GridTopMenu.Visibility = Visibility.Visible;
                WorkSpacesItemsControl.Visibility = Visibility.Visible;
                BtnDeleteSapce.Visibility = Visibility.Visible;

                NewWorkspacePanel.Visibility = Visibility.Collapsed;
                NewAppPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                GridTopMenu.Visibility = Visibility.Collapsed;
            }
        }

        private void Click_SaveWorkSpace(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GridTopMenu.Visibility == Visibility.Collapsed)
                {
                    TextSpaceHeader.Text = "Save WorkSpace";
                    GridTopMenu.Visibility = Visibility.Visible;
                    NewWorkspacePanel.Visibility = Visibility.Visible;
                    TextBoxWorkSpaceName.Text = ActiveWorkSpace.WorkSpaceName;

                    WorkSpacesItemsControl.Visibility = Visibility.Collapsed;
                    NewAppPanel.Visibility = Visibility.Collapsed;
                    BtnDeleteSapce.Visibility = Visibility.Collapsed;
                }
                else
                {
                    GridTopMenu.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void Click_SaveWorkSpaceExecute(object sender, RoutedEventArgs e)
        {
            try
            {
                var spaceName = TextBoxWorkSpaceName.Text.Trim();
                if (!string.IsNullOrEmpty(spaceName))
                {
                    if (ActiveWorkSpace.WorkSpaceName.ToLower().Trim() != spaceName.ToLower().Trim())
                    {
                        ActiveWorkSpace.Id = 0;
                    }

                    ActiveWorkSpace.WorkSpaceName = spaceName;
                    List<WorkSpacePage> items = new List<WorkSpacePage>();

                    foreach (var page in SocialPages)
                    {
                        WorkSpacePage spacePage = new WorkSpacePage
                        {
                            WorkSpaceId = ActiveWorkSpace.Id,
                            Address = page.Page.Url,
                            AppId = page.App.Id
                        };

                        items.Add(spacePage);
                    }

                    ActiveWorkSpace.IsActive = "True";
                    ActiveWorkSpace.WorkSpaceItems = items;
                    var response = Helper.SaveWorkSpace(ActiveWorkSpace);
                    if (response > 0)
                    {
                        ActiveWorkSpace.Id = response;
                        MessageBox.Show("Saved !");
                    }
                    else
                    {
                        MessageBox.Show("Some error Occurred ! Try again  ...");
                    }
                }
                else
                {
                    MessageBox.Show("WorkSpace Name is required !");
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            finally
            {
                LoadSpaces();
            }
        }

        private void Click_WorkSpaceChange(object sender, RoutedEventArgs e)
        {
            try
            {
                var requestedSpace = ((Button)sender).Tag as WorkSpace;
                if (requestedSpace != null)
                {
                    requestedSpace.IsActive = "True";
                    var resp = Helper.ChangeActiveSpace(requestedSpace);
                    if (resp > 0)
                    {
                        LoadSpaces();
                        ActivateSpace();
                    }
                    else
                    {
                        MessageBox.Show("WorkSpace change failed ! try again...");
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void SetMinWindowWidth()
        {
            if (GridContainer.ColumnDefinitions.Any())
            {
                var width = 400 * SocialPages.Count;
                if (width <= SystemParameters.WorkArea.Width)
                {
                    this.MinWidth = width;
                }
                else
                {
                    ClosePage(SocialPages.Last());
                    MessageBox.Show("Another App can't be opened ! not enough space to show...");
                }
            }
        }

        private void Click_SaveNewAppExecute(object sender, RoutedEventArgs e)
        {
            try
            {
                SocialApp item = new SocialApp();
                item.AppTitle = TextBoxAppName.Text.Trim();
                item.DefaultAddress = TextBoxAppAddress.Text.Trim().TrimEnd('/');

                if (string.IsNullOrEmpty(item.AppTitle))
                {
                    MessageBox.Show("Please provide a valid App name. E.g Twitter");
                    return;
                }

                if (!CheckUrlValid(item.DefaultAddress))
                {
                    MessageBox.Show(
                        "Please provide a valid App address/Url. E.g https://www.google.com Or http://www.google.com");
                    return;
                }

                var response = Helper.SaveSocialApp(item);
                if (!response)
                {
                    MessageBox.Show("Some error occurred ! try again...");
                }
                else
                {
                    MessageBox.Show("New App Added !");
                    LoadMenu();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void Btn_AddNewApp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GridTopMenu.Visibility == Visibility.Collapsed)
                {
                    TextSpaceHeader.Text = "Add New App";
                    GridTopMenu.Visibility = Visibility.Visible;
                    NewAppPanel.Visibility = Visibility.Visible;


                    WorkSpacesItemsControl.Visibility = Visibility.Collapsed;
                    NewWorkspacePanel.Visibility = Visibility.Collapsed;
                    BtnDeleteSapce.Visibility = Visibility.Collapsed;
                }
                else
                {
                    GridTopMenu.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private static bool CheckUrlValid(string source)
        {
            if (!Uri.TryCreate(source, UriKind.Absolute, out var uriResult) && uriResult?.Scheme == Uri.UriSchemeHttp)
            {
                if (!Uri.TryCreate(source, UriKind.Absolute, out var uriResult2) &&
                    uriResult2?.Scheme == Uri.UriSchemeHttps)
                {
                    return false;
                }
            }

            return true;
        }

        private void Button_DeleteActiveWorkSpace_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var confirm = MessageBox.Show("Do you want to delete this WorkSpace ?",
                    "Confirmation Required !", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirm == MessageBoxResult.No)
                {
                    return;
                }

                var resp = Helper.DeleteWorkSpace(ActiveWorkSpace);
                if (resp)
                {
                    WorkSpaces.Remove(ActiveWorkSpace);
                    GridContainer.Children.Clear();
                    GridContainer.ColumnDefinitions.Clear();

                    LoadSpaces();
                    MessageBox.Show("Deleted active WorkSpace.");
                }
                else
                {
                    MessageBox.Show("Unable to delete this WorkSpace.");
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void Click_OpenDeveloperPage(object sender, RoutedEventArgs e)
        {
            try
            {
                CreateNewPage(new SocialApp
                {
                    AppTitle = "Github | Hammas",
                    Id = 99,
                    DefaultAddress = "https://github.com/RaoHammas"
                });
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }
    } // end of class
}
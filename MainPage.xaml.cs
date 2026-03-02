using BufeApp.Models;
using BufeApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BufeApp
{
    public partial class MainPage : ContentPage
    {
        private readonly MainViewModel _viewModel;
        private bool _isCategoryBarSticky = false;
        private double _categoryBarThreshold = 0;
        private bool _isScrollingProgrammatically = false;
        private Dictionary<int, double> _categorySectionPositions = new();

        public MainPage(MainViewModel vm)
        {
            InitializeComponent();
            _viewModel = vm;
            _viewModel.ScrollToCategoryRequested += OnScrollToCategoryRequested;
            this.BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InitAsync();
            
            // Calculate positions after layout is complete
            Dispatcher.Dispatch(() =>
            {
                CalculateCategoryPositions();
            });
        }

        private void CalculateCategoryPositions()
        {
            // Get the Y position where the category bar should become sticky
            _categoryBarThreshold = CategoryBarContainer.Y;
            
            // Calculate positions for each category section
            _categorySectionPositions.Clear();
            var children = CategorySectionsContainer.Children;
            double currentY = CategorySectionsContainer.Y;
            
            for (int i = 0; i < _viewModel.Categories.Count && i < children.Count; i++)
            {
                var category = _viewModel.Categories[i];
                var element = children[i] as VisualElement;
                if (element != null)
                {
                    _categorySectionPositions[category.Id] = currentY + element.Y;
                }
            }
        }

        private void OnMainScrollViewScrolled(object sender, ScrolledEventArgs e)
        {
            // Recalculate if needed (first scroll might have wrong values)
            if (_categoryBarThreshold == 0)
            {
                CalculateCategoryPositions();
            }

            // Update sticky state based on scroll position
            var shouldBeSticky = e.ScrollY >= _categoryBarThreshold;
            
            if (shouldBeSticky != _isCategoryBarSticky)
            {
                _isCategoryBarSticky = shouldBeSticky;
                StickyCategoryBar.IsVisible = _isCategoryBarSticky;
            }

            // Update selected category based on visible section (only if not programmatically scrolling)
            if (!_isScrollingProgrammatically)
            {
                UpdateSelectedCategoryFromScroll(e.ScrollY);
            }
        }

        private void UpdateSelectedCategoryFromScroll(double scrollY)
        {
            if (_categorySectionPositions.Count == 0 || _viewModel.Categories.Count == 0)
                return;

            // Find which category section is currently visible
            CategorieResponseModel? visibleCategory = null;
            double offset = _categoryBarThreshold + 60; // Offset for sticky header

            foreach (var category in _viewModel.Categories)
            {
                if (_categorySectionPositions.TryGetValue(category.Id, out double sectionY))
                {
                    if (scrollY + offset >= sectionY)
                    {
                        visibleCategory = category;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (visibleCategory != null && _viewModel.SelectedCategory?.Id != visibleCategory.Id)
            {
                _viewModel.UpdateSelectedCategoryWithoutScroll(visibleCategory);
            }
        }

        private async void OnScrollToCategoryRequested(object? sender, CategorieResponseModel category)
        {
            // Recalculate positions if needed
            if (_categorySectionPositions.Count == 0)
            {
                await Task.Delay(100); // Wait for layout
                CalculateCategoryPositions();
            }

            if (_categorySectionPositions.TryGetValue(category.Id, out double targetY))
            {
                _isScrollingProgrammatically = true;
                
                // Scroll with offset for the sticky header
                double scrollTarget = targetY - 60;
                await MainScrollView.ScrollToAsync(0, Math.Max(0, scrollTarget), true);
                
                // Reset flag after animation completes
                await Task.Delay(500);
                _isScrollingProgrammatically = false;
            }
        }
    }

    public partial class MainViewModel : ObservableObject
    {
        public event EventHandler<CategorieResponseModel>? ScrollToCategoryRequested;
        
        public string Username => UserService.Name;

        [ObservableProperty]
        private ObservableCollection<CategorieResponseModel> categories;

        [ObservableProperty]
        private CategorieResponseModel? selectedCategory;

        [ObservableProperty]
        private ObservableCollection<ItemModel> featuredItems;

        [ObservableProperty]
        private bool isBusy;

        public MainViewModel()
        {
            Categories = new ObservableCollection<CategorieResponseModel>();
            FeaturedItems = new ObservableCollection<ItemModel>();
        }

        public async Task InitAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                var categoriesResult = await ApiService.GetAsync<List<CategorieResponseModel>>(
                    ApiService.CategoriesEndpoint, 
                    UserService.BearerToken);

                Categories.Clear();
                FeaturedItems.Clear();

                foreach (var category in categoriesResult)
                {
                    foreach (ItemModel i in category.Items)
                    {
                        i.PictureUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQtvjOOAjwn2EHz5VzgbYbCIRT7phazqKAh2w&s";
                    }
                    Categories.Add(category);

                    // Collect featured items from all categories
                    foreach (var item in category.Items.Where(i => i.IsFeatured))
                    {
                        FeaturedItems.Add(item);
                    }
                }

                // Select the first category by default
                if (Categories.Any())
                {
                    UpdateSelectedCategoryWithoutScroll(Categories.First());
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void UpdateSelectedCategoryWithoutScroll(CategorieResponseModel category)
        {
            // Deselect all categories
            foreach (var cat in Categories)
            {
                cat.IsSelected = false;
            }
            
            // Select the new category
            category.IsSelected = true;
            SelectedCategory = category;
        }

        [RelayCommand]
        private void SelectCategory(CategorieResponseModel category)
        {
            UpdateSelectedCategoryWithoutScroll(category);
            
            // Request scroll to the category section
            ScrollToCategoryRequested?.Invoke(this, category);
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            try
            {
                await UserService.LogoutUser();
                await Application.Current.MainPage.DisplayAlert("Success", "Logged out successfully", "OK");
                await Shell.Current.GoToAsync("//LoginPage");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}

using BufeApp.Models;
using BufeApp.Services;
using BufeApp.Pages;
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
        private bool _isInitialized = false;

        public MainPage(MainViewModel vm)
        {
            InitializeComponent();
            _viewModel = vm;
            _viewModel.ScrollToCategoryRequested += OnScrollToCategoryRequested;
            _viewModel.CategorySelectionChanged += OnCategorySelectionChanged;
            _viewModel.OpenBottomSheetRequested += OnOpenBottomSheetRequested;
            _viewModel.CloseBottomSheetRequested += OnCloseBottomSheetRequested;
            this.BindingContext = _viewModel;
        }

        private void OnOpenBottomSheetRequested(object? sender, EventArgs e)
        {
            ItemBottomSheet.IsOpen = true;
        }

        private void OnCloseBottomSheetRequested(object? sender, EventArgs e)
        {
            ItemBottomSheet.IsOpen = false;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            _viewModel.Username = UserService.Name;

            if (!_isInitialized)
            {
                await _viewModel.InitAsync();
                _isInitialized = true;
            }

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

        private async void OnCategorySelectionChanged(object? sender, CategorieResponseModel category)
        {
            // Scroll the category bar horizontally to show selected category at the left
            await ScrollCategoryBarToSelected(category);
        }

        private async Task ScrollCategoryBarToSelected(CategorieResponseModel category)
        {
            // Find the index of the selected category
            int index = _viewModel.Categories.IndexOf(category);
            if (index < 0) return;

            double targetX = 0;
            double spacing = 12; // Should match the spacing in your XAML

            // Try to get actual button widths from the UI
            var scrollContent = CategoryBarScrollView.Content as Layout;
            if (scrollContent != null && scrollContent.Children.Count > index)
            {
                for (int i = 0; i < index; i++)
                {
                    if (scrollContent.Children[i] is VisualElement button)
                    {
                        targetX += button.Width + spacing;
                    }
                }
            }
            else
            {
                // Fallback to estimation if UI not ready
                for (int i = 0; i < index; i++)
                {
                    var cat = _viewModel.Categories[i];
                    double estimatedWidth = (cat.Name.Length * 9) + 40;
                    targetX += estimatedWidth + spacing;
                }
            }

            // Scroll both category bars to show selected at left
            await Task.WhenAll(
                CategoryBarScrollView.ScrollToAsync(targetX, 0, true),
                StickyCategoryBarScrollView.ScrollToAsync(targetX, 0, true)
            );
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
        public event EventHandler<CategorieResponseModel>? CategorySelectionChanged;
        public event EventHandler? OpenBottomSheetRequested;
        public event EventHandler? CloseBottomSheetRequested;
        

        public ObservableCollection<CartItemModel> CartItems => CartService.Items;
        public decimal CartTotal => CartService.TotalPrice;

        [ObservableProperty]
        private ObservableCollection<CategorieResponseModel> categories;

        // Store original data for filtering
        private List<CategorieResponseModel> _allCategories = new();
        private Dictionary<int, List<ItemModel>> _originalCategoryItems = new();

        [ObservableProperty]
        private string searchQuery = string.Empty;

        partial void OnSearchQueryChanged(string value)
        {
            FilterCategories();
        }

        [ObservableProperty]
        private string username;

        [ObservableProperty]
        private CategorieResponseModel? selectedCategory;

        [ObservableProperty]
        private ObservableCollection<ItemModel> featuredItems;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private ItemModel? selectedItem;

        [ObservableProperty]
        private int quantity = 1;

        [ObservableProperty]
        private decimal totalPrice;

        public MainViewModel()
        {
            CartService.CartChanged += (s, e) => {
                OnPropertyChanged(nameof(CartTotal));
            };
            Categories = new ObservableCollection<CategorieResponseModel>();
            FeaturedItems = new ObservableCollection<ItemModel>();
        }

        public async Task InitAsync()
        {
            if (IsBusy)
                return;

            Username = UserService.Name;
            OnPropertyChanged(nameof(Username));

            try
            {
                IsBusy = true;

                var categoriesResult = await ApiService.GetAsync<List<CategorieResponseModel>>(
                    ApiService.CategoriesEndpoint, 
                    UserService.BearerToken);

                _allCategories = categoriesResult;
                _originalCategoryItems.Clear();
                Categories.Clear();
                FeaturedItems.Clear();

                foreach (var category in categoriesResult)
                {
                    // Cache original items for filtering
                    _originalCategoryItems[category.Id] = category.Items.ToList();
                    
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

        private void FilterCategories()
        {
            if (_allCategories == null || !_allCategories.Any())
                return;

            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                // Restore all categories and items
                Categories.Clear();
                foreach (var category in _allCategories)
                {
                    if (_originalCategoryItems.TryGetValue(category.Id, out var originalItems))
                    {
                        category.Items = originalItems;
                    }
                    Categories.Add(category);
                }
                return;
            }

            var lowerQuery = SearchQuery.ToLowerInvariant();
            Categories.Clear();

            foreach (var category in _allCategories)
            {
                if (!_originalCategoryItems.TryGetValue(category.Id, out var originalItems))
                    continue;

                // Determine if category name matches
                bool categoryMatches = category.Name.ToLowerInvariant().Contains(lowerQuery);

                // Filter items
                var matchingItems = originalItems.Where(i => 
                    i.Name.ToLowerInvariant().Contains(lowerQuery) || 
                    (i.Description != null && i.Description.ToLowerInvariant().Contains(lowerQuery))).ToList();

                if (matchingItems.Any() || categoryMatches)
                {
                    // If category name matches, should we show all items or just matching? Let's show matching, or all if none matches
                    category.Items = matchingItems.Any() ? matchingItems : originalItems;
                    Categories.Add(category);
                }
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
            
            // Notify to scroll the category bar horizontally
            CategorySelectionChanged?.Invoke(this, category);
        }

        partial void OnQuantityChanged(int value)
        {
            if (SelectedItem != null)
            {
                TotalPrice = SelectedItem.Price * value;
            }
        }
        
        [RelayCommand]
        private void SelectItem(ItemModel item)
        {
            SelectedItem = item;
            Quantity = 1;
            TotalPrice = item.Price;
            OpenBottomSheetRequested?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void IncreaseQuantity()
        {
            if (Quantity < 99)
            {
                Quantity++;
            }
        }

        [RelayCommand]
        private void DecreaseQuantity()
        {
            if (Quantity > 1)
            {
                Quantity--;
            }
        }

        [RelayCommand]
        private async Task AddToCartAsync()
        {
            if (SelectedItem == null)
                return;

            CartService.AddItem(SelectedItem, Quantity);

            CloseBottomSheetRequested?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void SelectCategory(CategorieResponseModel category)
        {
            UpdateSelectedCategoryWithoutScroll(category);
            
            // Request scroll to the category section
            ScrollToCategoryRequested?.Invoke(this, category);
        }

        [RelayCommand]
        private async Task GoToCartAsync()
        {
            await Shell.Current.GoToAsync($"///{nameof(CartPage)}");
        }
    }
}

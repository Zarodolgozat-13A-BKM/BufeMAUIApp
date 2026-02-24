using BufeApp.Models;
using BufeApp.Services;
using System.Collections.ObjectModel;

namespace BufeApp
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<CategorieResponseModel> Categories { get; set; }


        public MainPage()
        {
            InitializeComponent();
            this.BindingContext = this;
            Categories = new ObservableCollection<CategorieResponseModel>();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await Init();
        }

        public async Task Init()
        {
            try
            {
                var CategoriesResult = await ApiService.GetAsync<List<CategorieResponseModel>>(ApiService.CategoriesEndpoint, UserService.BearerToken);
                
                Categories.Clear();
                foreach (var category in CategoriesResult)
                {
                    Categories.Add(category);
                }
                
                await DisplayAlert("Success", $"Loaded {Categories.Count} categories", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }

}

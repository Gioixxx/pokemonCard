using PokemonCardManager.Services;
using PokemonCardManager.ViewModels;
using System.Windows.Controls;

namespace PokemonCardManager.Views
{
    public partial class DashboardView : Page
    {
        private readonly DashboardViewModel _viewModel;

        public DashboardView(ICardService cardService, ISaleService saleService, ILogger logger)
        {
            InitializeComponent();

            _viewModel = new DashboardViewModel(cardService, saleService, logger);
            DataContext = _viewModel;
        }
    }
}
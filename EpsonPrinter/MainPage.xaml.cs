using EpsonPrinter.Services;
using System.Collections.ObjectModel;

namespace EpsonPrinter
{
    public partial class MainPage : ContentPage
    {
        private readonly PrintService _printService;
        private ObservableCollection<PrinterDevice> _printers;

        public MainPage(PrintService printService)
        {
            InitializeComponent();
            _printService = printService;
            _printers = new ObservableCollection<PrinterDevice>();
            PrintersCollectionView.ItemsSource = _printers;

            // Set default values
            PrinterModelPicker.SelectedIndex = 0; // TM303H621W
            LanguagePicker.SelectedIndex = 0; // ANK
        }

        private void OnPrinterModelChanged(object sender, EventArgs e)
        {
            var selectedModel = PrinterModelPicker.SelectedItem?.ToString();
            System.Diagnostics.Debug.WriteLine($"OnPrinterModelChanged: Selected model = '{selectedModel}'");
            if (!string.IsNullOrEmpty(selectedModel))
            {
                _printService.SetPrinterModel(selectedModel);
                UpdateStatus($"Đã chọn máy in: {selectedModel}", Colors.Blue);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("OnPrinterModelChanged: Selected model is null or empty");
            }
        }

        private async void OnSearchPrintersClicked(object sender, EventArgs e)
        {
            SearchButton.IsEnabled = false;
            PrintButton.IsEnabled = false;
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            PrinterListContainer.IsVisible = false;
            SelectedPrinterContainer.IsVisible = false;
            _printers.Clear();

            UpdateStatus("Đang tìm kiếm máy in Bluetooth...", Colors.Blue);

            try
            {
                var selectedLanguage = LanguagePicker.SelectedItem?.ToString() ?? "ANK";
                _printService.SetLanguage(selectedLanguage);

                var printers = await _printService.SearchPrintersAsync();

                if (printers != null && printers.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Found {printers.Count} printers");
                    foreach (var printer in printers)
                    {
                        System.Diagnostics.Debug.WriteLine($"Adding printer: {printer.DeviceName} - {printer.Target}");
                        _printers.Add(printer);
                    }

                    System.Diagnostics.Debug.WriteLine($"Total printers in collection: {_printers.Count}");
                    PrinterListContainer.IsVisible = true;
                    UpdateStatus($"Tìm thấy {printers.Count} máy in. Vui lòng chọn máy in.", Colors.Green);
                }
                else
                {
                    UpdateStatus("Không tìm thấy máy in. Vui lòng kiểm tra:\n- Bluetooth đã bật\n- Máy in đã bật\n- Máy in ở gần thiết bị", Colors.Red);
                    await DisplayAlert("Không tìm thấy", "Không tìm thấy máy in Bluetooth nào.", "OK");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi tìm kiếm: {ex.Message}", Colors.Red);
                await DisplayAlert("Lỗi", $"Có lỗi xảy ra khi tìm kiếm: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
                SearchButton.IsEnabled = true;
            }
        }

        private void OnPrinterSelected(object sender, SelectionChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"OnPrinterSelected called. Selection count: {e.CurrentSelection?.Count ?? 0}");

            if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
            {
                var selectedPrinter = e.CurrentSelection[0] as PrinterDevice;
                System.Diagnostics.Debug.WriteLine($"Selected printer: {selectedPrinter?.DeviceName ?? "null"}");

                if (selectedPrinter != null)
                {
                    _printService.SetSelectedPrinter(selectedPrinter);
                    SelectedPrinterLabel.Text = $"{selectedPrinter.DeviceName}\n{selectedPrinter.Target}";
                    SelectedPrinterContainer.IsVisible = true;
                    PrintButton.IsEnabled = true;
                    UpdateStatus($"Đã chọn máy in: {selectedPrinter.DeviceName}", Colors.Green);
                    System.Diagnostics.Debug.WriteLine("Printer selected successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: Selected printer is null");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No printer selected or selection cleared");
            }
        }

        private async void OnPrintClicked(object sender, EventArgs e)
        {
            PrintButton.IsEnabled = false;
            SearchButton.IsEnabled = false;
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            UpdateStatus("Đang kết nối và in...", Colors.Blue);

            try
            {
                bool success = await _printService.PrintTestAsync();

                if (success)
                {
                    UpdateStatus("In thành công!", Colors.Green);
                    await DisplayAlert("Thành công", "Đã in mẫu thành công!", "OK");
                }
                else
                {
                    UpdateStatus("In thất bại. Vui lòng kiểm tra:\n- Kết nối máy in\n- Giấy in\n- Nắp máy in", Colors.Red);
                    await DisplayAlert("Lỗi", "Không thể in. Vui lòng kiểm tra máy in.", "OK");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi: {ex.Message}", Colors.Red);
                await DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
                PrintButton.IsEnabled = true;
                SearchButton.IsEnabled = true;
            }
        }

        private void UpdateStatus(string message, Color color)
        {
            StatusLabel.Text = message;
            StatusLabel.TextColor = color;
            StatusFrame.IsVisible = true;
        }
    }
}

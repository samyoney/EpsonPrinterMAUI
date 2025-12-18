# Changelog - Epson Printer MAUI App

## [1.0.0] - 2024

### ✨ Tính năng mới

#### Giao diện người dùng
- ✅ Giao diện chuyên nghiệp với Material Design
- ✅ Picker chọn loại máy in (9 models: TM-M30III, TM-M30II, TM-M30, TM-P20, TM-P60II, TM-P80, TM-T20, TM-T82, TM-T88VII)
- ✅ Picker chọn ngôn ngữ (ANK, Japanese, Chinese, Korean, Thai)
- ✅ Nút tìm kiếm máy in Bluetooth với icon
- ✅ CollectionView hiển thị danh sách máy in tìm thấy
- ✅ Hiển thị máy in đã chọn
- ✅ Loading indicator với animation
- ✅ Khung trạng thái với mã màu trực quan (xanh lá = thành công, đỏ = lỗi, xanh dương = đang xử lý)

#### Tính năng tìm kiếm và kết nối
- ✅ Tìm kiếm tự động tất cả máy in Epson qua Bluetooth (10 giây)
- ✅ Hiển thị tên và địa chỉ Bluetooth của mỗi máy in
- ✅ Người dùng chọn máy in từ danh sách
- ✅ Kết nối tự động với máy in đã chọn

#### Tính năng in
- ✅ In mẫu với format chuyên nghiệp:
  - Header với text size 2x2
  - Thông tin ngày giờ
  - Tên máy in
  - Nội dung mẫu
  - Căn chỉnh text (trái, giữa)
  - Cắt giấy tự động
- ✅ Xử lý lỗi chi tiết với thông báo rõ ràng
- ✅ Hỗ trợ nhiều model máy in
- ✅ Hỗ trợ nhiều ngôn ngữ

### 🔧 Kỹ thuật

#### PrintService.cs
- ✅ `PrinterDevice` class: Model chứa thông tin máy in (DeviceName, Target, IpAddress)
- ✅ `SetPrinterModel(string)`: Chuyển đổi tên model sang Printer constant
- ✅ `SetLanguage(string)`: Chuyển đổi tên ngôn ngữ sang MODEL constant
- ✅ `SearchPrintersAsync()`: Tìm kiếm Bluetooth và trả về danh sách máy in
- ✅ `SetSelectedPrinter(PrinterDevice)`: Lưu máy in được chọn
- ✅ `PrintTestAsync()`: In với máy in đã chọn
- ✅ Xử lý lỗi với error messages chi tiết

#### MainPage.xaml.cs
- ✅ `OnPrinterModelChanged()`: Xử lý khi chọn model
- ✅ `OnSearchPrintersClicked()`: Tìm kiếm và hiển thị danh sách
- ✅ `OnPrinterSelected()`: Xử lý khi chọn máy in từ danh sách
- ✅ `OnPrintClicked()`: Thực hiện in
- ✅ `UpdateStatus()`: Cập nhật trạng thái với màu sắc

### 🐛 Sửa lỗi

#### Constants
- ✅ Sửa `Printer.ModelAnk` → `Printer.MODEL_ANK`
- ✅ Sửa `Printer.ModelJapanese` → `Printer.MODEL_JAPANESE`
- ✅ Sửa `Printer.ModelChinese` → `Printer.MODEL_CHINESE`
- ✅ Sửa `Printer.ModelKorean` → `Printer.MODEL_KOREAN`
- ✅ Sửa `Printer.ModelThai` → `Printer.MODEL_THAI`
- ✅ Sửa `Printer.ParamDefault` → `Printer.PARAM_DEFAULT`
- ✅ Sửa `Printer.AlignLeft` → `Printer.ALIGN_LEFT`
- ✅ Sửa constants cục bộ `ALIGN_CENTER`, `CUT_FEED` → `Printer.ALIGN_CENTER`, `Printer.CUT_FEED`

#### Code cleanup
- ✅ Xóa method `FindPrinter()` không còn sử dụng
- ✅ Xóa biến `_selectedPrinterAddress` không còn sử dụng
- ✅ Thêm `#if ANDROID` directive cho `PrintText()` method
- ✅ Xóa `using System.Collections.ObjectModel` không sử dụng

### 📚 Documentation
- ✅ Tạo README.md với hướng dẫn đầy đủ
- ✅ Liệt kê 9 model máy in hỗ trợ
- ✅ Hướng dẫn sử dụng từng bước
- ✅ Giải thích xử lý lỗi phổ biến
- ✅ Hướng dẫn tùy chỉnh nội dung in

### 🔐 Quyền truy cập
- ✅ AndroidManifest.xml có đầy đủ quyền:
  - BLUETOOTH_SCAN & BLUETOOTH_CONNECT (Android 12+)
  - BLUETOOTH & BLUETOOTH_ADMIN (Android 11-)
  - Location permissions cho thiết bị cũ

### 📝 Cấu trúc thư mục
```
EpsonPrinter/
├── MainPage.xaml              # Giao diện chính
├── MainPage.xaml.cs           # Logic UI
├── Services/
│   └── PrintService.cs        # Service in và kết nối
├── MauiProgram.cs             # DI configuration
├── README.md                  # Hướng dẫn sử dụng
└── Platforms/
    └── Android/
        └── AndroidManifest.xml # Quyền Bluetooth
```

## Hướng dẫn sử dụng

1. Chọn model máy in từ dropdown
2. Chọn ngôn ngữ (mặc định: ANK)
3. Nhấn "🔍 Tìm máy in Bluetooth"
4. Chọn máy in từ danh sách
5. Nhấn "🖨️ In mẫu"

## Yêu cầu hệ thống

- Android 5.0 (API 21) trở lên
- Bluetooth enabled
- Máy in Epson hỗ trợ Bluetooth

## License

MIT License

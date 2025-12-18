# Epson Printer MAUI App

Ứng dụng .NET MAUI đơn giản để kết nối và in với máy in Epson qua Bluetooth.

## Tính năng

- ✅ Tìm kiếm máy in Epson qua Bluetooth
- ✅ Chọn loại máy in (TM-M30III, TM-T88VII, v.v.)
- ✅ Hỗ trợ nhiều ngôn ngữ (ANK, Japanese, Chinese, Korean, Thai)
- ✅ In mẫu đơn giản
- ✅ Giao diện thân thiện

## Yêu cầu

- Android 5.0 (API 21) trở lên
- Bluetooth phải được bật
- Máy in Epson hỗ trợ Bluetooth
- Quyền truy cập Bluetooth

## Cài đặt

1. Clone repository
2. Mở solution trong Visual Studio hoặc Rider
3. Build project EpsonBinding trước
4. Build và chạy EpsonPrinter

## Sử dụng

1. **Chọn loại máy in**: Chọn model máy in bạn đang sử dụng từ dropdown
2. **Chọn ngôn ngữ**: Chọn ngôn ngữ phù hợp (mặc định: ANK)
3. **Tìm máy in**: Nhấn nút "🔍 Tìm máy in Bluetooth"
   - Ứng dụng sẽ quét các máy in Bluetooth khả dụng trong 10 giây
   - Danh sách máy in sẽ hiển thị
4. **Chọn máy in**: Nhấn vào máy in bạn muốn kết nối
5. **In mẫu**: Nhấn nút "🖨️ In mẫu" để in trang thử

## Cấu trúc code

```
EpsonPrinter/
├── MainPage.xaml          # Giao diện chính
├── MainPage.xaml.cs       # Logic giao diện
├── Services/
│   └── PrintService.cs    # Service xử lý in và kết nối
└── MauiProgram.cs         # Cấu hình DI
```

## Các model máy in được hỗ trợ

- TM-M30III (mặc định)
- TM-M30II
- TM-M30
- TM-P20
- TM-P60II
- TM-P80
- TM-T20
- TM-T82
- TM-T88VII

## Xử lý lỗi

Ứng dụng xử lý các lỗi phổ biến:

- **Không tìm thấy máy in**: Kiểm tra Bluetooth đã bật, máy in đã bật và ở gần
- **Kết nối thất bại**: Kiểm tra nguồn máy in và kết nối Bluetooth
- **In thất bại**: Kiểm tra giấy in, nắp máy in

## Lưu ý

- Lần đầu sử dụng cần cấp quyền Bluetooth cho ứng dụng
- Máy in phải ở chế độ có thể phát hiện (discoverable)
- Một số máy in cần được ghép nối (pair) trước trong cài đặt Bluetooth của Android

## Phát triển

Để thêm tính năng in tùy chỉnh, chỉnh sửa method `PrintText()` trong [PrintService.cs](Services/PrintService.cs:237).

Example:

```csharp
_printer.AddText("Custom text\n");
_printer.AddTextSize(2, 2);  // Larger text
_printer.AddTextAlign(ALIGN_CENTER);
_printer.AddFeedLine(2);  // Feed 2 lines
```

## License

MIT License

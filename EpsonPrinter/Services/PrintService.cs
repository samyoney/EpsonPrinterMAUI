#if ANDROID
using Android.Runtime;
using Com.Epson.Epos2;
using Com.Epson.Epos2.Printer;
using Com.Epson.Epos2.Discovery;
using Com.Epson.Eposprint;
using Platform = Microsoft.Maui.ApplicationModel.Platform;
#endif
using System.Collections.Generic;
using System.Text.Json;
using Bridge.Const;

namespace EpsonPrinter.Services
{
    public class PrinterDevice
    {
        public string DeviceName { get; set; }
        public string Target { get; set; }
        public string IpAddress { get; set; }
    }

    public class PrintCommand
    {
        public string printLogo { get; set; }
        public string printFeedLine { get; set; }
        public string printTextHeader { get; set; }
        public string printTextLeft { get; set; }
        public string printTextRight { get; set; }
        public string printTextCenter { get; set; }
        public string printTextWithUL { get; set; }
        public string printTextCenter4xWithUL { get; set; }
        public string printBarcode { get; set; }
        public object print { get; set; }
        public object cut { get; set; }
    }

    public class PrintService : IDisposable
    {
#if ANDROID
        private Printer _printer;
        private PrinterDevice _selectedPrinter;
        private bool _isDiscovering;
        private int _printerModel = Printer.TmM30iii;
        private int _languageModel = Builder.ModelAnk;
#endif

        public void SetPrinterModel(string model)
        {
#if ANDROID
            if (string.IsNullOrEmpty(model))
            {
                System.Diagnostics.Debug.WriteLine("SetPrinterModel: Model is null or empty, using default TM-M30III");
                _printerModel = Printer.TmM30iii;
                return;
            }

            var oldModel = _printerModel;
            _printerModel = model switch
            {
                // Supported models only
                "TM303H621W" => Printer.TmM30iii,  // TM-m30III-H
                "TM55-611W" => Printer.TmM55,      // TM-m55
                "SBM30-901W" => Printer.TM_M30,    // TM-m30 Smart Bridge (default to TM_M30, can try MM30II if needed)
                "TM-m30II" => Printer.TmM30ii,     // TM-m30II
                "TM-P80" => Printer.TmP80,        // TM-P80 bản cũ
                // Alternative names
                "MM30III" => Printer.TmM30iii,
                "MM55" => Printer.TmM55,
                "MM30" => Printer.TM_M30,
                "MM30II" => Printer.TmM30ii,
                "MP80" => Printer.TmP80,
                _ => Printer.TmM30iii
            };

            System.Diagnostics.Debug.WriteLine($"SetPrinterModel: '{model}' -> {_printerModel} (old: {oldModel})");
            if (_printerModel == Printer.TmM30iii && model != "TM-M30III" && model != "MM30III")
            {
                System.Diagnostics.Debug.WriteLine($"WARNING: Model '{model}' not found in switch, using default TM-M30III");
            }
#endif
        }

        public void SetLanguage(string language)
        {
#if ANDROID
            _languageModel = language switch
            {
                "ANK" => Builder.ModelAnk,
                "Japanese" => Builder.ModelJapanese,
                "Chinese" => Builder.ModelChinese,
                "Korean" => Builder.ModelKorean,
                "Thai" => Builder.ModelThai,
                _ => Builder.ModelAnk
            };
#endif
        }

        public void SetSelectedPrinter(PrinterDevice printer)
        {
#if ANDROID
            _selectedPrinter = printer;
#endif
        }

        public async Task<List<PrinterDevice>> SearchPrintersAsync()
        {
#if ANDROID
            var printers = new List<PrinterDevice>();

            if (_isDiscovering)
            {
                System.Diagnostics.Debug.WriteLine("Printer discovery already in progress");
                return printers;
            }

            _isDiscovering = true;
            System.Diagnostics.Debug.WriteLine("Starting printer discovery...");

            var tcs = new TaskCompletionSource<bool>();
            var listener = new DiscoveryListener();
            var filterOption = new FilterOption { PortType = Discovery.PorttypeBluetooth };

            listener.OnPrinterFound += (s, deviceInfo) =>
            {
                var printer = new PrinterDevice
                {
                    DeviceName = deviceInfo.DeviceName,
                    Target = deviceInfo.Target,
                    IpAddress = deviceInfo.IpAddress
                };
                printers.Add(printer);
                System.Diagnostics.Debug.WriteLine($"Found printer: {printer.DeviceName} - {printer.Target}");
            };

            try
            {
                Discovery.Start(Platform.CurrentActivity, filterOption, listener);

                // Wait for discovery to complete
                await Task.Delay(10000); // 10 seconds

                return printers;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Printer discovery error: {ex.Message}");
                return printers;
            }
            finally
            {
                try
                {
                    Discovery.Stop();
                }
                catch
                {
                    // ignored
                }

                _isDiscovering = false;
            }
#else
            return new List<PrinterDevice>();
#endif
        }

        public async Task<bool> PrintTestAsync()
        {
#if ANDROID
            if (_selectedPrinter == null)
            {
                System.Diagnostics.Debug.WriteLine("No printer selected");
                return false;
            }

            try
            {
                await PrintFromDummyData();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Print error: {ex}");
                return false;
            }
#else
            return false;
#endif
        }

#if ANDROID
        private async Task PrintFromDummyData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"PrintFromDummyData: Using printer model {_printerModel}, language {_languageModel}");
                _printer = new Printer(_printerModel, _languageModel, Platform.CurrentActivity);

                string printerAddress = _selectedPrinter.Target;
                System.Diagnostics.Debug.WriteLine($"Connecting to printer: {printerAddress}...");

                await Task.Delay(500);
                _printer.Connect(printerAddress, Builder.ParamDefault);
                await Task.Delay(1000);

                try
                {
                    _printer.BeginTransaction();

                    // Set language once at the beginning based on language model
                    SetPrinterLanguage();

                    // Parse JSON commands from DummyData
                    var commands = JsonSerializer.Deserialize<List<PrintCommand>>(DummyData.Commands);

                    foreach (var cmd in commands)
                    {
                        ExecuteCommand(cmd);
                    }

                    System.Diagnostics.Debug.WriteLine("Sending data to printer...");
                    _printer.SendData(Builder.ParamDefault);

                    await Task.Delay(2000);
                    _printer.EndTransaction();

                    System.Diagnostics.Debug.WriteLine("Print completed successfully");
                }
                finally
                {
                    if (_printer != null)
                    {
                        try
                        {
                            await Task.Delay(500);
                            _printer.Disconnect();
                            System.Diagnostics.Debug.WriteLine("Printer disconnected");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Disconnect error: {ex.Message}");
                        }
                    }
                }
            }
            catch (Epos2Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Printer error: {GetPrinterErrorMessage(ex.ErrorStatus)}");
                throw new Exception(GetPrinterErrorMessage(ex.ErrorStatus), ex);
            }
            finally
            {
                _printer = null;
            }
        }

        private void SetPrinterLanguage()
        {
            try
            {
                // Set language code page for the entire print job
                if (_languageModel == Builder.ModelJapanese)
                {
                    System.Diagnostics.Debug.WriteLine("Setting printer language to Japanese");
                    _printer.AddTextLang(Builder.LangJa);
                }
                else if (_languageModel == Builder.ModelChinese)
                {
                    System.Diagnostics.Debug.WriteLine("Setting printer language to Chinese");
                    _printer.AddTextLang(Builder.LangZhCn);
                }
                else if (_languageModel == Builder.ModelKorean)
                {
                    System.Diagnostics.Debug.WriteLine("Setting printer language to Korean");
                    _printer.AddTextLang(Builder.LangKo);
                }
                else if (_languageModel == Builder.ModelThai)
                {
                    System.Diagnostics.Debug.WriteLine("Setting printer language to Thai");
                    _printer.AddTextLang(Builder.LangTh);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Setting printer language to English/ANK");
                    _printer.AddTextLang(Builder.LangEn);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SetPrinterLanguage error: {ex.Message}");
            }
        }

        private void ExecuteCommand(PrintCommand cmd)
        {
            // Debug log to see what properties are set
            System.Diagnostics.Debug.WriteLine($"ExecuteCommand: printLogo={cmd.printLogo != null}, printFeedLine={cmd.printFeedLine != null}, " +
                $"printTextHeader={cmd.printTextHeader != null}, printTextLeft={cmd.printTextLeft != null}, " +
                $"printTextRight={cmd.printTextRight != null}, printTextCenter={cmd.printTextCenter != null}, " +
                $"printTextWithUL={cmd.printTextWithUL != null}, printTextCenter4xWithUL={cmd.printTextCenter4xWithUL != null}, " +
                $"printBarcode={cmd.printBarcode != null}, print={cmd.print != null}, cut={cmd.cut != null}");

            if (cmd.printLogo != null)
            {
                PrintLogo(cmd.printLogo);
            }
            else if (cmd.printFeedLine != null)
            {
                _printer.AddFeedLine(int.Parse(cmd.printFeedLine));
            }
            else if (cmd.printTextHeader != null)
            {
                _printer.AddTextAlign(Builder.AlignCenter);
                _printer.AddTextSize(2, 2);
                AddTextWithEncoding(cmd.printTextHeader + "\n");
                _printer.AddTextSize(1, 1);
            }
            else if (cmd.printTextLeft != null)
            {
                _printer.AddTextAlign(Builder.AlignLeft);
                AddTextWithEncoding(cmd.printTextLeft + "\n");
            }
            else if (cmd.printTextRight != null)
            {
                _printer.AddTextAlign(Builder.AlignRight);
                AddTextWithEncoding(cmd.printTextRight + "\n");
            }
            else if (cmd.printTextCenter != null)
            {
                _printer.AddTextAlign(Builder.AlignCenter);
                AddTextWithEncoding(cmd.printTextCenter + "\n");
            }
            else if (cmd.printTextWithUL != null)
            {
                _printer.AddTextStyle(Builder.False, Builder.True, Builder.False, Builder.False);
                AddTextWithEncoding(cmd.printTextWithUL + "\n");
                _printer.AddTextStyle(Builder.False, Builder.False, Builder.False, Builder.False);
            }
            else if (cmd.printTextCenter4xWithUL != null)
            {
                _printer.AddTextAlign(Builder.AlignCenter);
                _printer.AddTextSize(4, 4);
                _printer.AddTextStyle(Builder.False, Builder.True, Builder.False, Builder.False);
                AddTextWithEncoding(cmd.printTextCenter4xWithUL + "\n");
                _printer.AddTextStyle(Builder.False, Builder.False, Builder.False, Builder.False);
                _printer.AddTextSize(1, 1);
            }
            else if (cmd.printBarcode != null)
            {
                _printer.AddBarcode(cmd.printBarcode,
                    Builder.BarcodeCode128,
                    Builder.HriBelow,
                    Builder.FontA,
                    2,
                    100);
            }
            else if (cmd.print != null)
            {
                // Print command - do nothing, just send
                System.Diagnostics.Debug.WriteLine("ExecuteCommand: Print command");
            }
            else
            {
                // If none of the above matched, this must be a cut command
                // The JSON has { "cut": null } which means all other properties are null
                System.Diagnostics.Debug.WriteLine("ExecuteCommand: Cut command detected (all other properties null) - adding cut");
                _printer.AddCut(Builder.CutFeed);
            }
        }

        private void AddTextWithEncoding(string text)
        {
            // Language is already set once at the beginning via SetPrinterLanguage()
            // Just add text directly - SDK handles encoding based on the language model set during initialization
            _printer.AddText(text);
        }

        private void PrintLogo(string base64Image)
        {
            try
            {
                // Convert base64 string to byte array
                byte[] imageBytes = Convert.FromBase64String(base64Image);

                // Create bitmap from bytes
                Android.Graphics.Bitmap bitmap = Android.Graphics.BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);

                if (bitmap != null)
                {
                    _printer.AddImage(bitmap, 0, 0,
                        bitmap.Width,
                        bitmap.Height,
                        Builder.ColorNone,
                        Builder.ModeGray16,
                        0, // halftone
                        1.0, // brightness
                        0); // compress
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logo print error: {ex.Message}");
            }
        }

        private string GetPrinterErrorMessage(int errorCode) => errorCode switch
        {
            1 => "Invalid printer settings",
            2 => "Connection failed - Check printer power and Bluetooth connection",
            3 => "Connection timeout",
            4 => "Printer not found",
            8 => "Paper end or cover open",
            _ => $"Printer error (Code: {errorCode})"
        };
#endif

        public void Dispose()
        {
#if ANDROID
            try
            {
                if (_isDiscovering) Discovery.Stop();
                _printer?.Disconnect();
                _printer = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Dispose error: {ex.Message}");
            }
#endif
        }
    }

#if ANDROID
    public class DiscoveryListener : Java.Lang.Object, IDiscoveryListener
    {
        public event EventHandler<Com.Epson.Epos2.Discovery.DeviceInfo> OnPrinterFound;

        public void OnDiscovery([GeneratedEnum] Com.Epson.Epos2.Discovery.DeviceInfo deviceInfo)
        {
            if (deviceInfo != null)
                OnPrinterFound?.Invoke(this, deviceInfo);
        }
    }
#endif
}

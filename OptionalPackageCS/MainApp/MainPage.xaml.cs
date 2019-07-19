using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel;
using System.Diagnostics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MainApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            ReadOptionalPackageContent();

        }
        private async void LoadTextFromPackageAsync(Package package)
        {
            try
            {
                Windows.Storage.StorageFolder installFolder = package.InstalledLocation;

                var assetsFolder = await installFolder.GetFolderAsync("Content");
                if (assetsFolder != null)
                {
                    try
                    {
                        var targetFile = await assetsFolder.GetFileAsync("SampleFile.txt");
                        if (targetFile != null)
                        {
                            try
                            {
                                if (targetFile.IsAvailable)
                                {
                                    WriteToTextBox("Found SampleFile.txt - loading contents");
                                    Debug.WriteLine("    %ws is available:\n", targetFile.Name);
                                    var readStream = await targetFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
                                    if (readStream != null)
                                    {
                                        try
                                        {
                                            var size = (uint)readStream.Size;
                                            if (size <= uint.MaxValue)
                                            {
                                                var dataReader = new Windows.Storage.Streams.DataReader(readStream);
                                                var val = await dataReader.LoadAsync(size);
                                                var fileContent = dataReader.ReadString(val);
                                                WriteToTextBox(fileContent);
                                                dataReader.Dispose();
                                                Debug.WriteLine("        %ws\n", fileContent);
                                            }
                                            else
                                            {
                                                readStream.Dispose(); // As a best practice, explicitly close the readStream resource as soon as it is no longer needed.
                                                Debug.WriteLine("    File %ws is too big for LoadAsync to load in a single chunk. Files larger than 4GB need to be broken into multiple chunks to be loaded by LoadAsync.", targetFile.Name);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.WriteLine("    Error 0x%x reading text file\n", ex.HResult);
                                        }
                                    }
                                    else
                                    {
                                        Debug.WriteLine("    File not available\n");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("   Error 0x%x getting text file\n", ex.HResult);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("    Error 0x%x getting Contents folder\n", ex.HResult);
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("    Error 0x%x\n", ex.HResult);
            }
        }

        private void ReadOptionalPackageContent()
        {
            WriteToTextBox("Enumerating Packages");
            var optionalPackages = EnumerateInstalledPackages();
            foreach (var package in optionalPackages)
            {
                var packageName = package.Id.FullName;
                WriteToTextBox(packageName);

                LoadTextFromPackageAsync(package);
                WriteToTextBox("+++++++++++++++++++");
                WriteToTextBox("\n");
            }
            Debug.WriteLine("  Finished loading all optional packages\n");
            Debug.WriteLine("\n");
        }
        private List<Package> EnumerateInstalledPackages()
        {
            Debug.WriteLine("    Searching for optional packages...\n");

            // Obtain the app's package first to then find all related packages
            var currentAppPackage = Package.Current;

            // The dependencies list is where the list of optional packages (OP) can be determined
            var dependencies = currentAppPackage.Dependencies;
            var optionalPackages = new List<Package>();

            foreach (var package in dependencies)
            {
                //  If it is optional, then add it to our results vector
                if (package.IsOptional)
                {
                    Debug.WriteLine("    Optional Package found - %ws\n", package.Id.FullName);
                    optionalPackages.Add(package);
                }
            }

            return optionalPackages;    //  Return the resulting vector
        }

        private void WriteToTextBox(string str)
        {
            var dispatcher = Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
            new Windows.UI.Core.DispatchedHandler(() =>
            {
                ApptextBox.Text += str + "\n";
            }));
        }

    }
}

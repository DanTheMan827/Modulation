using DanTheMan827.TempFolders;
using System;
using System.Diagnostics;
using System.IO;

namespace DanTheMan827.ModulateDotNet
{
    public sealed class ModulateExe: IDisposable
    {
        /// <summary>
        /// Set this to a path before using the shared instance if you want the exe to be unpacked somewhere specific.
        /// </summary>
        public static string TempBasePath { get; set; } = null;
        private static ModulateExe? _lazyModulateExe = null;
        public static ModulateExe Shared => _lazyModulateExe == null ? _lazyModulateExe = new ModulateExe() : _lazyModulateExe;

        public string ExePath { get; private set; }
        private EasyTempFolder tempFolder = new EasyTempFolder("Modulate", TempBasePath);
        private bool disposedValue;

        /// <summary>
        /// Use the Shared static property instead.
        /// </summary>
        private ModulateExe()
        {
            UnpackExe();
        }

        private void UnpackExe()
        {
            ExePath = Path.Combine(tempFolder.Path, "Modulate.exe");

            File.WriteAllBytes(ExePath, Resources.Modulate);
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                if (_lazyModulateExe == this)
                {
                    _lazyModulateExe = new ModulateExe();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~ModulateExe()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

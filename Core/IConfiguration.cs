using System;
using System.Drawing;

namespace Updater.Core
{
    public interface IConfiguration
    {
        string ETag { get; set; }

        void ResetETag();
        void SaveETag();

        string Name { get; }
        Image Background { get; }
        Icon Icon { get; }

        Uri Manifest { get; }
        Uri News { get; }
        string ExePath { get; }
        string InstallDir { get; }
    }
}

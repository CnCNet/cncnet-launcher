using System;
using System.Drawing;

namespace Updater.Core
{
    public interface IConfiguration
    {
        /// <summary>
        /// Manifest ETag that is used to check for updates. Should be saved.
        /// </summary>
        /// <value>The ETag.</value>
        string ETag { get; set; }

        /// <summary>
        /// Resets ETag to default/empty.
        /// </summary>
        void ResetETag();

        /// <summary>
        /// Save ETag to persistent storage.
        /// </summary>
        void SaveETag();

        /// <summary>
        /// Application name that is being updated.
        /// </summary>
        /// <value>The application name.</value>
        string Name { get; }

        /// <summary>
        /// Image that is used for update checking and as the header for update.
        /// </summary>
        /// <remarks>If null, no background is set and the loading window is gray.</remarks>
        /// <value>The background.</value>
        Image Background { get; }

        /// <summary>
        /// Icon that is showed in task bar and in update windows.
        /// </summary>
        /// <remarks>If null, no icon is set.</remarks>
        /// <value>The icon.</value>
        Icon Icon { get; }

        /// <summary>
        /// Full URL to manifest file that contains updates.
        /// </summary>
        /// <remarks>This field is required.</remarks>
        /// <value>The manifest.</value>
        Uri Manifest { get; }

        /// <summary>
        /// News address.
        /// </summary>
        /// <remarks>If null, no news button is shown.</remarks>
        /// <value>The news.</value>
        Uri News { get; }

        /// <summary>
        /// Main executable path that is executed when updates are finished.
        /// </summary>
        /// <value>The main executable path.</value>
        string ExePath { get; }

        /// <summary>
        /// Root directory where updates are installed.
        /// </summary>
        /// <value>The install directory path.</value>
        string InstallDir { get; }

        /// <summary>
        /// Directory where temporary update files are stored. Can be relative to InstallDir or absolute.
        /// </summary>
        /// <value>The update directory path.</value>
        string UpdateDir { get; }
    }
}

using System;
using Updater.Core;

namespace Updater.Simple
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var c = new Configuration();

            foreach (var arg in args)
            {
                if (arg == "-log")
                    Log.Open();
                else if (arg == "-force")
                    c.ResetETag();
            }

            try
            {
                Log.Info("START " + DateTime.Now);
                Bootstrap.Run(c);
            }
            catch (Exception e)
            {
                Log.Error("{0}: {1}", e.GetType(), e.Message);
                throw;
            }
            finally
            {
                Log.Info("END {0}", DateTime.Now);
                Log.Close();
            }
        }
	}
}

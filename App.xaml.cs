using System.Windows;

namespace TrueStretchedValorant
{
    public partial class App : Application
    {
        private AppOrchestrator? _orchestrator;

        public void RegisterOrchestrator(AppOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                _orchestrator?.Shutdown();
            }
            catch
            {
                // Silently ensure we don't prevent Windows shutdown
            }
        }
    }
}

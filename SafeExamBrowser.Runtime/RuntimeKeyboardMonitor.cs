using System;
using System.Windows.Input;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.WindowsApi.Contracts;
using SafeExamBrowser.WindowsApi.Contracts.Events;

namespace SafeExamBrowser.Runtime
{
    internal class RuntimeKeyboardMonitor
    {
        private readonly ILogger logger;
        private readonly INativeMethods nativeMethods;
        private readonly Action shutdownAction;
        private Guid? hookId;

        public RuntimeKeyboardMonitor(ILogger logger, INativeMethods nativeMethods, Action shutdownAction)
        {
            this.logger = logger;
            this.nativeMethods = nativeMethods;
            this.shutdownAction = shutdownAction;
        }

        public void Start()
        {
            if (hookId == null)
            {
                hookId = nativeMethods.RegisterKeyboardHook(KeyboardHookCallback);
                logger.Info("Runtime emergency keyboard hook started.");
            }
        }

        public void Stop()
        {
            if (hookId.HasValue)
            {
                nativeMethods.DeregisterKeyboardHook(hookId.Value);
                hookId = null;
                logger.Info("Runtime emergency keyboard hook stopped.");
            }
        }

        private bool KeyboardHookCallback(int keyCode, KeyModifier modifier, KeyState state)
        {
            var key = KeyInterop.KeyFromVirtualKey(keyCode);

            // Emergency Exit: Ctrl + Alt + End
            if (modifier.HasFlag(KeyModifier.Ctrl) && modifier.HasFlag(KeyModifier.Alt) && key == Key.End)
            {
                logger.Warn("EMERGENCY SHUTDOWN TRIGGERED via Ctrl + Alt + End!");
                System.Threading.Tasks.Task.Run(() => shutdownAction?.Invoke());
                return true;
            }

            return false; // Don't block anything else in the runtime
        }
    }
}

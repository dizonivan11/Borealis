using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public delegate void ThreadSafeHandler();

namespace Borealis.Net {

    public static class ThreadSafeExtensions {
        public static void RunSafely(this Control control, ThreadSafeHandler action) {
            if (control.InvokeRequired) {
                control.Invoke(action);
            } else {
                action.Invoke();
            }
        }
    }
}

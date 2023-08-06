using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Superdp
{    public class MultiFormContext : ApplicationContext
    {
        // https://stackoverflow.com/questions/15300887/run-two-winform-windows-simultaneously
        private int openForms = 0;

        public void AddForm(Form form)
        {
            openForms++;
            form.FormClosed += (s, args) =>
            {
                //When we have closed the last of the "starting" forms, 
                //end the program.
                if (Interlocked.Decrement(ref openForms) == 0)
                    ExitThread();
            };

            form.Show();
        }
    }
}

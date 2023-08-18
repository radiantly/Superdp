namespace Superdp
{
    public class MultiFormContext : ApplicationContext
    {
        // https://stackoverflow.com/questions/15300887/run-two-winform-windows-simultaneously
        private int openForms = 0;

        public void AddForm(Form form)
        {
            openForms++;
            form.FormClosed += (s, args) =>
            {
                // End the program when all forms are closed
                if (Interlocked.Decrement(ref openForms) == 0)
                    ExitThread();
            };

            form.Show();
        }
    }
}

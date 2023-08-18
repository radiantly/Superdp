namespace Superdp
{
    static class RDPFormManager
    {
        readonly private static Dictionary<string, RDPForm> rdpForms = new();
        readonly private static HashSet<HeroForm> heroForms = new();

        private static RDPForm Get(HeroForm form, string clientId)
        {
            if (!heroForms.Contains(form))
            {
                heroForms.Add(form);
                form.FormClosing += (s, e) =>
                {
                    rdpForms
                        .Where(entry => entry.Value.OwningForm == form)
                        .Select(entry => entry.Key)
                        .ToList()
                        .ForEach(client_id => rdpForms.Remove(client_id));
                    heroForms.Remove(form);
                };
            }

            if (!rdpForms.ContainsKey(clientId))
            {
                var rdpForm = new RDPForm(form, clientId);
                rdpForms.Add(clientId, rdpForm);
            }

            rdpForms[clientId].OwningForm = form;
            return rdpForms[clientId];
        }

        public static void Connect(HeroForm form, string clientId, string host, string username, string password)
        {
            var rdpForm = Get(form, clientId);
            rdpForm.Connect(host, username, password);
        }

        public static void Disconnect(HeroForm form, string clientId)
        {
            var rdpForm = Get(form, clientId);
            rdpForm.Disconnect();
        }

        public static void SetCharacteristics(HeroForm form, string clientId, Point location, Size size, bool visible)
        {
            var rdpForm = Get(form, clientId);
            rdpForm.SetPositioning(location, size);
            rdpForm.ShouldBeVisible = visible;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSTSCLib;

namespace Superdp
{
    static class RDPFormManager
    {
        readonly private static Dictionary<string, RDPForm> rdpForms = new();

        private static RDPForm Get(HeroForm form, string clientId)
        {
            if (!rdpForms.ContainsKey(clientId))
            {
                var rdpForm = new RDPForm(form, clientId);
                rdpForms.Add(clientId, rdpForm);
            }

            return rdpForms[clientId];
        }

        public static void Connect(HeroForm form, string clientId, string host, string username, string password)
        {
            var rdpForm = Get(form, clientId);
            rdpForm.OwningForm = form;
            rdpForm.Connect(host, username, password);
        }

        public static void SetVisibility(HeroForm form, string clientId, bool visible) => Get(form, clientId).ShouldBeVisible = visible;
        public static void SetSize(HeroForm form, string clientId, Size size) => Get(form, clientId).SetPositioning(new Point(0, 35), size);
    }
}

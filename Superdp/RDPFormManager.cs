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

        public static void Disconnect(HeroForm form, string clientId)
        {
            var rdpForm = Get(form, clientId);
            Debug.Assert(form == rdpForm.OwningForm);
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

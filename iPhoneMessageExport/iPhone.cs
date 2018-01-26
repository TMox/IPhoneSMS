using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iPhoneSMS
{
    public class iPhone
    {
        List<iPhoneBackup.Person> _people;
        Dictionary<string, string> _contacts;

        public iPhone()
        {

        }

        public void GetContacts(iPhoneBackup backup)
        {
            _people = backup.GetiPhoneContacts();
            _contacts = new Dictionary<string, string>();
            _people.ForEach((p) => p.Contacts.ForEach((c) => _contacts[c.value] = p.FullName));
        }

        public string GroupNames(List<string> ids)
        {
            List<string> names = new List<string>(ids.Count);
            string s;
            ids.ForEach((i) => { if (!_contacts.TryGetValue(i, out s)) names.Add(i); else names.Add(s); });
            return string.Join(", ", names);
        }
        public string IdToName(string id)
        {
            string s;
            if (!_contacts.TryGetValue(id, out s))
                return id;
            return s.Trim();
        }
        public string IdToNameUnknown(string id)
        {
            if (id[0] != '+' && !char.IsNumber(id[0]))
                return "(Unknown)";
            string s;
            if (!_contacts.TryGetValue(id, out s))
                return "(Unknown)";
            return s;
        }
    }
}

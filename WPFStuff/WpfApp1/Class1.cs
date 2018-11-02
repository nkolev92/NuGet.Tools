using System;
using System.Collections.Generic;

namespace WpfApp1
{

    public class LicenseInformationModel
    {
        public string Id { get; }
        public string Authors { get; }
        public IList<Text> License { get; }

        public LicenseInformationModel(string id, string author, IList<Text> texts)
        {
            Id = id;
            Authors = author;
            License = texts;
        }

    }

    public class LicenseText : Text
    {
        public LicenseText(string text, Uri link)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Link = link ?? throw new ArgumentNullException(nameof(link));
        }

        public string Text { get; set; }
        public Uri Link { get; set; }
    }

    public class FreeText : Text
    {
        public FreeText(string text)
        {
            Text = text;
        }

        public string Text { get; }
    }

    public interface Text
    {
        string Text { get; }
    }
}

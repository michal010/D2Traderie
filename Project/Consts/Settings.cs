using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace D2Traderie.Project.Consts
{
    static class Settings
    {
        public static readonly string BaseEndpoint = "https://traderie.com/api/diablo2resurrected/";
        public static readonly string ItemsFileDataName = "Items";
        public static readonly string ItemsFileDataFullName = "Items.json";
        public static readonly string ItemTagsDataName = "ItemTags";
        public static readonly string ItemTagsDataFullName = "ItemTags.json";
        //public static string DataPath => Path.GetDirectoryName(Application.)
    }
}

// /*
// Copyright (C) 2015 Robin Hermanussen
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program. If not, see http://www.gnu.org/licenses/.
// */
using Sitecore.CodeGenerator.Domain;
using Sitecore.CodeGenerator.Serialization;
using Sitecore.Data;
using Sitecore.Data.Serialization.ObjectModel;
using Sitecore.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.CodeGenerator
{
    public class TemplatesResolver
    {
        public List<TemplateItem> Templates { get; private set; }

        public TemplatesResolver(
            string serializationPath,
            string[] includePaths,
            string db = "master")
        {
            Assert.ArgumentNotNullOrEmpty(serializationPath, "serializationPath");
            DirectoryInfo serializationFolder = new DirectoryInfo(serializationPath);
            Assert.IsTrue(serializationFolder.Exists, string.Format("Path '{0}' does not exist", serializationPath));

            List<SyncItem> syncItems = GetAllItems(serializationFolder, db, includePaths);

            Templates = syncItems
                .Where(s => s.TemplateID == TemplateIDs.Template.ToString())
                .Select(t => new TemplateItem(t, syncItems))
                .ToList();

            Dictionary<string, TemplateItem> templateLookup = Templates.ToDictionary(t => t.SyncItem.ID, t => t);

            // resolve inheritance hierarchy
            foreach (TemplateItem templateItem in Templates)
            {
                SyncField baseTemplates = templateItem.SyncItem.SharedFields
                    .FirstOrDefault(f => f.FieldID == FieldIDs.BaseTemplate.ToString());
                if (baseTemplates != null && !string.IsNullOrWhiteSpace(baseTemplates.FieldValue))
                {
                    ID[] baseTemplateIds = ID.ParseArray(baseTemplates.FieldValue);
                    var baseTemplateIdsInCurrentSet = baseTemplateIds
                        .Where(b => templateLookup.Keys.Contains(b.ToString()));
                    templateItem.BaseTemplates.AddRange(baseTemplateIdsInCurrentSet
                        .Select(b => templateLookup[b.ToString()]));

                    // resolve base templates outside of resolving set
                    foreach (ID baseTemplateId in baseTemplateIds
                        .Where(b => ! templateItem.BaseTemplates.Any(bt => bt.SyncItem.ID == b.ToString())))
                    {
                        string baseTemplateFilePath = baseTemplateId.FindFilePath(serializationFolder);
                        if (string.IsNullOrWhiteSpace(baseTemplateFilePath))
                        {
                            continue;
                        }
                        FileInfo baseTemplateFile = new FileInfo(baseTemplateFilePath);
                        if (! baseTemplateFile.Exists)
                        {
                            continue;
                        }
                        SyncItem baseTemplateSyncItem = SyncItem.ReadItem(new Tokenizer(baseTemplateFile.OpenText()));
                        if (baseTemplateSyncItem == null)
                        {
                            continue;
                        }
                        if (! templateItem.BaseTemplates.Any(b => b.SyncItem.ID == baseTemplateSyncItem.ID))
                        {
                            templateItem.BaseTemplates.Add(new TemplateItem(baseTemplateSyncItem, new List<SyncItem>()));
                        }
                    }
                }
            }
        }

        private List<SyncItem> GetAllItems(DirectoryInfo folder, string db, string[] includePaths)
        {
            List<SyncItem> result = new List<SyncItem>();
            foreach (FileInfo itemFile in folder.GetFiles("*.item", SearchOption.AllDirectories))
            {
                using (StreamReader sr = new StreamReader(itemFile.FullName))
                {
                    sr.ReadLine();
                    sr.ReadLine();
                    sr.ReadLine();
                    string dbStr = sr.ReadLine().Substring(10);
                    if (dbStr != db)
                    {
                        continue;
                    }
                    string pathStr = sr.ReadLine().Substring(6);
                    if (! includePaths.Any(p => pathStr.StartsWith(p)))
                    {
                        continue;
                    }
                }
                try
                {
                    result.Add(SyncItem.ReadItem(new Tokenizer(itemFile.OpenText())));
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Unable to deserialize '{0}'", itemFile.FullName, itemFile.FullName), ex);
                }
            }
            return result;
        }
    }
}

// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program. If not, see http://www.gnu.org/licenses/.
// */
namespace Sitecore.CodeGenerator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Data;
    using Data.Serialization.ObjectModel;
    using Diagnostics;
    using Domain;
    using Serialization;

    public abstract class TemplatesResolverBase
    {
        public List<TemplateItem> Templates { get; private set; }

        public TemplatesResolverBase(
            string serializationPath,
            string[] includePaths,
            string db = "master")
        {
            Assert.ArgumentNotNullOrEmpty(serializationPath, "serializationPath");
            DirectoryInfo serializationFolder = new DirectoryInfo(serializationPath);
            Assert.IsTrue(serializationFolder.Exists, string.Format("Path '{0}' does not exist", serializationPath));

            List<SyncItem> syncItems = GetAllItems(serializationFolder, db, includePaths);

            this.Templates = syncItems
                .Where(s => s.TemplateID == TemplateIDs.Template.ToString())
                .Select(t => new TemplateItem(t, syncItems))
                .ToList();

            Dictionary<string, TemplateItem> templateLookup = this.Templates.ToDictionary(t => t.SyncItem.ID, t => t);

            // resolve inheritance hierarchy
            foreach (TemplateItem templateItem in this.Templates)
            {
                SyncField baseTemplates = templateItem.SyncItem.SharedFields
                    .FirstOrDefault(f => f.FieldID == FieldIDs.BaseTemplate.ToString());
                if (baseTemplates != null && !string.IsNullOrWhiteSpace(baseTemplates.FieldValue))
                {
                    ID[] baseTemplateIds = baseTemplates.FieldValue
                        .Split(new [] { '|', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(ID.IsID)
                        .Select(ID.Parse)
                        .ToArray();
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

        protected abstract List<SyncItem> GetAllItems(DirectoryInfo folder, string db, string[] includePaths);
    }
}
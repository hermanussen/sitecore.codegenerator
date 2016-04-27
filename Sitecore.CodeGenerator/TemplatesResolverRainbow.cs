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
    using Rainbow.Storage.Yaml;

    public class TemplatesResolverRainbow : TemplatesResolverBase
    {
        public TemplatesResolverRainbow(string serializationPath, string[] includePaths, string db = "master")
            : base(serializationPath, includePaths, db)
        {
        }

        protected override List<SyncItem> GetAllItems(DirectoryInfo folder, string db, string[] includePaths)
        {
            var formatter = new YamlSerializationFormatter(null, null);

            List<SyncItem> result = new List<SyncItem>();
            foreach (FileInfo itemFile in folder.GetFiles("*.yml", SearchOption.AllDirectories))
            {
                using (StreamReader sr = new StreamReader(itemFile.FullName))
                {
                    try
                    {
                        var item = formatter.ReadSerializedItem(sr.BaseStream, itemFile.Name);
                        if (item == null || !includePaths.Any(p => item.Path.StartsWith(p)))
                        {
                            continue;
                        }

                        var syncItem = new SyncItem()
                            {
                                Name = item.Name,
                                BranchId = new ID(item.BranchId).ToString(),
                                DatabaseName = item.DatabaseName,
                                ID = new ID(item.Id).ToString(),
                                ItemPath = item.Path,
                                TemplateID = new ID(item.TemplateId).ToString(),
                                ParentID = new ID(item.ParentId).ToString()
                            };
                        foreach (var version in item.Versions)
                        {
                            var syncVersion = syncItem.AddVersion(version.Language.Name, version.VersionNumber.ToString(), version.VersionNumber.ToString());
                            foreach (var field in version.Fields)
                            {
                                syncVersion.AddField(new ID(field.FieldId).ToString(), field.NameHint, field.NameHint, field.Value, true);
                            }
                        }

                        foreach (var sharedField in item.SharedFields)
                        {
                            syncItem.AddSharedField(new ID(sharedField.FieldId).ToString(), sharedField.NameHint, sharedField.NameHint, sharedField.Value, true);
                        }

                        foreach (var unversionedField in item.UnversionedFields)
                        {
                            foreach (var version in syncItem.Versions.Where(v => v.Language == unversionedField.Language.Name))
                            {
                                foreach (var itemFieldValue in unversionedField.Fields)
                                {
                                    version.AddField(new ID(itemFieldValue.FieldId).ToString(), itemFieldValue.NameHint, itemFieldValue.NameHint, itemFieldValue.Value, true);
                                }
                            }
                        }

                        result.Add(syncItem);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Unable to deserialize '{0}'", itemFile.FullName), ex);
                    }
                }
            }
            return result;
        }
    }
}

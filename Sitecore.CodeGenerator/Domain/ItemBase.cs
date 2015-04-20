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
using System.Linq;
using Sitecore.Data.Serialization.ObjectModel;

namespace Sitecore.CodeGenerator.Domain
{
    /// <summary>
    /// Represents a deserialized item.
    /// Exposes the SyncItem, which can be used to get the contents of the items.
    /// </summary>
    public abstract class ItemBase
    {
        public ItemBase(SyncItem syncItem)
        {
            SyncItem = syncItem;
        }

        /// <summary>
        /// The Sitecore SyncItem, which contains all info from the .item file.
        /// </summary>
        public SyncItem SyncItem { get; private set; }

        /// <summary>
        /// Returns the shared field value, based on the field name.
        /// </summary>
        /// <param name="name">Name of the field</param>
        /// <param name="defaultValue">If provided, will fallback to this value if no field value is available</param>
        /// <returns>The field value</returns>
        public string GetSharedFieldValue(string name, string defaultValue = null)
        {
            SyncField typeField = SyncItem.SharedFields
                                          .FirstOrDefault(f => name.Equals(f.FieldName));
            return typeField != null && ! string.IsNullOrWhiteSpace(typeField.FieldValue)
                       ? typeField.FieldValue
                       : defaultValue;
        }

        /// <summary>
        /// Returns the field value in the first version it can find, based on the field name.
        /// </summary>
        /// <param name="name">Name of the field</param>
        /// <param name="defaultValue">If provided, will fallback to this value if no field value is available</param>
        /// <returns>The field value</returns>
        public string GetFieldValue(string name, string defaultValue = null)
        {
            SyncField typeField = SyncItem.Versions.SelectMany(v => v.Fields)
                                          .FirstOrDefault(f => name.Equals(f.FieldName));
            return typeField != null && !string.IsNullOrWhiteSpace(typeField.FieldValue)
                       ? typeField.FieldValue
                       : defaultValue;
        }
    }
}
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data.Serialization.ObjectModel;

namespace Sitecore.CodeGenerator.Domain
{
    /// <summary>
    /// Represents a deserialized template section.
    /// </summary>
    public class TemplateSection : ItemBase
    {
        /// <summary>
        /// The fields within this template section.
        /// </summary>
        public List<TemplateField> Fields { get; private set; }

        public TemplateSection(SyncItem sectionItem, List<SyncItem> syncItems)
            : base(sectionItem)
        {
            Fields = syncItems
                .Where(s => s.TemplateID == TemplateIDs.TemplateField.ToString() && s.ParentID == sectionItem.ID)
                .Select(s => new TemplateField(s))
                .ToList();
        }
    }
}

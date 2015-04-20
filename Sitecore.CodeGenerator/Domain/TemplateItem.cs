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
using Sitecore.Data.Serialization.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.CodeGenerator.Domain
{
    /// <summary>
    /// Represents a deserialized template item.
    /// </summary>
    public class TemplateItem : ItemBase
    {
        /// <summary>
        /// All sections within the template, excluding inherited ones.
        /// </summary>
        public List<TemplateSection> Sections { get; private set; }

        /// <summary>
        /// Direct base templates for the current template.
        /// </summary>
        public List<TemplateItem> BaseTemplates { get; private set; }

        public TemplateItem(SyncItem templateItem, List<SyncItem> syncItems)
            : base(templateItem)
        {
            BaseTemplates = new List<TemplateItem>();
            Sections = syncItems
                .Where(s => s.TemplateID == TemplateIDs.TemplateSection.ToString() && s.ParentID == templateItem.ID)
                .Select(s => new TemplateSection(s, syncItems))
                .ToList();
        }
    }
}

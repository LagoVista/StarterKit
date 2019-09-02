using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.IoT.StarterKit.Managers
{
    public abstract class InitializerBase
    {
        protected void AddAuditProperties(IAuditableEntity entity, DateTime creationTimeStamp, EntityHeader org, EntityHeader user)
        {
            entity.CreationDate = creationTimeStamp.ToJSONString();
            entity.LastUpdatedDate = creationTimeStamp.ToJSONString();
            entity.CreatedBy = user;
            entity.LastUpdatedBy = user;
        }

        protected void AddOwnedProperties(IOwnedEntity entity, EntityHeader org)
        {
            entity.OwnerOrganization = org;
            entity.IsPublic = false;
        }

        protected void AddId(IIDEntity entity)
        {
            entity.Id = Guid.NewGuid().ToId();
        }
    }
}

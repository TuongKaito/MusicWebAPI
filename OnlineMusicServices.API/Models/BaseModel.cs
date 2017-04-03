using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineMusicServices.API.Models
{
    public abstract class BaseModel<T>
    {
        public abstract void UpdateEntity(T entity);
        public abstract void CopyEntityData(T entity);
    }
}
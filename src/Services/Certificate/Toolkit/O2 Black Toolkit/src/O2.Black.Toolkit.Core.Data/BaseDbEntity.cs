using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace O2.Black.Toolkit.Core.Data
{
    public class BaseDbEntity : IEntity
    {
        protected BaseDbEntity()
        {
            CreateEntity();
        }

        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("added_date")]
        public long AddedDate { get; set; }

        [Column("modified_date")]
        public long ModifiedDate { get; set; }
        
        public virtual void UpdateEntity()
        {
            ModifiedDate = DateTime.Now.ConvertToUnixTime();
            foreach (var propertyInfo in GetType()
                .GetProperties(
                    BindingFlags.Public 
                    | BindingFlags.Instance))
            {
                if (typeof(IEntity).IsAssignableFrom(propertyInfo.PropertyType))
                {
                    (propertyInfo as IEntity)?.UpdateEntity();
                }
            }
        }

        public void CreateEntity()
        {
            Id = Guid.NewGuid();
            AddedDate = DateTime.Now.ConvertToUnixTime();
            ModifiedDate = AddedDate;
            foreach (var propertyInfo in GetType()
                .GetProperties(
                    BindingFlags.Public 
                    | BindingFlags.Instance))
            {
                if (typeof(IEntity).IsAssignableFrom(propertyInfo.PropertyType))
                {
                    (propertyInfo as IEntity)?.CreateEntity();
                }
            }
        }
    }
}
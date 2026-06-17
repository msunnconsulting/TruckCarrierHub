namespace PartnerCarrier.Infrastructure.Database
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Validation;
    using System.Linq;
    using System.Reflection;

    public partial class PartnerCarrier_DevEntities : DbContext
    {
        private readonly string[] CreatedOnColumns;
        private readonly string[] UpdatedOnColumns;

        public PartnerCarrier_DevEntities(bool enableLogging = true, string logFilePath = null, bool clearOnNewContext = true, string[] createdOnColumnNames = null, string[] updatedOnColumnNames = null)
            : base("name=PartnerCarrier_DevEntities")
        {
            if (enableLogging)
            {
                SQLLogger.Init(logFilePath, clearOnNewContext);
                //var logger = new SQLLogger(logFilePath, clearOnNewContext);
                Database.Log = s => SQLLogger.Log(s);
            }

            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            Configuration.ValidateOnSaveEnabled = true;
            Configuration.AutoDetectChangesEnabled = true; // http://stackoverflow.com/questions/16863382/dbcontext-autodetectchangesenabled-set-to-false-detecting-changes

            CreatedOnColumns = createdOnColumnNames;
            UpdatedOnColumns = updatedOnColumnNames;
        }

        ////http://stackoverflow.com/questions/12871892/entity-framework-validation-with-partial-updates

        /// <summary>
        /// Saves all unsaved changes to database.
        /// </summary>
        /// <returns>The number of objects written to the database.</returns>
        public override int SaveChanges()
        {
            ChangeTracker.DetectChanges(); // Important!            

            var dateNow = DateTime.Now;

            // for created on
            if (CreatedOnColumns != null)
            {
                ChangeTracker.Entries().ToList().ForEach(
                    e =>
                    {
                        e.Entity.GetType().GetProperties().Where(p =>
                            p.CanWrite
                            && (p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?))
                            && (CreatedOnColumns.Any(c => c == p.Name) || UpdatedOnColumns.Any(c => c == p.Name)) // set createdon or updatedon on create operation
                            ).ToList().ForEach(property =>
                            {
                                if (e.State == EntityState.Added) // no matter its CreatedOn or UpdatedOn, just set date value
                                    property.SetValue(e.Entity, dateNow);
                                else if (e.State == EntityState.Modified)
                                {
                                    if (CreatedOnColumns.Any(c => c == property.Name))
                                    {
                                        // if we are on CreatedOnColumn, mark it as UnModified
                                        e.Property(property.Name).IsModified = false;
                                    }
                                    else
                                    {
                                        // we are on UpdatedOnColumn, set its value to new date value
                                        property.SetValue(e.Entity, dateNow);
                                    }
                                }

                            });
                    }
                    );
            }

            //foreach (var entityEntry in changeSet)
            //{
            //    switch (entityEntry.State)
            //    {
            //        case EntityState.Added:
            //            entityEntry.Entity.CreatedOn = dateNow;
            //            entityEntry.Entity.UpdatedOn = dateNow;
            //            break;
            //        case EntityState.Modified:
            //            entityEntry.Property(m => m.CreatedOn).IsModified = false;
            //            entityEntry.Entity.UpdatedOn = dateNow;
            //            break;
            //    }
            //}

            return base.SaveChanges();
        }

        /// <summary>
        /// Validates entities being sent to database
        /// </summary>
        /// <param name="entityEntry">entityEntry for the items</param>
        /// <param name="items">items being validated</param>
        /// <returns>Returns validation result</returns>
        protected override DbEntityValidationResult ValidateEntity(DbEntityEntry entityEntry, IDictionary<object, object> items)
        {
            DbEntityValidationResult validationResult = base.ValidateEntity(entityEntry, items);

            // clear all those property which is not suppose to be validated
            // all those properties which is not modified, shouldn't be validated.
            // for newly added entity, nothing to be done.
            // for modified entity, only those properties which is modified=true, should be considered for validation to handle partial update            

            if (entityEntry.State == EntityState.Modified)
            {
                Type type = entityEntry.Entity.GetType();

                // do not do anything for virtual property. We'll leave it up to entity framework to handle it
                IEnumerable<PropertyInfo> properties = type.GetProperties().Where(m => !m.GetGetMethod().IsVirtual);
                foreach (PropertyInfo property in properties)
                {
                    // clearing non modified properties
                    if (entityEntry.Property(property.Name).IsModified == false)
                    {
                        var validationError = validationResult.ValidationErrors.FirstOrDefault(m => m.PropertyName == property.Name);
                        while (validationError != null)
                        {
                            // if there is any error for a non modified property, remove it.
                            validationResult.ValidationErrors.Remove(validationError);
                            validationError = validationResult.ValidationErrors.FirstOrDefault(m => m.PropertyName == property.Name); // moving to next error
                        }
                    }
                }
            }

            return validationResult; // result would not include error for nonmodified properties.
        }

        /// <summary>
        /// Handles disposing current dbContext object properly
        /// </summary>
        /// <param name="disposing">true indicates disposing</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}

namespace PartnerCarrier.Infrastructure.Database
{
    using Common.Utility;
    using Common.Utility.Extensions;
    using Common.Utility.ViewModels;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Reflection;

    //using System.Linq.Expressions;

    public static class EFExt
    {
        #region Select

        /// <summary>
        /// Select recor    s with pagination
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageIndex">1 based page index if using pagination</param>
        /// <param name="pageSize">number of entities to be fetched if using pagination</param>
        /// <param name="sortExpression">sortExpression required to setup Sorting object of PagedList</param>
        /// <param name="sortDirection">sortDirection required to setup Sorting object of PagedList</param>        
        /// <returns>Returns records by pagination</returns>
        public static PagedList<T> SelectByPaging<T>(
    this IQueryable<T> query,
    int pageIndex,
    int pageSize,
    string sortExpression,
    SortingDirection sortDirection,
    int? totalCountOverride = null
) where T : class
        {
            if (pageIndex <= 0)
                throw new Exception("pageIndex must be greater than or equal to 1");

            if (pageSize <= 0)
                throw new Exception("pageSize must be greater than or equal to 1");

            if (string.IsNullOrEmpty(sortExpression))
                throw new Exception("sortExpression must be supplied");

            int totalCount = totalCountOverride ?? query.Count();

            PagedList<T> pagedList = null;

            if (totalCount == 0)
            {
                pagedList = new PagedList<T>(pageIndex, pageSize, totalCount, sortExpression, sortDirection);
                pagedList.Items = new List<T>();
            }
            else
            {
                bool tried = false;
                query = query.AsNoTracking();
                // let's consider pagination query is always readonly and improve the performance here

                // 1 based pageIndex.
                int skip = (pageIndex - 1) * pageSize;
                query = query.Skip(skip).Take(pageSize);

                List<T> items = query.ToList();
                while (items.Count == 0 && !tried)
                {
                    // try to fetch previous page once
                    tried = true;
                    pageIndex--;
                    skip = (pageIndex - 1) * pageSize;
                    query = query.Skip(skip).Take(pageSize);
                    items = query.ToList();
                }

                if (items.Count == 0 && pageIndex != 1)
                {
                    // try once to fetch first page, if its still not showing any records
                    pageIndex = 1;
                    skip = (pageIndex - 1) * pageSize;
                    query = query.Skip(skip).Take(pageSize);
                    items = query.ToList();
                }

                // at last set the result.
                pagedList = new PagedList<T>(pageIndex, pageSize, totalCount, sortExpression, sortDirection);
                pagedList.Items = items;
            }

            return pagedList;
        }

        #endregion

        #region Change Entity State

        /// <summary>
        /// Add passed entity in dbContext with state = Added and returns it.
        /// </summary>
        /// <param name="entity">Entity to be added in dbcontext</param>
        /// <returns>Returns tracked entity with state = Added</returns>
        public static T MarkAsAdded<T>(this DbSet<T> dbSet, T entity) where T : class
        {
            return dbSet.Add(entity);
        }

        /// <summary>
        /// Mark entity as Modified in dbContext. If entity doesn't exist in dbContext, it would add and set the state Modified.
        /// </summary>
        /// <param name="entity">Entity to be added in dbcontext</param>
        /// <returns>Returns tracked entity with state = Modified</returns>
        public static T MarkAsUpdated<T>(this DbSet<T> dbSet, DbContext db, T entity) where T : class
        {
            Attach(dbSet, db, entity);
            db.Entry(entity).State = EntityState.Modified;
            return entity;
        }

        /// <summary>
        /// Mark only few properties of an entity as Modified. It doesn't mark the entity itself as modified. Subsequent Save changes will update those properties in database.
        /// Can update only properties with primiteive types, string, guid and datetime. It will ignore any other types
        /// </summary>
        /// <param name="entity">entity to be partially updated</param>        
        /// <param name="isInclude">indicates if only properties provided should be marked as updated or only they are the only properties should not be updated</param>
        /// <param name="properties">properties to be included or excluded</param>
        public static void MarkAsPartialUpdated<T>(this DbSet<T> dbSet, DbContext db, T entity, bool isInclude, params string[] properties) where T : class
        {
            Attach(dbSet, db, entity);

            if (properties == null || properties.Length == 0)
                throw new ArgumentNullException("properties");

            DbEntityEntry<T> entry = db.Entry(entity);
            Type type = entity.GetType();

            IEnumerable<PropertyInfo> entityProperties;
            if (isInclude)
            {
                entityProperties = type.GetProperties().Where(m => properties.Any(p => p.Equals(m.Name)));
                // for include, if user specified any property which is virtual, show error
                // virtual properties are Navigation properties which we doesn't support for partial updates
                if (entityProperties.Any(m => m.GetGetMethod().IsVirtual))
                {

                    ArrayList affectedTypes = new ArrayList(entityProperties.Where(m => m.GetGetMethod().IsVirtual).Select(m => (m.Name + " - Type(" + m.PropertyType.Name + ")")).ToArray());
                    throw new Exception(
                        "Ingenious.EF.DAL Doesn't Support Partial Update On Navigation Properties. Partial Update can only not work on any virtual properties. If you see this error, please contact Ingenious.EF.DAL Library Manager."
                        + Environment.NewLine
                        + "Problems In Fields : " + affectedTypes.ToString(", ")
                        );
                }
            }
            else
            {
                // for exclude, remove any property which is virtual
                entityProperties = type.GetProperties().Where(m => properties.Any(p => !p.Equals(m.Name)) && !m.GetGetMethod().IsVirtual);
            }

            foreach (PropertyInfo property in entityProperties)
                entry.Property(property.Name).IsModified = true;
        }

        /// <summary>
        /// Mark entity as Deleted in dbContext. If entity doesn't exist in dbContext, it would add and set the state. Returns entity in deleted state. Entity must have its PK set otherwise it would not be deleted from database.
        /// </summary>
        /// <param name="entity">entity to be marked as deleted</param>
        /// <returns>entity marked as deleted</returns>
        public static T MarkAsDeleted<T>(this DbSet<T> dbSet, DbContext db, T entity) where T : class
        {
            if (entity == null) return entity;
            Attach(dbSet, db, entity);
            dbSet.Remove(entity);
            return entity;
        }

        /// <summary>
        /// Method that manage concurrency between dbcontext and actual data store for Partial db updates. Must be called after SaveChanges for any partial update or else entities will remain in inconsistent state.
        /// </summary>
        /// <param name="entity">detached entity that has been partially updated</param>
        public static void MarkPartialUpdateCompleted<T>(this DbSet<T> dbSet, DbContext db, T entity) where T : class
        {
            Detach(dbSet, db, entity);
        }

        /// <summary>
        /// Mark entity as Detached in dbContext. This entity will not be part of SaveChanges now
        /// </summary>
        /// <param name="entity">entity to be detached</param>
        public static void Detach<T>(this DbSet<T> dbSet, DbContext db, T entity) where T : class
        {
            ((System.Data.Entity.Infrastructure.IObjectContextAdapter)db).ObjectContext.Detach(entity);
        }

        /// <summary>
        /// Mark entity as Unchanged in dbContext. If entity doesn't exist in dbContext, it would add and set the state Unchanged.
        /// </summary>
        /// <param name="entity">entity to be marked unchanged</param>
        public static void MarkAsUnchanged<T>(this DbSet<T> dbSet, DbContext db, T entity) where T : class
        {
            Attach(dbSet, db, entity);
            db.Entry(entity).State = EntityState.Unchanged;
        }

        #endregion

        #region CRUD In Database

        /// <summary>
        /// Save new entity in database and returns newly created attached entity in State Unchanged
        /// </summary>
        /// <param name="entity">entity to be created</param>
        /// <returns>Newly created attached entity in State Unchanged</returns>
        public static T Insert<T>(this DbSet<T> dbSet, DbContext db, T entity) where T : class
        {
            MarkAsAdded(dbSet, entity);
            db.SaveChanges();
            return entity;
        }

        /// <summary>
        /// It updates all fields of entity in database and return modified entity in Unchanged state.
        /// </summary>
        /// <param name="entity">entity to be modified</param>
        /// <returns>Modified attached entity in State Unchanged</returns>
        public static T Update<T>(this DbSet<T> dbSet, DbContext db, T entity) where T : class
        {
            MarkAsUpdated(dbSet, db, entity);
            db.SaveChanges();
            return entity;
        }

        /// <summary>
        /// Updates entity with only few properties in database. Returns detached entity that has been partially updated. No need to call MarkPartialUpdateCompleted here.
        /// </summary>
        /// <param name="entity">entity to be partially updated</param>        
        /// <param name="isInclude">indicates if only properties provided should be marked as updated or only they are the only properties should not be updated</param>
        /// <param name="properties">properties to be included or excluded</param>
        /// <returns>detached entity that has been partially updated</returns>
        public static T UpdatePartial<T>(this DbSet<T> dbSet, DbContext db, T entity, bool isInclude, params string[] properties) where T : class
        {
            MarkAsPartialUpdated(dbSet, db, entity, isInclude, properties);
            db.SaveChanges();
            MarkPartialUpdateCompleted(dbSet, db, entity);
            return entity;
        }

        /// <summary>
        /// delete entity passed from database and returns number of records deleted. Must have pk set for this entity
        /// </summary>
        /// <param name="entity">entity to be deleted</param>
        /// <returns>Number of records deleted</returns>
        public static int Delete<T>(this DbSet<T> dbSet, DbContext db, T entity) where T : class
        {
            MarkAsDeleted(dbSet, db, entity);
            return db.SaveChanges();
        }

        #endregion

        #region Is Status

        /// <summary>
        /// Used to find if entity is in added state. Added means SaveChanges will generate Insert query for this entity.
        /// </summary>        
        /// <param name="entity">entity whose state need to be known</param>
        /// <returns>True if entity is attached in dbcontext and is in added state</returns>
        public static bool IsAdded<T>(this DbSet<T> dbSet, DbContext db, T entity) where T : class
        {
            DbEntityEntry<T> entry = db.Entry(entity);
            return (entry.State == EntityState.Added);
        }

        /// <summary>
        /// Used to find if entity is in modified state. Modified means SaveChanges will generate Update query for this entity for all the properties which has IsModified flag true.
        /// </summary>        
        /// <param name="entity">entity whose state need to be known</param>
        /// <returns>True if entity is attached in dbcontext and is in modified state</returns>
        public static bool IsModified<T>(DbSet<T> dbSet, DbContext db, T entity) where T : class
        {
            DbEntityEntry<T> entry = db.Entry(entity);
            return (entry.State == EntityState.Modified);
        }

        /// <summary>
        /// Used to find if entity is in deleted state. deleted means entity has been marked for deletion from the database the next time SaveChanges is called
        /// </summary>        
        /// <param name="entity">entity whose state need to be known</param>
        /// <returns>True if entity is attached in dbcontext and is in deleted state</returns>
        public static bool IsDeleted<T>(DbSet<T> dbSet, DbContext db, T entity) where T : class
        {
            DbEntityEntry<T> entry = db.Entry(entity);
            return (entry.State == EntityState.Deleted);
        }

        /// <summary>
        /// Used to find if entity is in detached state. Detached means the entity will not be sent back to database for changes like insert, update or delete when SaveChanges is called
        /// </summary>        
        /// <param name="entity">entity whose state need to be known</param>
        /// <returns>True if entity is in detached state</returns>
        public static bool IsDetached<T>(DbSet<T> dbSet, DbContext db, T entity) where T : class
        {
            DbEntityEntry<T> entry = db.Entry(entity);
            return (entry.State == EntityState.Detached);
        }

        /// <summary>
        /// Used to find if entity is in unchanged state. Unchanged means no property value is changed since it is fetched from database
        /// </summary>        
        /// <param name="entity">entity whose state need to be known</param>
        /// <returns>True if entity is attached in dbcontext and is in unchanged state</returns>
        public static bool IsUnchanged<T>(DbSet<T> dbSet, DbContext db, T entity) where T : class
        {
            DbEntityEntry<T> entry = db.Entry(entity);
            return (entry.State == EntityState.Unchanged);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Attach an entity to dbContext as Unchanged State. If entity already exist in context, it doesn't change status of that entity
        /// </summary>
        /// <param name="entity">entity to be attached to dbcontext</param>
        private static void Attach<T>(DbSet<T> dbSet, DbContext db, T entity) where T : class
        {
            DbEntityEntry<T> entry = db.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                dbSet.Attach(entity);
            }
        }

        #endregion

        /// <summary>
        ///    Executes an arbitrary command directly against the data source using the
        ///     existing connection.  The command is specified using the server's native
        ///     query language, such as SQL.  As with any API that accepts SQL it is important
        ///     to parameterize any user input to protect against a SQL injection attack.
        ///     You can include parameter place holders in the SQL query string and then
        ///     supply parameter values as additional arguments. Any parameter values you
        ///     supply will automatically be converted to a DbParameter.  context.ExecuteStoreCommand("UPDATE
        ///     dbo.Posts SET Rating = 5 WHERE Author = @p0", userSuppliedAuthor); Alternatively,
        ///     you can also construct a DbParameter and supply it to SqlQuery. This allows
        ///     you to use named parameters in the SQL query string.  context.ExecuteStoreCommand("UPDATE
        ///     dbo.Posts SET Rating = 5 WHERE Author = @author", new SqlParameter("@author",
        ///     userSuppliedAuthor));
        ///     Note: If there isn't an existing local transaction a new transaction will be used to execute the command.
        /// </summary>
        /// <param name="commandText">The command specified in the server's native query language.</param>
        /// <param name="parameters">The parameter values to use for the query.</param>
        /// <returns>The number of rows affected.</returns>        
        public static int ExecuteCommand(this DbContext db, string commandText, params object[] parameters)
        {
            ObjectContext objCtx = ((IObjectContextAdapter)db).ObjectContext;
            return objCtx.ExecuteStoreCommand(commandText, parameters);
        }

        /// <summary>
        /// Creates a raw SQL query that will return elements of the given generic type.
        /// The type can be any type that has properties that match the names of the
        /// columns returned from the query, or can be a simple primitive type. The type
        /// does not have to be an entity type. The results of this query are never tracked
        /// by the context even if the type of object returned is an entity type. 
        /// As with any API that accepts SQL it is important to parameterize any user input to protect
        /// against a SQL injection attack. You can include parameter place holders in
        /// the SQL query string and then supply parameter values as additional arguments.
        /// Any parameter values you supply will automatically be converted to a DbParameter.
        /// context.Database.SqlQuery<Post>("SELECT * FROM dbo.Posts WHERE Author =
        /// @p0", userSuppliedAuthor); Alternatively, you can also construct a DbParameter
        /// and supply it to SqlQuery. This allows you to use named parameters in the
        /// SQL query string.  context.Database.SqlQuery<Post>("SELECT * FROM dbo.Posts
        /// WHERE Author = @author", new SqlParameter("@author", userSuppliedAuthor));
        /// You can execute stored procedure using same method like this : ("EXEC dbo.up_GetJobStatus @JobStatus ", params)
        /// </summary>
        /// <typeparam name="TElement">The type of object returned by the query.</typeparam>
        /// <param name="sql">The SQL query string.</param>
        /// <param name="parameters">
        ///  The parameters to apply to the SQL query string. If output parameters are
        ///  used, their values will not be available until the results have been read
        ///  completely. This is due to the underlying behavior of DbDataReader, see http://go.microsoft.com/fwlink/?LinkID=398589
        ///  for more details.
        /// </param>
        /// <returns>A System.Data.Entity.Infrastructure.DbRawSqlQuery<TElement> object that will execute the query when it is enumerated.</returns>
        public static DbRawSqlQuery<TElement> ExecuteQuery<TElement>(this DbContext db, string sql, params object[] parameters)
        {
            return db.Database.SqlQuery<TElement>(sql, parameters);
        }
    }
}

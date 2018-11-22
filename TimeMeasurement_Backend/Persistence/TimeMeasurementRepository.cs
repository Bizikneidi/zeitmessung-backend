using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Newtonsoft.Json;

namespace TimeMeasurement_Backend.Persistence
{
    /// <summary>
    /// A Generic Repository to do CRUD operations on the DB
    /// </summary>
    /// <typeparam name="T">The Class to work with</typeparam>
    public class TimeMeasurementRepository<T> where T : class
    {
        /// <summary>
        /// Insert into DB (id must be 0)
        /// </summary>
        /// <param name="item">object to insert</param>
        public void Create(T item)
        {
            using (var db = new TimeMeasurementDbContext())
            {
                try
                {
                    db.Set<T>().Attach(item);
                    db.Set<T>().Add(item);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        /// <summary>
        /// Delete from DB (if exists)
        /// </summary>
        /// <param name="item">object to delete</param>
        public void Delete(T item)
        {
            using (var db = new TimeMeasurementDbContext())
            {
                db.Set<T>().Attach(item);
                db.Set<T>().Remove(item);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Get Objects from the DB
        /// </summary>
        /// <param name="predicate">The WHERE clause</param>
        /// /// <param name="include">Object to include</param>
        /// <returns></returns>
        public IEnumerable<T> Get(Expression<Func<T, bool>> predicate = null, Expression<Func<T, object>> include = null)
        {
            using (var db = new TimeMeasurementDbContext())
            {
                var query = (IQueryable<T>)db.Set<T>();
                if (include != null)
                {
                    query = query.Include(include);
                }
                return predicate == null ? query.ToList() : query.Where(predicate).ToList();
            }
        }

        /// <summary>
        /// Update in DB (if exists)
        /// </summary>
        /// <param name="item">object to update</param>
        public void Update(T item)
        {
            using (var db = new TimeMeasurementDbContext())
            {
                db.Set<T>().Attach(item);
                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace TimeMeasurement_Backend.Persistence
{
    /// <summary>
    /// A Generic Repository to do CRUD operations on the DB
    /// </summary>
    /// <typeparam name="T">The Entity type to work with</typeparam>
    public class TimeMeasurementRepository<T> where T : class
    {
        /// <summary>
        /// Insert into DB (id must be 0)
        /// </summary>
        /// <param name="item">object to insert</param>
        public void Create(T item)
        {
            try
            {
                using (var db = new TimeMeasurementDbContext())
                {
                    db.Set<T>().Attach(item);
                    db.Set<T>().Add(item);
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }

        /// <summary>
        /// Delete from DB (if exists)
        /// </summary>
        /// <param name="item">object to delete</param>
        public void Delete(T item)
        {
            try
            {
                using (var db = new TimeMeasurementDbContext())
                {
                    db.Set<T>().Attach(item);
                    db.Set<T>().Remove(item);
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Get Objects from the DB
        /// </summary>
        /// <param name="predicate">The WHERE clause</param>
        /// <param name="includes">Objects to include</param>
        /// <returns>All entites matching the WHERE clause and the data requested in includes</returns>
        public IEnumerable<T> Get(Expression<Func<T, bool>> predicate = null, params Expression<Func<T, object>>[] includes)
        {
            try
            {
                using (var db = new TimeMeasurementDbContext())
                {
                    var query = (IQueryable<T>)db.Set<T>();
                    // ReSharper disable once InvertIf
                    if (includes != null)
                    {
                        query = includes.Aggregate(query, (current, include) => current.Include(include));
                    }

                    return predicate == null ? query.ToList() : query.Where(predicate).ToList();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// Update in DB (if exists)
        /// </summary>
        /// <param name="item">object to update</param>
        public void Update(T item)
        {
            try
            {
                using (var db = new TimeMeasurementDbContext())
                {
                    db.Set<T>().Attach(item);
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}